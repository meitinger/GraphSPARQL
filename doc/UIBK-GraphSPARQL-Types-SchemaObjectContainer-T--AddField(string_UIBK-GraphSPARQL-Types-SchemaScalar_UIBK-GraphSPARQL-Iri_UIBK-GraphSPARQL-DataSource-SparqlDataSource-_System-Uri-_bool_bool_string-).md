#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaObjectContainer&lt;T&gt;](./UIBK-GraphSPARQL-Types-SchemaObjectContainer-T-.md 'UIBK.GraphSPARQL.Types.SchemaObjectContainer&lt;T&gt;')
## SchemaObjectContainer&lt;T&gt;.AddField(string, UIBK.GraphSPARQL.Types.SchemaScalar, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?) Method
Adds a new scalar field to this container.  
```csharp
public UIBK.GraphSPARQL.Types.SchemaContainer AddField(string name, UIBK.GraphSPARQL.Types.SchemaScalar scalar, UIBK.GraphSPARQL.Iri predicateIri, UIBK.GraphSPARQL.DataSource.SparqlDataSource? dataSource=null, System.Uri? graphUri=null, bool isArray=true, bool isRequired=false, string? filter=null);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of field.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-scalar'></a>
`scalar` [SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar')  
The [SchemaScalar](./UIBK-GraphSPARQL-Types-SchemaScalar.md 'UIBK.GraphSPARQL.Types.SchemaScalar') the field returns.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-predicateIri'></a>
`predicateIri` [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri')  
The underlying predicate's [Iri](./UIBK-GraphSPARQL-Iri.md 'UIBK.GraphSPARQL.Iri').  
  
<a name='UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-dataSource'></a>
`dataSource` [SparqlDataSource](./UIBK-GraphSPARQL-DataSource-SparqlDataSource.md 'UIBK.GraphSPARQL.DataSource.SparqlDataSource')  
The underlying data source or `null` to use the default one.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-graphUri'></a>
`graphUri` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')  
The underlying graph [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri') or `null` to use the default one.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-isArray'></a>
`isArray` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
If `true`, the field returns a list of scalars.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-isRequired'></a>
`isRequired` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
If `true`, the parent object is only returned if a scalar exists.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-filter'></a>
`filter` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
An optional filter that is passed to the data source.  
  
#### Returns
[SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer')  
The current [SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer').  
#### Exceptions
[System.ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentException 'System.ArgumentException')  
If another field with the same [name](#UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-name 'UIBK.GraphSPARQL.Types.SchemaObjectContainer&lt;T&gt;.AddField(string, UIBK.GraphSPARQL.Types.SchemaScalar, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?).name') already exists, either in this container or an associated one, [name](#UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-name 'UIBK.GraphSPARQL.Types.SchemaObjectContainer&lt;T&gt;.AddField(string, UIBK.GraphSPARQL.Types.SchemaScalar, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?).name') contains invalid characters or [filter](#UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-filter 'UIBK.GraphSPARQL.Types.SchemaObjectContainer&lt;T&gt;.AddField(string, UIBK.GraphSPARQL.Types.SchemaScalar, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?).filter') cannot be parsed.  
[System.InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/System.InvalidOperationException 'System.InvalidOperationException')  
If the [scalar](#UIBK-GraphSPARQL-Types-SchemaObjectContainer-T--AddField(string_UIBK-GraphSPARQL-Types-SchemaScalar_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-)-scalar 'UIBK.GraphSPARQL.Types.SchemaObjectContainer&lt;T&gt;.AddField(string, UIBK.GraphSPARQL.Types.SchemaScalar, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?).scalar') belongs to a different schema.  
