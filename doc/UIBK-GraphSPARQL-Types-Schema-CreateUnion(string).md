#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema')
## Schema.CreateUnion(string) Method
Creates and adds a new [SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion') to the [Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema').  
```csharp
public UIBK.GraphSPARQL.Types.SchemaUnion CreateUnion(string name);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-Schema-CreateUnion(string)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the [SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion').  
  
#### Returns
[SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion')  
A new [SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion') instance.  
#### Exceptions
[System.ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentException 'System.ArgumentException')  
If another [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType'), [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface'), [SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion'), [SchemaCustomScalar](./UIBK-GraphSPARQL-Types-SchemaCustomScalar.md 'UIBK.GraphSPARQL.Types.SchemaCustomScalar') or [SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum') with the given [name](#UIBK-GraphSPARQL-Types-Schema-CreateUnion(string)-name 'UIBK.GraphSPARQL.Types.Schema.CreateUnion(string).name') already exists or [name](#UIBK-GraphSPARQL-Types-Schema-CreateUnion(string)-name 'UIBK.GraphSPARQL.Types.Schema.CreateUnion(string).name') contains invalid characters.  
