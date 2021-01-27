/*
 * GraphQL to SPARQL Bridge
 * Copyright (C) 2020  Manuel Meitinger
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Execution;
using GraphQL.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UIBK.GraphSPARQL.Query;
using UIBK.GraphSPARQL.Types;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;
using static System.FormattableString;

namespace UIBK.GraphSPARQL.DataSource
{
    /// <inheritdoc cref="IDataLoaderContextAccessor"/>
    public interface IDataSourceContextAccessor
    {
        /// <inheritdoc cref="IDataLoaderContextAccessor.Context"/>
        DataSourceContext Context { get; }

        internal void Set();
        internal void Reset();
    }

    /// <inheritdoc cref="DataLoaderContextAccessor"/>
    public class DataSourceContextAccessor : IDataSourceContextAccessor, IDataLoaderContextAccessor
    {
        private static readonly AsyncLocal<DataSourceContext?> _current = new AsyncLocal<DataSourceContext?>();

        /// <inheritdoc/>
        public DataSourceContext Context => _current.Value.RequireProperty();

        DataLoaderContext IDataLoaderContextAccessor.Context
        {
            get => Context;
            set => throw new NotSupportedException();
        }

        void IDataSourceContextAccessor.Set() => _current.Value = new DataSourceContext();
        void IDataSourceContextAccessor.Reset() => _current.Value = null;
    }

    /// <inheritdoc cref="DataLoaderDocumentListener"/>
    public class DataSourceDocumentListener : IDocumentExecutionListener
    {
        private readonly IDataSourceContextAccessor _accessor;

        /// <inheritdoc cref="DataLoaderDocumentListener(IDataLoaderContextAccessor)"/>
        public DataSourceDocumentListener(IDataSourceContextAccessor accessor) => _accessor = accessor;

        /// <inheritdoc/>
        public Task AfterValidationAsync(IExecutionContext context, IValidationResult validationResult) => Task.CompletedTask;

        /// <inheritdoc/>
        public Task BeforeExecutionAsync(IExecutionContext context)
        {
            _accessor.Set();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task BeforeExecutionAwaitedAsync(IExecutionContext context) => Task.CompletedTask;

        /// <inheritdoc/>
        public Task AfterExecutionAsync(IExecutionContext context)
        {
            var dataSourceContext = _accessor.Context;
            _accessor.Reset();
            return dataSourceContext.ProcessUpdatesAsync();
        }

        /// <inheritdoc/>
        public Task BeforeExecutionStepAwaitedAsync(IExecutionContext context) => Task.CompletedTask;
    }

    /// <inheritdoc cref="DataLoaderContext"/>
    public class DataSourceContext : DataLoaderContext
    {
        internal static readonly Iri TypeIri = "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";

        private class QueryResultGrouping : IGrouping<Request, Response>
        {
            private readonly IEnumerable<Response> _values;

            public QueryResultGrouping(Request key, IEnumerable<Response> values)
            {
                Key = key;
                _values = values;
            }

            public Request Key { get; }

            public IEnumerator<Response> GetEnumerator() => _values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class QueryHandler : BaseHandler, IRdfHandler
        {
            private sealed class Iris : IEnumerable<Iri>
            {
                private readonly ISet<Iri> _set = new HashSet<Iri>();
                private int _hashCode = 0;

                public void Add(Iri iri)
                {
                    if (_set.Add(iri)) _hashCode ^= iri.GetHashCode();
                }

                public override bool Equals(object? obj) => obj is Iris iris && iris._set.SetEquals(_set);

                public override int GetHashCode() => _hashCode;

                public IEnumerator<Iri> GetEnumerator() => _set.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            private sealed class IrisAndTypeInfo
            {
                private Iris? _iris = new Iris();

                public bool AnyIri => _iris is null;
                public bool IncludeTypeInfo { get; private set; }
                public Iris Iris => _iris ?? throw new InvalidOperationException("All iris should be queried.");

                public void AddIri(Iri iri) => _iris?.Add(iri);
                public void QueryAll() => _iris = null;
                public void SetIncludeTypeInfo() => IncludeTypeInfo = true;
            }

            private sealed class VarNameAndBuilders : IEnumerable<GraphPatternBuilder>
            {
                private readonly IList<GraphPatternBuilder> _builders = new List<GraphPatternBuilder>();

                public VarNameAndBuilders(string varName) => VarName = varName;

                public string VarName { get; }

                public void Add(GraphPatternBuilder builder) => _builders.Add(builder);

                public IEnumerator<GraphPatternBuilder> GetEnumerator() => _builders.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            private const string TriplesPrefix = "https://schema.uibk.ac.at/GraphSPARQL/triples/";
            private static readonly Uri TriplesPrefixUri = new Uri(TriplesPrefix);
            private static readonly Uri XsdPrefixUri = new Uri("http://www.w3.org/2001/XMLSchema");
            private static readonly Uri RdfPrefixUri = new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            private static readonly Uri RdfsPrefixUri = new Uri("http://www.w3.org/2000/01/rdf-schema#");

            private readonly SparqlDataSource _dataSource;
            private readonly IDictionary<Iri, Predicate> _predicatesMap = new Dictionary<Iri, Predicate>();
            private readonly IDictionary<Predicate, IrisAndTypeInfo> _predicates = new Dictionary<Predicate, IrisAndTypeInfo>();
            private readonly IDictionary<Predicate, IDictionary<Iri, ISet<INode>>> _results = new Dictionary<Predicate, IDictionary<Iri, ISet<INode>>>();
            private readonly IDictionary<Predicate, IDictionary<INode, ISet<Iri>>> _typesInfo = new Dictionary<Predicate, IDictionary<INode, ISet<Iri>>>();
            private readonly IDictionary<Iri, Predicate> _typesMap = new Dictionary<Iri, Predicate>();
            private readonly IDictionary<Iris, VarNameAndBuilders> _iriSets = new Dictionary<Iris, VarNameAndBuilders>();
            private readonly IList<GraphPatternBuilder> _anyIriSet = new List<GraphPatternBuilder>();

            public QueryHandler(SparqlDataSource dataSource) => _dataSource = dataSource;

            public void Add(Iri? subject, Predicate predicate, bool includeTypeInfo)
            {
                if (!_predicates.TryGetValue(predicate, out var irisAndTypeInfo))
                {
                    irisAndTypeInfo = new IrisAndTypeInfo();
                    _predicates.Add(predicate, irisAndTypeInfo);
                }
                if (subject is null) irisAndTypeInfo.QueryAll();
                else irisAndTypeInfo.AddIri(subject);
                if (includeTypeInfo) irisAndTypeInfo.SetIncludeTypeInfo();
            }

            private void AddFilter(Query.Filter filter, string subjectVarName, string objectVarName, Func<int, string> varName, IGraphPatternBuilder builder)
            {
                var expression = filter.Expression;
                var variables = expression.Variables.ToHashSet();
                var variableId = 0;
                foreach (var variable in variables)
                {
                    switch (variable)
                    {
                        case Constants.ParentFilterVariableName:
                        {
                            // subject iri (actual parent might be null)
                            expression = expression.ReplaceVar(Constants.ParentFilterVariableName, subjectVarName);
                            break;
                        }

                        case Constants.SelfFilterVariableName:
                        {
                            // object iri
                            expression = expression.ReplaceVar(Constants.SelfFilterVariableName, objectVarName);
                            break;
                        }

                        default:
                        {
                            var fieldPath = variable;
                            var filterVarName = objectVarName;
                            var container = filter.Container;

                            // use the parent type if variable starts with '__parent'
                            if (fieldPath.StartsWith(Constants.ParentFilterVariableName))
                            {
                                filterVarName = subjectVarName;
                                container = filter.ParentType;
                                if (fieldPath[Constants.ParentFilterVariableName.Length] != '_') throw new ExecutionError($"Invalid continuation after '{Constants.ParentFilterVariableName}' in '{variable}'.");
                                fieldPath = fieldPath[(Constants.ParentFilterVariableName.Length + 1)..];
                            }
                            else
                            {
                                // ignore any other variable starting with two underscores
                                if (fieldPath.StartsWith("__")) continue;
                            }

                            // walk down the hierarchy
                            if (fieldPath.Length > 0)
                            {
                                while (true)
                                {
                                    var nextFieldOffset = fieldPath.IndexOf('_', fieldPath[0] == '_' ? 1 : 0);
                                    var fieldName = nextFieldOffset == -1 ? fieldPath : fieldPath[0..nextFieldOffset];
                                    if (container is null) throw new ExecutionError($"No container to find '{fieldName}' for '{variable}'.");
                                    if (!container.TryGetField(fieldName, out var field)) throw new ExecutionError($"Field '{fieldName}' not found in {container} for '{variable}'.");
                                    if (field.DataSource != _dataSource) throw new ExecutionError($"Field {field} not in {_dataSource} for '{variable}'.");
                                    var filterSubjectVarName = filterVarName; // important, since it gets captured
                                    var filterObjectVarVarName = varName(variableId++);
                                    builder.GraphIf(field.GraphUri, graph => graph.OptionalWhere(where => where.Subject(filterSubjectVarName).PredicateUri(field.PredicateIri.Uri).Object(filterObjectVarVarName)));
                                    filterVarName = filterObjectVarVarName;
                                    container = field.Type == SchemaFieldType.Object ? field.Object as SchemaContainer : null;
                                    if (nextFieldOffset == -1) break;
                                    fieldPath = fieldPath[(nextFieldOffset + 1)..];
                                }
                            }
                            expression = expression.ReplaceVar(variable, filterVarName);
                            break;
                        }
                    }
                }
                builder.Filter(expression);
            }

            private void BuildPredicate(Predicate predicate, IrisAndTypeInfo subjectAndTypeInfo, int predicateId, IDescribeGraphPatternBuilder construct)
            {
                // prepare the mapping for results to queries
                var predicateIri = new Iri(Invariant($"{TriplesPrefix}p{predicateId}"));
                var subjectVarName = Invariant($"__s{predicateId}");
                var objectVarName = Invariant($"__o{predicateId}");
                var typeVarName = Invariant($"__t{predicateId}");
                _predicatesMap.Add(predicateIri, predicate);

                // define the iris if required
                var pattern = new GraphPatternBuilder();
                if (subjectAndTypeInfo.AnyIri)
                {
                    _anyIriSet.Add(pattern);
                }
                else if (_iriSets.TryGetValue(subjectAndTypeInfo.Iris, out var existingSet))
                {
                    subjectVarName = existingSet.VarName;
                    existingSet.Add(pattern);
                }
                else
                {
                    _iriSets.Add(subjectAndTypeInfo.Iris, new VarNameAndBuilders(subjectVarName) { pattern });
                }

                // build the CONSTRUCT part
                construct.Where(where =>
                {
                    where.Subject(subjectVarName).PredicateUri(predicateIri.Uri).Object(objectVarName);

                    if (subjectAndTypeInfo.IncludeTypeInfo)
                    {
                        var typePredicateIri = new Iri(Invariant($"{TriplesPrefix}t{predicateId}"));
                        _typesMap.Add(typePredicateIri, predicate);
                        where.Subject(objectVarName).PredicateUri(typePredicateIri.Uri).Object(typeVarName);
                    }
                });

                // build the pattern and filter
                pattern.GraphIf(predicate.GraphIri?.Uri, inner =>
                {
                    // predicate
                    inner.Where(where => where
                    .Subject(predicate.Inversed ? objectVarName : subjectVarName)
                    .PredicateUri(predicate.Iri.Uri)
                    .Object(predicate.Inversed ? subjectVarName : objectVarName));

                    // type info
                    if (subjectAndTypeInfo.IncludeTypeInfo)
                    {
                        pattern.OptionalWhere(where => where
                            .Subject(objectVarName)
                            .PredicateUri(TypeIri.Uri)
                            .Object(typeVarName));
                    }
                });
                if (predicate.Filter is not null) AddFilter(predicate.Filter, subjectVarName, objectVarName, id => Invariant($"__v{predicateId}_{id}"), pattern);
            }

            public Task Process(CancellationToken cancellationToken) => Task.Run(() =>
            {
                Debug.WriteLine($"Prepare query for {_dataSource}.");
                var builder = QueryBuilder.Construct(construct =>
                 {
                     var predicateId = 0;
                     foreach (var entry in _predicates) BuildPredicate(entry.Key, entry.Value, predicateId++, construct);
                 });
                builder.Union(new GraphPatternBuilder(), _anyIriSet.Concat(_iriSets.Select(set =>
                {
                    var iris = new GraphPatternBuilder();
                    var inlineData = iris.InlineData(set.Value.VarName);
                    set.Key.ForEach(iri => inlineData.Values(values => values.Value(iri.Uri)));
                    var iriSetBuilder = new GraphPatternBuilder();
                    iriSetBuilder.Union(iris, set.Value.ToArray());
                    return iriSetBuilder;
                })).ToArray());
                builder.Prefixes.AddNamespace(string.Empty, TriplesPrefixUri);
                builder.Prefixes.AddNamespace("xsd", XsdPrefixUri);
                builder.Prefixes.AddNamespace("rdf", RdfPrefixUri);
                builder.Prefixes.AddNamespace("rdfs", RdfsPrefixUri);
                _dataSource.Prefixes.ForEach(prefix => builder.Prefixes.AddNamespace(prefix.Key, prefix.Value));
                var query = builder.BuildQuery();
                Debug.WriteLine(query.ToString());
                _dataSource.QueryProcessor.ProcessQuery(this, null, query);
                Debug.WriteLine($"Finished query for {_dataSource}.");
            }, cancellationToken);

            public IEnumerable<Response> this[Request query]
            {
                get
                {
                    if (_results.TryGetValue(query.Predicate, out var iris))
                    {
                        if (query.Subject is null) return iris.Values.SelectMany(nodes => nodes.Select(BuildResult));
                        else if (iris.TryGetValue(query.Subject, out var nodes)) return nodes.Select(BuildResult);
                    }
                    return Enumerable.Empty<Response>();

                    Response BuildResult(INode node) => new Response(node, query.IncludeTypeInfo && _typesInfo.TryGetValue(query.Predicate, out var nodes) && nodes.TryGetValue(node, out var types) ? types : Enumerable.Empty<Iri>());
                }
            }

            bool IRdfHandler.AcceptsAll => true;
            void IRdfHandler.StartRdf() { }
            void IRdfHandler.EndRdf(bool ok) { }
            bool IRdfHandler.HandleNamespace(string prefix, Uri namespaceUri) => true;
            bool IRdfHandler.HandleBaseUri(Uri baseUri) => true;
            bool IRdfHandler.HandleTriple(Triple t)
            {
                if (t.Predicate is IUriNode predicateNode)
                {
                    // store the result
                    if (_predicatesMap.TryGetValue(predicateNode.Uri, out var predicate) && t.Subject is IUriNode subjectNode)
                    {
                        var subject = new Iri(subjectNode.Uri);
                        if (!_results.TryGetValue(predicate, out var iris)) _results.Add(predicate, iris = new Dictionary<Iri, ISet<INode>>());
                        if (!iris.TryGetValue(subject, out var nodes)) iris.Add(subject, nodes = new HashSet<INode>());
                        nodes.Add(t.Object);
                    }

                    // store returned type info
                    if (_typesMap.TryGetValue(predicateNode.Uri, out predicate) && t.Object is IUriNode objectNode)
                    {
                        if (!_typesInfo.TryGetValue(predicate, out var nodes)) _typesInfo.Add(predicate, nodes = new Dictionary<INode, ISet<Iri>>());
                        if (!nodes.TryGetValue(t.Subject, out var types)) nodes.Add(t.Subject, types = new HashSet<Iri>());
                        types.Add(objectNode.Uri);
                    }
                }
                return true;
            }
        }

        private class QueryContainer : ILookup<Request, Response>
        {
            private readonly IDictionary<SparqlDataSource, QueryHandler> _handlers = new Dictionary<SparqlDataSource, QueryHandler>();
            private readonly ISet<Request> _queries;

            public QueryContainer(IEnumerable<Request> queries)
            {
                _queries = new HashSet<Request>(queries);
                foreach (var query in _queries)
                {
                    if (!_handlers.TryGetValue(query.Predicate.DataSource, out var handler)) _handlers.Add(query.Predicate.DataSource, handler = new QueryHandler(query.Predicate.DataSource));
                    handler.Add(query.Subject, query.Predicate, query.IncludeTypeInfo);
                }
            }

            public IEnumerable<Response> this[Request key] => _handlers[key.Predicate.DataSource][key];

            public int Count => _handlers.Count;

            public bool Contains(Request key) => _queries.Contains(key);

            public IEnumerator<IGrouping<Request, Response>> GetEnumerator() => _queries.Select(query => new QueryResultGrouping(query, _handlers[query.Predicate.DataSource][query])).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public Task Process(CancellationToken cancellationToken) => Task.WhenAll(_handlers.Values.Select(handler => handler.Process(cancellationToken)));
        }

        private static GraphPattern BuildPattern(Iri subject, Predicate predicate, INode @object)
        {
            var bgp = new Bgp
            (
                predicate.Inversed
                    ? new TriplePattern(new NodeMatchPattern(@object), predicate.Iri, subject)
                    : new TriplePattern(subject, predicate.Iri, new NodeMatchPattern(@object))
            );
            if (predicate.GraphIri is null) return bgp.ToGraphPattern();
            return new VDS.RDF.Query.Algebra.Graph(bgp, predicate.GraphIri.Token).ToGraphPattern();
        }

        private readonly IDictionary<SparqlDataSource, IList<SparqlUpdateCommand>> _updates = new Dictionary<SparqlDataSource, IList<SparqlUpdateCommand>>();

        /// <summary>
        /// Appends an <see cref="SparqlUpdateCommand"/> to the cache.
        /// </summary>
        /// <param name="dataSource">The <see cref="SparqlDataSource"/> the <see cref="SparqlUpdateCommand"/> is run against.</param>
        /// <param name="command">The <see cref="SparqlUpdateCommand"/> to cache.</param>
        public void AddUpdateCommand(SparqlDataSource dataSource, SparqlUpdateCommand command)
        {
            lock (_updates)
            {
                if (_updates.TryGetValue(dataSource, out var commandList)) commandList.Add(command);
                else _updates.Add(dataSource, new List<SparqlUpdateCommand> { command });
            }
        }

        /// <summary>
        /// Deletes the given triple from the <see cref="SparqlDataSource"/>.
        /// </summary>
        /// <param name="subject">The subject <see cref="Iri"/>.</param>
        /// <param name="predicate">The <see cref="Predicate"/> specifying the predicate <see cref="Iri"/>, graph <see cref="Uri"/> and <see cref="SparqlDataSource"/>.</param>
        /// <param name="object">The <see cref="INode"/> to delete.</param>
        public void DeleteData(Iri subject, Predicate predicate, INode @object) => AddUpdateCommand(predicate.DataSource, new DeleteDataCommand(BuildPattern(subject, predicate, @object)));

        /// <summary>
        /// Inserts a given triple into the <see cref="SparqlDataSource"/>.
        /// </summary>
        /// <param name="subject">The subject <see cref="Iri"/>.</param>
        /// <param name="predicate">The <see cref="Predicate"/> specifying the predicate <see cref="Iri"/>, graph <see cref="Uri"/> and <see cref="SparqlDataSource"/>.</param>
        /// <param name="object">The <see cref="INode"/> to insert.</param>
        public void InsertData(Iri subject, Predicate predicate, INode @object) => AddUpdateCommand(predicate.DataSource, new InsertDataCommand(BuildPattern(subject, predicate, @object)));

        /// <summary>
        /// Gets the data loader.
        /// </summary>
        public IDataLoader<Request, IEnumerable<Response>> QueryData => this.GetOrAddCollectionBatchLoader<Request, Response>("SPARQL", async (queries, cancellationToken) =>
        {
            var query = new QueryContainer(queries);
            await query.Process(cancellationToken);
            return query;
        });

        internal Task ProcessUpdatesAsync() => Task.Run(() =>
        {
            foreach (var entry in _updates)
            {
                var update = new SparqlUpdateCommandSet(entry.Value);
                Debug.WriteLine(update);
                entry.Key.UpdateProcessor.ProcessCommandSet(update);
            }
        });
    }
}
