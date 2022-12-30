using System;
using System.Collections.Generic;

namespace AoC22;

public static class Pathfinding
{
    #region A-Star

    /// <summary>
    /// A* Pathfinding. Returns a list of nodes in reverse order from the target destination (included) to the starting point (excluded).
    /// Before calling this, ensure the Nodes' Neighbors have already been populated.
    /// </summary>
    public static List<Node> FindPath(Node start, Node end)
    {
        var toSearch = new OrderedList<Node>(new AStarHeuristic()) { start };
        HashSet<Node> processed = new();

        while (toSearch.Count > 0)
        {
            var current = toSearch.Min;
            
            toSearch.RemoveAt(0);
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
                    toSearch.Remove(neighbor);

                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    if (!inSearch)
                        neighbor.SetH(neighbor.GetDistance(end));
                    
                    toSearch.Add(neighbor);
                }
            }
        }
        return new List<Node>();
    }

    private class AStarHeuristic : IComparer<Node>
    {
        public int Compare(Node current, Node next) => current.F != next.F ? current.F.CompareTo(next.F) : current.H.CompareTo(next.H);
    }

    #endregion A-Star

    #region Dijkstra (if weighted) FloodFill/BFS (if unweighted)

    /// <summary>
    /// Floodfill (a.k.a. Breadth-First Search). Use when you don't have a specific singular target in mind.
    /// Returns a list of nodes in reverse order from the node passing the target condition (included) to the starting point (excluded).
    /// </summary>
    public static List<Node> FloodFillUntil<T>(T start, Predicate<T> targetCondition) where T : Node
    {
        var toSearch = new OrderedList<Node>(new DijkstraHeuristic()) { start };
        HashSet<Node> processed = new();

        while (toSearch.Count > 0)
        {
            var current = toSearch.Min;

            toSearch.RemoveAt(0); 
            processed.Add(current);

            if (targetCondition(current as T))
                return BacktrackRoute(current, start);

            foreach (var neighbor in current.Neighbors)
            {
                if (processed.Contains(neighbor)) continue;

                var inSearch = toSearch.Contains(neighbor);
                var costToNeighbor = current.G + neighbor.BaseCost;

                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    toSearch.Remove(neighbor);

                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    toSearch.Add(neighbor);
                }
            }
        }
        return new List<Node>();
    }

    private class DijkstraHeuristic : IComparer<Node>
    {
        public int Compare(Node current, Node next) => current.G.CompareTo(next.G);
    }

    #endregion Dijkstra (if weighted) FloodFill/BFS (if unweighted)

    // TODO: Add the following...
    // Depth First Search (DFS)
    // Best-First
    // Bi-directional A*
    // Iterative Deeping A* (IDA*)
    // Minimum Spanning Tree (MSP)
    // For reference: https://github.com/brean/python-pathfinding 

    #region Shared Methods

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

    #endregion Shared Methods
}