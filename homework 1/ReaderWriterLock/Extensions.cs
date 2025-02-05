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

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
    {
        var index = 0;
        foreach (var item in enumerable)
        {
            action(item, index);
            index++;
        }
    }
}