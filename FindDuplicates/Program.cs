using FindDuplicates;
using Spectre.Console.Cli;

var app = new CommandApp<ListCommand>();
app.Configure(cfg => cfg.AddCommand<AlgListCommand>("alglist"));
await app.RunAsync(args).ConfigureAwait(false);
