using Denomica.JsonLD.Extensions;
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
    }
}
