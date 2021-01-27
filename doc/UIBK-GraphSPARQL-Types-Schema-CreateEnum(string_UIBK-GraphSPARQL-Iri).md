#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema')
## Schema.CreateEnum(string, UIBK.GraphSPARQL.Iri) Method
Creates and adds a new [SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum') to the [Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema').  
```csharp
public UIBK.GraphSPARQL.Types.SchemaEnum CreateEnum(string name, UIBK.GraphSPARQL.Iri dataTypeIri);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-Schema-CreateEnum(string_UIBK-GraphSPARQL-Iri)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the [SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum').  
  
<a name='UIBK-GraphSPARQL-Types-Schema-CreateEnum(string_UIBK-GraphSPARQL-Iri)-dataTypeIri'></a>
`dataTypeIri` [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')  
The [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri') of the underlying data type.  
  
#### Returns
[SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum')  
A new [SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum') instance.  
#### Exceptions
[System.ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentException 'System.ArgumentException')  
If another [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType'), [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface'), [SchemaUnion](./UIBK-GraphSPARQL-Types-SchemaUnion.md 'UIBK.GraphSPARQL.Types.SchemaUnion'), [SchemaCustomScalar](./UIBK-GraphSPARQL-Types-SchemaCustomScalar.md 'UIBK.GraphSPARQL.Types.SchemaCustomScalar') or [SchemaEnum](./UIBK-GraphSPARQL-Types-SchemaEnum.md 'UIBK.GraphSPARQL.Types.SchemaEnum') with the given [name](#UIBK-GraphSPARQL-Types-Schema-CreateEnum(string_UIBK-GraphSPARQL-Iri)-name 'UIBK.GraphSPARQL.Types.Schema.CreateEnum(string, UIBK.GraphSPARQL.Iri).name') already exists or [name](#UIBK-GraphSPARQL-Types-Schema-CreateEnum(string_UIBK-GraphSPARQL-Iri)-name 'UIBK.GraphSPARQL.Types.Schema.CreateEnum(string, UIBK.GraphSPARQL.Iri).name') contains invalid characters.  
### Remarks
Use [IriDataTypeIri](./UIBK-GraphSPARQL-Types-SchemaScalar-IriDataTypeIri.md 'UIBK.GraphSPARQL.Types.SchemaScalar.IriDataTypeIri') for [dataTypeIri](#UIBK-GraphSPARQL-Types-Schema-CreateEnum(string_UIBK-GraphSPARQL-Iri)-dataTypeIri 'UIBK.GraphSPARQL.Types.Schema.CreateEnum(string, UIBK.GraphSPARQL.Iri).dataTypeIri') in case the enum values represent IRIs or [PlainLiteralDataTypeIri](./UIBK-GraphSPARQL-Types-SchemaScalar-PlainLiteralDataTypeIri.md 'UIBK.GraphSPARQL.Types.SchemaScalar.PlainLiteralDataTypeIri') to not use data types at all.  
