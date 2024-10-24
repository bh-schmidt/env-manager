﻿using EnvManager.Common;
using ImprovedConsole.CommandRunners.Arguments;

namespace EnvManager.Cli.Models
{
    public class StepContext(StageContext context, Step step)
    {
        public CommandArguments Arguments { get; } = context.Arguments;
        public Pipeline Pipeline { get; } = context.Pipeline;
        public Stage Stage { get; } = context.Stage;
        public Step Step { get; } = step;
        public string StepDirectory { get; }
        public string LogFilePath { get; } = context.LogDirectory.CombinePathWith(step.Id.ToString() + ".txt");
    }
}
