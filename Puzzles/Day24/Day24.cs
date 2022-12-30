using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AoC22;

public class Day24 : Puzzle
{
    private readonly Dictionary<int, List<int>> _upCol2RowIndices = new();
    private readonly Dictionary<int, List<int>> _downCol2RowIndices = new();
    private readonly Dictionary<int, List<int>> _leftRow2ColIndices = new();
    private readonly Dictionary<int, List<int>> _rightRow2ColIndices = new();
    private Bounds _boundary;
    private Vector2Int _start, _end;
    private int _minutesTraveled = 0;

    public Day24(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var data = ReadAllLines();
        _boundary = new Bounds(0, data.Length - 3, 0, data[0].Length - 3);
        _start = new Vector2Int(_boundary.XMin - 1, _boundary.YMin);
        _end = new Vector2Int(_boundary.XMax + 1, _boundary.YMax);

        for (int row = 1; row < data.Length - 1; row++)
        {
            for (int col = 1; col < data[0].Length - 1; col++)
            {
                switch (data[row][col])
                {
                    // adjusting them all to be 0-indexed for easier modulus math later
                    case '^': AddToOrCreate(_upCol2RowIndices, col - 1, row - 1); break;
                    case 'v': AddToOrCreate(_downCol2RowIndices, col - 1, row - 1); break;
                    case '<': AddToOrCreate(_leftRow2ColIndices, row - 1, col - 1); break;
                    case '>': AddToOrCreate(_rightRow2ColIndices, row - 1, col - 1); break;
                    default: break;
                }
            }
        }

        static void AddToOrCreate(Dictionary<int, List<int>> target, int key, int value)
        {
            if (target.TryGetValue(key, out var list)) list.Add(value);
            else target.Add(key, new List<int>() { value });
        }
    }

    public override void SolvePart1() // 3.5 seconds
    {
        _minutesTraveled = FindQuickestPath(_start, _end);
        _logger.Log(_minutesTraveled); // 332
    }

    public override void SolvePart2() // 5 seconds
    {
        _minutesTraveled = FindQuickestPath(_end, _start, _minutesTraveled);
        _minutesTraveled = FindQuickestPath(_start, _end, _minutesTraveled);
        _logger.Log(_minutesTraveled); // 942
    }

    private int FindQuickestPath(Vector2Int start, Vector2Int end, int startingMinutes = 0)
    {
        List<(int Minute, Vector2Int Pos, int Cost)> toSearch = new() { (startingMinutes, start, 0) };
        HashSet<(int, Vector2Int, int)> processed = new();

        while (toSearch.Count > 0)
        {
            var current = toSearch[0];
            foreach (var next in toSearch) // TODO: try to limit the amount of items we iterate through
                if (next.Cost < current.Cost || (next.Cost == current.Cost && next.Minute < current.Minute))
                    current = next;

            toSearch.Remove(current);
            processed.Add(current);

            var nextMinute = current.Minute + 1;
            var pos = current.Pos;

            foreach (var dir in Vector2Int.CardinalDirections)
            {
                var nextPos = pos + dir;
                if (nextPos == end) return nextMinute;

                if (!_boundary.Contains(nextPos)) continue;
                var dist = nextPos.DistanceManhattanTo(end);
                var nextMove = (nextMinute, nextPos, nextMinute + dist);
                if (!IsValidMovePosition(nextPos, nextMinute)) continue;
                if (toSearch.Contains(nextMove) || processed.Contains(nextMove)) continue;
                toSearch.Add(nextMove);
            }

            var stayStill = (nextMinute, pos, current.Cost + 1);
            if (IsValidMovePosition(pos, nextMinute) && !toSearch.Contains(stayStill) && !processed.Contains(stayStill))
                toSearch.Add(stayStill);
        }
        return 0;
    }

    private bool IsValidMovePosition(Vector2Int pos, int minute)
    {
        if (_upCol2RowIndices.TryGetValue(pos.Y, out var rowIndicesUp) &&
            rowIndicesUp.Contains((pos.X + minute).Mod(_boundary.Width + 1))) return false;

        if (_downCol2RowIndices.TryGetValue(pos.Y, out var rowIndicesDown) &&
            rowIndicesDown.Contains((pos.X - minute).Mod(_boundary.Width + 1))) return false;

        if (_leftRow2ColIndices.TryGetValue(pos.X, out var colIndicesLeft) &&
            colIndicesLeft.Contains((pos.Y + minute).Mod(_boundary.Height + 1))) return false;

        if (_rightRow2ColIndices.TryGetValue(pos.X, out var colIndicesRight) &&
            colIndicesRight.Contains((pos.Y - minute).Mod(_boundary.Height + 1))) return false;

        return true;
    }

    private void DrawSceneAt(int minute)
    {
        var sb = new StringBuilder($"Minute {minute} state:\n");
        for (int row = _boundary.XMin; row <= _boundary.XMax; row++)
        {
            for (int col = _boundary.YMin; col <= _boundary.YMax; col++)
                sb.Append(IsValidMovePosition(new(row, col), minute) ? '.' : 'X');
            sb.AppendLine();
        }
        _logger.Log(sb.ToString());
    }
}