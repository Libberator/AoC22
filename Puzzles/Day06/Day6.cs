using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC22;

public class Day6 : Puzzle
{
    private string _data = string.Empty;

    public Day6(ILogger logger, string path) : base(logger, path) { }

    public override void Setup() => _data = ReadAllLines().First();

    public override void SolvePart1() => _logger.Log(IndexOfMessageMarker(_data, 4));

    public override void SolvePart2() => _logger.Log(IndexOfMessageMarker(_data, 14));

    private static int IndexOfMessageMarker(string message, int distinctLength)
    {
        for (int i = 0; i < message.Length - distinctLength; i++)
            if (new HashSet<char>(message[i..(i + distinctLength)]).Count == distinctLength)
                return i + distinctLength;
        throw new Exception($"Unique sequence of length {distinctLength} not found.");
    }
}