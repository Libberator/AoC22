using System;

namespace AoC22;

public interface ILogger
{
    public string? LastMessage { get; }
    public void Log(string msg);
    public void Log(int msg) => Log(msg.ToString());
    public void Log(long msg) => Log(msg.ToString());
    public void Log(char msg) => Log(msg.ToString());
    public void Log(object? msg) => Log(msg?.ToString() ?? string.Empty);
}

public class ConsoleLogger : ILogger
{
    public string? LastMessage { get; private set; }

    public void Log(string msg)
    {
        LastMessage = msg;
        Console.WriteLine(msg);
    }
}

public class TestLogger : ILogger
{
    public string? LastMessage { get; private set; }

    public void Log(string msg) => LastMessage = msg;
}

// TODO: maybe add a Write to Local Disk Logger?