using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BlueprintEditor2.Resource;

namespace BlueprintEditor2
{
    /// <summary>
    ///     Логика взаимодействия для BackupManager.xaml
    /// </summary>
    public partial class BackupManager : Window
    {
        private readonly string _Path;

        public BackupManager(string pathToBlueprint)
        {
            _Path = pathToBlueprint + "\\Backups";
            InitializeComponent();
            InfoLabel.Content = Lang.SelectOne;
            foreach (string file in Directory.GetFiles(_Path)) BackupList.Items.Add(file.Split('\\').Last());
        }

        private void BackupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackupList.SelectedIndex < 0)
            {
                InfoLabel.Content = Lang.SelectOne;
                DeleteButton.IsEnabled = RestoreButton.IsEnabled = ReplaceButton.IsEnabled = false;
            }
            else
            {
                InfoLabel.Content = Lang.Created + ": " + File.GetCreationTime(_Path + '\\' + BackupList.SelectedItem);
                DeleteButton.IsEnabled = !BackupList.SelectedItem.ToString().Contains("Lastest-");
                RestoreButton.IsEnabled = ReplaceButton.IsEnabled = true;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (BackupList.SelectedIndex < 0) return;
            File.Delete(_Path + "\\" + BackupList.SelectedItem);
            BackupList.Items.RemoveAt(BackupList.SelectedIndex);
        }
    }
}
