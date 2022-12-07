using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC22;

public partial class Day7 : Puzzle
{
    private readonly List<Folder> _allFolders = new();
    private readonly Folder _rootDirectory = new("/");

    public Day7(ILogger logger, string path) : base(logger, path) { }

    private const string UP_ONE_FOLDER = "..";
    //private const string LIST_FILES = "$ ls"; // we just skip these

    public override void Setup()
    {
        Folder currentFolder = _rootDirectory;

        foreach (var line in ReadFromFile().Skip(1))
        {
            var folderMatch = FolderPattern().Match(line);
            if (folderMatch.Success)
            {
                var folderName = folderMatch.Groups[1].ValueSpan;
                var newFolder = new Folder(folderName, currentFolder);
                currentFolder.AddFolder(newFolder);
                _allFolders.Add(newFolder);
                continue;
            }

            var fileMatch = FilePattern().Match(line);
            if (fileMatch.Success)
            {
                var newFile = new DataFile(int.Parse(fileMatch.Groups[1].ValueSpan), fileMatch.Groups[2].Value);
                currentFolder.AddFile(newFile);
                continue;
            }

            var cdMatch = ChangeDirectoryPattern().Match(line);
            if (cdMatch.Success)
            {
                var nextFolder = cdMatch.Groups[1].Value;
                if (nextFolder == UP_ONE_FOLDER) currentFolder = currentFolder.Parent!;
                else currentFolder = currentFolder.SubFolders.Single(f => f.Name == nextFolder);
            }
        }
    }

    public override void SolvePart1()
    {
        long total = _allFolders.Where(f => f.Size <= 100_000).Sum(f => f.Size);
        _logger.Log(total);
    }

    public override void SolvePart2()
    {
        var amountOver = _rootDirectory.Size - 40_000_000;
        var amountToDelete = _allFolders.Where(f => f.Size >= amountOver).Min(f => f.Size);
        _logger.Log(amountToDelete);
    }

    [GeneratedRegex("\\$ cd ([/.\\w]+)")]
    private static partial Regex ChangeDirectoryPattern();
    [GeneratedRegex("(\\d+) ([a-z.]+)")]
    private static partial Regex FilePattern();
    [GeneratedRegex("dir (\\w+)")]
    private static partial Regex FolderPattern();
}

public record DataFile(int Size, string Name);

public class Folder
{
    public readonly string Name;
    public Folder? Parent;
    
    public readonly HashSet<Folder> SubFolders = new();
    public readonly HashSet<DataFile> Files = new();

    private bool _hasCalculatedSize = false;
    private long _totalFileSize;
    private long _folderSize;
    public long Size
    {
        get
        {
            if (!_hasCalculatedSize)
            {
                foreach (var subFolder in SubFolders)
                    _folderSize += subFolder.Size;
                _folderSize += _totalFileSize;
                _hasCalculatedSize = true;
            }
            return _folderSize;
        }
    }

    public Folder(ReadOnlySpan<char> name, Folder? parent = null)
    {
        Name = name.ToString();
        Parent = parent;
    }

    public void AddFolder(Folder newFolder) => SubFolders.Add(newFolder);

    public void AddFile(DataFile newFile)
    {
        if (Files.Add(newFile))
            _totalFileSize += newFile.Size;
    }
}