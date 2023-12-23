
namespace Lib
{
    public interface IBaseTask
    {
        bool IsCompleted { get; }
        bool IsRefused { get; }
        IBaseTask? Continuation { get; }
        void Run();
        void Refuse();
    }

    public interface IMyTask<TResult> : IBaseTask
    {
        TResult Result { get; }
        IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation);
    }
}
