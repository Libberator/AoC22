using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AoC22;

public partial class Day7 : Puzzle
{
    private readonly List<Folder> _allFolders = new();
    private readonly Folder _root = new("/");

    public Day7(ILogger logger, string path) : base(logger, path) { }

    private const string LIST_FILES = "$ ls";
    private const string UP_ONE_FOLDER = "..";

    public override void Setup()
    {
        Folder cwd = _root; // current working directory
        foreach (var line in ReadFromFile().Skip(1))
        {
            if (line == LIST_FILES) continue;

            var cdMatch = ChangeDirectoryPattern().Match(line);
            if (cdMatch.Success)
            {
                var nextFolder = cdMatch.Groups[1].Value;
                if (nextFolder == UP_ONE_FOLDER) cwd = cwd.Parent!;
                else cwd = cwd.SubFolders.First(f => f.Name == nextFolder);
                continue;
            }

            var fileMatch = FilePattern().Match(line);
            if (fileMatch.Success)
            {
                cwd.AddFile(new(fileMatch.Groups[2].Value, int.Parse(fileMatch.Groups[1].ValueSpan)));
                continue;
            }

            var folderMatch = FolderPattern().Match(line);
            if (folderMatch.Success)
            {
                var folderName = folderMatch.Groups[1].Value;
                var newFolder = new Folder(folderName, cwd);
                cwd.AddSubFolder(newFolder);
                _allFolders.Add(newFolder);
            }
        }
        //PrettyPrint();
    }

    public override void SolvePart1() => _logger.Log(_allFolders.Where(f => f.Size <= 100_000).Sum(f => f.Size));

    public override void SolvePart2()
    {
        var amountOver = _root.Size - 40_000_000;
        var amountToDelete = _allFolders.Where(f => f.Size >= amountOver).Min(f => f.Size);
        _logger.Log(amountToDelete);
    }

    // Bonus: prints out the filesystem visually
    public void PrettyPrint()
    {
        int depth = 0;
        _logger.Log("");
        Recurse(_root, ref depth);
        _logger.Log("");

        void Recurse(Folder dir, ref int depth)
        {
            _logger.Log($"{" ".Repeat(depth)}- {dir.Name} (dir)");
            depth++;
            foreach (var subFolder in dir.SubFolders)
                Recurse(subFolder, ref depth);
            foreach (var file in dir.Files)
                _logger.Log($"{" ".Repeat(depth)}- {file.Name} (file, size={file.Size})");
            depth--;
        }
    }

    public record File(string Name, int Size);

    public class Folder
    {
        public readonly string Name;
        public Folder Parent;
        public readonly HashSet<Folder> SubFolders = new();
        public readonly HashSet<File> Files = new();
        public int Size => GetSize();

        private int _totalFileSize;
        private int _folderSize;
        private bool _isDirty = true;

        public Folder(string name, Folder parent = null)
        {
            Name = name.ToString();
            Parent = parent;
        }

        public void AddSubFolder(Folder newFolder) => SubFolders.Add(newFolder);

        public void AddFile(File newFile)
        {
            if (Files.Add(newFile)) _totalFileSize += newFile.Size;
        }

        private int GetSize()
        {
            if (_isDirty)
            {
                _folderSize = _totalFileSize + SubFolders.Sum(f => f.Size);
                _isDirty = false;
            }
            return _folderSize;
        }
    }

    [GeneratedRegex("\\$ cd (.+)")]
    private static partial Regex ChangeDirectoryPattern();
    [GeneratedRegex("(\\d+) (.+)")]
    private static partial Regex FilePattern();
    [GeneratedRegex("dir (.+)")]
    private static partial Regex FolderPattern();
}