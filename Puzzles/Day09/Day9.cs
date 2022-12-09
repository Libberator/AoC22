using System.Collections.Generic;
using System.Numerics;

namespace AoC22;

public partial class Day9 : Puzzle
{
    private record Move(Vector2Int Direction, int Amount);
    private readonly List<Move> _moves = new();

    public Day9(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
            _moves.Add(new(AsDirection(line[0]), int.Parse(line[2..])));
    }

    public override void SolvePart1()
    {
        var knots = new Vector2Int[2];
        HashSet<Vector2Int> trail = new() { Vector2Int.Zero };
        foreach (var move in _moves)
            ApplyMove(knots, trail, move);
        _logger.Log(trail.Count);
    }

    public override void SolvePart2()
    {
        var knots = new Vector2Int[10];
        HashSet<Vector2Int> trail = new() { Vector2Int.Zero };
        foreach (var move in _moves)
            ApplyMove(knots, trail, move);
        _logger.Log(trail.Count);
    }

    private static void ApplyMove(Vector2Int[] knots, HashSet<Vector2Int> trail, Move move)
    {
        for (int n = 0; n < move.Amount; n++)
        {
            knots[0] += move.Direction;
            for (int i = 1; i < knots.Length; i++)
            {
                if (knots[i].DistanceChebyshev(knots[i - 1]) <= 1) break;
                knots[i] += (knots[i - 1] - knots[i]).Clamp(-1, 1, -1, 1);
                if (i == knots.Length - 1) trail.Add(knots[i]);
            }
        }
    }

    private static Vector2Int AsDirection(char dir) => dir switch
    {
        'U' => Vector2Int.Up,
        'R' => Vector2Int.Right,
        'D' => Vector2Int.Down,
        'L' => Vector2Int.Left,
        _ => Vector2Int.Zero
    };
}