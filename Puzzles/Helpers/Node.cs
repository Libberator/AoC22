using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC22;

/// <summary>Generic class used for a point in a grid. Used for A* Pathfinding and Floodfill.</summary>
public class Node
{
    /// <summary>Coordinate of this Node</summary>
    public Vector2Int Pos { get; private set; }
    /// <summary>Cost to go to this node. Make sure the magnitude of this lines up with the scale of the Distance calculations.</summary>
    public int BaseCost { get; private set; }
    /// <summary>Used for backtracking when pathfinding. Should be one of its neighbors.</summary>
    public Node Connection { get; private set; }
    /// <summary>List of connecting nodes. Populate this before trying to pathfind.</summary>
    public virtual List<Node> Neighbors { get; } = new();
    /// <summary>Cost from Start (all previous Costs + this BaseCost).</summary>
    public int G { get; private set; }
    /// <summary>Distance to target node. Aids in traveling more directly.</summary>
    public int H { get; private set; }
    /// <summary>Total Heuristic for traveling to this node.</summary>
    public int F => G + H;

    public Node(Vector2Int pos, int cost = 10)
    {
        Pos = pos;
        BaseCost = cost;
    }

    /// <summary>Set the G Cost. This is the total cost from the Start to this Node (all previous Costs + this BaseCost)</summary>
    public void SetG(int val) => G = val;
    /// <summary>Set the H Cost. This is the cost it would take from this Node to the final target if it were a clear path. This aids in selecting the next best Node.</summary>
    public void SetH(int val) => H = val;
    public void SetConnection(Node node) => Connection = node;
    /// <summary>Make sure the scale of this matches lines up with the BaseCost magnitude. e.g. BaseCost of 10 for 10*Euclidian, BaseCost of 1 for Manhattan/Chebyshev, etc.</summary>
    public int GetDistance(Node target) => (int)Math.Round(10 * Pos.DistanceEuclidianTo(target.Pos)); // Pos.DistanceChebyshevTo(target.Pos); // Pos.DistanceManhattanTo(target.Pos);
    public virtual bool IsValidNeighbor(Node other) => IsValidNeighbor(this, other);
    public virtual bool IsValidNeighbor(Node self, Node other) => self.Pos.IsAdjacentTo(other.Pos); // optional to add: || Pos.IsDiagonalTo(other.Pos);
    public virtual void InitNeighbors<T>(IDictionary<Vector2Int, T> grid, Func<T, T, bool> isValidNeighbor = null) where T : Node
    {
        isValidNeighbor ??= IsValidNeighbor;
        Neighbors.Clear();
        foreach (var dir in Vector2Int.CardinalDirections)
            if (grid.TryGetValue(Pos + dir, out var neighbor) && isValidNeighbor(this as T, neighbor)) AddNeighbor(neighbor);
    }
    public virtual void InitNeighbors<T>(Grid<T> grid, Func<T, T, bool> isValidNeighbor = null) where T : Node
    {
        isValidNeighbor ??= IsValidNeighbor;
        Neighbors.Clear();
        foreach (var dir in Vector2Int.CardinalDirections)
            if (grid.TryGetValue(Pos + dir, out var neighbor) && isValidNeighbor(this as T, neighbor)) AddNeighbor(neighbor);
    }
    public virtual void InitNeighbors<T>(IEnumerable<T> grid, Func<T, T, bool> isValidNeighbor = null) where T : Node
    {
        isValidNeighbor ??= IsValidNeighbor;
        Neighbors.Clear();
        AddNeighbors(grid.Where(n => isValidNeighbor(this as T, n)));
    }
    public void AddNeighbors<T>(IEnumerable<T> neighbors) where T : Node => Neighbors.AddRange(neighbors);
    public void AddNeighbor<T>(T neighbor) where T : Node => Neighbors.Add(neighbor);
    public void RemoveNeighbors<T>(IEnumerable<T> neighbors) where T : Node => Neighbors.RemoveAll(n => neighbors.Contains(n));
    public void RemoveNeighbor<T>(T neighbor) where T : Node => Neighbors.Remove(neighbor);

    public override int GetHashCode() => Pos.GetHashCode();
}

public class Node<T> : Node
{
    public T Value { get; set; }
    public Node(T value, Vector2Int pos, int cost = 10) : base(pos, cost) { Value = value; }
}

public static class Pathfinding
{
    /// <summary>
    /// A* Pathfinding. Returns a list of nodes in reverse order from the target destination (included) to the starting point (excluded).
    /// Before calling this, ensure the Nodes' Neighbors have already been populated.
    /// </summary>
    public static List<T> FindPath<T>(T start, T end) where T : Node
    {
        var toSearch = new List<Node>() { start };
        HashSet<Node> processed = new();

        while (toSearch.Count > 0)
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
                        toSearch.Add(neighbor);
                    }
                }
            }
        }
        return new List<T>();
    }

    /// <summary>Returns a value that indicates it's a cheaper cost to travel to next instead of the current leading node.</summary>
    private static bool IsBetterCandidateThan<T>(this T next, T current) where T : Node => next.F < current.F || (next.F == current.F && next.H < current.H);
    
    /// <summary>Returns a list of nodes in reverse order from the target destination (included) to the starting point (excluded).</summary>
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

    /// <summary>
    /// Floodfill (a.k.a. Breadth-First Search). Use when you don't have a specific singular target in mind.
    /// Returns a list of nodes in reverse order from the node passing the target condition (included) to the starting point (excluded).
    /// </summary>
    public static List<T> FloodFillUntil<T>(T start, Predicate<T> targetCondition) where T : Node
    {
        var toSearch = new List<T>() { start };
        HashSet<T> processed = new();

        while (toSearch.Count > 0)
        {
            var current = toSearch[0];
            foreach (var next in toSearch)
                if (next.IsBetterBFSCandidateThan(current))
                    current = next;
            
            toSearch.Remove(current);
            processed.Add(current);

            if (targetCondition(current))
                return BacktrackRoute(current, start);

            foreach (var neighbor in current.Neighbors)
            {
                if (processed.Contains(neighbor as T)) continue;

                var inSearch = toSearch.Contains(neighbor as T);
                var costToNeighbor = current.G + neighbor.BaseCost;

                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    if (!inSearch)
                        toSearch.Add(neighbor as T);
                }
            }
        }
        return new List<T>();
    }

    /// <summary>If all BaseCosts are the same, this is just a basic floodfill or BFS. If some nodes cost more to travel to, it will prioritize a cheaper route.</summary>
    private static bool IsBetterBFSCandidateThan<T>(this T next, T current) where T : Node => next.G < current.G;
}
