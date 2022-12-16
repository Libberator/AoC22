using System;
using System.Collections.Generic;
using System.Numerics;

namespace AoC22;

public class Day15 : Puzzle
{
    private readonly List<Data> _data = new();
    //private const int ROW = 2_000_000; // use 2 million for real input data
    private const int ROW = 10; // use 10 for test data

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
        var tuningFrequency = beacon.X * 4_000_000L + beacon.Y;
        _logger.Log(tuningFrequency);
    }

    // This takes advantage of the fact that the hidden beacon must be just outside the edge of the sensor's range
    // Specifically, there will be 2 pairs of sensors to cover each side of the hidden beacon
    // Using the lines along the gaps between the pairs (they will be perpendicular), X marks the spot
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
                if (distanceApart - (first.Distance + 1) != second.Distance + 1) continue;

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

                var intersect = Vector2Int.GetLineIntersect(PointA, SlopeA, PointB, SlopeB);

                if (IsValidBeaconPos(intersect)) return intersect;
            }
        }
        return Vector2Int.Zero;
    }

    // just in case there are more potential lines in the data set that are red herrings
    private bool IsValidBeaconPos(Vector2Int pos)
    {
        foreach (var data in _data)
            if (data.Sensor.DistanceManhattanTo(pos) <= data.Distance) return false;
        return true;
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