using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueprintEditor2.Classes
{
    public static class CrashLogger
    {
        public static void HandleUhEx()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashHappens);
        }
        private static void CrashHappens(object sende, UnhandledExceptionEventArgs UhEx)
        {
            if (!UhEx.IsTerminating) return;
            string Crash = "";
            Exception e = (Exception)UhEx.ExceptionObject;
            Crash += "Error message: " + e.Message;
            Crash += "\r\nSource: " + e.Source;
            Crash += "\r\nTarget site name: " + e.TargetSite.Name;
            Crash += "\r\nTarget site module name: " + e.TargetSite.Module.Name;
            //Crash += "\r\nStack trace:\r\n" + e.StackTrace;
            File.WriteAllText("LastCrash.txt", Crash);
            Process.Start(MyExtensions.AppFile,"Crash");
        }
    }
}
