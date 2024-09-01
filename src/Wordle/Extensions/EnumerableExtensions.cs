using System;

namespace Wordle.Extensions;

public static class EnumerableExtensions
{
    public static T PickOne<T>(this IEnumerable<T> enumerable)
    {
        if (!enumerable.Any())
        {
            throw new ArgumentException("The enumerable must have at leat one element");
        }
        
        if (enumerable.Count() == 1)
        {
            return enumerable.First();
        }

        var randomIndex = new Random().Next(enumerable.Count());
        return enumerable.ElementAt(randomIndex);
    }
}
