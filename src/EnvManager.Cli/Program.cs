using EnvManager.Cli.Common.Loggers;
using EnvManager.Cli.Common.Windows;
using EnvManager.Cli.LuaContexts;
using EnvManager.Common;
using ImprovedConsole;
using ImprovedConsole.CommandRunners;
using ImprovedConsole.CommandRunners.Arguments;
using ImprovedConsole.CommandRunners.Commands;
using MoonSharp.Interpreter;
using System.Reflection;

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
