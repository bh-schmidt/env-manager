using Serilog;
using Serilog.Filters;

namespace EnvManager.Cli.Common.Loggers
{
    public class SetupLogger
    {
        public static void Setup()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Logger(logger =>
                {
                    logger
                        .Filter.ByExcluding(Matching.WithProperty(LogCtx.StepFileOnlyName))
                        .WriteTo.Console(new LogFormatter())
                        .WriteTo.Logger(logger =>
                        {
                            logger
                                .Filter.ByIncludingOnly(Matching.WithProperty(LogCtx.PipelineLogPathName))
                                .WriteTo.Sink(new FileSink($"{{{LogCtx.PipelineLogPathName}}}", new LogFormatter()));
                        });
                })
                .WriteTo.Logger(logger =>
                {
                    logger
                        .Filter.ByExcluding(Matching.WithProperty(LogCtx.PipelineOnlyName))
                        .Filter.ByIncludingOnly(Matching.WithProperty(LogCtx.StepLogPathName))
                        .WriteTo.Sink(new FileSink($"{{{LogCtx.StepLogPathName}}}", new LogFormatter(new() { UsePadding = false })));
                })
                .CreateLogger();
        }
    }
}
