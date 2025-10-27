# Denomica.JsonLD.Extensions

This library faciliates working with JSON-LD data in .NET applications.

## Version Highlights

### v1.0.2

- Fixed JSON-LD graph handling. Now when enumerating objects in a JSON-LD graph, the returned objects will inherit the `@context` of the graph if they do not have their own context defined.
- Added support for `schema.org` objects that define multiple values in the `@type` attribute.

### v1.0.1

- Added support for parsing arrays of JSON-LD objects.

### v1.0.0

- Finalized the extension methods for JSON-LD processing.

### v1.0.0-alpha.1

- Initial release of the library.