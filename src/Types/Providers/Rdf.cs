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
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UIBK.GraphSPARQL.Configuration;
using UIBK.GraphSPARQL.DataSource;
using static System.FormattableString;

namespace UIBK.GraphSPARQL.Types.Providers
{
    /// <summary>
    /// Class that parses RDFS and OWL files, also supporting schema.org classes.
    /// </summary>
    public sealed class RdfHarvester
    {
        private class HarvestedType
        {
            private readonly ISet<SchemaInterface> _implementedInterfaces = new HashSet<SchemaInterface>();

            public HarvestedType(RdfType rdfType, SchemaType type, SchemaInterface iface)
            {
                RdfType = rdfType;
                Type = type;
                Interface = iface;
                _implementedInterfaces.Add(iface);
            }

            public RdfType RdfType { get; }
            public SchemaType Type { get; }
            public SchemaInterface Interface { get; }

            private void InheritFromClasses(IEnumerable<RdfType> baseClasses)
            {
                foreach (var baseClass in baseClasses)
                {
                    if (RdfType.Harvester._harvestedTypes.TryGetValue(baseClass.Iri, out var baseType) && !_implementedInterfaces.Contains(baseType.Interface))
                    {
                        _implementedInterfaces.Add(baseType.Interface);
                        if (RdfType.Harvester._unionsToFill.TryGetValue(baseType, out var unions)) unions.ForEach(u => u.AddType(Type, RdfType.HarvestError));
                        InheritFromClasses(baseType.RdfType.BaseClasses);
                    }
                }
            }

            public void HarvestInheritance()
            {
                InheritFromClasses(RdfType.BaseClasses);
                _implementedInterfaces.Append(RdfType.Harvester.GetAnyInterface(RdfType)).ForEach(iface => Type.AddInterface(iface, RdfType.HarvestError));
            }
        }

        private static readonly IReadOnlyDictionary<Iri, SchemaScalar> KnownScalars = new SchemaScalar[]
        {
            SchemaScalar.Boolean("http://schema.org/Boolean"),
            SchemaScalar.Date("http://schema.org/Date"),
            SchemaScalar.DateTime("http://schema.org/DateTime"),
            SchemaScalar.Duration("http://schema.org/Duration"),
            SchemaScalar.Float("http://schema.org/Number"),
            SchemaScalar.Float("http://schema.org/Float"),
            SchemaScalar.Int("http://schema.org/Integer"),
            SchemaScalar.String("http://schema.org/Text"),
            SchemaScalar.Time("http://schema.org/Time"),
            SchemaScalar.Duration("http://www.w3.org/2001/XMLSchema#duration"),
            SchemaScalar.DateTime("http://www.w3.org/2001/XMLSchema#dateTime"),
            SchemaScalar.Time("http://www.w3.org/2001/XMLSchema#time"),
            SchemaScalar.Date("http://www.w3.org/2001/XMLSchema#date"),
            SchemaScalar.Date("http://www.w3.org/2001/XMLSchema#gYearMonth", "yyyy-MMK"),
            SchemaScalar.Date("http://www.w3.org/2001/XMLSchema#gYear",  "yyyyK"),
            SchemaScalar.Date("http://www.w3.org/2001/XMLSchema#gMonth",  "--MM-K"),
            SchemaScalar.Date("http://www.w3.org/2001/XMLSchema#gMonthDay", "--MM-ddK"),
            SchemaScalar.Date("http://www.w3.org/2001/XMLSchema#gDay",  "---ddK"),
            SchemaScalar.String("http://www.w3.org/2001/XMLSchema#string"),
            SchemaScalar.Boolean("http://www.w3.org/2001/XMLSchema#boolean"),
            SchemaScalar.String("http://www.w3.org/2001/XMLSchema#base64Binary"),
            SchemaScalar.String("http://www.w3.org/2001/XMLSchema#hexBinary"),
            SchemaScalar.Float("http://www.w3.org/2001/XMLSchema#float"),
            SchemaScalar.Float("http://www.w3.org/2001/XMLSchema#decimal", "G19"),
            SchemaScalar.Float("http://www.w3.org/2001/XMLSchema#double"),
            SchemaScalar.String("http://www.w3.org/2001/XMLSchema#anyURI"),
            SchemaScalar.String("http://www.w3.org/2001/XMLSchema#QName"),
            SchemaScalar.String("http://www.w3.org/2001/XMLSchema#normalizedString"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#integer"),
            SchemaScalar.String("http://www.w3.org/2001/XMLSchema#token"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#nonPositiveInteger"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#long"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#nonNegativeInteger"),
            SchemaScalar.String("http://www.w3.org/2001/XMLSchema#language"),
            SchemaScalar.String("http://www.w3.org/2001/XMLSchema#Name"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#negativeInteger"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#int"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#unsignedLong"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#positiveInteger"),
            SchemaScalar.String("http://www.w3.org/2001/XMLSchema#NCName"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#short"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#unsignedInt"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#byte"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#unsignedShort"),
            SchemaScalar.Int("http://www.w3.org/2001/XMLSchema#unsignedByte"),
        }.ToImmutableDictionary(scalar => ((ISchemaDataTypedScalar)scalar).DataTypeIri);

        #region URIs

        private static readonly IReadOnlyCollection<Uri> KnownClassIri = ImmutableArray.Create
        (
            new Uri("http://www.w3.org/2000/01/rdf-schema#Class"),
            new Uri("http://www.w3.org/2002/07/owl#Class")
        );
        private static readonly IReadOnlyCollection<Uri> KnownDataTypeIri = ImmutableArray.Create
        (
            new Uri("http://www.w3.org/2000/01/rdf-schema#Datatype"),
            new Uri("http://schema.org/DataType")
        );
        private static readonly IReadOnlyCollection<Uri> KnownDomainIri = ImmutableArray.Create
        (
            new Uri("http://www.w3.org/2000/01/rdf-schema#domain"),
            new Uri("http://schema.org/domainIncludes")
        );
        private static readonly IReadOnlyCollection<Uri> KnownClassPropertyIri = ImmutableArray.Create
        (
            new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#Property"),
            new Uri("http://www.w3.org/2002/07/owl#ObjectProperty"),
            new Uri("http://www.w3.org/2002/07/owl#TransitiveProperty"),
            new Uri("http://www.w3.org/2002/07/owl#SymmetricProperty"),
            new Uri("http://www.w3.org/2002/07/owl#InverseFunctionalProperty")

        );
        private static readonly IReadOnlyCollection<Uri> KnownDataTypePropertyIri = ImmutableArray.Create
        (
             new Uri("http://www.w3.org/2002/07/owl#DatatypeProperty")
        );
        private static readonly IReadOnlyCollection<Uri> KnownRangeIri = ImmutableArray.Create
        (
            new Uri("http://www.w3.org/2000/01/rdf-schema#range"),
            new Uri("http://schema.org/rangeIncludes")
        );
        private static readonly Uri KnownSubClassOf = new Uri("http://www.w3.org/2000/01/rdf-schema#subClassOf");
        private static readonly Uri KnownSubPropertyOf = new Uri("http://www.w3.org/2000/01/rdf-schema#subPropertyOf");
        private static readonly Uri KnownType = new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");

        #endregion

        /// <summary>
        /// Constant name of the interface used for harvesting properties without domains or ranges.
        /// </summary>
        public const string AnyInterfaceName = "IAny";

        private readonly IDictionary<Iri, bool> _isClassDefined = new Dictionary<Iri, bool>();
        private readonly IDictionary<Iri, bool> _isDataType = new Dictionary<Iri, bool>();
        private readonly IDictionary<Iri, bool> _isDataTypeProperty = new Dictionary<Iri, bool>();
        private readonly IDictionary<Iri, bool> _isPropertyDefined = new Dictionary<Iri, bool>();
        private readonly IDictionary<Iri, HarvestedType> _harvestedTypes = new Dictionary<Iri, HarvestedType>();
        private readonly IDictionary<HarvestedType, ISet<SchemaUnion>> _unionsToFill = new Dictionary<HarvestedType, ISet<SchemaUnion>>();

        internal RdfHarvester(Schema schema, RdfProvider settings, ExceptionBuilder error)
        {
            // load the schema and prefetch rdf(s) nodes
            Error = error;
            Location = settings.Location;
            Namespaces = settings.Namespaces.Values;
            Graph = new VDS.RDF.Graph();
            Schema = schema;
            VDS.RDF.Parsing.UriLoader.Load(Graph, settings.Location, settings.Parser);
            ClassNodes = LookupNodes(KnownClassIri);
            DataTypeNodes = LookupNodes(KnownDataTypeIri);
            DomainNodes = LookupNodes(KnownDomainIri);
            PropertyNodes = LookupNodes(KnownClassPropertyIri.Concat(KnownDataTypePropertyIri));
            DataTypePropertyNodes = LookupNodes(KnownDataTypePropertyIri);
            RangeNodes = LookupNodes(KnownRangeIri);
            SubClassOf = Graph.GetUriNode(KnownSubClassOf);
            SubPropertyOf = Graph.GetUriNode(KnownSubPropertyOf);
            Type = Graph.GetUriNode(KnownType) ?? throw Error($"rdf:Type node is missing in.");
        }

        internal ExceptionBuilder Error { get; }

        /// <summary>
        /// Returns the source file's location.
        /// </summary>
        public Uri Location { get; }

        /// <summary>
        /// Gets all defined namespaces.
        /// </summary>
        public IEnumerable<RdfNamespace> Namespaces { get; }

        /// <summary>
        /// Gets the underlying RDF graph.
        /// </summary>
        public VDS.RDF.Graph Graph { get; }

        /// <summary>
        /// Gets the <see cref="Schema"/> that will be filled.
        /// </summary>
        public Schema Schema { get; }

        #region Node Properties

        /// <summary>
        /// Gets the following nodes if present: rdfs:Class, owl:Class
        /// </summary>
        public IEnumerable<VDS.RDF.IUriNode> ClassNodes { get; }

        /// <summary>
        /// Gets the following nodes if present: rdfs:Datatype, schema:DataType
        /// </summary>
        public IEnumerable<VDS.RDF.IUriNode> DataTypeNodes { get; }

        /// <summary>
        /// Gets the following nodes if present: owl:DatatypeProperty
        /// </summary>
        public IEnumerable<VDS.RDF.IUriNode> DataTypePropertyNodes { get; }

        /// <summary>
        /// Gets the following nodes if present: rdfs:domain, schema:domainIncludes
        /// </summary>
        public IEnumerable<VDS.RDF.IUriNode> DomainNodes { get; }

        /// <summary>
        /// Gets the following nodes if present: rdfs:Property, owl:*Property
        /// </summary>
        public IEnumerable<VDS.RDF.IUriNode> PropertyNodes { get; }

        /// <summary>
        /// Gets the following nodes if present: rdfs:range, schema:rangeIncludes
        /// </summary>
        public IEnumerable<VDS.RDF.IUriNode> RangeNodes { get; }

        /// <summary>
        /// Gets the following node if present: rdfs:subClassOf
        /// </summary>
        public VDS.RDF.IUriNode? SubClassOf { get; }

        /// <summary>
        /// Gets the following node if present: rdfs:subPropertyOf
        /// </summary>
        public VDS.RDF.IUriNode? SubPropertyOf { get; }

        /// <summary>
        /// Gets the following node: rdf:type
        /// </summary>
        public VDS.RDF.IUriNode Type { get; }

        #endregion

        /// <summary>
        /// Enumerates all <see cref="RdfType"/> in a namespace.
        /// </summary>
        public IEnumerable<RdfType> Types => GetNodesOf(ClassNodes.Concat(DataTypeNodes), (node, ns) => new RdfType(this, node, ns));

        /// <summary>
        /// Enumerates all <see cref="RdfProperty"/> that are harvested in a namespace.
        /// </summary>
        public IEnumerable<RdfProperty> Properties => GetNodesOf(PropertyNodes, (node, ns) => new RdfProperty(this, node, ns));

        #region Node and RdfElement Methods

        internal bool IsAnyOf(VDS.RDF.IUriNode node, IEnumerable<VDS.RDF.IUriNode> types) => types.Any(n => Graph.ContainsTriple(new VDS.RDF.Triple(node, Type, n)));

        internal bool IsClassDefined(RdfType type) => IsA(_isClassDefined, type.Iri, () => IsAnyOf(type.Node, ClassNodes));

        internal bool IsDataType(RdfType type) => IsA(_isDataType, type.Iri, () => IsAnyOf(type.Node, DataTypeNodes) || type.BaseClasses.Any(b => IsDataType(b)));

        internal bool IsDataType(RdfProperty property) => IsA(_isDataTypeProperty, property.Iri, () => IsAnyOf(property.Node, DataTypePropertyNodes) || property.BaseProperties.Any(b => IsDataType(b)));

        internal bool IsPropertyDefined(RdfProperty property) => IsA(_isPropertyDefined, property.Iri, () => IsAnyOf(property.Node, PropertyNodes));

        private bool IsA(IDictionary<Iri, bool> lookup, Iri iri, Func<bool> getter)
        {
            if (lookup.TryGetValue(iri, out var value)) return value;
            value = getter();
            lookup.Add(iri, value);
            return value;
        }

        private IEnumerable<T> GetNodesOf<T>(IEnumerable<VDS.RDF.INode> types, Func<VDS.RDF.IUriNode, RdfNamespace, T> creator) => types
            .SelectMany(node => Graph.GetTriplesWithPredicateObject(Type, node))
            .Select(triple => triple.Subject)
            .OfType<VDS.RDF.IUriNode>()
            .Distinct()
            .SelectMany(node => Namespaces
                .Where(ns => ns.Uri.IsBaseOf(node.Uri))
                .Select(ns => creator(node, ns)));

        private IReadOnlyCollection<VDS.RDF.IUriNode> LookupNodes(IEnumerable<Uri> iris) => iris
            .Select(Graph.GetUriNode)
            .Where(node => node is not null)
            .ToImmutableArray();

        #endregion

        #region Harvesting Methods

        private bool CanHarvestProperty(RdfProperty property)
        {
            // needs to in sync with GetPropertyDomain
            if (!property.Domains.Any())
            {
                if (property.Namespace.ExcludePropertiesWithoutDomain) return false;
            }
            else
            {
                if (!property.Domains.Any(t => !t.IsDataType && (t.AllowDeclaration || Schema.Types.Any(t2 => t2.ClassIri == t.Iri)))) return false;
            }

            // needs to in sync with GetPropertyRange
            if (!property.Ranges.Any())
            {
                if (property.Namespace.ExcludePropertiesWithoutRange) return false;
            }
            else
            {
                if (!property.IsDataType && !property.Ranges.Any(t => t.IsDataType || t.AllowDeclaration || Schema.Types.Any(t2 => t2.ClassIri == t.Iri))) return false;
            }
            return true;
        }

        private SchemaInterface GetAnyInterface(RdfElement rdfElement) => Schema.AddInternal(new SchemaInterface(Schema, AnyInterfaceName), rdfElement.HarvestError);

        private HarvestedType GetOrCreateInterfaceAndType(RdfType rdfType)
        {
            if (_harvestedTypes.TryGetValue(rdfType.Iri, out var existing)) return existing;
            var type = Schema.AddInternal(new SchemaType(Schema, rdfType.NameForTypes, rdfType.Iri), rdfType.HarvestError);
            var iface = Schema.AddInternal(new SchemaInterface(Schema, rdfType.NameForInterfaces), rdfType.HarvestError);
            if (rdfType.Namespace.RegisterQuery) RegisterQueryOrMutation(rdfType, rdfType.NameForFields, SchemaField.RootFieldIri, Schema.Query, rdfType.Namespace.UseInterfaceIfPossible ? iface : type);
            if (rdfType.Namespace.RegisterUpdate) RegisterQueryOrMutation(rdfType, "update" + rdfType.NameForTypes, SchemaField.RootFieldIri, Schema.Mutation, rdfType.Namespace.UseInterfaceIfPossible ? iface : type);
            if (rdfType.Namespace.RegisterCreate) RegisterQueryOrMutation(rdfType, "create" + rdfType.NameForTypes, SchemaField.CreateFieldIri, Schema.Mutation, type);
            var harvestedType = new HarvestedType(rdfType, type, iface);
            _harvestedTypes.Add(rdfType.Iri, harvestedType);
            return harvestedType;
        }

        private IEnumerable<SchemaContainer> GetPropertyDomain(RdfProperty property)
        {
            // needs to be in sync with CanHarvestProperty
            if (!property.Domains.Any())
            {
                yield return GetAnyInterface(property);
            }
            else
            {
                foreach (var rdfType in property.Domains.Where(t => !t.IsDataType))
                {
                    if (rdfType.AllowDeclaration) yield return GetOrCreateInterfaceAndType(rdfType).Interface;
                    else foreach (var type in Schema.Types.Where(t => t.ClassIri == rdfType.Iri)) yield return type;
                }
            }
        }

        private SchemaField GetPropertyRange(RdfProperty property)
        {
            // needs to be in sync with CanHarvestProperty
            if (!property.Ranges.Any())
            {
                // if there is no range we allow all harvested types or scalars to be returned
                if (property.IsDataType) return property.CreateField(Schema.AddInternal(new SchemaCustomScalar(Schema, property.NameForRanges), property.HarvestError));
                else return property.CreateField(GetAnyInterface(property));
            }
            else
            {
                if (property.IsDataType || property.Ranges.Any(t => t.IsDataType))
                {
                    // create a scalar
                    var rangeScalars = new Dictionary<Iri, SchemaScalar?>();
                    foreach (var rdfDataType in property.Ranges)
                    {
                        if (KnownScalars.TryGetValue(rdfDataType.Iri, out var scalar)) rangeScalars[rdfDataType.Iri] = scalar;
                        else if (rdfDataType.Iri == SchemaScalar.LangStringDataTypeIri) rangeScalars[SchemaScalar.LangStringDataTypeIri] = SchemaScalar.LanguageString();
                        else if (!rdfDataType.IsDataType || rdfDataType.Iri == SchemaScalar.IriDataTypeIri) rangeScalars[SchemaScalar.IriDataTypeIri] = SchemaScalar.Id();
                        else if (rdfDataType.AllowDeclaration) rangeScalars.Add(rdfDataType.Iri, Schema.AddInternal(new SchemaCustomScalar(Schema, rdfDataType.NameForTypes), rdfDataType.HarvestError));
                        else rangeScalars.Add(rdfDataType.Iri, null);
                    }
                    var rangeScalar = rangeScalars.Values.Count() == 1 ? rangeScalars.Values.Single() : null;
                    if (rangeScalar is null)
                    {
                        var anyScalar = Schema.AddInternal(new SchemaCustomScalar(Schema, property.NameForRanges), property.HarvestError);
                        rangeScalars.Keys.ForEach(iri => anyScalar.AddDataType(iri, property.HarvestError));
                        rangeScalar = anyScalar;
                    }
                    return property.CreateField(rangeScalar);
                }
                else
                {
                    // create a union or type
                    var rangeTypes = new HashSet<SchemaType>();
                    var typesToPropagate = new HashSet<HarvestedType>();
                    foreach (var rdfType in property.Ranges)
                    {
                        if (rdfType.AllowDeclaration)
                        {
                            var def = GetOrCreateInterfaceAndType(rdfType);
                            rangeTypes.Add(def.Type);
                            typesToPropagate.Add(def);
                        }
                        rangeTypes.UnionWith(Schema.Types.Where(t => t.ClassIri == rdfType.Iri));
                    }
                    if (rangeTypes.Count > 1)
                    {
                        // add all found types and make sure all other derived types are added at the end as well
                        var rangeUnion = Schema.AddInternal(new SchemaUnion(Schema, property.NameForRanges), property.HarvestError);
                        rangeTypes.ForEach(t => rangeUnion.AddType(t, property.HarvestError));
                        foreach (var def in typesToPropagate)
                        {
                            if (_unionsToFill.TryGetValue(def, out var unions)) unions.Add(rangeUnion);
                            else _unionsToFill.Add(def, new HashSet<SchemaUnion>() { rangeUnion });
                        }
                        return property.CreateField(rangeUnion);
                    }
                    else if (typesToPropagate.Any())
                    {
                        // return either the interface or the type of an harvested type
                        var def = typesToPropagate.Single();
                        return property.CreateField(def.RdfType.Namespace.UseInterfaceIfPossible ? def.Interface : def.Type);
                    }
                    else
                    {
                        return property.CreateField(rangeTypes.Single());
                    }
                }
            }
        }

        private void RegisterQueryOrMutation(RdfType rdfType, string name, Iri predicate, SchemaContainer container, ISchemaObject obj) => container.AddField(new SchemaField(Schema, name, predicate, rdfType.Namespace.DataSource, rdfType.Namespace.GraphUri, true, false, null, obj), rdfType.HarvestError);

        /// <summary>
        /// Harvest the RDF schema.
        /// </summary>
        public void Harvest()
        {
            // harvest all properties first, pulling in references classes
            foreach (var property in Properties.Where(p => p.AllowDeclaration && CanHarvestProperty(p)))
            {
                var field = GetPropertyRange(property);
                GetPropertyDomain(property).ForEach(d => d.AddField(field, property.HarvestError));
            }

            // ensure all other classes and datatypes are harvested, if requested
            Types
                .Where(c => c.AllowDeclaration && !(c.IsDataType ? c.Namespace.ExcludeUnusedDataTypes : c.Namespace.ExcludeUnusedClasses))
                .ForEach(c =>
                {
                    if (c.IsDataType) Schema.AddInternal(new SchemaCustomScalar(Schema, c.NameForTypes), c.HarvestError);
                    else GetOrCreateInterfaceAndType(c);
                });

            // handle inheritance
            _harvestedTypes.Values.ForEach(def => def.HarvestInheritance());
        }

        #endregion
    }

    /// <summary>
    /// Base class for RDF types and attributes.
    /// </summary>
    public abstract class RdfElement
    {
        internal static T GetCached<T>(ref T? cache, Func<T> generator) where T : class
        {
            if (cache is null) cache = generator();
            return cache;
        }

        internal static T GetCached<T>(ref T? cache, Func<T> generator) where T : struct
        {
            if (!cache.HasValue) cache = generator();
            return cache.Value;
        }

        private readonly RdfNamespace? _namespace;
        private string? _interfaceNameCache;
        private string? _fieldNameCache;
        private string? _normalizedNameCache;
        private string? _typeNameCache;

        internal RdfElement(RdfHarvester harvester, VDS.RDF.IUriNode node, RdfNamespace? ns)
        {
            _namespace = ns ?? harvester.Namespaces.SingleOrDefault(ns => ns.Uri.IsBaseOf(node.Uri));
            Harvester = harvester;
            Node = node;
            Iri = node.Uri;
        }

        internal SchemaField CreateField(ISchemaObject obj) => new SchemaField(Harvester.Schema, NameForFields, Iri, Namespace.DataSource, Namespace.GraphUri, true, false, null, obj);

        internal SchemaField CreateField(SchemaScalar scalar) => new SchemaField(Harvester.Schema, NameForFields, Iri, Namespace.DataSource, Namespace.GraphUri, true, false, null, scalar);

        internal IEnumerable<T> GetBase<T>(ref IEnumerable<T>? cache, VDS.RDF.IUriNode? subOf, Func<VDS.RDF.IUriNode, T> creator) => GetCached(ref cache, () =>
            subOf is null
                ? Enumerable.Empty<T>()
                : Harvester.Graph
                    .GetTriplesWithSubjectPredicate(Node, subOf)
                    .Select(triple => triple.Object)
                    .OfType<VDS.RDF.IUriNode>()
                    .Select(node => creator(node))
                    .ToImmutableArray());

        /// <summary>
        /// Indicates whether this element can be harvested.
        /// </summary>
        public virtual bool AllowDeclaration => _namespace is not null;

        /// <summary>
        /// Returns the harvester associated with this element.
        /// </summary>
        public RdfHarvester Harvester { get; }

        /// <summary>
        /// Returns the node's IRI.
        /// </summary>
        public Iri Iri { get; }

        /// <summary>
        /// Indicates whether this element is a datatype.
        /// </summary>
        public abstract bool IsDataType { get; }

        /// <summary>
        /// Gets the element's name if used in schema fields.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the element has no matching namespace.</exception>
        public string NameForFields => GetCached(ref _fieldNameCache, () => char.ToLower(NormalizedName[0]) + NormalizedName[1..]);

        /// <summary>
        /// Gets the element's name if used in schema interfaces.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the element has no matching namespace.</exception>
        public string NameForInterfaces => GetCached(ref _interfaceNameCache, () => "I" + NameForTypes);

        /// <summary>
        /// Gets the element's name if used in schema types, unions or named scalars.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the element has no matching namespace.</exception>
        public string NameForTypes => GetCached(ref _typeNameCache, () => (Namespace.Prefix ?? string.Empty) + NormalizedName);

        /// <summary>
        /// Returns the namespace the element belongs to.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the element has no matching namespace.</exception>
        public RdfNamespace Namespace => _namespace ?? throw new InvalidOperationException($"RDF element '{Iri}' in '{Harvester.Location}'has no assigned namespace.");

        /// <summary>
        /// Get the underlying graph node.
        /// </summary>
        public VDS.RDF.IUriNode Node { get; }

        private string NormalizedName => GetCached(ref _normalizedNameCache, () =>
        {
            // convert the relative uri to pascal case
            var builder = new StringBuilder();
            var capitalizeNext = true;
            foreach (var ch in Namespace.Uri.MakeRelativeUri(Node.Uri).ToString().Normalize(NormalizationForm.FormKD))
            {
                if (char.IsWhiteSpace(ch) || char.IsSeparator(ch) || char.IsPunctuation(ch))
                {
                    capitalizeNext = true;
                    continue;
                }
                else if (char.IsDigit(ch) || char.IsUpper(ch))
                {
                    builder.Append(ch);
                }
                else if (char.IsLower(ch))
                {
                    builder.Append(capitalizeNext ? char.ToUpper(ch) : ch);
                }
                else
                {
                    continue;
                }

                capitalizeNext = false;
            }
            if (builder.Length == 0) throw HarvestError("Name cannot be normalized.");
            if (char.IsDigit(builder[0])) builder.Insert(0, '_');
            return builder.ToString();
        });

        /// <summary>
        /// Constructs a new harvest error exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>A new error object instance.</returns>
        public Exception HarvestError(string message) => Harvester.Error($"Error while harvesting {GetType().Name} '{Iri}': {message}");
    }

    /// <summary>
    /// Class that describes a rdfs:Class, owl:Class, rdfs:Datatype or schema:DataType.
    /// </summary>
    public sealed class RdfType : RdfElement
    {
        private IEnumerable<RdfType>? _baseClassesCache;

        internal RdfType(RdfHarvester harvester, VDS.RDF.IUriNode node, RdfNamespace? ns) : base(harvester, node, ns) { }

        /// <inheritdoc/>
        public override bool AllowDeclaration => base.AllowDeclaration && (IsDataType ? Namespace.IncludeDataTypes : (Namespace.IncludeClasses && Harvester.IsClassDefined(this)));

        /// <summary>
        /// Enumerates all base classes as <see cref="RdfType"/>.
        /// </summary>
        public IEnumerable<RdfType> BaseClasses => GetBase(ref _baseClassesCache, Harvester.SubClassOf, node => new RdfType(Harvester, node, null));

        /// <inheritdoc/>
        public override bool IsDataType => Harvester.IsDataType(this);
    }

    /// <summary>
    /// Class that describes a rdfs:Property or owl:*Property.
    /// </summary>
    public sealed class RdfProperty : RdfElement
    {
        private IEnumerable<RdfProperty>? _basePropertiesCache;
        private IEnumerable<RdfType>? _domainsCache;
        private IEnumerable<RdfType>? _rangesCache;
        private string? _rangesNameCache;

        internal RdfProperty(RdfHarvester harvester, VDS.RDF.IUriNode node, RdfNamespace? ns) : base(harvester, node, ns) { }

        private IEnumerable<RdfType> GetTypesWith(ref IEnumerable<RdfType>? cache, IEnumerable<VDS.RDF.IUriNode> predicates) => GetCached(ref cache, () =>
            predicates
                .SelectMany(predicate => Harvester.Graph.GetTriplesWithSubjectPredicate(Node, predicate))
                .Select(triple => triple.Object)
                .OfType<VDS.RDF.IUriNode>()
                .Select(node => new RdfType(Harvester, node, null))
                .ToImmutableArray());

        /// <inheritdoc/>
        public override bool AllowDeclaration => base.AllowDeclaration && Namespace.IncludeProperties && (IsDataType || Harvester.IsPropertyDefined(this));

        /// <summary>
        /// Enumerates all base properties as <see cref="RdfProperty"/>.
        /// </summary>
        public IEnumerable<RdfProperty> BaseProperties => GetBase(ref _basePropertiesCache, Harvester.SubPropertyOf, node => new RdfProperty(Harvester, node, null));

        /// <summary>
        /// Returns all <see cref="RdfType"/> that are a subject of the property.
        /// </summary>
        public IEnumerable<RdfType> Domains => GetTypesWith(ref _domainsCache, Harvester.DomainNodes);

        /// <inheritdoc/>
        public override bool IsDataType => Harvester.IsDataType(this);

        /// <summary>
        /// Gets the properties's name if used in ranges unions.
        /// </summary>
        public string NameForRanges => GetCached(ref _rangesNameCache, () => NameForTypes + "Any");

        /// <summary>
        /// Returns all <see cref="RdfType"/> that are an object of the property.
        /// </summary>
        public IEnumerable<RdfType> Ranges => GetTypesWith(ref _rangesCache, Harvester.RangeNodes);
    }

    /// <summary>
    /// Class containing per-namespace settings for the RDF harvester.
    /// </summary>
    public sealed class RdfNamespace : JsonElement
    {
        private JsonTrace<string>? _dataSourceName;
        private Uri? _graphUri;
        private Uri? _uri;

        /// <summary>
        /// Gets the prefix that should be prepended to all named schema elements.
        /// </summary>
        [JsonProperty]
        public string? Prefix { get; private set; }

        /// <summary>
        /// Gets the namespace's URI.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Uri Uri
        {
            get => _uri.RequireProperty();
            private set => _uri = EnsureAbsoluteUri(value);
        }

        /// <summary>
        /// Gets the data source or <c>null</c> if the default one should be used.
        /// </summary>
        [JsonIgnore]
        public SparqlDataSource? DataSource => Schema.DataSources.GetByName(_dataSourceName);

        [JsonProperty(PropertyName = "DataSource")]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private string? DataSourceName
        {
            set => _dataSourceName = JsonTrace(value);
        }

        /// <summary>
        /// Gets the default graph URI.
        /// </summary>
        [JsonProperty(PropertyName = "Graph")]
        public Uri? GraphUri
        {
            get => _graphUri;
            private set => _graphUri = EnsureAbsoluteUri(value);
        }

        /// <summary>
        /// Indicates whether all harvested classes should be queryable.
        /// </summary>
        [JsonProperty]
        [DefaultValue(true)]
        public bool RegisterQuery { get; private set; }

        /// <summary>
        /// Indicates whether all harvested classes should be createable.
        /// </summary>
        [JsonProperty]
        [DefaultValue(true)]
        public bool RegisterCreate { get; private set; }

        /// <summary>
        /// Indicates whether all harvested classes should be updateable.
        /// </summary>
        [JsonProperty]
        [DefaultValue(true)]
        public bool RegisterUpdate { get; private set; }

        /// <summary>
        /// Indicates whether <see cref="SchemaInterface"/> should be used rather than <see cref="SchemaType"/> if possible.
        /// </summary>
        [JsonProperty]
        [DefaultValue(false)]
        public bool UseInterfaceIfPossible { get; private set; }

        /// <summary>
        /// Indicates whether classes should be harvested.
        /// If <c>false</c> and <see cref="IncludeProperties"/> is <c>true</c>, only properties
        /// referencing previously defined classes are harvested.
        /// </summary>
        [JsonProperty]
        [DefaultValue(true)]
        public bool IncludeClasses { get; private set; }

        /// <summary>
        /// Indicates whether a <see cref="RdfType"/> that represents a class that has not
        /// been referenced by any harvested <see cref="RdfProperty"/> should be ignored.
        /// </summary>
        [JsonProperty]
        [DefaultValue(false)]
        public bool ExcludeUnusedClasses { get; private set; }

        /// <summary>
        /// Indicates whether data types should be harvested.
        /// If <c>false</c> and <see cref="IncludeProperties"/> is <c>true</c>, only properties
        /// referencing previously defined or known data types are harvested.
        /// </summary>
        [JsonProperty]
        [DefaultValue(true)]
        public bool IncludeDataTypes { get; private set; }

        /// <summary>
        /// Indicates whether a <see cref="RdfType"/> that represents a data type that has not
        /// been referenced by any harvested <see cref="RdfProperty"/> should be ignored.
        /// </summary>
        [JsonProperty]
        [DefaultValue(true)]
        public bool ExcludeUnusedDataTypes { get; private set; }

        /// <summary>
        /// Indicates whether <see cref="RdfProperty"/> should be harvested.
        /// </summary>
        [JsonProperty]
        [DefaultValue(true)]
        public bool IncludeProperties { get; private set; }

        /// <summary>
        /// Indicates whether a <see cref="RdfProperty"/> with an empty domain should be ignored.
        /// If <c>false</c>, the <see cref="RdfProperty"/> gets harvested under the <c>IAny</c>
        /// interface that every other <see cref="SchemaType"/> inherits from.
        /// </summary>
        [JsonProperty]
        [DefaultValue(false)]
        public bool ExcludePropertiesWithoutDomain { get; private set; }

        /// <summary>
        /// Indicates whether a <see cref="RdfProperty"/> with an empty range should be ignored.
        /// If <c>false</c>, the <see cref="RdfProperty"/> gets harvested as returning either the
        /// <c>IAny</c> interface or a custom scalar with an <c>any</c> data type IRI.
        /// </summary>
        [JsonProperty]
        [DefaultValue(true)]
        public bool ExcludePropertiesWithoutRange { get; private set; }
    }

    internal sealed class RdfProvider : JsonElement, ISchemaProvider
    {
        private Uri? _location;
        private IReadOnlyDictionary<Uri, RdfNamespace>? _namespaces;

        [JsonConstructor]
        private RdfProvider() { }

        [JsonProperty(Required = Required.Always)]
        public Uri Location
        {
            get => _location.RequireProperty();
            private set => _location = EnsureAbsolutePath(value);
        }

        [JsonIgnore]
        public VDS.RDF.IRdfReader? Parser { get; private set; }

        [JsonProperty(PropertyName = "Parser")]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private string? ParserName
        {
            // if a specific parser was selected, find it in VDS.RDF.Parsing and create an instance
            set => Parser = value is null ? null : Activator.CreateInstance(typeof(VDS.RDF.IRdfReader).Assembly.GetType(Invariant($"VDS.RDF.Parsing.{value}Parser")) ?? throw JsonError($"Parser '{value}' not found.")) as VDS.RDF.IRdfReader ?? throw JsonError($"Parser '{value}' could not be created.");
        }

        [JsonIgnore]
        public IReadOnlyDictionary<Uri, RdfNamespace> Namespaces => _namespaces.RequireProperty();

        [JsonProperty(PropertyName = "Namespaces", Required = Required.Always)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private IEnumerable<RdfNamespace> NamespacesList
        {
            set
            {
                // ensure no namespaces overlap
                var builder = ImmutableDictionary.CreateBuilder<Uri, RdfNamespace>();
                foreach (var ns in value)
                {
                    if (builder.ContainsKey(ns.Uri) || builder.Keys.Any(uri => uri.IsBaseOf(ns.Uri) || ns.Uri.IsBaseOf(uri)))
                    {
                        throw JsonError($"Namespace '{ns.Uri}' is specified multiple times or overlappes with other namespace(s).");
                    }
                    builder.Add(ns.Uri, ns);
                }
                _namespaces = builder.ToImmutable();
            }
        }

        /// <inheritdoc/>
        public void FillSchema(Schema schema) => new RdfHarvester(schema, this, JsonError).Harvest();
    }
}
