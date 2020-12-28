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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using UIBK.GraphSPARQL.DataSource;
using UIBK.GraphSPARQL.Query;
using VDS.RDF.Query.Expressions;

namespace UIBK.GraphSPARQL.Types
{
    /// <summary>
    /// Enum that indicates the return type of a <see cref="SchemaField"/>.
    /// </summary>
    public enum SchemaFieldType
    {
        /// <summary>
        /// The <see cref="SchemaField"/> return an <see cref="SchemaInterface"/>, <see cref="SchemaType"/> or <see cref="SchemaUnion"/>.
        /// </summary>
        Object,

        /// <summary>
        /// The <see cref="SchemaField"/> return an <see cref="SchemaScalar"/>.
        /// </summary>
        Scalar,
    }

    /// <summary>
    /// Class for all fields in the schema.
    /// </summary>
    public sealed class SchemaField : SchemaElement
    {
        internal sealed class EqualityComparer : IEqualityComparer<SchemaField>
        {
            public static EqualityComparer Instance { get; } = new EqualityComparer();

            internal static int CalculateHashCode(SchemaField obj) =>
                obj.Name.GetHashCode() ^
                obj.PredicateIri.GetHashCode() ^
                obj.DataSource.GetHashCode() ^
                obj.GraphUri?.GetHashCode() ?? 0 ^
                obj.IsArray.GetHashCode() ^
                obj.IsRequired.GetHashCode() ^
                obj.Filter?.GetHashCode() ?? 0 ^
                obj.Value.GetHashCode();

            private EqualityComparer() { }

            public bool Equals([NotNullWhen(true)] SchemaField? x, [NotNullWhen(true)] SchemaField? y)
            {
                if (x == y) return true;
                if (x is null || y is null) return false;
                return
                    x.Name == y.Name &&
                    x.PredicateIri == y.PredicateIri &&
                    x.DataSource == y.DataSource &&
                    x.GraphUri == y.GraphUri &&
                    x.IsArray == y.IsArray &&
                    x.IsRequired == y.IsRequired &&
                    x.Filter == y.Filter &&
                    x.Value == y.Value;
            }

            public int GetHashCode(SchemaField obj) => obj.HashCode;
        }

        internal static readonly Iri CreateFieldIri = "https://schema.uibk.ac.at/GraphSPARQL/create";
        internal static readonly Iri RootFieldIri = "https://schema.uibk.ac.at/GraphSPARQL/queryOrUpdate";

        private SparqlDataSource? _dataSource;
        private Iri? _dataType;
        private ExceptionBuilder? _dataTypeError;
        private string? _description;
        private string? _filter;
        private string? _format;
        private ExceptionBuilder? _formatError;
        private Uri? _graphUri;
        private int? _hashCode;
        private bool _initialized;
        private SchemaScalar? _mutationScalar;
        private Iri? _predicateIri;
        private SchemaFieldType? _type;
        private ISchemaElement? _value;
        private string? _valueName;
        private ExceptionBuilder? _valueNameError;

        [JsonConstructor]
        private SchemaField() : base() { }

        private SchemaField(Schema schema, string name, Iri? predicateIri, SparqlDataSource? dataSource, Uri? graphUri, bool isArray, bool isRequired)
        : base(schema, name)
        {
            _predicateIri = predicateIri;
            _dataSource = dataSource;
            _graphUri = graphUri;
            IsArray = isArray;
            IsRequired = isRequired;
        }

        internal SchemaField(Schema schema, string name, Iri? predicateIri, SparqlDataSource? dataSource, Uri? graphUri, bool isArray, bool isRequired, string? filter)
        : this(schema, name, predicateIri, dataSource, graphUri, isArray, isRequired) => SetFilter(filter, s => new ArgumentException(s, nameof(filter)));

        internal SchemaField(Schema schema, string name, Iri predicateIri, SparqlDataSource? dataSource, Uri? graphUri, bool isArray, bool isRequired, string? filter, ISchemaObject @object)
        : this(schema, name, predicateIri, dataSource, graphUri, isArray, isRequired, filter) => Object = @object;

        internal SchemaField(Schema schema, string name, Iri predicateIri, SparqlDataSource? dataSource, Uri? graphUri, bool isArray, bool isRequired, string? filter, SchemaScalar scalar)
        : this(schema, name, predicateIri, dataSource, graphUri, isArray, isRequired, filter) => Scalar = scalar;

        internal SchemaField(Schema schema, string name, Iri? predicateIri, SparqlDataSource? dataSource, Uri? graphUri, bool isArray, bool isRequired, string? filter, string valueName, Iri? dataType, string? format, ExceptionBuilder error)
        : this(schema, name, predicateIri, dataSource, graphUri, isArray, isRequired)
        {
            _valueName = valueName;
            _dataType = dataType;
            _format = format;
            SetFilter(filter, error);
            Schema.Resolve += () => DoResolve(valueName, error);
        }

        /// <inheritdoc/>
        protected override void JsonInitialize()
        {
            if (_valueName is null) throw JsonError("Neither object name nor scalar name specified.");
            Schema.Resolve += () => DoResolve(_valueName, JsonError);
            base.JsonInitialize();
        }

        private void DoResolve(string name, ExceptionBuilder error)
        {
            if (_type != SchemaFieldType.Object && SchemaScalar.TryGetBuiltin(name, _dataType, _dataTypeError ?? error, _format, _formatError ?? error, out var builtinScalar))
            {
                Scalar = builtinScalar;
            }
            else
            {
                if (!Schema.TryGetElement(name, out var element)) throw (_valueNameError ?? error)($"Schema element '{name}' not found.");
                if (_dataType is not null || _format is not null) throw error($"Neither data type nor format are supported for {element}.");
                if (_type != SchemaFieldType.Scalar && element is ISchemaObject obj) Object = obj;
                else if (_type != SchemaFieldType.Object && element is SchemaScalar scalar) Scalar = scalar;
                else throw (_valueNameError ?? error)(_type.HasValue ? $"{element} is not a {_type}." : $"{element} is neither an object nor a scalar.");
            }
        }

        private void SetFilter(string? filter, ExceptionBuilder error)
        {
            FilterExpression = filter is null ? null : Query.Filter.Parse(filter, error);
            _filter = filter;
        }

        private void SetTypedFromJson<T>(ref T? variable, T? value, ref ExceptionBuilder? error, SchemaFieldType type) where T : class
        {
            if (_type.HasValue && _type.Value != type) throw JsonError("Object and scalar properties cannot be mixed.");
            variable = value;
            error = JsonTrace().Error;
            _type = type;
        }

        internal void Initialize(Iri? defaultPredicateIri, Uri? defaulNamespaceUri, SparqlDataSource defaultDataSource, Uri? defaultGraphUri, ExceptionBuilder error)
        {
            // only run this code once
            if (_initialized) return;
            _initialized = true;

            // set missing values from context
            if (_dataSource is null) _dataSource = defaultDataSource;
            if (_graphUri is null) _graphUri = defaultGraphUri ?? _dataSource.DefaultGraphUri;
            if (_predicateIri is null) _predicateIri = defaultPredicateIri ?? new Iri(defaulNamespaceUri ?? _dataSource.DefaultNamespaceUri ?? throw error($"No namespace given and {_dataSource} has no default."), Name);

            // finalize field
            _description = $"{_predicateIri} in {_graphUri?.ToString() ?? "default graph" } of {_dataSource}";
            _hashCode = EqualityComparer.CalculateHashCode(this);
        }

        internal IGraphType GetQueryTypeFromSchemaType(ISchemaTypeElement type) =>
            IsRequired
                ? (IsArray ? type.NonNullListQueryType : type.NonNullQueryType)
                : (IsArray ? type.ListQueryType : type.QueryType);

        #region Internal Properties

        [JsonIgnore]
        internal string Description => _description.RequireProperty();

        [JsonIgnore]
        internal ISparqlExpression? FilterExpression { get; private set; }

        [JsonIgnore]
        internal int HashCode => _hashCode.RequireProperty();

        [JsonIgnore]
        internal SchemaScalar MutationScalar => _mutationScalar.RequireProperty();

        [JsonIgnore]
        internal FieldType MutationType => new MutationField(this);

        [JsonIgnore]
        internal ISchemaObject Object
        {
            get => (_value as ISchemaObject).RequireProperty();
            private set
            {
                EnsureSameSchema(value);
                (value as SchemaTypeElement)?.LockMerge();
                _value = value;
                _valueName = value.Name;
                _dataType = null;
                _format = null;
                _type = SchemaFieldType.Object;
                _mutationScalar = SchemaScalar.Id();
            }
        }

        [JsonIgnore]
        internal FieldType QueryType => Type switch
        {
            SchemaFieldType.Object =>
               _predicateIri == CreateFieldIri
                   ? new CreationFieldType(this)
                   : _predicateIri == RootFieldIri
                       ? new RootFieldType(this)
                       : new ChildFieldType(this),
            SchemaFieldType.Scalar => new ScalarFieldType(this),
            _ => throw new NotImplementedException()
        };

        [JsonIgnore]
        internal SchemaScalar Scalar
        {
            get => (_value as SchemaScalar).RequireProperty();
            private set
            {
                EnsureSameSchema(value);
                _value = value;
                _valueName = value.Name;
                _dataType = value is ISchemaDataTypedScalar dataTypedScalar ? dataTypedScalar.DataTypeIri : null;
                _format = value is ISchemaFormattableScalar formattableScalar ? formattableScalar.Format : null;
                _type = SchemaFieldType.Scalar;
                _mutationScalar = value;
            }
        }

        [JsonIgnore]
        internal ISchemaElement Value => _value.RequireProperty();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the data source.
        /// </summary>
        [JsonIgnore]
        public SparqlDataSource DataSource => _dataSource.RequireProperty();

        /// <summary>
        /// Sets an optional filter that is passed to the data source.
        /// </summary>
        [JsonProperty(Order = OrderOfFilter)]
        public string? Filter
        {
            get => _filter;
            private set => SetFilter(value, JsonError);
        }

        /// <summary>
        /// Gets the source graph URI.
        /// </summary>
        [JsonProperty(PropertyName = "Graph", Order = OrderOfGraph)]
        public Uri? GraphUri
        {
            get => _graphUri;
            private set => _graphUri = EnsureAbsoluteUri(value);
        }

        /// <summary>
        /// Gets or sets whether this field returns an array.
        /// </summary>
        [JsonProperty(Order = OrderOfIsArray)]
        [DefaultValue(false)]
        public bool IsArray { get; private set; }

        /// <summary>
        /// Indicates whether the field is optional or required.
        /// </summary>
        [JsonProperty(Order = OrderOfIsRequired)]
        [DefaultValue(false)]
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Gets the <see cref="Iri"/> of the underlying predicate.
        /// </summary>
        [JsonProperty(PropertyName = "Predicate", Order = OrderOfPredicate)]
        public Iri PredicateIri
        {
            get => _predicateIri.RequireProperty();
            private set => _predicateIri = value;
        }

        /// <summary>
        /// Gets the <see cref="SchemaFieldType"/> of this <see cref="SchemaField"/>;
        /// </summary>
        [JsonIgnore]
        public SchemaFieldType Type => _type.RequireProperty();

        #endregion

        #region Private JSON Properties

        [JsonProperty(PropertyName = "DataSource", Order = OrderOfDataSource)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private string? DataSourceName
        {
            get => _dataSource?.Name;
            set => _dataSource = Schema.DataSources.GetByName(JsonTrace(value));
        }

        [JsonProperty(Order = OrderOfDataType)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private Iri? DataType
        {
            get => Type == SchemaFieldType.Scalar && Scalar is ISchemaDataTypedScalar dataTyped && dataTyped.DataTypeIri != dataTyped.DefaultDataTypeIri ? dataTyped.DataTypeIri : null;
            set => SetTypedFromJson(ref _dataType, value, ref _dataTypeError, SchemaFieldType.Scalar);
        }

        [JsonProperty(Order = OrderOfFormat)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private string? Format
        {
            get => Type == SchemaFieldType.Scalar && Scalar is ISchemaFormattableScalar formattable && formattable.Format != formattable.DefaultFormat ? formattable.Format : null;
            set => SetTypedFromJson(ref _format, value, ref _formatError, SchemaFieldType.Scalar);
        }

        [JsonProperty(PropertyName = "Object", Order = OrderOfObjectOrScalar)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private string? ObjectName
        {
            get => Type == SchemaFieldType.Object ? Object.Name : null;
            set => SetTypedFromJson(ref _valueName, value, ref _valueNameError, SchemaFieldType.Object);
        }

        [JsonProperty(PropertyName = "Scalar", Order = OrderOfObjectOrScalar)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private string? ScalarName
        {
            get => Type == SchemaFieldType.Scalar ? Scalar.Name : null;
            set => SetTypedFromJson(ref _valueName, value, ref _valueNameError, SchemaFieldType.Scalar);
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString() => $@"{base.ToString()}(Predicate={PredicateIri},DataSource={DataSource.Name},Graph={GraphUri},IsArray={IsArray},IsRequired={IsRequired},Filter={Filter},{_type switch
        {
            null => $"Value={_valueName}",
            SchemaFieldType.Object => $"Object={_valueName}",
            SchemaFieldType.Scalar => $"Scalar={_valueName},DataType={_dataType},Format={_format}",
            _ => throw new NotImplementedException()
        }})";
    }
}
