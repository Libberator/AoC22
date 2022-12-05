using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC22;

public class Day5 : Puzzle
{
    private readonly Dictionary<int, Stack<char>> _data = new(9);
    private readonly List<Instruction> _instructions = new();
    private record Instruction(int Amount, int From, int To);

    public Day5(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        _instructions.Clear();
        for (int i = 1; i <= 9; i++)
            _data[i] = new();

        bool onFirstPart = true;
        foreach (var line in ReadFromFile())
        {
            if (string.IsNullOrEmpty(line))
            {
                onFirstPart = false;
                continue;
            }
            if (onFirstPart)
            {
                // Important indices: 1, 5, 9, 13, 17, 21, 25, 29, 33
                for (int i = 1; i < line.Length; i += 4)
                    if (line[i] != ' ')
                        _data[((i - 1) / 4) + 1].Push(line[i]);
                // Side note: we've also added the numbers 1-9 to the stack
            }
            else
            {
                var digits = Regex.Replace(line, "[a-z]", string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).ConvertToInts();
                _instructions.Add(new Instruction(digits[0], digits[1], digits[2]));
            }
        }
    }

    public override void SolvePart1()
    {
        var copiedData = CopyData(_data);
        foreach (var move in _instructions)
            MoveFromTo(copiedData, move.Amount, move.From, move.To);
        _logger.Log(TopCrates(copiedData));
    }

    public override void SolvePart2()
    {
        var copiedData = CopyData(_data);
        foreach (var move in _instructions)
            MoveFromToMaintainOrder(copiedData, move.Amount, move.From, move.To);
        _logger.Log(TopCrates(copiedData));
    }

    private static void MoveFromTo(Dictionary<int, Stack<char>> source, int amount, int from, int to)
    {
        for (int i = 0; i < amount; i++)
            source[to].Push(source[from].Pop());
    }

    private static void MoveFromToMaintainOrder(Dictionary<int, Stack<char>> source, int amount, int from, int to)
    {
        var temp = new Stack<char>();
        for (int i = 0; i < amount; i++)
            temp.Push(source[from].Pop());
        while (temp.Count != 0)
            source[to].Push(temp.Pop());
    }

    // We make a copy of it so that we're not affecting the original and can run both Parts and also do performance tests
    private static Dictionary<int, Stack<char>> CopyData(Dictionary<int, Stack<char>> source)
    {
        Dictionary<int, Stack<char>> copy = new(9);
        foreach (var kvp in source)
            copy[kvp.Key] = new(kvp.Value); // this correctly *reverses* the order for the stack copy
        return copy;
    }

    private static string TopCrates(Dictionary<int, Stack<char>> source)
    {
        var result = new StringBuilder();
        foreach (var kvp in source)
            result.Append(kvp.Value.Count > 1 ? kvp.Value.Peek() : string.Empty); // the bottom-most are the numbers 1 through 9, so we ignore those
        return result.ToString();
    }
}