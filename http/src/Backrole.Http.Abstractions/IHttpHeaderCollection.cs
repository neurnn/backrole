using System.Collections.Generic;

namespace Backrole.Http.Abstractions
{
    /// <summary>
    /// Contains the http headers.
    /// </summary>
    public interface IHttpHeaderCollection : IList<KeyValuePair<string, string>>
    {
        /// <summary>
        /// Get the index of the key.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        int IndexOf(string Key);

        /// <summary>
        /// Get the last index of the key.
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        int LastIndexOf(string Key);
    }

    /// <summary>
    /// Header coll
    /// </summary>
    public static class IHttpHeaderCollectionExtensions
    {
        /// <summary>
        /// Get a header.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static string GetValue(this IHttpHeaderCollection This, string Key, string Default = null)
        {
            var Index = This.LastIndexOf(Key);
            if (Index >= 0)
            {
                return This[Index].Value.Trim();
            }

            return Default;
        }

        /// <summary>
        /// Add a header.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static IHttpHeaderCollection Add(this IHttpHeaderCollection This, string Key, string Value)
        {
            if (string.IsNullOrWhiteSpace(Value))
                return This;

            This.Add(new KeyValuePair<string, string>(Key, Value));
            return This;
        }

        /// <summary>
        /// Set a header.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static IHttpHeaderCollection Set(this IHttpHeaderCollection This, string Key, string Value) => Remove(This, Key).Add(Key, Value);

        /// <summary>
        /// Remove a key from the collection.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static IHttpHeaderCollection Remove(this IHttpHeaderCollection This, string Key)
        {
            while (true)
            {
                var Index = This.LastIndexOf(Key);
                if (Index >= 0)
                {
                    This.RemoveAt(Index);
                    continue;
                }

                return This;
            }
        }
    }
}
