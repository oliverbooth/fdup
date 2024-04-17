using System.ComponentModel;
using Humanizer;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FindDuplicates;

[Description("Display a list of usable hashing algorithms.")]
internal sealed class AlgListCommand : Command
{
    public override int Execute(CommandContext context)
    {
        AnsiConsole.WriteLine("The default algorithm fdup uses is SHA512.");
        AnsiConsole.MarkupLine("To specify a different one, use the [cyan]-a[/] or [cyan]--algorithm[/] flag, and pass one of the values below:");

        var table = new Table();
        table.AddColumn("Algorithm");
        table.AddColumn("Value");

        foreach (Algorithm algorithm in Enum.GetValues<Algorithm>())
            table.AddRow($"{algorithm.Humanize()}", $"{algorithm.ToString().ToLower()}");

        AnsiConsole.Write(table);
        return 0;
    }
}
