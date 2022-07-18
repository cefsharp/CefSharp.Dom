using System.Threading.Tasks;
using CefSharp.DevTools.Dom;
using PuppeteerSharp.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace PuppeteerSharp.Tests.QuerySelectorTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class PageQuerySelectorAllEvalTests : DevToolsContextBaseTest
    {
        public PageQuerySelectorAllEvalTests(ITestOutputHelper output) : base(output)
        {
        }

        [PuppeteerFact]
        public async Task ShouldWork()
        {
            await DevToolsContext.SetContentAsync("<div>hello</div><div>beautiful</div><div>world!</div>");
            var divsCount = await DevToolsContext.QuerySelectorAllHandleAsync("div").EvaluateFunctionAsync<int>("divs => divs.length");
            Assert.Equal(3, divsCount);
        }

        [PuppeteerFact]
        public async Task ShouldWorkWithAwaitedElements()
        {
            await DevToolsContext.SetContentAsync("<div>hello</div><div>beautiful</div><div>world!</div>");
            var divs = await DevToolsContext.QuerySelectorAllHandleAsync("div");
            var divsCount = await divs.EvaluateFunctionAsync<int>("divs => divs.length");
            Assert.Equal(3, divsCount);
        }
    }
}
