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
using GraphQL.Language.AST;
using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UIBK.GraphSPARQL.DataSource;
using UIBK.GraphSPARQL.Types;
using VDS.RDF;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace UIBK.GraphSPARQL.Query
{
    internal abstract class BaseFieldType : FieldType
    {
        protected static readonly INodeFactory NodeFactory = new NodeFactory();
        private static readonly ConcurrentDictionary<Tuple<Iri, SparqlDataSource, Uri?, bool>, Predicate> _predicateCache = new ConcurrentDictionary<Tuple<Iri, SparqlDataSource, Uri?, bool>, Predicate>();

        private readonly IDataSourceContextAccessor _accessor;

        internal BaseFieldType(SchemaField field)
        {
            _accessor = field.Schema.GetRequiredService<IDataSourceContextAccessor>();
            Field = field;
            Name = field.Name;
            Description = field.Description;
            Predicate = _predicateCache.GetOrAdd(Tuple.Create(PredicateIri, field.DataSource, field.GraphUri, Inversed), tuple => new Predicate(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));
            Arguments = BuildArguments();
        }

        protected DataSourceContext Context => _accessor.Context;

        protected SchemaField Field { get; }

        protected virtual bool Inversed => false;

        public Predicate Predicate { get; }

        protected virtual Iri PredicateIri => Field.PredicateIri;

        protected abstract QueryArguments? BuildArguments();
    }

    [Flags]
    internal enum UpdateableFieldCapabilities
    {
        CanFilter = 0x1,
        CanRequire = 0x2,
        CanUpdate = 0x4,
        CanId = 0x8,
        CanLimit = 0x10,
    }

    internal abstract class UpdateableFieldType<TReturn> : BaseFieldType, IFieldResolver where TReturn : class
    {
        // dotNetRDF objects cache
        private static readonly ISparqlExpression IdTerm = new VariableTerm(Constants.SelfFilterVariableName);

        // GraphQL argument cache
        private static readonly QueryArgument FilterArgument = new QueryArgument(new StringGraphType()) { Name = "filter" };
        private static readonly QueryArgument IdArgument = new QueryArgument(new StringGraphType()) { Name = "id" };
        private static readonly QueryArgument IdsArgument = new QueryArgument(new ListGraphType(new NonNullGraphType(new StringGraphType()))) { Name = "ids" };
        private static readonly QueryArgument LimitArgument = new QueryArgument(new IntGraphType()) { Name = "limit" };
        private static readonly QueryArgument OffsetArgument = new QueryArgument(new IntGraphType()) { Name = "offset" };
        private static readonly QueryArgument RequireArgument = new QueryArgument(new ListGraphType(new NonNullGraphType(new StringGraphType()))) { Name = "require" };

        // update argument names
        private const string Add = "add";
        private const string Remove = "remove";
        private const string Set = "set";

        // arguments cache
        private static readonly ConcurrentDictionary<Tuple<SchemaScalar, bool, UpdateableFieldCapabilities>, QueryArguments> _argumentsCache = new ConcurrentDictionary<Tuple<SchemaScalar, bool, UpdateableFieldCapabilities>, QueryArguments>();

        private static QueryArguments BuildArguments(SchemaScalar mutationScalar, bool isArray, UpdateableFieldCapabilities capabilities)
        {
            var updateType = isArray ? mutationScalar.ListQueryType : mutationScalar.QueryType;
            var args = new QueryArguments();
            if ((capabilities & UpdateableFieldCapabilities.CanFilter) != 0) args.Add(FilterArgument);
            if ((capabilities & UpdateableFieldCapabilities.CanRequire) != 0) args.Add(RequireArgument);
            if ((capabilities & UpdateableFieldCapabilities.CanUpdate) != 0) args.Add(new QueryArgument(updateType) { Name = Set });
            if ((capabilities & UpdateableFieldCapabilities.CanId) != 0) args.Add(IdArgument);
            if (isArray)
            {
                if ((capabilities & UpdateableFieldCapabilities.CanId) != 0) args.Add(IdsArgument);
                if ((capabilities & UpdateableFieldCapabilities.CanLimit) != 0)
                {
                    args.Add(LimitArgument);
                    args.Add(OffsetArgument);
                }
                if ((capabilities & UpdateableFieldCapabilities.CanUpdate) != 0)
                {
                    args.Add(new QueryArgument(updateType) { Name = Add });
                    args.Add(new QueryArgument(updateType) { Name = Remove });
                }
            }
            return args;
        }

        internal UpdateableFieldType(SchemaField field, ISchemaTypeElement type) : base(field)
        {
            ResolvedType = field.GetQueryTypeFromSchemaType(type);
            Resolver = this;
        }

        protected override QueryArguments BuildArguments() => _argumentsCache.GetOrAdd(Tuple.Create(Field.MutationScalar, Field.IsArray, Capabilities), template => BuildArguments(template.Item1, template.Item2, template.Item3));

        private VDS.RDF.INode CreateNode(object obj) => Field.MutationScalar.ToSparql(obj, NodeFactory) ?? throw new ExecutionError($"Failed to convert value '{obj}' of type {obj.GetType().Name} to {Field.MutationScalar}.");

        private ISet<VDS.RDF.INode> NodesFromArgument(IResolveFieldContext request, string name) => request.GetArgument(name, Enumerable.Empty<object>()).Select(CreateNode).ToHashSet();

        object IFieldResolver.Resolve(IResolveFieldContext context) => Field.IsArray ? ResolveMultiple(context.As<Instance?>()) : ResolveSingle(context.As<Instance?>());

        protected abstract TReturn Resolve(IResolveFieldContext<Instance?> request, VDS.RDF.INode node, IEnumerable<Iri> types);

        private IDataLoaderResult<IEnumerable<TReturn>> ResolveMultiple(IResolveFieldContext<Instance?> request)
        {
            var filter = DefaultFilter;

            void And(ISparqlExpression expr)
            {
                if (filter is null) filter = expr;
                else filter = new AndExpression(filter, expr);
            }

            if (CanFilter)
            {
                if (Field.FilterExpression is not null) And(Field.FilterExpression);
                if (request.HasArgument(FilterArgument)) And(Filter.Parse(request.GetArgument<string>(FilterArgument), s => new ExecutionError(s)));
            }
            if (CanId)
            {
                if (request.HasArgument(IdArgument)) And(new EqualsExpression(IdTerm, new Iri(request.GetArgument<string>(IdArgument)).Term));
                if (Field.IsArray)
                {
                    request
                        .GetArgument(IdsArgument, Enumerable.Empty<string>())
                        .Select(id => new EqualsExpression(IdTerm, new Iri(id).Term))
                        .ForEach(And);
                }
            }
            if (CanRequire)
            {
                if (Field.IsRequired) And(new BoundFunction(new VariableTerm(Field.Name)));
                request
                    .GetArgument(RequireArgument, Enumerable.Empty<string>())
                    .Select(field => new BoundFunction(new VariableTerm(field)))
                    .ForEach(And);
            }

            return Context.QueryData.LoadAsync(new Request(
                request.Source?.Iri,
                filter is null ? Predicate : new Predicate(Predicate, new Filter(request.Source?.Type, Container, filter)),
                IncludeTypeInfo)).Then(values =>
            {
                if (CanUpdate) values = Update(request, values);
                if (CanLimit && Field.IsArray)
                {
                    if (request.HasArgument(OffsetArgument)) values = values.Skip(request.GetArgument<int>(OffsetArgument));
                    if (request.HasArgument(LimitArgument)) values = values.Take(request.GetArgument<int>(LimitArgument));
                }
                return
                    !Field.IsRequired || values.Any()
                        ? values.Select(value => Resolve(request, value.Object, value.Types))
                        : throw new Exception($"No entry was returned for {Field}.");
            });
        }

        private IDataLoaderResult<TReturn?> ResolveSingle(IResolveFieldContext<Instance?> request) => ResolveMultiple(request).Then(values =>
        {
            // limit to one result
            using var enumerator = values.GetEnumerator();
            if (!enumerator.MoveNext()) return !Field.IsRequired ? default(TReturn) : throw new ExecutionError($"No entry was returned for {Field}.");
            var result = enumerator.Current;
            if (enumerator.MoveNext()) throw new ExecutionError($"More than one entry was returned for {Field}.");
            return result;
        });

        private IEnumerable<Response> Update(IResolveFieldContext<Instance?> request, IEnumerable<Response> results)
        {
            if (request.Operation.OperationType == OperationType.Mutation)
            {
                var subject = request.Source?.Iri ?? throw new ExecutionError("Operation not supported on root.");
                if (Field.IsArray)
                {
                    // find out what to insert, delete and what already exists
                    var existingNodes = results.ToDictionary(result => result.Object);
                    ISet<VDS.RDF.INode> nodesToInsert;
                    ISet<VDS.RDF.INode> nodesToDelete;
                    if (request.HasArgument(Set))
                    {
                        if (request.HasArgument(Add) || request.HasArgument(Remove)) throw new ExecutionError($"If argument '{Set}' is given, neither '{Add}' nor '{Remove}' must specified.");
                        nodesToInsert = NodesFromArgument(request, Set);
                        nodesToDelete = new HashSet<VDS.RDF.INode>(existingNodes.Keys);
                    }
                    else
                    {
                        nodesToInsert = NodesFromArgument(request, Add);
                        nodesToDelete = NodesFromArgument(request, Remove);
                    }
                    nodesToDelete.IntersectWith(existingNodes.Keys); // only delete existing data
                    nodesToInsert.ExceptWith(existingNodes.Keys); // don't create existing data
                    nodesToInsert.ExceptWith(nodesToDelete); // delete takes precedence

                    // cache the operation
                    nodesToInsert.ForEach(node => Context.InsertData(subject, Predicate, node));
                    nodesToDelete.ForEach(node => Context.DeleteData(subject, Predicate, node));

                    // return all non-deleted results and the inserted nodes
                    return
                        existingNodes
                            .Where(entry => !nodesToDelete.Contains(entry.Key))
                            .Select(entry => entry.Value)
                            .Concat(nodesToInsert.Select(node => new Response(node)));
                }
                else
                {
                    if (request.HasArgument(Set))
                    {
                        switch (results.Count())
                        {
                            case 0: break;
                            case 1: Context.DeleteData(subject, Predicate, results.Single().Object); break;
                            default: throw new ExecutionError($"More than a single entry currently exist for {Field}.");
                        }
                        var value = request.GetArgument(Set, (object?)null);
                        if (value is not null)
                        {
                            var @object = CreateNode(value);
                            Context.InsertData(subject, Predicate, @object);
                            return new Response(@object).AsEnumerable();
                        }
                        else
                        {
                            return Enumerable.Empty<Response>();
                        }
                    }
                    else
                    {
                        return results;
                    }
                }
            }
            else
            {
                if (Field.IsArray)
                {
                    if (request.HasArgument(Add) || request.HasArgument(Remove) || request.HasArgument(Set)) throw new ExecutionError($"The following arguments are only allowed for mutations: '{Add}', '{Remove}' and '{Set}'");
                }
                else
                {
                    if (request.HasArgument(Set)) throw new ExecutionError($"The following argument is only allowed for mutations: '{Set}'");
                }
                return results;
            }
        }

        protected virtual bool CanLimit => true;

        protected virtual bool CanFilter => true;

        protected virtual bool CanId => false;

        protected virtual bool CanRequire => Container is not null;

        protected virtual bool CanUpdate => false;

        internal UpdateableFieldCapabilities Capabilities =>
            (CanLimit ? UpdateableFieldCapabilities.CanLimit : 0) |
            (CanFilter ? UpdateableFieldCapabilities.CanFilter : 0) |
            (CanId ? UpdateableFieldCapabilities.CanId : 0) |
            (CanRequire ? UpdateableFieldCapabilities.CanRequire : 0) |
            (CanUpdate ? UpdateableFieldCapabilities.CanUpdate : 0);

        protected abstract SchemaContainer? Container { get; }

        protected virtual ISparqlExpression? DefaultFilter => null;

        protected virtual bool IncludeTypeInfo => false;
    }

    internal abstract class ObjectFieldType : UpdateableFieldType<Instance>
    {
        internal ObjectFieldType(SchemaField field) : base(field, field.Object) { }

        protected override bool CanId => true;

        protected override SchemaContainer? Container => Field.Object as SchemaContainer;

        protected override bool IncludeTypeInfo => Field.Object.Definition.Types.Count() > 1;

        protected sealed override Instance Resolve(IResolveFieldContext<Instance?> request, VDS.RDF.INode node, IEnumerable<Iri> types)
        {
            var availableTypes = Field.Object.Definition.Types
                .OrderByDescending(type => request.SubFields.Keys.Count(field => type.ContainsField(field)))
                .ThenBy(type => type.FieldCount);
            return new Instance
            (
                request.Source,
                availableTypes.FirstOrDefault(type => types.Contains(type.ClassIri)) ?? availableTypes.FirstOrDefault() ?? throw new ExecutionError($"No available type for {Field.Object}."),
                node is IUriNode uriNode ? uriNode.Uri : throw new ExecutionError($"{node} is not an IRI.")
            );
        }
    }

    internal sealed class ChildFieldType : ObjectFieldType
    {
        internal ChildFieldType(SchemaField field) : base(field) { }

        protected override bool CanUpdate => true;
    }

    internal sealed class RootFieldType : ObjectFieldType
    {
        internal RootFieldType(SchemaField field) : base(field) { }

        protected override ISparqlExpression? DefaultFilter => Field.Object.Definition.TypeFilter;

        protected override bool Inversed => true;

        protected override Iri PredicateIri => DataSourceContext.TypeIri;
    }

    internal sealed class ScalarFieldType : UpdateableFieldType<object>
    {
        internal ScalarFieldType(SchemaField field) : base(field, field.Scalar) { }

        protected override bool CanUpdate => true;

        protected override SchemaContainer? Container => null;

        protected override object Resolve(IResolveFieldContext<Instance?> request, VDS.RDF.INode node, IEnumerable<Iri> _) =>
            Field.Scalar.FromSparql(node) ?? throw new ExecutionError($"Failed to convert from node {node} to {Field}.");
    }

    internal sealed class MutationField : BaseFieldType
    {
        internal MutationField(SchemaField field) : base(field) => ResolvedType = field.GetQueryTypeFromSchemaType(field.MutationScalar);

        protected override QueryArguments? BuildArguments() => null;

        private void InsertData(Iri subject, object value) => Context.InsertData(subject, Predicate, Field.MutationScalar.ToSparql(value, NodeFactory) ?? throw new ExecutionError($"Failed to convert from value '{value}' to {Field}."));

        public void PersistValue(Iri subject, object value)
        {
            if (Field.IsArray) foreach (var element in (value as IEnumerable<object>) ?? throw new ExecutionError($"Value '{value}' is not enumerable which is required for {Field}.")) InsertData(subject, element);
            else InsertData(subject, value);
        }
    }

    internal sealed class CreationFieldType : BaseFieldType, IFieldResolver
    {
        private static readonly ConcurrentDictionary<SchemaType, QueryArguments> _argumentsCache = new ConcurrentDictionary<SchemaType, QueryArguments>();

        private static readonly QueryArgument IdArgument = new QueryArgument(new NonNullGraphType(new IdGraphType())) { Name = "id" };
        private const string TemplateArgumentName = "template";

        private static QueryArguments BuildArguments(SchemaType type) => new QueryArguments()
        {
            IdArgument,
            new QueryArgument(type.NonNullMutationType) { Name = TemplateArgumentName }
        };

        internal CreationFieldType(SchemaField field) : base(field)
        {
            ResolvedType = SchemaType.NonNullQueryType;
            Resolver = this;
        }

        protected override QueryArguments BuildArguments() => _argumentsCache.GetOrAdd(SchemaType, BuildArguments);

        object IFieldResolver.Resolve(IResolveFieldContext context) => Resolve(context);

        private Instance Resolve(IResolveFieldContext context)
        {
            var iri = new Iri(context.GetArgument<string>(IdArgument));
            Context.InsertData(iri, Predicate, SchemaType.ClassIri.Node);
            var template = context.GetArgument<IDictionary<string, object>>(TemplateArgumentName);
            foreach (var field in SchemaType.MutationType.Fields.OfType<MutationField>())
            {
                if (template.TryGetValue(field.Name, out var value)) field.PersistValue(iri, value);
            }
            return new Instance(null, SchemaType, iri);
        }

        protected override Iri PredicateIri => DataSourceContext.TypeIri;

        public SchemaType SchemaType => Field.Object as SchemaType ?? throw new ArgumentException($"Only supported for {nameof(SchemaType)}.");
    }
}
