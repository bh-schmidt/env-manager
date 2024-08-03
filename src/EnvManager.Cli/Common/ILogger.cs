namespace EnvManager.Cli.Common
{
    public interface ILogger
    {
        ILogger WriteLine();
        ILogger WriteLine(object obj);
        ILogger Write(object obj);
    }
}
