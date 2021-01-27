#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types.Providers](./UIBK-GraphSPARQL-Types-Providers.md 'UIBK.GraphSPARQL.Types.Providers').[RdfNamespace](./UIBK-GraphSPARQL-Types-Providers-RdfNamespace.md 'UIBK.GraphSPARQL.Types.Providers.RdfNamespace')
## RdfNamespace.ExcludePropertiesWithoutRange Property
Indicates whether a [RdfProperty](./UIBK-GraphSPARQL-Types-Providers-RdfProperty.md 'UIBK.GraphSPARQL.Types.Providers.RdfProperty') with an empty range should be ignored.  
If `false`, the [RdfProperty](./UIBK-GraphSPARQL-Types-Providers-RdfProperty.md 'UIBK.GraphSPARQL.Types.Providers.RdfProperty') gets harvested as returning either the  
`IAny` interface or a custom scalar with an `any` data type IRI.  
```csharp
public bool ExcludePropertiesWithoutRange { get; set; }
```
#### Property Value
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
