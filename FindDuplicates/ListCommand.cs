using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FindDuplicates;

internal sealed class ListCommand : AsyncCommand<ListSettings>
{
    private readonly Dictionary<string, List<FileInfo>> _fileHashMap = new();

    public override async Task<int> ExecuteAsync(CommandContext context, ListSettings settings)
    {
        var inputDirectory = new DirectoryInfo(settings.InputPath);
        if (!inputDirectory.Exists)
        {
            AnsiConsole.MarkupLine($"[red]{inputDirectory} does not exist![/]");
            return -1;
        }

        AnsiConsole.MarkupLineInterpolated($"Searching [cyan]{inputDirectory.FullName}[/]");
        AnsiConsole.MarkupLine($"Recursive mode is {(settings.Recursive ? "[green]ON" : "[red]OFF")}[/]");

        await SearchAsync(inputDirectory, settings);

        AnsiConsole.WriteLine();

        int duplicates = 0;
        foreach ((string hash, List<FileInfo> files) in _fileHashMap)
        {
            int fileCount = files.Count;

            if (fileCount > 1)
            {
                duplicates += fileCount;
                AnsiConsole.MarkupLineInterpolated($"Found [cyan]{fileCount}[/] identical files");
                AnsiConsole.MarkupLineInterpolated($"SHA512 [green]{hash}[/]:");

                foreach (FileInfo file in files)
                    AnsiConsole.MarkupLineInterpolated($"- {file.FullName}");

                AnsiConsole.WriteLine();
            }
        }

        if (duplicates == 0)
            AnsiConsole.MarkupLine("[green]No duplicates found![/]");
        else
            AnsiConsole.MarkupLineInterpolated($"[yellow]Found [cyan]{duplicates}[/] duplicates![/]");

        return 0;
    }

    private async Task SearchAsync(DirectoryInfo inputDirectory, ListSettings settings)
    {
        var tasks = new List<Task>();
        var directoryStack = new Stack<DirectoryInfo>([inputDirectory]);
        while (directoryStack.Count > 0)
        {
            DirectoryInfo currentDirectory = directoryStack.Pop();
            string relativePath = Path.GetRelativePath(inputDirectory.FullName, currentDirectory.FullName);
            if (relativePath != ".")
                AnsiConsole.MarkupLineInterpolated($"Searching [cyan]{relativePath}[/]");

            if (settings.Recursive)
            {
                foreach (DirectoryInfo childDirectory in currentDirectory.EnumerateDirectories())
                    directoryStack.Push(childDirectory);
            }

            foreach (FileInfo file in currentDirectory.EnumerateFiles())
            {
                string relativeFilePath = Path.GetRelativePath(inputDirectory.FullName, file.FullName);
                AnsiConsole.MarkupLineInterpolated($"Checking hash for [cyan]{relativeFilePath}[/]");
                tasks.Add(Task.Run(() => ProcessFile(file)));
            }
        }

        await Task.WhenAll(tasks);
    }

    private void ProcessFile(FileInfo file)
    {
        Span<byte> buffer = stackalloc byte[64];
        using FileStream stream = file.OpenRead();
        using BufferedStream bufferedStream = new BufferedStream(stream, 1048576 /* 1MB */);
        SHA512.HashData(bufferedStream, buffer);
        string hash = ByteSpanToString(buffer);
        Trace.WriteLine($"{file.FullName}: {hash}");

        if (!_fileHashMap.TryGetValue(hash, out List<FileInfo>? cache))
            _fileHashMap[hash] = cache = new List<FileInfo>();

        lock (cache)
            cache.Add(file);
    }

    private static string ByteSpanToString(ReadOnlySpan<byte> buffer)
    {
        var builder = new StringBuilder();

        foreach (byte b in buffer)
            builder.Append($"{b:X2}");

        return builder.ToString();
    }
}
