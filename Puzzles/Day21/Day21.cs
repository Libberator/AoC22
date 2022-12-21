using System;
using System.Collections.Generic;

namespace AoC22;

public class Day21 : Puzzle
{
    private readonly Dictionary<string, Monkey> _monkeys = new();
    private Monkey _root;
    private Monkey _human;
    public Day21(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        Monkey monkey;
        foreach (var line in ReadFromFile())
        {
            var split = line.Split(": ");
            var id = split[0];
            var job = split[1];
            if (double.TryParse(job, out var value))
                monkey = new(id, value);
            else
            {
                var operation = job[5];
                var left = job[..4];
                var right = job[^4..];
                monkey = new(id, operation, left, right);
            }

            _monkeys.Add(id, monkey);
            if (id == "root") _root = monkey;
            else if (id == "humn") _human = monkey;
        }
    }

    public override void SolvePart1() => _logger.Log(_root.GetValue(_monkeys));

    // Uses Newton-Raphson's Root-Finding to converge to 0. 
    // x1 = x0 - f(x0) / f'(x0)
    public override void SolvePart2()
    {
        _root.Operation = '-'; // equality (=) is just subtraction and comparing against 0

        var x0 = _human.Value;
        var y0 = _root.GetValue(_monkeys);
        double x1 = x0 + y0;
        double y1 = 1;

        while (y1 != 0)
        {
            _human.Value = x1;
            try { y1 = _root.GetValue(_monkeys); } // catch any divide-by-zeros, if at all possible
            catch (Exception e) { _logger.Log($"Error on {x1}: {e.Message}"); }
            var slope = (y1 - y0) / (x1 - x0);
            (x0, x1) = (x1, x0 - y0 / slope);
            y0 = y1;
        }
        _logger.Log(x0);
    }

    private class Monkey
    {
        public readonly string Id;
        public double Value; // Using doubles because whole numbers won't be as accurate for part 2 due to integer (or long) division
        public char Operation;
        public readonly string Left, Right;

        public Monkey(string id) => Id = id;
        public Monkey(string id, double value) : this(id) => Value = value;
        public Monkey(string id, char operation, string left, string right) : this(id)
        {
            Operation = operation;
            Left = left;
            Right = right;
        }

        public double GetValue(Dictionary<string, Monkey> lookup)
        {
            return Operation switch
            {
                '+' => lookup[Left].GetValue(lookup) + lookup[Right].GetValue(lookup),
                '-' => lookup[Left].GetValue(lookup) - lookup[Right].GetValue(lookup),
                '*' => lookup[Left].GetValue(lookup) * lookup[Right].GetValue(lookup),
                '/' => lookup[Left].GetValue(lookup) / lookup[Right].GetValue(lookup),
                _ => Value,
            };
        }
    }
}