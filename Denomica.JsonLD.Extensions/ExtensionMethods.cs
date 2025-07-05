using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Denomica.JsonLD.Extensions
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Creates an <see cref="HtmlDocument"/> instance by parsing the specified HTML string.
        /// </summary>
        /// <remarks>This method uses the <see cref="HtmlDocument.LoadHtml(string)"/> method to parse the
        /// provided HTML string. Ensure the input string contains valid HTML content to avoid unexpected
        /// behavior.</remarks>
        /// <param name="html">The HTML content to parse. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>An <see cref="HtmlDocument"/> representing the parsed HTML content.</returns>
        public static HtmlDocument CreateHtmlDocument(this string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }

        /// <summary>
        /// Extracts JSON-LD elements from the specified HTML document.
        /// </summary>
        /// <remarks>This method searches the provided HTML document for <c>script</c> elements with a
        /// <c>type</c> attribute of <c>application/ld+json</c>. The contents of each matching <c>script</c> element are
        /// parsed as JSON. If parsing fails for a particular element, it is skipped.  The method returns an
        /// asynchronous stream, allowing the caller to process the JSON-LD elements as they are discovered. Use
        /// <c>await foreach</c> to enumerate the results.</remarks>
        /// <param name="document">The HTML document to search for JSON-LD script elements.</param>
        /// <returns>An asynchronous stream of <see cref="JsonElement"/> objects representing the parsed JSON-LD data. Each
        /// element corresponds to a <c>script</c> tag with a <c>type</c> attribute of <c>application/ld+json</c>.</returns>
        public static async IAsyncEnumerable<JsonElement> GetJsonLDElementsAsync(this HtmlDocument document)
        {
            foreach (var htmlNode in document.QuerySelectorAll("script[type='application/ld+json']"))
            {
                JsonElement? elem = null;
                try
                {
                    using (var strm = new MemoryStream())
                    {
                        using (var writer = new StreamWriter(strm))
                        {
                            await writer.WriteAsync(htmlNode.InnerText);
                            await writer.FlushAsync();
                            strm.Position = 0;

                            var doc = await JsonDocument.ParseAsync(strm);
                            elem = doc.RootElement;
                        }
                    }
                }
                catch { }

                if (elem.HasValue)
                {
                    yield return elem.Value;
                }
            }
        }

        /// <summary>
        /// Asynchronously retrieves JSON-LD objects from the specified HTML document.
        /// </summary>
        /// <remarks>This method processes the HTML document to locate and parse JSON-LD elements,
        /// returning each parsed object as a <see cref="JsonElement"/>. The method uses asynchronous enumeration to
        /// efficiently handle large documents or multiple JSON-LD elements.</remarks>
        /// <param name="document">The HTML document to search for JSON-LD objects. Cannot be null.</param>
        /// <returns>An asynchronous stream of <see cref="JsonElement"/> instances representing the JSON-LD objects found within
        /// the document. The stream will be empty if no JSON-LD objects are present.</returns>
        public static async IAsyncEnumerable<JsonElement> GetJsonLDObjectsAsync(this HtmlDocument document)
        {
            await foreach (var elem in document.GetJsonLDElementsAsync())
            {
                await foreach (var obj in elem.GetJsonLDObjectsAsync())
                {
                    yield return obj;
                }
            }

            yield break;
        }

        /// <summary>
        /// Asynchronously retrieves JSON-LD objects of the specified type from the given HTML document.
        /// </summary>
        /// <remarks>This method filters JSON-LD objects within the HTML document based on their type. The
        /// type comparison is performed using schema.org conventions. If no objects match the specified type, the
        /// returned stream will be empty.</remarks>
        /// <param name="document">The HTML document to search for JSON-LD objects.</param>
        /// <param name="type">The type of JSON-LD objects to filter by, typically corresponding to a schema.org type.</param>
        /// <returns>An asynchronous stream of <see cref="JsonElement"/> objects that match the specified type.</returns>
        public static async IAsyncEnumerable<JsonElement> GetJsonLDObjectsAsync(this HtmlDocument document, string type)
        {
            await foreach (var obj in document.GetJsonLDObjectsAsync())
            {
                if (obj.IsSchemaOrgObjectType(type))
                {
                    yield return obj;
                }
            }
        }

        /// <summary>
        /// Asynchronously retrieves JSON-LD objects from the specified <see cref="JsonElement"/>.
        /// </summary>
        /// <remarks>This method processes the input <see cref="JsonElement"/> to identify and yield
        /// JSON-LD objects. If the element contains a `@graph` property, the method enumerates its array and
        /// recursively retrieves JSON-LD objects from each item. If the element has a `@type` property with a string
        /// value, the element itself is yielded as a JSON-LD object.</remarks>
        /// <param name="element">The <see cref="JsonElement"/> to search for JSON-LD objects.</param>
        /// <returns>An asynchronous stream of <see cref="JsonElement"/> instances representing JSON-LD objects.</returns>
        public static async IAsyncEnumerable<JsonElement> GetJsonLDObjectsAsync(this JsonElement element)
        {
            if (element.TryGetSchemaOrgGraphArray(out JsonElement graph))
            {
                foreach (var item in graph.EnumerateArray())
                {
                    await foreach (var obj in item.GetJsonLDObjectsAsync())
                    {
                        yield return obj;
                    }
                }
            }
            else if (element.TryGetProperty("@type", out var objType) && objType.ValueKind == JsonValueKind.String)
            {
                yield return element;
            }
        }

        /// <summary>
        /// Asynchronously retrieves JSON-LD objects of the specified type from the given <see cref="JsonElement"/>.
        /// </summary>
        /// <remarks>This method filters JSON-LD objects based on their "@type" property. It uses
        /// asynchronous enumeration to allow processing large JSON structures efficiently without loading all objects
        /// into memory.</remarks>
        /// <param name="element">The <see cref="JsonElement"/> to search for JSON-LD objects.</param>
        /// <param name="type">The type of JSON-LD objects to filter by. This should match the value of the "@type" property in the JSON-LD
        /// object.</param>
        /// <returns>An asynchronous stream of <see cref="JsonElement"/> instances representing the JSON-LD objects of the
        /// specified type.</returns>
        public static async IAsyncEnumerable<JsonElement> GetJsonLDObjectsAsync(this JsonElement element, string type)
        {
            await foreach (var obj in element.GetJsonLDObjectsAsync())
            {
                if (obj.IsSchemaOrgObjectType(type))
                {
                    yield return obj;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="JsonElement"/> contains a property with the given name.
        /// </summary>
        /// <param name="element">The <see cref="JsonElement"/> to inspect.</param>
        /// <param name="propertyName">The name of the property to check for. This value is case-sensitive.</param>
        /// <returns><see langword="true"/> if the <see cref="JsonElement"/> contains a property with the specified name; 
        /// otherwise, <see langword="false"/>.</returns>
        public static bool HasProperty(this JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out _);
        }

        /// <summary>
        /// Determines whether the specified <see cref="JsonElement"/> represents a Schema.org element.
        /// </summary>
        /// <remarks>This method checks for the presence of the "@context" property in the JSON element
        /// and verifies  that its value matches the Schema.org context URI.</remarks>
        /// <param name="element">The <see cref="JsonElement"/> to evaluate.</param>
        /// <returns><see langword="true"/> if the <paramref name="element"/> contains a property named "@context"  with the
        /// value "https://schema.org"; otherwise, <see langword="false"/>.</returns>
        public static bool IsSchemaOrgElement(this JsonElement element)
        {
            if(element.HasProperty("@context") && element.TryGetProperty("@context", out var context) && context.ValueKind == JsonValueKind.String)
            {
                var text = context.GetString()?.ToLower();
                return text == "http://schema.org" || text == "http://schema.org/" || text == "https://schema.org" || text == "https://schema.org/";
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="JsonElement"/> represents a Schema.org object of the given type.
        /// </summary>
        /// <remarks>This method checks if the <paramref name="element"/> contains a Schema.org type
        /// definition and compares it to the specified type. The comparison is case-insensitive.</remarks>
        /// <param name="element">The <see cref="JsonElement"/> to evaluate. Must represent a valid Schema.org element.</param>
        /// <param name="type">The type to check against, as a case-insensitive string.</param>
        /// <returns><see langword="true"/> if the <paramref name="element"/> is a Schema.org object and its type matches the
        /// specified <paramref name="type"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsSchemaOrgObjectType(this JsonElement element, string type)
        {
            if(element.IsSchemaOrgElement() && element.TryGetProperty("@type", out var typeObj) && typeObj.ValueKind == JsonValueKind.String)
            {
                var text = typeObj.GetString();
                return string.Equals(text, type, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to a <see cref="Task{TResult}"/> that represents a list of
        /// elements.
        /// </summary>
        /// <remarks>This method enumerates all elements in the <paramref name="enumerable"/>
        /// asynchronously and adds them to a list. It is useful for scenarios where you need to materialize an <see
        /// cref="IAsyncEnumerable{T}"/> into a concrete collection.</remarks>
        /// <typeparam name="T">The type of elements in the asynchronous enumerable.</typeparam>
        /// <param name="enumerable">The asynchronous enumerable to convert to a list.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of elements from the
        /// asynchronous enumerable.</returns>
        public static async Task<IList<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            var result = new List<T>();

            await foreach (var item in enumerable)
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Attempts to retrieve the "@graph" property as a JSON array from a Schema.org-compliant <see
        /// cref="JsonElement"/>.
        /// </summary>
        /// <remarks>This method checks whether the provided <paramref name="element"/> is a
        /// Schema.org-compliant JSON object and contains a "@graph" property with a value of type <see
        /// cref="JsonValueKind.Array"/>. If these conditions are met, the method sets <paramref name="graphArray"/> to
        /// the "@graph" property and returns <see langword="true"/>. The <paramref name="graphArray"/> represents a JSON array.</remarks>
        /// <param name="element">The <see cref="JsonElement"/> to inspect. Must represent a Schema.org-compliant JSON object.</param>
        /// <param name="graphArray">When this method returns <see langword="true"/>, contains the "@graph" property as a <see
        /// cref="JsonElement"/> with a <see cref="JsonValueKind.Array"/> value. If the property is not found or is not
        /// an array, this will be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the "@graph" property exists and is a JSON array; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool TryGetSchemaOrgGraphArray(this JsonElement element, out JsonElement graphArray)
        {
            graphArray = default;
            if(element.IsSchemaOrgElement() && element.TryGetProperty("@graph", out var graph) && graph.ValueKind == JsonValueKind.Array)
            {
                graphArray = graph;
                return true;
            }

            return false;
        }

    }
}
