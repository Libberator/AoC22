using System.Linq;
using System.Collections.Generic;
using System.Numerics;

namespace AoC22;

public class Day8 : Puzzle
{
    private readonly Dictionary<Vector2Int, int> _trees = new();
    private Bounds _forestBounds;
    private readonly Vector2Int[] _directions = new Vector2Int[4] { Vector2Int.Up, Vector2Int.Left, Vector2Int.Down, Vector2Int.Right };

    public Day8(ILogger logger, string path) : base(logger, path) { }
    
    public override void Setup()
    {
        int x = 0, y = 0;
        foreach (var line in ReadFromFile())
        {
            for (x = 0; x < line.Length; x++)
                _trees.Add(new Vector2Int(x, y), line[x] - '0');
            y++;
        }
        _forestBounds = new Bounds(0, x - 1, 0, y - 1);
    }

    public override void SolvePart1() => _logger.Log(_trees.Count(tree => IsVisible(tree.Key)));

    public override void SolvePart2() => _logger.Log(_trees.Max(tree => ScenicScore(tree.Key, tree.Value)));

    private bool IsEdge(Vector2Int pos) => _forestBounds.IsOnEdge(pos);
    
    private bool IsVisible(Vector2Int pos) => IsEdge(pos) || _directions.Any(dir => WalkInDirection(pos, dir, _trees[pos]).CanSeeEdge);
    
    private int ScenicScore(Vector2Int pos, int height) => IsEdge(pos) ? 0 : _directions.Aggregate(1, (total, dir) => total *= WalkInDirection(pos, dir, height).ViewingDistance);
    
    private (bool CanSeeEdge, int ViewingDistance) WalkInDirection(Vector2Int pos, Vector2Int dir, int height)
    {
        int count = 0;
        while (!IsEdge(pos))
        {
            pos += dir;
            count++;
            if (_trees[pos] >= height) return (false, count);
        }
        return (true, count);
    }
}