#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar')
## SchemaScalar.LanguageString(string?) Method
Defines a new language string scalar.  
```csharp
public static UIBK.GraphSPARQL.Types.SchemaScalar LanguageString(string? format=null);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaScalar-LanguageString(string-)-format'></a>
`format` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The default locale or `null` to use the thread's current culture.  
  
#### Returns
[SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar')  
A [SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar') describing a language string.  
