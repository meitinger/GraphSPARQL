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
using GraphQL.Conversion;
using GraphQL.Introspection;
using GraphQL.Types;
using GraphQL.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UIBK.GraphSPARQL.Configuration;
using UIBK.GraphSPARQL.DataSource;

namespace UIBK.GraphSPARQL.Types
{
    /// <summary>
    /// Interface for schema providers.
    /// </summary>
    /// <remarks>Inherit from <see cref="Configuration.JsonElement"/> as well if want your provider settings to be deserialized from the main configuration file.</remarks>
    public interface ISchemaProvider
    {
        /// <summary>
        /// Populates the given schema with additional elements.
        /// </summary>
        /// <param name="schema">The <see cref="Schema"/> to populate, which may or may not be empty.</param>
        void FillSchema(Schema schema);
    }

    /// <summary>
    /// Root class describing the GraphQL schema.
    /// </summary>
    public sealed class Schema : MetadataProvider, ISchema, ISchemaElement, IServiceProvider
    {
        private static readonly IEnumerable<SchemaScalar> SpecialScalars = ImmutableArray.Create
        (
            SchemaScalar.Duration(),
            SchemaScalar.Date(),
            SchemaScalar.Time(),
            SchemaScalar.DateTime(),
            SchemaScalar.LanguageString()
        );

        private static readonly IEnumerable<IAstFromValueConverter> ValueConverters = ImmutableArray.Create<IAstFromValueConverter>
        (
            new SchemaDurationScalar.Representation.ScalarValueConverter(),
            new SchemaDateScalar.Representation.ScalarValueConverter(),
            new SchemaTimeScalar.Representation.ScalarValueConverter(),
            new SchemaDateScalar.Representation.ScalarValueConverter(),
            new SchemaLanguageStringScalar.Representation.ScalarValueConverter()
        );

        /// <summary>
        /// Gets a read-only dictionary mapping names to build-in scalars with default data type IRI and formatting.
        /// </summary>
        public static IReadOnlyDictionary<string, SchemaScalar> BuiltinScalars { get; } = new SchemaScalar[]
        {
            SchemaScalar.Boolean(),
            SchemaScalar.String(),
            SchemaScalar.Int(),
            SchemaScalar.Float(),
            SchemaScalar.Id(),
        }.Concat(SpecialScalars).ToImmutableDictionary(s => s.Name);

        private readonly IDictionary<string, SchemaTypeElement> _elements = new SortedDictionary<string, SchemaTypeElement>(StringComparer.Ordinal);
        private readonly IDictionary<SchemaField, SchemaField> _fields = new Dictionary<SchemaField, SchemaField>(SchemaField.EqualityComparer.Instance);
        private readonly Lazy<GraphTypesLookup> _lookup;
        private readonly SchemaMutation? _mutation;
        private readonly SchemaQuery? _query;
        private readonly IServiceProvider _serviceProvider;
        private bool _resolveLocked = false;
        private Action? _resolve;

        /// <inheritdoc cref="GraphQL.Types.Schema.Schema()"/>
        public Schema() : this(new DefaultServiceProvider()) { }

        /// <inheritdoc cref="GraphQL.Types.Schema.Schema(IServiceProvider)"/>
        public Schema(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _lookup = new Lazy<GraphTypesLookup>(BuildTypesLookup);
            _query = AddFromApi(new SchemaQuery(this));
            _mutation = AddFromApi(new SchemaMutation(this));
        }

        private T AddFromApi<T>(T element) where T : SchemaTypeElement => AddInternal(element, s => new ArgumentException(s), false);

        private void AddFromJson<T>(T element) where T : SchemaTypeElement => AddInternal(element, element.JsonError);

        private GraphTypesLookup BuildTypesLookup()
        {
            _resolveLocked = true;
            Debug.WriteLine($"{_resolve?.GetInvocationList()?.Length ?? 0} resolve entries registered.");
            _resolve?.Invoke();
            Debug.WriteLine($"{_fields.Count} fields registered");
            var result = GraphTypesLookup.Create
            (
                Elements
                    .Select(e => e.QueryType)
                    .Concat(Types.Select(t => t.MutationType))
                    .Concat(SpecialScalars.Select(scalar => scalar.QueryType)),
                Directives,
                type => (IGraphType)_serviceProvider.GetRequiredService(type),
                NameConverter,
                true
            );
            var totalTypes = result.All().Count();
            Debug.WriteLine($"{totalTypes} types registered.");
            return result;
        }

        private void EnsureNotInitialized()
        {
            if (Initialized) throw new InvalidOperationException("Schema has already been initialized.");
        }

        internal SchemaField AddInternal(SchemaField field)
        {
            // the following is an optimization that we can use reference equal on fields
            EnsureNotInitialized();
            field.EnsureSameSchema(this);
            if (_fields.TryGetValue(field, out var existingField)) return existingField;
            _fields.Add(field, field);
            return field;
        }

        internal T AddInternal<T>(T element, ExceptionBuilder error, bool allowMerge = true) where T : SchemaTypeElement
        {
            EnsureNotInitialized();
            element.EnsureSameSchema(this);
            if (BuiltinScalars.ContainsKey(element.Name)) throw error($"Name '{element.Name}' is reserved.");
            if (_elements.TryGetValue(element.Name, out var existingElement))
            {
                if (!allowMerge || existingElement is not T existingT) throw error($"Another {existingElement.GetType().Name} with the name '{element.Name}' was already created.");
                element.MergeInto(existingT, error);
                return existingT;
            }
            _elements.Add(element.Name, element);
            return element;
        }

        internal T GetElement<T>(string name, ExceptionBuilder error) where T : SchemaTypeElement => TryGetElement<T>(name, out var element) ? element : throw error($"{typeof(T).Name} element '{name}' not found.");

        internal event Action? Resolve
        {
            add
            {
                if (_resolveLocked) throw new InvalidOperationException("Resolve is not allowed in this state.");
                _resolve += value;
            }
            remove => throw new NotSupportedException();
        }

        /// <summary>
        /// Return a collection of all available <see cref="SparqlDataSource"/>s.
        /// </summary>
        [JsonIgnore]
        public SparqlDataSourceCollection DataSources { get; } = new SparqlDataSourceCollection();

        /// <inheritdoc/>
        [JsonIgnore]
        public IEnumerable<DirectiveGraphType> Directives
        {
            get
            {
                yield return DirectiveGraphType.Include;
                yield return DirectiveGraphType.Skip;
                yield return DirectiveGraphType.Deprecated;
            }
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Enumerates all <see cref="SchemaElement"/>s within the <see cref="Schema"/>.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<SchemaTypeElement> Elements => _elements.Values;

        /// <inheritdoc/>
        [JsonIgnore]
        public bool Initialized => _lookup.IsValueCreated;

        /// <inheritdoc/>
        [JsonIgnore]
        public INameConverter NameConverter { get; set; } = CamelCaseNameConverter.Instance;

        /// <summary>
        /// Parses a given configuration file.
        /// </summary>
        /// <param name="fileName">The path to the configuration file.</param>
        /// <returns>The current <see cref="Schema"/>.</returns>
        public Schema Configure(string fileName)
        {
            EnsureNotInitialized();
            fileName = Path.GetFullPath(fileName);
            Debug.WriteLine($"Configure schema using '{fileName}'.");
            using var stream = new StreamReader(fileName);
            using var reader = new JsonTextReader(stream);
            var configuration = new JsonContext(reader, fileName, this).Deserialize<SchemaConfiguration>() ?? throw new JsonException($"Failed to deserialize the configuration file '{fileName}'.");
            DataSources.AddRange(configuration.DataSources);
            configuration.Definitions.ForEach(def => def.Provider.FillSchema(this));
            return this;
        }

        /// <summary>
        /// Tries to find a <see cref="SchemaElement"/> with a given name.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaTypeElement"/> to retrieve.</param>
        /// <param name="element">The matching <see cref="SchemaTypeElement"/>.</param>
        /// <returns><c>true</c> if the <see cref="Schema"/> contains the specified <see cref="SchemaTypeElement"/>, <c>false</c> otherwise.</returns>
        public bool TryGetElement(string name, [NotNullWhen(returnValue: true)] out SchemaTypeElement? element) => _elements.TryGetValue(name, out element);

        /// <summary>
        /// Tries to find a <see cref="SchemaElement"/> of type <typeparamref name="T"/> with a given name.
        /// </summary>
        /// <typeparam name="T">The desired <see cref="SchemaTypeElement"/> type.</typeparam>
        /// <param name="name">The name of the <see cref="SchemaTypeElement"/> to retrieve.</param>
        /// <param name="element">The matching <see cref="SchemaTypeElement"/>.</param>
        /// <returns><c>true</c> if the <see cref="Schema"/> contains the specified <see cref="SchemaTypeElement"/>, <c>false</c> otherwise.</returns>
        public bool TryGetElement<T>(string name, [NotNullWhen(returnValue: true)] out T? element) where T : SchemaTypeElement => (element = TryGetElement(name, out var e) ? e as T : null) is not null;

        /// <summary>
        /// Gets the <see cref="SchemaQuery"/> for this <see cref="Schema"/>.
        /// </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse, Order = 1)]
        public SchemaQuery Query => _query.RequireProperty();

        /// <summary>
        /// Gets the <see cref="SchemaMutation"/> for this <see cref="Schema"/>.
        /// </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse, Order = 2)]
        public SchemaMutation Mutation => _mutation.RequireProperty();

        /// <summary>
        /// Creates and adds a new <see cref="SchemaEnum"/> to the <see cref="Schema"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaEnum"/>.</param>
        /// <param name="dataTypeIri">The <see cref="Iri"/> of the underlying data type.</param>
        /// <returns>A new <see cref="SchemaEnum"/> instance.</returns>
        /// <exception cref="ArgumentException">If another <see cref="SchemaType"/>, <see cref="SchemaInterface"/>, <see cref="SchemaUnion"/>, <see cref="SchemaCustomScalar"/> or <see cref="SchemaEnum"/> with the given <paramref name="name"/> already exists or <paramref name="name"/> contains invalid characters.</exception>
        /// <remarks>Use <see cref="SchemaScalar.IriDataTypeIri"/> for <paramref name="dataTypeIri"/> in case the enum values represent IRIs or <see cref="SchemaScalar.PlainLiteralDataTypeIri"/> to not use data types at all.</remarks>
        public SchemaEnum CreateEnum(string name, Iri dataTypeIri) => AddFromApi(new SchemaEnum(this, name, dataTypeIri));

        /// <summary>
        /// Enumerates all <see cref="SchemaEnum"/>s within the <see cref="Schema"/>.
        /// </summary>
        [JsonProperty(Order = 3)]
        public IEnumerable<SchemaEnum> Enums
        {
            get => Elements.OfType<SchemaEnum>();
            private set => value.ForEach(AddFromJson);
        }

        /// <summary>
        /// Creates and adds a new <see cref="SchemaInterface"/> to the <see cref="Schema"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaInterface"/>.</param>
        /// <returns>A new <see cref="SchemaInterface"/> instance.</returns>
        /// <exception cref="ArgumentException">If another <see cref="SchemaType"/>, <see cref="SchemaInterface"/>, <see cref="SchemaUnion"/>, <see cref="SchemaCustomScalar"/> or <see cref="SchemaEnum"/> with the given <paramref name="name"/> already exists or <paramref name="name"/> contains invalid characters.</exception>
        public SchemaInterface CreateInterface(string name) => AddFromApi(new SchemaInterface(this, name));

        /// <summary>
        /// Enumerates all <see cref="SchemaInterface"/>s within the <see cref="Schema"/>.
        /// </summary>
        [JsonProperty(Order = 4)]
        public IEnumerable<SchemaInterface> Interfaces
        {
            get => Elements.OfType<SchemaInterface>();
            private set => value.ForEach(AddFromJson);
        }

        /// <summary>
        /// Creates and adds a new <see cref="SchemaCustomScalar"/> to the <see cref="Schema"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaCustomScalar"/>.</param>
        /// <returns>A new <see cref="SchemaScalar"/> instance.</returns>
        /// <exception cref="ArgumentException">If another <see cref="SchemaType"/>, <see cref="SchemaInterface"/>, <see cref="SchemaUnion"/>, <see cref="SchemaCustomScalar"/> or <see cref="SchemaEnum"/> with the given <paramref name="name"/> already exists or <paramref name="name"/> contains invalid characters.</exception>
        public SchemaCustomScalar CreateScalar(string name) => AddFromApi(new SchemaCustomScalar(this, name));

        /// <summary>
        /// Enumerates all <see cref="SchemaCustomScalar"/>s within the <see cref="Schema"/>.
        /// </summary>
        [JsonProperty(Order = 5)]
        public IEnumerable<SchemaCustomScalar> Scalars
        {
            get => Elements.OfType<SchemaCustomScalar>();
            set => value.ForEach(AddFromJson);
        }

        /// <summary>
        /// Creates and adds a new <see cref="SchemaType"/> to the <see cref="Schema"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaType"/>.</param>
        /// <param name="classIri">The underlying class's IRI.</param>
        /// <returns>A new <see cref="SchemaType"/> instance.</returns>
        /// <exception cref="ArgumentException">If another <see cref="SchemaType"/>, <see cref="SchemaInterface"/>, <see cref="SchemaUnion"/>, <see cref="SchemaCustomScalar"/> or <see cref="SchemaEnum"/> with the given <paramref name="name"/> already exists or <paramref name="name"/> contains invalid characters.</exception>
        public SchemaType CreateType(string name, Iri classIri) => AddFromApi(new SchemaType(this, name, classIri));

        /// <summary>
        /// Enumerates all <see cref="SchemaType"/>s within the <see cref="Schema"/>.
        /// </summary>
        [JsonProperty(Order = 6)]
        public IEnumerable<SchemaType> Types
        {
            get => Elements.OfType<SchemaType>();
            private set => value.ForEach(AddFromJson);
        }

        /// <summary>
        /// Creates and adds a new <see cref="SchemaUnion"/> to the <see cref="Schema"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaUnion"/>.</param>
        /// <returns>A new <see cref="SchemaUnion"/> instance.</returns>
        /// <exception cref="ArgumentException">If another <see cref="SchemaType"/>, <see cref="SchemaInterface"/>, <see cref="SchemaUnion"/>, <see cref="SchemaCustomScalar"/> or <see cref="SchemaEnum"/> with the given <paramref name="name"/> already exists or <paramref name="name"/> contains invalid characters.</exception>
        public SchemaUnion CreateUnion(string name) => AddFromApi(new SchemaUnion(this, name));

        /// <summary>
        /// Enumerates all <see cref="SchemaUnion"/> within the <see cref="Schema"/>.
        /// </summary>
        [JsonProperty(Order = 7)]
        public IEnumerable<SchemaUnion> Unions
        {
            get => Elements.OfType<SchemaUnion>();
            private set => value.ForEach(AddFromJson);
        }

        /// <summary>
        /// Serializes the <see cref="Schema"/> and writes the JSON structure using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> used to write the JSON structure.</param>
        public void ToJson(TextWriter writer) => JsonContext.Writer().Serialize(writer, this);

        /// <summary>
        /// Serializes the <see cref="Schema"/> and writes the JSON structure using the specified <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="jsonWriter">The <see cref="JsonWriter"/> used to write the JSON structure.</param>
        public void ToJson(JsonWriter jsonWriter) => JsonContext.Writer().Serialize(jsonWriter, this);

        #region ISchema

        IEnumerable<Type> ISchema.AdditionalTypes => Enumerable.Empty<Type>();
        IEnumerable<IGraphType> ISchema.AllTypes => _lookup.Value.All();
        string? ISchema.Description { get; set; }
        ISchemaFilter ISchema.Filter { get; set; } = new DefaultSchemaFilter();
        IObjectGraphType? ISchema.Mutation { get => Mutation.QueryType; set => throw new NotSupportedException(); }
        IObjectGraphType? ISchema.Query { get => Query.QueryType; set => throw new NotSupportedException(); }
        FieldType ISchema.SchemaMetaFieldType => _lookup.Value.SchemaMetaFieldType;
        IObjectGraphType? ISchema.Subscription { get => null; set => throw new NotSupportedException(); }
        FieldType ISchema.TypeMetaFieldType => _lookup.Value.TypeMetaFieldType;
        FieldType ISchema.TypeNameMetaFieldType => _lookup.Value.TypeNameMetaFieldType;

        DirectiveGraphType? ISchema.FindDirective(string name) => Directives.FirstOrDefault(d => d.Name == name);
        IGraphType? ISchema.FindType(string name) => name.StartsWith("__") ? null : _lookup.Value[name];
        IAstFromValueConverter? ISchema.FindValueConverter(object value, IGraphType type) => ValueConverters.FirstOrDefault(v => v.Matches(value, type));
        void ISchema.Initialize() => _lookup.Value.All();
        void ISchema.RegisterDirective(DirectiveGraphType directive) => throw new NotSupportedException();
        void ISchema.RegisterDirectives(params DirectiveGraphType[] directives) => throw new NotSupportedException();
        void ISchema.RegisterType<T>() => throw new NotSupportedException();
        void ISchema.RegisterType(IGraphType type) => throw new NotSupportedException();
        void ISchema.RegisterTypes(params IGraphType[] types) => throw new NotSupportedException();
        void ISchema.RegisterTypes(params Type[] types) => throw new NotSupportedException();
        void ISchema.RegisterValueConverter(IAstFromValueConverter converter) => throw new NotSupportedException();

        #endregion

        bool ISchemaElement.HasSchema => true;
        Schema ISchemaElement.Schema => this;
        object IServiceProvider.GetService(Type serviceType) => _serviceProvider.GetService(serviceType);
    }
}
