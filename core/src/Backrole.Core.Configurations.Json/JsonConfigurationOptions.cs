using Newtonsoft.Json;

namespace Backrole.Core.Configurations.Json
{
    public sealed class JsonConfigurationOptions
    {
        /// <summary>
        /// Json Serialization Settings.
        /// </summary>
        public JsonSerializerSettings Settings { get; set; } = new JsonSerializerSettings();

        /// <summary>
        /// Prefix of the configuration key.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// Treat all keys as lower-case.
        /// </summary>
        public bool AsLowerCase { get; set; } = true;
    }
}
