#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType')
## SchemaType.AddInterface(UIBK.GraphSPARQL.Types.SchemaInterface) Method
Declares that the type inherits from a given interface.  
```csharp
public UIBK.GraphSPARQL.Types.SchemaType AddInterface(UIBK.GraphSPARQL.Types.SchemaInterface @interface);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaType-AddInterface(UIBK-GraphSPARQL-Types-SchemaInterface)-interface'></a>
`interface` [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface')  
The [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface') that this type should inherit from.  
  
#### Returns
[SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType')  
This current [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType').  
#### Exceptions
[System.ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentException 'System.ArgumentException')  
Another type with the same [ClassIri](./UIBK-GraphSPARQL-Types-SchemaType-ClassIri.md 'UIBK.GraphSPARQL.Types.SchemaType.ClassIri') already inherits from the [interface](#UIBK-GraphSPARQL-Types-SchemaType-AddInterface(UIBK-GraphSPARQL-Types-SchemaInterface)-interface 'UIBK.GraphSPARQL.Types.SchemaType.AddInterface(UIBK.GraphSPARQL.Types.SchemaInterface).interface').  
[System.InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/System.InvalidOperationException 'System.InvalidOperationException')  
If the [interface](#UIBK-GraphSPARQL-Types-SchemaType-AddInterface(UIBK-GraphSPARQL-Types-SchemaInterface)-interface 'UIBK.GraphSPARQL.Types.SchemaType.AddInterface(UIBK.GraphSPARQL.Types.SchemaInterface).interface') belongs to a different schema.  
