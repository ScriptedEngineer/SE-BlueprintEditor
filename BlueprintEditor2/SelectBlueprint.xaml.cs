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
        static public SelectBlueprint window;
        readonly string BluePatch = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpaceEngineers\Blueprints\local\";
        internal MyXmlBlueprint CurrentBlueprint;
        public SelectBlueprint()
        {
            if(File.Exists("update.vbs"))File.Delete("update.vbs");
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr");
            InitializeComponent();
            window = this;
            foreach (string dir in Directory.GetDirectories(BluePatch)) {
                BlueList.Items.Add(MyListElement.fromBlueprint(dir));
            }
            BlueText.Text = Lang.SelectBlue;
            MyExtensions.AsyncWorker(() =>
            {
                string[] Vers = MyExtensions.ApiServer(ApiServerAct.CheckVersion).Split(' ');
                if (Vers.Length == 3 && Vers[0] == "0")//0 xyzs.ru 0.000.1.028
                {
                    new UpdateAvailable(Vers[2], Vers[1]).Show();
                }
            });
        }
        internal void BlueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyListElement Selected = (MyListElement)BlueList.SelectedItem;
            CurrentBlueprint = new MyXmlBlueprint(BluePatch + Selected.Elements[0]);
            BluePicture.Source = CurrentBlueprint.GetPic();
            BlueText.Text = Lang.Blueprint + ": " + Selected.Elements[0] + "\n" +
                Lang.Name + ": " + CurrentBlueprint.Name + "\n" +
                Lang.Created + ": " + Selected.Elements[2] + "\n" +
                Lang.Changed + ": " + Selected.Elements[3] + "\n" +
                Lang.GridCount + ": " + CurrentBlueprint.Grids.Length + "\n" +
                Lang.BlockCount + ": " + Selected.Elements[4] + "\n" +
                Lang.Owner + ": " + CurrentBlueprint.DisplayName + "(" + CurrentBlueprint.Owner + ")\n";
            CalculateButton.IsEnabled = true;
            EditButton.IsEnabled = true;
            BackupButton.IsEnabled = Directory.Exists(CurrentBlueprint.Patch + "/Backups");
            foreach (string file in Directory.GetFiles(CurrentBlueprint.Patch, "bp.sbc*",SearchOption.TopDirectoryOnly))
            {
                if(Path.GetFileName(file) != "bp.sbc") File.Delete(file);
            }
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(Lang.ComingSoon); return;
            if (!File.Exists(CurrentBlueprint.Patch+"/~lock.dat"))
            {
                Left = 0;
                Top = SystemParameters.PrimaryScreenHeight / 2 - (Height / 2);
                CurrentBlueprint.SaveBackup();
                EditBlueprint Form = new EditBlueprint(File.Create(CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose),CurrentBlueprint);
                Form.Show();
                BackupButton.IsEnabled = true;
            }
            else MessageBox.Show(Lang.AlreadyOpened);
        }
        private void PicMenuItemNormalize_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBlueprint != null) BluePicture.Source = CurrentBlueprint.GetPic(true);
        }
        private void SelectorMenuItemFolder_Click(object sender, RoutedEventArgs e) => Process.Start(BluePatch);
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
                new BackupManager(File.Create(CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose),CurrentBlueprint).Show();
            }
            else MessageBox.Show(Lang.AlreadyOpened);
            
        }
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Lang.ComingSoon);return;
            if (!File.Exists(CurrentBlueprint.Patch + "/~lock.dat"))
            {
                Left = SystemParameters.PrimaryScreenWidth / 2 - ((360 + 800) / 2);
                Top = SystemParameters.PrimaryScreenHeight / 2 - (Height / 2);
                Calculator Form = new Calculator(File.Create(CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose), CurrentBlueprint);
                Form.Show();
                Form.Left = Left + 360;
                Form.Top = Top;
                Form.Height = Height;
            }
            else MessageBox.Show(Lang.AlreadyOpened);
        }
        private void BackupsMenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            Lock.Height = SystemParameters.PrimaryScreenHeight;
            new Dialog(DialogPicture.warn, Lang.UnsafeAction, Lang.ItWillDeleteAllBackps, (Dial) =>
            {
                if (Dial == DialоgResult.Yes)
                {
                    foreach (string dir in Directory.GetDirectories(BluePatch))
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
            if(Dialog.Last != null) Dialog.Last.Focus();
        }
    }
}
