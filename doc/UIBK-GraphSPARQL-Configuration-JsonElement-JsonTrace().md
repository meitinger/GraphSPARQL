#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Configuration](./UIBK-GraphSPARQL-Configuration.md 'UIBK.GraphSPARQL.Configuration').[JsonElement](./UIBK-GraphSPARQL-Configuration-JsonElement.md 'UIBK.GraphSPARQL.Configuration.JsonElement')
## JsonElement.JsonTrace() Method
Captures the current JSON position.  
```csharp
protected UIBK.GraphSPARQL.Configuration.JsonTrace JsonTrace();
```
#### Returns
[JsonTrace](./UIBK-GraphSPARQL-Configuration-JsonTrace.md 'UIBK.GraphSPARQL.Configuration.JsonTrace')  
A captured [JsonTrace](./UIBK-GraphSPARQL-Configuration-JsonTrace.md 'UIBK.GraphSPARQL.Configuration.JsonTrace').  
#### Exceptions
[System.InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/System.InvalidOperationException 'System.InvalidOperationException')  
If the object is not currently deserialized.  
