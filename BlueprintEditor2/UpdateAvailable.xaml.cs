using BlueprintEditor2.Resource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для UpdateAvailable.xaml
    /// </summary>
    public partial class UpdateAvailable : Window
    {
        static public UpdateAvailable window;
        static internal bool last_open = false;
        private readonly string UpdateLink;

        public UpdateAvailable(string NewestVer,string updateLink, string git_ingo)
        {
            Logger.Add("Update available show");
            InitializeComponent();
            window = this;
            UpdateLink = updateLink;
            AvailableVer.Content = Resource.Lang.UpdateAvailable+" - " +NewestVer;
            CurrentVer.Content = Resource.Lang.CurrentVer + " - " + MyExtensions.Version;
            //UpdLoge = PrepareLog(MyExtensions.ApiServer(ApiServerAct.GetUpdateLog),Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName ,true);
            string updLog = "";
            string[] gVersions = git_ingo.Split(new string[] { @"},{""url"":" }, StringSplitOptions.RemoveEmptyEntries);
            Logger.Add("Parse GitHub log");
            foreach (string gVer in gVersions) {
                string vsi = MyExtensions.RegexMatch(gVer, @"""tag_name"":""([^""]*)""");
                if (vsi == MyExtensions.Version) break;
                string[] Lines = MyExtensions.RegexMatch(gVer, @"""body"":""([^""]*)""").Split(new string[]{"\\r\\n"}, StringSplitOptions.RemoveEmptyEntries);
                bool writes = !(MySettings.Current.LangCultureID == 1049);
                foreach (var Line in Lines)
                {
                    if(Line.StartsWith("# "))
                    {
                        string[] lang = Line.Split(']');
                        if (lang[0] == "# [RU") writes = !writes;
                        if (lang.Length == 1) writes = true;
                        if (writes) updLog += "\r\n" + Line + "\r\n";
                    }
                    else
                    if (Line.StartsWith("### "))
                    {
                        //Ingore
                    }
                    else
                    {
                        if(writes) updLog += Line + "\r\n";
                    }
                }
                //Console.WriteLine(gVer);
            }
            UpdLog.Text = updLog.Trim();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (last_open) Application.Current.Shutdown();
            else
            {
                e.Cancel = true;
                Hide();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (last_open) Application.Current.Shutdown();
            else Hide();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MyExtensions.CloseAllWindows();
            new Updater(UpdateLink).Show();
        }

        private void UpdLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            //UpdLog.Text = UpdLoge;
        }
    }
}
