#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types').[SchemaMutation](./UIBK-GraphSPARQL-Types-SchemaMutation.md 'UIBK.GraphSPARQL.Types.SchemaMutation')
## SchemaMutation.AddCreateField(string, UIBK.GraphSPARQL.Types.SchemaType, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, string?) Method
Adds a new field that allows to create a new instance of a [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType').  
```csharp
public UIBK.GraphSPARQL.Types.SchemaMutation AddCreateField(string name, UIBK.GraphSPARQL.Types.SchemaType type, UIBK.GraphSPARQL.DataSource.SparqlDataSource? dataSource=null, System.Uri? graphUri=null, bool isArray=true, string? filter=null);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Types-SchemaMutation-AddCreateField(string_UIBK-GraphSPARQL-Types-SchemaType_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_string-)-name'></a>
`name` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
The name of [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField').  
  
<a name='UIBK-GraphSPARQL-Types-SchemaMutation-AddCreateField(string_UIBK-GraphSPARQL-Types-SchemaType_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_string-)-type'></a>
`type` [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType')  
The [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType') to create.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaMutation-AddCreateField(string_UIBK-GraphSPARQL-Types-SchemaType_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_string-)-dataSource'></a>
`dataSource` [SparqlDataSource](./UIBK-GraphSPARQL-DataSource-SparqlDataSource.md 'UIBK.GraphSPARQL.DataSource.SparqlDataSource')  
The underlying [SparqlDataSource](./UIBK-GraphSPARQL-DataSource-SparqlDataSource.md 'UIBK.GraphSPARQL.DataSource.SparqlDataSource') or `null` to use the default one.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaMutation-AddCreateField(string_UIBK-GraphSPARQL-Types-SchemaType_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_string-)-graphUri'></a>
`graphUri` [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri')  
The underlying graph [System.Uri](https://docs.microsoft.com/en-us/dotnet/api/System.Uri 'System.Uri') or `null` to use the default one.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaMutation-AddCreateField(string_UIBK-GraphSPARQL-Types-SchemaType_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_string-)-isArray'></a>
`isArray` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')  
If `true`, multiple instances can be created at once.  
  
<a name='UIBK-GraphSPARQL-Types-SchemaMutation-AddCreateField(string_UIBK-GraphSPARQL-Types-SchemaType_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_string-)-filter'></a>
`filter` [System.String](https://docs.microsoft.com/en-us/dotnet/api/System.String 'System.String')  
An optional filter that used to check the instance before creation.  
  
#### Returns
[SchemaMutation](./UIBK-GraphSPARQL-Types-SchemaMutation.md 'UIBK.GraphSPARQL.Types.SchemaMutation')  
The current [SchemaMutation](./UIBK-GraphSPARQL-Types-SchemaMutation.md 'UIBK.GraphSPARQL.Types.SchemaMutation').  
#### Exceptions
[System.ArgumentException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentException 'System.ArgumentException')  
If another field with the same [name](#UIBK-GraphSPARQL-Types-SchemaMutation-AddCreateField(string_UIBK-GraphSPARQL-Types-SchemaType_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_string-)-name 'UIBK.GraphSPARQL.Types.SchemaMutation.AddCreateField(string, UIBK.GraphSPARQL.Types.SchemaType, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, string?).name') already exists, [name](#UIBK-GraphSPARQL-Types-SchemaMutation-AddCreateField(string_UIBK-GraphSPARQL-Types-SchemaType_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_string-)-name 'UIBK.GraphSPARQL.Types.SchemaMutation.AddCreateField(string, UIBK.GraphSPARQL.Types.SchemaType, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, string?).name') contains invalid characters or [filter](#UIBK-GraphSPARQL-Types-SchemaMutation-AddCreateField(string_UIBK-GraphSPARQL-Types-SchemaType_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_string-)-filter 'UIBK.GraphSPARQL.Types.SchemaMutation.AddCreateField(string, UIBK.GraphSPARQL.Types.SchemaType, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, string?).filter') cannot be parsed.  
[System.InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/System.InvalidOperationException 'System.InvalidOperationException')  
If the [type](#UIBK-GraphSPARQL-Types-SchemaMutation-AddCreateField(string_UIBK-GraphSPARQL-Types-SchemaType_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_string-)-type 'UIBK.GraphSPARQL.Types.SchemaMutation.AddCreateField(string, UIBK.GraphSPARQL.Types.SchemaType, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, string?).type') belongs to a different schema.  
