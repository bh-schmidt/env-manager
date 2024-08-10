using EnvManager.Cli.Models;
using Mapster;
using MoonSharp.Interpreter;
using YamlDotNet.Core.Tokens;

namespace EnvManager.Cli.LuaContexts
{
    public class LuaContext
    {
        public static Pipeline BuildWithStages(string file, ImprovedConsole.CommandRunners.Arguments.CommandArguments arguments)
        {
            var stages = GetStages(file, arguments);
            return new Pipeline(stages);
        }

        public static Pipeline BuildWithSteps(string file, ImprovedConsole.CommandRunners.Arguments.CommandArguments arguments)
        {
            var stage = new Stage
            {
                Id = $"STAGE_001",
                Name = "Default Stage"
            };

            var script = new Script();
            script.Options.ScriptLoader = new CustomLoader(file);

            var provider = new StepProvider();
            provider.SetCurrentStage(stage);

            script.Globals["Parameters"] = new Parameters(arguments);
            script.Globals["Steps"] = provider.Steps;
            script.Globals["Stage"] = (DynValue value) =>
            {
                throw new Exception("Can't create an stage inside another stage.");
            };

            var fileName = Path.GetFileName(file);
            script.DoFile(fileName);

            return new Pipeline([stage]);
        }

        private static List<Stage> GetStages(string file, ImprovedConsole.CommandRunners.Arguments.CommandArguments arguments)
        {
            bool stagesMapped = false;
            var stages = new List<Stage>();
            var currentDir = Path.GetDirectoryName(file);

            var script = new Script();
            script.Options.ScriptLoader = new CustomLoader(file);

            script.Globals["Parameters"] = new Parameters(arguments);
            script.Globals["Stage"] = (DynValue value) =>
            {
                var dto = DynValueParser.Parse<Stage>(value);
                if (stagesMapped)
                    throw new Exception("Can't create an stage inside another stage.");

                var number = stages.Count + 1;
                dto.Id = $"STAGE_{number:000}";
                stages.Add(dto);
                return dto.Id;
            };

            var provider = new StepProvider();
            script.Globals["Steps"] = provider.Steps;

            var fileName = Path.GetFileName(file);
            script.DoFile(fileName);

            stagesMapped = true;

            foreach (var stage in stages)
            {
                provider.SetCurrentStage(stage);
                stage.Body.Call();
            }

            return stages;
        }
    }
}
