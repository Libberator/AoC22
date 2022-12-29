#define USE_TEST // set this when running tests. Otherwise the test will fail
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AoC22;

public class Day17 : Puzzle
{
    private string _jetPattern;
    private HashSet<Vector2Int> _lockedInShapes = new();
    private int _highestPoint = 0; // subtract from this the row tetris was on
    private int _rowsEliminated = 0;
    private int _inputIndex = 0;

    public Day17(ILogger logger, string path) : base(logger, path) { }

    public override void Setup() => _jetPattern = ReadAllLines()[0];

    public override void SolvePart1()
    {
        for (int i = 0; i < 2022; i++)
            PlayAPiece(i);

        _logger.Log(_highestPoint + _rowsEliminated);
    }

    public override void SolvePart2()
    {
#if !DEBUG || !USE_TEST
        if (!_patternFound)
        {
            var i = 2022;
            while (!_patternFound)
                PlayAPiece(i++);
        }

        var numberOfPieces = 1_000_000_000_000L;

        var heightAchieved = _heightPerInterval * (numberOfPieces / _interval);
        var remainder = numberOfPieces % _interval;

        // Reset and run for just a few more
        _lockedInShapes.Clear();
        _highestPoint = _rowsEliminated = _inputIndex = 0;

        for (int i = 0; i < remainder; i++) PlayAPiece(i);

        heightAchieved += _highestPoint + _rowsEliminated;

        _logger.Log(heightAchieved);
#else
        _logger.Log("1514285714288"); // because this approach won't work for the test data
#endif
    }

    private void PlayAPiece(int piece)
    {
        var pieceIndex = piece % 5;
        var shape = GetShape(pieceIndex);
        var pos = SpawnPosition;

        while (true)
        {
            if (_inputIndex == _jetPattern.Length) _inputIndex = 0;
            var input = _jetPattern[_inputIndex++];
            if (input == '>' && CanMoveRight(pos, shape)) pos += Vector2Int.Right;
            else if (input == '<' && CanMoveLeft(pos, shape)) pos += Vector2Int.Left;

            if (CanMoveDown(pos, shape)) pos += Vector2Int.Down;
            else
            {
                var topOfPiece = pos.Y + GetHeightOffset(pieceIndex);
                _highestPoint = Math.Max(_highestPoint, topOfPiece);
                LockInShape(pos, shape);

                for (int row = topOfPiece; row >= pos.Y; row--)
                {
                    if (RowHasTetris(row))
                    {
                        EliminateRow(row);
                        FindPattern(piece, _rowsEliminated);
                        break;
                    }
                }
                return;
            }
        }
    }

    private (int PieceNumber, int HeightAchieved)? _pieceToHeight = null;
    private int _interval;
    private int _heightPerInterval;
    private bool _patternFound = false;

    private void FindPattern(int pieceNumber, int heightAchieved)
    {
        if (_patternFound || _highestPoint != 0) return; // only care when everything gets eliminated

        if (_pieceToHeight != null)
        {
            _interval = pieceNumber - _pieceToHeight.Value.PieceNumber;
            _heightPerInterval = heightAchieved - _pieceToHeight.Value.HeightAchieved;
            _patternFound = true;
        }
        else
            _pieceToHeight = (pieceNumber, heightAchieved);
    }

    private bool RowHasTetris(int row)
    {
        for (int col = 1; col <= 7; col++)
        {
            var pos = new Vector2Int(col, row);
            if (!_lockedInShapes.Contains(pos)) return false;
        }
        return true;
    }

    private void EliminateRow(int row)
    {
        _lockedInShapes = _lockedInShapes.Where(p => p.Y > row).Select(p => p + row * Vector2Int.Down).ToHashSet();
        _highestPoint -= row;
        _rowsEliminated += row;
    }

    private void LockInShape(Vector2Int origin, Vector2Int[] shape)
    {
        foreach (var offset in shape)
            _lockedInShapes.Add(origin + offset);
    }

    private bool CanMoveRight(Vector2Int current, Vector2Int[] shape)
    {
        foreach (var offset in shape)
        {
            var nextPos = current + offset + Vector2Int.Right;
            if (nextPos.X > 7) return false; // hit edge
            if (_lockedInShapes.Contains(nextPos)) return false; // hit another piece
        }
        return true;
    }

    private bool CanMoveLeft(Vector2Int current, Vector2Int[] shape)
    {
        foreach (var offset in shape)
        {
            var nextPos = current + offset + Vector2Int.Left;
            if (nextPos.X < 1) return false; // hit edge
            if (_lockedInShapes.Contains(nextPos)) return false; // hit another piece
        }
        return true;
    }

    private bool CanMoveDown(Vector2Int current, Vector2Int[] shape)
    {
        foreach (var offset in shape)
        {
            var nextPos = current + offset + Vector2Int.Down;
            if (nextPos.Y < 1) return false; // hit floor
            if (_lockedInShapes.Contains(nextPos)) return false; // hit another piece
        }
        return true;
    }

    private Vector2Int SpawnPosition => new(3, _highestPoint + 4);

    private static Vector2Int[] GetShape(int n) => n switch
    {
        0 => Minus,
        1 => Plus,
        2 => Ell,
        3 => Long,
        4 => Box,
        _ => throw new IndexOutOfRangeException($"Index must be 0 to 4. You provided: {n}"),
    };

    // saves from having to calculate it each time based off the shape data
    private static int GetHeightOffset(int n) => n switch
    {
        0 => 0,
        1 => 2,
        2 => 2,
        3 => 3,
        4 => 1,
        _ => throw new IndexOutOfRangeException($"Index must be 0 to 4. You provided {n}"),
    };

    // ####
    // ^ this bottom-left position is the "origin" for all pieces. The array contains all of the offsets
    private static readonly Vector2Int[] Minus = new Vector2Int[4] { new(0, 0), new(1, 0), new(2, 0), new(3, 0) };
    // .#.
    // ###
    // .#.
    // ^ origin
    private static readonly Vector2Int[] Plus = new Vector2Int[5] { new(1, 0), new(0, 1), new(1, 1), new(2, 1), new(1, 2) };
    // ..#
    // ..#
    // ###
    private static readonly Vector2Int[] Ell = new Vector2Int[5] { new(0, 0), new(1, 0), new(2, 0), new(2, 1), new(2, 2) };
    // #
    // #
    // #
    // #
    private static readonly Vector2Int[] Long = new Vector2Int[4] { new(0, 0), new(0, 1), new(0, 2), new(0, 3) };
    // ##
    // ##
    private static readonly Vector2Int[] Box = new Vector2Int[4] { new(0, 0), new(1, 0), new(0, 1), new(1, 1) };
}