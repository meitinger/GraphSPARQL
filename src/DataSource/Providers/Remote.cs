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
using System.Net;
using UIBK.GraphSPARQL.Configuration;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace UIBK.GraphSPARQL.DataSource.Providers
{
    internal sealed class RemoteSparqlProvider : JsonElement, IDataSourceProvider
    {
        private Uri? _endointUri;
        private Uri? _updateEndpointUri;

        [JsonProperty(Required = Required.Always)]
        public Uri EndpointUri
        {
            get => _endointUri.RequireProperty();
            private set => _endointUri = EnsureAbsoluteUri(value);
        }

        [JsonProperty]
        public Uri? UpdateEndpointUri
        {
            get => _updateEndpointUri;
            private set => _updateEndpointUri = EnsureAbsoluteUri(value);
        }

        [JsonProperty]
        public CredentialsConfiguration? Credentials { get; private set; }

        [JsonProperty]
        public ProxyConfiguration? Proxy { get; private set; }

        [JsonProperty]
        public int? Timeout { get; private set; }

        private T AdjustEndpoint<T>(T endpoint) where T : BaseEndpoint
        {
            if (Timeout.HasValue) endpoint.Timeout = Timeout.Value;
            if (Credentials is not null) endpoint.Credentials = Credentials;
            if (Proxy is not null)
            {
                endpoint.Proxy = new WebProxy(Proxy.Address);
                if (Proxy.Credentials is not null) endpoint.ProxyCredentials = Proxy.Credentials;
            }
            return endpoint;
        }

        public ISparqlQueryProcessor CreateQueryProcessor() => new RemoteQueryProcessor(AdjustEndpoint(new SparqlRemoteEndpoint(EndpointUri)));

        public ISparqlUpdateProcessor CreateUpdateProcessor() => new RemoteUpdateProcessor(AdjustEndpoint(new SparqlRemoteUpdateEndpoint(UpdateEndpointUri ?? EndpointUri)));
    }
}
