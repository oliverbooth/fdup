using System.ComponentModel;
using Spectre.Console.Cli;

namespace FindDuplicates;

internal sealed class ListSettings : CommandSettings
{
    [CommandArgument(0, "<path>")]
    [Description("The path to search.")]
    public string InputPath { get; set; } = string.Empty;

    [CommandOption("-r|--recursive")]
    [Description("When this flag is set, the directory will be scanned recursively. This may take longer.")]
    [DefaultValue(false)]
    public bool Recursive { get; set; } = false;
}
