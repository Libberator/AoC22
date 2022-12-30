using System.Collections.Generic;

namespace AoC22;

public interface IDay
{
    public void Setup();
    public void SolvePart1();
    public void SolvePart2();
}

public abstract class Puzzle : IDay
{
    protected readonly ILogger _logger;
    protected readonly string _path;
    public Puzzle(ILogger logger, string path)
    {
        _logger = logger;
        _path = path;
    }

    protected string[] ReadAllLines() => Utils.ReadAllLines(_path);
    protected IEnumerable<string> ReadFromFile(bool ignoreWhiteSpace = false) => Utils.ReadFrom(_path, ignoreWhiteSpace);

    public abstract void Setup();
    public abstract void SolvePart1();
    public abstract void SolvePart2();
}