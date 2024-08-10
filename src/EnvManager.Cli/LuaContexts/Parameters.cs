using ImprovedConsole.CommandRunners.Arguments;
using MoonSharp.Interpreter;

namespace EnvManager.Cli.LuaContexts
{
    [MoonSharpUserData]
    public class Parameters(CommandArguments arguments)
    {
        public bool IsTest => arguments.Options.Contains("--test");
    }
}
