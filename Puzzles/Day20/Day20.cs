using System.Collections.Generic;
using System.Linq;

namespace AoC22;

public class Day20 : Puzzle
{
    private List<Wrapper<int>> _data;
    private Wrapper<int> _marker;
    private int _count;
    public Day20(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        _data = ReadAllLines().Select(line => new Wrapper<int>(int.Parse(line))).ToList();
        _marker = _data.First(i => i.Value == 0);
        _count = _data.Count;
    }

    public override void SolvePart1()
    {
        var data = new List<Wrapper<int>>(_data);
        MixByValue(data);
        var result = GetGroveCoordinates(data);
        _logger.Log(result);
    }

    public override void SolvePart2()
    {
        var data = new List<Wrapper<int>>(_data);
        var decryptionKey = 811_589_153;
        var multiplier = decryptionKey % (_count - 1);

        for (int i = 0; i < 10; i++)
            MixByValue(data, multiplier);

        var result = GetGroveCoordinates(data);
        _logger.Log(result * (long)decryptionKey); // cast to long to avoid integer overflow
    }

    private void MixByValue(List<Wrapper<int>> movingData, int multiplier = 1)
    {
        foreach (var item in _data)
        {
            var value = item.Value * multiplier;
            var index = movingData.IndexOf(item);
            var nextIndex = (index + value).Mod(_count - 1);
            movingData.Remove(item);
            movingData.Insert(nextIndex, item);
        }
    }

    private int GetGroveCoordinates(List<Wrapper<int>> data)
    {
        var indexOfZero = data.IndexOf(_marker);
        var thousandth = data[(indexOfZero + 1000) % _count].Value;
        var twoThousandth = data[(indexOfZero + 2000) % _count].Value;
        var threeThousandth = data[(indexOfZero + 3000) % _count].Value;
        return thousandth + twoThousandth + threeThousandth;
    }

    // This is so that we can have ints as a reference type for quick IndexOf lookups
    private class Wrapper<T>
    {
        public readonly T Value;
        public Wrapper(T value) => Value = value;
    }
}