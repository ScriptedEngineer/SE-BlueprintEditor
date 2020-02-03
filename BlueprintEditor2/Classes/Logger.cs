using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlueprintEditor2
{
    public static class Logger
    {
        static StringBuilder Log = new StringBuilder();
        static int LogLenght = 0; 
        public static void HandleUnhandledException()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashHappens);
        }
        public static void Add(string log)
        {
            Log.Append($"[{DateTime.UtcNow}] {(string.IsNullOrEmpty(Thread.CurrentThread.Name)? "Thread"+Thread.CurrentThread.ManagedThreadId: Thread.CurrentThread.Name)}: ").Append(log).Append('\n');
#if DEBUG
            //Console.WriteLine(log);
#endif
            LogLenght++;
            if (LogLenght > 1000)
            {
                Log.Remove(0, Convert.ToString(Log).Split('\n').FirstOrDefault().Length + 1);
            }
        }
        private static void CrashHappens(object sende, UnhandledExceptionEventArgs UhEx)
        {
            if (!UhEx.IsTerminating)
            {
#if DEBUG
                Console.WriteLine((UhEx.ExceptionObject as Exception).StackTrace);
#endif
                return;
            }
            string Crash = Log.ToString()+"\r\nError:\r\n";
            Exception e = (Exception)UhEx.ExceptionObject;
            Crash += "Error message: " + e.Message;
            Crash += "\r\nSource: " + e.Source;
            Crash += "\r\nTarget site name: " + e.TargetSite.Name;
            Crash += "\r\nTarget site module name: " + e.TargetSite.Module.Name;
            Crash += "\r\nStack trace:\r\n" + e.StackTrace;
            File.WriteAllText("LastCrash.txt", Crash);
            Process.Start(MyExtensions.AppFile,"Crash");
        }
    }
}
