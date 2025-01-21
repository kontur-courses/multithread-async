using System;
using System.Collections.Generic;

namespace ReaderWriterLock;

public static class Extensions
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
            action(item);
    }
}