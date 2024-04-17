using System.Collections.Concurrent;
using System.Text;
using Humanizer;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FindDuplicates;

internal sealed class ListCommand : AsyncCommand<ListSettings>
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<FileInfo>> _fileHashMap = new();

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
        AnsiConsole.MarkupLine($"Using hash algorithm [cyan]{settings.Algorithm.Humanize()}[/]");

        await AnsiConsole.Status()
            .StartAsync("Waiting to hash files...", DoHashWaitAsync)
            .ConfigureAwait(false);

        AnsiConsole.WriteLine();

        int duplicates = 0;
        foreach ((string hash, ConcurrentBag<FileInfo> files) in _fileHashMap)
        {
            int fileCount = files.Count;

            if (fileCount <= 1)
                continue;

            duplicates += fileCount;
            AnsiConsole.MarkupLineInterpolated($"Found [cyan]{fileCount}[/] identical files");
            AnsiConsole.MarkupLineInterpolated($"{settings.Algorithm.Humanize()} [green]{hash}[/]:");

            foreach (FileInfo file in files)
                AnsiConsole.MarkupLineInterpolated($"- {file.FullName}");

            AnsiConsole.WriteLine();
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

            AddChildDirectories(settings, currentDirectory, directoryStack);

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
        Span<byte> buffer = stackalloc byte[settings.Algorithm.GetByteCount()];
        try
        {
            using FileStream stream = file.OpenRead();
            using BufferedStream bufferedStream = new BufferedStream(stream, 1048576 /* 1MB */);
            settings.Algorithm.HashData(bufferedStream, buffer);
            string hash = ByteSpanToString(buffer);
            if (settings.Verbose)
                AnsiConsole.WriteLine($"{file.FullName} ->\n    {hash}");

            ConcurrentBag<FileInfo> cache = _fileHashMap.GetOrAdd(hash, _ => []);
            cache.Add(file);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] {ex.Message}");
        }
    }

    private static void AddChildDirectories(ListSettings settings, DirectoryInfo directory, Stack<DirectoryInfo> stack)
    {
        if (!settings.Recursive)
            return;

        try
        {
            foreach (DirectoryInfo childDirectory in directory.EnumerateDirectories())
                stack.Push(childDirectory);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] {ex.Message}");
        }
    }

    private static string ByteSpanToString(ReadOnlySpan<byte> buffer)
    {
        var builder = new StringBuilder(buffer.Length * 2);

        foreach (byte b in buffer)
            builder.Append($"{b:X2}");

        return builder.ToString();
    }
}
