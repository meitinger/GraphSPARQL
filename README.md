GraphSPARQL
===========

_GraphSPARQL_ is a library that allows you to run a _GraphQL_ queries like
```graphql
query {
  scientist (id: "http://dbpedia.org/resource/Albert_Einstein") {
    spouse (filter: "bound(?child_label) && regex(?child_label,'^Lieserl')") {
      label (filter: "lang(?_)='en'")
      birthDate
      child (limit: 2) {
        _id
      }
    }
  }
}
```
against one or more _SPARQL_ endpoints (e.g.
[DBpedia](https://dbpedia.org/sparql)) returning the following _JSON_
```json
{
  "data": {
    "scientist": [
      {
        "spouse": [
          {
            "label": [{ "value": "Mileva Marić", "language": "en" }],
            "birthDate": [{ "year": 1875, "month": 12, "day": 19, "kind": 0 }],
            "child": [
              { "_id": "http://dbpedia.org/resource/Hans_Albert_Einstein" },
              { "_id": "http://dbpedia.org/resource/Lieserl_Einstein" }
            ]
          }
        ]
      }
    ]
  }
}
```
without having to know almost any _SPARQL_ (except for filtering).

It supports harvesting schemas from _RDF(S)_, _OWL_, _GraphQL_ and _JSON_.
Each field can belong to a different SPARQL endpoint, even a different named
graph within that endpoint. In other words, it fully supports _linked data_.

There is also support for _SPARQL_ updates through _GraphQL_ mutations.
For example, the following mutation (falsely) attributes another child to
Einstein:
```graphql
mutation {
  createPerson (id: "http://fakepedia.com/Ente_Einstein", template: {
    label: ["'Ente Einstein'@de", "'Canard Einstein'@en"]
    birthDate: { year: 1900, month: 2, day: 8 }
  }) {_id}
  updateScientist (filter: "?label='Albert Einstein'@en") {
    child (add: "http://fakepedia.com/Ente_Einstein") {_id}
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
