using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace AoC22;

public class Grid<T> : IEnumerable<T>
{
    public readonly T[,] Data;
    public readonly Bounds Bounds;

    public Grid(int rows, int cols)
    {
        Data = new T[rows, cols];
        Bounds = new Bounds(0, rows - 1, 0, cols - 1);
    }

    public bool TryGetValue(int x, int y, out T point)
    {
        point = default;
        if (!Bounds.Contains(x, y)) return false;
        point = Data[x, y];
        return true;
    }

    public bool TryGetValue(Vector2Int pos, out T point) => TryGetValue(pos.X, pos.Y, out point);

    public void Add(int x, int y, T value) => Data[x, y] = value;
    public void Add(Vector2Int pos, T value) => Add(pos.X, pos.Y, value);

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var pos in Bounds.GetAllCoordinates())
            yield return Data[pos.X, pos.Y];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }

    public T this[int row, int col]
    {
        get => Data[row, col];
        set => Data[row, col] = value;
    }

    public T this[Vector2Int pos]
    {
        get => this[pos.X, pos.Y];
        set=> this[pos.X, pos.Y] = value;
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