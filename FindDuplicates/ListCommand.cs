using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FindDuplicates;

internal sealed class ListCommand : AsyncCommand<ListSettings>
{
    private readonly ConcurrentDictionary<string, List<FileInfo>> _fileHashMap = new();

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

        await AnsiConsole.Status()
            .StartAsync("Waiting to hash files...", DoHashWaitAsync)
            .ConfigureAwait(false);

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

        async Task DoHashWaitAsync(StatusContext ctx)
        {
            await WaitForHashCompletionAsync(settings, inputDirectory, ctx);
        }
    }

    private async Task WaitForHashCompletionAsync(ListSettings settings,
        DirectoryInfo inputDirectory,
        StatusContext ctx)
    {
        var tasks = new List<Task>();
        SearchDuplicates(inputDirectory, settings, tasks);
        await Task.Run(() =>
        {
            int incompleteTasks;
            do
            {
                incompleteTasks = tasks.Count(t => !t.IsCompleted);
                ctx.Status($"Waiting to hash {incompleteTasks} {(incompleteTasks == 1 ? "file" : "files")}...");
                ctx.Refresh();
            } while (tasks.Count > 0 && incompleteTasks > 0);

            ctx.Status("Hash complete");
        }).ConfigureAwait(false);
    }

    private void SearchDuplicates(DirectoryInfo inputDirectory, ListSettings settings, ICollection<Task> tasks)
    {
        var directoryStack = new Stack<DirectoryInfo>([inputDirectory]);
        while (directoryStack.Count > 0)
        {
            DirectoryInfo currentDirectory = directoryStack.Pop();
            string relativePath = Path.GetRelativePath(inputDirectory.FullName, currentDirectory.FullName);
            if (relativePath != ".")
                AnsiConsole.MarkupLineInterpolated($"Searching [cyan]{relativePath}[/]");

            if (settings.Recursive)
            {
                try
                {
                    foreach (DirectoryInfo childDirectory in currentDirectory.EnumerateDirectories())
                        directoryStack.Push(childDirectory);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] {ex.Message}");
                }
            }

            try
            {
                foreach (FileInfo file in currentDirectory.EnumerateFiles())
                {
                    string relativeFilePath = Path.GetRelativePath(inputDirectory.FullName, file.FullName);
                    AnsiConsole.MarkupLineInterpolated($"Checking hash for [cyan]{relativeFilePath}[/]");
                    tasks.Add(Task.Run(() => ProcessFile(file, settings)));
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] {ex.Message}");
            }
        }
    }

    private void ProcessFile(FileInfo file, ListSettings settings)
    {
        Span<byte> buffer = stackalloc byte[64];
        try
        {
            using FileStream stream = file.OpenRead();
            using BufferedStream bufferedStream = new BufferedStream(stream, 1048576 /* 1MB */);
            SHA512.HashData(bufferedStream, buffer);
            string hash = ByteSpanToString(buffer);
            if (settings.Verbose)
                AnsiConsole.WriteLine($"{file.FullName} ->\n    {hash}");

            if (!_fileHashMap.TryGetValue(hash, out List<FileInfo>? cache))
                _fileHashMap[hash] = cache = new List<FileInfo>();

            lock (cache)
                cache.Add(file);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] {ex.Message}");
        }
    }

    private static string ByteSpanToString(ReadOnlySpan<byte> buffer)
    {
        var builder = new StringBuilder();

        foreach (byte b in buffer)
            builder.Append($"{b:X2}");

        return builder.ToString();
    }
}
