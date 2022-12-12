namespace AoC22;

public class DayTemplate : Puzzle
{
    public DayTemplate(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    { 
        // Access file with ReadFromFile() for 1 line at a time or ReadAllLines() as a string dump
    }

    public override void SolvePart1()
    {
        _logger.Log("Part 1 Answer");
    }

    public override void SolvePart2()
    {
        _logger.Log("Part 2 Answer");
    }
}