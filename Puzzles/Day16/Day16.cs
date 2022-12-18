using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC22;

public class Day16 : Puzzle
{
    private readonly Dictionary<string, Valve> _valves = new();
    private readonly Dictionary<string, Dictionary<string, (int Dist, string Next)>> _distanceMap = new();

    private int _valvesWithFlow;

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

            if (flowRate > 0) _valvesWithFlow++;
        }

        foreach (var mapping in _distanceMap)
        {
            //mapping.Value[mapping.Key] = (0, mapping.Key);
            foreach (var firstStep in _valves[mapping.Key].NextValves)
                SearchDeeper(mapping.Key, mapping.Value, firstStep, firstStep);
        }
    }

    private void SearchDeeper(string key, Dictionary<string, (int Dist, string Next)> map, string firstStep, string target, int distance = 0)
    {
        distance++;
        if (map.TryGetValue(target, out var value) && value.Dist <= distance) return;
        map[target] = (distance, firstStep);

        foreach (var neighbor in _valves[target].NextValves)
            SearchDeeper(key, map, firstStep, neighbor, distance);
    }

    public override void SolvePart1() => _logger.Log(TakePath(30, "AA", new List<string>()));

    private int TakePath(int minutes, string current, List<string> opened, int totalPressure = 0)
    {
        if (opened.Count == _valvesWithFlow) return totalPressure;

        int bestPathScore = 0;
        var targets = _distanceMap[current].
                Where(kvp => /*kvp.Key != current &&*/ FlowRateOf(kvp.Key) > 0 && kvp.Value.Dist < minutes && !opened.Contains(kvp.Key)).
                OrderByDescending(kvp => TargetScore(current, kvp.Key, minutes));

        if (!targets.Any()) return totalPressure;

        foreach (var next in targets)
        {
            bestPathScore = Math.Max(bestPathScore, 
                TakePath(minutes - next.Value.Dist - 1, next.Key, new List<string>(opened) { next.Key }, totalPressure + TargetScore(current, next.Key, minutes)));
        }
        return bestPathScore;
    }

    public override void SolvePart2()
    {
        _logger.Log(TakeTwoPaths(26, "AA", "AA", new List<string>()));
        _logger.Log(1707);
    }

    private int TakeTwoPaths(int minutes, string current1, string current2, List<string> opened, string target1 = "", string target2 = "",  int totalPressure = 0)
    {
        if (opened != null && opened.Count == _valvesWithFlow) return totalPressure;
        
        int bestPathScore = 0;


        if (string.IsNullOrEmpty(target1))
        {
            var targets1 = _distanceMap[current1].
                Where(kvp => FlowRateOf(kvp.Key) > 0 && kvp.Value.Dist < minutes && !opened.Contains(kvp.Key) && kvp.Key != target2).
                OrderByDescending(kvp => TargetScore(current1, kvp.Key, minutes));

            if (string.IsNullOrEmpty(target2))
            {
                var targets2 = _distanceMap[current2].
                    Where(kvp => FlowRateOf(kvp.Key) > 0 && kvp.Value.Dist < minutes && !opened.Contains(kvp.Key)).
                    OrderByDescending(kvp => TargetScore(current1, kvp.Key, minutes));

                if (!targets1.Any() && !targets2.Any()) return totalPressure;

                foreach (var targetFor1 in targets1)
                {
                    foreach (var targetFor2 in targets2)
                    {
                        if (targetFor1.Key == targetFor2.Key) continue;

                        var target1Dist = targetFor1.Value.Dist;
                        var target2Dist = targetFor2.Value.Dist;

                        if (target1Dist == target2Dist)
                        {
                            // subtract the same amount from minutes
                            var updatedList = new List<string>(opened) { targetFor1.Key, targetFor2.Key };
                            var updatedScore = totalPressure + TargetScore(current1, targetFor1.Key, minutes) + TargetScore(current2, targetFor2.Key, minutes);
                            bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, targetFor1.Key, targetFor2.Key, updatedList, totalPressure: updatedScore));
                        }
                        else if (target1Dist < target2Dist)
                        {
                            // subtract just target 1's distance from minutes
                            var updatedList = new List<string>(opened) { targetFor1.Key };
                            var updatedScore = totalPressure + TargetScore(current1, targetFor1.Key, minutes);
                            var updatedCurrent2 = StepTowards(current2, targetFor2.Key, target1Dist + 1);
                            bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, targetFor1.Key, updatedCurrent2, updatedList, target2: targetFor2.Key, totalPressure: updatedScore));
                        }
                        else 
                        {
                            // subtract just target 2's distance from minutes
                            var updatedList = new List<string>(opened) { targetFor2.Key };
                            var updatedScore = totalPressure + TargetScore(current2, targetFor2.Key, minutes);
                            var updatedCurrent1 = StepTowards(current1, targetFor1.Key, target2Dist + 1);
                            bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target2Dist - 1, updatedCurrent1, targetFor2.Key, updatedList, target1: targetFor1.Key, totalPressure: updatedScore));
                        }
                    }
                }
            }
            else
            {
                var target2Dist = DistanceBetween(current2, target2);

                foreach (var targetFor1 in targets1)
                {
                    var target1Dist = targetFor1.Value.Dist;

                    if (target1Dist == target2Dist)
                    {
                        // subtract the same amount from minutes
                        var updatedList = new List<string>(opened) { targetFor1.Key, target2 };
                        var updatedScore = totalPressure + TargetScore(current1, targetFor1.Key, minutes) + TargetScore(current2, target2, minutes);
                        bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, targetFor1.Key, target2, updatedList, totalPressure: updatedScore));
                    }
                    else if (target1Dist < target2Dist)
                    {
                        // subtract just target 1's distance from minutes
                        var updatedList = new List<string>(opened) { targetFor1.Key };
                        var updatedScore = totalPressure + TargetScore(current1, targetFor1.Key, minutes);
                        var updatedCurrent2 = StepTowards(current2, target2, target1Dist + 1);
                        bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, targetFor1.Key, updatedCurrent2, updatedList, target2: target2, totalPressure: updatedScore));
                    }
                    else
                    {
                        // subtract just target 2's distance from minutes
                        var updatedList = new List<string>(opened) { target2 };
                        var updatedScore = totalPressure + TargetScore(current2, target2, minutes);
                        var updatedCurrent1 = StepTowards(current1, targetFor1.Key, target2Dist + 1);
                        bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target2Dist - 1, updatedCurrent1, target2, updatedList, target1: targetFor1.Key, totalPressure: updatedScore));
                    }
                }
            }
        }
        else if (string.IsNullOrEmpty(target2))
        {
            var targets2 = _distanceMap[current2].
                    Where(kvp => FlowRateOf(kvp.Key) > 0 && kvp.Value.Dist < minutes && !opened.Contains(kvp.Key) && kvp.Key != target1).
                    OrderByDescending(kvp => TargetScore(current1, kvp.Key, minutes));

            if (!targets2.Any())
            {
                // TODO: finish up current target1's objective
                return totalPressure;
            }

            foreach (var targetFor2 in targets2)
            {
                var target1Dist = DistanceBetween(current1, target1);
                var target2Dist = targetFor2.Value.Dist;

                if (target1Dist == target2Dist)
                {
                    // subtract the same amount from minutes
                    var updatedList = new List<string>(opened) { target1, targetFor2.Key };
                    var updatedScore = totalPressure + TargetScore(current1, target1, minutes) + TargetScore(current2, targetFor2.Key, minutes);
                    bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, target1, targetFor2.Key, updatedList, totalPressure: updatedScore));
                }
                else if (target1Dist < target2Dist)
                {
                    // subtract just target 1's distance from minutes
                    var updatedList = new List<string>(opened) { target1 };
                    var updatedScore = totalPressure + TargetScore(current1, target1, minutes);
                    var updatedCurrent2 = StepTowards(current2, targetFor2.Key, target1Dist + 1);
                    bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, target1, updatedCurrent2, updatedList, target2: targetFor2.Key, totalPressure: updatedScore));
                }
                else
                {
                    // subtract just target 2's distance from minutes
                    var updatedList = new List<string>(opened) { targetFor2.Key };
                    var updatedScore = totalPressure + TargetScore(current2, targetFor2.Key, minutes);
                    var updatedCurrent1 = StepTowards(current1, target1, target2Dist + 1);
                    bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target2Dist - 1, updatedCurrent1, targetFor2.Key, updatedList, target1: target1, totalPressure: updatedScore));
                }
            }
        }
        
        return bestPathScore;
    }

    private void SolvePart2Method()
    {
        int totalPressure = 0;
        List<string> openedValves = new();
        string current1 = "AA";
        string current2 = "AA";
        string target1 = string.Empty;
        string target2 = string.Empty;

        HashSet<string> target1Path = new();
        HashSet<string> target2Path = new();

        for (int minutes = 26; minutes > 0; minutes--)
        {
            if (openedValves.Count == _valvesWithFlow) break;

            // get the cost to the top two next nodes
            // compare against the cost difference if the other runner took your best one and you took the second best instead

            if (string.IsNullOrEmpty(target1))
            {
                target1 = _distanceMap[current1].
                        Where(kvp => !openedValves.Contains(kvp.Key) && kvp.Key != target2 && _valves[kvp.Key].FlowRate > 0).
                        OrderByDescending(kvp => TargetScore(current1, kvp.Key, minutes)).
                        Select(kvp => kvp.Key).
                        FirstOrDefault();
                if (string.IsNullOrEmpty(target2))
                {
                    // get list, compare against target1's list to see if we have a better score
                    target2 = _distanceMap[current2].
                    Where(kvp => !openedValves.Contains(kvp.Key) && kvp.Key != target1 && _valves[kvp.Key].FlowRate > 0).
                    OrderByDescending(kvp => TargetScore(current2, kvp.Key, minutes)).
                    FirstOrDefault().Key;
                    target2Path.Add(target2);
                    _logger.Log($"Target 2 Selected {target2}");
                }
                // take top from list
                target1Path.Add(target1);
                _logger.Log($"Target 1 Selected {target1}");
            }
            else if (string.IsNullOrEmpty(target2))
            {
                target2 = _distanceMap[current2].
                    Where(kvp => !openedValves.Contains(kvp.Key) && kvp.Key != target1 && _valves[kvp.Key].FlowRate > 0).
                    OrderByDescending(kvp => TargetScore(current2, kvp.Key, minutes)).
                    FirstOrDefault().Key;
                target2Path.Add(target2);
                _logger.Log($"Target 2 Selected {target2}");
            }

            if (current1 == target1 && !openedValves.Contains(target1) && _valves[target1].FlowRate > 0)
            {
                openedValves.Add(target1);
                totalPressure += _valves[target1].FlowRate * (minutes - 1);
                target1 = string.Empty;
            }
            else if (!string.IsNullOrEmpty(target1))
            {
                current1 = _distanceMap[current1][target1].Next;
            }

            if (current2 == target2 && !openedValves.Contains(target2) && _valves[target2].FlowRate > 0)
            {
                openedValves.Add(target2);
                totalPressure += _valves[target2].FlowRate * (minutes - 1);
                target2 = string.Empty;
            }
            else if (!string.IsNullOrEmpty(target2))
            {
                current2 = _distanceMap[current2][target2].Next;
            }
        }

        _logger.Log(target1Path.Aggregate("", (a, b) => $"{a}{b},"));
        _logger.Log(target2Path.Aggregate("", (a, b) => $"{a}{b},"));

        _logger.Log(totalPressure);

    }

   
    private string StepTowards(string current, string target, int steps)
    {
        if (DistanceBetween(current, target) <= steps) return target;
        else
        {
            for (int i = 0; i < steps; i++)
                current = _distanceMap[current][target].Next;
        }
        return current;
    }

    private int TargetScore(string current, string target, int minutes)
    {
        var distance = DistanceBetween(current, target);
        var score = (minutes - distance - 1) * FlowRateOf(target);
        return score;
    }

    private int DistanceBetween(string from, string to) => from == to ? 0 : _distanceMap[from][to].Dist;
    
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