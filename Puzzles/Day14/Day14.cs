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
        DrawResult();

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
            pos.Y += 1; // down 1 step

            // falling into the endless void (a.k.a. hit the floor)
            if (pos.Y == _maxDepth) return false;

            // if can't move down. try moving left
            if (_walls.Contains(pos) || _sands.Contains(pos))
            {
                pos.X -= 1; // down-left position

                // if can't move left. try moving right
                if (_walls.Contains(pos) || _sands.Contains(pos))
                {
                    pos.X += 2; // down-right position

                    // if can't move down, down-left, or down-right, the sand settles
                    if (_walls.Contains(pos) || _sands.Contains(pos)) return true;
                }
            }
        }
    }

    private void DrawResult()
    {
        var xMin = _sands.Min(s => s.X);
        var xMax = _sands.Max(s => s.X);
        //_logger.Log($"xmin: {xMin}, xmax: {xMax}, ymin: 0, ymax: {_maxDepth}");
        var sb = new StringBuilder();

        for (int y = 0; y < _maxDepth; y++) // +1?
        {
            for (int x = xMin; x <= xMax; x++)
            {
                var pos = new Vector2Int(x, y);
                if (_walls.Contains(pos)) sb.Append('▒');
                else if (_sands.Contains(pos)) sb.Append('o');
                else sb.Append('.');
            }
            sb.AppendLine();
        }

        _logger.Log(sb.ToString());
    }
}