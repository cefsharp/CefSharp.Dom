namespace CefSharp.Dom.Messaging
{
    internal class NetworkSetCacheDisabledRequest
    {
        public NetworkSetCacheDisabledRequest(bool cacheDisabled)
        {
            CacheDisabled = cacheDisabled;
        }

        public bool CacheDisabled { get; set; }
    }
}
