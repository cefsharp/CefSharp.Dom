using System.Threading.Tasks;
using CefSharp.Dom;
using PuppeteerSharp.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace PuppeteerSharp.Tests.JSHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ClickablePointTests : DevToolsContextBaseTest
    {
        public ClickablePointTests(ITestOutputHelper output) : base(output)
        {
        }

        [PuppeteerFact]
        public async Task ShouldWork()
        {
            await DevToolsContext.EvaluateExpressionAsync(@"document.body.style.padding = '0';
                document.body.style.margin = '0';
                document.body.innerHTML = '<div style=""cursor: pointer; width: 120px; height: 60px; margin: 30px; padding: 15px;""></div>';
                ");

            await DevToolsContext.EvaluateExpressionAsync("new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));");

            var divHandle = await DevToolsContext.QuerySelectorAsync("div");

            var clickablePoint = await divHandle.ClickablePointAsync();

            // margin + middle point offset
            Assert.Equal(clickablePoint.X, 45 + 60);
            Assert.Equal(clickablePoint.Y, 45 + 30);

            clickablePoint = await divHandle.ClickablePointAsync(new Offset { X = 10, Y = 15 });

            // margin + offset
            Assert.Equal(clickablePoint.X, 30 + 10);
            Assert.Equal(clickablePoint.Y, 30 + 15);
        }

        [PuppeteerFact]
        public async Task ShouldWorkForIFrames()
        {
            await DevToolsContext.EvaluateExpressionAsync(@"document.body.style.padding = '10px';
                document.body.style.margin = '10px';
                document.body.innerHTML = `<iframe style=""border: none; margin: 0; padding: 0;"" seamless sandbox srcdoc=""<style>* { margin: 0; padding: 0;}</style><div style='cursor: pointer; width: 120px; height: 60px; margin: 30px; padding: 15px;' />""></iframe>`
                ");

            await DevToolsContext.EvaluateExpressionAsync("new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));");

            var frame = DevToolsContext.FirstChildFrame();

            var divHandle = await frame.QuerySelectorAsync("div");

            var clickablePoint = await divHandle.ClickablePointAsync();

            // iframe pos + margin + middle point offset
            Assert.Equal(clickablePoint.X, 20 + 45 + 60);
            Assert.Equal(clickablePoint.Y, 20 + 45 + 30);

            clickablePoint = await divHandle.ClickablePointAsync(new Offset { X = 10, Y = 15 });

            // iframe pos + margin + offset
            Assert.Equal(clickablePoint.X, 20 + 30 + 10);
            Assert.Equal(clickablePoint.Y, 20 + 30 + 15);
        }
    }
}
