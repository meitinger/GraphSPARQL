#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType')
## SchemaType.TryGetInterface(string, UIBK.GraphSPARQL.Types.SchemaInterface?) Method
Tries to find and return an added [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface') by name.  
```csharp
public bool TryGetInterface(string name, out UIBK.GraphSPARQL.Types.SchemaInterface? @interface);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaType-TryGetInterface(string_UIBK-GraphSPARQL-Types-SchemaInterface-)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface').  
  
<a name='UIBK-GraphSPARQL-Types-SchemaType-TryGetInterface(string_UIBK-GraphSPARQL-Types-SchemaInterface-)-interface'></a>
`interface` [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface')  
The found [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface').  
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface') with the given [name](#UIBK-GraphSPARQL-Types-SchemaType-TryGetInterface(string_UIBK-GraphSPARQL-Types-SchemaInterface-)-name 'UIBK.GraphSPARQL.Types.SchemaType.TryGetInterface(string, UIBK.GraphSPARQL.Types.SchemaInterface?).name') has been found, `falce` otherwise.  
