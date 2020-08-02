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
            ItemTitle.Content = Lang.PleaseWait;
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
                response aResponse = null;
                string Type = "Undefined";
                try
                {
                    FileID = text.Split(new string[] { "id=" }, StringSplitOptions.RemoveEmptyEntries).Last().Split('&').First();
                    if (!long.TryParse(FileID, out long xss)) 
                        throw new Exception();
                    string backData = MyExtensions.ApiServer(
                        ApiServerAct.SteamApiGetPublishedFileDetails,
                        ApiServerOutFormat.@string,
                        $",\"fileid\":\"{FileID}\",\"format\":\"xml\"");
                    XmlSerializer formatter = new XmlSerializer(typeof(response));
                    using (var reader = new StringReader(backData))
                    {
                        aResponse = (response)formatter.Deserialize(reader);
                        LoadLink = aResponse.publishedfiledetails.publishedfile.file_url;
                        FileID = aResponse.publishedfiledetails.publishedfile.publishedfileid.ToString();
                        if (aResponse.publishedfiledetails.publishedfile.tags != null)
                            foreach (var x in aResponse.publishedfiledetails.publishedfile.tags)
                            {
                                switch (x.tag)
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
                }
                finally
                {
                    MyExtensions.AsyncWorker(() =>
                    {
                        Title = "Workshop downloader - SE BlueprintEditor";
                        StatusLabel.Content = "";
                        if (aResponse == null
                        || string.IsNullOrEmpty(aResponse?.publishedfiledetails?.publishedfile?.title))
                        {
                            ItemTitle.Content = Lang.Error;
                            DownloadProgress.IsIndeterminate = false;
                            return;
                        }
                        var fleinfod = aResponse.publishedfiledetails.publishedfile;
                        if (fleinfod.consumer_app_id != 244850)
                        {
                            ItemInfo.Content = Lang.FileFromAnnotherGame;

                        }
                        else
                        {
                            ItemInfo.Content = $"{Lang.Type}: {Type}\r\n{Lang.Size}: {fleinfod.file_size / 1024}kb\r\n{Lang.LastUpdate}: {MyExtensions.UnixTimestampToDateTime(fleinfod.time_updated)}";
                        }
                        ItemTitle.Content = FileTitle = fleinfod.title;
                        if (string.IsNullOrEmpty(fleinfod.preview_url))
                            fleinfod.preview_url = "https://steamcommunity-a.akamaihd.net/public/images/sharedfiles/steam_workshop_default_image.png";
                        FilePicture.Source = new BitmapImage(new Uri(fleinfod.preview_url, UriKind.RelativeOrAbsolute));
                        DownloadButton.IsEnabled = !string.IsNullOrEmpty(LoadFolder);
                        if (!DownloadButton.IsEnabled)
                        {
                            StatusLabel.Content = "downloading is not possible";
                        }
                        DownloadProgress.Maximum = fleinfod.file_size;
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
                        string ansv = MyExtensions.PostReq("https://api.steamworkshopdownloader.io/api/download/request",
                            $"{{\"publishedFileId\":{FileID},\"collectionId\":null,\"extract\":false,\"hidden\":false,\"direct\":false,\"autodownload\":false}}");
                        var rgx = Regex.Match(ansv, "\"uuid\":\"([^\"]*)\"");
                        string uuid = rgx.Groups[1].Value;
                        string status, anssv;
                        do
                        {
                            anssv = MyExtensions.PostReq("https://api.steamworkshopdownloader.io/api/download/status",
                                $"{{\"uuids\":[\"{uuid}\"]}}");
                            status = Regex.Match(anssv, "\"status\":\"([^\"]*)\"").Groups[1].Value;
                            string statusDesc = Regex.Match(anssv, "\"progressText\":\"([^\"]*)\"").Groups[1].Value;
                            MyExtensions.AsyncWorker(() => { StatusLabel.Content = statusDesc; });
                            Thread.Sleep(500);
                        }
                        while (status == "dequeued" || status == "retrieving");
                        Thread.Sleep(100);
                        if(!string.IsNullOrEmpty(uuid))
                            LoadLink = "https://api.steamworkshopdownloader.io/api/download/transmit?uuid="+uuid;
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

        // Примечание. Для запуска созданного кода может потребоваться NET Framework версии 4.5 или более поздней версии и .NET Core или Standard версии 2.0 или более поздней.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class response
        {

            /// <remarks/>
            public byte result { get; set; }

            /// <remarks/>
            public byte resultcount { get; set; }

            /// <remarks/>
            public responsePublishedfiledetails publishedfiledetails { get; set; }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class responsePublishedfiledetails
        {

            /// <remarks/>
            public responsePublishedfiledetailsPublishedfile publishedfile { get; set; }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class responsePublishedfiledetailsPublishedfile
        {

            /// <remarks/>
            public uint publishedfileid { get; set; }

            /// <remarks/>
            public byte result { get; set; }

            /// <remarks/>
            public ulong creator { get; set; }

            /// <remarks/>
            public uint creator_app_id { get; set; }

            /// <remarks/>
            public uint consumer_app_id { get; set; }

            /// <remarks/>
            public object filename { get; set; }

            /// <remarks/>
            public uint file_size { get; set; }

            /// <remarks/>
            public string file_url { get; set; }

            /// <remarks/>
            public ulong hcontent_file { get; set; }

            /// <remarks/>
            public string preview_url { get; set; }

            /// <remarks/>
            public ulong hcontent_preview { get; set; }

            /// <remarks/>
            public string title { get; set; }

            /// <remarks/>
            public string description { get; set; }

            /// <remarks/>
            public long time_created { get; set; }

            /// <remarks/>
            public long time_updated { get; set; }

            /// <remarks/>
            public byte visibility { get; set; }

            /// <remarks/>
            public byte banned { get; set; }

            /// <remarks/>
            public object ban_reason { get; set; }

            /// <remarks/>
            public ushort subscriptions { get; set; }

            /// <remarks/>
            public byte favorited { get; set; }

            /// <remarks/>
            public ushort lifetime_subscriptions { get; set; }

            /// <remarks/>
            public byte lifetime_favorited { get; set; }

            /// <remarks/>
            public ushort views { get; set; }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("tag", IsNullable = false)]
            public responsePublishedfiledetailsPublishedfileTag[] tags { get; set; }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class responsePublishedfiledetailsPublishedfileTag
        {

            /// <remarks/>
            public string tag { get; set; }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) => Process.Start(((Hyperlink)sender).NavigateUri.ToString());
    }
}
