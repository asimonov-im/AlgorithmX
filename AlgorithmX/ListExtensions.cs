using System.Collections.Generic;

namespace AlgorithmX
{
    public static class ListExtensions
    {
        public static void RemoveLast<T>(this List<T> lst)
            => lst.RemoveAt(lst.Count - 1);
    }
}
