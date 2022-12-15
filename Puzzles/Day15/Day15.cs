using System;
using System.Collections.Generic;
using System.Numerics;

namespace AoC22;

public class Day15 : Puzzle
{
    private readonly HashSet<Reading> _readings = new();
    private const int ROW = 10; // 2_000_000; // for tests, use 10
    private const int REGION = 20; // 4_000_000; // for tests, use 20

    public Day15(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var pattern = Utils.NumberPattern();

        foreach (var line in ReadFromFile())
        {
            var matches = pattern.Matches(line);
            var sensor = new Vector2Int(int.Parse(matches[0].ValueSpan), int.Parse(matches[1].ValueSpan));
            var beacon = new Vector2Int(int.Parse(matches[2].ValueSpan), int.Parse(matches[3].ValueSpan));
            _readings.Add(new Reading(sensor, beacon));
        }
    }

    public override void SolvePart1()
    {
        HashSet<int> occupiedAlongRow = new();
        HashSet<int> beaconsAlongRow = new();
        
        foreach (var reading in _readings)
        {
            if (reading.Beacon.Y == ROW)
                beaconsAlongRow.Add(reading.Beacon.X);

            var deltaY = Math.Abs(ROW - reading.Sensor.Y);
            var remainingSteps = reading.Distance - deltaY;
            for (int x = reading.Sensor.X - remainingSteps; x <= reading.Sensor.X + remainingSteps; x++)
                occupiedAlongRow.Add(x);
        }

        _logger.Log(occupiedAlongRow.Count - beaconsAlongRow.Count);
    }

    public override void SolvePart2()
    {
        var beacon = FindBeacon();
        
        var tuningFrequency = beacon.X * 4_000_000L + beacon.Y;
        
        _logger.Log(tuningFrequency);
    }

    private Vector2Int FindBeacon()
    {
        for (int y = 0; y <= REGION; y++)
        {
            for (int x = 0; x <= REGION; x++)
            {
                var pos = new Vector2Int(x, y);
                bool hasFoundBeacon = true;
                foreach (var reading in _readings)
                {
                    if (pos.DistanceManhattanTo(reading.Sensor) <= reading.Distance)
                    {
                        var deltaY = Math.Abs(pos.Y - reading.Sensor.Y);
                        var remainingDist = reading.Distance - deltaY;
                        x = reading.Sensor.X + remainingDist; // take big steps
                        // x++ will put us outside the range of this sensor and into the next
                        hasFoundBeacon = false;
                        break;
                    }
                }
                if (hasFoundBeacon) return pos;
            }
        }
        return Vector2Int.Zero;
    }

    private class Reading
    {
        public Vector2Int Sensor;
        public Vector2Int Beacon;
        public int Distance;

        public Reading(Vector2Int sensor, Vector2Int beacon) : this(sensor, beacon, sensor.DistanceManhattanTo(beacon)) { }
        public Reading(Vector2Int sensor, Vector2Int beacon, int distance)
        {
            Sensor = sensor;
            Beacon = beacon;
            Distance = distance;
        }
    }
}