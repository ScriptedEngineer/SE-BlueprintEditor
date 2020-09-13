using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using BlueprintEditor2.Resource;

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для Updater.xaml
    /// </summary>
    public partial class Updater : Window //Updater of Updater actually
    {
        string UpdaterPath = Path.Combine(Directory.GetCurrentDirectory(), "Updater.exe");
        public Updater(string link)
        {
            InitializeComponent();
            string lastVersion, downloadURL;
            using (var client = new System.Net.WebClient())
            {
                client.Headers.Add("User-Agent", "SE-BlueprintEditor");
                client.Encoding = Encoding.UTF8;
                string git_ingo = client.DownloadString("https://api.github.com/repos/ScriptedEngineer/AutoUpdater/releases");
                lastVersion = MyExtensions.RegexMatch(git_ingo, @"""tag_name"":""([^""]*)""");
                downloadURL = MyExtensions.RegexMatch(git_ingo, @"""browser_download_url"":""([^""]*)""");
            }
            bool updaterNeedUpdate = true;
            if (File.Exists(UpdaterPath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(UpdaterPath);
                updaterNeedUpdate = MyExtensions.CheckVersion(lastVersion, versionInfo.ProductVersion);
            }
            if(updaterNeedUpdate)
            {
                Status.Content = Lang.DownNewVer;
                WebClient web = new WebClient();
                web.DownloadFileAsync(new Uri(downloadURL), UpdaterPath);
                web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                web.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
            }
            else
            {
                DownloadFileCompleted(this,null);
            }
        }
        public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
            Procents.Content = e.ProgressPercentage + "%";
            Status.Content = Lang.DownNewVer+"... (" + (e.BytesReceived/1024) + "kb of " + (e.TotalBytesToReceive/1024) + "kb)";
        }
        public void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Process.Start(UpdaterPath, (MySettings.Current.Theme == MyThemeEnum.Dark? "/DarkTheme " : "")+ "/DownloadingFile \"" + Lang.DownloadUpdate + "\" /ExtractingUpdate \"" + Lang.ExtractingUpdate + "\" /PleaseWait \"" + Lang.PleaseWaitServers + "\" /GitHub \"ScriptedEngineer/SE-BlueprintEditor\" /RunApp \"" + MyExtensions.AppFile+"\" /JustDownload");
            Application.Current.Shutdown();
        }
    }
}
