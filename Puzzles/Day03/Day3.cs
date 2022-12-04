using System;
using System.Linq;

namespace AoC22;

public class Day3 : Puzzle
{
    private string[] _data = Array.Empty<string>();

    public Day3(ILogger logger, string path) : base(logger, path) { }

    public override void Setup() => _data = Utils.ReadAllLines(_path);

    public override void SolvePart1()
    {
        int score = 0;
        foreach (var line in _data)
        {
            var half = line.Length >> 1;
            var charInCommon = line[..half].Intersect(line[half..]).FirstOrDefault();
            score += ScoreForChar(charInCommon);
        }
        _logger.Log(score);
    }

    public override void SolvePart2()
    {
        int score = 0;
        for (int i = 0; i < _data.Length - 2; i += 3)
        {
            var charInCommon = _data[i].Intersect(_data[i + 1]).Intersect(_data[i + 2]).FirstOrDefault();
            score += ScoreForChar(charInCommon);
        }
        _logger.Log(score);
    }

    private static int ScoreForChar(char c) => c switch
    {
        { } when c is >= 'a' and <= 'z' => c - 96, // 'a' is 97, 'z' is 122. Returns 1-26
        { } when c is >= 'A' and <= 'Z' => c - 38, // 'A' is 65, 'Z' is 90.  Returns 27-52
        _ => 0,
    };
}