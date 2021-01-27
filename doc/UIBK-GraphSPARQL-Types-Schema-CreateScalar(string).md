#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema')
## Schema.CreateScalar(string) Method
Creates and adds a new [SchemaCustomScalar](./UIBK-GraphSPARQL-Types-SchemaCustomScalar.md 'UIBK.GraphSPARQL.Types.SchemaCustomScalar') to the [Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema').  
```csharp
public UIBK.GraphSPARQL.Types.SchemaCustomScalar CreateScalar(string name);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-Schema-CreateScalar(string)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the [SchemaCustomScalar](./UIBK-GraphSPARQL-Types-SchemaCustomScalar.md 'UIBK.GraphSPARQL.Types.SchemaCustomScalar').  
  
#### Returns
[SchemaCustomScalar](./UIBK-GraphSPARQL-Types-SchemaCustomScalar.md 'UIBK.GraphSPARQL.Types.SchemaCustomScalar')  
A new [SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar') instance.  
#### Exceptions
[System.ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentException 'System.ArgumentException')  
If another [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType'), [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface'), [SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion'), [SchemaCustomScalar](./UIBK-GraphSPARQL-Types-SchemaCustomScalar.md 'UIBK.GraphSPARQL.Types.SchemaCustomScalar') or [SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum') with the given [name](#UIBK-GraphSPARQL-Types-Schema-CreateScalar(string)-name 'UIBK.GraphSPARQL.Types.Schema.CreateScalar(string).name') already exists or [name](#UIBK-GraphSPARQL-Types-Schema-CreateScalar(string)-name 'UIBK.GraphSPARQL.Types.Schema.CreateScalar(string).name') contains invalid characters.  
