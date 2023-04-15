using GameServer.Network;
using System;
using System.Diagnostics;
using System.Threading;

namespace GameServer
{
    class Program
    {
        private static bool isRunning = false;

        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(100, 26950);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread has started. Running at {Settings.TICKS_PER_SEC} ticks per second");

            Stopwatch stopWatch = new Stopwatch();
            ThreadManager.SetStopwatch(stopWatch);
            stopWatch.Start();

            double totalTime = 0;
            float deltaTime = 0;
            while (isRunning)
            {
                ThreadManager.Update();

                double newTime = stopWatch.Elapsed.TotalSeconds;
                deltaTime = (float)(newTime - totalTime) * 1000;
                if (deltaTime < Settings.MS_PER_TICK)
                    Thread.Sleep(Settings.MS_PER_TICK - (int)deltaTime);

                newTime = stopWatch.Elapsed.TotalSeconds;
                deltaTime = (float)(newTime - totalTime);
                totalTime = newTime;
            }
        }
    }
}
