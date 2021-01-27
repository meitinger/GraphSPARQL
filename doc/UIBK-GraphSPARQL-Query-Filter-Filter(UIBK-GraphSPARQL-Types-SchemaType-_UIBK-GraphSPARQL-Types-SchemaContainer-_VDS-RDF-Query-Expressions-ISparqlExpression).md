#### [GraphSPARQL](./index.md 'index')
### [UIBK.GraphSPARQL.Query](./UIBK-GraphSPARQL-Query.md 'UIBK.GraphSPARQL.Query').[Filter](./UIBK-GraphSPARQL-Query-Filter.md 'UIBK.GraphSPARQL.Query.Filter')
## Filter(UIBK.GraphSPARQL.Types.SchemaType?, UIBK.GraphSPARQL.Types.SchemaContainer?, VDS.RDF.Query.Expressions.ISparqlExpression) Constructor
Create a new [Filter](./UIBK-GraphSPARQL-Query-Filter.md 'UIBK.GraphSPARQL.Query.Filter') instance.  
```csharp
public Filter(UIBK.GraphSPARQL.Types.SchemaType? parentType, UIBK.GraphSPARQL.Types.SchemaContainer? container, VDS.RDF.Query.Expressions.ISparqlExpression expression);
```
#### Parameters
<a name='UIBK-GraphSPARQL-Query-Filter-Filter(UIBK-GraphSPARQL-Types-SchemaType-_UIBK-GraphSPARQL-Types-SchemaContainer-_VDS-RDF-Query-Expressions-ISparqlExpression)-parentType'></a>
`parentType` [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType')  
The [SchemaType](./UIBK-GraphSPARQL-Types-SchemaType.md 'UIBK.GraphSPARQL.Types.SchemaType') of the [Instance](./UIBK-GraphSPARQL-Query-Instance.md 'UIBK.GraphSPARQL.Query.Instance') that this [Filter](./UIBK-GraphSPARQL-Query-Filter.md 'UIBK.GraphSPARQL.Query.Filter') is called on.  
  
<a name='UIBK-GraphSPARQL-Query-Filter-Filter(UIBK-GraphSPARQL-Types-SchemaType-_UIBK-GraphSPARQL-Types-SchemaContainer-_VDS-RDF-Query-Expressions-ISparqlExpression)-container'></a>
`container` [SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer')  
The [SchemaContainer](./UIBK-GraphSPARQL-Types-SchemaContainer.md 'UIBK.GraphSPARQL.Types.SchemaContainer') that defines available [SchemaField](./UIBK-GraphSPARQL-Types-SchemaField.md 'UIBK.GraphSPARQL.Types.SchemaField')s.  
  
<a name='UIBK-GraphSPARQL-Query-Filter-Filter(UIBK-GraphSPARQL-Types-SchemaType-_UIBK-GraphSPARQL-Types-SchemaContainer-_VDS-RDF-Query-Expressions-ISparqlExpression)-expression'></a>
`expression` [VDS.RDF.Query.Expressions.ISparqlExpression](https://docs.microsoft.com/en-us/dotnet/api/VDS.RDF.Query.Expressions.ISparqlExpression 'VDS.RDF.Query.Expressions.ISparqlExpression')  
The filter [VDS.RDF.Query.Expressions.ISparqlExpression](https://docs.microsoft.com/en-us/dotnet/api/VDS.RDF.Query.Expressions.ISparqlExpression 'VDS.RDF.Query.Expressions.ISparqlExpression').  
  
