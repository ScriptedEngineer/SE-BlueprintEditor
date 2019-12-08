using System;
using System.Collections.Generic;
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
        string UpdateLink,UpdLoge;
        public UpdateAvailable(string NewestVer,string updateLink)
        {
            InitializeComponent();
            window = this;
            UpdateLink = updateLink;
            AvailableVer.Content = Resource.Lang.UpdateAvailable+" - " +NewestVer;
            CurrentVer.Content = Resource.Lang.CurrentVer + " - " + MyExtensions.Version;
            UpdLoge = PrepareLog(MyExtensions.ApiServer(ApiServerAct.GetUpdateLog),Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName ,true);
            UpdLog.Text = UpdLoge;
        }

        string PrepareLog(string log, string Lid = "en", bool cut = false)
        {
            string[] Versions = log.Split('*');
            string Backlog = "";
            foreach (var version in Versions)
            {
                if (version == "") continue;
                bool breaked = false;
                //string[] versio = version.Split(':');
                string[] Strings = version.Split(new string[] { ":", "\n", "\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var stringe in Strings)
                {
                    if (stringe == "") continue;
                    string[] langs = stringe.Split('|');
                    if (langs.Length > 1)
                        Backlog += (langs[Lid == "ru" ? 1 : 0]) + "\r\n";
                    else
                    {
                        if (cut && langs[0] == MyExtensions.Version)
                        {
                            breaked = true;
                            break;
                        }
                        //Backlog += langs[0] + "\r\n";
                    }

                }
                if (breaked) break;
            }

            return Backlog;
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
            UpdLog.Text = UpdLoge;
        }
    }
}
