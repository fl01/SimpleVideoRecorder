using System;
using System.Threading.Tasks;

namespace SimpleVideoRecorder.Core.ScreenCapture
{
    public sealed class SequentialInvoker
    {
        private readonly object sync = new object();
        private Task lastTask;

        public SequentialInvoker()
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);

            lastTask = tcs.Task;
        }

        public void Invoke(Action action)
        {
            Task prevTask;
            var tcs = new TaskCompletionSource<bool>();

            lock (sync)
            {
                prevTask = lastTask;
                lastTask = tcs.Task;
            }

            try
            {
                prevTask.Wait();
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    throw;
                }
                tcs.SetResult(true);
            }
            finally
            {
                tcs.TrySetResult(false);
            }
        }

        public Task InvokeAsync(Action action)
        {
            Task result;
            lock (sync)
            {
                result = lastTask.ContinueWith(x => action.Invoke());
                lastTask = result;
            }

            return result;
        }

        public void WaitForPendingInvocations()
        {
            Task taskToWait;
            lock (sync)
            {
                taskToWait = lastTask;
            }
            taskToWait.Wait();
        }
    }
}
