using System.Threading.Tasks;
using PuppeteerSharp.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace PuppeteerSharp.Tests.QuerySelectorTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class PageContextXPathTests : DevToolsContextBaseTest
    {
        public PageContextXPathTests(ITestOutputHelper output) : base(output)
        {
        }

        [PuppeteerFact]
        public async Task ShouldQueryExistingElement()
        {
            await DevToolsContext.SetContentAsync("<section>test</section>");
            var elements = await DevToolsContext.XPathAsync("/html/body/section");
            Assert.NotNull(elements[0]);
            Assert.Single(elements);
        }

        [PuppeteerFact]
        public async Task ShouldReturnEmptyArrayForNonExistingElement()
        {
            var elements = await DevToolsContext.XPathAsync("/html/body/non-existing-element");
            Assert.Empty(elements);
        }

        [PuppeteerFact]
        public async Task ShouldReturnMultipleElements()
        {
            await DevToolsContext.SetContentAsync("<div></div><div></div>");
            var elements = await DevToolsContext.XPathAsync("/html/body/div");
            Assert.Equal(2, elements.Length);
        }
    }
}
