
namespace Lib
{
    public class MyTask<TResult> : IMyTask<TResult>
    {
        private Func<TResult> _task;
        private TResult? _result;
        private Exception? _exception;
        private readonly object _lock = new object();

        public bool IsCompleted { get; private set; }
        public bool IsRefused { get; private set; }
        public IBaseTask? Continuation { get; private set; }

        public void Refuse()
        {
            lock (_lock)
            {
                IsCompleted = false;
                IsRefused = true;
                Monitor.PulseAll(_lock);
            }
        }

        public MyTask(Func<TResult> task)
        {
            _task = task;
        }
        public void Run()
        {
            try
            {
                var result = _task.Invoke();
                _result = result;
            }
            catch (Exception e)
            {
                _exception = e;
            }
            finally
            {
                lock (_lock)
                {
                    IsCompleted = true;
                    Monitor.PulseAll(_lock);
                }
            }
        }

        public TResult Result
        {
            get
            {
                lock ( _lock )
                {
                    if (!IsCompleted && !IsRefused) {
                        Monitor.Wait(_lock);
                    }
                }

                if (_exception != null)
                {
                    throw new AggregateException(_exception);
                }

                return _result;
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
        {
            var continuedTask = new MyTask<TNewResult>(() => continuation.Invoke(Result));
            Continuation = continuedTask;
            return continuedTask;
        }
    }
}
