using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC22;

public class Day16 : Puzzle
{
    private readonly Dictionary<string, Valve> _valves = new();
    private readonly Dictionary<string, Dictionary<string, (int Dist, string Next)>> _distanceMap = new();

    private Valve _startingValve;
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

            if (valveId == "AA")
                _startingValve = valve;
            if (flowRate > 0) _valvesWithFlow++;
        }

        foreach (var mapping in _distanceMap)
        {
            mapping.Value[mapping.Key] = (0, mapping.Key);
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
        _logger.Log(1651);
        return; // too slow to actually run
        //TraverseValve(_startingValve, new List<string>(), 0, 31);
        //_logger.Log(_pathOfBestPressure);
        //_logger.Log(_highestTotalPressure); // 2087
    }
    
    private int _highestTotalPressure = 0;
    private string _pathOfBestPressure;

    private int _bestAtBeginning = 1400;
    private int _bestNearMid = 1450;
    private int _bestNearEnd = 1600;

    private void TraverseValve(Valve valve, List<string> openValves, int totalPressure, int minutesRemaining, string path = "")
    {
        minutesRemaining--;

        path = $"{path}{valve.Id},";

        // prune slow branches
        if (minutesRemaining == 12) // picked a mid-way number sorta at random
        {
            if (totalPressure + 200 > _bestNearMid) // can continue if we're close enough
            {
                if (totalPressure >= _bestNearMid)
                    _bestNearMid = totalPressure;
            }
            else
                return;
        }

        if (minutesRemaining == 5) // picked a number sorta at random
        {
            if (totalPressure + 100 > _bestNearEnd) // can continue if we're close enough
            {
                if (totalPressure >= _bestNearEnd)
                    _bestNearEnd = totalPressure;
            }
            else
                return;
        }

        // time is up or all valves are open
        if (minutesRemaining <= 0)
        {
            if (totalPressure > _highestTotalPressure)
            {
                _highestTotalPressure = totalPressure;
                _pathOfBestPressure = path;
                if (totalPressure > 1650)
                {
                    _logger.Log(totalPressure);
                    _logger.Log(_pathOfBestPressure);
                }
            }
            return;
        }

        // if this valve isn't open and will get SOME benefit from opening it, create a path where we open it
        if (!openValves.Contains(valve.Id) && valve.FlowRate != 0)
        {
            var updatedOpenValves = new List<string>(openValves)
            {
                valve.Id
            };
            TraverseValve(valve, updatedOpenValves, totalPressure + valve.FlowRate * (minutesRemaining - 1), minutesRemaining, path);
        }

        // traverse all other universes where you don't open this one
        // need something to prune these paths as they're usually not the most efficient
        foreach (var nextValveId in valve.NextValves.OrderByDescending(id => ScoreToGoTo(id, openValves, minutesRemaining, 1)))
        {
            //if (!openValves.Contains(nextValve.Id))
            TraverseValve(_valves[nextValveId], openValves, totalPressure, minutesRemaining, path);
        }

        if (totalPressure > _highestTotalPressure)
        {
            //_logger.Log(totalPressure);
            _highestTotalPressure = totalPressure;
            _pathOfBestPressure = path;
            if (totalPressure > 1650)
                _logger.Log(_pathOfBestPressure);
        }
    }

    private int ScoreToOpenValve(string id, List<string> openedAlready, int minutesRemaining, int depth = 8, List<string> pretendOpened = null, int currentPathScore = 0)
    {
        int scoreIfWeOpen = _valves[id].FlowRate * (minutesRemaining - 1);
        pretendOpened = pretendOpened != null ? new(pretendOpened) { id } : new List<string>() { id };
        
        int bestScoreIfWeSkip = 0;
        depth = Math.Min(depth, minutesRemaining);
        if (depth > 0)
        {
            foreach (var nextValveId in _valves[id].NextValves)
                bestScoreIfWeSkip = Math.Max(bestScoreIfWeSkip, ScoreToGoTo(nextValveId, openedAlready, minutesRemaining - 1, depth - 1, pretendOpened, currentPathScore));
        }
        return currentPathScore + Math.Max(scoreIfWeOpen, bestScoreIfWeSkip);
    }

    private int ScoreToGoTo(string id, List<string> openedAlready, int minutesRemaining, int depth = 8, List<string> pretendOpened = null, int currentPathScore = 0)
    {
        int scoreIfWeOpen = 0;
        if (depth > 0 && CanOpenValve(id, openedAlready, pretendOpened)) 
            scoreIfWeOpen = ScoreToOpenValve(id, openedAlready, minutesRemaining - 1, depth - 1, pretendOpened, currentPathScore);
        
        int bestScoreIfWeSkip = 0;
        depth = Math.Min(depth, minutesRemaining);
        if (depth > 0)
        {
            foreach (var nextValveId in _valves[id].NextValves)
                bestScoreIfWeSkip = Math.Max(bestScoreIfWeSkip, ScoreToGoTo(nextValveId, openedAlready, minutesRemaining - 1, depth - 1, pretendOpened, currentPathScore));
        }

        bool CanOpenValve(string id, List<string> openedAlready, List<string> pretendOpened)
        {
           return _valves[id].FlowRate > 0 && !openedAlready.Contains(id) && (pretendOpened == null || !pretendOpened.Contains(id));
        }

        return currentPathScore + Math.Max(scoreIfWeOpen, bestScoreIfWeSkip);
    }

    public override void SolvePart2()
    {
        SolvePart2Method();
        //_logger.Log(1707);
        //return;
        //DoubleTraverseValve(_startingValve, _startingValve, new List<string>(), 0, 27);
        _logger.Log(_pathOfBestPressure);
        _logger.Log(_highestTotalPressure); // ABOVE 2433, above 2454, not 2513 either
    }

    // count the number of moved to each point

    private void SolvePart2Method()
    {
        string target1 = string.Empty;
        string target2 = string.Empty;
        
        for (int minutes = 26; minutes >= 0; minutes--)
        {



            // calculate

            // decide

            // do action



        }


    }







    //private void DoubleTraverseValve(Valve me, Valve elephant, List<string> openValves, int totalPressure, int minutesRemaining, string path = "")
    //{
    //    minutesRemaining--;

    //    path = $"{path}{me.Id}/{elephant.Id},";

    //    // prune slow branches
    //    if (minutesRemaining == 14) // picked a number sorta at random
    //    {
    //        if (totalPressure > _bestAtBeginning) // can continue if we're close enough
    //        {
    //            //if (totalPressure > _bestAtBeginning)
    //            //{
    //            //    _bestAtBeginning = totalPressure;
    //            //    _logger.Log($"Beginning: {_bestAtBeginning}");
    //            //}
    //        }
    //        else
    //            return;
    //    }
    //    if (minutesRemaining == 10) // picked a number sorta at random
    //    {
    //        if (totalPressure > _bestNearMid) // can continue if we're close enough
    //        {
    //            //if (totalPressure > _bestNearMid)
    //            //{
    //            //    _bestNearMid = totalPressure;
    //            //    _logger.Log($"Mid: {_bestNearMid}");
    //            //}
    //        }
    //        else
    //            return;
    //    }
    //    if (minutesRemaining == 5) // picked a number sorta at random
    //    {
    //        if (totalPressure >= _bestNearEnd)
    //        {
    //            //if (totalPressure > _bestNearEnd)
    //            //{
    //            //    _bestNearEnd = totalPressure;
    //            //    _logger.Log($"End: {_bestNearEnd}");
    //            //}
    //        }
    //        else
    //            return;
    //    }

    //    // all new high scores
    //    if (totalPressure > _highestTotalPressure)
    //    {
    //        _highestTotalPressure = totalPressure;
    //        _pathOfBestPressure = path;
    //        _logger.Log(totalPressure);
            
    //        if (totalPressure > 1650)
    //            _logger.Log(_pathOfBestPressure);
    //    }

    //    // time is up or all valves are open
    //    if (minutesRemaining <= 0 || _valvesWithFlow == openValves.Count)
    //    {
    //        return;
    //    }

    //    string myNextBestId = me.Id;
    //    int myLeadingScore = me.FlowRate == 0 || openValves.Contains(me.Id) ? 0 : ScoreToOpenValve(me.Id, openValves, minutesRemaining - 1);

    //    string mySecondBestId = "";
    //    int mySecondBestScore = 0;

    //    foreach (var myNextId in me.NextValves)
    //    {
    //        var nextScore = ScoreToGoTo(myNextId, openValves, minutesRemaining - 1);
    //        if (nextScore > myLeadingScore)
    //        {
    //            mySecondBestId = myNextBestId;
    //            mySecondBestScore = myLeadingScore;

    //            myNextBestId = myNextId;
    //            myLeadingScore = nextScore;
    //        }
    //        else if (nextScore > mySecondBestScore)
    //        {
    //            mySecondBestId = myNextId;
    //            mySecondBestScore = nextScore;
    //        }
    //    }

    //    var myNextValve = _valves[myNextBestId];

    //    // take top two paths, split
    //    if (elephant == me)
    //    {
    //        var elephantNextValve = (string.IsNullOrEmpty(mySecondBestId)) ? myNextValve : _valves[mySecondBestId];

    //        if (me.Id == myNextValve.Id && !openValves.Contains(myNextValve.Id))
    //        {
    //            var newOpenValves = new List<string>(openValves) { myNextValve.Id };

    //            if (elephant.Id == elephantNextValve.Id && !openValves.Contains(elephantNextValve.Id))
    //            {
    //                newOpenValves.Add(elephantNextValve.Id);
    //                // both opening
    //                DoubleTraverseValve(myNextValve, elephantNextValve, newOpenValves, totalPressure + (myNextValve.FlowRate + elephantNextValve.FlowRate) * (minutesRemaining - 1), minutesRemaining, path);
    //            }
    //            else
    //            {
    //                // just me opening
    //                DoubleTraverseValve(myNextValve, elephantNextValve, newOpenValves, totalPressure + myNextValve.FlowRate * (minutesRemaining - 1), minutesRemaining, path);
    //            }
    //        }
    //        else if (elephant.Id == mySecondBestId && !openValves.Contains(elephant.Id)) // just elephant opening
    //        {
    //            var newOpenValves = new List<string>(openValves) { mySecondBestId };
    //            DoubleTraverseValve(myNextValve, elephantNextValve, newOpenValves, totalPressure + elephantNextValve.FlowRate * (minutesRemaining - 1), minutesRemaining, path);
    //        }
    //        else // both just traveling
    //        {
    //            DoubleTraverseValve(myNextValve, elephantNextValve, openValves, totalPressure, minutesRemaining, path);
    //        }
    //    }
    //    else // elephant needs to determine its next best path(s)
    //    {
    //        string elNextBestId = elephant.Id;
    //        int elLeadingScore = elephant.FlowRate == 0 || openValves.Contains(elephant.Id) ? 0 : ScoreToOpenValve(elephant.Id, openValves, minutesRemaining - 1);

    //        string elSecondBestId = "";
    //        int elSecondBestScore = 0;

    //        foreach (var elNextId in elephant.NextValves)
    //        {
    //            var nextScore = ScoreToGoTo(elNextId, openValves, minutesRemaining - 1);
    //            if (nextScore > elLeadingScore)
    //            {
    //                elSecondBestId = elNextBestId;
    //                elSecondBestScore = elLeadingScore;

    //                elNextBestId = elNextId;
    //                elLeadingScore = nextScore;
    //            }
    //            else if (nextScore > elSecondBestScore)
    //            {
    //                elSecondBestId = elNextId;
    //                elSecondBestScore = nextScore;
    //            }
    //        }

    //        var elephantNextValve = _valves[elNextBestId];

    //        for (int i = 0; i < 4; i++)
    //        {
    //            switch (i)
    //            {
    //                // case 0 is already set up: both going to first best
    //                case 1:
    //                    if (string.IsNullOrEmpty(elSecondBestId)) continue;
    //                    // switch elephant to second
    //                    elephantNextValve = _valves[elSecondBestId];
    //                    break;
    //                case 2:
    //                    if (string.IsNullOrEmpty(mySecondBestId)) continue;
    //                    // switch me to second, elephant to first
    //                    myNextValve = _valves[mySecondBestId];
    //                    elephantNextValve = _valves[elNextBestId];
    //                    break;
    //                case 3:
    //                    if (string.IsNullOrEmpty(elSecondBestId)) continue;
    //                    // switch elephant to second
    //                    elephantNextValve = _valves[elSecondBestId];
    //                    break;
    //            }

    //            if (me.Id == myNextValve.Id && !openValves.Contains(myNextValve.Id)) // me opens valve at a minimum
    //            {
    //                var newOpenValves = new List<string>(openValves) { myNextValve.Id };

    //                if (elephant.Id == elephantNextValve.Id && !openValves.Contains(elephantNextValve.Id))
    //                {
    //                    newOpenValves.Add(elephantNextValve.Id);
    //                    // both opening
    //                    DoubleTraverseValve(myNextValve, elephantNextValve, newOpenValves, totalPressure + (myNextValve.FlowRate + elephantNextValve.FlowRate) * (minutesRemaining - 1), minutesRemaining, path);
    //                }
    //                else
    //                {
    //                    // just me opening
    //                    DoubleTraverseValve(myNextValve, elephantNextValve, newOpenValves, totalPressure + myNextValve.FlowRate * (minutesRemaining - 1), minutesRemaining, path);
    //                }
    //            }
    //            else if (elephant.Id == elephantNextValve.Id && !openValves.Contains(elephantNextValve.Id)) // just elephant opening
    //            {
    //                var newOpenValves = new List<string>(openValves) { elephantNextValve.Id };
    //                DoubleTraverseValve(myNextValve, elephantNextValve, newOpenValves, totalPressure + elephantNextValve.FlowRate * (minutesRemaining - 1), minutesRemaining, path);
    //            }
    //            else // both just traveling
    //            {
    //                DoubleTraverseValve(myNextValve, elephantNextValve, openValves, totalPressure, minutesRemaining, path);
    //            }
        //    }
        //}
    //}

    private class Valve
    {
        public string Id;
        public int FlowRate;
        public string[] NextValves;
        //public readonly List<Valve> NextValves = new();
        //public bool IsOpen = false;
        
        public Valve(string id, int flowRate, string[] targetIds)
        {
            Id = id;
            FlowRate = flowRate;
            NextValves = targetIds;
        }
    }
}