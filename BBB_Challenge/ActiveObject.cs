using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadUtils
{
    public abstract class ActiveObject : Worker, IDisposable
    {
        public Task MyTask { get; private set; }
        public CancellationTokenSource source = new CancellationTokenSource();

        public ActiveObject() 
        {
            var token = source.Token;
            MyTask = Task.Factory.StartNew(DoWork, token);
        }

        public void Dispose()
        {
            if (MyTask == null)
                return;

            _shouldStop = true;
            source.Cancel();
            MyTask.Wait();
            MyTask = null;
        }

    }
}


