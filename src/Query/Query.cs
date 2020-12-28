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

using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UIBK.GraphSPARQL.DataSource;
using UIBK.GraphSPARQL.Types;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;
using static System.FormattableString;

namespace UIBK.GraphSPARQL.Query
{
    /// <summary>
    /// Class describing a query filter.
    /// </summary>
    public sealed class Filter
    {
        internal static ISparqlExpression Parse(string filter, ExceptionBuilder error)
        {
            try { return new SparqlQueryParser().ParseFromString(Invariant($"PREFIX xsd: <http://www.w3.org/2001/XMLSchema>\nPREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\nPREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>\nCONSTRUCT{{}}WHERE{{FILTER({filter})}}")).RootGraphPattern.Filter.Expression; }
            catch (RdfParseException e) { throw error(e.Message); }
        }

        private readonly string _filter;
        private readonly int _hashCode;

        /// <summary>
        /// Create a new <see cref="Filter"/> instance.
        /// </summary>
        /// <param name="parentType">The <see cref="SchemaType"/> of the <see cref="Instance"/> that this <see cref="Filter"/> is called on.</param>
        /// <param name="container">The <see cref="SchemaContainer"/> that defines available <see cref="SchemaField"/>s.</param>
        /// <param name="expression">The filter <see cref="ISparqlExpression"/>.</param>
        public Filter(SchemaType? parentType, SchemaContainer? container, ISparqlExpression expression)
        {
            ParentType = parentType;
            Container = container;
            Expression = expression;
            _filter = expression.ToString() ?? throw new ArgumentException("No string returned from expression.", nameof(expression));
            _hashCode = parentType?.GetHashCode() ?? 0 ^ container?.GetHashCode() ?? 0 ^ _filter.GetHashCode();
        }

        /// <summary>
        /// The <see cref="SchemaType"/> of the <see cref="Instance"/> that this <see cref="Filter"/> is called on.
        /// </summary>
        public SchemaType? ParentType { get; }

        /// <summary>
        /// The <see cref="SchemaContainer"/> that defines available <see cref="SchemaField"/>s.
        /// </summary>
        public SchemaContainer? Container { get; }

        /// <summary>
        /// The filter <see cref="ISparqlExpression"/>.
        /// </summary>
        public ISparqlExpression Expression { get; }

        /// <inheritdoc/>
        public sealed override bool Equals(object? obj) =>
            obj is Filter filter &&
            filter.ParentType == ParentType &&
            filter.Container == Container &&
            filter._filter == _filter;

        /// <inheritdoc/>
        public sealed override int GetHashCode() => _hashCode;

        /// <inheritdoc/>
        public static bool operator ==(Filter? obj1, Filter? obj2) => object.Equals(obj1, obj2);

        /// <inheritdoc/>
        public static bool operator !=(Filter? obj1, Filter? obj2) => !object.Equals(obj1, obj2);
    }

    /// <summary>
    /// Class describing a concrete typed resource.
    /// </summary>
    public sealed class Instance
    {
        internal static IObjectGraphType TypeResolver(object source) => ((Instance)source).Type.QueryType;

        private readonly int _hashCode;

        internal Instance(Instance? parent, SchemaType type, Iri iri)
        {
            Parent = parent;
            Type = type;
            Iri = iri;
            _hashCode = parent?.GetHashCode() ?? 0 ^ type.GetHashCode() ^ iri.GetHashCode();
        }

        /// <summary>
        /// The parent <see cref="Instance"/> or <c>null</c> if this is a top-level <see cref="Instance"/>.
        /// </summary>
        public Instance? Parent { get; }

        /// <summary>
        /// The <see cref="SchemaType"/> of this <see cref="Instance"/>.
        /// </summary>
        public SchemaType Type { get; }

        /// <summary>
        /// The resource <see cref="Iri"/>.
        /// </summary>
        public Iri Iri { get; }

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is Instance instance &&
            instance.Parent == Parent &&
            instance.Type == Type &&
            instance.Iri == Iri;

        /// <inheritdoc/>
        public override int GetHashCode() => _hashCode;

        /// <inheritdoc/>
        public static bool operator ==(Instance? obj1, Instance? obj2) => object.Equals(obj1, obj2);

        /// <inheritdoc/>
        public static bool operator !=(Instance? obj1, Instance? obj2) => !object.Equals(obj1, obj2);
    }

    /// <summary>
    /// Class that uniquely describes a predicate.
    /// </summary>
    public sealed class Predicate
    {
        private readonly int _hashCode;

        /// <summary>
        /// Creates a new <see cref="Predicate"/>.
        /// </summary>
        /// <param name="iri">The predicate's <see cref="GraphSPARQL.Iri"/>.</param>
        /// <param name="dataSource">The <see cref="SparqlDataSource"/> that contains the predicate.</param>
        /// <param name="graphIri">The graph <see cref="GraphSPARQL.Iri"/> that contains the predicate.</param>
        /// <param name="inversed"><c>true</c> if the subject should be queried from a given object, <c>false</c> for the other way around.</param>
        /// <param name="filter">The <see cref="Filter"/> to use or <c>null</c> to query the predicate unfiltered.</param>
        public Predicate(Iri iri, SparqlDataSource dataSource, Iri? graphIri, bool inversed = false, Filter? filter = null)
        {
            DataSource = dataSource;
            GraphIri = graphIri;
            Iri = iri;
            Inversed = inversed;
            Filter = filter;
            _hashCode = iri.GetHashCode() ^ dataSource.GetHashCode() ^ (graphIri?.GetHashCode() ?? 0) ^ inversed.GetHashCode() ^ filter?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Creates a new <see cref="Predicate"/> based on an existing one with a different <see cref="Filter"/>.
        /// </summary>
        /// <param name="predicate">The existing <see cref="Predicate"/>.</param>
        /// <param name="filter">The new <see cref="Filter"/> to use.</param>
        public Predicate(Predicate predicate, Filter filter)
        {
            Iri = predicate.Iri;
            DataSource = predicate.DataSource;
            GraphIri = predicate.GraphIri;
            Inversed = predicate.Inversed;
            Filter = filter;
            _hashCode = predicate._hashCode ^ predicate.Filter?.GetHashCode() ?? 0 ^ filter.GetHashCode();
        }

        /// <summary>
        /// The predicate's <see cref="GraphSPARQL.Iri"/>.
        /// </summary>
        public Iri Iri { get; }

        /// <summary>
        /// The <see cref="SparqlDataSource"/> that contains the predicate.
        /// </summary>
        public SparqlDataSource DataSource { get; }

        /// <summary>
        /// The graph <see cref="GraphSPARQL.Iri"/> that contains the predicate.
        /// </summary>
        public Iri? GraphIri { get; }

        /// <summary>
        /// <c>true</c> if the subject should be queried from a given object, <c>false</c> for the other way around.
        /// </summary>
        public bool Inversed { get; }

        /// <summary>
        /// Gets the <see cref="Filter"/> to use or <c>null</c> to query the predicate unfiltered.
        /// </summary>
        public Filter? Filter { get; }

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is Predicate entry &&
            entry.Iri == Iri &&
            entry.DataSource == DataSource &&
            entry.GraphIri == GraphIri &&
            entry.Inversed == Inversed &&
            entry.Filter == Filter;

        /// <inheritdoc/>
        public override int GetHashCode() => _hashCode;

        /// <inheritdoc/>
        public static bool operator ==(Predicate? obj1, Predicate? obj2) => object.Equals(obj1, obj2);

        /// <inheritdoc/>
        public static bool operator !=(Predicate? obj1, Predicate? obj2) => !object.Equals(obj1, obj2);
    }

    /// <summary>
    /// Class that describes a RDF query.
    /// </summary>
    public sealed class Request
    {
        private readonly int _hashCode;

        /// <summary>
        /// Creates a new <see cref="Request"/>.
        /// </summary>
        /// <param name="subject">The subject's <see cref="Iri"/> of the requested triple.</param>
        /// <param name="predicate">The <see cref="Query.Predicate"/> which can also include a <see cref="Filter"/>.</param>
        /// <param name="includeTypeInfo">Indicates whether the object's type <see cref="Iri"/>s should be returned.</param>
        public Request(Iri? subject, Predicate predicate, bool includeTypeInfo)
        {
            Subject = subject;
            Predicate = predicate;
            IncludeTypeInfo = includeTypeInfo;
            _hashCode = subject?.GetHashCode() ?? 0 ^ predicate.GetHashCode() ^ includeTypeInfo.GetHashCode();
        }

        /// <summary>
        /// The subject's <see cref="Iri"/> of the requested triple.
        /// </summary>
        /// <remarks>If <see cref="Predicate.Inversed"/> of <see cref="Predicate"/> is <c>true</c>, this is the object's <see cref="Iri"/>.</remarks>
        public Iri? Subject { get; }

        /// <summary>
        /// The <see cref="Query.Predicate"/> which can also include a <see cref="Filter"/>.
        /// </summary>
        public Predicate Predicate { get; }

        /// <summary>
        /// Indicates whether the object's type <see cref="Iri"/>s should be returned.
        /// </summary>
        public bool IncludeTypeInfo { get; }

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is Request query &&
            query.Subject == Subject &&
            query.Predicate == Predicate &&
            query.IncludeTypeInfo == IncludeTypeInfo;

        /// <inheritdoc/>
        public override int GetHashCode() => _hashCode;

        /// <inheritdoc/>
        public static bool operator ==(Request? obj1, Request? obj2) => object.Equals(obj1, obj2);

        /// <inheritdoc/>
        public static bool operator !=(Request? obj1, Request? obj2) => !object.Equals(obj1, obj2);
    }

    /// <summary>
    /// Class that represents a RDF result.
    /// </summary>
    public sealed class Response
    {
        internal Response(INode @object) : this(@object, Enumerable.Empty<Iri>()) { }

        /// <summary>
        /// Creates a new <see cref="Response"/>.
        /// </summary>
        /// <param name="object">The object's <see cref="INode"/>.</param>
        /// <param name="types">The <paramref name="object"/>'s types <see cref="Iri"/>s.</param>
        public Response(INode @object, IEnumerable<Iri> types)
        {
            Object = @object;
            Types = types;
        }

        /// <summary>
        /// The object's <see cref="INode"/>.
        /// </summary>
        public INode Object { get; }

        /// <summary>
        /// The <see cref="Object"/>'s types <see cref="Iri"/>s.
        /// </summary>
        public IEnumerable<Iri> Types { get; }
    }
}
