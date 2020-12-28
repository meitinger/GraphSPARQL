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
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UIBK.GraphSPARQL.Types;

namespace UIBK.GraphSPARQL.Configuration
{
    internal sealed class JsonContext
    {
        private sealed class CustomContractResolver : DefaultContractResolver
        {
            public static readonly CustomContractResolver Instance = new CustomContractResolver();

            private CustomContractResolver() { }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (!property.IsRequiredSpecified) property.Required = Required.DisallowNull;
                if
                (
                    member is PropertyInfo propertyInfo &&
                    (property.DefaultValueHandling & DefaultValueHandling.Ignore) != 0 &&
                    (property.Required == Required.Default || property.Required == Required.DisallowNull) &&
                    typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
                )
                {
                    property.ShouldSerialize = instance => ((IEnumerable?)propertyInfo.GetValue(instance, null))?.GetEnumerator().MoveNext() ?? property.Required != Required.DisallowNull;
                }
                return property;
            }
        }

        private static JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings()
        {
            ContractResolver = CustomContractResolver.Instance,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        };

        internal static JsonSerializer Writer() => JsonSerializer.CreateDefault(SerializerSettings);

        private readonly JsonReader _reader;

        internal JsonContext(JsonReader reader, string fileName, Schema schema)
        {
            _reader = reader;
            FileName = fileName;
            Schema = schema;
        }

        public string FileName { get; }
        public Schema Schema { get; }
        public string Path => _reader.Path;
        public int LineNumber => (_reader as IJsonLineInfo)?.LineNumber ?? 0;
        public int LinePosition => (_reader as IJsonLineInfo)?.LinePosition ?? 0;

        public object? Deserialize(Type type) => Reader().Deserialize(_reader, type);

        public T? Deserialize<T>() => Reader().Deserialize<T>(_reader);

        public void Populate(object target) => Reader().Populate(_reader, target);

        internal JsonSerializer Reader()
        {
            var result = JsonSerializer.CreateDefault(SerializerSettings);
            result.Context = this;
            return result;
        }

        public static implicit operator StreamingContext(JsonContext jsonContext) => new StreamingContext(StreamingContextStates.Persistence, jsonContext);

        public static implicit operator JsonContext(StreamingContext streamingContext) => streamingContext.Context as JsonContext ?? throw new InvalidCastException($"{nameof(StreamingContext)} does not contain a {nameof(JsonContext)}.");
    }

    /// <summary>
    /// Class capturing the JSON context.
    /// </summary>
    public class JsonTrace
    {
        private readonly JsonContext _context;

        internal JsonTrace(JsonContext context)
        {
            _context = context;
            FileName = context.FileName;
            Path = context.Path;
            LineNumber = context.LineNumber;
            LinePosition = context.LinePosition;
        }

        /// <summary>
        /// The path to the current JSON file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The current JSON object path at time of capturing.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The current line number in a JSON file at time of capturing.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// The current line position in a JSON file at time of capturing.
        /// </summary>
        public int LinePosition { get; }

        /// <summary>
        /// Deserializes a JSON token using the captured context.
        /// </summary>
        /// <param name="token">The JSON token to deserialize.</param>
        /// <param name="type">The type that should be returned.</param>
        /// <returns>The deserialized object.</returns>
        public object? Deserialize(JToken token, Type type)
        {
            // TODO: maybe rethrow errors with line number and position
            using var reader = new JTokenReader(token, Path);
            return new JsonContext(reader, _context.FileName, _context.Schema).Deserialize(type);
        }

        /// <summary>
        /// Returns a new exception describing the captured context.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        /// <returns>A new exception object.</returns>
        public JsonSerializationException Error(string message) => new JsonSerializationException(message, Path, LineNumber, LinePosition, null);

        /// <summary>
        /// Populates an object with the JSON token using the captured context.
        /// </summary>
        /// <param name="token">The JSON token to deserialize.</param>
        /// <param name="target">The object that should be populated.</param>
        public void Populate(JToken token, object target)
        {
            // TODO: maybe rethrow errors with line number and position
            using var reader = new JTokenReader(token, Path);
            new JsonContext(reader, _context.FileName, _context.Schema).Populate(target);
        }
    }

    /// <summary>
    /// Class capturing the JSON context and a value.
    /// </summary>
    /// <typeparam name="T">The type of the captured value.</typeparam>
    public class JsonTrace<T> : JsonTrace
    {
        internal JsonTrace(JsonContext context, T value) : base(context) => Value = value;

        /// <summary>
        /// Returns the captured value.
        /// </summary>
        public T Value { get; }
    }

    /// <summary>
    /// Base class for all JSON-serialized configuration objects.
    /// </summary>
    public abstract class JsonElement
    {
        private Schema? _schema;
        private JsonTrace? _autoTrace;
        private JsonContext? _json;

        /// <summary>
        /// Creates a new instance and populates all default values.
        /// </summary>
        [JsonConstructor]
        public JsonElement()
        {
            HasSchema = true; // every JSON element is expected to have a schema eventually
            foreach (var property in GetType().GetProperties())
            {
                var attribute = property.GetCustomAttribute<DefaultValueAttribute>();
                if (attribute is not null) property.SetValue(this, attribute.Value);
            }
        }

        /// <summary>
        /// Creates a new instance with a given <see cref="Types.Schema"/>.
        /// </summary>
        /// <param name="schema">The <see cref="Types.Schema"/> this <see cref="JsonElement"/> belongs to or <c>null</c> if the element is internal.</param>
        public JsonElement(Schema? schema)
        {
            _schema = schema;
            HasSchema = schema is not null;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            if (!HasSchema) throw new ApplicationException($"{this} cannot be deserialized.");
            _json = context;
            _autoTrace = new JsonTrace(_json);
            if (_schema is not null && _schema != _json.Schema) throw new InvalidOperationException("Deserialization from different schema.");
            _schema = _json.Schema;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_json is null) throw new ApplicationException("OnDeserialized called without matching OnDeserializing.");
            try { JsonInitialize(); }
            finally { _json = null; }
        }

        private string GetFullPath(string fileName) => Path.Combine((_json is null ? AppDomain.CurrentDomain.BaseDirectory : Path.GetDirectoryName(_json.FileName)) ?? Environment.CurrentDirectory, fileName);

        internal T EnsureAbsoluteUri<T>(T uri, [CallerMemberName] string? memberName = null) where T : Uri?
        {
            if (uri?.IsAbsoluteUri ?? true) return uri;
            if (_json is null) throw new ArgumentException($"{memberName} must be an absolute URI.");
            throw JsonError("Must be an absolute URI.");
        }

        internal string EnsureAbsolutePath(string value) => Path.IsPathRooted(value) ? value : GetFullPath(value);

        internal Uri EnsureAbsolutePath(Uri value) => value.IsAbsoluteUri ? value : new Uri(GetFullPath(value.ToString()));

        /// <summary>
        /// Returns a new exception object describing the current JSON context.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        /// <returns>A new <see cref="JsonSerializationException"/> object.</returns>
        /// <exception cref="InvalidOperationException">If the method was called outside of an JSON object.</exception>
        protected internal JsonSerializationException JsonError(string message) => _json is not null
            ? new JsonSerializationException(message, _json.Path, _json.LineNumber, _json.LinePosition, null)
            : _autoTrace is not null ? _autoTrace.Error(message) : throw new InvalidOperationException($"Error on non-JSON object: {message}");

        /// <summary>
        /// Overrideable function that gets called once the object is deserialized.
        /// </summary>
        protected virtual void JsonInitialize() { }

        /// <summary>
        /// Captures the current JSON position.
        /// </summary>
        /// <returns>A captured <see cref="Configuration.JsonTrace"/>.</returns>
        /// <exception cref="InvalidOperationException">If the object is not currently deserialized.</exception>
        protected JsonTrace JsonTrace() => new JsonTrace(_json ?? throw new InvalidOperationException("No trace possible on non-JSON objects."));

        /// <summary>
        /// Captures the current JSON position together with a value.
        /// </summary>
        /// <typeparam name="T">The type of the value to capture.</typeparam>
        /// <param name="value">The value to capture.</param>
        /// <returns>A <see cref="Configuration.JsonTrace{T}"/> containing the value and captured JSON position or <c>null</c> if <paramref name="value"/> is <c>null</c>.</returns>
        /// <exception cref="InvalidOperationException">If the object is not currently deserialized.</exception>
        protected JsonTrace<T>? JsonTrace<T>(T? value) where T : class => value is null ? null : new JsonTrace<T>(_json ?? throw new InvalidOperationException("No value trace possible on non-JSON objects."), value);

        /// <summary>
        /// Captures the current JSON position together with a value.
        /// </summary>
        /// <typeparam name="T">The type of the value to capture.</typeparam>
        /// <param name="value">The value to capture.</param>
        /// <returns>A <see cref="Configuration.JsonTrace{T}"/> containing the value and captured JSON position or <c>null</c> if <paramref name="value"/> is <c>null</c>.</returns>
        /// <exception cref="InvalidOperationException">If the object is not currently deserialized.</exception>
        protected JsonTrace<T>? JsonTrace<T>(T? value) where T : struct => value is null ? null : new JsonTrace<T>(_json ?? throw new InvalidOperationException("No value trace possible on non-JSON objects."), value.Value);

        /// <summary>
        /// Indicates whether this object is internal and cannot be deserialized.
        /// </summary>
        [JsonIgnore]
        public bool HasSchema { get; }

        /// <summary>
        /// Gets the <see cref="Schema"/> this <see cref="JsonElement"/> belongs to.
        /// </summary>
        /// <exception cref="InvalidOperationException">If <see cref="HasSchema"/> is <c>false</c>.</exception>
        [JsonIgnore]
        public Schema Schema => _schema ?? throw new InvalidOperationException($"{this} is internal and does not belong to a schema.");
    }
}
