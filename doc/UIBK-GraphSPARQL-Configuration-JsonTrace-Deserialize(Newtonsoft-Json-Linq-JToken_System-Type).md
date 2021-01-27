#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Configuration](./UIBK-GraphSPARQL-Configuration.md 'UIBK.GraphSPARQL.Configuration').[JsonTrace](./UIBK-GraphSPARQL-Configuration-JsonTrace.md 'UIBK.GraphSPARQL.Configuration.JsonTrace')
## JsonTrace.Deserialize(Newtonsoft.Json.Linq.JToken, System.Type) Method
Deserializes a JSON token using the captured context.  
```csharp
public object? Deserialize(Newtonsoft.Json.Linq.JToken token, System.Type type);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Configuration-JsonTrace-Deserialize(Newtonsoft-Json-Linq-JToken_System-Type)-token'></a>
`token` [Newtonsoft.Json.Linq.JToken](https://docs.microsoft.com/en-us/dotnet/api/Newtonsoft.Json.Linq.JToken 'Newtonsoft.Json.Linq.JToken')  
The JSON token to deserialize.  
  
<a name='UIBK-GraphSPARQL-Configuration-JsonTrace-Deserialize(Newtonsoft-Json-Linq-JToken_System-Type)-type'></a>
`type` [System.Type](https://docs.microsoft.com/en-us/dotnet/api/System.Type 'System.Type')  
The type that should be returned.  
  
#### Returns
[System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object')  
The deserialized object.  
