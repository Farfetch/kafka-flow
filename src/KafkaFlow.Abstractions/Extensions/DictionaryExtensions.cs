namespace KafkaFlow
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// No needed
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Thread-safe gets or adds an item on a dictionary
        /// </summary>
        /// <param name="dictionary">The dictionary where the operation act</param>
        /// <param name="key">The item key</param>
        /// <param name="addFactory">A handler to return the value when adding</param>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The value type</typeparam>
        /// <returns></returns>
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> addFactory)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out value))
                {
                    return value;
                }

                value = addFactory(key);

                dictionary.Add(key, value);

                return value;
            }
        }
    }
}
