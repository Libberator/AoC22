﻿using AoC22;
using System;
using System.Diagnostics;

const int START_DAY = 16;
const int STOP_DAY = 16;

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

    var timer = new Stopwatch();
    timer.Start();
    puzzle.Setup();

    puzzle.SolvePart1();

    puzzle.SolvePart2();
    logger.Log($"Total run time: {timer.ElapsedMilliseconds} ms");
}

#if !DEBUG
Console.ReadLine(); // prevent closing a build automatically
#endif