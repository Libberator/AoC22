using System.Collections.Generic;
using System.Linq;

namespace AoC22;

public class Day1 : Puzzle
{
    private readonly List<int> _data = new();

    public Day1(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        _data.Clear();
        _data.Add(0);
        foreach (var line in Utils.ReadFrom(_path))
        {
            if (string.IsNullOrEmpty(line))
                _data.Add(0);
            else
                _data[^1] += int.Parse(line);
        }
    }

    public override void SolvePart1() => _logger.Log(_data.Max());

    public override void SolvePart2() => _logger.Log(_data.OrderDescending().Take(3).Sum());
}