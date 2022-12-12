using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC22;

public class Day12 : Puzzle
{
    private readonly Dictionary<Vector2Int, Point> _points = new();
    private Point _startPoint;
    private Point _endPoint;

    public Day12(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        int row = 0;
        foreach (var line in ReadFromFile())
        {
            for (int column = 0; column < line.Length; column++)
            {
                var height = line[column];
                var pos = new Vector2Int(column, row);
                var point = new Point(pos, value: height);
                _points.Add(pos, point);
                if (height == 'S')
                {
                    point.Value = 'a';
                    _startPoint = point;
                }
                else if (height == 'E')
                {
                    point.Value = 'z';
                    _endPoint = point;
                }
            }
            row++;
        }

        foreach (var point in _points.Values)
            point.FindAndAddNeighbors(_points);
    }

    public override void SolvePart1()
    {
        var path = Pathfinding.FindPath(_startPoint, _endPoint);
        _logger.Log(path.Count);
    }

    public override void SolvePart2()
    {
        int minTravel = int.MaxValue;
        var lowPoints = _points.Values.Where(a => a.Value == 'a');
        foreach (var lowPoint in lowPoints.Where(a => a.Neighbors.Any(n => n is Point b && b.Value > a.Value)))
        {
            var path = Pathfinding.FindPath(lowPoint, _endPoint);
            if (path.Count < minTravel && path.Count != 0)
                minTravel = path.Count;
        }
        _logger.Log(minTravel);
    }

    private class Point : Node
    {
        public char Value { get; set; }
        public Point(Vector2Int pos, char value, int cost = 1) : base(pos, cost) { Value = value; }
        protected override bool IsValidNeighbor<T>(T other) => other is Point p && p.Value - Value <= 1; // Adjacency check handled in FindAndAddNeighbors base method
        public override int GetDistance<Point>(Point target) => Pos.DistanceChebyshevTo(target.Pos);
    }
}