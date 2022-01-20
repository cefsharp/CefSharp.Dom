using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace PuppeteerSharp.Tests.NetworkTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ResponseStatusTextTests : PuppeteerPageBaseTest
    {
        public ResponseStatusTextTests(ITestOutputHelper output) : base(output)
        {
        }

        [PuppeteerTest("network.spec.ts", "Response.statusText", "should work")]
        [PuppeteerFact]
        public async Task ShouldWork()
        {
            Server.SetRoute("/cool", (context) =>
            {
                context.Response.StatusCode = 200;
                //There are some debates about this on these issues
                //https://github.com/aspnet/HttpAbstractions/issues/395
                //https://github.com/aspnet/HttpAbstractions/issues/486
                //https://github.com/aspnet/HttpAbstractions/issues/794
                context.Features.Get<IHttpResponseFeature>().ReasonPhrase = "cool!";
                return Task.CompletedTask;
            });
            var response = await DevToolsContext.GoToAsync(TestConstants.ServerUrl + "/cool");
            Assert.Equal("cool!", response.StatusText);
        }
    }
}
