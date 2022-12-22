using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC22;

public class Day16 : Puzzle
{
    private readonly Dictionary<string, Valve> _valves = new();
    private readonly Dictionary<string, Dictionary<string, (int Dist, string Next)>> _distanceMap = new();
    // Used for pruning branches
    private readonly Dictionary<string, int> _snapshots = new();
    private int _currentMax = 0;

    public Day16(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var pattern = Utils.NumberPattern();
        foreach (var line in ReadFromFile())
        {
            var valveId = line[6..8];
            var flowRate = int.Parse(pattern.Match(line).ValueSpan);
            var targetIds = line[49..].Split(',', StringSplitOptions.TrimEntries);

            var valve = new Valve(valveId, flowRate, targetIds);
            _valves[valveId] = valve;
            _distanceMap.Add(valveId, new());
        }

        foreach (var mapping in _distanceMap)
        {
            //mapping.Value[mapping.Key] = (0, mapping.Key); // no point to include self-travel
            foreach (var firstStep in _valves[mapping.Key].NextValves)
                SearchDeeper(mapping.Key, mapping.Value, firstStep, firstStep);
        }

        void SearchDeeper(string key, Dictionary<string, (int Dist, string Next)> map, string firstStep, string target, int distance = 0)
        {
            distance++;
            if (map.TryGetValue(target, out var value) && value.Dist <= distance) return;
            map[target] = (distance, firstStep);

            foreach (var neighbor in _valves[target].NextValves)
                SearchDeeper(key, map, firstStep, neighbor, distance);
        }
    }

    public override void SolvePart1()
    {
        ResetPruningHelpers();
        var score = TakePath(30, "AA", new List<string>());
        _logger.Log(score);
    }

    public override void SolvePart2()
    {
        ResetPruningHelpers();
        var score = TakeTwoPaths(26, "AA", "AA", new List<string>());
        _logger.Log(score);
    }

    private void ResetPruningHelpers()
    {
        _snapshots.Clear();
        _currentMax = 0;
    }

    private int TakePath(int minutes, string current, List<string> opened, int totalPressure = 0)
    {
        if (totalPressure > _currentMax) _currentMax = totalPressure;
        // Pruning weak branches
        if (MaxPressure(minutes, opened, totalPressure) < _currentMax) return totalPressure;
        var snapshot = opened.Order().Aggregate("", (a, b) => $"{a}{b}");
        if (_snapshots.TryGetValue(snapshot, out var value) && totalPressure < value) return totalPressure;
        _snapshots[snapshot] = totalPressure;

        if (!TryGetTargets(minutes, current, opened, out var targets)) return totalPressure;

        int bestPathScore = totalPressure;
        foreach (var next in targets)
        {
            bestPathScore = Math.Max(bestPathScore,
                TakePath(minutes - next.Value.Dist - 1, next.Key, new List<string>(opened) { next.Key }, totalPressure + ScoreFor(current, next.Key, minutes)));
        }
        return bestPathScore;
    }

    private int TakeTwoPaths(int minutes, string current1, string current2, List<string> opened, string target1 = "", string target2 = "", int totalPressure = 0)
    {
        if (totalPressure > _currentMax) _currentMax = totalPressure;
        // Pruning weak branches
        var maxPressure = MaxPressure(minutes, opened, totalPressure, current1, current2);
        if (maxPressure < _currentMax) return totalPressure;
        var snapshot = opened.Order().Aggregate("", (a, b) => $"{a}{b}");
        if (_snapshots.TryGetValue(snapshot, out var value) && maxPressure < value) return totalPressure;
        _snapshots[snapshot] = maxPressure;

        int bestPathScore = totalPressure;
        if (string.IsNullOrEmpty(target1) && !string.IsNullOrEmpty(current1) && TryGetTargets(minutes, current1, opened, out var targets1, target2))
        {
            if (string.IsNullOrEmpty(target2) && !string.IsNullOrEmpty(current2) && TryGetTargets(minutes, current2, opened, out var targets2, target1))
            {
                foreach (var targetFor1 in targets1)
                {
                    var target1Dist = targetFor1.Value.Dist;
                    foreach (var targetFor2 in targets2)
                    {
                        if (targetFor1.Key == targetFor2.Key) continue;
                        var target2Dist = targetFor2.Value.Dist;
                        bestPathScore = Math.Max(bestPathScore, Recurse(minutes, current1, current2, targetFor1.Key, targetFor2.Key, opened, target1Dist, target2Dist, totalPressure));
                    }
                }
            }
            else
            {
                var target2Dist = DistanceBetween(current2, target2);
                foreach (var targetFor1 in targets1)
                {
                    var target1Dist = targetFor1.Value.Dist;
                    bestPathScore = Math.Max(bestPathScore, Recurse(minutes, current1, current2, targetFor1.Key, target2, opened, target1Dist, target2Dist, totalPressure));
                }
            }
        }
        else if (string.IsNullOrEmpty(target2) && !string.IsNullOrEmpty(current2) && TryGetTargets(minutes, current2, opened, out var targets2, target1))
        {
            var target1Dist = DistanceBetween(current1, target1);
            foreach (var targetFor2 in targets2)
            {
                var target2Dist = targetFor2.Value.Dist;
                bestPathScore = Math.Max(bestPathScore, Recurse(minutes, current1, current2, target1, targetFor2.Key, opened, target1Dist, target2Dist, totalPressure));
            }
        }
        // finish opening up any current final targets
        else if (!string.IsNullOrEmpty(target1))
        {
            var target1Dist = DistanceBetween(current1, target1);
            bestPathScore = Math.Max(bestPathScore, Recurse(minutes, current1, current2, target1, target2, opened, target1Dist, int.MaxValue, totalPressure));
        }
        else if (!string.IsNullOrEmpty(target2))
        {
            var target2Dist = DistanceBetween(current2, target2);
            bestPathScore = Math.Max(bestPathScore, Recurse(minutes, current1, current2, target1, target2, opened, int.MaxValue, target2Dist, totalPressure));
        }
        return bestPathScore;
    }

    // this always sets 1 or both targets to "" so that we can try to get a new target next cycle
    private int Recurse(int minutes, string current1, string current2, string targetFor1, string targetFor2, List<string> opened, int target1Dist, int target2Dist, int totalPressure)
    {
        if (target1Dist == target2Dist)
        {
            // subtract the same amount from minutes
            var updatedOpened = new List<string>(opened) { targetFor1, targetFor2 };
            var updatedScore = totalPressure + ScoreFor(current1, targetFor1, minutes) + ScoreFor(current2, targetFor2, minutes);
            return TakeTwoPaths(minutes - target1Dist - 1, targetFor1, targetFor2, updatedOpened, totalPressure: updatedScore);
        }
        else if (target1Dist < target2Dist)
        {
            // subtract just target 1's distance from minutes
            var updatedCurrent2 = StepTowards(current2, targetFor2, target1Dist + 1);
            var updatedOpened = new List<string>(opened) { targetFor1 };
            var updatedScore = totalPressure + ScoreFor(current1, targetFor1, minutes);
            return TakeTwoPaths(minutes - target1Dist - 1, targetFor1, updatedCurrent2, updatedOpened, target2: targetFor2, totalPressure: updatedScore);
        }
        else
        {
            // subtract just target 2's distance from minutes
            var updatedCurrent1 = StepTowards(current1, targetFor1, target2Dist + 1);
            var updatedOpened = new List<string>(opened) { targetFor2 };
            var updatedScore = totalPressure + ScoreFor(current2, targetFor2, minutes);
            return TakeTwoPaths(minutes - target2Dist - 1, updatedCurrent1, targetFor2, updatedOpened, target1: targetFor1, totalPressure: updatedScore);
        }
    }

    private bool TryGetTargets(int minutesRemaining, string current, List<string> opened, out IEnumerable<KeyValuePair<string, (int Dist, string Next)>> targets, string otherTarget = "")
    {
        targets = _distanceMap[current]
            .Where(kvp => FlowRateOf(kvp.Key) > 0 && kvp.Value.Dist < minutesRemaining && !opened.Contains(kvp.Key) && kvp.Key != otherTarget)
            .OrderByDescending(kvp => TargetScore(current, kvp.Key, minutesRemaining));
        return targets.Any();
    }

    private string StepTowards(string current, string target, int steps)
    {
        if (string.IsNullOrEmpty(target)) return string.Empty;

        if (DistanceBetween(current, target) <= steps) return target;
        for (int i = 0; i < steps; i++)
            current = _distanceMap[current][target].Next;
        return current;
    }

    private int MaxPressure(int minutesRemaining, List<string> opened, int totalPressure)
    {
        int excess = 0;
        foreach (var kvp in _valves)
        {
            if (kvp.Value.FlowRate == 0 || opened.Contains(kvp.Key)) continue;
            excess += kvp.Value.FlowRate * (minutesRemaining - 2);
        }
        return totalPressure + excess;
    }

    private int MaxPressure(int minutesRemaining, List<string> opened, int totalPressure, string current1, string current2)
    {
        int excess = 0;
        foreach (var kvp in _valves)
        {
            if (kvp.Value.FlowRate == 0 || opened.Contains(kvp.Key)) continue;
            var score1 = ScoreFor(current1, kvp.Key, minutesRemaining);
            var score2 = ScoreFor(current2, kvp.Key, minutesRemaining);
            excess += Math.Max(score1, score2);
        }
        return totalPressure + excess;
    }

    // this divides by the distance for a "score per step" scaling to choose the higher value option
    private float TargetScore(string current, string target, int minutes)
    {
        var distance = DistanceBetween(current, target);
        var score = ScoreFor(current, target, minutes);
        return score / (float)distance;
    }

    private int ScoreFor(string current, string target, int minutesRemaining)
    {
        if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(target)) return 0;

        var distance = DistanceBetween(current, target);
        var score = (minutesRemaining - distance - 1) * FlowRateOf(target);
        return score;
    }

    private int DistanceBetween(string from, string to) => string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) ? int.MaxValue : from == to ? 0 : _distanceMap[from][to].Dist;

    private int FlowRateOf(string valve) => _valves[valve].FlowRate;

    private class Valve
    {
        public string Id;
        public int FlowRate;
        public string[] NextValves;

        public Valve(string id, int flowRate, string[] targetIds)
        {
            Id = id;
            FlowRate = flowRate;
            NextValves = targetIds;
        }
    }
}