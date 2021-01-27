#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema')
## Schema.TryGetElement&lt;T&gt;(string, T?) Method
Tries to find a [SchemaElement](./UIBK-GraphSPARQL-Types-SchemaElement.md 'UIBK.GraphSPARQL.Types.SchemaElement') of type [T](#UIBK-GraphSPARQL-Types-Schema-TryGetElement-T-(string_T-)-T 'UIBK.GraphSPARQL.Types.Schema.TryGetElement&lt;T&gt;(string, T?).T') with a given name.  
```csharp
public bool TryGetElement<T>(string name, out T? element)
    where T : UIBK.GraphSPARQL.Types.SchemaTypeElement;
```
#### Type parameters
<a name='UIBK-GraphSPARQL-Types-Schema-TryGetElement-T-(string_T-)-T'></a>
`T`  
The desired [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement') type.  

Constraints [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement')  
  
#### Parameters
<a name='UIBK-GraphSPARQL-Types-Schema-TryGetElement-T-(string_T-)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement') to retrieve.  
  
<a name='UIBK-GraphSPARQL-Types-Schema-TryGetElement-T-(string_T-)-element'></a>
`element` [T](#UIBK-GraphSPARQL-Types-Schema-TryGetElement-T-(string_T-)-T 'UIBK.GraphSPARQL.Types.Schema.TryGetElement&lt;T&gt;(string, T?).T')  
The matching [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement').  
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the [Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema') contains the specified [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement'), `false` otherwise.  
