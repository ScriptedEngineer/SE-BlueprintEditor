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
            if (File.Exists("update.vbs")) File.Delete("update.vbs");
            MySettings.Deserialize();
            MySettings.Current.ApplySettings();
            InitializeComponent();
            window = this;
            currentBluePatch = MySettings.Current.BlueprintPatch;
            InitBlueprints();
            new Task(() =>
            {
                string[] Vers = MyExtensions.ApiServer(ApiServerAct.CheckVersion).Split(' ');
                if (Vers.Length == 3 && Vers[0] == "0")
                {
                    MyExtensions.AsyncWorker(() => new UpdateAvailable(Vers[2], Vers[1]).Show());
                }
                //MyExtensions.AsyncWorker(() => new Dialog(x => Console.WriteLine(x), DialogPicture.attention, "TEST", "PleaseInput").Show());
            }).Start();
        }
        internal void BlueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyDisplayBlueprint Selected = (MyDisplayBlueprint)BlueList.SelectedItem;
            if (Selected != null && File.Exists(currentBluePatch + Selected.Name + "\\bp.sbc"))
            {
                CurrentBlueprint = new MyXmlBlueprint(currentBluePatch + Selected.Name);
                BluePicture.Source = CurrentBlueprint.GetPic();
                BlueText.Text = Lang.Blueprint + ": " + Selected.Name + "\n" +
                    Lang.Name + ": " + CurrentBlueprint.Name + "\n" +
                    Lang.Created + ": " + Selected.CreationTimeText + "\n" +
                    Lang.Changed + ": " + Selected.LastEditTimeText + "\n" +
                    Lang.GridCount + ": " + Selected.GridCountText + "\n" +
                    Lang.BlockCount + ": " + Selected.BlockCountText + "\n" +
                    Lang.Owner + ": " + Selected.Owner + "(" + CurrentBlueprint.Owner + ")\n";
                CalculateButton.IsEnabled = true;
                EditButton.IsEnabled = true;
                BackupButton.IsEnabled = Directory.Exists(CurrentBlueprint.Patch + "/Backups");
                foreach (string file in Directory.GetFiles(CurrentBlueprint.Patch, "bp.sbc*", SearchOption.TopDirectoryOnly))
                {
                    if (Path.GetFileName(file) != "bp.sbc") File.Delete(file);
                }
            }
            else
            {
                if (BlueList.SelectedIndex != -1) BlueList.Items.Remove(BlueList.SelectedItem);
                CurrentBlueprint = null;
                BluePicture.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/blueprints-textures_00394054.jpg"));
                BlueText.Text = Lang.SelectBlue;
                CalculateButton.IsEnabled = false;
                EditButton.IsEnabled = false;
                BackupButton.IsEnabled = false;
            }
            Height++; Height--;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(CurrentBlueprint.Patch + "/~lock.dat"))
            {
                Left = 0;
                Top = SystemParameters.PrimaryScreenHeight / 2 - (Height / 2);
                CurrentBlueprint.SaveBackup();
                EditBlueprint Form = new EditBlueprint(File.Create(CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose), CurrentBlueprint);
                Form.Show();
                if (!MySettings.Current.MultiWindow) Hide();
                BackupButton.IsEnabled = true;
            }
            else new MesassageDialog(DialogPicture.warn, "Error", Lang.AlreadyOpened, null, DialogType.Message).Show();
        }
        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(CurrentBlueprint.Patch + "/~lock.dat"))
            {
                new BackupManager(File.Create(CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose), CurrentBlueprint).Show();
            }
            else new MesassageDialog(DialogPicture.warn, "Error", Lang.AlreadyOpened, null, DialogType.Message).Show();

        }
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            new MesassageDialog(DialogPicture.attention, "Attention", Lang.ComingSoon, null, DialogType.Message).Show(); return;
            /*if (!File.Exists(CurrentBlueprint.Patch + "/~lock.dat"))
            {
                Left = SystemParameters.PrimaryScreenWidth / 2 - ((360 + 800) / 2);
                Top = SystemParameters.PrimaryScreenHeight / 2 - (Height / 2);
                Calculator Form = new Calculator(File.Create(CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose), CurrentBlueprint);
                Form.Show();
                Form.Left = Left + 360;
                Form.Top = Top;
                Form.Height = Height;
            }
            else new Dialog(DialogPicture.warn, "Error", Lang.AlreadyOpened, null, DialogType.Message).Show();
        */
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            InitBlueprints();
        }
        private void PicMenuItemNormalize_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBlueprint != null) BluePicture.Source = CurrentBlueprint.GetPic(true);
        }
        private void SelectorMenuItemFolder_Click(object sender, RoutedEventArgs e) => 
            Process.Start(MySettings.Current.BlueprintPatch);
        private void SelectorMenuItemFolder2_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBlueprint != null) Process.Start(CurrentBlueprint.Patch);
            else new MesassageDialog(DialogPicture.attention, "Attention", Lang.SelectBlueForOpen, null, DialogType.Message).Show();
        }
        private void BackupsMenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            Lock.Height = SystemParameters.PrimaryScreenHeight;
            Lock.DataContext = 0;
            new MesassageDialog(DialogPicture.warn, Lang.UnsafeAction, Lang.ItWillDeleteAllBackps, (Dial) =>
            {
                if (Dial == DialоgResult.Yes)
                {
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
            if (About.LastWindow == null) new About().Show();
            else About.LastWindow.Focus();
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.LastWindow == null) new Settings().Show();
            else Settings.LastWindow.Focus();
        }
        private void MenuFolderItem_Click(object sender, RoutedEventArgs e)
        {
            string newDir = currentBluePatch + ((MenuItem)sender).Header + "\\";
            if (Directory.Exists(newDir))
            {
                currentBluePatch = newDir;
                InitBlueprints();
            }
            else
            {
                ((MenuItem)sender).Visibility = Visibility.Collapsed;
            }
        }
        private void MenuBackItem_Click(object sender, RoutedEventArgs e)
        {
            currentBluePatch = Path.GetDirectoryName(currentBluePatch.TrimEnd('\\')) + "\\";
            InitBlueprints();
        }
        private void MenuHomeItem_Click(object sender, RoutedEventArgs e)
        {
            currentBluePatch = MySettings.Current.BlueprintPatch;
            InitBlueprints();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                MySettings.Serialize();
                if (UpdateAvailable.window != null && UpdateAvailable.window.IsLoaded)
                {
                    e.Cancel = true;
                    Hide();
                    UpdateAvailable.window.Show();
                    UpdateAvailable.last_open = true;
                }
                else Application.Current.Shutdown();
            }
            catch
            {

            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //new Task(() =>
            //{
            //    Thread.Sleep(100);
            //    MyExtensions.AsyncWorker(() =>
            MinHeight = (ImageRow.ActualHeight + TextRow.ActualHeight + 40 + (ActualHeight - ControlsConteiner.ActualHeight));
            //});
        }

        private void Lock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (Lock.DataContext)
            {
                case 0:
                    if (MesassageDialog.Last != null) MesassageDialog.Last.Focus();
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

        private void InitBlueprints()
        {
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
                    MyExtensions.AsyncWorker(() => BlueList.Items.Add(Elem));
                });
                new Task(() =>
                {
                    while (!status.IsCompleted) { }
                    MyExtensions.AsyncWorker(() =>
                    {
                        foreach (var x in foldrmenu.OrderBy(x => x.Header))
                            FoldersItem.Items.Add(x);
                        if (OldSortBy != null)
                            GoSort(OldSortBy,null);
                        Title = "SE BlueprintEditor";
                    });
                }).Start();
            }).Start();
        }
        GridViewColumnHeader OldSortBy;
        private void GoSort(object sender, RoutedEventArgs e)
        {
            
            GridViewColumnHeader SortBy = (GridViewColumnHeader)sender;
            if(OldSortBy != null)
                OldSortBy.Content = OldSortBy.Content.ToString().Trim('↓', '↑', ' ');
            string PropertyPatch = ((Binding)SortBy.Column.DisplayMemberBinding).Path.Path.Replace("Text","");
            ListSortDirection OldDirection = ListSortDirection.Descending;
            if (BlueList.Items.SortDescriptions.Count > 0 && BlueList.Items.SortDescriptions[0].PropertyName == PropertyPatch)
                OldDirection = BlueList.Items.SortDescriptions[0].Direction;
            ListSortDirection NewDirection;
            if (e != null)
                NewDirection = OldDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else
                NewDirection = OldDirection;
            BlueList.Items.SortDescriptions.Clear();
            BlueList.Items.SortDescriptions.Add(new SortDescription(PropertyPatch, NewDirection));
            //BlueList.Items.SortDescriptions.Add(new SortDescription("Name", NewDirection));
            SortBy.Content += NewDirection == ListSortDirection.Ascending ? " ↓" : " ↑";
            OldSortBy = SortBy;
        }
    }
}
