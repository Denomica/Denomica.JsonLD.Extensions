# Denomica.JsonLD.Extensions
A library that facilitates working with JSON LD objects defined by [schema.org](https://schema.org).

## Getting Started

- Install [Denomica.JsonLD.Extensions](https://www.nuget.org/packages/Denomica.JsonLD.Extensions/) from Nuget.
- Add the using statement `using Denomica.JsonLD.Extensions;`
- Add the using statement `using System.Text.Json;`

### Getting JSON-LD Data From HTML

Often, JSON-LD data is embedded within HTML pages in `script` elements. One HTML page can contain multiple `script` elements with JSON-LD data.

The following code sample demonstates how you can enumerate through all of these JSON-LD elements.

``` C#
using Denomica.JsonLD.Extensions;
using System.Text.Json;

string html = GetHtml(); // Assume this method retrieves the HTML content
await foreach(JsonElement jsonLd in html.CreateHtmlDocument().GetJsonLDElementsAsync())
{
	// Process each JSON-LD element
}
```

### Extracting JSON-LD Objects

If you want to extract each JSON-LD object defined in the JSON-LD elements, you can follow the code sample below.

``` C#
using Denomica.JsonLD.Extensions;
using System.Text.Json;

string html = GetHtml(); // Assume this method retrieves the HTML content
await foreach(JsonElement jsonLdObject in html.CreateHtmlDocument().GetJsonLDObjectsAsync())
{
	// Process each JSON-LD object
}
```

### Extracting JSON-LD Objects of Specific Types

Finally, if you want to extract specific types of JSON-LD objects, you can use the overloaded `GetJsonLDObjectsAsync(string)` method that accepts a type name as a parameter.

``` C#
using Denomica.JsonLD.Extensions;
using System.Text.Json;

string html = GetHtml(); // Assume this method retrieves the HTML content
await foreach(JsonElement jsonLdObject in html.CreateHtmlDocument().GetJsonLDObjectsAsync("Product"))
{
	// Process each JSON-LD Product object.
}
```