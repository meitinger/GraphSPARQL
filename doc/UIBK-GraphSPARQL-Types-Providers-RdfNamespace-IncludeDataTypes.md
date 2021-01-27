#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types.Providers](./UIBK-GraphSPARQL-Types-Providers.md 'UIBK.GraphSPARQL.Types.Providers').[RdfNamespace](./UIBK-GraphSPARQL-Types-Providers-RdfNamespace.md 'UIBK.GraphSPARQL.Types.Providers.RdfNamespace')
## RdfNamespace.IncludeDataTypes Property
Indicates whether data types should be harvested.  
If `false` and [IncludeProperties](./UIBK-GraphSPARQL-Types-Providers-RdfNamespace-IncludeProperties.md 'UIBK.GraphSPARQL.Types.Providers.RdfNamespace.IncludeProperties') is `true`, only properties  
referencing previously defined or known data types are harvested.  
```csharp
public bool IncludeDataTypes { get; set; }
```
#### Property Value
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
