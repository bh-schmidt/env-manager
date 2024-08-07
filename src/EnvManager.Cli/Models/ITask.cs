﻿using EnvManager.Cli.Common;
using ImprovedConsole.CommandRunners.Arguments;

namespace EnvManager.Cli.Models
{
    public interface ITask
    {
        string Code { get; }
        Guid Id { get; set; }
        string Name { get; set; }
        void Run(CommandArguments arguments, PipeLogger logger);
    }
}
