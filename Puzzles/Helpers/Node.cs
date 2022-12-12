using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC22;

/// <summary>Generic class used for a point in a grid. Used for A* pathfinding.</summary>
public class Node
{
    /// <summary>Coordinate of this Node</summary>
    public Vector2Int Pos { get; private set; }
    /// <summary>Cost to go to this node</summary>
    public int BaseCost { get; private set; }
    /// <summary>Used for backtracking when pathfinding. Should be one of its neighbors.</summary>
    public Node Connection { get; private set; }
    /// <summary>List of connecting nodes. Populate this before trying to pathfind.</summary>
    public List<Node> Neighbors { get; } = new();
    /// <summary>Cost from Start (all previous Costs + this BaseCost)</summary>
    public int G { get; private set; }
    /// <summary>Distance to target node. Aids in traveling more directly</summary>
    public int H { get; private set; }
    /// <summary>Total Heuristic for traveling to this node</summary>
    public int F => G + H;

    public Node(int x, int y, int cost = 1) : this(new(x, y), cost) { }
    public Node(Vector2Int pos, int cost = 1)
    {
        Pos = pos;
        BaseCost = cost;
    }

    public void SetG(int val) => G = val;
    public void SetH(int val) => H = val;
    public void SetConnection(Node node) => Connection = node;
    // alternatives: Pos.DistanceSquaredTo(target.Pos); or Pos.DistanceManhattanTo(target.Pos); or  Pos.DistanceChebyshevTo(target.Pos);
    /// <summary>May have mixed results depending on many factors. Consider overriding this with Manhattan or Chebyshev Distance.</summary>
    public virtual int GetDistance<T>(T target) where T : Node => (int)Math.Round(10 * Pos.DistanceEuclidianTo(target.Pos));
    protected virtual bool IsValidNeighbor<T>(T other) where T : Node => Pos.IsAdjacentTo(other.Pos); // optional to add: || Pos.IsDiagonalTo(other.Pos);
    public virtual void FindAndAddNeighbors<T>(IDictionary<Vector2Int, T> grid) where T : Node
    {
        foreach (var dir in Vector2Int.CompassDirections)
            if (grid.TryGetValue(Pos + dir, out var neighbor) && IsValidNeighbor(neighbor)) AddNeighbor(neighbor);
    }
    public virtual void FindAndAddNeighbors<T>(IEnumerable<T> grid) where T : Node => AddNeighbors(grid.Where(IsValidNeighbor));
    public void AddNeighbors(IEnumerable<Node> neighbors) => Neighbors.AddRange(neighbors);
    public void AddNeighbor(Node neighbor) => Neighbors.Add(neighbor);
    public void RemoveNeighbors(IEnumerable<Node> neighbors) => Neighbors.RemoveAll(n => neighbors.Contains(n));
    public void RemoveNeighbor(Node neighbor) => Neighbors.Remove(neighbor);
}

public static class Pathfinding
{
    /// <summary>
    /// A* Pathfinding. Returns a list of nodes in reverse order from the target destination (included) to the starting point (excluded).
    /// Before calling this, ensure the Nodes' Neighbors have already been populated.
    /// </summary>
    public static List<T> FindPath<T>(T start, T end) where T : Node
    {
        var toSearch = new List<T>() { start };
        var processed = new List<T>();

        while (toSearch.Any())
        {
            var current = toSearch[0];
            foreach (var next in toSearch)
                if (next.IsBetterCandidateThan(current))
                    current = next;

            toSearch.Remove(current);
            processed.Add(current);

            if (current == end)
                return BacktrackRoute(end, start);

            foreach (var neighbor in current.Neighbors)
            {
                if (processed.Contains(neighbor)) continue;

                var inSearch = toSearch.Contains(neighbor);
                var costToNeighbor = current.G + neighbor.BaseCost;

                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    if (!inSearch)
                    {
                        neighbor.SetH(neighbor.GetDistance(end));
                        toSearch.Add(neighbor as T);
                    }
                }
            }
        }
        return new List<T>();
    }

    /// <summary>Returns a value that indicates it's a cheaper cost to travel to next instead of the current leading node.</summary>
    private static bool IsBetterCandidateThan<T>(this T next, T current) where T : Node => next.F < current.F || (next.F == current.F && next.H < current.H);

    private static List<T> BacktrackRoute<T>(T target, T start) where T : Node
    {
        var path = new List<T>();
        var currentNode = target;
        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.Connection as T;
        }
        return path;
    }
}
