﻿using System;
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
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            InitializeComponent();
            window = this;
            foreach (string dir in Directory.GetDirectories(BluePatch)) {
                BlueList.Items.Add(dir.Split('\\').Last());
            }
            BlueText.Text = Lang.SelectBlue;
        }
        private void BlueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentBlueprint = new MyXmlBlueprint(BluePatch + BlueList.SelectedItem);
            BluePicture.Source = CurrentBlueprint.GetPic();
            BlueText.Text = Lang.BlueName + ": " + CurrentBlueprint.Name + "\n" +
                Lang.GridCount + ": " + CurrentBlueprint.Grids.Length + "\n" +
                Lang.BlockCount + ": " + CurrentBlueprint.BlockCount + "\n" +
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
                Left = SystemParameters.PrimaryScreenWidth / 2 - ((360 + 800) / 2);
                Top = SystemParameters.PrimaryScreenHeight / 2 - (Height / 2);
                CurrentBlueprint.SaveBackup();
                EditBlueprint Form = new EditBlueprint(File.Create(CurrentBlueprint.Patch + "/~lock.dat", 256, FileOptions.DeleteOnClose),CurrentBlueprint);
                Form.Show();
                Form.Left = Left+360;
                Form.Top = Top;
                Form.Height = Height;
            }
            else MessageBox.Show(Lang.AlreadyOpened);
        }
        private void PicMenuItemNormalize_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBlueprint != null) BluePicture.Source = CurrentBlueprint.GetPic(true);
        }
        private void SelectorMenuItemFolder_Click(object sender, RoutedEventArgs e) => Process.Start(BluePatch);
        private void SelectorMenuItemFolder2_Click(object sender, RoutedEventArgs e) => Process.Start(CurrentBlueprint.Patch);
        private void WindowsMenuItemAbout_Click(object sender, RoutedEventArgs e) => new About().Show();
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Application.Current.Shutdown();
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
    }
}
