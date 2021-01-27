#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Configuration](./UIBK-GraphSPARQL-Configuration.md 'UIBK.GraphSPARQL.Configuration').[JsonElement](./UIBK-GraphSPARQL-Configuration-JsonElement.md 'UIBK.GraphSPARQL.Configuration.JsonElement')
## JsonElement.Schema Property
Gets the [Schema](./UIBK-GraphSPARQL-Configuration-JsonElement-Schema.md 'UIBK.GraphSPARQL.Configuration.JsonElement.Schema') this [JsonElement](./UIBK-GraphSPARQL-Configuration-JsonElement.md 'UIBK.GraphSPARQL.Configuration.JsonElement') belongs to.  
```csharp
public UIBK.GraphSPARQL.Types.Schema Schema { get; }
```
#### Property Value
[Schema](./UIBK-GraphSPARQL-Types-Schema.md 'UIBK.GraphSPARQL.Types.Schema')  
#### Exceptions
[System.InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/System.InvalidOperationException 'System.InvalidOperationException')  
If [HasSchema](./UIBK-GraphSPARQL-Configuration-JsonElement-HasSchema.md 'UIBK.GraphSPARQL.Configuration.JsonElement.HasSchema') is `false`.  
