#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.DataSource](./UIBK-GraphSPARQL-DataSource.md 'UIBK.GraphSPARQL.DataSource').[SparqlDataSourceCollection](./UIBK-GraphSPARQL-DataSource-SparqlDataSourceCollection.md 'UIBK.GraphSPARQL.DataSource.SparqlDataSourceCollection')
## SparqlDataSourceCollection.TryGetByName(string, UIBK.GraphSPARQL.DataSource.SparqlDataSource?) Method
Finds and returns a data source with a given name.  
```csharp
public bool TryGetByName(string name, out UIBK.GraphSPARQL.DataSource.SparqlDataSource? dataSource);
```
#### Parameters
<a name='UIBK-GraphSPARQL-DataSource-SparqlDataSourceCollection-TryGetByName(string_UIBK-GraphSPARQL-DataSource-SparqlDataSource-)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the data source to return.  
  
<a name='UIBK-GraphSPARQL-DataSource-SparqlDataSourceCollection-TryGetByName(string_UIBK-GraphSPARQL-DataSource-SparqlDataSource-)-dataSource'></a>
`dataSource` [SparqlDataSource](./UIBK-GraphSPARQL-DataSource-SparqlDataSource.md 'UIBK.GraphSPARQL.DataSource.SparqlDataSource')  
The output variable receiving the data source.  
  
#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
`true` if the data source was found, `false` otherwise.  
