using System;
using System.Linq;

namespace AoC22;

public class Day4 : Puzzle
{
    private BoundsPair[] _data = Array.Empty<BoundsPair>();

    public Day4(ILogger logger, string path) : base(logger, path) { }

    public override void Setup() => _data = ReadFromFile().Select(line => CreateBounds(line.Split('-', ',').ToIntArray())).ToArray();
    public override void SolvePart1() => _logger.Log(_data.Count(pair => pair.A.Contains(pair.B) || pair.B.Contains(pair.A)));
    public override void SolvePart2() => _logger.Log(_data.Count(pair => pair.A.Overlaps(pair.B)));

    private record BoundsPair(Bounds A, Bounds B);
    private static BoundsPair CreateBounds(int[] input) => new(new Bounds(input[0], input[1], 0, 0), new Bounds(input[2], input[3], 0, 0));
}