#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types')
## SchemaObjectContainer&lt;T&gt; Class
Base class of [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType') and [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface').  
```csharp
public abstract class SchemaObjectContainer<T> : UIBK.GraphSPARQL.Types.SchemaContainer<T>
    where T : GraphQL.Types.IComplexGraphType, new()
```
Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [JsonElement](./UIBK-GraphSPARQL-Configuration-JsonElement.md 'UIBK.GraphSPARQL.Configuration.JsonElement') &#129106; [SchemaElement](./UIBK-GraphSPARQL-Types-SchemaElement.md 'UIBK.GraphSPARQL.Types.SchemaElement') &#129106; [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement') &#129106; [SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer') &#129106; [UIBK.GraphSPARQL.Types.SchemaContainer&lt;](./UIBK-GraphSPARQL-Types-SchemaContainer-T-.md 'UIBK.GraphSPARQL.Types.SchemaContainer&lt;T&gt;')[T](#UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--T 'UIBK.GraphSPARQL.Types.SchemaObjectContainer&lt;T&gt;.T')[&gt;](./UIBK-GraphSPARQL-Types-SchemaContainer-T-.md 'UIBK.GraphSPARQL.Types.SchemaContainer&lt;T&gt;') &#129106; SchemaObjectContainer&lt;T&gt;  

Derived  
&#8627; [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface')  
&#8627; [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType')  
#### Type parameters
<a name='UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--T'></a>
`T`  

Constraints [GraphQL.Types.IComplexGraphType](https://docs.microsoft.com/en-us/dotnet/api/GraphQL.Types.IComplexGraphType 'GraphQL.Types.IComplexGraphType')  
  
### Methods
- [AddField(string, UIBK.GraphSPARQL.Types.SchemaScalar, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?)](./UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-).md 'UIBK.GraphSPARQL.Types.SchemaObjectContainer&lt;T&gt;.AddField(string, UIBK.GraphSPARQL.Types.SchemaScalar, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?)')
