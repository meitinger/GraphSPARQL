#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types')
## Schema Class
Root class describing the GraphQL schema.  
```csharp
public sealed class Schema : GraphQL.Utilities.MetadataProvider,
GraphQL.Types.ISchema,
GraphQL.Types.IProvideMetadata,
System.IServiceProvider
```
Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [GraphQL.Utilities.MetadataProvider](https://docs.microsoft.com/en-us/dotnet/api/GraphQL.Utilities.MetadataProvider 'GraphQL.Utilities.MetadataProvider') &#129106; Schema  

Implements [GraphQL.Types.ISchema](https://docs.microsoft.com/en-us/dotnet/api/GraphQL.Types.ISchema 'GraphQL.Types.ISchema'), [GraphQL.Types.IProvideMetadata](https://docs.microsoft.com/en-us/dotnet/api/GraphQL.Types.IProvideMetadata 'GraphQL.Types.IProvideMetadata'), [System.IServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/System.IServiceProvider 'System.IServiceProvider')  
### Properties
- [BuiltinScalars](./UIBK-GraphSPARQL-Types-Schema-BuiltinScalars.md 'UIBK.GraphSPARQL.Types.Schema.BuiltinScalars')
- [DataSources](./UIBK-GraphSPARQL-Types-Schema-DataSources.md 'UIBK.GraphSPARQL.Types.Schema.DataSources')
- [Elements](./UIBK-GraphSPARQL-Types-Schema-Elements.md 'UIBK.GraphSPARQL.Types.Schema.Elements')
- [Enums](./UIBK-GraphSPARQL-Types-Schema-Enums.md 'UIBK.GraphSPARQL.Types.Schema.Enums')
- [Interfaces](./UIBK-GraphSPARQL-Types-Schema-Interfaces.md 'UIBK.GraphSPARQL.Types.Schema.Interfaces')
- [Mutation](./UIBK-GraphSPARQL-Types-Schema-Mutation.md 'UIBK.GraphSPARQL.Types.Schema.Mutation')
- [Query](./UIBK-GraphSPARQL-Types-Schema-Query.md 'UIBK.GraphSPARQL.Types.Schema.Query')
- [Scalars](./UIBK-GraphSPARQL-Types-Schema-Scalars.md 'UIBK.GraphSPARQL.Types.Schema.Scalars')
- [Types](./UIBK-GraphSPARQL-Types-Schema-Types.md 'UIBK.GraphSPARQL.Types.Schema.Types')
- [Unions](./UIBK-GraphSPARQL-Types-Schema-Unions.md 'UIBK.GraphSPARQL.Types.Schema.Unions')
### Methods
- [Configure(string)](./UIBK-GraphSPARQL-Types-Schema-Configure(string).md 'UIBK.GraphSPARQL.Types.Schema.Configure(string)')
- [CreateEnum(string, UIBK.GraphSPARQL.Iri)](./UIBK-GraphSPARQL-Types-Schema-CreateEnum(string_UIBK-GraphSPARQL-Iri).md 'UIBK.GraphSPARQL.Types.Schema.CreateEnum(string, UIBK.GraphSPARQL.Iri)')
- [CreateInterface(string)](./UIBK-GraphSPARQL-Types-Schema-CreateInterface(string).md 'UIBK.GraphSPARQL.Types.Schema.CreateInterface(string)')
- [CreateScalar(string)](./UIBK-GraphSPARQL-Types-Schema-CreateScalar(string).md 'UIBK.GraphSPARQL.Types.Schema.CreateScalar(string)')
- [CreateType(string, UIBK.GraphSPARQL.Iri)](./UIBK-GraphSPARQL-Types-Schema-CreateType(string_UIBK-GraphSPARQL-Iri).md 'UIBK.GraphSPARQL.Types.Schema.CreateType(string, UIBK.GraphSPARQL.Iri)')
- [CreateUnion(string)](./UIBK-GraphSPARQL-Types-Schema-CreateUnion(string).md 'UIBK.GraphSPARQL.Types.Schema.CreateUnion(string)')
- [ToJson(Newtonsoft.Json.JsonWriter)](./UIBK-GraphSPARQL-Types-Schema-ToJson(Newtonsoft-Json-JsonWriter).md 'UIBK.GraphSPARQL.Types.Schema.ToJson(Newtonsoft.Json.JsonWriter)')
- [ToJson(System.IO.TextWriter)](./UIBK-GraphSPARQL-Types-Schema-ToJson(System-IO-TextWriter).md 'UIBK.GraphSPARQL.Types.Schema.ToJson(System.IO.TextWriter)')
- [TryGetElement(string, UIBK.GraphSPARQL.Types.SchemaTypeElement?)](./UIBK-GraphSPARQL-Types-Schema-TryGetElement(string_UIBK-GraphSPARQL-Types-SchemaTypeElement-).md 'UIBK.GraphSPARQL.Types.Schema.TryGetElement(string, UIBK.GraphSPARQL.Types.SchemaTypeElement?)')
- [TryGetElement&lt;T&gt;(string, T?)](./UIBK-GraphSPARQL-Types-Schema-TryGetElement-T-(string_T-).md 'UIBK.GraphSPARQL.Types.Schema.TryGetElement&lt;T&gt;(string, T?)')
