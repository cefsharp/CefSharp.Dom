using System.Collections.Generic;

namespace CefSharp.Dom.Messaging
{
    internal class NetworkSetExtraHTTPHeadersRequest
    {
        public NetworkSetExtraHTTPHeadersRequest(Dictionary<string, string> headers)
        {
            Headers = headers;
        }

        public Dictionary<string, string> Headers { get; set; }
    }
}
