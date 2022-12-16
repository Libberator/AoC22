using System;
using System.Collections.Generic;
using System.Numerics;

namespace AoC22;

public class Day15 : Puzzle
{
    private readonly List<Data> _data = new();
    private const int ROW = 2_000_000; // for tests, use 10

    public Day15(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var pattern = Utils.NumberPattern();

        foreach (var line in ReadFromFile())
        {
            var matches = pattern.Matches(line);
            var sensor = new Vector2Int(int.Parse(matches[0].ValueSpan), int.Parse(matches[1].ValueSpan));
            var beacon = new Vector2Int(int.Parse(matches[2].ValueSpan), int.Parse(matches[3].ValueSpan));
            _data.Add(new Data(sensor, beacon, sensor.DistanceManhattanTo(beacon)));
        }
    }

    public override void SolvePart1()
    {
        HashSet<int> beaconsAlongRow = new();
        SortedList<int, int> minMaxRanges = new();

        foreach (var data in _data)
        {
            if (data.Beacon.Y == ROW)
                beaconsAlongRow.Add(data.Beacon.X);

            var deltaY = Math.Abs(ROW - data.Sensor.Y);
            if (data.Distance < deltaY) continue; // doesn't touch the ROW

            var minX = data.Sensor.X - (data.Distance - deltaY);
            var maxX = data.Sensor.X + (data.Distance - deltaY);

            if (minMaxRanges.TryGetValue(minX, out int value))
                minMaxRanges[minX] = Math.Max(maxX, value);
            else
                minMaxRanges.Add(minX, maxX);
        }

        int occupiedCount = 0;
        int x = int.MinValue;
        foreach (var minMax in minMaxRanges)
        {
            x = Math.Max(x, minMax.Key); // shift the current position up to the next minimum
            var max = minMax.Value;
            if (x <= max)
            {
                occupiedCount += max - x + 1;
                x = max + 1;
            }
        }

        _logger.Log(occupiedCount - beaconsAlongRow.Count);
    }

    public override void SolvePart2()
    {
        var beacon = FindBeacon();
        //_logger.Log($"Hidden beacon at: {beacon}");
        var tuningFrequency = beacon.X * 4_000_000L + beacon.Y;
        _logger.Log(tuningFrequency);
    }

    // This takes advantage of the fact that the hidden beacon must be on the outer edge of a sensor's range
    // Specifically there will be 2 pairs of sensors to cover each side of the hidden beacon
    // We take the two perpendicular lines between the pairs and X marks the spot
    private Vector2Int FindBeacon()
    {
        List<(Vector2Int Point, int Slope)> lines = new();

        for (int i = 0; i < _data.Count - 1; i++)
        {
            var first = _data[i];
            for (int j = i + 1; j < _data.Count; j++)
            {
                var second = _data[j];
                var distanceApart = first.Sensor.DistanceManhattanTo(second.Sensor);
                if (distanceApart - 1 != first.Distance + second.Distance + 1) continue;

                var slope = (first.Sensor.X < second.Sensor.X) ^ (first.Sensor.Y < second.Sensor.Y) ? 1 : -1;
                var point = first.Sensor.X < second.Sensor.X ?
                    first.Sensor + (first.Distance + 1) * Vector2Int.Right :
                    second.Sensor + (second.Distance + 1) * Vector2Int.Right;

                lines.Add((point, slope));
            }
        }

        for (int i = 0; i < lines.Count - 1; i++)
        {
            var (PointA, SlopeA) = lines[i];
            for (int j = i + 1; j < lines.Count; j++)
            {
                var (PointB, SlopeB) = lines[j];
                if (SlopeA == SlopeB) continue;

                var intersect = GetLineIntersect(PointA, SlopeA, PointB, SlopeB);

                if (IsValidBeaconPos(intersect)) return intersect;
            }
        }

        // just in case there are more potential lines in the data set that are red herrings
        bool IsValidBeaconPos(Vector2Int pos)
        {
            foreach (var data in _data)
                if (data.Sensor.DistanceManhattanTo(pos) <= data.Distance) return false;
            return true;
        }

        return Vector2Int.Zero;
    }

    // Given two lines that each passes through a point with a slope, return their intersection.
    // This is simplified for integers and a known diagonal slope of 1 or -1
    private static Vector2Int GetLineIntersect(Vector2Int pt1, int slope1, Vector2Int pt2, int slope2)
    {
        if (slope1 == slope2) throw new Exception("Lines are parallel and don't have a single intersect");

        var x = (pt1.X - slope1 * pt1.Y + pt2.X - slope2 * pt2.Y) / 2;
        var y = (-slope1 * pt1.X + pt1.Y - slope2 * pt2.X + pt2.Y) / 2;
        return new Vector2Int(x, y);
    }

    private class Data
    {
        public Vector2Int Sensor;
        public Vector2Int Beacon;
        public int Distance;
        public Data(Vector2Int sensor, Vector2Int beacon, int distance)
        {
            Sensor = sensor;
            Beacon = beacon;
            Distance = distance;
        }
    }
}