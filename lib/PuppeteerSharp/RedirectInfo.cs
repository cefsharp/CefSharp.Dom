using CefSharp.Dom.Messaging;

namespace CefSharp.Dom
{
    internal class RedirectInfo
    {
        public RequestWillBeSentResponse Event { get; set; }

        public string FetchRequestId { get; set; }
    }
}
