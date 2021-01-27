#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Query](./UIBK-GraphSPARQL-Query.md 'UIBK.GraphSPARQL.Query').[Request](./UIBK-GraphSPARQL-Query-Request.md 'UIBK.GraphSPARQL.Query.Request')
## Request(UIBK.GraphSPARQL.Iri?, UIBK.GraphSPARQL.Query.Predicate, bool) Constructor
Creates a new [Request](./UIBK-GraphSPARQL-Query-Request.md 'UIBK.GraphSPARQL.Query.Request').  
```csharp
public Request(UIBK.GraphSPARQL.Iri? subject, UIBK.GraphSPARQL.Query.Predicate predicate, bool includeTypeInfo);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Query-Request-Request(UIBK-GraphSPARQL-Iri-_UIBK-GraphSPARQL-Query-Predicate_bool)-subject'></a>
`subject` [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')  
The subject's [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri') of the requested triple.  
  
<a name='UIBK-GraphSPARQL-Query-Request-Request(UIBK-GraphSPARQL-Iri-_UIBK-GraphSPARQL-Query-Predicate_bool)-predicate'></a>
`predicate` [Predicate](./UIBK-GraphSPARQL-Query-Predicate.md 'UIBK.GraphSPARQL.Query.Predicate')  
The [Predicate](./UIBK-GraphSPARQL-Query-Predicate.md 'UIBK.GraphSPARQL.Query.Predicate') which can also include a [Filter](./UIBK-GraphSPARQL-Query-Filter.md 'UIBK.GraphSPARQL.Query.Filter').  
  
<a name='UIBK-GraphSPARQL-Query-Request-Request(UIBK-GraphSPARQL-Iri-_UIBK-GraphSPARQL-Query-Predicate_bool)-includeTypeInfo'></a>
`includeTypeInfo` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
Indicates whether the object's type [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')s should be returned.  
  
