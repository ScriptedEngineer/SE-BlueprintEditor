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
using MColor = System.Windows.Media.Color;
using DColor = System.Drawing.Color;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace BlueprintEditor2
{
    static class MyExtensions
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
        public static string AppFile = System.Windows.Forms.Application.ExecutablePath;
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
        public static string CheckMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }
        public static string PostReq(string url, string postData)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }
        public static void QuickSort<T>(ref T[] a, Func<T, T, int> comparer) => QuickSort(ref a, 0, a.Length, comparer);
        public static void QuickSort<T>(ref T[] a, int l, int r, Func<T,T,int> comparer)
        {
            T temp;
            var x = a[l + (r - l) / 2];
            int i = l;
            int j = r-1;

            while (i <= j)
            {
                while (comparer(a[i], x) < 0) i++;
                while (comparer(a[j], x) > 0) j--;
                if (i <= j)
                {
                    temp = a[i];
                    a[i] = a[j];
                    a[j] = temp;
                    i++;
                    j--;
                }
            }
            if (i < r)
                QuickSort(ref a, i, r, comparer);

            if (l < j)
                QuickSort(ref a, l, j, comparer);
        }

        public static bool CheckGameLicense()
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam\Apps\244850", "Installed", -1) as int? >= 0;
        }
        public static string GetSteamLastGameNameUsed()
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "LastGameNameUsed", MySettings.Current.UserName) as string;
        }
        public static void ThemeChange(string style)
        {
            if (style == "Default")
                Application.Current.Resources.MergedDictionaries.Clear();
            else
            {
                var uri = new Uri("Themes/" + style + ".xaml", UriKind.Relative);
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
        }
        public static DateTime UnixTimestampToDateTime(double unixTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
        }
        public static string RegexMatch(string source, string regex)
        {
            Match Mxx = Regex.Match(source, regex);
            if (Mxx.Success) return Mxx.Groups[1].Value;
            else return null;
        }
        public static bool CheckVersion(string newest, string current)
        {
            if (string.IsNullOrEmpty(current)) return true;
            string[] VFc = current.Split('.');
            string[] VFl = newest.Split('.');
            bool oldVer = false;
            for (int i = 0; i < VFc.Length; i++)
            {
                if (VFc[i] != VFl[i])
                {
                    int.TryParse(VFc[i], out int VFci);
                    int.TryParse(VFl[i], out int VFli);
                    if (VFli > VFci) oldVer = true;
                }
            }
            return oldVer;
        }
    }
    public static class SE_ColorConverter
    {
        public static MColor ToMediaColor(this DColor color)
        {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }
        public static DColor ToDrawingColor(this MColor color)
        {
            return DColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static DColor ColorFromSE_HSV(double x, double y, double z)
        {
            try
            {
                double H, S, V;
                H = x * 360;
                S = y + 0.8;
                V = z + 0.45;
                return ColorFromHSV(H, Math.Max(Math.Min(S, 1), 0), Math.Max(Math.Min(V, 1), 0));
            }
            catch
            {
                return new DColor();
            }
        }
        public static void ColorToSE_HSV(DColor color, out double x, out double y, out double z)
        {
            try
            {
                ColorToHSV(color, out double H, out double S, out double V);
                x = H / 360;
                y = S - 0.8;
                z = V - 0.45;
            }
            catch
            {
                x = 0;
                y = 0;
                z = 0;
            }
        }

        #region Ctrl+C Ctrl+V
        public static void ColorToHSV(DColor color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }
        public static DColor ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return DColor.FromArgb(255, v, t, p);
            else if (hi == 1)
                return DColor.FromArgb(255, q, v, p);
            else if (hi == 2)
                return DColor.FromArgb(255, p, v, t);
            else if (hi == 3)
                return DColor.FromArgb(255, p, q, v);
            else if (hi == 4)
                return DColor.FromArgb(255, t, p, v);
            else
                return DColor.FromArgb(255, v, p, q);
        }
        #endregion
    }
    public enum ApiServerAct
    {
        CheckVersion,
        GetUpdateLog,
        GetCustomData,
        Report, 
        SteamApiGetPlayerSummaries,
        SteamApiGetPublishedFileDetails
    }
    public enum ApiServerOutFormat
    {
        @string,
        @bool,
        json,
        xml
    }

}
