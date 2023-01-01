using System;
using System.Collections.Generic;
using System.Numerics;

namespace AoC22;

public interface INode
{
    /// <summary>Coordinate of this Node</summary>
    Vector2Int Pos { get; }
    /// <summary>Cost to go to this node. Make sure the magnitude of this lines up with the scale of the HCost (a.k.a. Distance) calculation.</summary>
    int BaseCost { get; }
    /// <summary>Total Heuristic for traveling to this node.</summary>
    int F { get; }
    /// <summary>Cost from Start (all previous Costs + this BaseCost).</summary>
    int G { get; set; }
    /// <summary>Distance to target node. Aids in selecting the next best node.</summary>
    int H { get; set; }
    /// <summary>Used for backtracking when pathfinding. Will be one of its neighbors.</summary>
    INode Connection { get; set; }
    /// <summary>
    /// Returns the cost to go to the <paramref name="target"/> if it were a clear path. 
    /// Make sure this and the BaseCost have the same magnitude. e.g. BaseCost of 10 for 10*Euclidian, BaseCost of 1 for Manhattan/Chebyshev, etc.
    /// </summary>
    int GetHCostTo(INode target);
    /// <summary>Connected nodes that we can go to from this node (may not be symmetric for directed graphs).</summary>
    IEnumerable<INode> Neighbors { get; }
}

/// <summary>Generic class to represent a point on a grid, and store extra info in Value. Used for A* Pathfinding and Floodfill.</summary>
public class Node<T> : INode
{
    protected readonly Grid<T> _grid;
    public T Value { get; set; }

    public Node(Vector2Int pos, Grid<T> grid, int cost = 10) : this(default, pos, grid, cost) { }
    public Node(T value, Vector2Int pos, Grid<T> grid, int cost = 10)
    {
        Value = value;
        Pos = pos;
        BaseCost = cost;
        _grid = grid;
    }

    public Vector2Int Pos { get; }
    public int BaseCost { get; }
    public int F => G + H;
    public int G { get; set; }
    public int H { get; set; }
    public INode Connection { get; set; }
    public virtual IEnumerable<INode> Neighbors => _grid.GetNeighborsOf(this);
    public virtual int GetHCostTo(INode target) => (int)Math.Round(10 * Pos.DistanceEuclidianTo(target.Pos)); // Pos.DistanceChebyshevTo(target.Pos); // Pos.DistanceManhattanTo(target.Pos);

    public override int GetHashCode() => Pos.GetHashCode();
}