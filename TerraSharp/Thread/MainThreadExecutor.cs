using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraSharp.Thread
{
    public class MainThreadExecutor
    {
        private static readonly ConcurrentQueue<Action> taskQueue = new ConcurrentQueue<Action>();

        public static void QueueTask(Action task)
        {
            taskQueue.Enqueue(task);
        }

        public static void UpdateTasks()
        {
            while (taskQueue.TryDequeue(out Action task))
            {
                task?.Invoke();
            }
        }
    }
}
