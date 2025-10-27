using Denomica.JsonLD.Extensions;
using Denomica.Text.Json;
using System.Text.Json;

namespace Denomica.JsonLD.Extensions.Tests
{
    [TestClass]
    public sealed class ParsingTests
    {
        [TestMethod]
        public async Task Parse01()
        {
            var doc = Utils.GetHtmlDocument(Properties.Resources.HTMLPage001);
            int count = 0;
            await foreach (var ld in doc.GetJsonLDElementsAsync())
            {
                count++;
            }

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public async Task Parse02()
        {
            var objects = await Properties.Resources.HTMLPage001.CreateHtmlDocument().GetJsonLDObjectsAsync("Product").ToListAsync();
            Assert.AreEqual(2, objects.Count);
        }

        [TestMethod]
        public async Task Parse03()
        {
            var objects = await Properties.Resources.HTMLPage001.CreateHtmlDocument().GetJsonLDObjectsAsync("Organization").ToListAsync();
            Assert.AreEqual(1, objects.Count);
        }

        [TestMethod]
        public async Task Parse04()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD004).RootElement;
            var objects = await jsonElem.GetJsonLDObjectsAsync().ToListAsync();
            Assert.AreEqual(4, objects.Count);
        }

        [TestMethod]
        public async Task Parse05()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD005).RootElement;
            var products = await jsonElem.GetJsonLDObjectsAsync("Product").ToListAsync();
            Assert.AreEqual(0, products.Count);

            var gprs = await jsonElem.GetJsonLDObjectsAsync("ProductGroup").ToListAsync();
            Assert.AreEqual(1, gprs.Count);
        }

        [TestMethod]
        public async Task Parse06()
        {
            var htmlDoc = Properties.Resources.HTMLPage002.CreateHtmlDocument();
            var objects = await htmlDoc.GetJsonLDObjectsAsync().ToListAsync();

            Assert.AreNotEqual(0, objects.Count);

            foreach(var obj in objects)
            {
                var d = obj.ToJsonDictionary();
                d.TryGetValue("@context", out var context);
                Assert.AreEqual("https://schema.org", context);
            }
        }

        [TestMethod]
        public async Task Parse07()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD006).RootElement;
            var persons = await jsonElem.GetJsonLDObjectsAsync("Person").ToListAsync();
            var orgs = await jsonElem.GetJsonLDObjectsAsync("Organization").ToListAsync();

            Assert.AreEqual(1, persons.Count);
            Assert.AreEqual(persons.Count, orgs.Count);
        }

        [TestMethod]
        public async Task Parse08()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD007).RootElement;
            Assert.IsTrue(jsonElem.IsSchemaOrgObjectType("Person"));
            Assert.IsTrue(jsonElem.IsSchemaOrgObjectType("Organization"));
        }

        [TestMethod]
        public async Task Parse09()
        {
            var htmlDoc = Properties.Resources.HTMLPage003.CreateHtmlDocument();
            var objects = await htmlDoc.GetJsonLDObjectsAsync().ToListAsync();
            Assert.AreEqual(1, objects.Count);

            var obj = objects.First();
            var d = obj.ToJsonDictionary();
            d.TryGetValue("@context", out var context);
            Assert.AreEqual("https://schema.org", context);
        }
    }
}
