using System;
using System.Collections.Generic;
using System.Numerics;

namespace AoC22;

public class Grid<T>
{
    private readonly T[][] _data;
    private readonly Dictionary<Vector2Int, Node<T>> _nodes = new();
    private readonly Bounds _bounds;

    /// <summary>
    /// Extra constraint to determine if two nodes are connected. 
    /// First argument is primary node, second argument is a prospective neighbor.
    /// </summary>
    public Func<Node<T>, Node<T>, bool> AreValidNeighbors { get; set; } = (node, neighbor) => true;
    /// <summary>Directions to search for neighbors. Default is Cardinal (N,E,S,W).</summary>
    public Vector2Int[] NeighborDirections { get; set; } = Vector2Int.CardinalDirections;

    public Grid(T[][] data, Func<Node<T>, Node<T>, bool> validNeighborCheck, Vector2Int[] neighborDirections) : this(data, neighborDirections) => AreValidNeighbors = validNeighborCheck;
    public Grid(T[][] data, Func<Node<T>, Node<T>, bool> validNeighborCheck) : this(data) => AreValidNeighbors = validNeighborCheck;
    public Grid(T[][] data, Vector2Int[] neighborDirections) : this(data) => NeighborDirections = neighborDirections;
    public Grid(T[][] data) // assumes a rectangular grid, despite being a jagged array
    {
        _data = data;
        _bounds = new(0, data.Length - 1, 0, data[0].Length - 1);
    }

    public T this[int row, int col]
    {
        get => _data[row][col];
        set => _data[row][col] = value;
    }

    public virtual IEnumerable<Node<T>> GetNeighborsOf(Node<T> node)
    {
        foreach (var dir in NeighborDirections)
            if (TryGetNode(node.Pos + dir, out var neighbor) && AreValidNeighbors(node, neighbor))
                yield return neighbor;
    }

    public virtual bool TryGetNode(Vector2Int pos, out Node<T> node)
    {
        node = default;
        if (!_bounds.Contains(pos)) return false;

        if (!_nodes.TryGetValue(pos, out node))
        {
            node = new Node<T>(_data[pos.X][pos.Y], pos, this);
            _nodes.Add(pos, node);
        }
        return true;
    }

    public bool AddNode(Node<T> node) => _nodes.TryAdd(node.Pos, node);
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