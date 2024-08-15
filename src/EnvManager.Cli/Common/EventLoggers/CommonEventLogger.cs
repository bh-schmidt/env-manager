using EnvManager.Cli.Common.Loggers;
using Serilog;

namespace EnvManager.Cli.Common.EventLoggers
{
    public class CommonEventLogger
    {
        private readonly Queue<StreamReader> _streams = [];
        private bool finished;
        private TaskCompletionSource completionSource;

        public CommonEventLogger()
        {
        }

        public CommonEventLogger(StreamReader streamReader)
        {
            _streams.Enqueue(streamReader);

            ArgumentNullException.ThrowIfNull(streamReader);
            Task.Factory.StartNew(Run);
        }

        public void Queue(StreamReader streamReader)
        {
            lock (_streams)
                _streams.Enqueue(streamReader);
        }

        public void Start()
        {
            completionSource = new();
            finished = false;
            Task.Factory.StartNew(Run);
        }

        public void Finish()
        {
            finished = true;
            completionSource.Task.Wait();
        }

        private async Task Run()
        {
            while (true)
            {
                StreamReader streamReader;
                lock (_streams)
                {
                    if (_streams.Count == 0)
                    {
                        completionSource.SetResult();
                        return;
                    }

                    streamReader = _streams.Dequeue();
                }

                await Run(streamReader);
            }
        }

        private async Task Run(StreamReader streamReader)
        {
            int bufferSize = 8 * 1024;
            var buffer = new char[bufferSize];

            using (LogCtx.SetStepFileOnly())
            using (LogCtx.NoLf())
            {
                while (true)
                {
                    var chunkLength = await streamReader.ReadAsync(buffer, 0, buffer.Length);
                    if (chunkLength == 0 && finished)
                    {
                        break;
                    }

                    Log.Information(new string(buffer, 0, chunkLength));
                }
            }
        }
    }
}
