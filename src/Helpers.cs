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
using GraphQL.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace UIBK.GraphSPARQL
{
    internal static class Constants
    {
        public const string FieldsFieldName = "_fields";
        public const string IdFieldName = "_id";
        public const string InputTypeNamePrefix = "_Input";
        public const string ParentFilterVariableName = "__parent";
        public const string SelfFilterVariableName = "_";
    }

    /// <summary>
    /// Class representing an IRI.
    /// </summary>
    /// <remarks>Used instead of <see cref="Uri"/> to support strict equality.</remarks>
    [JsonConverter(typeof(JsonConverter))]
    public class Iri : IEquatable<Iri>
    {
        private sealed class JsonConverter : Newtonsoft.Json.JsonConverter
        {
            /// <inheritdoc/>
            public override bool CanConvert(Type objectType) => objectType == typeof(Iri);

            public override bool CanRead => false;

            /// <inheritdoc/>
            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => throw new NotImplementedException();

            /// <inheritdoc/>
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                switch (value)
                {
                    case Iri iri:
                        writer.WriteValue(iri.ToString());
                        break;
                    case null:
                        writer.WriteNull();
                        break;
                    default:
                        throw new JsonSerializationException($"Invalid value type {value.GetType().Name}, expected {nameof(Iri)}.");
                }
            }
        }

        private class RdfNode : BaseUriNode
        {
            public RdfNode(Iri iri) : base(iri.Uri) { }
        }

        private class RdfToken : BaseToken
        {
            public RdfToken(Iri iri) : base(VDS.RDF.Parsing.Tokens.Token.URI, iri._s, 0, 0, 0, 0) { }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private static string StringFromUri(Uri uri)
        {
            var s = uri.OriginalString;
            return string.IsNullOrEmpty(s) ? uri.AbsoluteUri : s;
        }

        private readonly string _s;
        private RdfNode? _node;
        private Uri? _uri;

        internal IUriNode Node
        {
            get
            {
                if (_node is null) _node = new RdfNode(this);
                return _node;
            }
        }

        internal ConstantTerm Term => new ConstantTerm(Node);

        internal IToken Token => new RdfToken(this);

        internal Uri Uri
        {
            get
            {
                if (_uri is null) _uri = new Uri(_s, UriKind.RelativeOrAbsolute);
                return _uri;
            }
        }

        /// <summary>
        /// Creates a new IRI from a <see cref="string"/>.
        /// </summary>
        /// <param name="s">The string representation.</param>
        public Iri(string s) => _s = s;

        /// <summary>
        /// Creates a new IRI from an <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">The URI representation.</param>
        public Iri(Uri uri)
        {
            _s = StringFromUri(uri);
            _uri = uri;
        }

        /// <summary>
        /// Create a new IRI from a base IRI and relative name.
        /// </summary>
        /// <param name="baseIri">The base <see cref="Iri"/>.</param>
        /// <param name="name">The relative name.</param>
        public Iri(Iri baseIri, string name) : this(baseIri._s + name) { }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj switch
        {
            string s => s == _s,
            Iri iri => iri._s == _s,
            Uri uri => StringFromUri(uri) == _s,
            _ => false,
        };

        /// <inheritdoc/>
        public override int GetHashCode() => _s.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => _s;

        /// <inheritdoc/>
        public bool Equals([AllowNull] Iri other) => other?._s == _s;

        /// <inheritdoc/>
        public static bool operator ==(Iri? iri1, Iri? iri2) => iri1?._s == iri2?._s;

        /// <inheritdoc/>
        public static bool operator !=(Iri? iri1, Iri? iri2) => iri1?._s != iri2?._s;

        /// <inheritdoc/>
        [return: NotNullIfNotNull("s")]
        public static implicit operator Iri?(string? s) => s is null ? null : new Iri(s);

        /// <inheritdoc/>
        [return: NotNullIfNotNull("uri")]
        public static implicit operator Iri?(Uri? uri) => uri is null ? null : new Iri(uri);

        /// <inheritdoc/>
        public static implicit operator PatternItem(Iri iri) => new NodeMatchPattern(iri.Node);
    }

    internal sealed class GraphUri : Uri
    {
        public GraphUri(Uri uri) : base(string.IsNullOrEmpty(uri.OriginalString) ? uri.AbsoluteUri : uri.OriginalString) { }

        public override string ToString() => OriginalString;
    }

    internal delegate Exception ExceptionBuilder(string s);

    internal static class Extensions
    {
        public static ISparqlExpression ReplaceVar(this ISparqlExpression expr, string findVar, string replaceVar) => new VariableSubstitutionTransformer(findVar, replaceVar).Transform(expr);

        public static IGraphPatternBuilder GraphIf(this IGraphPatternBuilder builder, Uri? graphUri, Action<IGraphPatternBuilder> buildGraphPattern)
        {
            if (graphUri is null) buildGraphPattern(builder);
            else builder.Graph(new GraphUri(graphUri), buildGraphPattern);
            return builder;
        }

        public static IGraphPatternBuilder OptionalWhere(this IGraphPatternBuilder builder, Action<ITriplePatternBuilder> buildTriplePatterns) => builder.Optional(opt => opt.Where(buildTriplePatterns));

        public static bool HasArgument(this IResolveFieldContext context, QueryArgument arg) => context.HasArgument(arg.Name);

        public static T GetArgument<T>(this IResolveFieldContext context, QueryArgument arg, T defaultValue = default) => context.GetArgument(arg.Name, defaultValue)!;

        public static T RequireProperty<T>(this T? value, [CallerMemberName] string? propertyName = null) where T : class => value ?? throw new InvalidOperationException($"Property '{propertyName}' not set.");

        public static T RequireProperty<T>(this T? value, [CallerMemberName] string? propertyName = null) where T : struct => value ?? throw new InvalidOperationException($"Property '{propertyName}' not set.");

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? enumerable) => enumerable ?? Enumerable.Empty<T>();

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable) action(item);
        }
    }
}
