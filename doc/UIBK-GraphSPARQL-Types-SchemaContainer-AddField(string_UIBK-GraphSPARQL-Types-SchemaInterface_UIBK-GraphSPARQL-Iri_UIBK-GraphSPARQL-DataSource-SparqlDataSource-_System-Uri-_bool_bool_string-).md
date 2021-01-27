#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer')
## SchemaContainer.AddField(string, UIBK.GraphSPARQL.Types.SchemaInterface, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?) Method
Creates and adds a new [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField') returning a [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface') to this [SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer').  
```csharp
public UIBK.GraphSPARQL.Types.SchemaContainer AddField(string name, UIBK.GraphSPARQL.Types.SchemaInterface @interface, UIBK.GraphSPARQL.Iri predicateIri, UIBK.GraphSPARQL.DataSource.SparqlDataSource? dataSource=null, System.Uri? graphUri=null, bool isArray=true, bool isRequired=false, string? filter=null);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of the [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField').  
  
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-interface'></a>
`interface` [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface')  
The [SchemaInterface](./UIBK-GraphSPARQL-Types-SchemaInterface.md 'UIBK.GraphSPARQL.Types.SchemaInterface') the field returns.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-predicateIri'></a>
`predicateIri` [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')  
The underlying predicate's [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri').  
  
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-dataSource'></a>
`dataSource` [SparqlDataSource](./UIBK-GraphSPARQL-DataSource-SparqlDataSource.md 'UIBK.GraphSPARQL.DataSource.SparqlDataSource')  
The underlying [SparqlDataSource](./UIBK-GraphSPARQL-DataSource-SparqlDataSource.md 'UIBK.GraphSPARQL.DataSource.SparqlDataSource') or `null` to use the default one.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-graphUri'></a>
`graphUri` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')  
The underlying graph [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri') or `null` to use the default one.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-isArray'></a>
`isArray` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
If `true`, the field returns a list of objects.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-isRequired'></a>
`isRequired` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
If `true`, the parent object is only returned if an object exists.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-filter'></a>
`filter` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
An optional filter [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String') that is passed to the data source.  
  
#### Returns
[SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer')  
The current [SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer').  
#### Exceptions
[System.ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentException 'System.ArgumentException')  
If another [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField') with the same [name](#UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-name 'UIBK.GraphSPARQL.Types.SchemaContainer.AddField(string, UIBK.GraphSPARQL.Types.SchemaInterface, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?).name') already exists, either in this container or an associated one, [name](#UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-name 'UIBK.GraphSPARQL.Types.SchemaContainer.AddField(string, UIBK.GraphSPARQL.Types.SchemaInterface, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?).name') contains invalid characters or [filter](#UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-filter 'UIBK.GraphSPARQL.Types.SchemaContainer.AddField(string, UIBK.GraphSPARQL.Types.SchemaInterface, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?).filter') cannot be parsed.  
[System.InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/System.InvalidOperationException 'System.InvalidOperationException')  
If the [interface](#UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-interface 'UIBK.GraphSPARQL.Types.SchemaContainer.AddField(string, UIBK.GraphSPARQL.Types.SchemaInterface, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?).interface') belongs to a different schema.  
