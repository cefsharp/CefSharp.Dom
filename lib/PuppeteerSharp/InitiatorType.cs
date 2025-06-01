using System.Runtime.Serialization;
using CefSharp.Dom.Helpers.Json;
using Newtonsoft.Json;

namespace CefSharp.Dom
{

    /// <summary>
    /// Type of the <see cref="Initiator"/>.
    /// </summary>
    [JsonConverter(typeof(FlexibleStringEnumConverter), Other)]
    public enum InitiatorType
    {
        /// <summary>
        /// Parser.
        /// </summary>
        Parser,

        /// <summary>
        /// Script.
        /// </summary>
        Script,

        /// <summary>
        /// Preload.
        /// </summary>
        Preload,

        /// <summary>
        /// SignedExchange.
        /// </summary>
        [EnumMember(Value = "SignedExchange")]
        SignedExchange,

        /// <summary>
        /// Preflight.
        /// </summary>
        Preflight,

        /// <summary>
        /// Other.
        /// </summary>
        Other,
    }
}
