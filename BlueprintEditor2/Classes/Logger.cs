using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlueprintEditor2
{
    public static class Logger
    {
        static readonly StringBuilder Log = new StringBuilder();
        static int LogLenght = 0; 
        public static void HandleUnhandledException()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashHappens);
        }
        public static void Add(string log)
        {
            StringBuilder Lgo = new StringBuilder();
            Lgo.Append("[").Append(DateTime.UtcNow).Append("] ");
            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                Lgo.Append("Thread").Append(Thread.CurrentThread.ManagedThreadId);
            else
                Lgo.Append(Thread.CurrentThread.Name);
            Lgo.Append(": ").Append(log).Append('\n');
            Log.Append(Lgo);
            //string logf = $"[{DateTime.UtcNow}] {(string.IsNullOrEmpty(Thread.CurrentThread.Name) ? "Thread" + Thread.CurrentThread.ManagedThreadId : Thread.CurrentThread.Name)}: {log}";
#if DEBUG
            new Task(() =>
            {
                if (ConsoleManager.HasConsole)
                    Console.Write(Lgo.ToString());
            }).Start();
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
    [SuppressUnmanagedCodeSecurity]
    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(Kernel32_DllName)]
        private static extern int GetConsoleOutputCP();

        public static bool HasConsole
        {
            get { return GetConsoleWindow() != IntPtr.Zero; }
        }

        /// <summary>
        /// Creates a new console instance if the process is not attached to a console already.
        /// </summary>
        public static void Show()
        {
            //#if DEBUG
            if (!HasConsole)
            {
                AllocConsole();
                InvalidateOutAndError();
            }
            //#endif
        }

        /// <summary>
        /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.
        /// </summary>
        public static void Hide()
        {
            //#if DEBUG
            if (HasConsole)
            {
                SetOutAndErrorNull();
                FreeConsole();
            }
            //#endif
        }

        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        static void InvalidateOutAndError()
        {
            Type type = typeof(System.Console);

            System.Reflection.FieldInfo _out = type.GetField("_out",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            System.Reflection.FieldInfo _error = type.GetField("_error",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            System.Reflection.MethodInfo _InitializeStdOutError = type.GetMethod("InitializeStdOutError",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            Debug.Assert(_out != null);
            Debug.Assert(_error != null);

            Debug.Assert(_InitializeStdOutError != null);

            _out.SetValue(null, null);
            _error.SetValue(null, null);

            _InitializeStdOutError.Invoke(null, new object[] { true });
        }

        static void SetOutAndErrorNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
    }
}
