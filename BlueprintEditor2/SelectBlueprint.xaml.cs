using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Globalization;
using System.Threading;
using BlueprintEditor2.Resource;
using System.Diagnostics;
using Path = System.IO.Path;

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
                    MyExtensions.AsyncWorker(()=> new UpdateAvailable(Vers[2], Vers[1]).Show());
                }
            }).Start();
            
        }
        internal void BlueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyDisplayBlueprint Selected = (MyDisplayBlueprint)BlueList.SelectedItem;
            if (Selected != null)
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
                BluePicture.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/thumbDefault.png"));
                BlueText.Text = Lang.SelectBlue;
                CalculateButton.IsEnabled = false;
                EditButton.IsEnabled = false;
                BackupButton.IsEnabled = false;
            }
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(Lang.ComingSoon); return;
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
            else MessageBox.Show(Lang.AlreadyOpened);
        }
        private void PicMenuItemNormalize_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBlueprint != null) BluePicture.Source = CurrentBlueprint.GetPic(true);
        }
        private void SelectorMenuItemFolder_Click(object sender, RoutedEventArgs e) => Process.Start(MySettings.Current.BlueprintPatch);
        private void SelectorMenuItemFolder2_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBlueprint != null) Process.Start(CurrentBlueprint.Patch);
            else MessageBox.Show(Lang.SelectBlueForOpen);
        }
        private void WindowsMenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            if (About.LastWindow == null) new About().Show();
            else About.LastWindow.Focus();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(CurrentBlueprint.Patch + "/~lock.dat"))
            {
                new BackupManager(File.Create(CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose), CurrentBlueprint).Show();
            }
            else MessageBox.Show(Lang.AlreadyOpened);

        }
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Lang.ComingSoon); return;
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
            else MessageBox.Show(Lang.AlreadyOpened);*/
        }
        private void BackupsMenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            Lock.Height = SystemParameters.PrimaryScreenHeight;
            Lock.DataContext = 0;
            new Dialog(DialogPicture.warn, Lang.UnsafeAction, Lang.ItWillDeleteAllBackps, (Dial) =>
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
        private void Lock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (Lock.DataContext)
            {
                case 0:
                    if (Dialog.Last != null) Dialog.Last.Focus();
                    break;
                default:
                    Window wind = (Window)Lock.DataContext;
                    if(wind != null) wind.Focus();
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
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.LastWindow == null) new Settings().Show();
            else Settings.LastWindow.Focus();
        }

        private void MenuFolderItem_Click(object sender, RoutedEventArgs e)
        {
            currentBluePatch += ((MenuItem)sender).Header + "\\";
            InitBlueprints();
        }
        private void MenuBackItem_Click(object sender, RoutedEventArgs e)
        {
            
            currentBluePatch = Path.GetDirectoryName(currentBluePatch.TrimEnd('\\')) + "\\";
            InitBlueprints();
        }
        void InitBlueprints()
        {  
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
                FoldersItem.Items.Add(new Separator());
            }else FoldersItem.IsEnabled = false;
            object sync = new object();
            BitmapImage fldicn = new BitmapImage(new Uri("pack://application:,,,/Resource/img_308586.png"));
            Parallel.ForEach(Directory.GetDirectories(currentBluePatch), x =>
            {

                MyDisplayBlueprint Elem = MyDisplayBlueprint.fromBlueprint(x);
                if (Elem is null)
                {
                    lock (sync)
                    {
                        MyExtensions.AsyncWorker(() =>
                        {
                            FoldersItem.IsEnabled = true;
                            MenuItem Fldr = new MenuItem();
                            Fldr.Header = Path.GetFileNameWithoutExtension(x);
                            Fldr.Icon = new Image
                            {
                                Source = fldicn
                            };
                            Fldr.Click += MenuFolderItem_Click;
                            FoldersItem.Items.Add(Fldr);
                        });
                    }
                    return;
                }
                lock (sync)
                {
                    MyExtensions.AsyncWorker(() => BlueList.Items.Add(Elem));
                }
            });
        }


    }
}
