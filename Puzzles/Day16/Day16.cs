using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public override void SolvePart1()
    {
        var timer = new Stopwatch();
        timer.Start();
        var score = TakePath(30, "AA", new List<string>());
        timer.Stop();
        _logger.Log($"Part 1 took {timer.ElapsedMilliseconds} ms");
        _logger.Log(score);
    }

    private int TakePath(int minutes, string current, List<string> opened, int totalPressure = 0)
    {
        if (opened.Count == _valvesWithFlow) return totalPressure;

        var targets = _distanceMap[current].
                Where(kvp => FlowRateOf(kvp.Key) > 0 && kvp.Value.Dist < minutes && !opened.Contains(kvp.Key)).
                OrderByDescending(kvp => TargetScore(current, kvp.Key, minutes)); // Note: Adjusted the comparison to divide by distance for a "score per step" scaling

        if (!targets.Any()) return totalPressure;

        int bestPathScore = 0;
        foreach (var next in targets)
        {
            bestPathScore = Math.Max(bestPathScore, 
                TakePath(minutes - next.Value.Dist - 1, next.Key, new List<string>(opened) { next.Key }, totalPressure + ScoreFor(current, next.Key, minutes)));
        }
        return bestPathScore;
    }

    public override void SolvePart2()
    {
        var timer = new Stopwatch();

        timer.Start();
        var score = TakeTwoPaths(26, "AA", "AA", new List<string>());
        timer.Stop();
        _logger.Log($"Part 2 took {timer.ElapsedMilliseconds}ms");
        _logger.Log(score); // 2591 is the expected answer
        //_logger.Log(1707);
    }
    
    // dictionary
    
    // strings for each node-pair (from A to B) every step
    // if we go with this, then we compare choice values
    // this may only create a heatmap of what paths are just more likely to go down, and not necessarily about
    // what path is actually better based on score. Each Node should compare incoming values (~~with their current value~~) and take the Max
    // then we send our current value, plus the list of opened nodes to each next (aka previous) possible nodes
    // limit the search space so that it's just a sparse graph of possible connections

    // every node knows what paths can come to it
    // every node carries a list of node IDs it has personally opened that it adds to when it chooses the path of opening the node

    // incoming values and paths
    // append your node to it
    // pass it along
    // when you receive a bunch of nodes (values and paths), keep the largest (??) and pass that along only


    // they won't have whether or not they've been opened, they all just start opened, pass along their current value, which is the sum
    // of the minutes into the simulation * the distance to travel between each node
    
    // Potential Idea to implement
    //private void ReverseWalkThePaths()
    //{

    //}
    
    
    private int _currentLeadingPressure = 1000;
    private int TakeTwoPaths(int minutes, string current1, string current2, List<string> opened, string target1 = "", string target2 = "", int totalPressure = 0)
    {
        if (totalPressure > _currentLeadingPressure)
        {
            var path = opened.Aggregate("", (a, b) => $"{a}{b},");
            _logger.Log($"{totalPressure} - {path}");
            _currentLeadingPressure = totalPressure;
        }

        if (opened.Count == _valvesWithFlow) return totalPressure;

        int bestPathScore = 0;

        if (string.IsNullOrEmpty(target1) && TryGetTargets(minutes, current1, opened, target2, out var targets1))
        {
            if (string.IsNullOrEmpty(target2) && TryGetTargets(minutes, current2, opened, target1, out var targets2))
            {
                foreach (var targetFor1 in targets1)
                {
                    var target1Dist = targetFor1.Value.Dist;
                    foreach (var targetFor2 in targets2)
                    {
                        if (targetFor1.Key == targetFor2.Key) continue;

                        var target2Dist = targetFor2.Value.Dist;

                        if (target1Dist == target2Dist)
                        {
                            // subtract the same amount from minutes
                            var updatedList = new List<string>(opened) { targetFor1.Key, targetFor2.Key };
                            var updatedScore = totalPressure + ScoreFor(current1, targetFor1.Key, minutes) + ScoreFor(current2, targetFor2.Key, minutes);
                            bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, targetFor1.Key, targetFor2.Key, updatedList, totalPressure: updatedScore));
                        }
                        else if (target1Dist < target2Dist)
                        {
                            // subtract just target 1's distance from minutes
                            var updatedList = new List<string>(opened) { targetFor1.Key };
                            var updatedScore = totalPressure + ScoreFor(current1, targetFor1.Key, minutes);
                            var updatedCurrent2 = StepTowards(current2, targetFor2.Key, target1Dist + 1);
                            bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, targetFor1.Key, updatedCurrent2, updatedList, target2: targetFor2.Key, totalPressure: updatedScore));
                        }
                        else
                        {
                            // subtract just target 2's distance from minutes
                            var updatedList = new List<string>(opened) { targetFor2.Key };
                            var updatedScore = totalPressure + ScoreFor(current2, targetFor2.Key, minutes);
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
                        var updatedScore = totalPressure + ScoreFor(current1, targetFor1.Key, minutes) + ScoreFor(current2, target2, minutes);
                        bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, targetFor1.Key, target2, updatedList, totalPressure: updatedScore));
                    }
                    else if (target1Dist < target2Dist)
                    {
                        // subtract just target 1's distance from minutes
                        var updatedList = new List<string>(opened) { targetFor1.Key };
                        var updatedScore = totalPressure + ScoreFor(current1, targetFor1.Key, minutes);
                        var updatedCurrent2 = StepTowards(current2, target2, target1Dist + 1);
                        bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, targetFor1.Key, updatedCurrent2, updatedList, target2: target2, totalPressure: updatedScore));
                    }
                    else
                    {
                        // subtract just target 2's distance from minutes
                        var updatedList = new List<string>(opened) { target2 };
                        var updatedScore = totalPressure + ScoreFor(current2, target2, minutes);
                        var updatedCurrent1 = StepTowards(current1, targetFor1.Key, target2Dist + 1);
                        bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target2Dist - 1, updatedCurrent1, target2, updatedList, target1: targetFor1.Key, totalPressure: updatedScore));
                    }
                }
            }
        }
        else if (string.IsNullOrEmpty(target2) && TryGetTargets(minutes, current2, opened, target1, out var targets2))
        {
            var target1Dist = DistanceBetween(current1, target1);
            foreach (var targetFor2 in targets2)
            {
                var target2Dist = targetFor2.Value.Dist;

                if (target1Dist == target2Dist)
                {
                    // subtract the same amount from minutes
                    var updatedList = new List<string>(opened) { target1, targetFor2.Key };
                    var updatedScore = totalPressure + ScoreFor(current1, target1, minutes) + ScoreFor(current2, targetFor2.Key, minutes);
                    bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, target1, targetFor2.Key, updatedList, totalPressure: updatedScore));
                }
                else if (target1Dist < target2Dist)
                {
                    // subtract just target 1's distance from minutes
                    var updatedList = new List<string>(opened) { target1 };
                    var updatedScore = totalPressure + ScoreFor(current1, target1, minutes);
                    var updatedCurrent2 = StepTowards(current2, targetFor2.Key, target1Dist + 1);
                    bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target1Dist - 1, target1, updatedCurrent2, updatedList, target2: targetFor2.Key, totalPressure: updatedScore));
                }
                else
                {
                    // subtract just target 2's distance from minutes
                    var updatedList = new List<string>(opened) { targetFor2.Key };
                    var updatedScore = totalPressure + ScoreFor(current2, targetFor2.Key, minutes);
                    var updatedCurrent1 = StepTowards(current1, target1, target2Dist + 1);
                    bestPathScore = Math.Max(bestPathScore, TakeTwoPaths(minutes - target2Dist - 1, updatedCurrent1, targetFor2.Key, updatedList, target1: target1, totalPressure: updatedScore));
                }
            }
        }
        return bestPathScore;
    }

    private bool TryGetTargets(int minutes, string current, List<string> opened, string otherTarget, out IEnumerable<KeyValuePair<string, (int Dist, string Next)>> targets)
    {
        targets = null;
        if (string.IsNullOrEmpty(current)) return false;

        targets = _distanceMap[current].
                        Where(kvp => FlowRateOf(kvp.Key) > 0 && kvp.Value.Dist < minutes && !opened.Contains(kvp.Key) && kvp.Key != otherTarget).
                        OrderByDescending(kvp => ScoreFor(current, kvp.Key, minutes));
        return targets.Any();
    }
   
    private string StepTowards(string current, string target, int steps)
    {
        if (string.IsNullOrEmpty(target)) return string.Empty;

        if (DistanceBetween(current, target) <= steps) return target;
        else
        {
            for (int i = 0; i < steps; i++)
                current = _distanceMap[current][target].Next;
        }
        return current;
    }

    private float TargetScore(string current, string target, int minutes) 
    {
        var distance = DistanceBetween(current, target);
        var scoreOf = ScoreFor(current, target, minutes);
        return scoreOf / (float)distance;
    }

    private int ScoreFor(string current, string target, int minutes)
    {
        var distance = DistanceBetween(current, target);
        var score = (minutes - distance - 1) * FlowRateOf(target);
        return score;
    }

    private int DistanceBetween(string from, string to) => string.IsNullOrEmpty(to) ? int.MaxValue : from == to ? 0 : _distanceMap[from][to].Dist;
    
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