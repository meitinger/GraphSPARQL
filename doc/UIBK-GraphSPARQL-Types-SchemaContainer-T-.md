#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types')
## SchemaContainer&lt;T&gt; Class
Base class of [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType'), [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface'), [SchemaQuery](./UIBK-GraphSPARQL-Types-SchemaQuery.md 'UIBK.GraphSPARQL.Types.SchemaQuery') and [SchemaMutation](./UIBK-GraphSPARQL-Types-SchemaMutation.md 'UIBK.GraphSPARQL.Types.SchemaMutation').  
```csharp
public abstract class SchemaContainer<T> : UIBK.GraphSPARQL.Types.SchemaContainer
    where T : GraphQL.Types.IComplexGraphType, new()
```
Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [JsonElement](./UIBK-GraphSPARQL-Configuration-JsonElement.md 'UIBK.GraphSPARQL.Configuration.JsonElement') &#129106; [SchemaElement](./UIBK-GraphSPARQL-Types-SchemaElement.md 'UIBK.GraphSPARQL.Types.SchemaElement') &#129106; [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement') &#129106; [SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer') &#129106; SchemaContainer&lt;T&gt;  

Derived  
&#8627; [SchemaObjectContainer&lt;T&gt;](./UIBK-GraphSPARQL-Types-SchemaObjectContainer-T-.md 'UIBK.GraphSPARQL.Types.SchemaObjectContainer&lt;T&gt;')  
&#8627; [SchemaRootContainer](./UIBK-GraphSPARQL-Types-SchemaRootContainer.md 'UIBK.GraphSPARQL.Types.SchemaRootContainer')  
#### Type parameters
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-T--T'></a>
`T`  
The type of the [QueryType](./UIBK-GraphSPARQL-Types-SchemaTypeElement-QueryType.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement.QueryType').  

Constraints [GraphQL.Types.IComplexGraphType](https://docs.microsoft.com/en-us/dotnet/api/GraphQL.Types.IComplexGraphType 'GraphQL.Types.IComplexGraphType')  
  
### Properties
- [QueryType](./UIBK-GraphSPARQL-Types-SchemaContainer-T--QueryType.md 'UIBK.GraphSPARQL.Types.SchemaContainer&lt;T&gt;.QueryType')
