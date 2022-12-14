using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AoC22;

public class Day14 : Puzzle
{
    private readonly HashSet<Vector2Int> _walls = new();
    private readonly HashSet<Vector2Int> _sands = new();
    private readonly Vector2Int _startingPos = new(500, 0);
    private int _maxDepth;

    public Day14(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var pattern = Utils.NumberPattern();
        foreach (var line in ReadFromFile())
        {
            var matches = pattern.Matches(line);
            var posA = new Vector2Int(int.Parse(matches[0].ValueSpan), int.Parse(matches[1].ValueSpan));
            for (int i = 2; i < matches.Count; i += 2)
            {
                var posB = new Vector2Int(int.Parse(matches[i].ValueSpan), int.Parse(matches[i + 1].ValueSpan));
                foreach (var pos in Vector2Int.GetChebyshevPathTo(posA, posB))
                {
                    _walls.Add(pos);
                    _maxDepth = Math.Max(_maxDepth, pos.Y);
                }
                posA = posB;
            }
        }
        _maxDepth += 2;
    }

    public override void SolvePart1()
    {
        while (TryGetNextPosition(_startingPos, out var settledPos))
            _sands.Add(settledPos);
        _logger.Log(_sands.Count);
    }

    public override void SolvePart2()
    {
        var settledPos = Vector2Int.Zero;
        while (settledPos != _startingPos)
        {
            _ = TryGetNextPosition(_startingPos, out settledPos);
            _sands.Add(settledPos);
        }
        _logger.Log(_sands.Count);
    }

    private bool TryGetNextPosition(Vector2Int pos, out Vector2Int settlePos)
    {
        while (true)
        {
            settlePos = pos; // capture last known safe position to settle
            pos.Y += 1; // move down 1 step
            if (pos.Y >= _maxDepth) return false; // endless void (a.k.a. hit the floor)
            if (IsBlocked(pos)) // can't move down, try moving left
            {
                pos.X -= 1; // to down-left position
                if (IsBlocked(pos)) // can't move left, try moving right
                {
                    pos.X += 2; // to down-right position
                    if (IsBlocked(pos)) return true; // can't move, the sand settles
                }
            }
        }
    }

    private bool IsBlocked(Vector2Int pos) => _walls.Contains(pos) || _sands.Contains(pos);

    private void DrawResults()
    {
        var sb = new StringBuilder();
        var xMin = _sands.Min(s => s.X);
        var xMax = _sands.Max(s => s.X);
        for (int y = 0; y < _maxDepth; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                var pos = new Vector2Int(x, y);
                if (_walls.Contains(pos)) sb.Append('#');
                else if (_sands.Contains(pos)) sb.Append('o');
                else sb.Append('.');
            }
            sb.AppendLine();
        }
        _logger.Log(sb.ToString());
    }
}