using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using BlueprintEditor2.Resource;
using System.Diagnostics;
using Path = System.IO.Path;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Text;
using System.Globalization;

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class SelectBlueprint : Window
    {
        public static SelectBlueprint window;
        internal MyXmlBlueprint CurrentBlueprint;
        string currentBluePatch;
        public SelectBlueprint()
        {
#if DEBUG
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
#endif
            MySettings.Deserialize();
            MySettings.Current.ApplySettings();
            window = this;
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && args[1] == "Crash")
            {
#if DEBUG
                Application.Current.Shutdown();
                return;
#else
                new Reporter().Show();
                Hide();
                return;
#endif
            }
            Logger.HandleUnhandledException();
            Thread.CurrentThread.Name = "Main";
            Logger.Add("Startup"); 
            if (File.Exists("update.vbs")) File.Delete("update.vbs");
            if (File.Exists("upd.bat")) File.Delete("upd.bat");
            if (File.Exists("Updater.exe"))
            {
                try
                {
                    File.Delete("Updater.exe");
                }
                catch 
                {
                    
                }
            }
            /*if (File.Exists("lang.txt"))
                try
                {
                    Logger.Add("Post update language pack init");
                    string langfold = File.ReadAllText("lang.txt");
                    File.Delete("./" + langfold + "/SE-BlueprintEditor.resources.dll");
                    File.Move("./" + langfold + "/SE-BlueprintEditor.resources.dll.upd", "./" + langfold + "/SE-BlueprintEditor.resources.dll");
                    File.Delete("lang.txt");
                    /*FileStream Batch = File.Create("update.vbs");
                    byte[] Data = Encoding.Default.GetBytes("WScript.Sleep(2000)"
        + "\r\nOn Error Resume next"
        + "\r\nSet WshShell = WScript.CreateObject(\"WScript.Shell\")"
        + "\r\n     WshShell.Run \"" + MyExtensions.AppFile + "\""
        + "\r\nOn Error GoTo 0");
                    Batch.Write(Data, 0, Data.Length);
                    Batch.Close();*//*
                    Process.Start(MyExtensions.AppFile);
                    Application.Current.Shutdown();
                }
                catch(Exception e)
                {
                    Logger.Add($"Error {e.Message}");
                }*/
            if (MySettings.Current.LangCultureID == 1049 && !Directory.Exists("ru"))
                try
                {
                    Logger.Add("language pack not exists. language pack downloading");
                    string langpackfolder = Path.GetDirectoryName(MyExtensions.AppFile) + "/ru/";
                    if (!Directory.Exists(langpackfolder))
                        Directory.CreateDirectory(langpackfolder);
                    WebClient web = new WebClient();
                    web.DownloadFile(new Uri(@"https://wsxz.ru/downloads/SE-BlueprintEditor.resources.dll"), langpackfolder + "SE-BlueprintEditor.resources.dll");
                    Process.Start(MyExtensions.AppFile);
                    Application.Current.Shutdown();
                }
                catch (Exception e)
                {
                    Logger.Add($"Error {e.Message}");
                }
            Logger.Add("Init GUI");
            InitializeComponent();
            currentBluePatch = MySettings.Current.BlueprintPatch;
            InitBlueprints();
            new Task(() =>
            {
                Thread.CurrentThread.Name = "Updating";
                Logger.Add("Check Update");
                string[] Vers = MyExtensions.ApiServer(ApiServerAct.CheckVersion).Split(' ');
                if (Vers.Length == 3) {
                    if (Vers[0] == "0")
                    { 
                        Logger.Add("Update found");
                        MyExtensions.AsyncWorker(() => new UpdateAvailable(Vers[2], Vers[1]).Show());
                        if (!File.Exists("Updater.exe"))
                        {
                            WebClient web = new WebClient();
                            web.DownloadFile(new Uri(@"https://wsxz.ru/downloads/Updater.exe"), "Updater.exe");
                        }
                    }
                    else
                    {
                        Logger.Add("Update not found");
                    }
                }
                else
                {
                    Logger.Add("No have access to API");
                }
                //MyExtensions.AsyncWorker(() => new Dialog(x => Console.WriteLine(x), DialogPicture.attention, "TEST", "PleaseInput").Show());
            }).Start();
            Logger.Add("Finish Init GUI");
            OldSortBy = FirstSorter;
            if (string.IsNullOrWhiteSpace(MySettings.Current.SteamWorkshopPatch))
            {
                ToolsMenu.IsEnabled = false;
                ToolsContext.IsEnabled = false;
            }
            Welcome.Content = Lang.Welcome+" "+MySettings.Current.UserName.Replace("_", "__");
            //MessageBox.Show("Hello "+MySettings.Current.UserName);
            Logger.Add("GUI Loaded");
            new Task(() => {
                Thread.CurrentThread.Name = "DataParser";
                Logger.Add("Game data parse start");
                MyGameData.Init();
                Logger.Add("Game data parse end");
                MyExtensions.AsyncWorker(() => { 
                CalculateButton.IsEnabled = BlueList.SelectedIndex != -1;
                });
            }).Start();
        }
        internal void BlueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyDisplayBlueprint Selected = (MyDisplayBlueprint)BlueList.SelectedItem;
            if (Selected != null && File.Exists(currentBluePatch + Selected.Name + "\\bp.sbc"))
            {
                Logger.Add($"Bluerpint '{Selected.Name}' selected");
                CurrentBlueprint = new MyXmlBlueprint(currentBluePatch + Selected.Name);
                if(MySettings.Current.DontOpenBlueprintsOnScan)Selected.addXmlData(CurrentBlueprint);
                BluePicture.Source = CurrentBlueprint.GetPic();
                string Owner = Selected.Owner + "(" + CurrentBlueprint.Owner + ")";
                if (CurrentBlueprint.Owner == MySettings.Current.SteamID)
                    Owner = MySettings.Current.UserName + "(You)";
                BlueText.Text = Lang.Blueprint + ": " + Selected.Name + "\n" +
                    Lang.Name + ": " + CurrentBlueprint.Name + "\n" +
                    Lang.Created + ": " + Selected.CreationTimeText + "\n" +
                    Lang.Changed + ": " + Selected.LastEditTimeText + "\n" +
                    Lang.GridCount + ": " + Selected.GridCountText + "\n" +
                    Lang.BlockCount + ": " + Selected.BlockCountText + "\n" +
                    Lang.Owner + ": " + Owner + "\n";
                CalculateButton.IsEnabled = MyGameData.IsInitialized;
                EditButton.IsEnabled = true;
                PrefabButton.IsEnabled = true;
                BackupButton.IsEnabled = Directory.Exists(CurrentBlueprint.Patch + "/Backups");
                foreach (string file in Directory.GetFiles(CurrentBlueprint.Patch, "bp.sbc*", SearchOption.TopDirectoryOnly))
                {
                    if (File.Exists(file) && Path.GetFileName(file) != "bp.sbc") File.Delete(file);
                }
                if (MySettings.Current.DontOpenBlueprintsOnScan)
                {
                    if (OldSortBy != null)
                        GoSort(OldSortBy, null);
                    BlueList.ScrollIntoView(Selected);
                }
            }
            else
            {
                Logger.Add($"Bluerpint unselected");
                if (BlueList.SelectedIndex != -1) BlueList.Items.Remove(BlueList.SelectedItem);
                CurrentBlueprint = null;
                BluePicture.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/blueprints-textures_00394054.jpg"));
                BlueText.Text = Lang.SelectBlue;
                CalculateButton.IsEnabled = false;
                EditButton.IsEnabled = false;
                PrefabButton.IsEnabled = false;
                BackupButton.IsEnabled = false;
            }
            Height++; Height--;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(CurrentBlueprint.Patch + "/~lock.dat"))
            {
                
                if (!MySettings.Current.MultiWindow) Hide();
                else
                {
                    //Left = 0;
                    //Top = SystemParameters.PrimaryScreenHeight / 2 - (Height / 2);
                }
                CurrentBlueprint.SaveBackup();
                Logger.Add($"Open editor for [{CurrentBlueprint.Name}]");
                EditBlueprint Form = new EditBlueprint(File.Create(CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose), CurrentBlueprint);
                Form.Show();
                BackupButton.IsEnabled = true;
            }
            else
            {
                Logger.Add($"Try to open editor for [{CurrentBlueprint.Name}], but blueprint locked");
                new MessageDialog(DialogPicture.warn, "Error", Lang.AlreadyOpened, null, DialogType.Message).Show();
            }
        }
        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(CurrentBlueprint.Patch + "/~lock.dat"))
            {
                Logger.Add($"Open backups for [{CurrentBlueprint.Name}]");
                new BackupManager(File.Create(CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose), CurrentBlueprint).Show();
            }
            else
            {
                Logger.Add($"Try to open backups for [{CurrentBlueprint.Name}], but blueprint locked");
                new MessageDialog(DialogPicture.warn, "Error", Lang.AlreadyOpened, null, DialogType.Message).Show();
            }
        }
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            //new MesassageDialog(DialogPicture.attention, "Attention", Lang.ComingSoon, null, DialogType.Message).Show(); return;
            if (!File.Exists(CurrentBlueprint.Patch + "/~lock.dat"))
            {
                if (!MySettings.Current.MultiWindow) Hide();
                else
                {
                    //Left = SystemParameters.PrimaryScreenWidth / 2 - ((360 + 800) / 2);
                    //Top = SystemParameters.PrimaryScreenHeight / 2 - (Height / 2);
                }
                Logger.Add($"Open calculator for [{CurrentBlueprint.Name}]");
                Calculator Form = new Calculator(File.Create(CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose), CurrentBlueprint);
                try
                {
                    Form.Show();
                }
                catch
                {
                    Focus();
                }
            }
            else
            {
                Logger.Add($"Try to open calculator for [{CurrentBlueprint.Name}], but blueprint locked");
                new MessageDialog(DialogPicture.warn, "Error", Lang.AlreadyOpened, null, DialogType.Message).Show();
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            InitBlueprints();
        }
        private void PicMenuItemNormalize_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBlueprint != null)
            {
                Logger.Add($"Normalize picture for [{CurrentBlueprint.Name}]");
                BluePicture.Source = CurrentBlueprint.GetPic(true);
            }
        }
        private void SelectorMenuItemFolder_Click(object sender, RoutedEventArgs e)
        {
            Logger.Add("Open blueprints patch");
            Process.Start(MySettings.Current.BlueprintPatch);
        }
        private void SelectorMenuItemFolder2_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBlueprint != null)
            {
                Logger.Add($"Open directory for [{CurrentBlueprint.Name}]");
                Process.Start(CurrentBlueprint.Patch);
            }
            else
            {
                Logger.Add("Try to open blueprint directory, but blueprint not selected");
                new MessageDialog(DialogPicture.attention, "Attention", Lang.SelectBlueForOpen, null, DialogType.Message).Show();
            }
        }
        private void BackupsMenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            Lock.Height = SystemParameters.PrimaryScreenHeight;
            Lock.DataContext = 0;
            Logger.Add("Show delete backups attention");
            new MessageDialog(DialogPicture.warn, Lang.UnsafeAction, Lang.ItWillDeleteAllBackps, (Dial) =>
            {
                if (Dial == DialоgResult.Yes)
                {
                    Logger.Add("Delete all backups in folder");
                    foreach (string dir in Directory.GetDirectories(currentBluePatch))
                    {
                        if (Directory.Exists(dir + "\\Backups")) Directory.Delete(dir + "\\Backups", true);
                    }
                    BlueList_SelectionChanged(null, null);
                }
                Lock.Height = 0;
            }).Show();
        }
        private void WindowsMenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            Logger.Add("About window open");
            if (About.LastWindow == null) new About().Show();
            else About.LastWindow.Focus();
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Logger.Add("Settings window open");
            if (Settings.LastWindow == null) new Settings().Show();
            else Settings.LastWindow.Focus();
        }
        private void MenuFolderItem_Click(object sender, RoutedEventArgs e)
        {
            string folder = ((MenuItem)sender).Header.ToString();
            Logger.Add($"Open directory [{folder}]");
            string newDir = currentBluePatch + folder + "\\";
            if (Directory.Exists(newDir))
            {
                Logger.Add("Directory opened");
                currentBluePatch = newDir;
                InitBlueprints();
            }
            else
            {
                Logger.Add("Directory not exists");
                ((MenuItem)sender).Visibility = Visibility.Collapsed;
            }
        }
        private void MenuBackItem_Click(object sender, RoutedEventArgs e)
        {
            Logger.Add("Directory go back");
            currentBluePatch = Path.GetDirectoryName(currentBluePatch.TrimEnd('\\')) + "\\";
            InitBlueprints();
        }
        private void MenuHomeItem_Click(object sender, RoutedEventArgs e)
        {
            Logger.Add("Directory go home");
            currentBluePatch = MySettings.Current.BlueprintPatch;
            InitBlueprints();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Logger.Add("App closing");
            try
            {
                MySettings.Serialize();
                if (UpdateAvailable.window != null && UpdateAvailable.window.IsLoaded)
                {
                    Logger.Add("Update windows showed");
                    e.Cancel = true;
                    Hide();
                    UpdateAvailable.window.Show();
                    UpdateAvailable.last_open = true;
                }
                else
                {
                    Logger.Add("App closed");
                    Application.Current.Shutdown();
                }
            }
            catch(Exception ee)
            {
                Logger.Add($"Error {ee.Message}");
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Logger.Add($"Main window size changed to {Width}x{Height}");
            //new Task(() =>
            //{
            //    Thread.Sleep(100);
            //    MyExtensions.AsyncWorker(() =>
            MinHeight = (ImageRow.ActualHeight + TextRow.ActualHeight + 40 + (ActualHeight - ControlsConteiner.ActualHeight));
            //});
        }

        private void Lock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Logger.Add("Lock pressed");
            switch (Lock.DataContext)
            {
                case 0:
                    if (MessageDialog.Last != null) MessageDialog.Last.Focus();
                    break;
                case 1:
                    break;
                default:
                    Window wind = (Window)Lock.DataContext;
                    if (wind != null) wind.Focus();
                    break;
            }
        }
        public void SetLock(bool lockly, object DataContext)
        {
            if (lockly)
            {
                Lock.Height = SystemParameters.PrimaryScreenHeight;
                Lock.DataContext = DataContext;
            }
            else
            {
                Lock.Height = 0;
                Lock.DataContext = null;
            }
        }
        bool BlueprintInitializing = false;
        private void InitBlueprints()
        {
            Logger.Add("Init Blueprints");
            if (BlueprintInitializing) return;
            BlueprintInitializing = true;
            Title = "SE BlueprintEditor Loading...";
            FoldersItem.Items.Clear();
            BlueList.Items.Clear();
            if (currentBluePatch != MySettings.Current.BlueprintPatch)
            {
                MenuItem Fldr = new MenuItem();
                Fldr.Header = Lang.GoBack;
                Fldr.Icon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resource/img_354138.png"))
                };
                Fldr.Click += MenuBackItem_Click;
                FoldersItem.Items.Add(Fldr);
                MenuItem Fldr2 = new MenuItem();
                Fldr2.Header = Lang.GoHome;
                Fldr2.Icon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resource/img_144440.png"))
                };
                Fldr2.Click += MenuHomeItem_Click;
                FoldersItem.Items.Add(Fldr2);
            }
            else FoldersItem.IsEnabled = false;
            BitmapImage fldicn = new BitmapImage(new Uri("pack://application:,,,/Resource/img_308586.png"));
            List<MenuItem> foldrmenu = new List<MenuItem>();
            string SerachQuery = Search.Text.ToLower();
            new Task(() =>
            {
                bool First = true;
                ParallelLoopResult status = Parallel.ForEach(Directory.GetDirectories(currentBluePatch), x =>//.OrderBy(x => Path.GetFileName(x))
                {

                    MyDisplayBlueprint Elem = MyDisplayBlueprint.fromBlueprint(x);
                    if (Elem is null)
                    {
                        MyExtensions.AsyncWorker(() =>
                        {
                            if (First && FoldersItem.IsEnabled)
                            {
                                FoldersItem.Items.Add(new Separator());
                            }
                            First = false;
                            FoldersItem.IsEnabled = true;
                            MenuItem Fldr = new MenuItem();
                            Fldr.Header = Path.GetFileNameWithoutExtension(x);
                            Fldr.Icon = new Image
                            {
                                Source = fldicn
                            };
                            Fldr.Click += MenuFolderItem_Click;
                            foldrmenu.Add(Fldr);
                            //FoldersItem.Items.Add(Fldr);
                        });
                        return;
                    }
                     MyExtensions.AsyncWorker(() =>
                     {
                         bool AddIt = true;
                         if (!string.IsNullOrEmpty(Search.Text))
                         {
                             switch (SearchBy.SelectedIndex)
                             {
                                 case 0:
                                     if (!Elem.Name.ToLower().Contains(SerachQuery)) AddIt = false;
                                     break;
                                 case 1:
                                     if (!Elem.Owner.ToLower().Contains(SerachQuery)) AddIt = false;
                                     break;
                                 case 2:
                                     if (!Elem.CreationTimeText.ToLower().Contains(SerachQuery)) AddIt = false;
                                     break;
                                 case 3:
                                     if (!Elem.CreationTimeText.ToLower().Contains(SerachQuery)) AddIt = false;
                                     break;
                             }
                         }
                         if (AddIt)
                             BlueList.Items.Add(Elem);
                     });
                });
                new Task(() =>
                {
                    while (!status.IsCompleted) { }
                    MyExtensions.AsyncWorker(() =>
                    {
                        foreach (MenuItem x in foldrmenu.OrderBy(x => x.Header))
                            FoldersItem.Items.Add(x);
                        if (OldSortBy != null)
                            GoSort(OldSortBy,null);
                        Title = "SE BlueprintEditor";
                        BlueprintInitializing = false;
                    });
                }).Start();
            }).Start();
        }
        GridViewColumn OldSortBy;
        private void GoSort(object sender, RoutedEventArgs e)
        {
            Logger.Add("Sotring algoritm start");
            GridViewColumn SortBy = sender as GridViewColumn;
            if(SortBy == null) SortBy = ((GridViewColumnHeader)sender).Column;
            string PropertyPatch = ((Binding)SortBy.DisplayMemberBinding).Path.Path.Replace("Text", "");
            if (OldSortBy != null)
                OldSortBy.Header = OldSortBy.Header.ToString().Trim('↓', '↑', ' ');
            ListSortDirection OldDirection = ListSortDirection.Ascending;
            if (BlueList.Items.SortDescriptions.Count > 0 && BlueList.Items.SortDescriptions[0].PropertyName == PropertyPatch)
                OldDirection = BlueList.Items.SortDescriptions[0].Direction;
            ListSortDirection NewDirection;
            if (e != null)
                NewDirection = OldDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else
                NewDirection = OldDirection;
            Logger.Add($"Sort Blueprints by {PropertyPatch} in {NewDirection}");
            BlueList.Items.SortDescriptions.Clear();
            BlueList.Items.SortDescriptions.Add(new SortDescription(PropertyPatch, NewDirection));
            //BlueList.Items.SortDescriptions.Add(new SortDescription("Name", NewDirection));
            SortBy.Header += NewDirection == ListSortDirection.Ascending ? " ↓" : " ↑";
            OldSortBy = SortBy;
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            int x = 0;
            int y = 1 / x;*/
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Logger.Add($"Blueprints search '{Search.Text}' in '{SearchBy.Text}'");
            InitBlueprints();
            /*
            int x = 0;
            int y = 13 / x;*/
        
        }
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (CurrentBlueprint != null)
            {
                Lock.Height = SystemParameters.PrimaryScreenHeight;
                Lock.DataContext = 0;
                Logger.Add("Show delete backups attention");
                new MessageDialog(DialogPicture.warn, Lang.UnsafeAction, Lang.ItWillDeleteThisBackp, (Dial) =>
                {
                    if (Dial == DialоgResult.Yes)
                    {
                        Logger.Add("Delete blueprint backups");
                        if (Directory.Exists(CurrentBlueprint?.Patch + "\\Backups"))
                            Directory.Delete(CurrentBlueprint.Patch + "\\Backups", true);
                        BlueList_SelectionChanged(null, null);
                    }
                    Lock.Height = 0;
                }).Show();
            }
            else
            {
                Logger.Add("Try to delete blueprint backups, but blueprint not selected");
                new MessageDialog(DialogPicture.attention, "Attention", Lang.SelectBlueForDelBack, null, DialogType.Message).Show();
            }
        }
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Logger.Add("Clear workshop cache");
            WorkshopCache.ClearMods();
        }
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            Logger.Add("Creating world with cached mods");
            if (string.IsNullOrWhiteSpace(MySettings.Current.SavesPatch))
            {
                Logger.Add("Saves patch not found");
                new MessageDialog(DialogPicture.attention, "Saves patch not found", Lang.SPNF, null, DialogType.Message).Show();
                return;
            }
            string mods = WorkshopCache.GetModsForWorld();
            new MessageDialog((x)=> {
                Logger.Add($"World [{x}] created");
                MyWorld.Create(x, mods);
            }, Lang.CreateWorld, Lang.EnterWorldNameForCreate).Show();
        }
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            Logger.Add("Move cached blueprints to local");
            WorkshopCache.MoveBlueprintsToLocal();
            InitBlueprints();
        }

        private void InDev(object sender, RoutedEventArgs e)
        {
            Logger.Add($"Show InDev message");
            new MessageDialog(DialogPicture.attention, "InDev", "This features in development, please wait for new version!", null, DialogType.Message).Show();
        }

        private void PrefabButton_Click(object sender, RoutedEventArgs e)
        {
            new MessageDialog(DialogPicture.attention, "Experemental", "This experemental function, your prefab in file PrefabTest.xml.", (x) => {
                File.WriteAllText("PrefabTest.xml", CurrentBlueprint.ConvertToPrefab());
            },DialogType.Message).Show();
            
        }
    }
}
