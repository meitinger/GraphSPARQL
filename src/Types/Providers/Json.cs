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
using System.IO;
using UIBK.GraphSPARQL.Configuration;

namespace UIBK.GraphSPARQL.Types.Providers
{
    internal sealed class JsonProvider : JsonElement, ISchemaProvider
    {
        private string? _path;

        [JsonConstructor]
        private JsonProvider() { }

        [JsonProperty(Required = Required.Always)]
        public string Path
        {
            get => _path.RequireProperty();
            private set => _path = EnsureAbsolutePath(value);
        }

        public void FillSchema(Schema schema)
        {
            using var stream = new StreamReader(Path);
            using var reader = new JsonTextReader(stream);
            new JsonContext(reader, Path, schema).Populate(schema);
        }
    }
}
