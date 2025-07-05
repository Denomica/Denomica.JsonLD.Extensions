using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Denomica.JsonLD.Extensions.Tests
{
    internal static class Utils
    {

        public static HtmlDocument GetHtmlDocument(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc;
        }

        public static JsonDocument GetJsonDocument(string json)
        {
            var doc = JsonDocument.Parse(json);
            return doc;
        }

    }
}
