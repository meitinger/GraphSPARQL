#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema')
## Schema.TryGetElement(string, UIBK.GraphSPARQL.Types.SchemaTypeElement?) Method
Tries to find a [SchemaElement](./UIBK-GraphSPARQL-Types-SchemaElement.md 'UIBK.GraphSPARQL.Types.SchemaElement') with a given name.  
```csharp
public bool TryGetElement(string name, out UIBK.GraphSPARQL.Types.SchemaTypeElement? element);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-Schema-TryGetElement(string_UIBK-GraphSPARQL-Types-SchemaTypeElement-)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement') to retrieve.  
  
<a name='UIBK-GraphSPARQL-Types-Schema-TryGetElement(string_UIBK-GraphSPARQL-Types-SchemaTypeElement-)-element'></a>
`element` [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement')  
The matching [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement').  
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the [Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema') contains the specified [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement'), `false` otherwise.  
