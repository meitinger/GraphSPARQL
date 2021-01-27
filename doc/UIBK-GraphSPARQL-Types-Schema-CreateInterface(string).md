#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema')
## Schema.CreateInterface(string) Method
Creates and adds a new [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface') to the [Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema').  
```csharp
public UIBK.GraphSPARQL.Types.SchemaInterface CreateInterface(string name);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-Schema-CreateInterface(string)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface').  
  
#### Returns
[SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface')  
A new [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface') instance.  
#### Exceptions
[System.ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentException 'System.ArgumentException')  
If another [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType'), [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface'), [SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion'), [SchemaCustomScalar](./UIBK-GraphSPARQL-Types-SchemaCustomScalar.md 'UIBK.GraphSPARQL.Types.SchemaCustomScalar') or [SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum') with the given [name](#UIBK-GraphSPARQL-Types-Schema-CreateInterface(string)-name 'UIBK.GraphSPARQL.Types.Schema.CreateInterface(string).name') already exists or [name](#UIBK-GraphSPARQL-Types-Schema-CreateInterface(string)-name 'UIBK.GraphSPARQL.Types.Schema.CreateInterface(string).name') contains invalid characters.  
