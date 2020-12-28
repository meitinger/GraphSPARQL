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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using UIBK.GraphSPARQL.Configuration;

namespace UIBK.GraphSPARQL.DataSource
{
    /// <summary>
    /// Interface for data source provider.
    /// </summary>
    public interface IDataSourceProvider
    {
        /// <summary>
        /// Creates a new query processor.
        /// </summary>
        /// <returns>A new <see cref="VDS.RDF.Query.ISparqlQueryProcessor"/> instance.</returns>
        VDS.RDF.Query.ISparqlQueryProcessor CreateQueryProcessor();

        /// <summary>
        /// Creates a new update processor.
        /// </summary>
        /// <returns>A new <see cref="VDS.RDF.Update.ISparqlUpdateProcessor"/> instance.</returns>
        VDS.RDF.Update.ISparqlUpdateProcessor CreateUpdateProcessor();
    }

    /// <summary>
    /// Class that represents a SPARQL endpoint.
    /// </summary>
    public sealed class SparqlDataSource
    {
        private readonly DataSourceProviderConfiguration _configuration;

        internal SparqlDataSource(DataSourceProviderConfiguration configuration)
        {
            _configuration = configuration;
            QueryProcessor = configuration.Provider.CreateQueryProcessor();
            UpdateProcessor = configuration.Provider.CreateUpdateProcessor();
        }

        internal bool IsDefault => _configuration.IsDefault;

        /// <summary>
        /// Gets the unique name identifying the data source.
        /// </summary>
        public string Name => _configuration.Name;

        /// <summary>
        /// Gets the default namespace for the data source.
        /// </summary>
        public Uri? DefaultNamespaceUri => _configuration.DefaultNamespaceUri;

        /// <summary>
        /// Gets the default graph to be used when querying the data source.
        /// </summary>
        public Uri? DefaultGraphUri => _configuration.DefaultGraphUri;

        /// <summary>
        /// Gets a mapping of prefixes to <see cref="Uri"/>s to be used when querying the <see cref="SparqlDataSource"/>.
        /// </summary>
        public IDictionary<string, Uri> Prefixes => _configuration.Prefixes ?? ImmutableDictionary<string, Uri>.Empty;

        /// <summary>
        /// Gets the <see cref="VDS.RDF.Query.ISparqlQueryProcessor"/>.
        /// </summary>
        public VDS.RDF.Query.ISparqlQueryProcessor QueryProcessor { get; }

        /// <summary>
        /// Gets the <see cref="VDS.RDF.Update.ISparqlUpdateProcessor"/>.
        /// </summary>
        public VDS.RDF.Update.ISparqlUpdateProcessor UpdateProcessor { get; }

        /// <inheritdoc/>
        public override string ToString() => $"{GetType().Name} {Name}";
    }

    /// <summary>
    /// Class that represents a collection of data sources with unique names.
    /// </summary>
    public sealed class SparqlDataSourceCollection : IEnumerable<SparqlDataSource>
    {
        private readonly IDictionary<string, SparqlDataSource> _dataSources = new SortedDictionary<string, SparqlDataSource>(StringComparer.OrdinalIgnoreCase);

        internal void AddRange(IEnumerable<DataSourceProviderConfiguration> dataSources)
        {
            foreach (var dataSource in dataSources)
            {
                if (_dataSources.ContainsKey(dataSource.Name)) throw dataSource.NameTrace.Error($"Another data source with the name '{dataSource.Name}' has already been registered.");
                var sparql = new SparqlDataSource(dataSource);
                if (dataSource.IsDefault)
                {
                    if (Default is not null && Default.IsDefault) throw dataSource.IsDefaultTrace.Error($"{Default} has already been set as default.");
                    Default = sparql;
                }
                _dataSources.Add(dataSource.Name, sparql);
                if (Default is null) Default = sparql;
            }
        }

        internal SparqlDataSource? GetByName(JsonTrace<string>? name) => name is null ? null : GetByName(name.Value, name.Error);

        internal SparqlDataSource GetByName(string name, ExceptionBuilder error) => TryGetByName(name, out var dataSource) ? dataSource : throw error($"Data source '{name}' not found.");

        /// <summary>
        /// Gets the data source that has been configured as default or the first one.
        /// </summary>
        public SparqlDataSource? Default { get; private set; }

        /// <summary>
        /// Finds and returns a data source with a given name.
        /// </summary>
        /// <param name="name">The name of the data source to return.</param>
        /// <param name="dataSource">The output variable receiving the data source.</param>
        /// <returns><c>true</c> if the data source was found, <c>false</c> otherwise.</returns>
        public bool TryGetByName(string name, [NotNullWhen(returnValue: true)] out SparqlDataSource? dataSource) => _dataSources.TryGetValue(name, out dataSource);

        /// <inheritdoc/>
        public IEnumerator<SparqlDataSource> GetEnumerator() => _dataSources.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _dataSources.Values.GetEnumerator();
    }
}
