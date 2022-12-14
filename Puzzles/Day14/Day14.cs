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
    private readonly Stack<Vector2Int> _startingPoints = new();
    private readonly Vector2Int _startingPos = new(500, 0);
    private int _maxDepth;

    public Day14(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
        {
            string[] pairs = line.Split("->", StringSplitOptions.TrimEntries);
            var posA = ParseToVector(pairs[0]);
            _maxDepth = Math.Max(_maxDepth, posA.Y);
            for (int i = 1; i < pairs.Length; i++)
            {
                var posB = ParseToVector(pairs[i]);
                _maxDepth = Math.Max(_maxDepth, posB.Y);
                foreach (var pos in Vector2Int.GetChebyshevPath(posA, posB))
                    _walls.Add(pos);
                posA = posB;
            }
        }

        static Vector2Int ParseToVector(string pair)
        {
            var numbers = pair.Split(',');
            return new Vector2Int(int.Parse(numbers[0]), int.Parse(numbers[1]));
        }

        _maxDepth += 2;
        _startingPoints.Push(_startingPos);
    }

    public override void SolvePart1()
    {
        while (TryGetNextPosition(_startingPoints, out var settledPos))
            _sands.Add(settledPos);
        _logger.Log(_sands.Count);
    }

    public override void SolvePart2()
    {
        var settledPos = Vector2Int.Zero;
        while (settledPos != _startingPos)
        {
            _ = TryGetNextPosition(_startingPoints, out settledPos);
            _sands.Add(settledPos);
        }
        _logger.Log(_sands.Count);
    }

    private bool TryGetNextPosition(Stack<Vector2Int> startingPoints, out Vector2Int settlePos)
    {
        var pos = startingPoints.Pop();

        while (true)
        {
            settlePos = pos; // capture last known safe position to settle
            pos.Y += 1; // move down 1 step
            if (pos.Y >= _maxDepth) return false; // endless void (a.k.a. hit the floor)
            startingPoints.Push(settlePos); // cache a safe starting point
            if (!IsBlocked(pos)) continue; // can move down
            pos.X -= 1; // can't move down, try down-left position
            if (!IsBlocked(pos)) continue; // can move left
            pos.X += 2; // can't move left, try down-right position
            if (!IsBlocked(pos)) continue; // can move right
            startingPoints.Pop(); // remove last cached starting point as it will get filled
            return true; // can't move down/left/right, the sand settles
        }

        bool IsBlocked(Vector2Int pos) => _sands.Contains(pos) || _walls.Contains(pos);
    }

    private void DrawResults()
    {
        var sb = new StringBuilder();
        var xMin = _sands.Min(s => s.X);
        var xMax = _sands.Max(s => s.X);
        for (int y = 0; y <= _maxDepth; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                var pos = new Vector2Int(x, y);
                if (x == 500 && y == 0) sb.Append('+');
                else if (y == _maxDepth || _walls.Contains(pos)) sb.Append('#');
                else if (_sands.Contains(pos)) sb.Append('o');
                else sb.Append('.');
            }
            sb.AppendLine();
        }
        _logger.Log(sb.ToString());
    }
}