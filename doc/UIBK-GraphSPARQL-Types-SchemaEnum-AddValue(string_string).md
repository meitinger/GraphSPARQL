#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum')
## SchemaEnum.AddValue(string, string) Method
Registers a value for a given name.  
```csharp
public UIBK.GraphSPARQL.Types.SchemaEnum AddValue(string name, string value);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaEnum-AddValue(string_string)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The value's name. This is what GraphQL uses.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaEnum-AddValue(string_string)-value'></a>
`value` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The value send to SPARQL.  
  
#### Returns
[SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum')  
The current [SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum').  
#### Exceptions
[System.ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentException 'System.ArgumentException')  
If another entry with the given [name](#UIBK-GraphSPARQL-Types-SchemaEnum-AddValue(string_string)-name 'UIBK.GraphSPARQL.Types.SchemaEnum.AddValue(string, string).name') or [value](#UIBK-GraphSPARQL-Types-SchemaEnum-AddValue(string_string)-value 'UIBK.GraphSPARQL.Types.SchemaEnum.AddValue(string, string).value') has already been defined.  
