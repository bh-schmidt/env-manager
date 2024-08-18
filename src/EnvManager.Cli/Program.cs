using EnvManager.Cli.Common.Concurrency;
using EnvManager.Cli.Common.IO.Internal;
using EnvManager.Cli.Common.Loggers;
using EnvManager.Cli.LuaContexts;
using EnvManager.Common;
using ImprovedConsole;
using ImprovedConsole.CommandRunners;
using ImprovedConsole.CommandRunners.Arguments;
using ImprovedConsole.CommandRunners.Commands;
using MoonSharp.Interpreter;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

var sw = new Stopwatch();
sw.Start();
var filter = new FileMatcherFilters
{
    SourceDir = "c:/_w",
    IgnorePatterns = [
        "**/bin/**/*",
        "**/obj/**/*",
        "**/node_modules/**/*",
        "**/.next/**/*",
        "**/.vs/**/*",
        "**/.env.local",
        "**/.env.*.local"
    ],
};
var matcher = new FileMatcher(filter)
{
    MaxConcurrency = 10
};

matcher.Match();
sw.Stop();

var res1 = matcher.Matches.Where(e => !e.IsDirectory && !e.Excluded).ToArray();
var res2 = res1.DistinctBy(e => e.Path).ToArray();

//39494
//9118
//4559
//0.91

//w 0.31

Console.WriteLine(sw.Elapsed);


var bag = new ConcurrentBag<int>();

var box = new ConcurrentCollection<int>()
{
    MaxConcurrency = 10,
};

for (int i = 0; i < 100; i++)
    box.Add(i);

box.Foreach(i =>
{
    var newValue = i + 100;

    if (newValue < 100000)
        box.Add(newValue);
});

var x = box.ToArray();
//var x = bag.ToArray();
var count = x.Length;






SetupLogger.Setup();

UserData.RegisterAssembly(Assembly.GetCallingAssembly());

var builder = new CommandBuilder(new CommandBuilderOptions()
{
    CliName = "envm"
});

builder.AddCommand(run =>
{
    run
        .WithName("run")
        .WithDescription("Runs the pipeline configured in the lua file")
        .AddParameter("file", "The lua file containing the pipeline")
        .AddFlag("-s", "Runs the pipeline without using stages")
        .AddFlag("-v", "Enables the verbose mode for logs")
        .AddFlag("--test", "Setup the test environment")
        .SetHandler(Run);
});

var runner = new SafeCommandRunner(builder);
runner.Run(args);

static void Run(CommandArguments arguments)
{
    var template = arguments.Parameters["file"];
    var runSteps = arguments.Options.Contains("-s");
    var verbose = arguments.Options.Contains("-v");

    if (template is null)
    {
        ConsoleWriter.WriteLine("The template can't be null");
        return;
    }

    try
    {
        var path = template.Value
            .FixUserPath()
            .FixWindowsDisk();

        var pipeline = runSteps ?
            LuaContext.BuildWithSteps(path, arguments) :
            LuaContext.BuildWithStages(path, arguments);

        pipeline.Run(arguments);
    }
    catch (ScriptRuntimeException ex)
    {
        var message = ex.DecoratedMessage ?? ex.Message;
        var treatedMessage = verbose ?
            message + '\n' + ex.StackTrace :
            message;

        ConsoleWriter.WriteLine("An error ocurred.");
        ConsoleWriter.WriteLine(treatedMessage);
    }
    catch (Exception ex)
    {
        var message = verbose ?
            ex.Message + '\n' + ex.StackTrace :
            ex.Message;

        ConsoleWriter.WriteLine("An error ocurred.");
        ConsoleWriter.WriteLine(message);
    }
}
