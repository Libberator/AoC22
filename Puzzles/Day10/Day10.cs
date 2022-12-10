using System;
using System.Text;

namespace AoC22;

public partial class Day10 : Puzzle
{
    public Day10(ILogger logger, string path) : base(logger, path) { }

    private const string NO_OP = "noop";
    private readonly StringBuilder _crtDisplay = new();
    private int _registerX = 1, _cycle, _totalStrength;

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
        {
            IncrementCycle();
            if (line == NO_OP) continue;
            IncrementCycle();
            _registerX += int.Parse(line.Split(' ')[1]);
        }
    }

    public override void SolvePart1() => _logger.Log(_totalStrength);

    public override void SolvePart2() => _logger.Log(_crtDisplay.ToString());

    private void IncrementCycle()
    {
        _crtDisplay.Append(Math.Abs((_cycle % 40) - _registerX) <= 1 ? '#' : '.'); // draw pixel
        _cycle++;
        if (_cycle % 40 == 0) _crtDisplay.AppendLine(); // new row at 40, 80, 120, 160, ...
        if ((_cycle + 20) % 40 == 0) _totalStrength += _cycle * _registerX; // 20, 60, 100, 140, ...
    }
}