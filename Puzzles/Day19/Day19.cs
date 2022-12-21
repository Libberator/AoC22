using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC22;

public class Day19 : Puzzle
{
    private readonly List<Blueprint> _blueprints = new();
    private static readonly Material _startingRobots = new(1, 0, 0, 0);

    public Day19(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var pattern = Utils.NumberPattern();

        foreach (var line in ReadFromFile())
        {
            var numbers = pattern.Matches(line);

            var id = int.Parse(numbers[0].ValueSpan);
            var ore = int.Parse(numbers[1].ValueSpan);
            var clay = int.Parse(numbers[2].ValueSpan);
            var obsidian_ore = int.Parse(numbers[3].ValueSpan);
            var obsidian_clay = int.Parse(numbers[4].ValueSpan);
            var geode_ore = int.Parse(numbers[5].ValueSpan);
            var geode_obsidian = int.Parse(numbers[6].ValueSpan);

            var blueprint = new Blueprint(id, ore, clay, obsidian_ore, obsidian_clay, geode_ore, geode_obsidian);
            _blueprints.Add(blueprint);
        }
    }

    public override void SolvePart1()
    {
        //_logger.Log("33"); // For Test Data. 1 * 9 + 2 * 12 = 33
        //return;

        int totalQualityLevel = 0;

        foreach (var blueprint in _blueprints)
            totalQualityLevel += blueprint.Id * StartCycles(24, blueprint);

        _logger.Log(totalQualityLevel); // 1719
    }

    public override void SolvePart2()
    {
        //_logger.Log("3472"); // For Test Data: 56 * 62 = 3472
        //return;

        int geodeProduct = 1;

        foreach (var blueprint in _blueprints.Take(3))
            geodeProduct *= StartCycles(32, blueprint);

        _logger.Log(geodeProduct);
    }

    // Used for pruning branches
    private readonly Dictionary<Snapshot, int> _snapshots = new();
    private int _currentMax = 0;

    private int StartCycles(int maxMinutes, Blueprint blueprint, int elapsedMinutes = 0)
    {
        _currentMax = 0;
        _snapshots.Clear();
        return RunCycles(maxMinutes, blueprint, new Material(), _startingRobots, elapsedMinutes);
    }

    private int RunCycles(int maxMinutes, Blueprint blueprint, Material inventory, Material robots, int elapsedMinutes = 0)
    {
        var mostGeodes = TotalGeodes();
        if (mostGeodes > _currentMax) _currentMax = mostGeodes;
        if (elapsedMinutes == maxMinutes) return inventory.Geode;

        // Pruning. Removing branches where even the most optimistic (buying geode robot every round) won't beat current max
        if (MaxGeodesPossible() < _currentMax) return mostGeodes;
        // Pruning. Removing branches we've already gone down. aka eliminating "transpositions"
        var snapshot = new Snapshot(elapsedMinutes, inventory, robots);
        if (_snapshots.TryGetValue(snapshot, out var geodes)) return geodes;

        for (int i = 3; i >= 0; i--)
        {
            var cost = blueprint.RobotCost(i);
            if (TryPurchaseRobot(i, blueprint, inventory, robots, out int reqdMinutes) && (elapsedMinutes + reqdMinutes) < maxMinutes)
            {
                var updatedInventory = inventory - cost + (reqdMinutes + 1) * robots;
                var updatedRobots = robots + new Material(i == 0 ? 1 : 0, i == 1 ? 1 : 0, i == 2 ? 1 : 0, i == 3 ? 1 : 0);
                mostGeodes = Math.Max(mostGeodes, RunCycles(maxMinutes, blueprint, updatedInventory, updatedRobots, elapsedMinutes + reqdMinutes + 1));
            }
        }

        _snapshots.Add(snapshot, mostGeodes);
        return mostGeodes;

        int TotalGeodes() => inventory.Geode + (maxMinutes - elapsedMinutes) * robots.Geode;
        int MaxGeodesPossible() => TotalGeodes() + Utils.GetTriangleNumber(maxMinutes - elapsedMinutes - 1);
    }

    private static bool TryPurchaseRobot(int materialIndex, Blueprint blueprint, Material inventory, Material robots, out int reqdMinutes)
    {
        reqdMinutes = 0;
        var cost = blueprint.RobotCost(materialIndex);
        if (inventory >= cost) return true;

        for (int i = 0; i < 4; i++)
        {
            if (cost[i] > 0)
            {
                if (robots[i] == 0) return false; // no means of producing required material to buy this robot
                var minutesToEarnMaterial = (int)Math.Ceiling((cost[i] - inventory[i]) / (double)robots[i]);
                reqdMinutes = Math.Max(reqdMinutes, minutesToEarnMaterial);
            }
        }

        return true;
    }

    private record struct Snapshot(int MinutesRemaining, Material Inventory, Material Robots);

    private class Blueprint
    {
        public readonly int Id;
        public readonly Material OreRobotCost;
        public readonly Material ClayRobotCost;
        public readonly Material ObsidianRobotCost;
        public readonly Material GeodeRobotCost;

        public Blueprint(int id, int oreCost, int clayCost, int obsidianCostOre, int obsidianCostClay, int geodeCostOre, int geodeCostObsidian)
        {
            Id = id;
            OreRobotCost = new(oreCost, 0, 0, 0);
            ClayRobotCost = new(clayCost, 0, 0, 0);
            ObsidianRobotCost = new(obsidianCostOre, obsidianCostClay, 0, 0);
            GeodeRobotCost = new(geodeCostOre, 0, geodeCostObsidian, 0);
        }

        public Material RobotCost(int index) => index switch
        {
            0 => OreRobotCost,
            1 => ClayRobotCost,
            2 => ObsidianRobotCost,
            3 => GeodeRobotCost,
            _ => throw new IndexOutOfRangeException($"{index} is out of range"),
        };
    }

    // TODO: Turn this into a Vector4Int
    private struct Material
    {
        public int Ore = 0;
        public int Clay = 0;
        public int Obsidian = 0;
        public int Geode = 0;

        public Material() { }
        public Material(int ore, int clay, int obsidian, int geode)
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

        public static Material operator +(Material a, Material b) => new(a.Ore + b.Ore, a.Clay + b.Clay, a.Obsidian + b.Obsidian, a.Geode + b.Geode);
        public static Material operator -(Material a, Material b) => new(a.Ore - b.Ore, a.Clay - b.Clay, a.Obsidian - b.Obsidian, a.Geode - b.Geode);
        public static Material operator *(int a, Material b) => new(a * b.Ore, a * b.Clay, a * b.Obsidian, a * b.Geode);
        public static bool operator >(Material a, Material b) => a.Ore > b.Ore && a.Clay > b.Clay && a.Obsidian > b.Obsidian && a.Geode > b.Geode;
        public static bool operator <(Material a, Material b) => a.Ore < b.Ore && a.Clay < b.Clay && a.Obsidian < b.Obsidian && a.Geode < b.Geode;
        public static bool operator >=(Material a, Material b) => a.Ore >= b.Ore && a.Clay >= b.Clay && a.Obsidian >= b.Obsidian && a.Geode >= b.Geode;
        public static bool operator <=(Material a, Material b) => a.Ore <= b.Ore && a.Clay <= b.Clay && a.Obsidian <= b.Obsidian && a.Geode <= b.Geode;
        public static bool operator ==(Material a, Material b) => a.Ore == b.Ore && a.Clay == b.Clay && a.Obsidian == b.Obsidian && a.Geode == b.Geode;
        public static bool operator !=(Material a, Material b) => a.Ore != b.Ore || a.Clay != b.Clay || a.Obsidian != b.Obsidian || a.Geode != b.Geode;
        public override bool Equals(object obj) => obj is Material other && this == other;
        public override int GetHashCode() => HashCode.Combine(Ore, Clay, Obsidian, Geode);
    }
}