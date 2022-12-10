using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC22;

/// <summary>Class used for a point in a grid. Used for A* pathfinding.</summary>
public class Node
{
    /// <summary>Coordinate of this Node</summary>
    public Vector2Int Pos { get; private set; }
    /// <summary>Cost to go to this node</summary>
    public int Value { get; private set; }
    /// <summary>Used for backtracking when pathfinding. Should be one of its neighbors.</summary>
    public Node? Connection { get; private set; }
    /// <summary>List of connecting nodes. Populate this before trying to pathfind.</summary>
    public List<Node> Neighbors { get; private set; } = new();
    /// <summary>Cost from Start (all previous Values + this Value)</summary>
    public int G { get; private set; }
    /// <summary>Distance to target node. Aids in traveling more directly</summary>
    public int H { get; private set; }
    /// <summary>Total Heuristic for traveling to this node</summary>
    public int F => G + H;

    public Node(int x, int y, int value = 1)
    {
        Pos = new(x, y);
        Value = value;
    }

    public void SetG(int val) => G = val;
    public void SetH(int val) => H = val;
    public void SetConnection(Node node) => Connection = node;
    // alternatives: Pos.DistanceSquared(target.Pos); or Pos.DistanceManhattan(target.Pos); or Pos.DistanceChebyshev(target.Pos);
    public virtual int GetDistance(Node target) => (int)Math.Round(10 * Pos.DistanceEuclidianTo(target.Pos));
    protected virtual bool IsANeighbor(Node other) => Pos.IsAdjacentTo(other.Pos); // optional to add: || Pos.IsDiagonalTo(other.Pos);
    public void FindAndAddNeighbors(IEnumerable<Node> grid) => AddNeighbors(grid.Where(IsANeighbor));
    public void AddNeighbors(IEnumerable<Node> allNeighbors) => Neighbors.AddRange(allNeighbors);
    public void AddNeighbor(Node neighbor) => Neighbors.Add(neighbor);
}

public static class Pathfinding
{
    /// <summary>A* Pathfinding. Before calling this, ensure the Nodes' Neighbors have already been populated.</summary>
    public static List<Node> FindPath(Node start, Node end)
    {
        var toSearch = new List<Node>() { start };
        var processed = new List<Node>();

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
                var costToNeighbor = current.G + neighbor.Value;

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
        return new List<Node>();
    }

    /// <summary>Returns a value that indicates it's a cheaper cost to travel to next instead of the current leading node.</summary>
    private static bool IsBetterCandidateThan(this Node next, Node current) => next.F < current.F || (next.F == current.F && next.H < current.H);

    /// <summary>Returns a list of nodes in reverse order from the target destination to the starting point.</summary>
    private static List<Node> BacktrackRoute(Node target, Node start)
    {
        var path = new List<Node>();
        Node currentNode = target;
        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.Connection!;
        }
        path.Add(start);
        return path;
    }
}
