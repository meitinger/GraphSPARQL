#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar')
## SchemaScalar.Boolean(UIBK.GraphSPARQL.Iri?, string?) Method
Defines a new boolean scalar.  
```csharp
public static UIBK.GraphSPARQL.Types.SchemaScalar Boolean(UIBK.GraphSPARQL.Iri? dataTypeIri=null, string? format=null);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaScalar-Boolean(UIBK-GraphSPARQL-Iri-_string-)-dataTypeIri'></a>
`dataTypeIri` [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')  
The underlying data type IRI.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaScalar-Boolean(UIBK-GraphSPARQL-Iri-_string-)-format'></a>
`format` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
`"c"` to format the boolean as `"yes"`/`"no"`, `"b"` to format the boolean as `"true"`/`"false"` or `"n"` to format the boolean as `"1"`/`"0"`.  
  
#### Returns
[SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar')  
A [SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar') describing a boolean.  
