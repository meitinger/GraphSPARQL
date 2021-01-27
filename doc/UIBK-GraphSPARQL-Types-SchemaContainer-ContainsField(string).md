#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer')
## SchemaContainer.ContainsField(string) Method
Checks if the [SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer') contains a [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField') with a given name.  
```csharp
public bool ContainsField(string name);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-ContainsField(string)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of a [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField') to check.  
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if a [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField') with the given [name](#UIBK-GraphSPARQL-Types-SchemaContainer-ContainsField(string)-name 'UIBK.GraphSPARQL.Types.SchemaContainer.ContainsField(string).name') has been added, `falce` otherwise.  
