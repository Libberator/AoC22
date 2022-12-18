using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC22;

public partial class Day18 : Puzzle
{
    private readonly List<Vector3Int> _positions = new();
    private Bounds3D _bounds;
    public Day18(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var pattern = Utils.NumberPattern();
        foreach (var line in ReadFromFile())
        {
            var numbers = pattern.Matches(line);
            var pos = new Vector3Int(int.Parse(numbers[0].ValueSpan), int.Parse(numbers[1].ValueSpan), int.Parse(numbers[2].ValueSpan));
            _positions.Add(pos);
        }
    }

    public override void SolvePart1()
    {
        _bounds = new Bounds3D(_positions.First());
        long totalSideCount = 6 * _positions.Count;
        long sidesHidden = 0;
        foreach (var pos in _positions)
        {
            _bounds.Encapsulate(pos);
            foreach (var dir in Vector3Int.AllDirections)
                if (_positions.Contains(pos + dir)) sidesHidden++;
        }
        _logger.Log(totalSideCount - sidesHidden);
    }

    public override void SolvePart2()
    {
        var bounds = new Bounds3D(_bounds);
        bounds.Expand(1);
        int totalSurfaceArea = 0;
        SurfaceAreaFloodFill(bounds, ref totalSurfaceArea);
        _logger.Log(totalSurfaceArea);
    }

    private void SurfaceAreaFloodFill(Bounds3D bounds, ref int totalSurfaceArea)
    {
        var toSearch = new List<Vector3Int>() { bounds.Min };
        var processed = new List<Vector3Int>();

        while (toSearch.Count > 0)
        {
            var current = toSearch[0];

            toSearch.Remove(current);
            processed.Add(current);

            foreach (var dir in Vector3Int.AllDirections)
            {
                var pos = current + dir;

                if (!bounds.Contains(pos)) continue;
                if (_positions.Contains(pos))
                {
                    totalSurfaceArea++;
                    continue;
                }
                if (processed.Contains(pos)) continue;
                if (!toSearch.Contains(pos))
                    toSearch.Add(pos);
            }
        }
    }
}