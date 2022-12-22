using System.Collections.Generic;

namespace AoC22;

public class Day22 : Puzzle
{
    List<int> _data = new();

    public Day22(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var pattern = Utils.NumberPattern();

        foreach (var line in ReadFromFile())
        {

        }
    }

    public override void SolvePart1()
    {
        _logger.Log("Part 1 Answer");
    }

    public override void SolvePart2()
    {
        _logger.Log("Part 2 Answer");
    }
}