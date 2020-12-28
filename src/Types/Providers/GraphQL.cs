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

using GraphQLParser;
using GraphQLParser.AST;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using UIBK.GraphSPARQL.Configuration;
using UIBK.GraphSPARQL.DataSource;

namespace UIBK.GraphSPARQL.Types.Providers
{
    namespace GraphQL
    {
        internal abstract class Directive<T>
        {
            private readonly IDictionary<string, string> _values = new Dictionary<string, string>();

            public static string Name { get; } = typeof(T).Name.ToLowerInvariant();

            public IEnumerable<GraphQLArgument> Arguments
            {
                set
                {
                    foreach (var arg in value)
                    {
                        var name = arg.Name?.Value;
                        if (name is null) continue;
                        if (arg.Value is not GraphQLScalarValue s || s.Value is null) continue;
                        _values[name] = s.Value;
                    }
                }
            }

            public Schema? Schema { private get; set; }

            public ExceptionBuilder Error { private get; set; } = s => new InvalidOperationException(s);

            protected string? Optional([CallerMemberName] string name = "") => _values.TryGetValue(name.ToLowerInvariant(), out var value) ? value : null;

            protected Iri? OptionalIri([CallerMemberName] string name = "")
            {
                var value = Optional(name);
                return value is null ? null : new Iri(value);
            }

            protected Uri? OptionalUri([CallerMemberName] string name = "")
            {
                var value = Optional(name);
                return value is null ? null : new Uri(value);
            }

            protected SparqlDataSource? OptionalDataSource([CallerMemberName] string name = "")
            {
                var value = Optional(name);
                return value is null ? null : (Schema ?? throw Error("Cannot resolve data source, no schema given.")).DataSources.GetByName(name, Error);
            }

            protected string Mandatory([CallerMemberName] string name = "") => Optional(name) ?? throw Error($"Argument {name.ToLowerInvariant()} not set.");
        }

        internal sealed class Context : Directive<Context>
        {
            public Uri? Namespace => OptionalUri();

            public SparqlDataSource? DataSource => OptionalDataSource();

            public Uri? Graph => OptionalUri();
        }

        internal sealed class DataType : Directive<DataType>
        {
            public Iri Iri => Mandatory();

            public Iri? IriOptional => OptionalIri("Iri");

            public string? Format => Optional();
        }

        internal sealed class Value : Directive<Value>
        {
            public string Literal => Mandatory();
        }

        internal sealed class Class : Directive<Class>
        {
            public Iri Iri => Mandatory();
        }

        internal sealed class Predicate : Directive<Predicate>
        {
            public Iri? Iri => OptionalIri();

            public SparqlDataSource? DataSource => OptionalDataSource();

            public Uri? Graph => OptionalUri();

            public string? Filter => Optional();
        }
    }

    internal sealed class GraphQLHarvester
    {
        private readonly Schema _schema;
        private readonly Uri _location;
        private readonly ExceptionBuilder _error;

        internal GraphQLHarvester(Schema schema, GraphQLProvider settings, ExceptionBuilder error)
        {
            _schema = schema;
            _location = settings.Location;
            _error = error;
        }

        private void HarvestFields(SchemaContainer container, IHasDirectivesNode node, IEnumerable<GraphQLFieldDefinition>? fields)
        {
            var context = SingleOrDefault<GraphQL.Context>(node);
            foreach (var fieldDef in fields.EmptyIfNull())
            {
                var predicate = SingleOrDefault<GraphQL.Predicate>(fieldDef);
                var dataType = SingleOrDefault<GraphQL.DataType>(fieldDef);
                Exception error() => _error($"Type of field '{fieldDef.Name?.Value}' in {FormatNode(fieldDef)} not supported.");
                (var isArray, var isRequired, var valueName) = fieldDef.Type switch
                {
                    GraphQLNonNullType nonNull => nonNull.Type switch
                    {
                        GraphQLListType nonNullList => nonNullList.Type switch
                        {
                            GraphQLNonNullType nonNullListNonNull => nonNullListNonNull.Type is GraphQLNamedType nonNullListNonNullType ? (true, true, Name(nonNullListNonNullType)) : throw error(),
                            GraphQLNamedType nonNullListType => (true, true, Name(nonNullListType)),
                            _ => throw error(),
                        },
                        GraphQLNamedType nonNullType => (false, true, Name(nonNullType)),
                        _ => throw error(),
                    },
                    GraphQLListType list => list.Type switch
                    {
                        GraphQLNonNullType listNonNull => listNonNull.Type is GraphQLNamedType listNonNullType ? (true, false, Name(listNonNullType)) : throw error(),
                        GraphQLNamedType listNamed => (true, false, Name(listNamed)),
                        _ => throw error(),
                    },
                    GraphQLNamedType named => (false, false, Name(named)),
                    _ => throw error(),
                };
                var field = new SchemaField(_schema, Name(fieldDef), predicate?.Iri, predicate?.DataSource, predicate?.Graph, isArray, isRequired, predicate?.Filter, valueName, dataType?.IriOptional, dataType?.Format, _error);
                container.AddField(field, _error, context?.Namespace, context?.DataSource, context?.Graph);
            }
        }

        private SchemaType? HarvestObject(GraphQLObjectTypeDefinition def, bool isExtension)
        {
            switch (Name(def))
            {
                case "Query":
                    HarvestFields(_schema.Query, def, def.Fields);
                    return null;
                case "Mutation":
                    HarvestFields(_schema.Mutation, def, def.Fields);
                    return null;
                default:
                    var type = isExtension ? _schema.GetElement<SchemaType>(Name(def), _error) : new SchemaType(_schema, Name(def), SingleOrDefault<GraphQL.Class>(def)?.Iri ?? new Iri(new Iri(SingleOrDefault<GraphQL.Context>(def)?.Namespace ?? throw _error($"Either @class.iri or @context.namespace must be set on {FormatNode(def)}.")), Name(def)));
                    HarvestFields(type, def, def.Fields);
                    def.Interfaces.EmptyIfNull().ForEach(iface => type.AddInterface(Name(iface), _error));
                    return type;
            }
        }

        private SchemaUnion HarvestUnion(GraphQLUnionTypeDefinition def)
        {
            var union = new SchemaUnion(_schema, Name(def));
            def.Types.EmptyIfNull().ForEach(type => union.AddType(Name(type), _error));
            return union;
        }

        private SchemaInterface HarvestInterface(GraphQLInterfaceTypeDefinition def)
        {
            var iface = new SchemaInterface(_schema, Name(def));
            HarvestFields(iface, def, def.Fields);
            return iface;
        }

        private SchemaEnum HarvestEnum(GraphQLEnumTypeDefinition def)
        {
            var enumeration = new SchemaEnum(_schema, Name(def), Single<GraphQL.DataType>(def).Iri);
            def.Values.EmptyIfNull().ForEach(value => enumeration.AddValue(Name(value), Single<GraphQL.Value>(value).Literal, _error));
            return enumeration;
        }

        private SchemaCustomScalar HarvestScalar(GraphQLScalarTypeDefinition def)
        {
            var scalar = new SchemaCustomScalar(_schema, Name(def));
            Multiple<GraphQL.DataType>(def).ForEach(dataType => scalar.AddDataType(dataType.Iri, _error));
            return scalar;
        }

        private string Name(INamedNode node) => SchemaElement.EnsureValidName(node.Name?.Value ?? throw _error($"Name of a {node.GetType().Name} node is missing."), node.GetType(), _error);

        private string FormatNode(IHasDirectivesNode node) => $"{node.GetType().Name} '{(node as INamedNode)?.Name?.Value}'";

        private T Single<T>(IHasDirectivesNode node) where T : GraphQL.Directive<T>, new() => SingleOrDefault<T>(node) ?? throw _error($"Directive '{GraphQL.Directive<T>.Name}' missing on {FormatNode(node)}.");

        private T? SingleOrDefault<T>(IHasDirectivesNode node) where T : GraphQL.Directive<T>, new()
        {
            using var results = Multiple<T>(node).GetEnumerator();
            if (!results.MoveNext()) return null;
            var result = results.Current;
            if (results.MoveNext()) throw _error($"Directive '{GraphQL.Directive<T>.Name}' occurs multiple times on {FormatNode(node)}.");
            return result;
        }

        private IEnumerable<T> Multiple<T>(IHasDirectivesNode node) where T : GraphQL.Directive<T>, new() => node
            .Directives
            .EmptyIfNull()
            .Where(d => d.Name?.Value == GraphQL.Directive<T>.Name)
            .Select(d => new T()
            {
                Arguments = d.Arguments.EmptyIfNull(),
                Error = s => _error($"Directive '{GraphQL.Directive<T>.Name}' on {FormatNode(node)}: {s}")
            });

        private string LoadSchema()
        {
            using var client = new WebClient();
            return client.DownloadString(_location);
        }

        public void Harvest()
        {
            var graphql = new Parser(new Lexer()).Parse(new Source(LoadSchema()));
            foreach (var definition in graphql.Definitions.EmptyIfNull())
            {
                var namedElement = definition switch
                {
                    GraphQLObjectTypeDefinition obj => HarvestObject(obj, false),
                    GraphQLTypeExtensionDefinition ext => ext.Definition is not null ? HarvestObject(ext.Definition, true) : null,
                    GraphQLUnionTypeDefinition union => HarvestUnion(union),
                    GraphQLInterfaceTypeDefinition iface => HarvestInterface(iface),
                    GraphQLEnumTypeDefinition enumeration => HarvestEnum(enumeration),
                    GraphQLScalarTypeDefinition scalar => HarvestScalar(scalar),
                    _ => (SchemaTypeElement?)null
                };
                if (namedElement is not null) _schema.AddInternal(namedElement, _error);
            }
        }
    }

    internal sealed class GraphQLProvider : JsonElement, ISchemaProvider
    {
        private Uri? _location;

        [JsonConstructor]
        private GraphQLProvider() { }

        [JsonProperty(Required = Required.Always)]
        public Uri Location
        {
            get => _location.RequireProperty();
            private set => _location = EnsureAbsolutePath(value);
        }

        public void FillSchema(Schema schema) => new GraphQLHarvester(schema, this, JsonError).Harvest();
    }
}
