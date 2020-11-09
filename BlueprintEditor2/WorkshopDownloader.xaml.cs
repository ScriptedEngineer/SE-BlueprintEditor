using BlueprintEditor2.Resource;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml.Serialization;

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для WorkshopDownloader.xaml
    /// </summary>
    public partial class WorkshopDownloader : Window
    {
        public static WorkshopDownloader Opened;
        private string LoadFolder = "";
        private string LoadLink = "";
        private string FileID = "", FileTitle = "";
        public WorkshopDownloader()
        {
            Opened = this;
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Opened = null;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.Changes.FirstOrDefault()?.AddedLength <= 1)
            {
                return;
            }
            Title = "Workshop downloader - SE BlueprintEditor loading...";
            ItemTitle.Text = Lang.PleaseWait;
            StatusLabel.Content = "loading file info";
            ItemInfo.Content = "";
            FilePicture.Source = new BitmapImage(new Uri("https://steamcommunity-a.akamaihd.net/public/images/sharedfiles/steam_workshop_default_image.png", UriKind.RelativeOrAbsolute));
            DownloadProgress.IsIndeterminate = true;
            DownloadProgress.Value = 0;
            DownloadButton.IsEnabled = false;
            LoadFolder = LoadLink = null;
            string text = SteamLink.Text;
            new Task(() =>
            {
                string aResponse = null;
                string Type = "Undefined";
                try
                {
                    FileID = text.Split(new string[] { "id=" }, StringSplitOptions.RemoveEmptyEntries).Last().Split('&').First();
                    if (!long.TryParse(FileID, out long xss))
                        throw new Exception();
                    aResponse = MyExtensions.ApiServer(
                        ApiServerAct.SteamApiGetPublishedFileDetails,
                        ApiServerOutFormat.@string,
                        $",\"fileid\":\"{FileID}\",\"format\":\"xml\"");
                    LoadLink = MyExtensions.RegexMatch(aResponse, @"<file_url>([^<]*)<\/file_url>");
                    FileID = MyExtensions.RegexMatch(aResponse, @"<publishedfileid>([^<]*)<\/publishedfileid>");
                    MatchCollection Tags = Regex.Matches(aResponse, @"<tag>([^<]*)<\/tag>");
                    if (Tags.Count > 0) 
                        foreach (Match x in Tags)
                        {
                            switch (x.Groups[1].Value)
                            {
                                case "blueprint":
                                    LoadFolder = MySettings.Current.BlueprintPatch;
                                    Type = "Blueprint";
                                    break;
                                case "world":
                                    LoadFolder = MySettings.Current.SavesPatch;
                                    Type = "World";
                                    break;
                                case "ingameScript":
                                    LoadFolder = MySettings.Current.ScriptsPatch;
                                    Type = "Script";
                                    break;
                                case "mod":
                                    LoadFolder = MySettings.Current.ModsPatch;
                                    Type = "Mod";
                                    break;
                                case "Scenario":
                                    Type = "Scenario";
                                    break;
                            }
                        }
                }
                finally
                {
                    MyExtensions.AsyncWorker(() =>
                    {
                        Title = "Workshop downloader - SE BlueprintEditor";
                        StatusLabel.Content = "";
                        FileTitle = MyExtensions.RegexMatch(aResponse, @"<title>([^<]*)<\/title>");
                        if (aResponse == null
                        || string.IsNullOrEmpty(FileTitle))
                        {
                            ItemTitle.Text = Lang.Error;
                            DownloadProgress.IsIndeterminate = false;
                            return;
                        }
                        long.TryParse(MyExtensions.RegexMatch(aResponse, @"<file_size>([^<]*)<\/file_size>"), out long file_size);
                        if (MyExtensions.RegexMatch(aResponse, @"<consumer_app_id>([^<]*)<\/consumer_app_id>") != "244850")
                        {
                            ItemInfo.Content = Lang.FileFromAnnotherGame;
                        }
                        else
                        {
                            double.TryParse(MyExtensions.RegexMatch(aResponse, @"<time_updated>([^<]*)<\/time_updated>"), out double time_updated);
                            ItemInfo.Content = $"{Lang.Type}: {Type}\r\n{Lang.Size}: {file_size / 1024}kb\r\n{Lang.LastUpdate}: {MyExtensions.UnixTimestampToDateTime(time_updated)}";
                        }
                        ItemTitle.Text = FileTitle;
                        string preview = MyExtensions.RegexMatch(aResponse, @"<preview_url>([^<]*)<\/preview_url>");
                        if (string.IsNullOrEmpty(preview))
                            preview = "https://steamcommunity-a.akamaihd.net/public/images/sharedfiles/steam_workshop_default_image.png";
                        FilePicture.Source = new BitmapImage(new Uri(preview, UriKind.RelativeOrAbsolute));
                        DownloadButton.IsEnabled = !string.IsNullOrEmpty(LoadFolder);
                        if (!DownloadButton.IsEnabled)
                        {
                            StatusLabel.Content = "downloading is not possible";
                        }
                        DownloadProgress.Maximum = file_size;
                        DownloadProgress.IsIndeterminate = false;
                    });
                }
            }).Start();
        }
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FileID)) return;
            if(string.IsNullOrEmpty(LoadFolder)) return;
            DownloadProgress.IsIndeterminate = true;
            StatusLabel.Content = "trying download";
            new Task(() =>
            {
                if (string.IsNullOrEmpty(LoadLink))
                {
                    try
                    {
                        string ansv = MyExtensions.PostReq("https://api_02.steamworkshopdownloader.io/api/download/request",
                            $"{{\"publishedFileId\":{FileID},\"collectionId\":null,\"extract\":false,\"hidden\":false,\"direct\":false,\"autodownload\":false}}");
                        var rgx = Regex.Match(ansv, "\"uuid\":\"([^\"]*)\"");
                        string uuid = rgx.Groups[1].Value;
                        string status, anssv;
                        do
                        {
                            anssv = MyExtensions.PostReq("https://api_02.steamworkshopdownloader.io/api/download/status",
                                $"{{\"uuids\":[\"{uuid}\"]}}");
                            status = Regex.Match(anssv, "\"status\":\"([^\"]*)\"").Groups[1].Value;
                            string statusDesc = Regex.Match(anssv, "\"progressText\":\"([^\"]*)\"").Groups[1].Value;
                            MyExtensions.AsyncWorker(() => { StatusLabel.Content = statusDesc; });
                            Thread.Sleep(2000);
                        }
                        while (status == "dequeued" || status == "retrieving");
                        Thread.Sleep(100);
                        if(!string.IsNullOrEmpty(uuid))
                            LoadLink = "https://api_02.steamworkshopdownloader.io/api/download/transmit?uuid=" + uuid;
                    }
                    catch { }
                }
                if (string.IsNullOrEmpty(LoadLink))
                {
                    MyExtensions.AsyncWorker(() => { 
                        StatusLabel.Content = "Error";
                        DownloadProgress.IsIndeterminate = false;
                    });
                    return;
                }
                MyExtensions.AsyncWorker(() =>
                {
                    StatusLabel.Content = "start downloading";
                    WebClient web = new WebClient();
                    web.DownloadFileAsync(new Uri(LoadLink), "tmp.zip");
                    web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Web_DownloadProgressChanged);
                    web.DownloadFileCompleted += new AsyncCompletedEventHandler(Web_DownloadFileCompleted);
                });
            }).Start();
        }

        private void Web_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            StatusLabel.Content = "downloaded";
            DownloadProgress.Value = DownloadProgress.Maximum;
            string extractPath = LoadFolder.TrimEnd('\\') + @"\" + FileTitle;
            new Task(() =>
            {
                MyExtensions.AsyncWorker(() => StatusLabel.Content = "try to extract");
                if (!Directory.Exists(extractPath))
                    Directory.CreateDirectory(extractPath);
                else
                    MyExtensions.ClearFolder(extractPath);
                System.IO.Compression.ZipFile.ExtractToDirectory("tmp.zip", extractPath);
                if (File.Exists("tmp.zip"))
                    File.Delete("tmp.zip");
                MyExtensions.AsyncWorker(() => StatusLabel.Content = "download completed");
            }).Start();
        }

        private void Web_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            StatusLabel.Content = $"downloading...";
            DownloadProgress.Value = e.BytesReceived;
            DownloadProgress.IsIndeterminate = false;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) => Process.Start(((Hyperlink)sender).NavigateUri.ToString());
    }
}
