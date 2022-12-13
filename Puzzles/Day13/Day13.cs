using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AoC22;

public partial class Day13 : Puzzle
{
    private readonly List<(Packet Left, Packet Right)> _packetPairs = new();
    private readonly List<Packet> _allPackets = new();
    public Day13(ILogger logger, string path) : base(logger, path) { }

    // Every list and number is contained as its own packet. It's packets all the way down
    private class Packet : IComparable<Packet>
    {
        public Packet Parent;
        public readonly List<Packet> Subpackets = new();
        public int? Number = null;

        public Packet(Packet parent = null) => Parent = parent;
        public Packet(int number, Packet parent = null) : this(parent) => Number = number;

        public int CompareTo(Packet other)
        {
            var result = IsInTheRightOrder(this, other);
            if (!result.HasValue) return 0;
            return result.Value ? -1 : 1;
        }
    }

    public override void Setup()
    {
        Packet left = null;
        Packet right = null;

        bool isLeftPacket = true;
        foreach (var line in ReadFromFile())
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                _packetPairs.Add((left, right));
                _allPackets.Add(left);
                _allPackets.Add(right);
                left = null;
                right = null;
                isLeftPacket = true;
                continue;
            }
            if (isLeftPacket)
            {
                left = ParseLine(line);
                isLeftPacket = false;
            }
            else
                right = ParseLine(line);
        }
        // capture the final pair in case we don't end on a newline
        if (left != null && right != null)
        {
            _packetPairs.Add((left, right));
            _allPackets.Add(left);
            _allPackets.Add(right);
        }
    }

    public override void SolvePart1()
    {
        int score = 0;
        int pairIndex = 1;
        foreach (var (Left, Right) in _packetPairs)
        {
            if (Left.CompareTo(Right) < 0) score += pairIndex;
            pairIndex++;
        }
        _logger.Log(score);
    }

    public override void SolvePart2()
    {
        var divider1 = new Packet(2);
        var divider2 = new Packet(6);
        _allPackets.Add(divider1);
        _allPackets.Add(divider2);

        _allPackets.Sort((left, right) => left.CompareTo(right));

        var index1 = _allPackets.IndexOf(divider1) + 1;
        var index2 = _allPackets.IndexOf(divider2) + 1;
        _logger.Log(index1 * index2);
    }

    private static Packet ParseLine(string line)
    {
        Packet currentPacket = null;
        var pattern = NumberPattern();
        var split = pattern.Split(line); // has all the brackets and commas
        var matches = pattern.Matches(line); // has all the numbers
        int matchIndex = 0;
        foreach (var symbols in split)
        {
            if (string.IsNullOrWhiteSpace(symbols)) continue;
            foreach (var symbol in symbols)
            {
                if (symbol == ',') continue;
                if (symbol == '[')
                {
                    var newSubpacket = new Packet(parent: currentPacket);
                    currentPacket?.Subpackets.Add(newSubpacket);
                    currentPacket = newSubpacket;
                }
                else if (symbol == ']')
                    currentPacket = currentPacket.Parent ?? currentPacket;
            }
            if (matchIndex < matches.Count)
                currentPacket.Subpackets.Add(new(int.Parse(matches[matchIndex++].ValueSpan), currentPacket));
        }
        return currentPacket;
    }

    // Recursive. True, False, or Null (if it's a tie)
    private static bool? IsInTheRightOrder(Packet left, Packet right)
    {
        // Comparing numbers. Both have a number assigned
        if (left.Number.HasValue && right.Number.HasValue)
        {
            if (left.Number.Value == right.Number.Value) return null;
            return left.Number.Value < right.Number.Value;
        }
        // Mixed types (number vs list) - push the value down into another packet layer and try again
        else if (left.Number.HasValue ^ right.Number.HasValue)
        {
            var packet = left.Number.HasValue ? left : right;
            var newValueWrapper = new Packet(packet.Number.Value, packet);
            packet.Number = null;
            packet.Subpackets.Add(newValueWrapper);

            return IsInTheRightOrder(left, right);
        }

        int maxToCompare = Math.Min(left.Subpackets.Count, right.Subpackets.Count);
        for (int i = 0; i < maxToCompare; i++)
        {
            var result = IsInTheRightOrder(left.Subpackets[i], right.Subpackets[i]);
            if (result.HasValue) return result.Value;
        }

        if (left.Subpackets.Count == right.Subpackets.Count) return null;
        return left.Subpackets.Count < right.Subpackets.Count;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberPattern();
}