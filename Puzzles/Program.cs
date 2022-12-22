using AoC22;
using System;
using System.Diagnostics;

const int START_DAY = 19;
const int STOP_DAY = 19;

ILogger logger = new ConsoleLogger();

for (int i = START_DAY; i <= STOP_DAY; i++)
{
    Puzzle puzzle;
    try
    {
        puzzle = Utils.GetClassOfType<Puzzle>($"Day{i}", logger, Utils.FullPath(i));
        logger.Log($"\x1b[32m-- Day {i} --\x1b[0m");
    }
    catch (Exception)// e)
    {
        //logger.Log(e.Message);
        continue;
    }

    var overallTimer = new Stopwatch();
    var setupTimer = new Stopwatch();
    var part1Timer = new Stopwatch();
    var part2Timer = new Stopwatch();

    overallTimer.Start();
    setupTimer.Start();
    puzzle.Setup();
    setupTimer.Stop();

    part1Timer.Start();
    puzzle.SolvePart1();
    part1Timer.Stop();

    part2Timer.Start();
    puzzle.SolvePart2();
    part2Timer.Stop();
    overallTimer.Stop();
    logger.Log($"Setup: {setupTimer.ElapsedMilliseconds}ms. Part1: {part1Timer.ElapsedMilliseconds}ms. Part2: {part2Timer.ElapsedMilliseconds}ms. Total: {overallTimer.ElapsedMilliseconds}ms");
}

#if !DEBUG
Console.ReadLine(); // prevent closing a build automatically
#endif