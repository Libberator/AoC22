using System.Collections.Generic;
using System.Linq;

namespace AoC22;

public class Day4 : Puzzle
{
    private List<int[]> _data = new();

    public Day4(ILogger logger, string path) : base(logger, path) { }

    public override void Setup() => _data = Utils.ReadFrom(_path).Select(line => line.Split('-', ',').ConvertToInts()).ToList();

    public override void SolvePart1() => _logger.Log(_data.Count(sets => FullyContains(sets[..2], sets[2..])));

    public override void SolvePart2() => _logger.Log(_data.Count(sets => PartiallyOverlaps(sets[..2], sets[2..])));

    private static bool FullyContains(int[] a, int[] b) => (a[0] <= b[0] && a[1] >= b[1]) || (b[0] <= a[0] && b[1] >= a[1]);

    private static bool PartiallyOverlaps(int[] a, int[] b) => a[0] <= b[1] && a[1] >= b[0];
}