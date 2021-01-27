#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Configuration](./UIBK-GraphSPARQL-Configuration.md 'UIBK.GraphSPARQL.Configuration').[JsonElement](./UIBK-GraphSPARQL-Configuration-JsonElement.md 'UIBK.GraphSPARQL.Configuration.JsonElement')
## JsonElement.JsonTrace&lt;T&gt;(System.Nullable&lt;T&gt;) Method
Captures the current JSON position together with a value.  
```csharp
protected UIBK.GraphSPARQL.Configuration.JsonTrace<T>? JsonTrace<T>(System.Nullable<T> value)
    where T : struct;
```
#### Type parameters
<a name='UIBK-GraphSPARQL-Configuration-JsonElement-JsonTrace-T-(System-Nullable-T-)-T'></a>
`T`  
The type of the value to capture.  
  
#### Parameters
<a name='UIBK-GraphSPARQL-Configuration-JsonElement-JsonTrace-T-(System-Nullable-T-)-value'></a>
`value` [System.Nullable&lt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')[T](#UIBK-GraphSPARQL-Configuration-JsonElement-JsonTrace-T-(System-Nullable-T-)-T 'UIBK.GraphSPARQL.Configuration.JsonElement.JsonTrace&lt;T&gt;(System.Nullable&lt;T&gt;).T')[&gt;](https://docs.microsoft.com/en-us/dotnet/api/System.Nullable-1 'System.Nullable`1')  
The value to capture.  
  
#### Returns
[UIBK.GraphSPARQL.Configuration.JsonTrace&lt;](./UIBK-GraphSPARQL-Configuration-JsonTrace-T-.md 'UIBK.GraphSPARQL.Configuration.JsonTrace&lt;T&gt;')[T](#UIBK-GraphSPARQL-Configuration-JsonElement-JsonTrace-T-(System-Nullable-T-)-T 'UIBK.GraphSPARQL.Configuration.JsonElement.JsonTrace&lt;T&gt;(System.Nullable&lt;T&gt;).T')[&gt;](./UIBK-GraphSPARQL-Configuration-JsonTrace-T-.md 'UIBK.GraphSPARQL.Configuration.JsonTrace&lt;T&gt;')  
A [JsonTrace&lt;T&gt;](./UIBK-GraphSPARQL-Configuration-JsonTrace-T-.md 'UIBK.GraphSPARQL.Configuration.JsonTrace&lt;T&gt;') containing the value and captured JSON position or `null` if [value](#UIBK-GraphSPARQL-Configuration-JsonElement-JsonTrace-T-(System-Nullable-T-)-value 'UIBK.GraphSPARQL.Configuration.JsonElement.JsonTrace&lt;T&gt;(System.Nullable&lt;T&gt;).value') is `null`.  
#### Exceptions
[System.InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/System.InvalidOperationException 'System.InvalidOperationException')  
If the object is not currently deserialized.  
