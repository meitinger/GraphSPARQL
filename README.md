GraphSPARQL
===========

_GraphSPARQL_ is a library that allows you to run a _GraphQL_ query like
```graphql
query {
  scientist(filter: "?label='Albert Einstein'@en") {
    spouse(filter: "?label='Mileva Marić'@en") {
      child(filter: "?height > 1.7") {
        label(filter: "lang(?_)='en'")
        height
      }
    }
  }
}
```
against one or more _SPARQL_ endpoints like
[DBpedia](https://dbpedia.org/sparql) returning _JSON_
```json
{
  "data": {
    "scientist": [
      {
        "spouse": [
          {
            "child": [
              {
                "label": [
                  {
                    "value": "Hans Albert Einstein",
                    "language": "en"
                  }
                ],
                "height": [
                  1.7272
                ]
              }
            ]
          }
        ]
      }
    ]
  }
}
```
without knowing almost any _SPARQL_ (except for filtering).

It supports harvesting schemas from _RDF(S)_, _OWL_, _GraphQL_ and _JSON_.
Each field can belong to a different SPARQL endpoint, even a different named
graph within that endpoint.

It also support _SPARQL_ updates through _GraphQL_ mutations.
For example to update the height from the example above you could run
```graphql
mutation {
  updateScientist(filter: "?label='Albert Einstein'@en") {
    spouse(filter: "?label='Mileva Marić'@en") {
      child(filter: "?height > 1.7") {
        label(filter: "lang(?_)='en'")
        height(set: 1.73)
      }
    }
  }
}
```

Configuration
-------------
The fastest way to get started is by looking at `.\example\config.json`, which
is loaded by default, and pulls in most of the _DBpedia_ schema through the
_RDF_ harvester.

Settings for the harvester are per namespace and documented in the
`RdfNamespace` class, located in the `.\src\Types\Providers\Rdf.cs` file.

More configuration examples are soon to come.

Usage
-----
Downloading the source allows you to run _GraphSPARQL_ in either _IIS Express_
or _Kestrel_. By running the debugger in _Visual Studio_ an instance of
[GraphiQL](https://github.com/graphql/graphiql) opens in your browser. Give it
a couple of seconds to load the schema and then start querying.

**Note**: Due to the size of the schema, ordinary introspection is disabled.
Instead use the special field `_fields` to get a list of all available fields.
This also works for the root objects `query` and `mutation`.

To get the _IRI_ of an object, use the special field `_id`. In filters, you
can refer to either the _IRI_ or the current value with `?_`, to the parent
with `?__parent`, and to any other field with `?fieldName`. Walking down the
class hierarchy in filters can be achieved by concatenating field names with
an underscore, e.g. `?childField_grandChildField`.

The following three prefixes are available in filters:

- [`xsd`](http://www.w3.org/2001/XMLSchema)
- [`rdf`](http://www.w3.org/1999/02/22-rdf-syntax-ns#)
- [`rdfs`](http://www.w3.org/2000/01/rdf-schema#)
