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

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для Updater.xaml
    /// </summary>
    public partial class Updater : Window
    {
        string downUrl, LangDest;
        public Updater(string link)
        {
            downUrl = link;
            InitializeComponent();
            MyExtensions.AsyncWorker(() =>
            {
                if (MySettings.Current.LCID == 9)
                {
                    Status.Content = Resource.Lang.DownNewVer;
                    WebClient web = new WebClient();
                    web.DownloadFileAsync(new Uri(downUrl), Path.GetFileNameWithoutExtension(MyExtensions.AppFile) + ".update");
                    web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged2);
                    web.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted2);
                }
                else
                {
                    string LCID = MySettings.Current.LCID.ToString();
                    string CultureFolder = "", PackLink = "";
                    foreach (string langpack in MyExtensions.ApiServer(ApiServerAct.GetCustomData).Split('\n'))
                    {
                        var langpart = langpack.Trim().Split('|');
                        if(langpart.Length >= 3)
                        {
                            if(langpart[0] == LCID)
                            {
                                CultureFolder = langpart[1];
                                PackLink = langpart[2];
                            }
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(CultureFolder) && !string.IsNullOrWhiteSpace(PackLink))
                    {
                        Status.Content = Resource.Lang.DowLangPack;
                        string langpackfolder = Path.GetDirectoryName(MyExtensions.AppFile) + "/" + CultureFolder + "/";
                        if (!Directory.Exists(langpackfolder))
                            Directory.CreateDirectory(langpackfolder);
                        File.WriteAllText("lang.txt", CultureFolder);
                        LangDest = langpackfolder + "SE-BlueprintEditor.resources.dll";
                        WebClient web = new WebClient();
                        web.DownloadFileAsync(new Uri(PackLink), langpackfolder + "SE-BlueprintEditor.resources.dll.upd");
                        web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                        web.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
                    }
                    else
                    {
                        Status.Content = Resource.Lang.DownNewVer;
                        WebClient web = new WebClient();
                        web.DownloadFileAsync(new Uri(downUrl), Path.GetFileNameWithoutExtension(MyExtensions.AppFile) + ".update");
                        web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged2);
                        web.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted2);
                    }
                }
            });
        }
        public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
            Procents.Content = e.ProgressPercentage + "%";
            Status.Content = Resource.Lang.DowLangPack + " (" + e.BytesReceived + "bytes/" + e.TotalBytesToReceive + "bytes)";
        }
        public void DownloadProgressChanged2(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
            Procents.Content = e.ProgressPercentage + "%";
            Status.Content = Resource.Lang.DownNewVer + " (" + e.BytesReceived + "bytes/" + e.TotalBytesToReceive + "bytes)";
        }
        public void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Thread.Sleep(200);
            WebClient web = new WebClient();
            web.DownloadFileAsync(new Uri(downUrl), Path.GetFileNameWithoutExtension(MyExtensions.AppFile) + ".update");
            web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged2);
            web.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted2);
        }
        public void DownloadFileCompleted2(object sender, AsyncCompletedEventArgs e)
        {
            FileStream Batch = File.Create("update.vbs");
            string UpdFile = Path.GetFileNameWithoutExtension(MyExtensions.AppFile) + ".update";
            byte[] Data = Encoding.Default.GetBytes("WScript.Sleep(2000)"
+ "\r\nOn Error Resume next"
+ "\r\nDim fso, Del, Del2, Upd, WshShell"
+ "\r\nSet fso = CreateObject(\"Scripting.FileSystemObject\")"
+ "\r\nSet WshShell = WScript.CreateObject(\"WScript.Shell\")"
+ "\r\nSet Del = fso.GetFile(\"" + MyExtensions.AppFile + "\")"
+ "\r\nIf (fso.FileExists(\"" + UpdFile + "\")) Then"
+ "\r\n     Set Upd = fso.GetFile(\"" + UpdFile + "\")"
+ "\r\n     Del.Delete"
+ "\r\n     Upd.Name = \"" + Path.GetFileName(MyExtensions.AppFile) + "\""
+ "\r\n     WshShell.Run \"" + MyExtensions.AppFile + "\""
+ "\r\nElse"
+ "\r\n     WshShell.Run \"" + MyExtensions.AppFile + "\""
+ "\r\nEnd If"
+ "\r\nSet Del2 = fso.GetFile(\"" + LangDest + "\")"
+ "\r\nDel2.Delete"
+ "\r\nOn Error GoTo 0");
            Batch.Write(Data, 0, Data.Length);
            Batch.Close();
            Process.Start("update.vbs");
            Application.Current.Shutdown();
        }
    }
}
