using EnvManager.Cli.Common.Loggers;
using Serilog;
using Serilog.Context;

namespace EnvManager.Cli
{
    public class Test
    {
        public static void Run()
        {
            using (LoggerPadding.AddPadding(4))
            {
                Log.ForContext("log_padding", "    ")
                    .ForContext(LogCtx.NoLF)
                    .Information("asdfasdfasdf");

                Log.ForContext("log_padding", "    ")
                    .Information("\rasaaaaaaa");

                using (LoggerPadding.AddPadding(4))
                {
                    Log.Logger.Information("aaaa");
                }

                Log.Logger.Information("aaaa");
            }

            using(LogContext.PushProperty("var", "teste_asd"))
            using(LogContext.PushProperty("file-only", null))
            {
                Log.Logger.Information("aaaab");
            }
        }
    }
}
