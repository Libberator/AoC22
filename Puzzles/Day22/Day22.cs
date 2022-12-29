using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC22;

public class Day22 : Puzzle
{
    private string[] _map;
    private string[] _instructions;
    private Vector2Int _startPosition;
    private readonly Dictionary<Vector2Int, List<Edge>> _edgeConnections = new() { { NORTH, new() }, { EAST, new() }, { SOUTH, new() }, { WEST, new() } };

    // compass is rotated 90 degrees so that x-values move along rows and y-values move along columns in a string[]
    private static readonly Vector2Int NORTH = new(-1, 0), EAST = new(0, 1), SOUTH = new(1, 0), WEST = new(0, -1);

    private const string LEFT = "L", RIGHT = "R";
    private const char SPACE = ' ', WALKABLE = '.', WALL = '#';

    public Day22(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var lines = ReadAllLines();
        _map = lines[..^2];
        _instructions = Utils.NumberPattern().Split(lines[^1]);
        _startPosition = new Vector2Int(0, _map[0].IndexOf(WALKABLE));
    }

    public override void SolvePart1()
    {
        var pos = _startPosition;
        var dir = EAST;

        foreach (var instruction in _instructions)
        {
            if (instruction == LEFT) dir.RotateLeft();
            else if (instruction == RIGHT) dir.RotateRight();
            else if (int.TryParse(instruction, out var steps)) TakeSteps(ref pos, dir, steps);
        }

        var password = GetPassword(pos, dir);
        _logger.Log(password);
    }

    public override void SolvePart2()
    {
        InitEdgeConnections();
        var pos = _startPosition;
        var dir = EAST;

        foreach (var instruction in _instructions)
        {
            if (instruction == LEFT) dir.RotateLeft();
            else if (instruction == RIGHT) dir.RotateRight();
            else if (int.TryParse(instruction, out var steps)) TakeSteps3D(ref pos, ref dir, steps);
        }

        var password = GetPassword(pos, dir);
        _logger.Log(password);
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

    private void TakeSteps3D(ref Vector2Int pos, ref Vector2Int dir, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var (NextPos, NextDir) = NextMove(pos, dir);
            if (_map[NextPos.X][NextPos.Y] == WALL) break;
            pos = NextPos;
            dir = NextDir;
        }

        (Vector2Int NextPos, Vector2Int NextDir) NextMove(Vector2Int pos, Vector2Int dir)
        {
            var nextPos = pos + dir;
            if (IsOutOfBounds(nextPos))
            {
                foreach (var edge in _edgeConnections[dir])
                {
                    if (edge.Contains(pos))
                    {
                        nextPos = edge.GetNextPosition(pos);
                        dir = edge.NextDirection;
                        break;
                    }
                }
            }
            return (nextPos, dir);
        }
    }

    private bool IsOutOfBounds(Vector2Int pos)
    {
        if (pos.X < 0 || pos.X >= _map.Length) return true;
        if (pos.Y < 0 || pos.Y >= _map[pos.X].Length) return true;
        if (_map[pos.X][pos.Y] == SPACE) return true;
        return false;
    }

    private static int GetPassword(Vector2Int pos, Vector2Int dir) => 1000 * (pos.X + 1) + 4 * (pos.Y + 1) + DirectionValue(dir);

    private static int DirectionValue(Vector2Int dir) => dir switch
    {
        { X:-1, Y: 0 } => 3, // NORTH
        { X: 0, Y:-1 } => 2, // WEST
        { X: 1, Y: 0 } => 1, // SOUTH
        { X: 0, Y: 1 } => 0, // EAST
        _ => -1,
    };

    #region Connecting Edges For a Cube Net

    private void InitEdgeConnections()
    {
        var sideLength = Utils.GreatestCommonDivisor(_map.Length, _map.Max(line => line.Length));
        var startingFacePos = _startPosition / sideLength;

        // Corner IDs. These get rotated around as the cube rolls to simulate unfolding
        int topLeft = 0, topRight = 1, botLeft = 2, botRight = 3; // the main 4 IDs that will be "stamped" to each face to line up corners/edges
        int topLeftFront = 4, topRightFront = 5, botLeftFront = 6, botRightFront = 7; // the other IDs directly above (or in front) to form the cube

        HashSet<Vector2Int> visited = new();
        Dictionary<int, (Vector2Int Start, Vector2Int End, Vector2Int ExitDirection)> unmatchedPairs = new();
        
        Recurse(startingFacePos);

        void Recurse(Vector2Int pos, Vector2Int dir = default)
        {
            visited.Add(pos);
            RollCube(dir);

            var topLeftPos = sideLength * pos;
            var topRightPos = topLeftPos + new Vector2Int(0, sideLength - 1);
            var botLeftPos = topLeftPos + new Vector2Int(sideLength - 1, 0);
            var botRightPos = botLeftPos + new Vector2Int(0, sideLength - 1);

            MatchEdges(topLeftPos, topRightPos, topLeft, topRight, NORTH);
            MatchEdges(topRightPos, botRightPos, topRight, botRight, EAST);
            MatchEdges(botLeftPos, botRightPos, botLeft, botRight, SOUTH);
            MatchEdges(topLeftPos, botLeftPos, topLeft, botLeft, WEST);

            foreach (var direction in Vector2Int.CardinalDirections)
            {
                var nextPos = pos + direction;
                if (IsOutOfBounds(sideLength * nextPos)) continue;
                if (visited.Contains(nextPos)) continue;
                Recurse(nextPos, direction);
            }

            RollCube(dir, undo: true);
        }

        void MatchEdges(Vector2Int start, Vector2Int end, int startCornerId, int endCornerId, Vector2Int direction)
        {
            if (startCornerId > endCornerId) (start, end) = (end, start); //to ensure we match the right corner pairs together
            var edgeId = 1 << startCornerId | 1 << endCornerId; // unique value that only 2 faces will connect to

            if (unmatchedPairs.TryGetValue(edgeId, out var pair))
            {
                _edgeConnections[direction].Add(new(start, end, pair.Start, pair.End, -pair.ExitDirection));
                _edgeConnections[pair.ExitDirection].Add(new(pair.Start, pair.End, start, end, -direction));
            }
            else
                unmatchedPairs.Add(edgeId, (start, end, direction));
        }

        void RollCube(Vector2Int dir, bool undo = false)
        {
            switch (dir)
            {
                case { X:-1, Y: 0 }: if (undo) RollDown();  else RollUp();    break; // North
                case { X: 0, Y: 1 }: if (undo) RollLeft();  else RollRight(); break; // East
                case { X: 1, Y: 0 }: if (undo) RollUp();    else RollDown();  break; // South
                case { X: 0, Y:-1 }: if (undo) RollRight(); else RollLeft();  break; // West
                default: break;
            }
        }

        void RollRight()
        {
            (topLeft, topRight, topRightFront, topLeftFront) = (topRight, topRightFront, topLeftFront, topLeft);
            (botLeft, botRight, botRightFront, botLeftFront) = (botRight, botRightFront, botLeftFront, botLeft);
        }

        void RollLeft()
        {
            (topLeft, topRight, topRightFront, topLeftFront) = (topLeftFront, topLeft, topRight, topRightFront);
            (botLeft, botRight, botRightFront, botLeftFront) = (botLeftFront, botLeft, botRight, botRightFront);
        }

        void RollDown()
        {
            (topLeft, botLeft, botLeftFront, topLeftFront) = (botLeft, botLeftFront, topLeftFront, topLeft);
            (topRight, botRight, botRightFront, topRightFront) = (botRight, botRightFront, topRightFront, topRight);
        }

        void RollUp()
        {
            (topLeft, botLeft, botLeftFront, topLeftFront) = (topLeftFront, topLeft, botLeft, botLeftFront);
            (topRight, botRight, botRightFront, topRightFront) = (topRightFront, topRight, botRight, botRightFront);
        }
    }

    private class Edge
    {
        private readonly Vector2Int _fromStart, _fromEnd;
        private readonly Vector2Int _toStart, _toEnd;

        public readonly Vector2Int NextDirection;

        public Edge(Vector2Int fromStart, Vector2Int fromEnd, Vector2Int toStart, Vector2Int toEnd, Vector2Int nextDir)
        {
            _fromStart = fromStart;
            _fromEnd = fromEnd;
            _toStart = toStart;
            _toEnd = toEnd;
            NextDirection = nextDir;
        }

        public bool Contains(Vector2Int pos)
        {
            if (_fromStart.X == _fromEnd.X)
                return pos.X == _fromStart.X && pos.Y >= Math.Min(_fromStart.Y, _fromEnd.Y) && pos.Y <= Math.Max(_fromStart.Y, _fromEnd.Y);
            if (_fromStart.Y == _fromEnd.Y)
                return pos.Y == _fromStart.Y && pos.X >= Math.Min(_fromStart.X, _fromEnd.X) && pos.X <= Math.Max(_fromStart.X, _fromEnd.X);
            return false;
        }

        public Vector2Int GetNextPosition(Vector2Int pos) => Vector2Int.Map(_fromStart, _fromEnd, _toStart, _toEnd, pos);
    }

    #endregion Connecting Edges For a Cube Net
}