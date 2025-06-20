using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CefSharp.Dom.Input;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace CefSharp.Dom
{
    /// <summary>
    /// Provides methods to interact with a single page frame in Chromium. One <see cref="IDevToolsContext"/> instance might have multiple <see cref="Frame"/> instances.
    /// At every point of time, page exposes its current frame tree via the <see cref="IDevToolsContext.MainFrame"/> and <see cref="ChildFrames"/> properties.
    ///
    /// <see cref="Frame"/> object's lifecycle is controlled by three events, dispatched on the page object
    /// - <see cref="IDevToolsContext.FrameAttached"/> - fires when the frame gets attached to the page. A Frame can be attached to the page only once
    /// - <see cref="IDevToolsContext.FrameNavigated"/> - fired when the frame commits navigation to a different URL
    /// - <see cref="IDevToolsContext.FrameDetached"/> - fired when the frame gets detached from the page.  A Frame can be detached from the page only once
    /// </summary>
    /// <example>
    /// An example of dumping frame tree
    /// <code>
    /// <![CDATA[
    /// await devToolsContext.GoToAsync("https://www.google.com/chrome/browser/canary.html");
    /// dumpFrameTree(devToolsContext.MainFrame, string.Empty);
    ///
    /// void dumpFrameTree(Frame frame, string indent)
    /// {
    ///     Console.WriteLine(indent + frame.Url);
    ///     foreach (var child in frame.ChildFrames)
    ///     {
    ///         dumpFrameTree(child, indent + "  ");
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public class Frame
    {
        private readonly List<Frame> _childFrames = new List<Frame>();

        /// <summary>
        /// Frame Id
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// IsMainFrame - true if main frame
        /// </summary>
        public bool IsMainFrame { get; internal set; }

        internal string LoaderId { get; set; }

        internal List<string> LifecycleEvents { get; }

        internal string NavigationURL { get; private set; }

        internal DOMWorld MainWorld { get; }

        internal DOMWorld SecondaryWorld { get; }

        internal Frame(FrameManager frameManager, Frame parentFrame, string frameId, bool isMainFrame)
        {
            FrameManager = frameManager;
            ParentFrame = parentFrame;
            Id = frameId;
            IsMainFrame = isMainFrame;

            Logger = frameManager.Connection.LoggerFactory.CreateLogger(GetType());

            LifecycleEvents = new List<string>();

            MainWorld = new DOMWorld(FrameManager, this, FrameManager.TimeoutSettings);
            SecondaryWorld = new DOMWorld(FrameManager, this, FrameManager.TimeoutSettings);

            if (parentFrame != null)
            {
                ParentFrame.AddChildFrame(this);
            }
        }

        /// <summary>
        /// Gets the child frames of the this frame
        /// </summary>
        public List<Frame> ChildFrames
        {
            get
            {
                lock (_childFrames)
                {
                    return _childFrames.ToList();
                }
            }
        }

        /// <summary>
        /// Gets the frame's name attribute as specified in the tag
        /// If the name is empty, returns the id attribute instead
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the frame's url
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets a value indicating if the frame is detached or not
        /// </summary>
        public bool Detached { get; set; }

        /// <summary>
        /// Gets the parent frame, if any. Detached frames and main frames return <c>null</c>
        /// </summary>
        public Frame ParentFrame { get; private set; }

        /// <summary>
        /// Logger.
        /// </summary>
        protected ILogger Logger { get; }

        internal FrameManager FrameManager { get; }

        /// <summary>
        /// Navigates to an url
        /// </summary>
        /// <remarks>
        /// <see cref="GoToAsync(string, int?, WaitUntilNavigation[])"/> will throw an error if:
        /// - there's an SSL error (e.g. in case of self-signed certificates).
        /// - target URL is invalid.
        /// - the `timeout` is exceeded during navigation.
        /// - the remote server does not respond or is unreachable.
        /// - the main resource failed to load.
        ///
        /// <see cref="GoToAsync(string, int?, WaitUntilNavigation[])"/> will not throw an error when any valid HTTP status code is returned by the remote server,
        /// including 404 "Not Found" and 500 "Internal Server Error".  The status code for such responses can be retrieved by calling <see cref="Response.Status"/>
        ///
        /// > **NOTE** <see cref="GoToAsync(string, int?, WaitUntilNavigation[])"/> either throws an error or returns a main resource response.
        /// The only exceptions are navigation to `about:blank` or navigation to the same URL with a different hash, which would succeed and return `null`.
        ///
        /// > **NOTE** Headless mode doesn't support navigation to a PDF document. See the <see fref="https://bugs.chromium.org/p/chromium/issues/detail?id=761295">upstream issue</see>.
        /// </remarks>
        /// <param name="url">URL to navigate page to. The url should include scheme, e.g. https://.</param>
        /// <param name="options">Navigation parameters.</param>
        /// <returns>Task which resolves to the main resource response. In case of multiple redirects, the navigation will resolve with the response of the last redirect.</returns>
        /// <seealso cref="GoToAsync(string, int?, WaitUntilNavigation[])"/>
        public Task<Response> GoToAsync(string url, NavigationOptions options) => FrameManager.NavigateFrameAsync(this, url, options);

        /// <summary>
        /// Navigates to an url
        /// </summary>
        /// <param name="url">URL to navigate page to. The url should include scheme, e.g. https://.</param>
        /// <param name="timeout">maximum navigation time in milliseconds. Defaults to 30 seconds. Pass 0
        /// to disable timeout. The default value can be changed by using the <see cref="IDevToolsContext.DefaultNavigationTimeout"/>
        /// property.</param>
        /// <param name="waitUntil">When to consider navigation succeeded, defaults to <see cref="WaitUntilNavigation.Load"/>. Given an array of <see cref="WaitUntilNavigation"/>, navigation is considered to be successful after all events have been fired</param>
        /// <returns>Task which resolves to the main resource response. In case of multiple redirects, the navigation will resolve with the response of the last redirect</returns>
        public Task<Response> GoToAsync(string url, int? timeout = null, WaitUntilNavigation[] waitUntil = null)
            => GoToAsync(url, new NavigationOptions { Timeout = timeout, WaitUntil = waitUntil });

        /// <summary>
        /// This resolves when the frame navigates to a new URL or reloads.
        /// It is useful for when you run code which will indirectly cause the frame to navigate.
        /// </summary>
        /// <param name="options">navigation options</param>
        /// <returns>Task which resolves to the main resource response.
        /// In case of multiple redirects, the navigation will resolve with the response of the last redirect.
        /// In case of navigation to a different anchor or navigation due to History API usage, the navigation will resolve with `null`.
        /// </returns>
        /// <remarks>
        /// Usage of the <c>History API</c> <see href="https://developer.mozilla.org/en-US/docs/Web/API/History_API"/> to change the URL is considered a navigation
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var navigationTask = devToolsContext.WaitForNavigationAsync();
        /// await frame.ClickAsync("a.my-link");
        /// await navigationTask;
        /// ]]>
        /// </code>
        /// </example>
        public Task<Response> WaitForNavigationAsync(NavigationOptions options = null) => FrameManager.WaitForFrameNavigationAsync(this, options);

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        /// <seealso cref="EvaluateFunctionAsync{T}(string, object[])"/>
        /// <seealso cref="IDevToolsContext.EvaluateExpressionAsync{T}(string)"/>
        public Task<JToken> EvaluateExpressionAsync(string script) => MainWorld.EvaluateExpressionAsync(script);

        /// <summary>
        /// Executes a script in browser context
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        /// <seealso cref="EvaluateFunctionAsync{T}(string, object[])"/>
        /// <seealso cref="IDevToolsContext.EvaluateExpressionAsync{T}(string)"/>
        public Task<T> EvaluateExpressionAsync<T>(string script) => MainWorld.EvaluateExpressionAsync<T>(script);

        /// <summary>
        /// Executes a function in browser context
        /// </summary>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to script</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// <see cref="JSHandle"/> instances can be passed as arguments
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        /// <seealso cref="EvaluateExpressionAsync{T}(string)"/>
        /// <seealso cref="IDevToolsContext.EvaluateFunctionAsync{T}(string, object[])"/>
        public Task<JToken> EvaluateFunctionAsync(string script, params object[] args) => MainWorld.EvaluateFunctionAsync(script, args);

        /// <summary>
        /// Executes a function in browser context
        /// </summary>
        /// <typeparam name="T">The type to deserialize the result to</typeparam>
        /// <param name="script">Script to be evaluated in browser context</param>
        /// <param name="args">Arguments to pass to script</param>
        /// <remarks>
        /// If the script, returns a Promise, then the method would wait for the promise to resolve and return its value.
        /// <see cref="JSHandle"/> instances can be passed as arguments
        /// </remarks>
        /// <returns>Task which resolves to script return value</returns>
        /// <seealso cref="EvaluateExpressionAsync{T}(string)"/>
        /// <seealso cref="IDevToolsContext.EvaluateFunctionAsync{T}(string, object[])"/>
        public Task<T> EvaluateFunctionAsync<T>(string script, params object[] args) => MainWorld.EvaluateFunctionAsync<T>(script, args);

        /// <summary>
        /// Passes an expression to the <see cref="ExecutionContext.EvaluateExpressionHandleAsync(string)"/>, returns a <see cref="Task"/>, then <see cref="ExecutionContext.EvaluateExpressionHandleAsync(string)"/> would wait for the <see cref="Task"/> to resolve and return its value.
        /// </summary>
        /// <example>
        /// <code>
        /// var frame = devToolsContext.MainFrame;
        /// const handle = devToolsContext.MainFrame.EvaluateExpressionHandleAsync("1 + 2");
        /// </code>
        /// </example>
        /// <returns>Resolves to the return value of <paramref name="script"/></returns>
        /// <param name="script">Expression to be evaluated in the <seealso cref="ExecutionContext"/></param>
        public Task<JSHandle> EvaluateExpressionHandleAsync(string script) => MainWorld.EvaluateExpressionHandleAsync(script);

        /// <summary>
        /// Passes a function to the <see cref="ExecutionContext.EvaluateFunctionAsync(string, object[])"/>, returns a <see cref="Task"/>, then <see cref="ExecutionContext.EvaluateFunctionHandleAsync(string, object[])"/> would wait for the <see cref="Task"/> to resolve and return its value.
        /// </summary>
        /// <example>
        /// <code>
        /// var frame = devToolsContext.MainFrame;
        /// const handle = devToolsContext.MainFrame.EvaluateFunctionHandleAsync("() => Promise.resolve(self)");
        /// return handle; // Handle for the global object.
        /// </code>
        /// <see cref="JSHandle"/> instances can be passed as arguments to the <see cref="ExecutionContext.EvaluateFunctionAsync(string, object[])"/>:
        ///
        /// const handle = await devToolsContext.MainFrame.EvaluateExpressionHandleAsync("document.body");
        /// const resultHandle = await devToolsContext.MainFrame.EvaluateFunctionHandleAsync("body => body.innerHTML", handle);
        /// return await resultHandle.JsonValueAsync(); // prints body's innerHTML
        /// </example>
        /// <returns>Resolves to the return value of <paramref name="function"/></returns>
        /// <param name="function">Function to be evaluated in the <see cref="ExecutionContext"/></param>
        /// <param name="args">Arguments to pass to <paramref name="function"/></param>
        public Task<JSHandle> EvaluateFunctionHandleAsync(string function, params object[] args) => MainWorld.EvaluateFunctionHandleAsync(function, args);

        /// <summary>
        /// Gets the <see cref="ExecutionContext"/> associated with the frame.
        /// </summary>
        /// <returns><see cref="ExecutionContext"/> associated with the frame.</returns>
        public Task<ExecutionContext> GetExecutionContextAsync() => MainWorld.GetExecutionContextAsync();

        /// <summary>
        /// Waits for a selector to be added to the DOM
        /// </summary>
        /// <param name="selector">A selector of an element to wait for</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task that resolves when element specified by selector string is added to DOM.
        /// Resolves to `null` if waiting for `hidden: true` and selector is not found in DOM.</returns>
        /// <seealso cref="WaitForXPathAsync(string, WaitForSelectorOptions)"/>
        /// <seealso cref="IDevToolsContext.WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public async Task<ElementHandle> WaitForSelectorAsync(string selector, WaitForSelectorOptions options = null)
        {
            var handle = await SecondaryWorld.WaitForSelectorAsync(selector, options).ConfigureAwait(false);
            if (handle == null)
            {
                return null;
            }
            var mainExecutionContext = await MainWorld.GetExecutionContextAsync().ConfigureAwait(false);
            var result = await mainExecutionContext.AdoptElementHandleAsync(handle).ConfigureAwait(false);
            await handle.DisposeAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Waits for a selector to be added to the DOM
        /// </summary>
        /// <param name="xpath">A xpath selector of an element to wait for</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task which resolves when element specified by xpath string is added to DOM.
        /// Resolves to `null` if waiting for `hidden: true` and xpath is not found in DOM.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// string currentURL = null;
        /// devToolsContext.MainFrame
        ///     .WaitForXPathAsync("//img")
        ///     .ContinueWith(_ => Console.WriteLine("First URL with image: " + currentURL));
        /// foreach (var current in new[] { "https://example.com", "https://google.com", "https://bbc.com" })
        /// {
        ///     currentURL = current;
        ///     await devToolsContext.GoToAsync(currentURL);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
        /// <seealso cref="IDevToolsContext.WaitForXPathAsync(string, WaitForSelectorOptions)"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public async Task<ElementHandle> WaitForXPathAsync(string xpath, WaitForSelectorOptions options = null)
        {
            var handle = await SecondaryWorld.WaitForXPathAsync(xpath, options).ConfigureAwait(false);
            if (handle == null)
            {
                return null;
            }
            var mainExecutionContext = await MainWorld.GetExecutionContextAsync().ConfigureAwait(false);
            var result = await mainExecutionContext.AdoptElementHandleAsync(handle).ConfigureAwait(false);
            await handle.DisposeAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Waits for a timeout
        /// </summary>
        /// <param name="milliseconds">The amount of time to wait.</param>
        /// <returns>A task that resolves when after the timeout</returns>
        /// <seealso cref="IDevToolsContext.WaitForTimeoutAsync(int)"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public Task WaitForTimeoutAsync(int milliseconds) => Task.Delay(milliseconds);

        /// <summary>
        /// Waits for a function to be evaluated to a truthy value
        /// </summary>
        /// <param name="script">Function to be evaluated in browser context</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <param name="args">Arguments to pass to <c>script</c></param>
        /// <returns>A task that resolves when the <c>script</c> returns a truthy value</returns>
        /// <seealso cref="IDevToolsContext.WaitForFunctionAsync(string, WaitForFunctionOptions, object[])"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public Task<JSHandle> WaitForFunctionAsync(string script, WaitForFunctionOptions options, params object[] args)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return MainWorld.WaitForFunctionAsync(script, options, args);
        }

        /// <summary>
        /// Waits for an expression to be evaluated to a truthy value
        /// </summary>
        /// <param name="script">Expression to be evaluated in browser context</param>
        /// <param name="options">Optional waiting parameters</param>
        /// <returns>A task that resolves when the <c>script</c> returns a truthy value</returns>
        /// <seealso cref="IDevToolsContext.WaitForExpressionAsync(string, WaitForFunctionOptions)"/>
        /// <exception cref="WaitTaskTimeoutException">If timeout occurred.</exception>
        public Task<JSHandle> WaitForExpressionAsync(string script, WaitForFunctionOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return MainWorld.WaitForExpressionAsync(script, options);
        }

        /// <summary>
        /// Triggers a change and input event once all the provided options have been selected.
        /// If there's no <![CDATA[<select>]]> element matching selector, the method throws an error.
        /// </summary>
        /// <exception cref="SelectorException">If there's no element matching <paramref name="selector"/></exception>
        /// <param name="selector">A selector to query page for</param>
        /// <param name="values">Values of options to select. If the <![CDATA[<select>]]> has the multiple attribute,
        /// all values are considered, otherwise only the first one is taken into account.</param>
        /// <returns>Returns an array of option values that have been successfully selected.</returns>
        /// <seealso cref="IDevToolsContext.SelectAsync(string, string[])"/>
        public Task<string[]> SelectAsync(string selector, params string[] values) => SecondaryWorld.SelectAsync(selector, values);

        /// <summary>
        /// Queries frame for the selector. If there's no such element within the frame, the method will resolve to <c>null</c>.
        /// </summary>
        /// <param name="selector">Selector to query frame for</param>
        /// <returns>Task which resolves to <see cref="ElementHandle"/> pointing to the frame element</returns>
        /// <seealso cref="IDevToolsContext.QuerySelectorAsync(string)"/>
        public Task<ElementHandle> QuerySelectorAsync(string selector) => MainWorld.QuerySelectorAsync(selector);

        /// <summary>
        /// Queries frame for the selector. If no elements match the selector, the return value resolve to <see cref="Array.Empty{T}"/>.
        /// </summary>
        /// <param name="selector">A selector to query frame for</param>
        /// <returns>Task which resolves to ElementHandles pointing to the frame elements</returns>
        /// <seealso cref="IDevToolsContext.QuerySelectorAllAsync(string)"/>
        public Task<ElementHandle[]> QuerySelectorAllAsync(string selector) => MainWorld.QuerySelectorAllAsync(selector);

        /// <summary>
        /// Evaluates the XPath expression
        /// </summary>
        /// <param name="expression">Expression to evaluate <see href="https://developer.mozilla.org/en-US/docs/Web/API/Document/evaluate"/></param>
        /// <returns>Task which resolves to an array of <see cref="ElementHandle"/></returns>
        /// <seealso cref="IDevToolsContext.XPathAsync(string)"/>
        public Task<ElementHandle[]> XPathAsync(string expression) => MainWorld.XPathAsync(expression);

        /// <summary>
        /// Adds a <c><![CDATA[<link rel="stylesheet">]]></c> tag into the page with the desired url or a <c><![CDATA[<link rel="stylesheet">]]></c> tag with the content
        /// </summary>
        /// <param name="options">add style tag options</param>
        /// <returns>Task which resolves to the added tag when the stylesheet's onload fires or when the CSS content was injected into frame</returns>
        /// <seealso cref="IDevToolsContext.AddStyleTagAsync(AddTagOptions)"/>
        /// <seealso cref="IDevToolsContext.AddStyleTagAsync(string)"/>
        public Task<ElementHandle> AddStyleTagAsync(AddTagOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return MainWorld.AddStyleTagAsync(options);
        }

        /// <summary>
        /// Adds a <c><![CDATA[<script>]]></c> tag into the page with the desired url or content
        /// </summary>
        /// <param name="options">add script tag options</param>
        /// <returns>Task which resolves to the added tag when the script's onload fires or when the script content was injected into frame</returns>
        /// <seealso cref="IDevToolsContext.AddScriptTagAsync(AddTagOptions)"/>
        /// <seealso cref="IDevToolsContext.AddScriptTagAsync(string)"/>
        public Task<ElementHandle> AddScriptTagAsync(AddTagOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return MainWorld.AddScriptTagAsync(options);
        }

        /// <summary>
        /// Gets the full HTML contents of the page, including the doctype.
        /// </summary>
        /// <returns>Task which resolves to the HTML content.</returns>
        /// <seealso cref="IDevToolsContext.GetContentAsync"/>
        public Task<string> GetContentAsync() => SecondaryWorld.GetContentAsync();

        /// <summary>
        /// Sets the HTML markup to the page
        /// </summary>
        /// <param name="html">HTML markup to assign to the page.</param>
        /// <param name="options">The options</param>
        /// <returns>Task.</returns>
        /// <seealso cref="IDevToolsContext.SetContentAsync(string, NavigationOptions)"/>
        public Task SetContentAsync(string html, NavigationOptions options = null)
            => SecondaryWorld.SetContentAsync(html, options);

        /// <summary>
        /// Returns page's title
        /// </summary>
        /// <returns>page's title</returns>
        /// <seealso cref="IDevToolsContext.GetTitleAsync"/>
        public Task<string> GetTitleAsync() => SecondaryWorld.GetTitleAsync();

        /// <summary>
        /// Fetches an element with <paramref name="selector"/>, scrolls it into view if needed, and then uses <see cref="IDevToolsContext.Mouse"/> to click in the center of the element.
        /// </summary>
        /// <param name="selector">A selector to search for element to click. If there are multiple elements satisfying the selector, the first will be clicked.</param>
        /// <param name="options">click options</param>
        /// <exception cref="SelectorException">If there's no element matching <paramref name="selector"/></exception>
        /// <returns>Task which resolves when the element matching <paramref name="selector"/> is successfully clicked</returns>
        public Task ClickAsync(string selector, ClickOptions options = null)
            => SecondaryWorld.ClickAsync(selector, options);

        /// <summary>
        /// Fetches an element with <paramref name="selector"/>, scrolls it into view if needed, and then uses <see cref="IDevToolsContext.Mouse"/> to hover over the center of the element.
        /// </summary>
        /// <param name="selector">A selector to search for element to hover. If there are multiple elements satisfying the selector, the first will be hovered.</param>
        /// <exception cref="SelectorException">If there's no element matching <paramref name="selector"/></exception>
        /// <returns>Task which resolves when the element matching <paramref name="selector"/> is successfully hovered</returns>
        public Task HoverAsync(string selector) => SecondaryWorld.HoverAsync(selector);

        /// <summary>
        /// Fetches an element with <paramref name="selector"/> and focuses it
        /// </summary>
        /// <param name="selector">A selector to search for element to focus. If there are multiple elements satisfying the selector, the first will be focused.</param>
        /// <exception cref="SelectorException">If there's no element matching <paramref name="selector"/></exception>
        /// <returns>Task which resolves when the element matching <paramref name="selector"/> is successfully focused</returns>
        public Task FocusAsync(string selector) => SecondaryWorld.FocusAsync(selector);

        /// <summary>
        /// Sends a <c>keydown</c>, <c>keypress</c>/<c>input</c>, and <c>keyup</c> event for each character in the text.
        /// </summary>
        /// <param name="selector">A selector of an element to type into. If there are multiple elements satisfying the selector, the first will be used.</param>
        /// <param name="text">A text to type into a focused element</param>
        /// <param name="options">The options to apply to the type operation.</param>
        /// <exception cref="SelectorException">If there's no element matching <paramref name="selector"/></exception>
        /// <remarks>
        /// To press a special key, like <c>Control</c> or <c>ArrowDown</c> use <see cref="Keyboard.PressAsync(string, PressOptions)"/>
        /// </remarks>
        /// <example>
        /// <code>
        /// await frame.TypeAsync("#mytextarea", "Hello"); // Types instantly
        /// await frame.TypeAsync("#mytextarea", "World", new TypeOptions { Delay = 100 }); // Types slower, like a user
        /// </code>
        /// </example>
        /// <returns>Task</returns>
        public Task TypeAsync(string selector, string text, TypeOptions options = null)
             => SecondaryWorld.TypeAsync(selector, text, options);

        /// <summary>
        /// The method runs <c>document.querySelector</c> within the page. If no element matches the selector, the return value resolve to <c>null</c>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="HtmlElement"/> or derived type</typeparam>
        /// <param name="querySelector">A selector to query page for</param>
        /// <returns>Task which resolves to <see cref="HtmlElement"/> pointing to the frame element</returns>
        /// <remarks>
        /// Shortcut for <c>page.MainFrame.QuerySelectorAsync(selector)</c>
        /// </remarks>
        /// <seealso cref="Frame.QuerySelectorAsync(string)"/>
        public async Task<T> QuerySelectorAsync<T>(string querySelector)
            where T : Element
        {
            var handle = await QuerySelectorAsync(querySelector).ConfigureAwait(false);

            if (handle == null)
            {
                return default;
            }

            return handle.ToDomHandle<T>();
        }

        /// <summary>
        /// Runs <c>document.querySelectorAll</c> within the page. If no elements match the selector, the return value resolve to <see cref="Array.Empty{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type derived from <see cref="Element"/></typeparam>
        /// <param name="querySelector">A selector to query page for</param>
        /// <returns>Task which resolves to ElementHandles pointing to the frame elements</returns>
        /// <seealso cref="Frame.QuerySelectorAllAsync(string)"/>
        public async Task<T[]> QuerySelectorAllAsync<T>(string querySelector)
            where T : Element
        {
            var elements = await QuerySelectorAllAsync(querySelector).ConfigureAwait(false);

            var list = new List<T>();

            foreach (var element in elements)
            {
                list.Add(element.ToDomHandle<T>());
            }

            return list.ToArray();
        }

        internal void AddChildFrame(Frame frame)
        {
            lock (_childFrames)
            {
                _childFrames.Add(frame);
            }
        }

        internal void RemoveChildFrame(Frame frame)
        {
            lock (_childFrames)
            {
                _childFrames.Remove(frame);
            }
        }

        internal void OnLoadingStopped()
        {
            LifecycleEvents.Add("DOMContentLoaded");
            LifecycleEvents.Add("load");
        }

        internal void OnLifecycleEvent(string loaderId, string name)
        {
            if (name == "init")
            {
                LoaderId = loaderId;
                LifecycleEvents.Clear();
            }
            LifecycleEvents.Add(name);
        }

        internal void Navigated(FramePayload framePayload)
        {
            Name = framePayload.Name ?? string.Empty;
            NavigationURL = framePayload.Url + framePayload.UrlFragment;
            Url = framePayload.Url + framePayload.UrlFragment;
        }

        internal void NavigatedWithinDocument(string url) => Url = url;

        internal void Detach()
        {
            Detached = true;
            MainWorld.Detach();
            SecondaryWorld.Detach();
            if (ParentFrame != null)
            {
                ParentFrame.RemoveChildFrame(this);
            }
            ParentFrame = null;
        }

        /// <summary>
        /// The frame element associated with this frame (if any).
        /// </summary>
        /// <returns>Task which resolves to the frame element.</returns>
        public async Task<ElementHandle> FrameElementAsync()
        {
            var parentFrame = ParentFrame;
            if (parentFrame == null)
            {
                return null;
            }

            var list = await parentFrame.EvaluateFunctionHandleAsync(@"() => {
                return document.querySelectorAll('iframe, frame');
            }").ConfigureAwait(false);

            await foreach (var iframe in list.TransposeIterableHandleAsync())
            {
                var frame = await iframe.ContentFrameAsync().ConfigureAwait(false);
                if (frame?.Id == Id)
                {
                    return iframe as ElementHandle;
                }

                try
                {
                    await iframe.DisposeAsync().ConfigureAwait(false);
                }
                catch
                {
                    Logger.LogWarning("FrameElementAsync: Error disposing iframe");
                }
            }

            return null;
        }
    }
}
