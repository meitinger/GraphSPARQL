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
using UIBK.GraphSPARQL.Configuration;

namespace UIBK.GraphSPARQL.Types.Providers
{
    internal sealed class InlineProvider : JsonElement, ISchemaProvider
    {
        private JsonTrace<JObject>? _object;

        [JsonConstructor]
        private InlineProvider() { }

        [JsonProperty(PropertyName = "Schema", Required = Required.Always)]
        public JObject SchemaDefinition
        {
            get => _object.RequireProperty().Value;
            private set => _object = JsonTrace(value);
        }

        public void FillSchema(Schema schema) => _object.RequireProperty(nameof(SchemaDefinition)).Populate(SchemaDefinition, schema);
    }
}
