#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum')
## SchemaEnum.TryGetValueName(string, string?) Method
Tries to find an entry by value and return its name.  
```csharp
public bool TryGetValueName(string value, out string? name);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaEnum-TryGetValueName(string_string-)-value'></a>
`value` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The entry's value.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaEnum-TryGetValueName(string_string-)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The found entry's name.  
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if an with the given [value](#UIBK-GraphSPARQL-Types-SchemaEnum-TryGetValueName(string_string-)-value 'UIBK.GraphSPARQL.Types.SchemaEnum.TryGetValueName(string, string?).value') has been found, `falce` otherwise.  
