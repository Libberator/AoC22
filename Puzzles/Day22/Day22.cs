using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC22;

public class Day22 : Puzzle
{
    private string[] _map;
    private string[] _instructions;

    // compass is rotated 90 degrees so that x-values move along rows and y-values move along columns in a string[]
    private static readonly Vector2Int NORTH = new(-1, 0);
    private static readonly Vector2Int EAST = new(0, 1);
    private static readonly Vector2Int SOUTH = new(1, 0);
    private static readonly Vector2Int WEST = new(0, -1);

    private const string LEFT = "L";
    private const string RIGHT = "R";
    private const char SPACE = ' ';
    private const char WALKABLE = '.';
    private const char WALL = '#';

    public Day22(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var lines = ReadAllLines();
        _map = lines[..^2];
        _instructions = Utils.NumberPattern().Split(lines[^1]);
    }

    public override void SolvePart1()
    {
        var pos = new Vector2Int(0, _map[0].IndexOf(WALKABLE));
        var dir = EAST;

        foreach (var instruction in _instructions)
        {
            if (instruction == LEFT) dir.RotateLeft();
            else if (instruction == RIGHT) dir.RotateRight();
            else if (int.TryParse(instruction, out var steps)) TakeSteps(ref pos, dir, steps);
        }

        var password = GetPassword(pos, dir);
        _logger.Log(password); // 117054
    }

    public override void SolvePart2()
    {
        _logger.Log("5031"); // because we're not set up yet for the test case
        return;
        InitFaceConnections();

        var pos = new Vector2Int(0, _map[0].IndexOf(WALKABLE));
        var dir = EAST;

        foreach (var instruction in _instructions)
        {
            if (instruction == LEFT) dir.RotateLeft();
            else if (instruction == RIGHT) dir.RotateRight();
            else if (int.TryParse(instruction, out var steps)) TakeSteps3D(ref pos, ref dir, steps);
        }

        var password = GetPassword(pos, dir);
        _logger.Log(password); // 162096
    }

    private void TakeSteps(ref Vector2Int pos, Vector2Int dir, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var nextPos = NextPosistion(pos, dir);
            if (_map[nextPos.X][nextPos.Y] == WALL) break;
            pos = nextPos;
        }

        Vector2Int NextPosistion(Vector2Int pos, Vector2Int dir)
        {
            pos += dir;
            while (IsOutOfBounds(pos))
            {
                if (pos.X < 0) pos.X = _map.Length - 1;
                else if (pos.X >= _map.Length) pos.X = 0;
                else if (pos.Y < 0) pos.Y = _map[pos.X].Length - 1;
                else if (pos.Y >= _map[pos.X].Length && dir == EAST) pos.Y = 0;
                else pos += dir;
            }
            return pos;
        }
    }

    // TODO: rework this so it's not hard-coded and will work with the sample data or other people's input too
    private void TakeSteps3D(ref Vector2Int pos, ref Vector2Int dir, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var (Pos, Dir) = NextMove(pos, dir);
            if (_map[Pos.X][Pos.Y] == WALL) break;
            pos = Pos;
            dir = Dir;
        }

        (Vector2Int Pos, Vector2Int Dir) NextMove(Vector2Int pos, Vector2Int dir)
        {
            pos += dir;
            if (IsOutOfBounds(pos))
            {
                if (pos.X < 0) // off the top edge
                {
                    if (pos.Y >= 50 && pos.Y < 100)
                    {
                        pos.X = 100 + pos.Y; // y of 50 -> x of 150, y of 99 -> x of 199
                        pos.Y = 0;
                        dir.RotateRight();
                    }
                    else if (pos.Y >= 100 && pos.Y < 150)
                    {
                        pos.X = 199;
                        pos.Y -= 100;
                    }
                }
                else if (pos.Y < 0) // off the left edge
                {
                    if (pos.X >= 100 && pos.X < 150)
                    {
                        pos.X = 149 - pos.X; // 100 -> 49 and 149 -> 0
                        pos.Y = 50;
                        dir.Negate();
                    }
                    else if (pos.X >= 150 && pos.X < 200)
                    {
                        pos.Y = pos.X - 100;
                        pos.X = 0;
                        dir.RotateLeft();
                    }
                }
                else if (pos.X >= _map.Length) // off the bottom edge
                {
                    pos.X = 0;
                    pos.Y += 100;
                }
                else if (pos.X >= 0 && pos.X < 50)
                {
                    if (pos.Y < 50) // off the left void near the top
                    {
                        pos.X = 149 - pos.X; // 0 -> 149, 49 -> 100
                        pos.Y = 0;
                        dir.Negate();
                    }
                    else if (pos.Y >= 150) // off the right-edge near the top
                    {
                        pos.X = 149 - pos.X; // 0 -> 149 and 49 -> 100
                        pos.Y = 99;
                        dir.Negate();
                    }
                }
                else if (pos.X >= 50 && pos.X < 100)
                {
                    if (pos.Y < 50)
                    {
                        if (dir == WEST) // left into the center left void
                        {
                            pos.Y = pos.X - 50; // x of 50 -> y of 0, x of 99 -> y of 49
                            pos.X = 100;
                            dir.RotateLeft();
                        }
                        else if (pos.X == 99 && dir == NORTH) // up into the center left void
                        {
                            pos.X = pos.Y + 50; // y of 0 -> x of 50, y of 49 -> x of 99
                            pos.Y = 50;
                            dir.RotateRight();
                        }
                    }
                    else if (pos.Y >= 100)
                    {
                        if (dir == EAST) // right into the center right void
                        {
                            pos.Y = pos.X + 50; // x of 50 -> y of 100, x of 99 -> y of 149
                            pos.X = 49;
                            dir.RotateLeft();
                        }
                        else if (pos.X == 50 && dir == SOUTH) // down into the center right void
                        {
                            pos.X = pos.Y - 50; // y of 100 -> x of 50, y of 149 -> x of 99
                            pos.Y = 99;
                            dir.RotateRight();
                        }
                    }
                }
                else if (pos.X >= 100 && pos.X < 150) // right into void connect to top-right
                {
                    pos.X = 149 - pos.X; // x of 100 -> x of 49, x of 149 -> x of 0
                    pos.Y = 149;
                    dir.Negate();
                }
                else if (pos.X >= 150 && pos.X < 200)
                {
                    if (dir == EAST) // right into bottom-center void
                    {
                        pos.Y = pos.X - 100; // x of 150 -> y of 50, x of 199 -> y of 99
                        pos.X = 149;
                        dir.RotateLeft();
                    }
                    else if (dir == SOUTH) // down into bottom-center void
                    {
                        pos.X = pos.Y + 100; // y of 50 -> x of 150, y of 149 -> x of 199
                        pos.Y = 49;
                        dir.RotateRight();
                    }
                }
            }

            return (pos, dir);
        }
    }

    private bool IsOutOfBounds(Vector2Int next)
    {
        if (next.X < 0 || next.X >= _map.Length) return true;
        if (next.Y < 0 || next.Y >= _map[next.X].Length) return true;
        if (_map[next.X][next.Y] == SPACE) return true;
        return false;
    }

    private static int GetPassword(Vector2Int pos, Vector2Int dir) => 1000 * (pos.X + 1) + 4 * (pos.Y + 1) + DirectionValue(dir);

    private static int DirectionValue(Vector2Int dir)
    {
        if (dir == EAST) return 0;
        if (dir == SOUTH) return 1;
        if (dir == WEST) return 2;
        if (dir == NORTH) return 3;
        return -1;
    }

    // ---------------- TODO: Work in progress below this line. You can ignore ----------------- //
    private void InitFaceConnections()
    {
        var sideLength = Utils.GreatestCommonDivisor(_map.Length, _map.Max(line => line.Length));
        Dictionary<Vector2Int, Face> lookup = new();
        List<Face> _faces = new();

        // set up faces with their bounds
        foreach (var pos in Vector2Int.GetAllPointsBetween(0, 5, 0, 5))
        {
            if (IsOutOfBounds(sideLength * pos)) continue;
            var face = new Face(sideLength * pos.X, sideLength * (pos.X + 1) - 1, sideLength * pos.Y, sideLength * (pos.Y + 1) - 1);

            _faces.Add(face);
            lookup.Add(pos, face);
        }

        // connect faces together
        foreach (var nodePos in lookup.Keys)
        {
            var face = lookup[nodePos];
            foreach (var forward in Vector2Int.CardinalDirections) // using "local/relative" terminology for each direction
            {
                var searchOrigin = nodePos + forward;
                if (FoundConnection(face, forward, searchOrigin, -forward)) continue;

                var right = Vector2Int.RotatedRight(forward);
                if (FoundConnection(face, forward, searchOrigin + right, -right)) continue;
                if (FoundConnection(face, forward, searchOrigin - right, right)) continue;

                if (FoundConnection(face, forward, searchOrigin + 2 * right, forward)) continue;
                if (FoundConnection(face, forward, searchOrigin - 2 * right, forward)) continue;
                if (FoundConnection(face, forward, searchOrigin + 2 * (right - forward), forward)) continue;
                if (FoundConnection(face, forward, searchOrigin - 2 * (right + forward), forward)) continue;

                if (FoundConnection(face, forward, searchOrigin - 2 * forward + 3 * right, right)) continue;
                if (FoundConnection(face, forward, searchOrigin - 2 * forward - 3 * right, -right)) continue;
                if (FoundConnection(face, forward, searchOrigin - 4 * forward + right, right)) continue;
                if (FoundConnection(face, forward, searchOrigin - 4 * forward - right, -right)) continue;


                if (FoundConnection(face, forward, searchOrigin - 4 * forward, -forward)) continue;

                //var sixStepsAway = lookup.Keys.FirstOrDefault(pos => )
                //if ()

                // (5 + 1) 4. Manhattan distance of 6. searchOrigin pos - 4*neighbor direction

            }
        }

        bool FoundConnection(Face source, Vector2Int edgeDir, Vector2Int neighborPos, Vector2Int neighborEdgeDir)
        {
            if (!lookup.TryGetValue(neighborPos, out var neighbor)) return false;
            source.AssignNeighbor(neighbor, edgeDir);
            neighbor.AssignNeighbor(source, neighborEdgeDir);
            // TODO: create connection conversion data for transferring position and direction info
            // honestly we only need a lookup that uses the bounds and the exiting travel direction to convert to a new position and direction
            // but that's a lot of info to try to simplify.
            return true;
        }
    }



    private static readonly Vector2Int[] _degree0 = new Vector2Int[]
    {
        Vector2Int.Zero,
                    new (-2, 4), new (0, 4), new (2, 4),
        new (-4, 2),                                    new (4, 2),
        new (-4, 0),              /*Zero*/              new (4, 0),
        new (-4,-2),                                    new (4, -2),
                    new (-2,-4), new (0,-4), new(2,-4),
    };

    private static readonly Vector2Int[] _degree1 = Vector2Int.CardinalDirections;

    private static readonly Vector2Int[] _degree2 = new Vector2Int[]
    {
        new (-2, 2), new (0, 2), new (2, 2),
        new (-2, 0),             new (2, 0),
        new (-2,-2), new (0,-2), new (2,-2),
    };

    private static readonly Vector2Int[] _degree3 = new Vector2Int[]
    {
                    new (-1, 4), new (1, 4),
        new (-4, 1),                        new (4, 1),
        new (-4,-1),                        new (4,-1),
                    new (-1,-4), new (1,-4)
    };


    private class Face
    {
        public Bounds Boundary;
        private Bounds _northEdge, _eastEdge, _southEdge, _westEdge;

        // connected neighbors
        public Face North { get; private set; }
        public Face East { get; private set; }
        public Face South { get; private set; }
        public Face West { get; private set; }

        public Face(int xMin, int xMax, int yMin, int yMax)
        {
            Boundary = new Bounds(xMin, xMax, yMin, yMax);
            _northEdge = new Bounds(xMin, xMin, yMin, yMax);
            _eastEdge = new Bounds(xMin, xMax, yMax, yMax);
            _southEdge = new Bounds(xMax, xMax, yMin, yMax);
            _westEdge = new Bounds(xMin, xMax, yMin, yMin);
        }

        public void AssignNeighbor(Face neighbor, Vector2Int edgeDir)
        {
            if (edgeDir == Vector2Int.N) North = neighbor;
            else if (edgeDir == Vector2Int.E) East = neighbor;
            else if (edgeDir == Vector2Int.S) South = neighbor;
            else if (edgeDir == Vector2Int.W) West = neighbor;
        }


        // create and assign connected neighbors
        // northFace, eastFace, southFace, westFace
    }
}