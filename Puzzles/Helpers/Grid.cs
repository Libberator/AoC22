using System;
using System.Collections.Generic;
using System.Numerics;

namespace AoC22;

public class Grid<T>
{
    public readonly Dictionary<Vector2Int, T> Data = new();

    public Bounds Bounds { get; set; }

    public bool TryGetPointAt(int x, int y, out T point) => TryGetPointAt(new(x, y), out point);
    public bool TryGetPointAt(Vector2Int pos, out T point) => Data.TryGetValue(pos, out point);

    public void Add(int x, int y, T value) => Add(new(x, y), value);
    public void Add(Vector2Int pos, T value)
    {
        Data.Add(pos, value);
        Bounds.Encapsulate(pos);
    }

    public void ApplyToAll(Action<T> action)
    {
        foreach (var kvp in Data)
            action?.Invoke(kvp.Value);
    }

    public IEnumerable<T> Where(Predicate<T> predicate)
    {
        if (predicate == null) throw new Exception("No valid predicate provided.");

        foreach (var kvp in Data)
            if (predicate(kvp.Value))
                yield return kvp.Value;
    }

    /*
        TODO Features:
        Slice (for copy or cut), top-left pos, width, height
        Paste (and similarly, Fill)
        FlipX/FlipY
        Rotate
        Shift
        Get, Set
        *, +, -
        Foreach
    */

    /*
        Grid - to - graph
        Add a pretty drawer for multi-dimensional array
        characters for empty space / wall / node / etc
        A---C---F
        |   |   |
        |   D---G
        |   |
        B---E 
    */
}