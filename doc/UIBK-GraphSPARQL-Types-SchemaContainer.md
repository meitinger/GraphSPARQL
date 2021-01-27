#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Types](./UIBK-GraphSPARQL-Types.md 'UIBK.GraphSPARQL.Types')
## SchemaContainer Class
Base class of all containers.  
```csharp
public abstract class SchemaContainer : UIBK.GraphSPARQL.Types.SchemaTypeElement
```
Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; [JsonElement](./UIBK-GraphSPARQL-Configuration-JsonElement.md 'UIBK.GraphSPARQL.Configuration.JsonElement') &#129106; [SchemaElement](./UIBK-GraphSPARQL-Types-SchemaElement.md 'UIBK.GraphSPARQL.Types.SchemaElement') &#129106; [SchemaTypeElement](./UIBK-GraphSPARQL-Types-SchemaTypeElement.md 'UIBK.GraphSPARQL.Types.SchemaTypeElement') &#129106; SchemaContainer  

Derived  
&#8627; [SchemaContainer&lt;T&gt;](./UIBK-GraphSPARQL-Types-SchemaContainer-T-.md 'UIBK.GraphSPARQL.Types.SchemaContainer&lt;T&gt;')  
### Properties
- [FieldCount](./UIBK-GraphSPARQL-Types-SchemaContainer-FieldCount.md 'UIBK.GraphSPARQL.Types.SchemaContainer.FieldCount')
- [Fields](./UIBK-GraphSPARQL-Types-SchemaContainer-Fields.md 'UIBK.GraphSPARQL.Types.SchemaContainer.Fields')
- [LinkedContainers](./UIBK-GraphSPARQL-Types-SchemaContainer-LinkedContainers.md 'UIBK.GraphSPARQL.Types.SchemaContainer.LinkedContainers')
### Methods
- [AddField(string, UIBK.GraphSPARQL.Types.SchemaInterface, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?)](./UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaInterface_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-).md 'UIBK.GraphSPARQL.Types.SchemaContainer.AddField(string, UIBK.GraphSPARQL.Types.SchemaInterface, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?)')
- [AddField(string, UIBK.GraphSPARQL.Types.SchemaType, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?)](./UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaType_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-).md 'UIBK.GraphSPARQL.Types.SchemaContainer.AddField(string, UIBK.GraphSPARQL.Types.SchemaType, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?)')
- [AddField(string, UIBK.GraphSPARQL.Types.SchemaUnion, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?)](./UIBK-GraphSPARQL-Types-SchemaContainer-AddField(string_UIBK-GraphSPARQL-Types-SchemaUnion_UIBK-GraphSPARQL-Iri_UIBK-GraphSPARQL-DataSource-SparqlDataSource-_System-Uri-_bool_bool_string-).md 'UIBK.GraphSPARQL.Types.SchemaContainer.AddField(string, UIBK.GraphSPARQL.Types.SchemaUnion, UIBK.GraphSPARQL.Iri, UIBK.GraphSPARQL.DataSource.SparqlDataSource?, System.Uri?, bool, bool, string?)')
- [ContainsField(string)](./UIBK-GraphSPARQL-Types-SchemaContainer-ContainsField(string).md 'UIBK.GraphSPARQL.Types.SchemaContainer.ContainsField(string)')
- [JsonInitialize()](./UIBK-GraphSPARQL-Types-SchemaContainer-JsonInitialize().md 'UIBK.GraphSPARQL.Types.SchemaContainer.JsonInitialize()')
- [TryGetField(string, UIBK.GraphSPARQL.Types.SchemaField?)](./UIBK-GraphSPARQL-Types-SchemaContainer-TryGetField(string_UIBK-GraphSPARQL-Types-SchemaField-).md 'UIBK.GraphSPARQL.Types.SchemaContainer.TryGetField(string, UIBK.GraphSPARQL.Types.SchemaField?)')
