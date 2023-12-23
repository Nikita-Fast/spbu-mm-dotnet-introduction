using Lib;


namespace Tests
{
    public class TestThreadPool
    {
        [Fact]
        public void TestSimpleTask()
        {
            Lib.ThreadPool threadPool = new Lib.ThreadPool(8);
            MyTask<int> task = new MyTask<int>(() => 100 * 1000 + 500);
            threadPool.Enqueue(task);
            Assert.Equal(100500, task.Result);
            threadPool.Dispose();
        }

        [Fact]
        public void TestManyTasks()
        {
            Lib.ThreadPool threadPool = new Lib.ThreadPool(8);
            List<Func<int>> funcs = new();
            for (int i = 0; i < 100; i++)
            {
                funcs.Add(() => i + i);
            }

            var tasks = new List<MyTask<int>>();
            for (int i = 0; i < 100; i++)
            {
                var task = new MyTask<int>(funcs[i]);
                tasks.Add(task);
                threadPool.Enqueue(task);
            }

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(funcs[i].Invoke(), tasks[i].Result);
            }
            threadPool.Dispose();
        }

        [Fact]
        public void TestContinuation()
        {
            Lib.ThreadPool threadPool = new Lib.ThreadPool(1);
            MyTask<int> task = new MyTask<int>(() => 100 * 1000);
            MyTask<int> continuedTask = (MyTask<int>)task.ContinueWith<int>(x => x + 500);
            threadPool.Enqueue(task);
            Assert.Equal(100500, continuedTask.Result);
            threadPool.Dispose();
        }

        [Fact]
        public void TestContinuation2()
        {
            Lib.ThreadPool threadPool = new Lib.ThreadPool(8);
            string fullName = "Nikita Fast";
            MyTask<int> task = new MyTask<int>(() => 6);
            MyTask<string> continuedTask = (MyTask<string>)task.ContinueWith<string>(len => fullName.Substring(0, len));
            threadPool.Enqueue(task);
            Assert.Equal("Nikita", continuedTask.Result);
            threadPool.Dispose();
        }

        [Fact]
        public void TestContinuations()
        {
            Lib.ThreadPool threadPool = new Lib.ThreadPool(8);
            MyTask<int> task1 = new MyTask<int>(() => 100);
            var task2 = task1.ContinueWith<int>(x => x * 1000);
            var task3 = task2.ContinueWith<int>(x => x + 500);
            var task4 = task3.ContinueWith<int>(x => x - 1);

            threadPool.Enqueue(task1);
            Assert.Equal(100499, task4.Result);
            threadPool.Dispose();
        }

        [Fact]
        public void TestException()
        {
            Lib.ThreadPool threadPool = new Lib.ThreadPool(8);
            int zero = 0;
            MyTask<int> task = new MyTask<int>(() => 100500 / zero);
            threadPool.Enqueue(task);

            try
            {
                var res = task.Result;
            }
            catch (AggregateException ex)
            {
                Assert.Equal("Attempted to divide by zero.", ex.InnerException.Message);
            }
            threadPool.Dispose();
        }

        [Fact]
        public void TestThreadNumber()
        {
            int expected = 8;
            HashSet<int> actualThreads = new();
            Lib.ThreadPool threadPool = new Lib.ThreadPool(expected);
            int n = 16;
            List<IMyTask<int>> functions = new List<IMyTask<int>>();

            for (int i = 0; i < n; i++)
            {
                IMyTask<int> task = new MyTask<int>(() => Thread.CurrentThread.ManagedThreadId);
                functions.Add(task);
                threadPool.Enqueue(task);
            }

            for (int i = 0; i < n; i++)
            {
                actualThreads.Add(functions[i].Result);
            }

            Assert.Equal(expected, actualThreads.Count);

            threadPool.Dispose();
        }

        [Fact]
        public void EnqueueTaskWhenThreadPoolDisposed()
        {
            var threadPool = new Lib.ThreadPool(8);

            var task = new MyTask<int>(() => 100500);

            threadPool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => threadPool.Enqueue(task));
        }

        [Fact]
        public void TestManyContinuations()
        {
            var threadPool = new Lib.ThreadPool(4);
            var task = new MyTask<int>(() => 0);
            var last = task;
            int[] vals = { 1, 2, 3, 4 , 5, 6, 7, 8, 9, 10};
            for (int i = 0; i < vals.Length; i++)
            {
                var y = vals[i];
                var cont = last.ContinueWith<int>(x => x + y);
                last = (MyTask<int>)cont;
            }
            threadPool.Enqueue(task);
            
            Thread.Sleep(1000);
            Assert.Equal(vals.Sum(), last.Result);
            threadPool.Dispose();
        }
    }
}
