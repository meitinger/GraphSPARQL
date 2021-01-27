#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Query](./UIBK-GraphSPARQL-Query.md 'UIBK.GraphSPARQL.Query').[Predicate](./UIBK-GraphSPARQL-Query-Predicate.md 'UIBK.GraphSPARQL.Query.Predicate')
## Predicate(UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource, UIBK.GraphSPARQL.Iri?, bool, UIBK.GraphSPARQL.Query.Filter?) Constructor
Creates a new [Predicate](./UIBK-GraphSPARQL-Query-Predicate.md 'UIBK.GraphSPARQL.Query.Predicate').  
```csharp
public Predicate(UIBK.GraphSPARQL.Iri iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource dataSource, UIBK.GraphSPARQL.Iri? graphIri, bool inversed=false, UIBK.GraphSPARQL.Query.Filter? filter=null);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Query-Predicate-Predicate(UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource_UIBK-GraphSPARQL-Iri-_bool_UIBK-GraphSPARQL-Query-Filter-)-iri'></a>
`iri` [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')  
The predicate's [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri').  
  
<a name='UIBK-GraphSPARQL-Query-Predicate-Predicate(UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource_UIBK-GraphSPARQL-Iri-_bool_UIBK-GraphSPARQL-Query-Filter-)-dataSource'></a>
`dataSource` [SparqlDataSource](./UIBK-GraphSPARQL-DataSource-SparqlDataSource.md 'UIBK.GraphSPARQL.DataSource.SparqlDataSource')  
The [SparqlDataSource](./UIBK-GraphSPARQL-DataSource-SparqlDataSource.md 'UIBK.GraphSPARQL.DataSource.SparqlDataSource') that contains the predicate.  
  
<a name='UIBK-GraphSPARQL-Query-Predicate-Predicate(UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource_UIBK-GraphSPARQL-Iri-_bool_UIBK-GraphSPARQL-Query-Filter-)-graphIri'></a>
`graphIri` [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')  
The graph [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri') that contains the predicate.  
  
<a name='UIBK-GraphSPARQL-Query-Predicate-Predicate(UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource_UIBK-GraphSPARQL-Iri-_bool_UIBK-GraphSPARQL-Query-Filter-)-inversed'></a>
`inversed` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the subject should be queried from a given object, `false` for the other way around.  
  
<a name='UIBK-GraphSPARQL-Query-Predicate-Predicate(UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource_UIBK-GraphSPARQL-Iri-_bool_UIBK-GraphSPARQL-Query-Filter-)-filter'></a>
`filter` [Filter](./UIBK-GraphSPARQL-Query-Filter.md 'UIBK.GraphSPARQL.Query.Filter')  
The [Filter](./UIBK-GraphSPARQL-Query-Predicate-Filter.md 'UIBK.GraphSPARQL.Query.Predicate.Filter') to use or `null` to query the predicate unfiltered.  
  
