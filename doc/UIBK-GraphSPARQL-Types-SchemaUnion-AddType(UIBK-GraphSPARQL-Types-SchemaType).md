#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion')
## SchemaUnion.AddType(UIBK.GraphSPARQL.Types.SchemaType) Method
Declares a given type to be contained in this union.  
```csharp
public UIBK.GraphSPARQL.Types.SchemaUnion AddType(UIBK.GraphSPARQL.Types.SchemaType type);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaUnion-AddType(UIBK-GraphSPARQL-Types-SchemaType)-type'></a>
`type` [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType')  
The [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType') to add to this union.  
  
#### Returns
[SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion')  
The current [SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion').  
#### Exceptions
[System.ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentException 'System.ArgumentException')  
Another type with the same [ClassIri](./UIBK-GraphSPARQL-Types-SchemaType-ClassIri.md 'UIBK.GraphSPARQL.Types.SchemaType.ClassIri') has already been added.  
[System.InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/System.InvalidOperationException 'System.InvalidOperationException')  
If the [type](#UIBK-GraphSPARQL-Types-SchemaUnion-AddType(UIBK-GraphSPARQL-Types-SchemaType)-type 'UIBK.GraphSPARQL.Types.SchemaUnion.AddType(UIBK.GraphSPARQL.Types.SchemaType).type') belongs to a different schema.  
