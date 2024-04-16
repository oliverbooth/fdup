using FindDuplicates;
using Spectre.Console.Cli;

var app = new CommandApp<ListCommand>();
await app.RunAsync(args).ConfigureAwait(false);
