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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using UIBK.GraphSPARQL.DataSource;
using UIBK.GraphSPARQL.DataSource.Providers;
using UIBK.GraphSPARQL.Types;
using UIBK.GraphSPARQL.Types.Providers;

namespace UIBK.GraphSPARQL.Configuration
{
    /// <summary>
    /// Base class for all provider elements, with or without serializable settings.
    /// </summary>
    /// <typeparam name="T">The provider's interface.</typeparam>
    public abstract class ProviderConfiguration<T> : JsonElement where T : class
    {
        /// <summary>
        /// Helper class to easily create an <see cref="ImmutableDictionary{String, Type}"/>.
        /// </summary>
        protected class KnownTypesDictionary : IEnumerable<KeyValuePair<string, Type>>
        {
            private readonly ImmutableDictionary<string, Type>.Builder _builder = ImmutableDictionary.CreateBuilder<string, Type>(StringComparer.OrdinalIgnoreCase);

            /// <inheritdoc cref="ImmutableDictionary{String, Type}.Builder.Add(String, Type)"/>
            public void Add(string alias, Type type) => _builder.Add(alias, type);

            /// <inheritdoc/>
            public IEnumerator<KeyValuePair<string, Type>> GetEnumerator() => _builder.GetEnumerator();

            /// <inheritdoc cref="ImmutableDictionary{String, Type}.Builder.ToImmutable()"/>
            public IReadOnlyDictionary<string, Type> ToImmutable() => _builder.ToImmutable();

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() => _builder.GetEnumerator();
        }

        private T? _provider;
        private Type? _providerType;
        private JsonTrace<JObject>? _settings;

        [JsonProperty(PropertyName = "Provider", Required = Required.Always)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private string ProviderName
        {
            set
            {
                // lookup the type and create an instance
                if (!Aliases.TryGetValue(value, out var type)) type = Type.GetType(value) ?? throw JsonError($"Type '{value}' not found.");
                if (!typeof(T).IsAssignableFrom(type)) throw JsonError($"Type '{type}' does not inherit from '{typeof(T)}'.");
                _providerType = type;
            }
        }

        /// <inheritdoc/>
        protected override void JsonInitialize()
        {
            if (_providerType is null) return;
            _provider = (T?)
                (_providerType.IsSubclassOf(typeof(JsonElement))
                    ? _settings is not null ? _settings.Deserialize(_settings.Value, _providerType) : throw JsonError("No settings for configurable provider specified.")
                    : Activator.CreateInstance(_providerType)
                ) ?? throw JsonError($"Cannot create instance of type '{_providerType}'.");
            base.JsonInitialize();
        }

        /// <summary>
        /// Gets a mapping of known or predefined types.
        /// </summary>
        [JsonIgnore]
        protected abstract IReadOnlyDictionary<string, Type> Aliases { get; }

        /// <summary>
        /// Gets or sets the provider-dependent <see cref="ProviderConfiguration{T}"/> configuration data.
        /// </summary>
        [JsonProperty]
        public JObject? Settings
        {
            get => _settings?.Value;
            private set => _settings = JsonTrace(value);
        }

        /// <summary>
        /// Gets the provider instance.
        /// </summary>
        [JsonIgnore]
        public T Provider => _provider.RequireProperty();
    }

    internal sealed class DataSourceProviderConfiguration : ProviderConfiguration<IDataSourceProvider>
    {
        private static readonly IReadOnlyDictionary<string, Type> KnownTypes = new KnownTypesDictionary()
        {
            {"remote", typeof(RemoteSparqlProvider) }
        }.ToImmutable();

        private Uri? _defaultGraphUri;
        private Uri? _defaultNamespaceUri;
        private JsonTrace? _isDefault;
        private JsonTrace<string>? _name;

        protected override IReadOnlyDictionary<string, Type> Aliases => KnownTypes;

        [JsonProperty]
        public bool IsDefault
        {
            get => _isDefault is not null;
            private set => _isDefault = value ? JsonTrace() : null;
        }

        [JsonIgnore]
        internal JsonTrace IsDefaultTrace => _isDefault.RequireProperty();

        [JsonProperty(Required = Required.Always)]
        public string Name
        {
            get => _name.RequireProperty().Value;
            private set => _name = JsonTrace(value);
        }

        [JsonIgnore]
        internal JsonTrace NameTrace => _name.RequireProperty();

        [JsonProperty(PropertyName = "DefaultNamespace")]
        public Uri? DefaultNamespaceUri
        {
            get => _defaultNamespaceUri;
            private set => _defaultNamespaceUri = EnsureAbsoluteUri(value);
        }

        [JsonProperty(PropertyName = "DefaultGraph")]
        public Uri? DefaultGraphUri
        {
            get => _defaultGraphUri;
            private set => _defaultGraphUri = EnsureAbsoluteUri(value);
        }

        [JsonProperty]
        public IDictionary<string, Uri>? Prefixes { get; private set; }
    }

    internal sealed class SchemaProviderConfiguration : ProviderConfiguration<ISchemaProvider>
    {
        private static readonly IReadOnlyDictionary<string, Type> KnownTypes = new KnownTypesDictionary()
        {
            { "rdf", typeof(RdfProvider) },
            { "json", typeof(JsonProvider) },
            { "inline", typeof(InlineProvider) },
            { "graphql", typeof(GraphQLProvider) }
        }.ToImmutable();

        protected override IReadOnlyDictionary<string, Type> Aliases => KnownTypes;
    }
}
