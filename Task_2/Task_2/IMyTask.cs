﻿
namespace Lib
{
    public interface IBaseTask
    {
        bool IsCompleted { get; }
        bool IsFaulted { get; }
        bool IsRefused { get; }
        bool HasContinuation { get; }
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
