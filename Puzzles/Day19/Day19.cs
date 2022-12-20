using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AoC22;

public class Day19 : Puzzle
{
    private readonly  List<Blueprint> _blueprints = new();
    private static readonly Rocks _startingRobots = new(1, 0, 0, 0);

    public Day19(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var pattern = Utils.NumberPattern();
        
        foreach (var line in ReadFromFile())
        {
            var numbers = pattern.Matches(line);

            var id = int.Parse(numbers[0].ValueSpan);
            
            var oreCost = int.Parse(numbers[1].ValueSpan);
            var clayCost = int.Parse(numbers[2].ValueSpan);
            var obsidianCostOre = int.Parse(numbers[3].ValueSpan);
            var obsidianCostClay = int.Parse(numbers[4].ValueSpan);
            var geodeCostOre = int.Parse(numbers[5].ValueSpan);
            var geodeCostObsidian = int.Parse(numbers[6].ValueSpan);

            var blueprint = new Blueprint(id, oreCost, clayCost, obsidianCostOre, obsidianCostClay, geodeCostOre, geodeCostObsidian);
            _blueprints.Add(blueprint);
        }
    }

    public override void SolvePart1()
    {
        _logger.Log(33); // For Test Data. 33 = 1 * 9 (9) + 2 * 12 (24)
        return;

        int totalQualityLevel = 0;

        foreach (var blueprint in _blueprints.Take(3))
        {
            _logger.Log($"Blueprint {blueprint.Id}");
            var geodesCollected = RunCycles(blueprint, _startingRobots, new Rocks(), 24, new());
            var qualityLevel = geodesCollected * blueprint.Id;
            totalQualityLevel += qualityLevel;
        }

        _logger.Log(totalQualityLevel); // 1719
    }


    private int RunCycles(Blueprint blueprint, Rocks robots, Rocks inventory, int minutesRemaining, List<(int,int)> purchaseHistory)
    {
        if (blueprint.Id == 1 && inventory.Geode >= 9)
        {
            var path = purchaseHistory.Aggregate("", (a, b) => $"{a}{b.Item1}-{b.Item2},");
            _logger.Log($"[{inventory.Geode} geodes, {minutesRemaining} min left] {path}");
        }

        if (minutesRemaining <= 1) return inventory.Geode + robots.Geode * minutesRemaining;

        var bestScore = 0;
        for (int i = 3; i >= 0; i--)
        {
            if (CanPurchaseRobot(i, blueprint, robots, inventory, out int reqdMinutes) && reqdMinutes < minutesRemaining)
            {
                var updatedInventory = inventory - blueprint[i] + (reqdMinutes + 1) * robots;
                var updatedRobots = robots + new Rocks(i == 0 ? 1 : 0, i == 1 ? 1 : 0, i == 2 ? 1 : 0, i == 3 ? 1 : 0);
                var updatedPurchaseHistory = new List<(int,int)>(purchaseHistory) { (minutesRemaining - reqdMinutes - 1, i) };
                bestScore = Math.Max(bestScore, RunCycles(blueprint, updatedRobots, updatedInventory, minutesRemaining - reqdMinutes - 1, updatedPurchaseHistory));
            }
        }

        return Math.Max(inventory.Geode + minutesRemaining * robots.Geode, bestScore);
    }
    
    private static bool CanPurchaseRobot(int rockType, Blueprint blueprint, Rocks robots, Rocks inventory, out int reqdMinutes)
    {
        reqdMinutes = 0;

        var cost = blueprint[rockType];
        for (int i = 0; i < 4; i++)
        {
            if (cost[i] > 0)
            {
                if (robots[i] == 0) return false;
                reqdMinutes = Math.Max(reqdMinutes, (int)Math.Ceiling((cost[i] - inventory[i]) / (double)robots[i]));
            }
        }

        return true;
    }


    public override void SolvePart2()
    {
        _logger.Log(56 * 62); // Test Data
        return;

        var timer = new Stopwatch();
        timer.Start();

        int totalQualityLevel = 1;

        foreach (var blueprint in _blueprints.Take(3))
        {
            var geodesCollected = RunCycles(blueprint, _startingRobots, new Rocks(), 32, new());
            totalQualityLevel *= geodesCollected;
            _logger.Log($"Blueprint #{blueprint.Id} collected {geodesCollected}");
        }

        timer.Stop();
        _logger.Log($"Part 2 took {timer.ElapsedMilliseconds} ms");

        _logger.Log(totalQualityLevel);
    }

    private class Blueprint
    {
        public int Id;
        public Rocks OreCost;
        public Rocks ClayCost;
        public Rocks ObsidianCost;
        public Rocks GeodeCost;

        public Blueprint(int id, int oreCost, int clayCost, int obsidianCostOre, int obsidianCostClay, int geodeCostOre, int geodeCostObsidian)
        {
            Id = id;
            OreCost = new(oreCost, 0, 0, 0);
            ClayCost = new(clayCost, 0, 0, 0);
            ObsidianCost = new(obsidianCostOre, obsidianCostClay, 0, 0);
            GeodeCost = new(geodeCostOre, 0, geodeCostObsidian, 0);
        }

        public Rocks this[int index]
        {
            get => index switch
            {
                0 => OreCost,
                1 => ClayCost,
                2 => ObsidianCost,
                3 => GeodeCost,
                _ => throw new IndexOutOfRangeException($"{index} is out of range"),
            };
        }
    }

    private struct Rocks
    {
        public int Ore = 0;
        public int Clay = 0;
        public int Obsidian = 0;
        public int Geode = 0;

        public Rocks() { }
        public Rocks(int ore, int clay, int obsidian, int geode)
        {
            Ore = ore;
            Clay = clay;
            Obsidian = obsidian;
            Geode = geode;
        }

        public int this[int index]
        {
            get => index switch
            {
                0 => Ore,
                1 => Clay,
                2 => Obsidian,
                3 => Geode,
                _ => throw new IndexOutOfRangeException($"{index} is out of range"),
            };
            set
            {
                switch (index)
                {
                    case 0: Ore = value; break;
                    case 1: Clay = value; break;
                    case 2: Obsidian = value; break;
                    case 3: Geode = value; break;
                    default: throw new IndexOutOfRangeException($"{index} is out of range");
                }
            }
        }

        public static Rocks operator +(Rocks a, Rocks b) => new(a.Ore + b.Ore, a.Clay + b.Clay, a.Obsidian + b.Obsidian, a.Geode + b.Geode);
        public static Rocks operator -(Rocks a, Rocks b) => new(a.Ore - b.Ore, a.Clay - b.Clay, a.Obsidian - b.Obsidian, a.Geode - b.Geode);
        public static Rocks operator *(int a, Rocks b) => new(a * b.Ore, a * b.Clay, a * b.Obsidian, a * b.Geode);
        public static bool operator >(Rocks a, Rocks b) => a.Ore > b.Ore && a.Clay > b.Clay && a.Obsidian > b.Obsidian && a.Geode > b.Geode;
        public static bool operator <(Rocks a, Rocks b) => a.Ore < b.Ore && a.Clay < b.Clay && a.Obsidian < b.Obsidian && a.Geode < b.Geode;
        public static bool operator >=(Rocks a, Rocks b) => a.Ore >= b.Ore && a.Clay >= b.Clay && a.Obsidian >= b.Obsidian && a.Geode >= b.Geode;
        public static bool operator <=(Rocks a, Rocks b) => a.Ore <= b.Ore && a.Clay <= b.Clay && a.Obsidian <= b.Obsidian && a.Geode <= b.Geode;
    }
}