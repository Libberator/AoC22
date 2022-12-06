using AoC22;
using System;

const int START_DAY = 6;
const int STOP_DAY = 25;

var logger = new ConsoleLogger();

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

    puzzle.Setup();
    
    puzzle.SolvePart1();

    puzzle.SolvePart2();
}

#if !DEBUG
Console.ReadLine(); // prevent closing a build automatically
#endif