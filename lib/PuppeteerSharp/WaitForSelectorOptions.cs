namespace CefSharp.Dom
{
    /// <summary>
    /// Optional waiting parameters.
    /// </summary>
    /// <seealso cref="IDevToolsContext.WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
    /// <seealso cref="Frame.WaitForSelectorAsync(string, WaitForSelectorOptions)"/>
    public class WaitForSelectorOptions
    {
        /// <summary>
        /// Maximum time to wait for in milliseconds. Defaults to `30000` (30 seconds).
        /// Pass `0` to disable timeout.
        /// The default value can be changed by using <seealso cref="IDevToolsContext.DefaultTimeout"/>  method
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// If set to <see langword="true" />, the method will return <see langword="null"/> if the timeout is reached.
        /// The default (<see langword="false" />) will throw a <see cref="WaitTaskTimeoutException"/> if the timeout is reached."/>
        /// </summary>
        public bool TimeoutReturnsNull { get; set; } = false;

        /// <summary>
        /// Wait for element to be present in DOM and to be visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Wait for element to not be found in the DOM or to be hidden.
        /// </summary>
        public bool Hidden { get; set; }
    }
}
