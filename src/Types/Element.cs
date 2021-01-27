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
using GraphQL.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using UIBK.GraphSPARQL.Configuration;

namespace UIBK.GraphSPARQL.Types
{
    internal interface ISchemaElement
    {
        bool HasSchema { get; }
        Schema Schema { get; }
    }

    internal interface ISchemaTypeElement : ISchemaElement
    {
        IGraphType QueryType { get; }
        IGraphType NonNullQueryType { get; }
        IGraphType ListQueryType { get; }
        IGraphType NonNullListQueryType { get; }
    }

    /// <summary>
    /// Base class for every schema element.
    /// </summary>
    public abstract class SchemaElement : JsonElement, ISchemaElement
    {
        #region JSON order

        // common
        internal const int OrderOfName = 1;
        internal const int OrderOfDataSource = OrderOfName + 1;
        internal const int OrderOfGraph = OrderOfDataSource + 1;

        // fields and scalars
        internal const int OrderOfPredicate = OrderOfGraph + 1;
        internal const int OrderOfObjectOrScalar = OrderOfPredicate + 1;
        internal const int OrderOfDataType = OrderOfObjectOrScalar + 1;
        internal const int OrderOfFormat = OrderOfDataType + 1;
        internal const int OrderOfValues = OrderOfFormat + 1;
        internal const int OrderOfDataTypes = OrderOfValues + 1;
        internal const int OrderOfIsArray = OrderOfDataTypes + 1;
        internal const int OrderOfIsRequired = OrderOfIsArray + 1;
        internal const int OrderOfFilter = OrderOfIsRequired + 1;

        // containers
        internal const int OrderOfClass = OrderOfGraph + 1;
        internal const int OrderOfNamespace = OrderOfClass + 1;
        internal const int OrderOfInterfaces = OrderOfNamespace + 1;
        internal const int OrderOfFields = OrderOfInterfaces + 1;
        internal const int OrderOfTypes = OrderOfFields + 1;

        #endregion

        internal static string EnsureValidName(string name, Type type, ExceptionBuilder error)
        {
            if (name.Length > 0 && name[0] == '_' && (name.Length == 1 || !char.IsDigit(name[1]))) throw error($"Name '{name}' starts with an underscore, which is only allowed if followed by a number.");
            try { NameValidator.ValidateName(name, type.Name); }
            catch (ArgumentOutOfRangeException e) { throw error(e.Message); }
            return name;
        }

        private string? _name;

        [JsonConstructor]
        internal SchemaElement() : base() { }

        internal SchemaElement(Schema? schema, string name) : base(schema) => _name = EnsureValidName(name, GetType(), s => new ArgumentException(s, nameof(name)));

        internal void EnsureSameSchema(ISchemaElement obj)
        {
            if (HasSchema && obj.HasSchema && Schema != obj.Schema)
            {
                throw new InvalidOperationException($"{obj} belongs to a different schema than {this}.");
            }
        }

        /// <summary>
        /// Gets the element's name.
        /// </summary>
        [JsonProperty(Required = Required.Always, Order = OrderOfName)]
        public virtual string Name
        {
            get => _name.RequireProperty();
            private set => _name = EnsureValidName(value, GetType(), JsonError);
        }

        /// <summary>
        /// Returns a string describing the type and name of this element.
        /// </summary>
        public override string ToString() => $"{GetType().Name} {Name}";
    }

    /// <summary>
    /// Base class of all named elements in a GraphQL schema.
    /// </summary>
    public abstract class SchemaTypeElement : SchemaElement, ISchemaTypeElement
    {
        private bool _lockMerge = false;

        [JsonConstructor]
        internal SchemaTypeElement() : base()
        {
            QueryType = CreateQueryType();
            NonNullQueryType = new NonNullGraphType(QueryType);
            ListQueryType = new ListGraphType(NonNullQueryType);
            NonNullListQueryType = new NonNullGraphType(ListQueryType);
        }

        internal SchemaTypeElement(Schema? schema, string name) : base(schema, name)
        {
            QueryType = CreateQueryType();
            NonNullQueryType = new NonNullGraphType(QueryType);
            ListQueryType = new ListGraphType(NonNullQueryType);
            NonNullListQueryType = new NonNullGraphType(ListQueryType);
            QueryType.Name = name;
        }

        /// <summary>
        /// Initializes <see cref="QueryType"/> from JSON.
        /// </summary>
        protected override void JsonInitialize()
        {
            QueryType.Name = Name;
            base.JsonInitialize();
        }

        /// <summary>
        /// If not <c>null</c>, addition of components must be forwarded to the target.
        /// </summary>
        [JsonIgnore]
        internal SchemaTypeElement? MergeTarget { get; private set; }

        /// <summary>
        /// Gets the GraphQL type used for querying.
        /// </summary>
        [JsonIgnore]
        public IGraphType QueryType { get; }

        /// <summary>
        /// Gets the GraphQL type used for querying wrapped in a <see cref="NonNullGraphType"/>.
        /// </summary>
        [JsonIgnore]
        public IGraphType NonNullQueryType { get; }

        /// <summary>
        /// Gets the GraphQL type used for querying wrapped in a <see cref="ListGraphType"/>.
        /// </summary>
        [JsonIgnore]
        public IGraphType ListQueryType { get; }

        /// <summary>
        /// Gets the GraphQL type used for querying wrapped in a <see cref="ListGraphType"/>, which in is wrapped in a <see cref="NonNullGraphType"/>.
        /// </summary>
        [JsonIgnore]
        public IGraphType NonNullListQueryType { get; }

        internal abstract IGraphType CreateQueryType();

        internal void MergeInto(SchemaTypeElement element, ExceptionBuilder error)
        {
            if
            (
                _lockMerge ||
                MergeTarget is not null ||
                element.GetType() != GetType() ||
                element is SchemaType t1 && this is SchemaType t2 && t1.ClassIri != t2.ClassIri ||
                element is SchemaEnum e1 && this is SchemaEnum e2 && e1.DataTypeIri != e2.DataTypeIri
            )
            {
                throw error($"{this} cannot be merged into {element}.");
            }

            MergeTarget = element;
        }

        internal void LockMerge()
        {
            if (MergeTarget is not null) throw new InvalidOperationException($"{this} has already been merged.");
            _lockMerge = true;
        }
    }
}
