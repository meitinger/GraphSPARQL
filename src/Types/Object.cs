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

using System.Collections.Generic;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Primary;

namespace UIBK.GraphSPARQL.Types
{
    internal interface ISchemaObject : ISchemaTypeElement
    {
        string Name { get; }
        SchemaObjectDefinition Definition { get; }
    }

    internal sealed class SchemaObjectDefinition
    {
        private static readonly ISparqlExpression ParentTerm = new VariableTerm(Constants.ParentFilterVariableName);

        private readonly ISchemaObject _parent;
        private readonly IDictionary<Iri, SchemaType> _classesToTypes = new Dictionary<Iri, SchemaType>();

        internal SchemaObjectDefinition(ISchemaObject parent) => _parent = parent;

        public ISparqlExpression? TypeFilter { get; private set; }

        public IEnumerable<SchemaType> Types => _classesToTypes.Values;

        internal void EnsureCanAddType(SchemaType type, ExceptionBuilder error)
        {
            if (_classesToTypes.TryGetValue(type.ClassIri, out var existingType)) throw error($"{type} cannot be added to {_parent} because {existingType} has already been added.");
        }

        public void AddType(SchemaType type)
        {
            _classesToTypes.Add(type.ClassIri, type);
            var filter = new EqualsExpression(ParentTerm, type.ClassIri.Term);
            TypeFilter = TypeFilter is null ? filter : new OrExpression(TypeFilter, filter);
        }
    }
}
