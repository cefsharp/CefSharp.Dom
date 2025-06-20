using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using CefSharp.Dom;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace PuppeteerSharp.Tests.NetworkTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class ResponseTextTests : DevToolsContextBaseTest
    {
        public ResponseTextTests(ITestOutputHelper output) : base(output)
        {
        }

        [PuppeteerTest("network.spec.ts", "Response.text", "should work")]
        [PuppeteerFact]
        public async Task ShouldWork()
        {
            var response = await DevToolsContext.GoToAsync(TestConstants.ServerUrl + "/simple.json");
            Assert.Equal("{\"foo\": \"bar\"}", (await response.TextAsync()).Trim());
        }

        [PuppeteerTest("network.spec.ts", "Response.text", "should return uncompressed text")]
        [PuppeteerFact]
        public async Task ShouldReturnUncompressedText()
        {
            Server.EnableGzip("/simple.json");
            var response = await DevToolsContext.GoToAsync(TestConstants.ServerUrl + "/simple.json");
            Assert.Equal("gzip", response.Headers["Content-Encoding"]);
            Assert.Equal("{\"foo\": \"bar\"}", (await response.TextAsync()).Trim());
        }

        [PuppeteerTest("network.spec.ts", "Response.text", "should throw when requesting body of redirected response")]
        [PuppeteerFact]
        public async Task ShouldThrowWhenRequestingBodyOfRedirectedResponse()
        {
            Server.SetRedirect("/foo.html", "/empty.html");
            var response = await DevToolsContext.GoToAsync(TestConstants.ServerUrl + "/foo.html");
            var redirectChain = response.Request.RedirectChain;
            Assert.Single(redirectChain);
            var redirected = redirectChain[0].Response;
            Assert.Equal(HttpStatusCode.Redirect, redirected.Status);

            var exception = await Assert.ThrowsAsync<PuppeteerException>(async () => await redirected.TextAsync());
            Assert.Contains("Response body is unavailable for redirect responses", exception.Message);
        }

        [PuppeteerTest("network.spec.ts", "Response.text", "should wait until response completes")]
        [PuppeteerFact(Skip = "Investigate - Works most of the time")]
        public async Task ShouldWaitUntilResponseCompletes()
        {
            await DevToolsContext.GoToAsync(TestConstants.EmptyPage);
            // Setup server to trap request.
            var serverResponseCompletion = new TaskCompletionSource<bool>();
            HttpResponse serverResponse = null;
            Server.SetRoute("/get", context =>
            {
                serverResponse = context.Response;
                context.Response.WriteAsync("hello ");
                return serverResponseCompletion.Task;
            });
            // Setup page to trap response.
            Response pageResponse = null;
            var requestFinished = false;
            DevToolsContext.Response += (_, e) => pageResponse = e.Response;
            DevToolsContext.RequestFinished += (_, _) => requestFinished = true;
            // send request and wait for server response
            Task WaitForPageResponseEvent()
            {
                var completion = new TaskCompletionSource<bool>();
                DevToolsContext.Response += (_, e) =>
                {
                    if (!TestUtils.IsFavicon(e.Response.Request))
                    {
                        completion.SetResult(true);
                    }
                };
                return completion.Task;
            }

            await Task.WhenAll(
                Server.WaitForRequest("/get"),
                DevToolsContext.EvaluateExpressionAsync("fetch('/get', { method: 'GET'})"),
                WaitForPageResponseEvent()
            );

            Assert.NotNull(serverResponse);
            Assert.NotNull(pageResponse);
            Assert.Equal(HttpStatusCode.OK, pageResponse.Status);
            Assert.False(requestFinished);

            var responseText = pageResponse.TextAsync();
            // Write part of the response and wait for it to be flushed.
            await serverResponse.WriteAsync("wor");
            // Finish response.
            await serverResponse.WriteAsync("ld!");
            serverResponseCompletion.SetResult(true);
            Assert.Equal("hello world!", await responseText);
        }
    }
}
