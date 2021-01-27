#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion')
## SchemaUnion.TryGetType(string, UIBK.GraphSPARQL.Types.SchemaType?) Method
Tries to find and return an added [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType') by name.  
```csharp
public bool TryGetType(string name, out UIBK.GraphSPARQL.Types.SchemaType? type);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaUnion-TryGetType(string_UIBK-GraphSPARQL-Types-SchemaType-)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType').  
  
<a name='UIBK-GraphSPARQL-Types-SchemaUnion-TryGetType(string_UIBK-GraphSPARQL-Types-SchemaType-)-type'></a>
`type` [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType')  
The found [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType').  
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType') with the given [name](#UIBK-GraphSPARQL-Types-SchemaUnion-TryGetType(string_UIBK-GraphSPARQL-Types-SchemaType-)-name 'UIBK.GraphSPARQL.Types.SchemaUnion.TryGetType(string, UIBK.GraphSPARQL.Types.SchemaType?).name') has been found, `falce` otherwise.  
