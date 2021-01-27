#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Configuration](./UIBK-GraphSPARQL-Configuration.md 'UIBK.GraphSPARQL.Configuration').[JsonElement](./UIBK-GraphSPARQL-Configuration-JsonElement.md 'UIBK.GraphSPARQL.Configuration.JsonElement')
## JsonElement.JsonError(string) Method
Returns a new exception object describing the current JSON context.  
```csharp
protected internal Newtonsoft.Json.JsonSerializationException JsonError(string message);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Configuration-JsonElement-JsonError(string)-message'></a>
`message` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The exception's message.  
  
#### Returns
[Newtonsoft.Json.JsonSerializationException](https://docs.microsoft.com/en-us/dotnet/api/Newtonsoft.Json.JsonSerializationException 'Newtonsoft.Json.JsonSerializationException')  
A new [Newtonsoft.Json.JsonSerializationException](https://docs.microsoft.com/en-us/dotnet/api/Newtonsoft.Json.JsonSerializationException 'Newtonsoft.Json.JsonSerializationException') object.  
#### Exceptions
[System.InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/System.InvalidOperationException 'System.InvalidOperationException')  
If the method was called outside of an JSON object.  
