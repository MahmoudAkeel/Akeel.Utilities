using System;
using System.Collections.Generic;

namespace Akeel.Utilities.ExtensionMethods
{
    public static class DictionaryExtensions
    {
        public static TResult TryGetValueOrDefault<TKey, TResult>(this Dictionary<TKey, TResult> dictionary, TKey key,
            Func<TResult> defaultValue)
            => dictionary.TryGetValue(key, out TResult result) ? result : defaultValue();
    }
}
