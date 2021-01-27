#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.DataSource](./UIBK-GraphSPARQL-DataSource.md 'UIBK.GraphSPARQL.DataSource').[DataSourceContext](./UIBK-GraphSPARQL-DataSource-DataSourceContext.md 'UIBK.GraphSPARQL.DataSource.DataSourceContext')
## DataSourceContext.InsertData(UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.Query.Predicate, VDS.RDF.INode) Method
Inserts a given triple into the [SparqlDataSource](./UIBK-GraphSPARQL-DataSource-SparqlDataSource.md 'UIBK.GraphSPARQL.DataSource.SparqlDataSource').  
```csharp
public void InsertData(UIBK.GraphSPARQL.Iri subject, UIBK.GraphSPARQL.Query.Predicate predicate, VDS.RDF.INode @object);
```
#### Parameters
<a name='UIBK-GraphSPARQL-DataSource-DataSourceContext-InsertData(UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-Query-Predicate_VDS-RDF-INode)-subject'></a>
`subject` [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')  
The subject [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri').  
  
<a name='UIBK-GraphSPARQL-DataSource-DataSourceContext-InsertData(UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-Query-Predicate_VDS-RDF-INode)-predicate'></a>
`predicate` [Predicate](./UIBK-GraphSPARQL-Query-Predicate.md 'UIBK.GraphSPARQL.Query.Predicate')  
The [Predicate](./UIBK-GraphSPARQL-Query-Predicate.md 'UIBK.GraphSPARQL.Query.Predicate') specifying the predicate [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri'), graph [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri') and [SparqlDataSource](./UIBK-GraphSPARQL-DataSource-SparqlDataSource.md 'UIBK.GraphSPARQL.DataSource.SparqlDataSource').  
  
<a name='UIBK-GraphSPARQL-DataSource-DataSourceContext-InsertData(UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-Query-Predicate_VDS-RDF-INode)-object'></a>
`object` [VDS.RDF.INode](https://docs.microsoft.com/en-us/dotnet/api/VDS.RDF.INode 'VDS.RDF.INode')  
The [VDS.RDF.INode](https://docs.microsoft.com/en-us/dotnet/api/VDS.RDF.INode 'VDS.RDF.INode') to insert.  
  
