using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assignment1
{
    /// <summary>
    /// ThreadHelper is a simple helper class which executes a no-arg function (task) on a seperate
    /// worker thread enabling running multiple tasks concurrently, and a wait function for the main calling thread
    /// to rejoin when all tasks are complete
    /// </summary>
    public static class ThreadHelper
    {
        private static ManualResetEvent threadsWaitEvent = new ManualResetEvent(false);
        private static long threadTaskCounter = 0;
        private static readonly int maxThreads = 1000;

        public delegate void SomeTask();

        public static void AddThread(SomeTask task)
        {
            if (Interlocked.Read(ref threadTaskCounter) == maxThreads)
                WaitThreads();
            Interlocked.Increment(ref threadTaskCounter);
            ThreadPool.QueueUserWorkItem((x) =>
            {
                task();
                Thread.Sleep(10);
                if (Interlocked.Decrement(ref threadTaskCounter) == 0)
                    threadsWaitEvent.Set();
            });
        }

        public static void WaitThreads()
        {
            threadsWaitEvent.WaitOne();
            threadsWaitEvent.Reset();
        }
    }
}
