using System;

namespace AoC22;

public class Day25 : Puzzle
{
    private long _sum = 0;
    public Day25(ILogger logger, string path) : base(logger, path) { }

    public override void Setup()
    {
        foreach (var line in ReadFromFile())
            _sum += SNAFU2Decimal(line);
    }

    public override void SolvePart1() => _logger.Log(Decimal2SNAFU(_sum)); // 2---1010-0=1220-=010

    public override void SolvePart2() => _logger.Log("Day 25 Complete!");

    private static long SNAFU2Decimal(string input)
    {
        long result = 0;
        long multiplier = 1;
        for (int i = input.Length - 1; i >= 0; i--)
        {
            result += multiplier * ToDecimal(input[i]);
            multiplier *= 5;
        }
        return result;

        static int ToDecimal(char c) => c switch
        {
            '=' => -2,
            '-' => -1,
            _ => c - '0'
        };
    }

    private static string Decimal2SNAFU(long value)
    {
        var result = string.Empty;
        var snafuLength = (int)Math.Ceiling(Math.Log(value, 5));
        for (int power = snafuLength; power >= 0; power--)
        {
            var place = (long)Math.Pow(5, power);
            var bounds = (place - 1) / 2; // threshold for follow-up values to sum to 0. 2,12,62... https://oeis.org/A125831
            for (int i = 2; i >= -2; i--)
            {
                var toSubtract = place * i;
                if (value - toSubtract > bounds || value - toSubtract < -bounds) continue;

                value -= toSubtract;
                if (i != 0 || result.Length != 0) // ignores leading 0's
                    result += ToSNAFU(i);
                break;
            }
        }
        return result;

        static char ToSNAFU(int i) => i switch
        {
            -2 => '=',
            -1 => '-',
            _ => (char)('0' + i),
        };
    }
}