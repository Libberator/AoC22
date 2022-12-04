using System;
using System.Linq;

namespace AoC22;

public class Day3 : Puzzle
{
    private string[] _data = Array.Empty<string>();

    public Day3(ILogger logger, string path) : base(logger, path) { }

    public override void Setup() => _data = ReadAllLines();

    public override void SolvePart1()
    {
        int score = 0;
        foreach (var line in _data)
        {
            var half = line.Length >> 1;
            var charInCommon = line[..half].Intersect(line[half..]).Single();
            score += ScoreForChar(charInCommon);
        }
        _logger.Log(score);
    }

    public override void SolvePart2()
    {
        int score = 0;
        for (int i = 0; i < _data.Length - 2; i += 3)
        {
            var charInCommon = _data[i].Intersect(_data[i + 1]).Intersect(_data[i + 2]).Single();
            score += ScoreForChar(charInCommon);
        }
        _logger.Log(score);
    }

    private static int ScoreForChar(char c) => c switch
    {
        { } when c is >= 'a' and <= 'z' => c - 'a' + 1, // Returns 1-26
        { } when c is >= 'A' and <= 'Z' => c - 'A' + 27, // Returns 27-52
        _ => 0,
    };
}