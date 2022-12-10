using System;
using System.Text;

namespace AoC22;

public partial class Day10 : Puzzle
{
    public Day10(ILogger logger, string path) : base(logger, path) { }

    private const string NO_OP = "noop";
    private readonly StringBuilder _crtDisplay = new();
    private int _totalStrength;
    private int _registerX = 1;
    private int _cycle;

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
        {
            IncrementCycle();
            if (line == NO_OP) continue;

            var addxValue = int.Parse(line.Split(' ')[1]);
            IncrementCycle();
            _registerX += addxValue;
        }
    }

    public override void SolvePart1() => _logger.Log(_totalStrength);

    public override void SolvePart2() => _logger.Log(_crtDisplay.ToString());

    private void IncrementCycle()
    {
        // draw '#' if X's pos is w/in 1 from cycle's pos, else '.'
        _crtDisplay.Append(Math.Abs((_cycle % 40) - _registerX) <= 1 ? '#' : '.');

        _cycle++;

        if ((_cycle + 20) % 40 == 0) // 20, 60, 100, 140, ...
            _totalStrength += _cycle * _registerX;

        if (_cycle % 40 == 0)
            _crtDisplay.AppendLine();
    }
}