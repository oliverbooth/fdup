using System.ComponentModel;
using Spectre.Console.Cli;

namespace FindDuplicates;

internal sealed class ListSettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    [Description("The path to search. Defaults to the current directory.")]
    [DefaultValue(".")]
    public string InputPath { get; set; } = ".";

    [CommandOption("-r|--recursive")]
    [Description("When this flag is set, the directory will be scanned recursively. This may take longer.")]
    [DefaultValue(false)]
    public bool Recursive { get; set; } = false;

    [CommandOption("--verbose")]
    [Description("Enable verbose output.")]
    [DefaultValue(false)]
    public bool Verbose { get; set; } = false;
}
