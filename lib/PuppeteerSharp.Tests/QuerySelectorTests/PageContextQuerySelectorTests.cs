using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using PuppeteerSharp.Tests.Attributes;

namespace PuppeteerSharp.Tests.QuerySelectorTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class PageContextQuerySelectorTests : DevToolsContextBaseTest
    {
        public PageContextQuerySelectorTests(ITestOutputHelper output) : base(output)
        {
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public static async Task Usage()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            #region QuerySelector

            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await DevToolsContext.GoToAsync("http://www.google.com"); // In case of fonts being loaded from a CDN, use WaitUntilNavigation.Networkidle0 as a second param.

            // Add using PuppeteerSharp.Dom to access QuerySelectorAsync<T> and QuerySelectorAllAsync<T> extension methods.
            // Get element by Id
            // https://developer.mozilla.org/en-US/docs/Web/API/Document/querySelector
            var element = await DevToolsContext.QuerySelectorAsync<HtmlElement>("#myElementId");

            //Strongly typed element types (this is only a subset of the types mapped)
            var htmlDivElement = await DevToolsContext.QuerySelectorAsync<HtmlDivElement>("#myDivElementId");
            var htmlSpanElement = await DevToolsContext.QuerySelectorAsync<HtmlSpanElement>("#mySpanElementId");
            var htmlSelectElement = await DevToolsContext.QuerySelectorAsync<HtmlSelectElement>("#mySelectElementId");
            var htmlInputElement = await DevToolsContext.QuerySelectorAsync<HtmlInputElement>("#myInputElementId");
            var htmlFormElement = await DevToolsContext.QuerySelectorAsync<HtmlFormElement>("#myFormElementId");
            var htmlAnchorElement = await DevToolsContext.QuerySelectorAsync<HtmlAnchorElement>("#myAnchorElementId");
            var htmlImageElement = await DevToolsContext.QuerySelectorAsync<HtmlImageElement>("#myImageElementId");
            var htmlTextAreaElement = await DevToolsContext.QuerySelectorAsync<HtmlImageElement>("#myTextAreaElementId");
            var htmlButtonElement = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("#myButtonElementId");
            var htmlParagraphElement = await DevToolsContext.QuerySelectorAsync<HtmlParagraphElement>("#myParagraphElementId");
            var htmlTableElement = await DevToolsContext.QuerySelectorAsync<HtmlTableElement>("#myTableElementId");

            // Get a custom attribute value
            var customAttribute = await element.GetAttributeAsync<string>("data-customAttribute");

            //Set innerText property for the element
            await element.SetInnerTextAsync("Welcome!");

            //Get innerText property for the element
            var innerText = await element.GetInnerTextAsync();

            //Get all child elements
            var childElements = await element.QuerySelectorAllAsync("div");

            //Change CSS style background colour
            await element.EvaluateFunctionAsync("e => e.style.backgroundColor = 'yellow'");

            //Type text in an input field
            await element.TypeAsync("Welcome to my Website!");

            //Click The element
            await element.ClickAsync();

            // Simple way of chaining method calls together when you don't need a handle to the HtmlElement
            var htmlButtonElementInnerText = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("#myButtonElementId")
                .AndThen(x => x.GetInnerTextAsync());

            //Event Handler
            //Expose a function to javascript, functions persist across navigations
            //So only need to do this once
            await DevToolsContext.ExposeFunctionAsync("jsAlertButtonClick", () =>
            {
                _ = page.EvaluateExpressionAsync("window.alert('Hello! You invoked window.alert()');");
            });

            var jsAlertButton = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("#jsAlertButton");

            //Write up the click event listner to call our exposed function
            _ = jsAlertButton.AddEventListenerAsync("click", "jsAlertButtonClick");

            //Get a collection of HtmlElements
            var divElements = await DevToolsContext.QuerySelectorAllAsync<HtmlDivElement>("div");

            foreach (var div in divElements)
            {
                // Get a reference to the CSSStyleDeclaration
                var style = await div.GetStyleAsync();

                //Set the border to 1px solid red
                await style.SetPropertyAsync("border", "1px solid red", important: true);

                await div.SetAttributeAsync("data-customAttribute", "123");
                await div.SetInnerTextAsync("Updated Div innerText");
            }

            //Using standard array
            var tableRows = await htmlTableElement.GetRowsAsync().ToArrayAsync();

            foreach (var row in tableRows)
            {
                var cells = await row.GetCellsAsync().ToArrayAsync();
                foreach (var cell in cells)
                {
                    var newDiv = await DevToolsContext.CreateHtmlElementAsync<HtmlDivElement>("div");
                    await newDiv.SetInnerTextAsync("New Div Added!");
                    await cell.AppendChildAsync(newDiv);
                }
            }

            //Get a reference to the HtmlCollection and use async enumerable
            //Requires Net Core 3.1 or higher
            var tableRowsHtmlCollection = await htmlTableElement.GetRowsAsync();

            await foreach (var row in tableRowsHtmlCollection)
            {
                var cells = await row.GetCellsAsync();
                await foreach (var cell in cells)
                {
                    var newDiv = await DevToolsContext.CreateHtmlElementAsync<HtmlDivElement>("div");
                    await newDiv.SetInnerTextAsync("New Div Added!");
                    await cell.AppendChildAsync(newDiv);
                }
            }

            #endregion
        }

        [PuppeteerFact]
        public async Task ShouldQueryExistingElement()
        {
            await DevToolsContext.SetContentAsync("<section>test</section>");
            var element = await DevToolsContext.QuerySelectorAsync("section");
            Assert.NotNull(element);
        }

        [PuppeteerFact]
        public async Task ShouldReturnNullForNonExistingElement()
        {
            var element = await DevToolsContext.QuerySelectorAsync("non-existing-element");
            Assert.Null(element);
        }
    }
}
