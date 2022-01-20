using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CefSharp.Puppeteer.PageCoverage;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace PuppeteerSharp.Tests.CoverageTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class JSResetOnNavigationTests : PuppeteerPageBaseTest
    {
        public JSResetOnNavigationTests(ITestOutputHelper output) : base(output)
        {
        }

        [PuppeteerTest("coverage.spec.ts", "resetOnNavigation", "should report scripts across navigations when disabled")]
        [PuppeteerFact]
        public async Task ShouldReportScriptsAcrossNavigationsWhenDisabled()
        {
            await DevToolsContext.Coverage.StartJSCoverageAsync(new CoverageStartOptions
            {
                ResetOnNavigation = false
            });
            await DevToolsContext.GoToAsync(TestConstants.ServerUrl + "/jscoverage/multiple.html");
            await DevToolsContext.GoToAsync(TestConstants.EmptyPage);
            var coverage = await DevToolsContext.Coverage.StopJSCoverageAsync();
            Assert.Equal(2, coverage.Length);
        }

        [PuppeteerTest("coverage.spec.ts", "resetOnNavigation", "should NOT report scripts across navigations when enabled")]
        [PuppeteerFact]
        public async Task ShouldNotReportScriptsAcrossNavigationsWhenEnabled()
        {
            await DevToolsContext.Coverage.StartJSCoverageAsync();
            await DevToolsContext.GoToAsync(TestConstants.ServerUrl + "/jscoverage/multiple.html");
            await DevToolsContext.GoToAsync(TestConstants.EmptyPage);
            var coverage = await DevToolsContext.Coverage.StopJSCoverageAsync();
            Assert.Empty(coverage);
        }
    }
}
