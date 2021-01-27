#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar')
## SchemaScalar.DateTime(UIBK.GraphSPARQL.Iri?, string?) Method
Defines a new date and time scalar.  
```csharp
public static UIBK.GraphSPARQL.Types.SchemaScalar DateTime(UIBK.GraphSPARQL.Iri? dataTypeIri=null, string? format=null);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaScalar-DateTime(UIBK-GraphSPARQL-Iri-_string-)-dataTypeIri'></a>
`dataTypeIri` [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')  
The underlying data type IRI.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaScalar-DateTime(UIBK-GraphSPARQL-Iri-_string-)-format'></a>
`format` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The formatting string to use on serializing the date and time, see [System.DateTime.ToString(System.String)](https://docs.microsoft.com/en-us/dotnet/api/System.DateTime.ToString#System_DateTime_ToString_System_String_ 'System.DateTime.ToString(System.String)').  
  
#### Returns
[SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar')  
A [SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar') describing date and time.  
