using System;
using System.Linq;
using System.Numerics;

namespace AoC22;
// Note: trying to line up row/column coordinates to get the char from the string[] combined with directional vectors is difficult
// I settled on (Row,Column) vector2ints, and a "right" move to match the prompt visuals would be Vector2Int.Up.
// Basically, compass is rotated 90 degrees where Vector2Int.Left points North for traversal. Will clean this up later so it's less confusing..
public class Day22 : Puzzle
{
    private string[] _map;
    private int[] _steps;
    private string[] _turns;

    private const char WALKABLE = '.';
    private const char WALL = '#';
    private const char SPACE = ' ';
    private const string LEFT = "L";
    private const string RIGHT = "R";

    public Day22(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var lines = ReadAllLines();
        _map = lines[..^2];
        var pattern = Utils.NumberPattern();
        var numbers = pattern.Matches(lines[^1]);
        _steps = numbers.Select(m => int.Parse(m.ValueSpan)).ToArray();
        _turns = lines[^1].Split("0123456789".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries); // pattern.Split(lines[^1]);
    }

    public override void SolvePart1()
    {
        var pos = new Vector2Int(0, _map[0].IndexOf(WALKABLE)); // (row, col)
        var dir = Vector2Int.Up; // this is our "right" to suit the data: (0, 1)

        for (int i = 0; i < _turns.Length; i++)
        {
            TakeSteps(ref pos, dir, _steps[i], _map);
            Turn(ref dir, _turns[i]);
        }
        TakeSteps(ref pos, dir, _steps[^1], _map);

        var row = pos.X + 1;
        var col = pos.Y + 1;
        var password = 1000 * row + 4 * col + DirectionValue(dir);
        _logger.Log(password);
    }

    public override void SolvePart2()
    {
        var pos = new Vector2Int(0, _map[0].IndexOf(WALKABLE)); // (row, col)
        var dir = Vector2Int.Up; // this is our "right" to suit the data: (0, 1)

        for (int i = 0; i < _turns.Length; i++)
        {
            TakeSteps3D(ref pos, ref dir, _steps[i], _map);
            Turn(ref dir, _turns[i]);
        }
        TakeSteps3D(ref pos, ref dir, _steps[^1], _map);

        var row = pos.X + 1;
        var col = pos.Y + 1;
        var password = 1000 * row + 4 * col + DirectionValue(dir);
        _logger.Log(password);
    }

    private static void TakeSteps(ref Vector2Int pos, Vector2Int dir, int amount, string[] grid)
    {
        for (int i = 0; i < amount; i++)
        {
            var nextPos = NextPosistion(pos, dir);
            if (grid[nextPos.X][nextPos.Y] == WALL) break;
            pos = nextPos;
        }

        Vector2Int NextPosistion(Vector2Int pos, Vector2Int dir)
        {
            var nextPos = pos + dir;
            if (dir == Vector2Int.Left) // moving up, potentially landed into a void or off the top edge
            {
                while (!IsOutOfBounds(nextPos))
                {
                    if (nextPos.X < 0) nextPos.X = grid.Length - 1;
                    else nextPos.X--;
                }
            }
            else if (dir == Vector2Int.Right) // moving down and landed into a void or off the bottom edge
            {
                while (!IsOutOfBounds(nextPos))
                {
                    if (nextPos.X == grid.Length) nextPos.X = 0;
                    else nextPos.X++;
                }
            } 
            else if (dir == Vector2Int.Down) // moving left, either into a void or off the left edge
            {
                while (!IsOutOfBounds(nextPos))
                {
                    if (nextPos.Y < 0) 
                        nextPos.Y = Math.Max(grid[nextPos.X].LastIndexOf(WALKABLE), grid[nextPos.X].LastIndexOf(WALL));
                    else nextPos.Y--;
                }
            }
            else if (dir == Vector2Int.Up) // moving right, either into a void or off a right edge
            {
                while (!IsOutOfBounds(nextPos))
                {
                    if (nextPos.Y == grid[nextPos.X].Length)
                        nextPos.Y = Math.Min(grid[nextPos.X].IndexOf(WALKABLE), grid[nextPos.X].IndexOf(WALL));
                    else nextPos.Y++;
                }
            }
            
            return nextPos;

            bool IsOutOfBounds(Vector2Int next)
            {
                if (next.X < 0 || next.X >= grid.Length) return false; // into the void
                if (next.Y < 0 || next.Y >= grid[next.X].Length) return false; // into the void
                if (grid[next.X][next.Y] == SPACE) return false;
                return true;
            }
        }
    }

    private static void TakeSteps3D(ref Vector2Int pos, ref Vector2Int dir, int amount, string[] grid)
    {
        for (int i = 0; i < amount; i++)
        {
            var (Pos, Dir) = NextMove(pos, dir);
            if (grid[Pos.X][Pos.Y] == WALL) break;
            pos = Pos;
            dir = Dir;
        }

        (Vector2Int Pos, Vector2Int Dir) NextMove(Vector2Int pos, Vector2Int dir)
        {
            var nextPos = pos + dir;
            var nextDir = dir;
            if (IsOutOfBounds(nextPos))
            {
                if (nextPos.X < 0) // off the top edge
                {
                    if (nextPos.Y >= 50 && nextPos.Y < 100)
                    {
                        nextPos.X = 100 + nextPos.Y; // y of 50 -> x of 150, y of 99 -> x of 199
                        nextPos.Y = 0;
                        nextDir.RotateRight();
                    }
                    else if (nextPos.Y >= 100 && nextPos.Y < 150)
                    {
                        nextPos.X = 199;
                        nextPos.Y -= 100;
                    }
                }
                else if (nextPos.Y < 0) // off the left edge
                {
                    if (nextPos.X >= 100 && nextPos.X < 150)
                    {
                        nextPos.X = 149 - nextPos.X; // 100 -> 49 and 149 -> 0
                        nextPos.Y = 50;
                        nextDir.Negate();
                    }
                    else if (nextPos.X >= 150 && nextPos.X < 200)
                    {
                        nextPos.Y = nextPos.X - 100;
                        nextPos.X = 0;
                        nextDir.RotateLeft();
                    }
                }
                else if (nextPos.X >= grid.Length) // off the bottom edge
                {
                    nextPos.X = 0;
                    nextPos.Y += 100;
                }
                else if (nextPos.X >= 0 && nextPos.X < 50)
                {
                    if (nextPos.Y < 50) // off the left void near the top
                    {
                        nextPos.X = 149 - nextPos.X; // 0 -> 149, 49 -> 100
                        nextPos.Y = 0;
                        nextDir.Negate();
                    }
                    else if (nextPos.Y >= 150) // off the right-edge near the top
                    {
                        nextPos.X = 149 - nextPos.X; // 0 -> 149 and 49 -> 100
                        nextPos.Y = 99;
                        nextDir.Negate();
                    }
                }
                else if (nextPos.X >= 50 && nextPos.X < 100)
                {
                    if (nextPos.Y < 50)
                    {
                        if (nextDir == Vector2Int.Down) // left into the center left void
                        {
                            nextPos.Y = nextPos.X - 50; // x of 50 -> y of 0, x of 99 -> y of 49
                            nextPos.X = 100;
                            nextDir.RotateLeft();
                        }
                        else if (nextPos.X == 99 && nextDir == Vector2Int.Left) // up into the center left void
                        {
                            nextPos.X = nextPos.Y + 50; // y of 0 -> x of 50, y of 49 -> x of 99
                            nextPos.Y = 50;
                            nextDir.RotateRight();
                        }
                    }
                    else if (nextPos.Y >= 100)
                    {
                        if (nextDir == Vector2Int.Up) // right into the center right void
                        {
                            nextPos.Y = nextPos.X + 50; // x of 50 -> y of 100, x of 99 -> y of 149
                            nextPos.X = 49;
                            nextDir.RotateLeft();
                        }
                        else if (nextPos.X == 50 && nextDir == Vector2Int.Right) // down into the center right void
                        {
                            nextPos.X = nextPos.Y - 50; // y of 100 -> x of 50, y of 149 -> x of 99
                            nextPos.Y = 99;
                            nextDir.RotateRight();
                        }
                    }
                }
                else if (nextPos.X >= 100 && nextPos.X < 150) // right into void connect to top-right
                {
                    nextPos.X = 149 - nextPos.X; // x of 100 -> x of 49, x of 149 -> x of 0
                    nextPos.Y = 149;
                    nextDir.Negate();
                }
                else if (nextPos.X >= 150 && nextPos.X < 200)
                {
                    if (nextDir == Vector2Int.Up) // right into bottom-center void
                    {
                        nextPos.Y = nextPos.X - 100; // x of 150 -> y of 50, x of 199 -> y of 99
                        nextPos.X = 149;
                        nextDir.RotateLeft();
                    }
                    else if (nextDir == Vector2Int.Right) // down into bottom-center void
                    {
                        nextPos.X = nextPos.Y + 100; // y of 50 -> x of 150, y of 149 -> x of 199
                        nextPos.Y = 49;
                        nextDir.RotateRight();
                    }
                }
            }

            return (nextPos, nextDir);

            bool IsOutOfBounds(Vector2Int next)
            {
                if (next.X < 0 || next.X >= grid.Length) return true;
                if (next.Y < 0 || next.Y >= grid[next.X].Length) return true;
                if (grid[next.X][next.Y] == SPACE) return true;
                return false;
            }
        }
    }

    private static void Turn(ref Vector2Int dir, string rotation)
    {
        if (rotation == LEFT) dir.RotateLeft();
        else if (rotation == RIGHT) dir.RotateRight();
    }

    private static int DirectionValue(Vector2Int dir)
    {
        if (dir == Vector2Int.Right) return 1; // (1,0) is down = 1
        if (dir == Vector2Int.Down) return 2; // (0,-1) is left = 2
        if (dir == Vector2Int.Left) return 3; // (-1,0) is up = 3
        if (dir == Vector2Int.Up) return 0; // (0,1) is right = 0
        return -1;
    }
}