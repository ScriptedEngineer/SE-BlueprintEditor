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

namespace Updater
{
    /// <summary>
    /// Логика взаимодействия для Updater.xaml
    /// </summary>
    public partial class Updatere : Window
    {
        string downUrl;
        public Updatere()
        {
            if (!File.Exists("upd"))
            {
                Process.Start("SE-BlueprintEditor.exe");
                Application.Current.Shutdown();
                return;
            }
            InitializeComponent();
            string link = null;
            if (link == null)
            {
                while (true)
                {
                    try
                    {
                        File.Delete("SE-BlueprintEditor.exe");
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(100);
                    }
                }
                link = File.ReadAllText("upd");
                File.Delete("upd");
            }
            downUrl = link;
            
            MyExtensions.AsyncWorker(() =>
            {
                DownloadProg();
                /*
                if (MySettings.Current.LangCultureID == 9)
                {
                    DownloadProg();
                }
                else
                {
                    string LCID = MySettings.Current.LangCultureID.ToString();
                    CultureFolder = "";
                    string PackLink = "";
                    foreach (string langpack in MyExtensions.ApiServer(ApiServerAct.GetCustomData).Split('\n'))
                    {
                        var langpart = langpack.Trim().Split('|');
                        if (langpart.Length >= 3)
                        {
                            if (langpart[0] == LCID)
                            {
                                CultureFolder = langpart[1];
                                PackLink = langpart[2];
                            }
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(CultureFolder) && !string.IsNullOrWhiteSpace(PackLink))
                    {
                        
                        Status.Content = Resource.Lang.DowLangPack;
                        string langpackfolder = Path.GetDirectoryName(MyExtensions.AppFile) + "\\" + CultureFolder + "\\";
                        if (!Directory.Exists(langpackfolder))
                            Directory.CreateDirectory(langpackfolder);
                        File.WriteAllText("lang.txt", CultureFolder);
                        LangDest = langpackfolder + "SE-BlueprintEditor.resources.dll";
                        //File.Delete(LangDest);
                        //Process.Start("cmd", "/c del /f " + LangDest);
                        WebClient web = new WebClient();
                        web.DownloadFileAsync(new Uri(PackLink), LangDest);
                        web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                        web.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
                    }
                    else
                    {
                        DownloadProg();
                    }
                }*/
            });
        }
        public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
            Procents.Content = e.ProgressPercentage + "%";
            Status.Content = "Downloading language pack(" + e.BytesReceived + "bytes/" + e.TotalBytesToReceive + "bytes)";
        }
        public void DownloadProgressChanged2(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
            Procents.Content = e.ProgressPercentage + "%";
            Status.Content = "Downloading new version (" + e.BytesReceived + "bytes/" + e.TotalBytesToReceive + "bytes)";
        }
        public void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //Thread.Sleep(200);
            DownloadProg();
        }
        public void DownloadFileCompleted2(object sender, AsyncCompletedEventArgs e)
        {
            /*FileStream Batch = File.Create("update.vbs");
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
            //Process.Start("update.vbs");*/
            //File.WriteAllText("upd.bat",$"taskkill /f /im {Path.GetFileName(MyExtensions.AppFile)}"+
            //  (string.IsNullOrEmpty(LangDest) ?"":"\n rmdir /s /q "+ CultureFolder) +
            // "\n start SE-BlueprintEditor.exe");
            if (Directory.Exists("ru"))
            {
                MyExtensions.ClearFolder("ru");
                Directory.Delete("ru");
            }
            Process.Start("SE-BlueprintEditor.exe");
            //Process.Start("SE-BlueprintEditor.exe");
            Application.Current.Shutdown();
        }

        private void DownloadProg()
        {
            Status.Content = "Downloading new version";
            WebClient web = new WebClient();
            web.DownloadFileAsync(new Uri(downUrl), "SE-BlueprintEditor.exe");
            web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged2);
            web.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted2);
        }
    }
}
