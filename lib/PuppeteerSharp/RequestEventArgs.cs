using System;

namespace CefSharp.Dom
{
    /// <summary>
    /// Arguments used by <see cref="IDevToolsContext"/> events.
    /// </summary>
    /// <seealso cref="IDevToolsContext.Request"/>
    /// <seealso cref="IDevToolsContext.RequestFailed"/>
    /// <seealso cref="IDevToolsContext.RequestFinished"/>
    public class RequestEventArgs : EventArgs
    {
        /// <summary>
        /// RequestEventArgs
        /// </summary>
        /// <param name="request">request</param>
        public RequestEventArgs(Request request)
        {
            Request = request;
        }

        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        /// <value>The request.</value>
        public Request Request { get; }
    }
}
