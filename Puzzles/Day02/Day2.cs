using System.Collections.Generic;
using System.Linq;

namespace AoC22;

public class Day2 : Puzzle
{
    private readonly Dictionary<string, int> _data = new(); // just keeping track of number of occurances for each matchup

    private const char ROCK = 'X';
    private const char PAPER = 'Y';
    private const char SCISSORS = 'Z';

    public Day2(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        _data.Clear();
        foreach (var line in ReadFromFile())
            _data.AddToExistingOrCreate(line, 1);
    }

    public override void SolvePart1()
    {
        var total = _data.Aggregate(0, (sum, kvp) => sum + kvp.Value * (PointsForThrowing(kvp.Key[^1]) + PointsForResult(kvp.Key)));
        _logger.Log(total);
    }

    public override void SolvePart2()
    {
        var total = _data.Aggregate(0, (sum, kvp) => sum + kvp.Value * (PointsForThrowing(UpdatedThrow(kvp.Key)) + PointsForKnownResult(kvp.Key[^1])));
        _logger.Log(total);
    }

    private static int PointsForThrowing(char c) => c switch
    {
        ROCK or 'A' => 1,
        PAPER or 'B' => 2,
        SCISSORS or 'C' => 3,
        _ => 0,
    };

    private static int PointsForResult(string matchup) => matchup switch
    {
        "A Y" or "B Z" or "C X" => 6, // Win
        "A X" or "B Y" or "C Z" => 3, // Tie
        _ => 0, // Loss in all other cases
    };

    // Updated rules: X = need to lose, Y = tie, Z = win. This returns the value of what we need to *throw* to meet the condition
    private static char UpdatedThrow(string matchup) => matchup switch
    {
        "A Y" or "B X" or "C Z" => ROCK,     // Ties (Y) Rock (A), Loses (X) to Paper (B), Beats (Z) Scissors (C)
        "A Z" or "B Y" or "C X" => PAPER,    // Beats (Z) Rock (A), Ties (Y) Paper (B), Loses (X) to Scissors (C)
        "A X" or "B Z" or "C Y" => SCISSORS, // Loses (X) to Rock (A), Beats (Z) Paper (B), Ties (Y) Scissors (C)
        _ => '\0',
    };

    // X/Y/Z now means Loss/Tie/Win so we just remap the point values to match: 1/2/3 -> 0/3/6
    private static int PointsForKnownResult(char c) => 3 * (PointsForThrowing(c) - 1);
}