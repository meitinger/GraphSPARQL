#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema')
## Schema.CreateType(string, UIBK.GraphSPARQL.Iri) Method
Creates and adds a new [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType') to the [Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema').  
```csharp
public UIBK.GraphSPARQL.Types.SchemaType CreateType(string name, UIBK.GraphSPARQL.Iri classIri);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-Schema-CreateType(string_UIBK-GraphSPARQL-Iri)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType').  
  
<a name='UIBK-GraphSPARQL-Types-Schema-CreateType(string_UIBK-GraphSPARQL-Iri)-classIri'></a>
`classIri` [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')  
The underlying class's IRI.  
  
#### Returns
[SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType')  
A new [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType') instance.  
#### Exceptions
[System.ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentException 'System.ArgumentException')  
If another [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType'), [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface'), [SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion'), [SchemaCustomScalar](./UIBK-GraphSPARQL-Types-SchemaCustomScalar.md 'UIBK.GraphSPARQL.Types.SchemaCustomScalar') or [SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum') with the given [name](#UIBK-GraphSPARQL-Types-Schema-CreateType(string_UIBK-GraphSPARQL-Iri)-name 'UIBK.GraphSPARQL.Types.Schema.CreateType(string, UIBK.GraphSPARQL.Iri).name') already exists or [name](#UIBK-GraphSPARQL-Types-Schema-CreateType(string_UIBK-GraphSPARQL-Iri)-name 'UIBK.GraphSPARQL.Types.Schema.CreateType(string, UIBK.GraphSPARQL.Iri).name') contains invalid characters.  
