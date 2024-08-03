using EnvManager.Cli.Common;
using ImprovedConsole;

namespace EnvManager.Cli
{
    public class Test
    {
        public async Task A()
        {
            await Task.CompletedTask;
            //CustomLogger._padding = 2;
            using var _ = CustomLogger.AddPadding(2);
            ConsoleWriter.WriteLine("a");
            B();
            ConsoleWriter.WriteLine("a");
        }

        public void B()
        {
            ConsoleWriter.WriteLine("b");
            using var x = CustomLogger.AddPadding(2);
            ConsoleWriter.WriteLine("b");
            C().Wait();
            ConsoleWriter.WriteLine("b");
        }

        public async Task C()
        {
            using var x = CustomLogger.AddPadding(2);
            ConsoleWriter.WriteLine("c");
            await Task.Delay(1000);
            ConsoleWriter.WriteLine("c");
        } 
    }
}
