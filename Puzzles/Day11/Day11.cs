using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC22;

public partial class Day11 : Puzzle
{
    // item starting values and positions (which monkey is holding them) - gets copied
    private readonly List<long> _itemStartingValues = new();
    private readonly List<byte> _itemStartingHolders = new();
    private readonly List<Monkey> _monkeys = new();

    public Day11(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        byte monkeyID = 0;
        var pattern = NumberPattern();
        Monkey? monkey = null;
        foreach (var line in ReadFromFile(ignoreWhiteSpace: true))
        {
            if (line.Contains("Monkey"))
            {
                monkeyID = byte.Parse(pattern.Match(line).Groups[0].ValueSpan);
                monkey = new Monkey(monkeyID);
                _monkeys.Add(monkey);
            }
            else if (line.Contains("Starting items"))
            {
                var matches = pattern.Matches(line);
                for (int i = 0; i < matches.Count; i++)
                {
                    _itemStartingValues.Add(int.Parse(matches[i].Groups[0].ValueSpan));
                    _itemStartingHolders.Add(monkeyID);
                }
            }
            else if (line.Contains("Operation"))
            {
                var match = pattern.Match(line);
                if (!match.Success) monkey!.Operation = old => old * old;
                else
                {
                    var value = int.Parse(match.Groups[0].ValueSpan);
                    if (line.Contains('*')) monkey!.Operation = old => old * value;
                    else monkey!.Operation = old => old + value; // line.Contains('+')
                }
            }
            else
            {
                var value = byte.Parse(pattern.Match(line).Groups[0].ValueSpan);
                if (line.Contains("Test")) monkey!.DivisibleByValue = value;
                else if (line.Contains("If true")) monkey!.TrueTarget = value;
                else if (line.Contains("If false")) monkey!.FalseTarget = value;
            }
        }
    }

    public override void SolvePart1()
    {
        // copy over item values and initial positions to not affect the original
        var items = new List<long>(_itemStartingValues); 
        var itemHolders = new List<byte>(_itemStartingHolders);
        
        var inspectionTotals = new long[_monkeys.Count];
        Func<long, long> postInspection = worry => worry / 3;

        RunRounds(20, items, itemHolders, inspectionTotals, postInspection);

        var monkeyBusiness = inspectionTotals.OrderDescending().Take(2).Product();
        _logger.Log(monkeyBusiness);
    }

    public override void SolvePart2()
    {
        // copy over item values and initial positions to not affect the original
        var items = new List<long>(_itemStartingValues);
        var itemHolders = new List<byte>(_itemStartingHolders);
        
        var inspectionTotals = new long[_monkeys.Count];
        var greatedCommonDivisor = _monkeys.Select(m => m.DivisibleByValue).Product();
        Func<long, long> postInspection = worry => worry % greatedCommonDivisor;

        RunRounds(10_000, items, itemHolders, inspectionTotals, postInspection);

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
        public byte DivisibleByValue, TrueTarget, FalseTarget;

        public Monkey(byte id) => Id = id;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberPattern();
}