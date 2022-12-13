using System.Collections.Generic;
using System.Numerics;

namespace AoC22;

public class Day12 : Puzzle
{
    private Grid<Point> _grid;
    private readonly List<Point> _lowPoints = new();
    private Point _startPoint;
    private Point _endPoint;

    public Day12(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var data = ReadAllLines();
        var rows = data.Length;
        var cols = data[0].Length;
        _grid = new Grid<Point>(rows, cols);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var c = data[row][col];
                var height = c switch
                {
                    'S' => 'a',
                    'E' => 'z',
                    _ => c
                };

                var pos = new Vector2Int(row, col);
                var point = new Point(pos, height);
                _grid.Add(row, col, point);

                if (height == 'a') _lowPoints.Add(point);
                if (c == 'S') _startPoint = point;
                else if (c == 'E') _endPoint = point;
            }
        }
    }

    public override void SolvePart1()
    {
        foreach (var point in _grid) point.InitNeighbors(_grid, point.IsValidNeighbor);
        var path = Pathfinding.FindPath(_startPoint, _endPoint);
        _logger.Log(path.Count);
    }

    public override void SolvePart2()
    {
        // Reassign the neighbors to do a floodfill starting from the End point until we reach an 'a'.
        foreach (var point in _grid) point.InitNeighbors(_grid, point.IsValidNeighborReverse);
        var path = Pathfinding.FloodFillUntil(_endPoint, p => p.Value == 'a');
        _logger.Log(path.Count);
    }

    private class Point : Node
    {
        public char Value { get; set; } // Height
        public Point(Vector2Int pos, char value, int cost = 10) : base(pos, cost) { Value = value; }
        public bool IsValidNeighbor(Point other) => other.Value - Value <= 1;
        public bool IsValidNeighborReverse(Point other) => other.Value - Value >= -1;
    }
}