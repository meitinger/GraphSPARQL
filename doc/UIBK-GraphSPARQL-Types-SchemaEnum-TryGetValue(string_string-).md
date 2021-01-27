#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum')
## SchemaEnum.TryGetValue(string, string?) Method
Tries to find an entry by name and return its value.  
```csharp
public bool TryGetValue(string name, out string? value);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaEnum-TryGetValue(string_string-)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The entry's name.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaEnum-TryGetValue(string_string-)-value'></a>
`value` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The found entry's value.  
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if an with the given [name](#UIBK-GraphSPARQL-Types-SchemaEnum-TryGetValue(string_string-)-name 'UIBK.GraphSPARQL.Types.SchemaEnum.TryGetValue(string, string?).name') has been found, `falce` otherwise.  
