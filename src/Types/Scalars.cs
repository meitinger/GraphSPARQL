﻿/*
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

using GraphQL;
using GraphQL.Language.AST;
using GraphQL.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace UIBK.GraphSPARQL.Types
{
    internal interface ISchemaDataTypedScalar
    {
        Iri DefaultDataTypeIri { get; }
        Iri DataTypeIri { get; }
        SchemaScalar Clone(Iri? dataTypeIri);
    }

    internal interface ISchemaFormattableScalar
    {
        string DefaultFormat { get; }
        string Format { get; }
        SchemaScalar Clone(string? format);
    }

    /// <summary>
    /// Base class for all enums, built-in and custom scalars.
    /// </summary>
    public abstract class SchemaScalar : SchemaTypeElement
    {
        #region Cache

        private static readonly ConcurrentDictionary<Tuple<Type, Iri?, string?>, SchemaScalar> _cache = new ConcurrentDictionary<Tuple<Type, Iri?, string?>, SchemaScalar>();

        internal static bool TryGetBuiltin(string name, Iri? dataType, ExceptionBuilder dataTypeError, string? format, ExceptionBuilder formatError, [NotNullWhen(true)] out SchemaScalar? scalar)
        {
            if (!Schema.BuiltinScalars.TryGetValue(name, out scalar)) return false;
            if (dataType is null && format is null) return true; // already in base format
            scalar = _cache.GetOrAdd(Tuple.Create(scalar.GetType(), dataType, format), (_, scalar) =>
            {
                if (dataType is not null) scalar = scalar is ISchemaDataTypedScalar dataTyped ? dataTyped.Clone(dataType) : throw dataTypeError($"{scalar} does not support custom data types.");
                if (format is not null) scalar = scalar is ISchemaFormattableScalar formattable ? formattable.Clone(format) : throw formatError($"{scalar} does not support custom formatting.");
                return scalar;
            }, scalar);
            return true;
        }

        private static SchemaScalar Singleton(Type type, Iri? dataTypeIri, string? format, Func<Iri?, string?, SchemaScalar> creator) => _cache.GetOrAdd(Tuple.Create(type, dataTypeIri, format), tuple => creator(tuple.Item2, tuple.Item3));

        private static SchemaScalar Singleton<T>() where T : SchemaScalar, new() => Singleton(typeof(T), null, null, (_, _) => new T());

        private static SchemaScalar Singleton<T>(Iri? dataTypeIri) where T : SchemaScalar, ISchemaDataTypedScalar, new() => Singleton(typeof(T), dataTypeIri, null, (iri, _) => new T().Clone(iri));

        private static SchemaScalar Singleton<T>(Iri? dataTypeIri, string? format) where T : SchemaScalar, ISchemaDataTypedScalar, ISchemaFormattableScalar, new() => Singleton(typeof(T), dataTypeIri, format, (iri, fmt) => ((T)new T().Clone(iri)).Clone(fmt));

        private static SchemaScalar Singleton<T>(string? format) where T : SchemaScalar, ISchemaFormattableScalar, new() => Singleton(typeof(T), null, format, (_, fmt) => new T().Clone(fmt));

        #endregion

        /// <summary>
        /// Gets an <see cref="Iri"/> that indicates if no underlying data type should be specified.
        /// </summary>
        public static Iri PlainLiteralIri { get; } = new Iri("http://www.w3.org/1999/02/22-rdf-syntax-ns#PlainLiteral");

        /// <summary>
        /// Gets an <see cref="Iri"/> that indicates if a custom scalar can get and set language strings. 
        /// </summary>
        public static Iri LangStringIri { get; } = new Iri("http://www.w3.org/1999/02/22-rdf-syntax-ns#langString");

        /// <summary>
        /// Gets an <see cref="Iri"/> that indicates if a custom scalar can get or set resource IRIs.
        /// </summary>
        public static Iri ClassIri { get; } = new Iri("http://www.w3.org/2000/01/rdf-schema#Class");

        /// <summary>
        /// Defines a new boolean scalar.
        /// </summary>
        /// <param name="dataTypeIri">The underlying data type IRI.</param>
        /// <param name="format"><c>"c"</c> to format the boolean as <c>"yes"</c>/<c>"no"</c>, <c>"b"</c> to format the boolean as <c>"true"</c>/<c>"false"</c> or <c>"n"</c> to format the boolean as <c>"1"</c>/<c>"0"</c>.</param>
        /// <returns>A <see cref="SchemaScalar"/> describing a boolean.</returns>
        public static SchemaScalar Boolean(Iri? dataTypeIri = null, string? format = null) => Singleton<SchemaBooleanScalar>(dataTypeIri, format);

        /// <summary>
        /// Defines a new string scalar.
        /// </summary>
        /// <param name="dataTypeIri">The underlying data type IRI.</param>
        /// <returns>A <see cref="SchemaScalar"/> describing a string.</returns>
        public static SchemaScalar String(Iri? dataTypeIri = null) => Singleton<SchemaStringScalar>(dataTypeIri);

        /// <summary>
        /// Defines a new int scalar.
        /// </summary>
        /// <param name="dataTypeIri">The underlying data type IRI.</param>
        /// <param name="format">The formatting string to use when serializing the int, see <see cref="int.ToString(string?)"/>.</param>
        /// <returns>A <see cref="SchemaScalar"/> describing an int.</returns>
        public static SchemaScalar Int(Iri? dataTypeIri = null, string? format = null) => Singleton<SchemaIntScalar>(dataTypeIri, format);

        /// <summary>
        /// Defines a new float scalar.
        /// </summary>
        /// <param name="dataTypeIri">The underlying data type IRI.</param>
        /// <param name="format">The formatting string to use when serializing the float, see <see cref="float.ToString(string?)"/>.</param>
        /// <returns>A <see cref="SchemaScalar"/> describing a float.</returns>
        public static SchemaScalar Float(Iri? dataTypeIri = null, string? format = null) => Singleton<SchemaFloatScalar>(dataTypeIri, format);

        /// <summary>
        /// Defines a new ID, which is a resource IRI.
        /// </summary>
        /// <returns>A <see cref="SchemaScalar"/> describing an ID.</returns>
        public static SchemaScalar Id() => Singleton<SchemaIdScalar>();

        /// <summary>
        /// Defines a new duration scalar.
        /// </summary>
        /// <param name="dataTypeIri">The underlying data type IRI.</param>
        /// <returns>A <see cref="SchemaScalar"/> describing a duration.</returns>
        public static SchemaScalar Duration(Iri? dataTypeIri = null) => Singleton<SchemaDurationScalar>(dataTypeIri);

        /// <summary>
        /// Defines a new date-only scalar.
        /// </summary>
        /// <param name="dataTypeIri">The underlying data type IRI.</param>
        /// <param name="format">The formatting string to use when serializing the date, see <see cref="System.DateTime.ToString(string?)"/>.</param>
        /// <returns>A <see cref="SchemaScalar"/> describing date.</returns>
        public static SchemaScalar Date(Iri? dataTypeIri = null, string? format = null) => Singleton<SchemaDateScalar>(dataTypeIri, format);

        /// <summary>
        /// Defines a new time-only scalar.
        /// </summary>
        /// <param name="dataTypeIri">The underlying data type IRI.</param>
        /// <param name="format">The formatting string to use on serializing the time, see <see cref="System.DateTime.ToString(string?)"/>.</param>
        /// <returns>A <see cref="SchemaScalar"/> describing time.</returns>
        public static SchemaScalar Time(Iri? dataTypeIri = null, string? format = null) => Singleton<SchemaTimeScalar>(dataTypeIri, format);

        /// <summary>
        /// Defines a new date and time scalar.
        /// </summary>
        /// <param name="dataTypeIri">The underlying data type IRI.</param>
        /// <param name="format">The formatting string to use on serializing the date and time, see <see cref="System.DateTime.ToString(string?)"/>.</param>
        /// <returns>A <see cref="SchemaScalar"/> describing date and time.</returns>
        public static SchemaScalar DateTime(Iri? dataTypeIri = null, string? format = null) => Singleton<SchemaDateTimeScalar>(dataTypeIri, format);

        /// <summary>
        /// Defines a new language string scalar.
        /// </summary>
        /// <param name="format">The default locale or <c>null</c> to use the thread's current culture.</param>
        /// <returns>A <see cref="SchemaScalar"/> describing a language string.</returns>
        public static SchemaScalar LanguageString(string? format = null) => Singleton<SchemaLanguageStringScalar>(format);

        [JsonConstructor]
        internal SchemaScalar() : base() { }

        internal SchemaScalar(Schema? schema, string name) : base(schema, name) { }

        internal abstract object? FromSparQL(VDS.RDF.INode value);

        internal abstract VDS.RDF.INode? ToSparQL(object value, VDS.RDF.INodeFactory factory);
    }

    /// <summary>
    /// Base class for all non-enum scalars.
    /// </summary>
    public abstract class SchemaTypedScalar : SchemaScalar
    {
        private sealed class GraphQLScalar : ScalarGraphType
        {
            private readonly SchemaTypedScalar _scalar;

            internal GraphQLScalar(SchemaTypedScalar scalar) => _scalar = scalar;

            public override object? ParseLiteral(IValue value) => _scalar.FromGraphQL(value);

            public override object? ParseValue(object? value) => ValueConverter.ConvertTo(value, _scalar.GraphQLType);
        }

        internal interface IRepresenation
        {
            void FromString(string s);
        }

        internal abstract class TypedRepresentation<T> : IRepresenation where T : class, IRepresenation, new()
        {
            internal sealed class ValueNode : ValueNode<T>
            {
                internal void SetValue(T value) => Value = value;
                protected override bool Equals(ValueNode<T> node) => Value.Equals(node.Value);
            }

            internal sealed class ScalarValueConverter : IAstFromValueConverter
            {
                public IValue Convert(object value, IGraphType type)
                {
                    var result = new ValueNode();
                    result.SetValue((T)value);
                    return result;
                }

                public bool Matches(object value, IGraphType type) => value is T;
            }

            internal static readonly Regex LangStringFormat = new Regex("^'(?<value>.*)'@(?<language>[a-z]+(-[a-z0-9]+)*)$", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
            internal static readonly Regex UriFormat = new Regex("^<(?<uri>.*)>$", RegexOptions.ExplicitCapture);
            internal static readonly Regex DataTypeFormat = new Regex("^'(?<value>.*)'^^<(?<datatype>.*)>$", RegexOptions.ExplicitCapture);

            private static readonly PropertyInfo[] _properties = typeof(T).GetProperties();

            static TypedRepresentation() => ValueConverter.Register<string, T>(Parse);

            public static T Parse(string s)
            {
                var t = new T();
                t.FromString(s);
                return t;
            }

            public static T? FromValue(IValue value)
            {
                if (value is StringValue s) return Parse(s.Value);
                if (value is not ObjectValue obj) return null;
                var result = new T();
                foreach (var prop in _properties)
                {
                    var field = obj.Field(prop.Name) ?? obj.Field(char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..]);
                    if (field is not null) prop.SetValue(result, ValueConverter.ConvertTo(field.Value.Value, prop.PropertyType));
                }
                return result;
            }

            protected abstract void FromString(string s);
            void IRepresenation.FromString(string s) => FromString(s);
        }

        [JsonConstructor]
        internal SchemaTypedScalar() : base() { }

        internal SchemaTypedScalar(Schema? schema, string name) : base(schema, name) { }

        [JsonIgnore]
        internal abstract Type GraphQLType { get; }

        internal override IGraphType CreateQueryType() => new GraphQLScalar(this);

        internal abstract object? FromGraphQL(IValue value);
    }

    internal abstract class SchemaInternalScalar<T, U> : SchemaTypedScalar, ISchemaDataTypedScalar where U : SchemaInternalScalar<T, U>, new()
    {
        private Iri? _dataTypeIri;

        internal SchemaInternalScalar(string name) : base(null, name) { }

        internal sealed override Type GraphQLType => typeof(T);

        [JsonIgnore]
        public abstract Iri DefaultDataTypeIri { get; }

        [JsonIgnore]
        public Iri DataTypeIri => _dataTypeIri ?? DefaultDataTypeIri;

        internal sealed override object? FromSparQL(VDS.RDF.INode value) => value is VDS.RDF.ILiteralNode node && (node.DataType is null || node.DataType == DataTypeIri) ? (object?)FromSparQL(node.Value) : null;

        internal abstract T FromSparQL(string value);

        internal sealed override VDS.RDF.INode? ToSparQL(object value, VDS.RDF.INodeFactory factory) => value is T t ? DataTypeIri == PlainLiteralIri ? factory.CreateLiteralNode(ToSparQL(t)) : factory.CreateLiteralNode(ToSparQL(t), DataTypeIri.Uri) : null;

        internal abstract string ToSparQL(T value);

        protected virtual U Clone() => new U() { _dataTypeIri = _dataTypeIri };

        public SchemaScalar Clone(Iri? dataTypeIri)
        {
            var clone = Clone();
            clone._dataTypeIri = dataTypeIri;
            return clone;
        }
    }

    /// <summary>
    /// Class describing a non-built-in scalar.
    /// </summary>
    public sealed class SchemaCustomScalar : SchemaTypedScalar
    {
        internal sealed class Representation : TypedRepresentation<Representation>
        {
            public Uri? Uri { get; set; }
            public string? Value { get; set; }
            public Uri? DataType { get; set; }
            public string? Language { get; set; }

            protected override void FromString(string s)
            {
                Match match;
                if ((match = LangStringFormat.Match(s)).Success)
                {
                    Value = match.Groups["value"].Value;
                    Language = match.Groups["language"].Value;
                }
                else if ((match = DataTypeFormat.Match(s)).Success)
                {
                    Value = match.Groups["value"].Value;
                    DataType = ValueConverter.ConvertTo<Uri>(match.Groups["datatype"].Value);
                }
                else if ((match = UriFormat.Match(s)).Success)
                {
                    Uri = ValueConverter.ConvertTo<Uri>(match.Groups["uri"].Value);
                }
                else
                {
                    Value = s;
                }
            }
        }

        private readonly ISet<Iri> _dataTypeIris = new SortedSet<Iri>(Comparer<Iri>.Create((iri1, iri2) => string.CompareOrdinal(iri1.ToString(), iri2.ToString())));

        [JsonConstructor]
        private SchemaCustomScalar() : base() { }

        internal SchemaCustomScalar(Schema schema, string name) : base(schema, name) { }

        internal override Type GraphQLType => typeof(Representation);

        /// <summary>
        /// Enumerates all sub scalars.
        /// </summary>
        [JsonProperty(PropertyName = "DataTypes", Order = OrderOfDataTypes)]
        public IEnumerable<Iri> DataTypeIris
        {
            get => _dataTypeIris;
            private set
            {
                var trace = JsonTrace();
                value.ForEach(dataType => AddDataType(dataType, trace.Error));
            }
        }

        internal override object? FromSparQL(VDS.RDF.INode value)
        {
            if (value is VDS.RDF.IUriNode u)
            {
                if (_dataTypeIris.Contains(ClassIri)) return new Representation() { Uri = u.Uri };
            }
            else if (value is VDS.RDF.ILiteralNode l)
            {
                if (l.DataType is not null)
                {
                    if (_dataTypeIris.Contains(l.DataType)) return new Representation() { Value = l.Value, DataType = l.DataType };
                }
                else if (!string.IsNullOrEmpty(l.Language))
                {
                    if (_dataTypeIris.Contains(LangStringIri)) return new Representation() { Value = l.Value, Language = l.Language };
                }
                else
                {
                    if (_dataTypeIris.Contains(PlainLiteralIri)) return new Representation() { Value = l.Value };
                }
            }
            return null;
        }

        internal override VDS.RDF.INode? ToSparQL(object value, VDS.RDF.INodeFactory factory)
        {
            if (value is not Representation r) return null;
            if (r.Uri is not null)
            {
                if (r.Value is not null || r.Language is not null) throw new FormatException($"{nameof(r.Uri)} cannot be set together with {nameof(r.Value)} or {nameof(r.Language)}.");
                if (_dataTypeIris.Contains(ClassIri)) return factory.CreateUriNode(r.Uri);
            }
            else if (r.DataType is not null)
            {
                if (!string.IsNullOrEmpty(r.Language)) throw new FormatException($"{nameof(r.DataType)} cannot be set together with {nameof(r.Language)}.");
                if (_dataTypeIris.Contains(r.DataType)) return factory.CreateLiteralNode(r.Value, r.DataType);
            }
            else if (!string.IsNullOrEmpty(r.Language))
            {
                if (_dataTypeIris.Contains(LangStringIri)) return factory.CreateLiteralNode(r.Value, r.Language);
            }
            else
            {
                if (_dataTypeIris.Contains(PlainLiteralIri)) return factory.CreateLiteralNode(r.Value);
            }
            return null;
        }

        internal override object? FromGraphQL(IValue value) => Representation.FromValue(value);

        private SchemaCustomScalar AddDataTypeImmediately(Iri dataTypeIri, ExceptionBuilder error)
        {
            if (MergeTarget is not null) return ((SchemaCustomScalar)MergeTarget).AddDataTypeImmediately(dataTypeIri, error);
            _dataTypeIris.Add(dataTypeIri);
            return this;
        }

        /// <summary>
        /// Adds another allowed data type <see cref="Iri"/> to the list of possible values.
        /// </summary>
        /// <param name="dataTypeIri">The data type's <see cref="Iri"/> to add.</param>
        /// <returns>The current <see cref="SchemaCustomScalar"/>.</returns>
        public SchemaCustomScalar AddDataType(Iri dataTypeIri) => AddDataTypeImmediately(dataTypeIri, s => new ArgumentException(s));

        internal void AddDataType(Iri dataTypeIri, ExceptionBuilder error) => Schema.Resolve += () => AddDataTypeImmediately(dataTypeIri, error);
    }

    internal abstract class SchemaFormattableInternalScalar<T, U> : SchemaInternalScalar<T, U>, ISchemaFormattableScalar where U : SchemaFormattableInternalScalar<T, U>, new()
    {
        private string? _format;

        internal SchemaFormattableInternalScalar(string name) : base(name) { }

        [JsonIgnore]
        public abstract string DefaultFormat { get; }

        public string Format => _format ?? DefaultFormat;

        protected override U Clone()
        {
            var clone = base.Clone();
            clone._format = _format;
            return clone;
        }

        public SchemaScalar Clone(string? format)
        {
            var clone = Clone();
            clone._format = format;
            return clone;
        }
    }

    #region Built-in Scalars

    internal sealed class SchemaStringScalar : SchemaInternalScalar<string, SchemaStringScalar>
    {
        private static readonly Iri XmlSchemaIri = "http://www.w3.org/2001/XMLSchema#string";

        public SchemaStringScalar() : base("String") { }

        public override Iri DefaultDataTypeIri => XmlSchemaIri;

        internal override IGraphType CreateQueryType() => new StringGraphType();

        internal override string FromSparQL(string value) => value;

        internal override string ToSparQL(string value) => value;

        internal override object? FromGraphQL(IValue value) => throw new NotImplementedException();
    }

    internal sealed class SchemaFloatScalar : SchemaFormattableInternalScalar<double, SchemaFloatScalar>
    {
        private static readonly Iri XmlSchemaIri = "http://www.w3.org/2001/XMLSchema#float";

        public SchemaFloatScalar() : base("Float") { }

        public override Iri DefaultDataTypeIri => XmlSchemaIri;

        public override string DefaultFormat => "f";

        internal override IGraphType CreateQueryType() => new FloatGraphType();

        internal override double FromSparQL(string value) => XmlConvert.ToDouble(value);

        internal override string ToSparQL(double value) => value.ToString(Format, NumberFormatInfo.InvariantInfo);

        internal override object? FromGraphQL(IValue value) => throw new NotImplementedException();
    }

    internal sealed class SchemaIntScalar : SchemaFormattableInternalScalar<int, SchemaIntScalar>
    {
        private static readonly Iri XmlSchemaIri = "http://www.w3.org/2001/XMLSchema#int";

        public SchemaIntScalar() : base("Int") { }

        public override Iri DefaultDataTypeIri => XmlSchemaIri;

        public override string DefaultFormat => "d";

        internal override IGraphType CreateQueryType() => new IntGraphType();

        internal override int FromSparQL(string value) => XmlConvert.ToInt32(value);

        internal override string ToSparQL(int value) => value.ToString(Format, NumberFormatInfo.InvariantInfo);

        internal override object? FromGraphQL(IValue value) => throw new NotImplementedException();
    }

    internal sealed class SchemaBooleanScalar : SchemaFormattableInternalScalar<bool, SchemaBooleanScalar>
    {
        private static readonly Iri XmlSchemaIri = "http://www.w3.org/2001/XMLSchema#boolean";
        private static readonly IDictionary<string, Tuple<string, string>> Formats = new Dictionary<string, Tuple<string, string>>()
        {
            { "b", Tuple.Create("true","false") },
            { "n", Tuple.Create("1","0") },
        };

        public SchemaBooleanScalar() : base("Boolean") { }

        public override Iri DefaultDataTypeIri => XmlSchemaIri;

        public override string DefaultFormat => "b";

        internal override IGraphType CreateQueryType() => new BooleanGraphType();

        internal override bool FromSparQL(string value) => XmlConvert.ToBoolean(value);

        internal override string ToSparQL(bool value) => Formats.TryGetValue(Format, out var format)
            ? (value ? format.Item1 : format.Item2)
            : throw new FormatException($"Format string can only be one of {string.Join(", ", Formats.Keys.Select(key => $"'{key}'"))}.");

        internal override object? FromGraphQL(IValue value) => throw new NotImplementedException();
    }

    internal sealed class SchemaIdScalar : SchemaTypedScalar
    {
        public SchemaIdScalar() : base(null, "ID") { }

        internal override Type GraphQLType => typeof(Uri);

        internal override IGraphType CreateQueryType() => new IdGraphType();

        internal override object? FromSparQL(VDS.RDF.INode value) => value is VDS.RDF.IUriNode node ? node.Uri : null;

        internal override VDS.RDF.INode? ToSparQL(object value, VDS.RDF.INodeFactory factory) => value is Uri uri ? factory.CreateUriNode(uri) : null;

        internal override object? FromGraphQL(IValue value) => throw new NotImplementedException();
    }

    #endregion

    #region Temporal Scalars

    internal abstract class SchemaBaseDateTimeScalar<T, U> : SchemaFormattableInternalScalar<T, U> where T : SchemaBaseDateTimeScalar<T, U>.DateTimeRepresentation, SchemaTypedScalar.IRepresenation, new() where U : SchemaBaseDateTimeScalar<T, U>, new()
    {
        internal abstract class DateTimeRepresentation : TypedRepresentation<T>
        {
            internal abstract DateTime DateTime { get; set; }

            protected sealed override void FromString(string s) => DateTime = XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.RoundtripKind);
        }

        internal SchemaBaseDateTimeScalar(string name) : base(name) { }

        internal sealed override T FromSparQL(string value) => new T() { DateTime = System.DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind) };

        internal sealed override string ToSparQL(T value) => value.DateTime.ToString(Format, CultureInfo.InvariantCulture);
    }

    internal sealed class SchemaDateTimeScalar : SchemaBaseDateTimeScalar<SchemaDateTimeScalar.Representation, SchemaDateTimeScalar>
    {
        internal sealed class Representation : DateTimeRepresentation
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int Day { get; set; }
            public int Hour { get; set; }
            public int Minute { get; set; }
            public int Second { get; set; }
            public int Millisecond { get; set; }
            public DateTimeKind Kind { get; set; }

            internal override DateTime DateTime
            {
                get => new DateTime(Year, Month, Day, Hour, Minute, Second, Millisecond, Kind);
                set
                {
                    Year = value.Year;
                    Month = value.Month;
                    Day = value.Day;
                    Hour = value.Year;
                    Minute = value.Minute;
                    Second = value.Second;
                    Millisecond = value.Millisecond;
                    Kind = value.Kind;
                }
            }
        }

        private static readonly Iri XmlSchemaIri = "http://www.w3.org/2001/XMLSchema#dateTime";

        public SchemaDateTimeScalar() : base("DateTime") { }

        public override Iri DefaultDataTypeIri => XmlSchemaIri;

        public override string DefaultFormat => "yyyy-MM-ddThh:mm:ssK";

        internal override object? FromGraphQL(IValue value) => Representation.FromValue(value);
    }

    internal sealed class SchemaDateScalar : SchemaBaseDateTimeScalar<SchemaDateScalar.Representation, SchemaDateScalar>
    {
        internal sealed class Representation : DateTimeRepresentation
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int Day { get; set; }
            public DateTimeKind Kind { get; set; }

            internal override DateTime DateTime
            {
                get => new DateTime(Year, Month, Day, 0, 0, 0, Kind);
                set
                {
                    Year = value.Year;
                    Month = value.Month;
                    Day = value.Day;
                    Kind = value.Kind;
                }
            }
        }

        private static readonly Iri XmlSchemaIri = "http://www.w3.org/2001/XMLSchema#date";

        public SchemaDateScalar() : base("Date") { }

        public override Iri DefaultDataTypeIri => XmlSchemaIri;

        public override string DefaultFormat => "yyyy-MM-dd";

        internal override object? FromGraphQL(IValue value) => Representation.FromValue(value);
    }

    internal sealed class SchemaTimeScalar : SchemaBaseDateTimeScalar<SchemaTimeScalar.Representation, SchemaTimeScalar>
    {
        internal sealed class Representation : DateTimeRepresentation
        {
            public int Hour { get; set; }
            public int Minute { get; set; }
            public int Second { get; set; }
            public int Millisecond { get; set; }
            public DateTimeKind Kind { get; set; }

            internal override DateTime DateTime
            {
                get => new DateTime(1, 1, 1, Hour, Minute, Second, Millisecond, Kind);
                set
                {
                    Hour = value.Year;
                    Minute = value.Minute;
                    Second = value.Second;
                    Millisecond = value.Millisecond;
                    Kind = value.Kind;
                }
            }
        }

        private static readonly Iri XmlSchemaIri = "http://www.w3.org/2001/XMLSchema#time";

        public SchemaTimeScalar() : base("Time") { }

        public override Iri DefaultDataTypeIri => XmlSchemaIri;

        public override string DefaultFormat => "hh:mm:ssK";

        internal override object? FromGraphQL(IValue value) => Representation.FromValue(value);
    }

    internal sealed class SchemaDurationScalar : SchemaInternalScalar<SchemaDurationScalar.Representation, SchemaDurationScalar>
    {
        internal sealed class Representation : TypedRepresentation<Representation>
        {
            public int Days { get; set; }
            public int Hours { get; set; }
            public int Minutes { get; set; }
            public int Seconds { get; set; }
            public int Milliseconds { get; set; }

            internal TimeSpan TimeSpan
            {
                get => new TimeSpan(Days, Hours, Minutes, Seconds, Milliseconds);
                set
                {
                    Days = value.Days;
                    Hours = value.Hours;
                    Minutes = value.Minutes;
                    Seconds = value.Seconds;
                    Milliseconds = value.Milliseconds;
                }
            }

            protected override void FromString(string s) => TimeSpan = XmlConvert.ToTimeSpan(s);
        }

        private static readonly Iri XmlSchemaIri = "http://www.w3.org/2001/XMLSchema#duration";

        public SchemaDurationScalar() : base("Duration") { }

        public override Iri DefaultDataTypeIri => XmlSchemaIri;

        internal override Representation FromSparQL(string value) => new Representation() { TimeSpan = XmlConvert.ToTimeSpan(value) };

        internal override string ToSparQL(Representation value) => XmlConvert.ToString(value.TimeSpan);

        internal override object? FromGraphQL(IValue value) => Representation.FromValue(value);
    }

    #endregion

    internal sealed class SchemaLanguageStringScalar : SchemaTypedScalar, ISchemaFormattableScalar
    {
        internal sealed class Representation : TypedRepresentation<Representation>
        {
            public string? Value { get; set; }
            public string? Language { get; set; }

            protected override void FromString(string s)
            {
                var match = LangStringFormat.Match(s);
                if (match.Success)
                {
                    Value = match.Groups["value"].Value;
                    Language = match.Groups["language"].Value;
                }
                else
                {
                    Value = s;
                }
            }
        }

        private string? _format;

        public SchemaLanguageStringScalar() : base(null, "LanguageString") { }

        internal override Type GraphQLType => typeof(Representation);

        public string DefaultFormat => Thread.CurrentThread.CurrentCulture.Name;

        public string Format => _format ?? DefaultFormat;

        internal override object? FromSparQL(VDS.RDF.INode value) => value is VDS.RDF.ILiteralNode node && node.DataType is null ? new Representation() { Value = node.Value, Language = node.Language } : null;

        internal override VDS.RDF.INode? ToSparQL(object value, VDS.RDF.INodeFactory factory) => value is Representation r ? factory.CreateLiteralNode(r.Value ?? string.Empty, r.Language ?? Format) : null;

        internal override object? FromGraphQL(IValue value) => Representation.FromValue(value);

        public SchemaScalar Clone(string? format) => new SchemaLanguageStringScalar { _format = format };
    }

    /// <summary>
    /// Class that describes an enumeration.
    /// </summary>
    public sealed class SchemaEnum : SchemaScalar
    {
        private sealed class GraphQLScalar : EnumerationGraphType
        {
            private readonly SchemaEnum _enum;

            internal GraphQLScalar(SchemaEnum @enum) => _enum = @enum;

            public override object? ParseLiteral(IValue value) => value is EnumValue e && _enum._nameToValue.TryGetValue(e.Name, out var s) ? s : null;

            public override object? ParseValue(object? value)
            {
                var s = value?.ToString();
                return s is not null && _enum._nameToValue.TryGetValue(s, out var v) ? v : null;
            }

            public override object? Serialize(object? value)
            {
                var s = value?.ToString();
                return s is null || _enum._nameToValue.ContainsKey(s) ? s : _enum._valueToName.TryGetValue(s, out var name) ? name : null;
            }
        }

        private Iri? _dataTypeIri;
        private readonly IDictionary<string, string> _nameToValue = new SortedDictionary<string, string>(StringComparer.Ordinal);
        private readonly IDictionary<string, string> _valueToName = new Dictionary<string, string>();

        [JsonConstructor]
        private SchemaEnum() : base() { }

        internal SchemaEnum(Schema schema, string name, Iri dataTypeIri) : base(schema, name) => _dataTypeIri = dataTypeIri;

        /// <summary>
        /// Gets the underlying data type.
        /// </summary>
        [JsonProperty(PropertyName = "DataType", Required = Required.Always, Order = OrderOfDataType)]
        public Iri DataTypeIri
        {
            get => _dataTypeIri.RequireProperty();
            set => _dataTypeIri = value;
        }

        /// <summary>
        /// Gets the name of all values.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<string> ValueNames => _nameToValue.Keys;

        /// <summary>
        /// Gets all values.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<string> Values => _nameToValue.Values;

        [JsonProperty(PropertyName = "Values", Required = Required.Always, Order = OrderOfValues)]
        [SuppressMessage("CodeQuality", "IDE0051")]
        private IDictionary<string, string> ValuesMap
        {
            get => _nameToValue;
            set
            {
                var trace = JsonTrace();
                value.ForEach(entry => AddValue(entry.Key, entry.Value, trace.Error));
            }
        }

        internal override IGraphType CreateQueryType() => new GraphQLScalar(this);

        internal override object? FromSparQL(VDS.RDF.INode value)
        {
            if (value is VDS.RDF.IUriNode u)
            {
                if (DataTypeIri == ClassIri) return u.Uri.ToString();
            }
            else if (value is VDS.RDF.ILiteralNode n && string.IsNullOrEmpty(n.Language))
            {
                if (n.DataType is null || DataTypeIri == n.DataType) return n.Value;
            }
            return null;
        }

        internal override VDS.RDF.INode? ToSparQL(object value, VDS.RDF.INodeFactory factory)
        {
            if (value is not string s) return null;
            if (DataTypeIri == PlainLiteralIri) return factory.CreateLiteralNode(s);
            else if (DataTypeIri == ClassIri) return factory.CreateUriNode(new Uri(s, UriKind.RelativeOrAbsolute));
            else return factory.CreateLiteralNode(s, DataTypeIri.Uri);
        }

        private SchemaEnum AddValueImmediately(string name, string value, ExceptionBuilder error)
        {
            if (MergeTarget is not null) return ((SchemaEnum)MergeTarget).AddValueImmediately(name, value, error);
            if (_nameToValue.TryGetValue(name, out var existingValue))
            {
                if (existingValue == value) return this;
                throw error($"{this} already contains value '{existingValue}' under name '{name}'.");
            }
            if (_valueToName.TryGetValue(value, out var existingName))
            {
                if (existingName == name) return this;
                throw error($"{this} already has name '{existingName}' mapped to value '{value}'.");
            }
            _nameToValue.Add(name, value);
            _valueToName.Add(value, name);
            return this;
        }

        /// <summary>
        /// Registers a value for a given name.
        /// </summary>
        /// <param name="name">The value's name. This is what GraphQL uses.</param>
        /// <param name="value">The value send to SPARQL.</param>
        /// <returns>The current <see cref="SchemaEnum"/>.</returns>
        /// <exception cref="ArgumentException">If another entry with the given <paramref name="name"/> or <paramref name="value"/> has already been defined.</exception>
        public SchemaEnum AddValue(string name, string value) => AddValueImmediately(name, value, s => new ArgumentException(s));

        internal void AddValue(string name, string value, ExceptionBuilder error) => Schema.Resolve += () => AddValueImmediately(name, value, error);

        /// <summary>
        /// Tries to find an entry by value and return its name.
        /// </summary>
        /// <param name="value">The entry's value.</param>
        /// <param name="name">The found entry's name.</param>
        /// <returns><c>true</c> if an with the given <paramref name="value"/> has been found, <c>falce</c> otherwise.</returns>
        public bool TryGetValueName(string value, [NotNullWhen(true)] out string? name) => _valueToName.TryGetValue(value, out name);

        /// <summary>
        /// Tries to find an entry by name and return its value.
        /// </summary>
        /// <param name="name">The entry's name.</param>
        /// <param name="value">The found entry's value.</param>
        /// <returns><c>true</c> if an with the given <paramref name="name"/> has been found, <c>falce</c> otherwise.</returns>
        public bool TryGetValue(string name, [NotNullWhen(true)] out string? value) => _nameToValue.TryGetValue(name, out value);

        /// <inheritdoc/>
        public override string ToString() => $"{base.ToString()}(DataType={DataTypeIri})";
    }
}