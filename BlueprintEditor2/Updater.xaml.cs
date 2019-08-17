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

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для Updater.xaml
    /// </summary>
    public partial class Updater : Window
    {
        string AppFile;
        public Updater(string link)
        {
            InitializeComponent();
            MyExtensions.AsyncWorker(() =>
            {
                WebClient web = new WebClient();
                AppFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
                Console.WriteLine(AppFile);
                web.DownloadFileAsync(new Uri(link), Path.GetFileNameWithoutExtension(AppFile) + ".update");
                web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged2);
                web.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted2);
            });
        }
        public void DownloadProgressChanged2(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
            Procents.Content = e.ProgressPercentage+"%";
            Status.Content = Resource.Lang.DownNewVer + " ("+e.BytesReceived+"bytes/"+e.TotalBytesToReceive+"bytes)";
        }
        public void DownloadFileCompleted2(object sender, AsyncCompletedEventArgs e)
        {
            FileStream Batch = File.Create("update.vbs");
            string UpdFile = Path.GetFileNameWithoutExtension(AppFile) + ".update";
            byte[] Data = Encoding.Default.GetBytes("WScript.Sleep(500)"
+ "\r\nOn Error Resume next"
+ "\r\nDim fso, Del, Upd, WshShell"
+ "\r\nSet fso = CreateObject(\"Scripting.FileSystemObject\")"
+ "\r\nSet WshShell = WScript.CreateObject(\"WScript.Shell\")"
+ "\r\nSet Del = fso.GetFile(\"" + AppFile + "\")"
+ "\r\nIf (fso.FileExists(\""+ UpdFile + "\")) Then"
+ "\r\n     Set Upd = fso.GetFile(\"" + UpdFile + "\")"
+ "\r\n     Del.Delete"
+ "\r\n     Upd.Name = \"" + Path.GetFileName(AppFile) + "\""
+ "\r\n     WshShell.Run \"" + AppFile + "\""
+ "\r\nElse"
+ "\r\n     WshShell.Run \"" + AppFile + "\""
+ "\r\nEnd If"
+ "\r\nOn Error GoTo 0");
            Batch.Write(Data, 0, Data.Length);
            Batch.Close();
            Process.Start("update.vbs");
            Application.Current.Shutdown();
        }
    }
}
