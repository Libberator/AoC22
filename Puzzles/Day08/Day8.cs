using System;
using System.Numerics;

namespace AoC22;

public class Day8 : Puzzle
{
    private string[] _data = Array.Empty<string>();
    private Bounds _forestBounds;

    public Day8(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        _data = ReadAllLines();
        _forestBounds = new Bounds(0, _data.Length - 1, 0, _data[0].Length - 1); // rows, columns
    }

    public override void SolvePart1()
    {
        var visibleTrees = 0;
        foreach (var pos in _forestBounds.GetAllCoordinates())
            if (IsVisible(pos, _data[pos.X][pos.Y])) visibleTrees++;
        _logger.Log(visibleTrees);
    }

    public override void SolvePart2()
    {
        var maxScenicScore = 0;
        foreach (var pos in _forestBounds.GetAllCoordinates())
        {
            var score = ScenicScore(pos, _data[pos.X][pos.Y]);
            if (score > maxScenicScore) maxScenicScore = score;
        }
        _logger.Log(maxScenicScore);
    }

    private bool IsEdge(Vector2Int pos) => _forestBounds.IsOnEdge(pos);
    private bool IsVisible(Vector2Int pos, int height)
    {
        if (IsEdge(pos)) return true;
        foreach (var dir in Vector2Int.CompassDirections)
            if (WalkInDirection(pos, dir, height).CanSeeEdge) return true;
        return false;
    }

    private int ScenicScore(Vector2Int pos, int height)
    {
        if (IsEdge(pos)) return 0;
        int score = 1;
        foreach (var dir in Vector2Int.CompassDirections)
            score *= WalkInDirection(pos, dir, height).ViewingDistance;
        return score;
    }

    private (bool CanSeeEdge, int ViewingDistance) WalkInDirection(Vector2Int pos, Vector2Int dir, int height)
    {
        int count = 0;
        while (!IsEdge(pos))
        {
            pos += dir;
            count++;
            if (_data[pos.X][pos.Y] >= height) return (false, count);
        }
        return (true, count);
    }
}