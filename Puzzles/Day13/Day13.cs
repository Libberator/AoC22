using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AoC22;

public partial class Day13 : Puzzle
{
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

        public int CompareTo(Packet other) => ComparePackets(this, other);
    }

    public override void Setup()
    {
        foreach (var line in ReadFromFile(ignoreWhiteSpace: true))
            _allPackets.Add(ParseLine(line));
    }

    public override void SolvePart1()
    {
        int score = 0;
        int pairIndex = 1;
        for (int i = 0; i < _allPackets.Count; i += 2)
        {
            if (_allPackets[i].CompareTo(_allPackets[i + 1]) == -1) score += pairIndex;
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

        _allPackets.Sort();

        var index1 = _allPackets.IndexOf(divider1) + 1;
        var index2 = _allPackets.IndexOf(divider2) + 1;
        _logger.Log(index1 * index2);
    }

    private static Packet ParseLine(string line)
    {
        Packet currentPacket = null;
        var split = NumberPattern().Split(line); // has all the brackets and commas
        var matches = NumberPattern().Matches(line); // has all the numbers
        int matchIndex = 0;
        foreach (var symbols in split)
        {
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
                currentPacket.Subpackets.Add(new Packet(int.Parse(matches[matchIndex++].ValueSpan), currentPacket));
        }
        return currentPacket;
    }

    // Recursive. -1 = left is correctly to the left of right. +1 = they're swapped. 0 = tied.
    private static int ComparePackets(Packet left, Packet right)
    {
        // Both have a number assigned
        if (left.Number.HasValue && right.Number.HasValue)
        {
            if (left.Number.Value == right.Number.Value) return 0;
            return left.Number.Value < right.Number.Value ? -1 : 1;
        }

        // Mixed types (number vs list) - push the value down into another packet layer
        if (left.Number.HasValue ^ right.Number.HasValue)
        {
            var packet = left.Number.HasValue ? left : right;
            var newValueWrapper = new Packet(packet.Number.Value, packet);
            packet.Number = null;
            packet.Subpackets.Add(newValueWrapper);
        }

        // Both have a list of more packets.
        int maxToCompare = Math.Min(left.Subpackets.Count, right.Subpackets.Count);
        for (int i = 0; i < maxToCompare; i++)
        {
            var result = ComparePackets(left.Subpackets[i], right.Subpackets[i]);
            if (result != 0) return result;
        }
        if (left.Subpackets.Count == right.Subpackets.Count) return 0;
        return left.Subpackets.Count < right.Subpackets.Count ? -1 : 1;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberPattern();
}