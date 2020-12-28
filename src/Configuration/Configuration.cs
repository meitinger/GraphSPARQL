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
using System;
using System.Collections.Generic;
using System.Net;

namespace UIBK.GraphSPARQL.Configuration
{
    /// <summary>
    /// Configuration element for credentials.
    /// </summary>
    public sealed class CredentialsConfiguration : JsonElement, ICredentials, ICredentialsByHost
    {
        private string? _userName;
        private string? _password;

        /// <inheritdoc/>
        protected override void JsonInitialize()
        {
            if (UseDefault)
            {
                if (_userName is not null || _password is not null || Domain is not null) throw JsonError("Neither username, password nor domain must be set for default credentials.");
            }
            else
            {
                if (_userName is null || _password is null) throw JsonError("Username and password are required.");
            }
            base.JsonInitialize();
        }

        NetworkCredential ICredentials.GetCredential(Uri uri, string authType) => this;

        NetworkCredential ICredentialsByHost.GetCredential(string host, int port, string authenticationType) => this;

        /// <summary>
        /// Indicates whether the user's default credentials should be used.
        /// </summary>
        [JsonProperty]
        public bool UseDefault { get; private set; }

        /// <summary>
        /// Gets the user name.
        /// </summary>
        /// <exception cref="InvalidOperationException">If <see cref="UseDefault"/> is <c>true</c>.</exception>
        [JsonProperty]
        public string UserName
        {
            get => _userName.RequireProperty();
            private set => _userName = value;
        }

        /// <summary>
        /// Get the password.
        /// </summary>
        /// <exception cref="InvalidOperationException">If <see cref="UseDefault"/> is <c>true</c>.</exception>
        [JsonProperty]
        public string Password
        {
            get => _password.RequireProperty();
            private set => _password = value;
        }

        /// <summary>
        /// Gets the domain or <c>null</c> if the default domain should be used or <see cref="UseDefault"/> is <c>true</c>.
        /// </summary>
        [JsonProperty]
        public string? Domain { get; private set; }

        /// <summary>
        /// Converts the configuration element into <see cref="NetworkCredential"/>.
        /// </summary>
        /// <param name="credentials">The <see cref="CredentialsConfiguration"/> to convert.</param>
        public static implicit operator NetworkCredential(CredentialsConfiguration credentials) =>
            credentials.UseDefault
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(credentials.UserName, credentials.Password, credentials.Domain ?? string.Empty);
    }

    /// <summary>
    /// Configuration element for proxy servers.
    /// </summary>
    public sealed class ProxyConfiguration : JsonElement
    {
        private Uri? _address;

        /// <summary>
        /// Gets the proxy server address.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Uri Address
        {
            get => _address.RequireProperty();
            private set => _address = EnsureAbsoluteUri(value);
        }

        /// <summary>
        /// Gets the credentials to be used when accessing the proxy or <c>null</c> if no login is required.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public CredentialsConfiguration? Credentials { get; private set; }
    }

    internal sealed class SchemaConfiguration : JsonElement
    {
        private IEnumerable<DataSourceProviderConfiguration>? _dataSources;
        private IEnumerable<SchemaProviderConfiguration>? _definitions;

        [JsonProperty(Required = Required.Always)]
        public IEnumerable<DataSourceProviderConfiguration> DataSources
        {
            get => _dataSources.RequireProperty();
            set => _dataSources = value;
        }

        [JsonProperty(Required = Required.Always)]
        public IEnumerable<SchemaProviderConfiguration> Definitions
        {
            get => _definitions.RequireProperty();
            set => _definitions = value;
        }
    }
}
