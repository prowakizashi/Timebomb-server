using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameServer
{
    class ThreadManager
    {
        private static readonly Queue<Action> taskQueue = new Queue<Action>();
        private static readonly List<DelayedTask> delayedTasks = new List<DelayedTask>();

        private static Stopwatch stopwatch;

        public static void SetStopwatch(Stopwatch _stopwatch)
        {
            stopwatch = _stopwatch;
        }

        public static void AddMainThreadTask(Action _action)
        {
            if (_action == null)
                return;

            lock (taskQueue)
            {
                taskQueue.Enqueue(_action);
            }
        }

        public static void DelayTask(int _delay, Action _task)
        {
            DelayedTask dTask = new DelayedTask(stopwatch.Elapsed.TotalMilliseconds + _delay, _task);
            if (delayedTasks.Count == 0)
            {
                delayedTasks.Add(dTask);
                return;
            }

            for (int i = 0; i < delayedTasks.Count; ++i)
            {
                if (dTask.ExecutionTime < delayedTasks[i].ExecutionTime)
                {
                    delayedTasks.Insert(i, dTask);
                    return;
                }
            }

            delayedTasks.Add(dTask);
        }

        public static void Update()
        {
            UpdateQueue();
            UpdateDelayedTask();
        }

        private static void UpdateQueue()
        {
            while (!isQueueEmpty())
            {
                Action _task = null;
                lock (taskQueue)
                {
                    _task = taskQueue.Dequeue();
                }

                _task?.Invoke();
            }
        }

        private static void UpdateDelayedTask()
        {
            while (delayedTasks.Count > 0 && stopwatch.Elapsed.TotalMilliseconds >= delayedTasks[0].ExecutionTime)
            {
                var dTask = delayedTasks[0];
                delayedTasks.RemoveAt(0);
                dTask.Task?.Invoke();
            }
        }

        private static bool isQueueEmpty()
        {
            bool _value = true;
            lock (taskQueue)
            {
                _value = taskQueue.Count == 0;
            }
            return _value;
        }

        private class DelayedTask
        {
            public double ExecutionTime { get; private set; }
            public Action Task { get; private set; }

            public DelayedTask(double _executionTime, Action _task)
            {
                ExecutionTime = _executionTime;
                Task = _task;
            }
        }
    }
}
