using Serilog.Context;
using Serilog.Core.Enrichers;

namespace EnvManager.Cli.Common.Loggers
{
    public class LogCtx
    {
        public const string NoLFName = "_LF_";
        public const string DateName = "_Date_";
        public const string StepFileOnlyName = "_StepOnly_";
        public const string PipelineOnlyName = "_PipelineOnly_";
        public const string PipelineLogPathName = "_PipelineLogPath_";
        public const string StepLogPathName = "_StepLogPath_";
        public const string LogPaddingName = "_LogPadding_";

        public readonly static PropertyEnricher NoLF = new(NoLFName, null);
        public readonly static PropertyEnricher Date = new(DateName, null);
        public readonly static PropertyEnricher StepFileOnly = new(StepFileOnlyName, null);
        public readonly static PropertyEnricher PipelineOnly = new(PipelineOnlyName, null);

        internal static IDisposable NoLf()
        {
            return LogContext.Push(NoLF);
        }

        internal static IDisposable SetStepFileOnly()
        {
            return LogContext.Push(StepFileOnly);
        }

        internal static IDisposable AddPadding(int padding)
        {
            return LoggerPadding.AddPadding(padding);
        }

        internal static IDisposable SetStepLogPath(string path)
        {
            return LogContext.PushProperty(StepLogPathName, path);
        }

        internal static IDisposable SetPipelineLogPath(string path)
        {
            return LogContext.PushProperty(PipelineLogPathName, path);
        }

        internal static string GetCurrentDate() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }
}
