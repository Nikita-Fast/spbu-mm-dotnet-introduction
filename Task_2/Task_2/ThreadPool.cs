
namespace Lib
{
    public class ThreadPool : IDisposable
    {
        private bool _disposed = false;
        private CancellationTokenSource _cts = new();
        private CancellationToken _ct;
        private Queue<IBaseTask> _tasksQueue;

        public ThreadPool(int maxThreads)
        {
            _ct = _cts.Token;
            _tasksQueue = new Queue<IBaseTask>();

            for (int i = 0; i < maxThreads; i++)
            {
                Thread t = new Thread(() => { ProcessTasksQueue(_ct); });
                t.Start();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                lock (_tasksQueue)
                {
                    // отказываемся от неначатых задач
                    while (_tasksQueue.Count > 0)
                    {
                        IBaseTask task = _tasksQueue.Dequeue();
                        task.Refuse();
                    }
                    _cts.Cancel();
                    _disposed = true;
                    Monitor.PulseAll(_tasksQueue);
                }
            }
        }

        public void Enqueue(IBaseTask task)
        {
            lock (_tasksQueue)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("Attempted to Enqueue when ThreadPool disposed");
                }
                _tasksQueue.Enqueue(task);
                Monitor.PulseAll(_tasksQueue); 
            }
        }

        private void ProcessTasksQueue(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && !_disposed)
            {
                IBaseTask? task;
                lock (_tasksQueue)
                {
                    // если очередь пуста, то засыпаем
                    if (!_tasksQueue.TryDequeue(out task))
                    {
                        Monitor.Wait(_tasksQueue);
                    }
                }

                if (task != null)
                {
                    task.Run();

                    var continuation = task.Continuation;
                    if (continuation != null)
                    {
                        Enqueue(continuation);
                    }
                }
            }
        }
    }
}
