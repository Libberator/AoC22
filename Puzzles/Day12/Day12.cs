using System.Linq;

namespace AoC22;

public class Day12 : Puzzle
{
    private Grid<char> _grid;
    private Node<char> _startPoint, _endPoint;

    public Day12(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        var data = ReadAllLines();
        _grid = new(data.Select(line => line.ToCharArray()).ToArray());

        bool foundStart = false, foundEnd = false;
        int row = 0;
        foreach (var line in data)
        {
            if (!foundStart && line.Contains('S'))
            {
                var col = line.IndexOf('S');
                _startPoint = new Node<char>('a', new(row, col), _grid);
                _grid.AddNode(_startPoint);
                foundStart = true;
            }

            if (!foundEnd && line.Contains('E'))
            {
                var col = line.IndexOf('E');
                _endPoint = new Node<char>('z', new(row, col), _grid);
                _grid.AddNode(_endPoint);
                foundEnd = true;
            }

            if (foundStart && foundEnd) break;
            row++;
        }
    }

    public override void SolvePart1()
    {
        _grid.AreValidNeighbors = (self, other) => other.Value - self.Value <= 1;
        var path = Pathfinding.FindPath_AStar(_startPoint, _endPoint);
        _logger.Log(path.Count);
    }

    public override void SolvePart2()
    {
        // Reassess what are considered valid neighbors, and do a floodfill from the End point until we reach an 'a'.
        _grid.AreValidNeighbors = (self, other) => self.Value - other.Value <= 1; // swapped the nodes
        var path = Pathfinding.FindPath_Dijkstra(_endPoint, p => (p as Node<char>).Value == 'a');
        _logger.Log(path.Count);
    }
}