using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Security.Cryptography;
using System.IO;

namespace Updater
{
    class MyExtensions
    {
        public static string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static void AsyncWorker(Action act) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, act);
        public static string ApiServer(ApiServerAct Actione, ApiServerOutFormat Formate = ApiServerOutFormat.@string, string JsonData = "")
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    string json = "{\"token\":\"J1H8MHUpN7N8BPZg9f9m6tf7NVHspVYo\",\"app\":\"SEBE2\",\"version\":\"" +
                         Version + "\"" + JsonData + "}";
                    client.Encoding = Encoding.UTF8;
                    return client.UploadString("https://wsxz.ru/api/"+ Actione.ToString() + "/"+ Formate.ToString(),json);
                }
            }
            catch
            {
                return "Error(Api unavailable)";
            }
        }
        public static string AppFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
        public static void CloseAllWindows()
        {
            for (int intCounter = Application.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
                Application.Current.Windows[intCounter].Hide();
        }
        static public void ClearFolder(string dir)
        {
            string[] files = System.IO.Directory.GetFiles(dir);
            foreach (string file in files)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch
                {

                }
            }
            files = System.IO.Directory.GetDirectories(dir);
            foreach (string file in files)
            {
                ClearFolder(file);
                try
                {
                    System.IO.Directory.Delete(file);
                }
                catch
                {

                }
            }
        }
        public static string checkMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }
    }
    public enum ApiServerAct
    {
        CheckVersion,
        GetUpdateLog,
        GetCustomData,
        Report
    }
    public enum ApiServerOutFormat
    {
        @string,
        @bool,
        json,
        xml
    }

}
