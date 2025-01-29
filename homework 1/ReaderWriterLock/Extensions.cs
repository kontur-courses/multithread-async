using System;
using System.Collections.Generic;
using System.Linq;

namespace ReaderWriterLock;

public static class Extensions
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
            action(item);
    }

    public static string GetRandomString(this Random rnd, int minLength, int maxLength)
    {
        return new string(new char[rnd.Next(minLength, maxLength)]
            .Select(ch => chars[rnd.Next(chars.Length)])
            .ToArray());
    }
}