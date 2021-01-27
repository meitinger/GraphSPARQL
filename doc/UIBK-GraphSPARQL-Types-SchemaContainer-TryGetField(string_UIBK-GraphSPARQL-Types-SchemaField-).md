#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer')
## SchemaContainer.TryGetField(string, UIBK.GraphSPARQL.Types.SchemaField?) Method
Tries to find and return an added [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField') by name.  
```csharp
public bool TryGetField(string name, out UIBK.GraphSPARQL.Types.SchemaField? field);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-TryGetField(string_UIBK-GraphSPARQL-Types-SchemaField-)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField').  
  
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-TryGetField(string_UIBK-GraphSPARQL-Types-SchemaField-)-field'></a>
`field` [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField')  
The found [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField').  
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField') with the given [name](#UIBK-GraphSPARQL-Types-SchemaContainer-TryGetField(string_UIBK-GraphSPARQL-Types-SchemaField-)-name 'UIBK.GraphSPARQL.Types.SchemaContainer.TryGetField(string, UIBK.GraphSPARQL.Types.SchemaField?).name') has been found, `falce` otherwise.  
