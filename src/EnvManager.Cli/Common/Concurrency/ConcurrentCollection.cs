using System.Collections;
using System.Collections.Concurrent;

namespace EnvManager.Cli.Common.Concurrency
{
    public class ConcurrentCollection<T>(IEnumerable<T> items) : IEnumerable<T>
    {
        private readonly object _lock = new();
        private readonly ConcurrentBag<T> _items = new(items);

        public ConcurrentCollection() : this([]) { }

        public int MaxConcurrency { get; set; } = 1;
        public Action<T> OnAdd { get; set; }

        public void Add(T item)
        {
            _items.Add(item);
            OnAdd?.Invoke(item);
        }

        public void Foreach(Action<T> action)
        {
            MaxConcurrency = Math.Max(1, MaxConcurrency);
            List<Task> tasks = [];
            ConcurrentQueue<T> queue;
            lock (_lock)
            {
                queue = new(_items);
                SetupForEach(action, queue, tasks);
            }

        Start:
            var lastCount = tasks.Count;
            Task.WhenAll(tasks).Wait();

            if (lastCount != tasks.Count)
                goto Start;
        }

        private void SetupForEach(Action<T> action, ConcurrentQueue<T> queue, List<Task> tasks)
        {
            int currentRunning = 0;
            var concurrencyNumber = Math.Min(queue.Count, MaxConcurrency);

            for (int i = 0; i < concurrencyNumber; i++)
            {
                currentRunning++;
                var task = Task.Factory.StartNew(() => RunForEach(action, queue, ref currentRunning));
                tasks.Add(task);
            }

            OnAdd += item =>
            {
                queue.Enqueue(item);
                lock (_lock)
                {
                    if (currentRunning < MaxConcurrency)
                    {
                        currentRunning++;
                        var task = Task.Factory.StartNew(() => RunForEach(action, queue, ref currentRunning));
                        tasks.Add(task);
                    }
                }
            };
        }

        private void RunForEach(Action<T> action, ConcurrentQueue<T> queue, ref int currentRunning)
        {
        Start:
            while (true)
            {
                T item;

                if (!queue.TryDequeue(out item))
                    break;

                action(item);
            }

            lock (_lock)
            {
                if (queue.Count > 0)
                    goto Start;

                currentRunning--;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
