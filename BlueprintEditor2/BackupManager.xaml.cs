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
        static BackupManager LastWindow;
        private bool CloseIt = false;
        string Patch;
        public BackupManager(string _pathc)
        {
            Patch = _pathc+ "\\Backups";
            if (LastWindow != null)
            {
                LastWindow.Show();
                LastWindow.Focus();
                CloseIt = true;
            }
            else LastWindow = this;
            InitializeComponent();
            InfoLabel.Content = Lang.SelectOne;
            foreach (string file in Directory.GetFiles(Patch))
            {
                BackupList.Items.Add(file.Split('\\').Last());   
            }
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            if (CloseIt) Close();
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

        private void Window_Closed(object sender, EventArgs e)
        {
            LastWindow = null;
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
    }
}
