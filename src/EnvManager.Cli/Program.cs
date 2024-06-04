using EnvManager.Models;
using ImprovedConsole.CommandRunners;
using ImprovedConsole.CommandRunners.Arguments;
using ImprovedConsole.CommandRunners.Commands;
using Newtonsoft.Json;

var builder = new InlineCommandBuilder();
builder.AddCommand("run", "Executes the settings configured in the json file", run =>
{
    run
        .AddParameter("json-template", "Json file containing all the settings")
        .AddFlag("-v", "Enables the verbose mode for logs")
        .AddFlag("-p", "Preview the execution of the settings configured in the json file")
        .SetHandler(Run);
});

var runner = new SafeCommandRunner(builder);
runner.Run(args);

static void Run(CommandArguments arguments)
{
    var preview = arguments.Options["-p"] is not null;

    var template = arguments.Parameters["json-template"];
    if (template is null)
    {
        Console.WriteLine("The template can't be null");
        return;
    }

    var fileContent = File.ReadAllText(template.Value);
    var settings = JsonConvert.DeserializeObject<Settings>(fileContent);

    settings.TransformTasks();

    if (preview)
    {
        Console.WriteLine("Running in PREVIEW MODE");
        Console.Write("\n------------------------------------------------------------------------------------------------------------------------\n\n");
    }
    else
    {
        Console.Write("------------------------------------------------------------------------------------------------------------------------\n\n");
    }

    var id = 1;
    foreach (var task in settings.InternalTasks)
    {
        Console.WriteLine($"Task: {id}\n");
        task.Run(arguments);
        Console.Write("\n------------------------------------------------------------------------------------------------------------------------\n\n");
        id++;
    }
}
