using System.Collections.Generic;

namespace CSLox
{
    public static class IListExtensionMethods
    {
        public static bool IsEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        public static bool IsNotEmpty<T>(this IList<T> list)
        {
            return !list.IsEmpty();
        }

        public static void Push<T>(this IList<T> list, T value)
        {
            list.Add(value);
        }

        public static T Pop<T>(this IList<T> list)
        {
            if (list.IsEmpty()) return default;

            var value = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);

            return value;
        }

        public static T Peek<T>(this IList<T> list)
        {
            return list.IsEmpty() ? default : list[list.Count - 1];
        }
    }
}
