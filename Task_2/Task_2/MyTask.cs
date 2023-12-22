
namespace Lib
{
    public class MyTask<TResult> : IMyTask<TResult>
    {
        private Func<TResult> _task;
        private TResult _result;
        private Exception? _exception;
        private ManualResetEvent _waitHandler = new ManualResetEvent(false);

        public bool IsCompleted { get; private set; }
        public bool IsFaulted { get; private set; }
        public bool IsRefused { get; private set; }
        public IBaseTask? Continuation { get; private set; }
        public bool HasContinuation { get { return Continuation != null; } }

        public void Refuse()
        {
            IsCompleted = false;
            IsFaulted = false;
            IsRefused = true;
            _waitHandler.Set();
        }

        public MyTask(Func<TResult> task)
        {
            _task = task;
        }
        public void Run()
        {
            try
            {
                _result = _task.Invoke();
            }
            catch (Exception e)
            {
                _exception = e;
                IsFaulted = true;
            }
            finally
            {
                IsCompleted = true;
                _waitHandler.Set();
            }
        }

        public TResult Result
        {
            get
            {
                while (!IsCompleted && !IsFaulted && !IsRefused) { _waitHandler.WaitOne(); }

                if (IsFaulted)
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
