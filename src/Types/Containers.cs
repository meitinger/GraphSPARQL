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

using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UIBK.GraphSPARQL.DataSource;
using UIBK.GraphSPARQL.Query;

namespace UIBK.GraphSPARQL.Types
{
    /// <summary>
    /// Base class of all containers.
    /// </summary>
    public abstract class SchemaContainer : SchemaTypeElement
    {
        private readonly IDictionary<string, SchemaField> _fields = new SortedDictionary<string, SchemaField>(StringComparer.Ordinal);
        private Uri? _graphUri;
        private IEnumerable<SchemaField>? _jsonFields;

        [JsonConstructor]
        internal SchemaContainer() : base() { }

        internal SchemaContainer(Schema schema, string name) : base(schema, name) { }

        /// <summary>
        /// Adds all fields from JSON.
        /// </summary>
        protected override void JsonInitialize()
        {
            _jsonFields.EmptyIfNull().ForEach(field => AddField(field, field.JsonError, NamespaceUri, DataSource, _graphUri));
            base.JsonInitialize();
        }

        [JsonIgnore]
        internal Uri? NamespaceUri { get; private set; }

        [JsonProperty(Order = OrderOfNamespace)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private Uri? Namespace
        {
            set => NamespaceUri = EnsureAbsoluteUri(value);
        }

        [JsonIgnore]
        internal SparqlDataSource? DataSource { get; private set; }

        [JsonProperty(PropertyName = "DataSource", Order = OrderOfDataSource)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private string? DataSourceName
        {
            set => DataSource = Schema.DataSources.GetByName(JsonTrace(value));
        }

        [JsonProperty(Order = OrderOfGraph)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private Uri? Graph
        {
            set => _graphUri = EnsureAbsoluteUri(value);
        }

        /// <summary>
        /// Enumerates all <see cref="SchemaContainer"/>s that will also receive any added <see cref="SchemaField"/>.
        /// </summary>
        [JsonIgnore]
        protected virtual IEnumerable<SchemaContainer> LinkedContainers { get; } = Enumerable.Empty<SchemaContainer>();

        /// <summary>
        /// Enumerates all <see cref="SchemaField"/>s that this <see cref="SchemaContainer"/> provides.
        /// </summary>
        [JsonProperty(Order = OrderOfFields)]
        public IEnumerable<SchemaField> Fields
        {
            get => _fields.Values;
            private set => _jsonFields = value;
        }

        /// <summary>
        /// Gets the number of <see cref="SchemaField"/>s added to this <see cref="SchemaContainer"/>.
        /// </summary>
        [JsonIgnore]
        public int FieldCount => _fields.Count;

        internal SchemaContainer AddFieldFromApi(SchemaField field) => AddFieldImmediately(field, s => new ArgumentException(s));

        internal SchemaContainer AddFieldImmediately(SchemaField field, ExceptionBuilder error, Uri? defaultNamespaceUri = null, SparqlDataSource? defaultDataSource = null, Uri? defaultGraphUri = null)
        {
            if (MergeTarget is not null) return ((SchemaContainer)MergeTarget).AddFieldImmediately(field, error, defaultNamespaceUri, defaultDataSource, defaultGraphUri);
            InitializeField(field, defaultNamespaceUri, defaultDataSource ?? Schema.DataSources.Default ?? throw error("No data source given and no default available."), defaultGraphUri, error);
            field = Schema.AddInternal(field); // ensures object equality

            //  ensure the field can be added
            if (_fields.TryGetValue(field.Name, out var existingField)) return field == existingField ? this : throw error($"Cannot add {field} because {existingField} has already been added to {this}.");
            foreach (var container in LinkedContainers)
            {
                if (container.TryGetField(field.Name, out existingField) && existingField != field)
                {
                    throw error($"Cannot add {field} to {this} because {container} already contains a different {existingField}.");
                }
            }

            // add the field to this container, query and linked containers
            _fields.Add(field.Name, field);
            RegisterField(field);
            LinkedContainers.ForEach(container => container.AddFieldImmediately(field, msg => new ApplicationException($"Unexpected exception when forwarding adding {field} from {this} to {container}: {msg}")));

            return this;
        }

        internal void AddField(SchemaField field, ExceptionBuilder error, Uri? defaultNamespaceUri = null, SparqlDataSource? defaultDataSource = null, Uri? defaultGraphUri = null) => Schema.Resolve += () => AddFieldImmediately(field, error, defaultNamespaceUri, defaultDataSource, defaultGraphUri);

        internal abstract void InitializeField(SchemaField field, Uri? defaultNamespaceUri, SparqlDataSource defaultDataSource, Uri? defaultGraphUri, ExceptionBuilder error);

        internal abstract void RegisterField(SchemaField field);

        /// <summary>
        /// Creates and adds a new <see cref="SchemaField"/> returning a <see cref="SchemaType"/> to this <see cref="SchemaContainer"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaField"/>.</param>
        /// <param name="type">The <see cref="SchemaType"/> the field returns.</param>
        /// <param name="predicateIri">The underlying predicate's <see cref="Iri"/>.</param>
        /// <param name="dataSource">The underlying <see cref="SparqlDataSource"/> or <c>null</c> to use the default one.</param>
        /// <param name="graphUri">The underlying graph <see cref="Uri"/> or <c>null</c> to use the default one.</param>
        /// <param name="isArray">If <c>true</c>, the field returns a list of objects.</param>
        /// <param name="isRequired">If <c>true</c>, the parent object is only returned if an object exists.</param>
        /// <param name="filter">An optional filter <see cref="string"/> that is passed to the data source.</param>
        /// <returns>The current <see cref="SchemaContainer"/>.</returns>
        /// <exception cref="ArgumentException">If another <see cref="SchemaField"/> with the same <paramref name="name"/> already exists, either in this container or an associated one, <paramref name="name"/> contains invalid characters or <paramref name="filter"/> cannot be parsed.</exception>
        /// <exception cref="InvalidOperationException">If the <paramref name="type"/> belongs to a different schema.</exception>
        public SchemaContainer AddField(string name, SchemaType type, Iri predicateIri, SparqlDataSource? dataSource = null, Uri? graphUri = null, bool isArray = true, bool isRequired = false, string? filter = null) =>
            AddFieldFromApi(new SchemaField(Schema, name, predicateIri, dataSource, graphUri, isArray, isRequired, filter, type));

        /// <summary>
        /// Creates and adds a new <see cref="SchemaField"/> returning a <see cref="SchemaInterface"/> to this <see cref="SchemaContainer"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaField"/>.</param>
        /// <param name="interface">The <see cref="SchemaInterface"/> the field returns.</param>
        /// <param name="predicateIri">The underlying predicate's <see cref="Iri"/>.</param>
        /// <param name="dataSource">The underlying <see cref="SparqlDataSource"/> or <c>null</c> to use the default one.</param>
        /// <param name="graphUri">The underlying graph <see cref="Uri"/> or <c>null</c> to use the default one.</param>
        /// <param name="isArray">If <c>true</c>, the field returns a list of objects.</param>
        /// <param name="isRequired">If <c>true</c>, the parent object is only returned if an object exists.</param>
        /// <param name="filter">An optional filter <see cref="string"/> that is passed to the data source.</param>
        /// <returns>The current <see cref="SchemaContainer"/>.</returns>
        /// <exception cref="ArgumentException">If another <see cref="SchemaField"/> with the same <paramref name="name"/> already exists, either in this container or an associated one, <paramref name="name"/> contains invalid characters or <paramref name="filter"/> cannot be parsed.</exception>
        /// <exception cref="InvalidOperationException">If the <paramref name="interface"/> belongs to a different schema.</exception>
        public SchemaContainer AddField(string name, SchemaInterface @interface, Iri predicateIri, SparqlDataSource? dataSource = null, Uri? graphUri = null, bool isArray = true, bool isRequired = false, string? filter = null) =>
            AddFieldFromApi(new SchemaField(Schema, name, predicateIri, dataSource, graphUri, isArray, isRequired, filter, @interface));

        /// <summary>
        /// Creates and adds a new <see cref="SchemaField"/> returning a <see cref="SchemaUnion"/> to this <see cref="SchemaContainer"/>.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaField"/>.</param>
        /// <param name="union">The <see cref="SchemaUnion"/> the field returns.</param>
        /// <param name="predicateIri">The underlying predicate's <see cref="Iri"/>.</param>
        /// <param name="dataSource">The underlying <see cref="SparqlDataSource"/> or <c>null</c> to use the default one.</param>
        /// <param name="graphUri">The underlying graph <see cref="Uri"/> or <c>null</c> to use the default one.</param>
        /// <param name="isArray">If <c>true</c>, the field returns a list of objects.</param>
        /// <param name="isRequired">If <c>true</c>, the parent object is only returned if an object exists.</param>
        /// <param name="filter">An optional filter <see cref="string"/> that is passed to the data source.</param>
        /// <returns>The current <see cref="SchemaContainer"/>.</returns>
        /// <exception cref="ArgumentException">If another <see cref="SchemaField"/> with the same <paramref name="name"/> already exists, either in this container or an associated one, <paramref name="name"/> contains invalid characters or <paramref name="filter"/> cannot be parsed.</exception>
        /// <exception cref="InvalidOperationException">If the <paramref name="union"/> belongs to a different schema.</exception>
        public SchemaContainer AddField(string name, SchemaUnion union, Iri predicateIri, SparqlDataSource? dataSource = null, Uri? graphUri = null, bool isArray = true, bool isRequired = false, string? filter = null) =>
            AddFieldFromApi(new SchemaField(Schema, name, predicateIri, dataSource, graphUri, isArray, isRequired, filter, union));

        /// <summary>
        /// Checks if the <see cref="SchemaContainer"/> contains a <see cref="SchemaField"/> with a given name.
        /// </summary>
        /// <param name="name">The name of a <see cref="SchemaField"/> to check.</param>
        /// <returns><c>true</c> if a <see cref="SchemaField"/> with the given <paramref name="name"/> has been added, <c>falce</c> otherwise.</returns>
        public bool ContainsField(string name) => _fields.ContainsKey(name);

        /// <summary>
        /// Tries to find and return an added <see cref="SchemaField"/> by name.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaField"/>.</param>
        /// <param name="field">The found <see cref="SchemaField"/>.</param>
        /// <returns><c>true</c> if the <see cref="SchemaField"/> with the given <paramref name="name"/> has been found, <c>falce</c> otherwise.</returns>
        public bool TryGetField(string name, [NotNullWhen(true)] out SchemaField? field) => _fields.TryGetValue(name, out field);
    }

    /// <summary>
    /// Base class of <see cref="SchemaType"/>, <see cref="SchemaInterface"/>, <see cref="SchemaQuery"/> and <see cref="SchemaMutation"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="SchemaTypeElement.QueryType"/>.</typeparam>
    public abstract class SchemaContainer<T> : SchemaContainer where T : IComplexGraphType, new()
    {
        private static readonly IGraphType FieldsGraphType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(new StringGraphType())));
        private static readonly FuncFieldResolver<IEnumerable<string>> FieldsResolver = new FuncFieldResolver<IEnumerable<string>>(context => context.ParentType.Fields.OrderBy(f => f.Name, StringComparer.InvariantCultureIgnoreCase).Select(f => f.Name));

        [JsonConstructor]
        internal SchemaContainer() : base() { }

        internal SchemaContainer(Schema schema, string name) : base(schema, name) { }

        internal sealed override IGraphType CreateQueryType()
        {
            var type = new T();
            type.AddField(new FieldType()
            {
                Name = Constants.FieldsFieldName,
                Description = "Retrieves all available fields.",
                ResolvedType = FieldsGraphType,
                Resolver = FieldsResolver
            });
            IntializeQueryType(type);
            return type;
        }

        internal virtual void IntializeQueryType(T type) { }

        internal override void RegisterField(SchemaField field) => QueryType.AddField(field.QueryType);

        /// <inheritdoc/>
        [JsonIgnore]
        public new T QueryType => (T)base.QueryType;
    }

    /// <summary>
    /// Base class of <see cref="SchemaQuery"/> and <see cref="SchemaMutation"/>.
    /// </summary>
    public abstract class SchemaRootContainer : SchemaContainer<ObjectGraphType>
    {
        internal SchemaRootContainer(Schema schema, string name) : base(schema, name) { }

        internal sealed override void InitializeField(SchemaField field, Uri? defaultNamespaceUri, SparqlDataSource defaultDataSource, Uri? defaultGraphUri, ExceptionBuilder error)
        {
            field.Initialize(SchemaField.RootFieldIri, defaultNamespaceUri, defaultDataSource, defaultGraphUri, error);
            if (field.Type != SchemaFieldType.Object) throw error("Root fields must be object fields.");
            if (field.IsRequired) throw error("Root fields cannot be required.");
            if (field.PredicateIri == SchemaField.CreateFieldIri)
            {
                if (field.Object is not SchemaType) throw error("Only concrete types can be created.");
            }
            else if (field.PredicateIri != SchemaField.RootFieldIri)
            {
                throw error("Root fields don't support custom predicates.");
            }
        }

        /// <inheritdoc/>
        [JsonIgnore]
        public override string Name => base.Name;
    }

    /// <summary>
    /// Base class of <see cref="SchemaType"/> and <see cref="SchemaInterface"/>.
    /// </summary>
    public abstract class SchemaObjectContainer<T> : SchemaContainer<T>, ISchemaObject where T : IComplexGraphType, new()
    {
        [JsonConstructor]
        internal SchemaObjectContainer() : base() => Definition = new SchemaObjectDefinition(this);

        internal SchemaObjectContainer(Schema schema, string name) : base(schema, name) => Definition = new SchemaObjectDefinition(this);

        [JsonIgnore]
        internal SchemaObjectDefinition Definition { get; }

        internal sealed override void InitializeField(SchemaField field, Uri? defaultNamespaceUri, SparqlDataSource defaultDataSource, Uri? defaultGraphUri, ExceptionBuilder error)
        {
            field.Initialize(null, defaultNamespaceUri, defaultDataSource, defaultGraphUri, error);
            if (field.PredicateIri == SchemaField.RootFieldIri || field.PredicateIri == SchemaField.CreateFieldIri) throw error($"Only root fields can have the predicate '{field.PredicateIri}'.");
        }

        /// <summary>
        /// Adds a new scalar field to this container.
        /// </summary>
        /// <param name="name">The name of field.</param>
        /// <param name="scalar">The <see cref="SchemaScalar"/> the field returns.</param>
        /// <param name="predicateIri">The underlying predicate's <see cref="Iri"/>.</param>
        /// <param name="dataSource">The underlying data source or <c>null</c> to use the default one.</param>
        /// <param name="graphUri">The underlying graph <see cref="Uri"/> or <c>null</c> to use the default one.</param>
        /// <param name="isArray">If <c>true</c>, the field returns a list of scalars.</param>
        /// <param name="isRequired">If <c>true</c>, the parent object is only returned if a scalar exists.</param>
        /// <param name="filter">An optional filter that is passed to the data source.</param>
        /// <returns>The current <see cref="SchemaContainer"/>.</returns>
        /// <exception cref="ArgumentException">If another field with the same <paramref name="name"/> already exists, either in this container or an associated one, <paramref name="name"/> contains invalid characters or <paramref name="filter"/> cannot be parsed.</exception>
        /// <exception cref="InvalidOperationException">If the <paramref name="scalar"/> belongs to a different schema.</exception>
        public SchemaContainer AddField(string name, SchemaScalar scalar, Iri predicateIri, SparqlDataSource? dataSource = null, Uri? graphUri = null, bool isArray = true, bool isRequired = false, string? filter = null) =>
            AddFieldFromApi(new SchemaField(Schema, name, predicateIri, dataSource, graphUri, isArray, isRequired, filter, scalar));

        SchemaObjectDefinition ISchemaObject.Definition => Definition;
    }

    /// <summary>
    /// Class providing the entry point for registering queryable data.
    /// </summary>
    public sealed class SchemaQuery : SchemaRootContainer
    {
        internal SchemaQuery(Schema schema) : base(schema, "Query") { }
    }

    /// <summary>
    /// Class providing the entry point for registering mutatable data.
    /// </summary>
    public sealed class SchemaMutation : SchemaRootContainer
    {
        internal SchemaMutation(Schema schema) : base(schema, "Mutation") { }

        /// <summary>
        /// Adds a new field that allows to create a new instance of a <see cref="SchemaType"/>.
        /// </summary>
        /// <param name="name">The name of <see cref="SchemaField"/>.</param>
        /// <param name="type">The <see cref="SchemaType"/> to create.</param>
        /// <param name="dataSource">The underlying <see cref="SparqlDataSource"/> or <c>null</c> to use the default one.</param>
        /// <param name="graphUri">The underlying graph <see cref="Uri"/> or <c>null</c> to use the default one.</param>
        /// <param name="isArray">If <c>true</c>, multiple instances can be created at once.</param>
        /// <param name="filter">An optional filter that used to check the instance before creation.</param>
        /// <returns>The current <see cref="SchemaMutation"/>.</returns>
        /// <exception cref="ArgumentException">If another field with the same <paramref name="name"/> already exists, <paramref name="name"/> contains invalid characters or <paramref name="filter"/> cannot be parsed.</exception>
        /// <exception cref="InvalidOperationException">If the <paramref name="type"/> belongs to a different schema.</exception>
        public SchemaMutation AddCreateField(string name, SchemaType type, SparqlDataSource? dataSource = null, Uri? graphUri = null, bool isArray = true, string? filter = null) =>
            (SchemaMutation)AddFieldFromApi(new SchemaField(Schema, name, SchemaField.CreateFieldIri, dataSource, graphUri, isArray, false, filter, type));
    }

    /// <summary>
    /// Class describing a named interface in GraphQL.
    /// </summary>
    public sealed class SchemaInterface : SchemaObjectContainer<InterfaceGraphType>
    {
        [JsonConstructor]
        private SchemaInterface() : base() { }

        internal SchemaInterface(Schema schema, string name) : base(schema, name) { }

        internal override void IntializeQueryType(InterfaceGraphType iface) => iface.ResolveType = Instance.TypeResolver;

        /// <inheritdoc/>
        protected override IEnumerable<SchemaContainer> LinkedContainers => Definition.Types;
    }

    /// <summary>
    /// Class describing a named type in GraphQL.
    /// </summary>
    public sealed class SchemaType : SchemaObjectContainer<ObjectGraphType>
    {
        private static readonly IGraphType IdGraphType = new NonNullGraphType(new IdGraphType());
        private static readonly FuncFieldResolver<Instance, string> IdResolver = new FuncFieldResolver<Instance, string>(context => context.Source.Iri.ToString());

        private readonly IDictionary<string, SchemaInterface> _interfaces = new SortedDictionary<string, SchemaInterface>(StringComparer.Ordinal);
        private Iri? _classIri;

        [JsonConstructor]
        private SchemaType() : base()
        {
            MutationType = new InputObjectGraphType();
            NonNullMutationType = new NonNullGraphType(MutationType);
        }

        internal SchemaType(Schema schema, string name, Iri classIri) : base(schema, name)
        {
            MutationType = new InputObjectGraphType();
            NonNullMutationType = new NonNullGraphType(MutationType);
            _classIri = classIri;
            InitializeClass();
        }

        /// <summary>
        /// Generates the IRI if necessary and initializes the class.
        /// </summary>
        protected override void JsonInitialize()
        {
            if (_classIri is null) _classIri = new Iri(NamespaceUri ?? (DataSource ?? Schema.DataSources.Default)?.DefaultNamespaceUri ?? throw JsonError("No default namespace, either the type's class or namespace must be given."), Name);
            InitializeClass();
            base.JsonInitialize();
        }

        private void InitializeClass()
        {
            Definition.AddType(this);
            QueryType.Description = $"Queries or updates an instance of class {ClassIri}.";
            MutationType.Name = Constants.InputTypeNamePrefix + Name;
            MutationType.Description = $"Creates a new instance of class {ClassIri}.";
        }

        internal override void IntializeQueryType(ObjectGraphType type) => type.AddField(new FieldType()
        {
            Name = Constants.IdFieldName,
            Description = "Retrieves the object's IRI.",
            ResolvedType = IdGraphType,
            Resolver = IdResolver
        });

        internal override void RegisterField(SchemaField field)
        {
            base.RegisterField(field);
            MutationType.AddField(field.MutationType);
        }

        /// <summary>
        /// Gets or sets the IRI of the underlying class.
        /// </summary>
        [JsonProperty(PropertyName = "Class", Order = OrderOfClass)]
        public Iri ClassIri
        {
            get => _classIri.RequireProperty();
            set => _classIri = value;
        }

        /// <summary>
        /// Enumerates all interfaces this type inherits from.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<SchemaInterface> Interfaces => _interfaces.Values;

        [JsonProperty(PropertyName = "Interfaces", Order = OrderOfInterfaces)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private IEnumerable<string> InterfacesNames
        {
            get => Interfaces.Select(iface => iface.Name);
            set
            {
                var trace = JsonTrace();
                value.ForEach(iface => AddInterface(iface, trace.Error));
            }
        }

        /// <summary>
        /// Gets the <see cref="InputObjectGraphType"/> used for creating instances.
        /// </summary>
        [JsonIgnore]
        public InputObjectGraphType MutationType { get; }

        /// <summary>
        /// Gets the <see cref="InputObjectGraphType"/> used for creating instances wrapped in a <see cref="NonNullGraphType"/>.
        /// </summary>
        [JsonIgnore]
        public NonNullGraphType NonNullMutationType { get; }

        private SchemaType AddInterfaceImmediately(SchemaInterface iface, ExceptionBuilder error)
        {
            if (MergeTarget is not null) return ((SchemaType)MergeTarget).AddInterfaceImmediately(iface, error);
            EnsureSameSchema(iface);
            if (_interfaces.ContainsKey(iface.Name)) return this; // must be same since same schema

            // ensure the interface can be added
            iface.Definition.EnsureCanAddType(this, error);
            foreach (var field in iface.Fields)
            {
                if (TryGetField(field.Name, out var existingField) && existingField != field)
                {
                    throw error($"Cannot implement {field} from {iface} because {this} already contains a different {existingField}.");
                }
            }

            // add the interface and its fields
            _interfaces.Add(iface.Name, iface);
            iface.Definition.AddType(this);
            iface.Fields.ForEach(field => AddFieldImmediately(field, msg => new ApplicationException($"Unexpected exception when pushing implementation of {field} from {iface} to {this}: {msg}")));

            // update the GraphQL part
            QueryType.AddResolvedInterface(iface.QueryType);
            iface.QueryType.AddPossibleType(QueryType);
            return this;
        }

        /// <summary>
        /// Declares that the type inherits from a given interface.
        /// </summary>
        /// <param name="interface">The <see cref="SchemaInterface"/> that this type should inherit from.</param>
        /// <returns>This current <see cref="SchemaType"/>.</returns>
        /// <exception cref="ArgumentException">Another type with the same <see cref="SchemaType.ClassIri"/> already inherits from the <paramref name="interface"/>.</exception>
        /// <exception cref="InvalidOperationException">If the <paramref name="interface"/> belongs to a different schema.</exception>
        public SchemaType AddInterface(SchemaInterface @interface) => AddInterfaceImmediately(@interface, s => new ArgumentException(s));

        internal void AddInterface(SchemaInterface @interface, ExceptionBuilder error) => Schema.Resolve += () => AddInterfaceImmediately(@interface, error);

        internal void AddInterface(string @interface, ExceptionBuilder error) => Schema.Resolve += () => AddInterfaceImmediately(Schema.GetElement<SchemaInterface>(@interface, error), error);

        /// <summary>
        /// Tries to find and return an added <see cref="SchemaInterface"/> by name.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaInterface"/>.</param>
        /// <param name="interface">The found <see cref="SchemaInterface"/>.</param>
        /// <returns><c>true</c> if the <see cref="SchemaInterface"/> with the given <paramref name="name"/> has been found, <c>falce</c> otherwise.</returns>
        public bool TryGetInterface(string name, [NotNullWhen(true)] out SchemaInterface? @interface) => _interfaces.TryGetValue(name, out @interface);

        /// <inheritdoc/>
        public override string ToString() => $"{base.ToString()}(Class={ClassIri})";
    }

    /// <summary>
    /// Class describing a named union in GraphQL.
    /// </summary>
    public sealed class SchemaUnion : SchemaTypeElement, ISchemaObject
    {
        private readonly SchemaObjectDefinition _definition;
        private readonly IDictionary<string, SchemaType> _types = new SortedDictionary<string, SchemaType>(StringComparer.Ordinal);

        [JsonConstructor]
        private SchemaUnion() : base() => _definition = new SchemaObjectDefinition(this);

        internal SchemaUnion(Schema schema, string name) : base(schema, name) => _definition = new SchemaObjectDefinition(this);

        private SchemaUnion AddTypeImmediately(SchemaType type, ExceptionBuilder error)
        {
            if (MergeTarget is not null) return ((SchemaUnion)MergeTarget).AddTypeImmediately(type, error);
            EnsureSameSchema(type);
            if (_types.ContainsKey(type.Name)) return this; // must be same since same schema
            _definition.EnsureCanAddType(type, error);
            _types.Add(type.Name, type);
            _definition.AddType(type);
            QueryType.AddPossibleType(type.QueryType);
            return this;
        }

        internal override IGraphType CreateQueryType() => new UnionGraphType() { ResolveType = Instance.TypeResolver };

        /// <inheritdoc/>
        [JsonIgnore]
        public new UnionGraphType QueryType => (UnionGraphType)base.QueryType;

        /// <summary>
        /// Enumerates all types that belong to the union.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<SchemaType> Types => _types.Values;

        [JsonProperty(PropertyName = "Types", Required = Required.Always, Order = OrderOfTypes)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private IEnumerable<string> TypesNames
        {
            get => Types.Select(type => type.Name);
            set
            {
                var trace = JsonTrace();
                value.ForEach(type => AddType(type, trace.Error));
            }
        }

        /// <summary>
        /// Declares a given type to be contained in this union.
        /// </summary>
        /// <param name="type">The <see cref="SchemaType"/> to add to this union.</param>
        /// <returns>The current <see cref="SchemaUnion"/>.</returns>
        /// <exception cref="ArgumentException">Another type with the same <see cref="SchemaType.ClassIri"/> has already been added.</exception>
        /// <exception cref="InvalidOperationException">If the <paramref name="type"/> belongs to a different schema.</exception>
        public SchemaUnion AddType(SchemaType type) => AddTypeImmediately(type, s => new ArgumentException(s));

        internal void AddType(SchemaType type, ExceptionBuilder error) => Schema.Resolve += () => AddTypeImmediately(type, error);

        internal void AddType(string type, ExceptionBuilder error) => Schema.Resolve += () => AddTypeImmediately(Schema.GetElement<SchemaType>(type, error), error);

        /// <summary>
        /// Tries to find and return an added <see cref="SchemaType"/> by name.
        /// </summary>
        /// <param name="name">The name of the <see cref="SchemaType"/>.</param>
        /// <param name="type">The found <see cref="SchemaType"/>.</param>
        /// <returns><c>true</c> if the <see cref="SchemaType"/> with the given <paramref name="name"/> has been found, <c>falce</c> otherwise.</returns>
        public bool TryGetType(string name, [NotNullWhen(true)] out SchemaType? type) => _types.TryGetValue(name, out type);

        SchemaObjectDefinition ISchemaObject.Definition => _definition;
    }
}
