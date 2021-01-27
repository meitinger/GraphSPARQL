#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types.Providers](./UIBK-GraphSPARQL-Types-Providers.md 'UIBK.GraphSPARQL.Types.Providers').[RdfNamespace](./UIBK-GraphSPARQL-Types-Providers-RdfNamespace.md 'UIBK.GraphSPARQL.Types.Providers.RdfNamespace')
## RdfNamespace.ExcludePropertiesWithoutDomain Property
Indicates whether a [RdfProperty](./UIBK-GraphSPARQL-Types-Providers-RdfProperty.md 'UIBK.GraphSPARQL.Types.Providers.RdfProperty') with an empty domain should be ignored.  
If `false`, the [RdfProperty](./UIBK-GraphSPARQL-Types-Providers-RdfProperty.md 'UIBK.GraphSPARQL.Types.Providers.RdfProperty') gets harvested under the `IAny`  
interface that every other [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType') inherits from.  
```csharp
public bool ExcludePropertiesWithoutDomain { get; set; }
```
#### Property Value
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
