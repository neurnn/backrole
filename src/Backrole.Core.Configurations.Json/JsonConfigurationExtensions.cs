using Backrole.Core.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Backrole.Core.Configurations.Json
{
    public static class JsonConfigurationExtensions
    {
        /// <summary>
        /// Make a configuration key that meets the <see cref="JsonConfigurationOptions"/>.
        /// </summary>
        /// <param name="Options"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        private static string MakeKey(this JsonConfigurationOptions Options, string Key) => Options.AsLowerCase ? Key.ToLower() : Key;

        /// <summary>
        /// Add configurations from the <paramref name="JsonString"/> with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="JsonReader"></param>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddJson(this IConfigurationBuilder This, string JsonString, Action<JsonConfigurationOptions> Configure = null)
        {
            using var Stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonString), false);
            using var Reader = new JsonTextReader(new StreamReader(Stream, Encoding.UTF8, false, 2048, true));
            return This.AddJson(Reader, Configure);
        }

        /// <summary>
        /// Add configurations from the <paramref name="JsonString"/> with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="JsonReader"></param>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddBson(this IConfigurationBuilder This, byte[] BsonBytes, Action<JsonConfigurationOptions> Configure = null)
            => This.AddBson(new BsonDataReader(new MemoryStream(BsonBytes, false)), Configure);

        /// <summary>
        /// Add configurations from the <see cref="JsonReader"/> with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="JsonReader"></param>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddJson(this IConfigurationBuilder This, JsonReader JsonReader, Action<JsonConfigurationOptions> Configure = null)
        {
            var Options = new JsonConfigurationOptions();
            Configure?.Invoke(Options);

            var JsonObject = JsonSerializer
                .CreateDefault(Options.Settings)
                .Deserialize<JObject>(JsonReader);

            if (!string.IsNullOrWhiteSpace(Options.Prefix))
                This = This.Subset(Options.Prefix.TrimEnd(':'));

            return This.AddJson(JsonObject, Options);
        }

        /// <summary>
        /// Add configurations from the <see cref="BsonDataReader"/> with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="BsonReader"></param>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddBson(this IConfigurationBuilder This, BsonDataReader BsonReader, Action<JsonConfigurationOptions> Configure = null)
        {
            var Options = new JsonConfigurationOptions();
            Configure?.Invoke(Options);

            var JsonObject = JsonSerializer
                .CreateDefault(Options.Settings)
                .Deserialize<JObject>(BsonReader);

            if (!string.IsNullOrWhiteSpace(Options.Prefix))
                This = This.Subset(Options.Prefix.TrimEnd(':'));

            return This.AddJson(JsonObject, Options);
        }

        /// <summary>
        /// Add configurations from the json file with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="FileInfo"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder This, FileInfo FileInfo, Action<JsonConfigurationOptions> Configure = null)
            => This.AddJsonFile(FileInfo.FullName, false, Configure);

        /// <summary>
        /// Add configurations from the bson file with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="FileInfo"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddBsonFile(this IConfigurationBuilder This, FileInfo FileInfo, Action<JsonConfigurationOptions> Configure = null)
            => This.AddBsonFile(FileInfo.FullName, false, Configure);

        /// <summary>
        /// Add configurations from the json file with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Required">Decides the <paramref name="FileInfo"/> is must-be or not.</param>
        /// <param name="FileInfo"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder This, FileInfo FileInfo, bool Required, Action<JsonConfigurationOptions> Configure = null)
            => This.AddJsonFile(FileInfo.FullName, Required, Configure);

        /// <summary>
        /// Add configurations from the bson file with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Required">Decides the <paramref name="FileInfo"/> is must-be or not.</param>
        /// <param name="FileInfo"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddBsonFile(this IConfigurationBuilder This, FileInfo FileInfo, bool Required, Action<JsonConfigurationOptions> Configure = null)
            => This.AddBsonFile(FileInfo.FullName, Required, Configure);

        /// <summary>
        /// Add configurations from the json file with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="PathToFile"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder This, string PathToFile, Action<JsonConfigurationOptions> Configure = null)
            => This.AddJsonFile(PathToFile, false, Configure);

        /// <summary>
        /// Add configurations from the bson file with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="PathToFile"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddBsonFile(this IConfigurationBuilder This, string PathToFile, Action<JsonConfigurationOptions> Configure = null)
            => This.AddBsonFile(PathToFile, false, Configure);

        /// <summary>
        /// Add configurations from the json file with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Required">Decides the <paramref name="PathToFile"/> is must-be or not.</param>
        /// <param name="PathToFile"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder This, string PathToFile, bool Required, Action<JsonConfigurationOptions> Configure = null)
        {
            var Cwd = Directory.GetCurrentDirectory();

            try
            {
                try
                {
                    Directory.SetCurrentDirectory(
                        Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                }
                catch { }

                if (!File.Exists(PathToFile))
                {
                    if (Required)
                        throw new FileNotFoundException($"File not found: {PathToFile}");

                    return This;
                }

                using var Stream = File.OpenRead(PathToFile);
                using var Reader = new JsonTextReader(new StreamReader(File.OpenRead(PathToFile), Encoding.UTF8, true, 2048, true));
                return This.AddJson(Reader, Configure);
            }

            finally { Directory.SetCurrentDirectory(Cwd); }
        }

        /// <summary>
        /// Add configurations from the bson file with <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Required">Decides the <paramref name="PathToFile"/> is must-be or not.</param>
        /// <param name="PathToFile"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddBsonFile(this IConfigurationBuilder This, string PathToFile, bool Required, Action<JsonConfigurationOptions> Configure = null)
        {
            var Cwd = Directory.GetCurrentDirectory();

            try
            {
                try
                {
                    Directory.SetCurrentDirectory(
                        Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                }
                catch { }

                if (!File.Exists(PathToFile))
                {
                    if (Required)
                        throw new FileNotFoundException($"File not found: {PathToFile}");

                    return This;
                }

                return This.AddBson(new BsonDataReader(File.OpenRead(PathToFile)), Configure);
            }

            finally { Directory.SetCurrentDirectory(Cwd); }
        }

        /// <summary>
        /// Add configurations from the <see cref="JObject"/> that contains json key-values.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="JsonObject"></param>
        /// <returns></returns>
        private static IConfigurationBuilder AddJson(this IConfigurationBuilder This, JObject JsonObject, JsonConfigurationOptions Options)
        {
            foreach(var Property in JsonObject.Properties())
            {
                switch (Property.Value.Type)
                {
                    case JTokenType.Null: break;
                    case JTokenType.Object:
                        This.Subset(Options.MakeKey(Property.Name)).AddJson(Property.Value.ToObject<JObject>(), Options);
                        break;

                    case JTokenType.Array:
                        This.Subset(Options.MakeKey(Property.Name)).AddJson(Property.Value.ToObject<JArray>(), Options);
                        break;

                    default:
                        This.Set(Options.MakeKey(Property.Name), Property.Value.ToString());
                        break;
                }
            }

            return This;
        }

        /// <summary>
        /// Add configurations from the <see cref="JObject"/> that contains json key-values.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="JsonArray"></param>
        /// <returns></returns>
        private static IConfigurationBuilder AddJson(this IConfigurationBuilder This, JArray JsonArray, JsonConfigurationOptions Options)
        {
            for(var i = 0; i < JsonArray.Count; ++i)
            {
                var Value = JsonArray[i];
                switch (Value.Type)
                {
                    case JTokenType.Null: break;
                    case JTokenType.Object:
                        This.Subset(i.ToString()).AddJson(Value.ToObject<JObject>(), Options);
                        break;

                    case JTokenType.Array:
                        This.Subset(i.ToString()).AddJson(Value.ToObject<JArray>(), Options);
                        break;

                    default:
                        This.Set(i.ToString(), Value.ToString());
                        break;
                }
            }

            return This;
        }

    }
}
