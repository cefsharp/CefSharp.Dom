using System;

namespace CefSharp.Dom
{
    /// <summary>
    /// <see cref="IDevToolsContext.Response"/> arguments.
    /// </summary>
    public class ResponseCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// ResponseCreatedEventArgs
        /// </summary>
        /// <param name="response">response</param>
        public ResponseCreatedEventArgs(Response response)
        {
            Response = response;
        }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>The response.</value>
        public Response Response { get; }
    }
}
