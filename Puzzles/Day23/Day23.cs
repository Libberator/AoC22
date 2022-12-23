using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AoC22;

public class Day23 : Puzzle
{
    private const char ELF = '#';
    private static readonly Vector2Int[] NORTH_GROUP = new Vector2Int[3] { Vector2Int.N, Vector2Int.NE, Vector2Int.NW };
    private static readonly Vector2Int[] SOUTH_GROUP = new Vector2Int[3] { Vector2Int.S, Vector2Int.SE, Vector2Int.SW };
    private static readonly Vector2Int[] WEST_GROUP = new Vector2Int[3] { Vector2Int.W, Vector2Int.NW, Vector2Int.SW };
    private static readonly Vector2Int[] EAST_GROUP = new Vector2Int[3] { Vector2Int.E, Vector2Int.NE, Vector2Int.SE };
    
    private static readonly Queue<Vector2Int[]> _regionsToCheck = new( new[] { NORTH_GROUP, SOUTH_GROUP, WEST_GROUP, EAST_GROUP } );
    
    private readonly HashSet<Vector2Int> _positions = new();
    private readonly Dictionary<Vector2Int, int> _targetCount = new();
    private readonly Dictionary<Vector2Int, Vector2Int> _proposedMoves = new();

    public Day23(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var data = ReadAllLines();
        int y = data.Length - 1;
        foreach (var line in data)
        {
            for (int x = 0; x < line.Length; x++)
                if (line[x] == ELF) _positions.Add(new Vector2Int(x, y));
            y--;
        }
    }

    public override void SolvePart1()
    {
        for (int i = 0; i < 10; i++) DoRound(_positions);

        var bounds = new Bounds();
        foreach (var pos in _positions)
            bounds.Encapsulate(pos);
        
        var area = (bounds.Width + 1) * (bounds.Height + 1);
        var elves = _positions.Count;
        _logger.Log(area - elves);
    }

    public override void SolvePart2()
    {
        var roundNumber = 11;
        while(DoRound(_positions)) roundNumber++;
        _logger.Log(roundNumber);
    }

    private bool DoRound(HashSet<Vector2Int> grid)
    {
        // First Half - Check the 8 direction, if too close to another elf, propose a spot to move to
        foreach (var pos in grid)
        {
            if (!IsNextToAnElf(pos, grid)) continue;
            if (!TryGetNextPosition(pos, grid, out var proposedPosition)) continue;
            _proposedMoves[pos] = proposedPosition;
            _targetCount.AddToExistingOrCreate(proposedPosition, 1);
        }

        if (_targetCount.All(kvp => kvp.Value > 1)) return false;

        // Second Half - Move each elf to their proposed position, if they're the only 1 going there
        foreach (var proposal in _proposedMoves)
        {
            if (_targetCount[proposal.Value] > 1) continue;
            grid.Remove(proposal.Key);
            grid.Add(proposal.Value);
        }

        // Finally - Cycle the first region from the front to the back
        _regionsToCheck.Enqueue(_regionsToCheck.Dequeue());
        _proposedMoves.Clear();
        _targetCount.Clear();
        return true;
    }

    private static bool TryGetNextPosition(Vector2Int pos, HashSet<Vector2Int> grid, out Vector2Int nextPos)
    {
        nextPos = Vector2Int.Zero;
        foreach (var region in _regionsToCheck)
        {
            nextPos = pos + region[0];
            if (region.All(dir => !grid.Contains(pos + dir))) return true;
        }
        return false;
    }

    private static bool IsNextToAnElf(Vector2Int pos, HashSet<Vector2Int> grid)
    {
        foreach (var dir in Vector2Int.CompassPoints)
            if (grid.Contains(pos + dir)) return true;
        return false;
    }

    private void PrintGrid(HashSet<Vector2Int> grid)
    {
        var bounds = new Bounds();
        foreach (var pos in grid)
            bounds.Encapsulate(pos);

        var sb = new StringBuilder();
        for (int y = bounds.YMax; y >= bounds.YMin; y--)
        {
            for (int x = bounds.XMin; x <= bounds.XMax; x++)
                sb.Append(grid.Contains(new Vector2Int(x, y)) ? '#' : '.');
            sb.AppendLine();
        }
        _logger.Log(sb.ToString());
    }
}