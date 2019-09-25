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

namespace BlueprintEditor2
{
    class MyExtensions
    {
        public static string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static void AsyncWorker(Action act) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, act);
        public static string ApiServer(ApiServerAct Actione, ApiServerOutFormat Formate = ApiServerOutFormat.@string)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    return client.UploadString("https://wsxz.ru/api/"+ Actione.ToString() + "/"+ Formate.ToString(),
                        "{\"token\":\"J1H8MHUpN7N8BPZg9f9m6tf7NVHspVYo\",\"app\":\"SEBE2\",\"version\":\"" +
                         Version + "\"}");
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
    }
    
    public enum ApiServerAct
    {
        CheckVersion,
        GetUpdateLog
    }
    public enum ApiServerOutFormat
    {
        @string,
        @bool,
        json,
        xml
    }

}
