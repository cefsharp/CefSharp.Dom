using System;

namespace CefSharp.DevTools.Dom.Transport
{
    /// <summary>
    /// Message received event arguments.
    /// <see cref="IConnectionTransport.MessageReceived"/>
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Transport.MessageReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public MessageReceivedEventArgs(string message) => Message = message;
        /// <summary>
        /// Initializes a new instance of the <see cref="Transport.MessageReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="ex">Exception</param>
        public MessageReceivedEventArgs(Exception ex) => Exception = ex;
        /// <summary>
        /// Transport message.
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// Exception
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// An exception occurred
        /// </summary>
        public bool HasException => Exception != null;
    }
}
