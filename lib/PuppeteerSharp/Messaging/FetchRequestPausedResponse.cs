namespace CefSharp.Dom.Messaging
{
    internal class FetchRequestPausedResponse : RequestWillBeSentResponse
    {
        public ResourceType? ResourceType { get; set; }
    }
}
