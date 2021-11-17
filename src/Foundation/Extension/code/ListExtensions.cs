using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blog.Foundation.Extensions
{
    public static class ListExtensions
    {
        public static bool AnyOrNotNull<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }

        public static IEnumerable<TSource> Between<TSource, TResult>
        (
            this IEnumerable<TSource> source, Func<TSource, TResult> selector,
            TResult lowest, TResult highest
        ) where TResult : IComparable<TResult>
        {
            return source.OrderBy(selector).
                SkipWhile(s => selector.Invoke(s).CompareTo(lowest) < 0).
                TakeWhile(s => selector.Invoke(s).CompareTo(highest) <= 0);
        }

        public static bool None<T>(this IEnumerable<T> collection) => !collection.Any();
        public static bool None<T>(this IEnumerable<T> collection, Func<T, bool> predicate) => !collection.Any(predicate);

        public static string Join(this IEnumerable<string> source, string separator) => String.Join(separator, source);

        public static bool IsNullOrEmpty(this IList obj)
        {
            return obj == null || obj.Count == 0;
        }
    }
}
