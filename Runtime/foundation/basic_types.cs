using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.UIWidgets.foundation {
    
    public delegate void ValueChanged<T>(T value);

    public delegate IEnumerable<T> EnumerableFilter<T>(IEnumerable<T> input);

    public static class ObjectUtils {
        public static T SafeDestroy<T>(T obj) where T : Object {
            if (Application.isEditor) {
                Object.DestroyImmediate(obj);
            } else {
                Object.Destroy(obj);
            }

            return null;
        }
    }

    public static class CollectionUtils {
        public static V putIfAbsent<K, V>(this IDictionary<K, V> it, K key, Func<V> ifAbsent) {
            V value;
            if (it.TryGetValue(key, out value)) {
                return value;
            }

            value = ifAbsent();
            it[key] = value;
            return value;
        }

        public static bool isEmpty<T>(this ICollection<T> it) {
            return it.Count == 0;
        }

        public static bool isNotEmpty<T>(this ICollection<T> it) {
            return it.Count != 0;
        }

        public static bool isEmpty<T>(this Queue<T> it) {
            return it.Count == 0;
        }

        public static bool isNotEmpty<T>(this Queue<T> it) {
            return it.Count != 0;
        }

        public static bool isEmpty<TKey, TValue>(this IDictionary<TKey, TValue> it) {
            return it.Count == 0;
        }

        public static bool isNotEmpty<TKey, TValue>(this IDictionary<TKey, TValue> it) {
            return it.Count != 0;
        }

        public static bool isEmpty(this string it) {
            return string.IsNullOrEmpty(it);
        }

        public static bool isNotEmpty(this string it) {
            return !string.IsNullOrEmpty(it);
        }
        
        public static TValue getOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> it, TKey key)
        {
            TValue v;
            it.TryGetValue(key, out v);
            return v;
        }
    }
}