namespace CefSharp.Dom
{
    /// <summary>
    /// Optional waiting parameters.
    /// </summary>
    /// <seealso cref="IDevToolsContext.WaitForFunctionAsync(string, WaitForFunctionOptions, object[])"/>
    /// <seealso cref="Frame.WaitForFunctionAsync(string, WaitForFunctionOptions, object[])"/>
    /// <seealso cref="WaitForSelectorOptions"/>
    public class WaitForFunctionOptions
    {
        /// <summary>
        /// Maximum time to wait for in milliseconds. Defaults to 30000 (30 seconds). Pass 0 to disable timeout.
        /// The default value can be changed by setting the <see cref="IDevToolsContext.DefaultTimeout"/> property.
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// If set to <see langword="true" />, the method will return <see langword="null"/> if the timeout is reached.
        /// The default (<see langword="false" />) will throw a <see cref="WaitTaskTimeoutException"/> if the timeout is reached."/>
        /// </summary>
        public bool TimeoutReturnsNull { get; set; } = false;

        /// <summary>
        /// An interval at which the <c>pageFunction</c> is executed. defaults to <see cref="WaitForFunctionPollingOption.Raf"/>
        /// </summary>
        public WaitForFunctionPollingOption Polling { get; set; } = WaitForFunctionPollingOption.Raf;

        /// <summary>
        /// An interval at which the <c>pageFunction</c> is executed. If no value is specified will use <see cref="Polling"/>
        /// </summary>
        public int? PollingInterval { get; set; }
    }
}
