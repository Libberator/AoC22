using System.Numerics;

namespace AoC22;

public class Day12 : Puzzle
{
    private Grid<Node<char>> _grid;
    private Node<char> _startPoint;
    private Node<char> _endPoint;

    public Day12(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var data = ReadAllLines();
        var rows = data.Length;
        var cols = data[0].Length;
        _grid = new Grid<Node<char>>(rows, cols);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var c = data[row][col];
                var height = c switch
                {
                    'S' => 'a',
                    'E' => 'z',
                    _ => c
                };

                var pos = new Vector2Int(row, col);
                var point = new Node<char>(height, pos);

                _grid.Add(row, col, point);
                if (c == 'S') _startPoint = point;
                else if (c == 'E') _endPoint = point;
            }
        }
    }

    public override void SolvePart1()
    {
        foreach (var point in _grid) point.InitNeighbors(_grid, IsValidNeighbor);
        var path = Pathfinding.FindPath_AStar(_startPoint, _endPoint);
        _logger.Log(path.Count);
    }

    public override void SolvePart2()
    {
        // Reassign the neighbors to do a floodfill starting from the End point until we reach an 'a'.
        foreach (var point in _grid) point.InitNeighbors(_grid, IsValidNeighborReverse);
        var path = Pathfinding.FindPath_Dijkstra(_endPoint, p => p.Value == 'a');
        _logger.Log(path.Count);
    }

    private static bool IsValidNeighbor(Node<char> self, Node<char> other) => other.Value - self.Value <= 1;
    private static bool IsValidNeighborReverse(Node<char> self, Node<char> other) => other.Value - self.Value >= -1;
}