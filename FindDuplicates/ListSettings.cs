using System.ComponentModel;
using Spectre.Console.Cli;

namespace FindDuplicates;

internal sealed class ListSettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    [Description("The path to search. Defaults to the current directory.")]
    [DefaultValue(".")]
    public string InputPath { get; set; } = ".";

    [CommandOption("-a|--algorithm <algorithm>")]
    [Description("The hash algorithm used for comparison. Defaults to SHA512. For a list of all available algorithms, run fdup alglist")]
    [DefaultValue(Algorithm.Sha512)]
    public Algorithm Algorithm { get; set; } = Algorithm.Sha512;

    [CommandOption("-r|--recursive")]
    [Description("Scans the directory recursively. This may increase run time and is not advised to use when at high order directories such as C: or /")]
    [DefaultValue(false)]
    public bool Recursive { get; set; } = false;

    [CommandOption("--verbose")]
    [Description("Enable verbose output.")]
    [DefaultValue(false)]
    public bool Verbose { get; set; } = false;
}
