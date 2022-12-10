using System;
using System.Text;

namespace AoC22;

public partial class Day10 : Puzzle
{
    public Day10(ILogger logger, string path) : base(logger, path) { }
    
    private const string NO_OP = "noop";
    private int _totalStrength = 0;
    private readonly StringBuilder _sb = new();
    private int _x = 1;
    private int _cycle = 0;
    private int Cycle
    {
        get => _cycle;
        set
        {
            // draw '#' if X's pos is w/in 1 from cycle's pos, else '.'
            _sb.Append(Math.Abs((_cycle % 40) - _x) <= 1 ? '#' : '.');

            _cycle = value;

            if ((_cycle + 20) % 40 == 0) // 20, 60, 100, 140, ...
                _totalStrength += _cycle * _x;
            
            if (_cycle % 40 == 0)
                _sb.AppendLine();
        }
    }

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
        {
            Cycle++;
            if (line != NO_OP)
            {
                var addxValue = int.Parse(line.Split(' ')[1]);
                Cycle++;
                _x += addxValue;
            }
        }
    }

    public override void SolvePart1() => _logger.Log(_totalStrength);

    public override void SolvePart2() => _logger.Log(_sb.ToString());
}