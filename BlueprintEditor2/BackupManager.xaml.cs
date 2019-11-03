using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using BlueprintEditor2.Resource;

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для BackupManager.xaml
    /// </summary>
    public partial class BackupManager : Window
    {
        string Patch;
        FileStream _lock;
        MyXmlBlueprint Blueprint;
        public BackupManager(FileStream Lock, MyXmlBlueprint _blueprint)
        {
            if (!MySettings.Current.MultiWindow) SelectBlueprint.window.SetLock(true, this);
            _lock = Lock;
            Blueprint = _blueprint;
            Patch = Blueprint.Patch + "\\Backups";
            InitializeComponent();
            InfoLabel.Content = Lang.SelectOne;
            foreach (string file in Directory.GetFiles(Patch))
            {
                BackupList.Items.Add(file.Split('\\').Last());   
            }
            Title = "[" + Blueprint.Patch.Split('\\').Last() + "] BackupManager - SE BlueprintEditor";
        }

        private void BackupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackupList.SelectedIndex != -1)
            {
                InfoLabel.Content = Lang.Created + ": " + DateTime.FromFileTimeUtc(long.Parse(BackupList.SelectedItem.ToString().Split('-').Last().Replace(".sbc", ""))).ToString();
                if (BackupList.SelectedItem.ToString().Contains("Lastest-")) DeleteButton.IsEnabled = false;
                else DeleteButton.IsEnabled = true;
                RestoreButton.IsEnabled = true;
                ReplaceButton.IsEnabled = true;
            }
            else
            {
                InfoLabel.Content = Lang.SelectOne;
                DeleteButton.IsEnabled = false;
                RestoreButton.IsEnabled = false;
                ReplaceButton.IsEnabled = false;
            }
        }
        private void Window_Closing(object sender, EventArgs e)
        {
            if (!MySettings.Current.MultiWindow) SelectBlueprint.window.SetLock(false, null);
            _lock.Dispose();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(Patch + "\\" + BackupList.SelectedItem.ToString());
            BackupList.Items.Clear();
            foreach (string file in Directory.GetFiles(Patch))
            {
                BackupList.Items.Add(file.Split('\\').Last());
            }
        }
        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(Patch + "\\" + BackupList.SelectedItem.ToString());
            Blueprint.SaveBackup();
            BackupList.Items.Clear();
            foreach (string file in Directory.GetFiles(Patch))
            {
                BackupList.Items.Add(file.Split('\\').Last());
            }
        }
        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            SelectBlueprint.window.SetLock(true, 0);
            new MessageDialog(DialogPicture.attention, Lang.UnsafeAction, Lang.ItWillDelete,(Dial) => 
            {
                if (Dial == DialоgResult.Yes)
                {
                    File.Delete(Blueprint.Patch + "\\bp.sbc");
                    File.Copy(Patch + "\\" + BackupList.SelectedItem.ToString(), Blueprint.Patch + "\\bp.sbc");
                    Close();
                    SelectBlueprint.window.SetLock(false, null);
                }
                else
                {
                    Show();
                    SelectBlueprint.window.SetLock(true, this);
                }
            }).Show();
        }
    }
}
