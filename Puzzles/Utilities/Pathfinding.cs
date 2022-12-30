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
    public static List<Node> FindPath_AStar(Node start, Node end)
    {
        SortedSet<Node> toSearch = new(new AStarHeuristic()) { start };
        HashSet<Node> processed = new();

        while (toSearch.Count > 0)
        {
            var current = toSearch.Min;
            
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
        public int Compare(Node current, Node next) => current.F != next.F ? current.F.CompareTo(next.F) : 
            current.H != next.H ? current.H.CompareTo(next.H) :
            current.Pos.GetHashCode().CompareTo(next.Pos.GetHashCode());
    }

    #endregion A-Star

    #region Dijkstra / FloodFill/BFS

    /// <summary>
    /// If the graph is unweighted (same cost to go to each node), then this is just Floodfill (a.k.a. Breadth-First Search). 
    /// If it's a weighted graph, then this is Dijkstra. Use this when you don't have a specific singular target in mind.
    /// Returns a list of nodes in reverse order from the node passing the target condition (included) to the starting point (excluded).
    /// </summary>
    public static List<Node> FindPath_Dijkstra<T>(T start, Predicate<T> endCondition) where T : Node
    {
        SortedSet<Node> toSearch = new(new DijkstraHeuristic()) { start };
        HashSet<Node> processed = new();

        while (toSearch.Count > 0)
        {
            var current = toSearch.Min;
            
            toSearch.Remove(current);
            processed.Add(current);

            if (endCondition(current as T))
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
        public int Compare(Node current, Node next) => current.G != next.G ? current.G.CompareTo(next.G) : 
            current.Pos.GetHashCode().CompareTo(next.Pos.GetHashCode());
    }

    #endregion Dijkstra / FloodFill/BFS

    // TODO: Convert these methods to be more flexible - consider interfaces. And then add the following...
    // Depth First Search (DFS)
    // Best-First
    // Bi-directional A*
    // Iterative Deeping A* (IDA*)
    // Minimum Spanning Tree (MSP)
    // For reference: https://github.com/brean/python-pathfinding 

    #region Shared Methods

    /// <summary>Returns a list of nodes in reverse order from the target destination (included) to the starting point (excluded).</summary>
    private static List<Node> BacktrackRoute(Node target, Node start)
    {
        var path = new List<Node>();
        var currentNode = target;
        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.Connection;
        }
        return path;
    }

    #endregion Shared Methods
}