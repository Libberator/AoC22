using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC22;

public partial class Day11 : Puzzle
{
    private readonly List<Monkey> _monkeys = new();
    // item starting values and their holder (which monkey ID is holding them) - these two get copied
    private readonly List<long> _itemStartingValues = new();
    private readonly List<byte> _itemStartingHolders = new();

    public Day11(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var pattern = NumberPattern();
        Monkey monkey = null;
        foreach (var line in ReadFromFile(ignoreWhiteSpace: true))
        {
            if (line.Contains("Monkey"))
            {
                monkey = new Monkey(byte.Parse(pattern.Match(line).ValueSpan));
                _monkeys.Add(monkey);
            }
            else if (line.Contains("Starting items"))
            {
                var matches = pattern.Matches(line);
                for (int i = 0; i < matches.Count; i++)
                {
                    _itemStartingValues.Add(int.Parse(matches[i].ValueSpan));
                    _itemStartingHolders.Add(monkey.Id);
                }
            }
            else if (line.Contains("Operation"))
            {
                var match = pattern.Match(line);
                if (!match.Success) monkey.Operation = old => old * old;
                else
                {
                    var value = int.Parse(match.ValueSpan);
                    if (line.Contains('*')) monkey.Operation = old => old * value;
                    else monkey.Operation = old => old + value; // line.Contains('+')
                }
            }
            else
            {
                var value = byte.Parse(pattern.Match(line).ValueSpan);
                if (line.Contains("Test")) monkey.DivisibleByValue = value;
                else if (line.Contains("If true")) monkey.TrueTarget = value;
                else if (line.Contains("If false")) monkey.FalseTarget = value;
            }
        }
    }

    public override void SolvePart1()
    {
        var items = new List<long>(_itemStartingValues); 
        var itemHolders = new List<byte>(_itemStartingHolders);
        var inspectionTotals = new long[_monkeys.Count];

        RunRounds(20, items, itemHolders, inspectionTotals, worry => worry / 3);

        var monkeyBusiness = inspectionTotals.OrderDescending().Take(2).Product();
        _logger.Log(monkeyBusiness);
    }

    public override void SolvePart2()
    {
        var items = new List<long>(_itemStartingValues);
        var itemHolders = new List<byte>(_itemStartingHolders);
        var inspectionTotals = new long[_monkeys.Count];
        var greatestCommonDivisor = _monkeys.Select(m => m.DivisibleByValue).Product();

        RunRounds(10_000, items, itemHolders, inspectionTotals, worry => worry % greatestCommonDivisor);

        var monkeyBusiness = inspectionTotals.OrderDescending().Take(2).Product();
        _logger.Log(monkeyBusiness);
    }

    private void RunRounds(int rounds, List<long> items, List<byte> itemHolders, long[] inspectionTotals, Func<long, long> postInspection)
    {
        for (int round = 0; round < rounds; round++)
            foreach (var monkey in _monkeys)
                for (int i = 0; i < itemHolders.Count; i++)
                {
                    if (monkey.Id != itemHolders[i]) continue;
                    inspectionTotals[monkey.Id]++;
                    items[i] = postInspection(monkey.Operation(items[i]));
                    itemHolders[i] = items[i] % monkey.DivisibleByValue == 0 ? monkey.TrueTarget : monkey.FalseTarget;
                }
    }

    private class Monkey
    {
        public readonly byte Id;
        public Func<long, long> Operation;
        public int DivisibleByValue;
        public byte TrueTarget, FalseTarget;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Monkey(byte id) => Id = id;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberPattern();
}