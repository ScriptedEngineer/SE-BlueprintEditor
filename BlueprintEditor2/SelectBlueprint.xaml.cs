using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using BlueprintEditor2.Resource;

namespace BlueprintEditor2
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class SelectBlueprint : Window
    {
        private static readonly string BlueprintsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpaceEngineers\Blueprints\local\";
        private MyXmlBlueprint _CurrentBlueprint;

        public SelectBlueprint()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru");
            InitializeComponent();
            foreach (string dir in Directory.GetDirectories(BlueprintsPath)) BlueList.Items.Add(dir.Split('\\').Last());
            BlueText.Text = Lang.SelectBlue;
        }

        private void BlueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _CurrentBlueprint = new MyXmlBlueprint(BlueprintsPath + BlueList.SelectedItem);
            BluePicture.Source = _CurrentBlueprint.GetPic();
            BlueText.Text = Lang.BlueName + ": " + _CurrentBlueprint.Name + "\n" +
                            Lang.GridCount + ": " + _CurrentBlueprint.Grids.Length + "\n" +
                            Lang.BlockCount + ": " + _CurrentBlueprint.BlockCount + "\n";
            EditButton.IsEnabled = true;
            BackupButton.IsEnabled = Directory.Exists(_CurrentBlueprint.Patch + "/Backups");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Left = SystemParameters.PrimaryScreenWidth / 2 - (360 + 800) / 2; //TODO: Темной магией увлекаетесь?
            Top = SystemParameters.PrimaryScreenHeight / 2 - Height / 2;
            if (!File.Exists(_CurrentBlueprint.Patch + "/~lock.dat"))
            {
                _CurrentBlueprint.SaveBackup();
                EditBlueprint Form = new EditBlueprint(File.Create(_CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose), _CurrentBlueprint, this);
                Form.Show();
                Form.Left = Left + 360;
                Form.Top = Top;
                Form.Height = Height;
            }
            else
            {
                MessageBox.Show(Lang.AlreadyOpened);
            }
        }

        private void PicMenuItemNormalize_Click(object sender, RoutedEventArgs e) => BluePicture.Source = _CurrentBlueprint.GetPic(true); //TODO: Исключение при попытке нормализовать стандартную синию картинку
        private void SelectorMenuItemFolder_Click(object sender, RoutedEventArgs e) => Process.Start(BlueprintsPath);
        private void SelectorMenuItemFolder2_Click(object sender, RoutedEventArgs e) => Process.Start(_CurrentBlueprint.Patch);
        private void WindowsMenuItemAbout_Click(object sender, RoutedEventArgs e) => new About().Show();
        private void Window_Closing(object sender, CancelEventArgs e) => Application.Current.Shutdown();
        private void BackupButton_Click(object sender, RoutedEventArgs e) => new BackupManager(_CurrentBlueprint.Patch).Show();
    }
}
