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
            double H, S, V;
            H = x * 360;
            S = y + 0.8;
            V = z + 0.45;
            return ColorFromHSV(H, Math.Max(Math.Min(S,1),0), Math.Max(Math.Min(V,1),0));
        }
        public static void ColorToSE_HSV(DColor color, out double x, out double y, out double z)
        {
            double H, S, V;
            ColorToHSV(color, out H, out S, out V);
            x = H / 360;
            y = S - 0.8;
            z = V - 0.45;
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

            value = value * 255;
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
