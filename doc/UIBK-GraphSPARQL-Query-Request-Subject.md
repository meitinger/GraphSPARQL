#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Query](./UIBK-GraphSPARQL-Query.md 'UIBK.GraphSPARQL.Query').[Request](./UIBK-GraphSPARQL-Query-Request.md 'UIBK.GraphSPARQL.Query.Request')
## Request.Subject Property
The subject's [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri') of the requested triple.  
```csharp
public UIBK.GraphSPARQL.Iri? Subject { get; }
```
#### Property Value
[Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')  
### Remarks
If [Inversed](./UIBK-GraphSPARQL-Query-Predicate-Inversed.md 'UIBK.GraphSPARQL.Query.Predicate.Inversed') of [Predicate](./UIBK-GraphSPARQL-Query-Request-Predicate.md 'UIBK.GraphSPARQL.Query.Request.Predicate') is `true`, this is the object's [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri').  
