using BlueprintEditor2.Resource;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        static internal Settings LastWindow;
        readonly int[] Langs = new int[] {9, 1049};
        bool RestartApp = false;
        public Settings()
        {
            int _indexLCID = Array.IndexOf(Langs, MySettings.Current.LangCultureID);
            InitializeComponent();
            LastWindow = this;
            BlueprintFolderSetting.Text = MySettings.Current.BlueprintPatch;
            GameFolderSetting.Text = MySettings.Current.GamePatch;
            SavesFolderSetting.Text = MySettings.Current.SavesPatch;
            WorkshopFolderSetting.Text = MySettings.Current.SteamWorkshopPatch;
            ScriptsFolderSetting.Text = MySettings.Current.ScriptsPatch;
            ModsFolderSetting.Text = MySettings.Current.ModsPatch;
            LangSelect.SelectedIndex = _indexLCID;
            MultiWindowCheckBox.IsChecked = MySettings.Current.MultiWindow;
            DOBSBox.IsChecked = MySettings.Current.DontOpenBlueprintsOnScan;
            NickName.Text = MySettings.Current.UserName;
            NickName.IsEnabled = !MySettings.Current.SyncNickName;
            SaveBackups.IsChecked = MySettings.Current.SaveBackups;
            SteamSync.IsChecked = MySettings.Current.SyncNickName;
            Themes.SelectedIndex = (int)MySettings.Current.Theme;
            RestartApp = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            LastWindow = null;
            Logger.Add("Settings closed");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (RestartApp)
            {
                Logger.Add("Show restart dialog");
                MySettings.Serialize();
                SelectBlueprint.window.SetLock(true, 0);
                new MessageDialog(DialogPicture.question, Lang.Settings, Lang.PleaseRestartApp, (Dial) =>
                {
                    if (Dial == DialоgResult.Yes)
                    {
                        Process.Start(MyExtensions.AppFile);
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        SelectBlueprint.window.SetLock(false, null);
                        LastWindow = null;
                        MySettings.Current.ApplySettings();
                    }
                }).Show();
            }
            else
            {
                LastWindow = null;
            }
            SelectBlueprint.window.SettingsUpdated();
        }

        private void MultiWindowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            MySettings.Current.MultiWindow = MultiWindowCheckBox.IsChecked.Value;
            //RestartApp = true;
        }

        private void LangSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LangSelect.SelectedIndex == -1) return;
            MySettings.Current.LangCultureID = Langs[LangSelect.SelectedIndex];
            RestartApp = true;
        }

        private void BlueprintFolderSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(BlueprintFolderSetting.Text))
            {
                MySettings.Current.BlueprintPatch = BlueprintFolderSetting.Text;
                RestartApp = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                
                dialog.SelectedPath = BlueprintFolderSetting.Text;
                dialog.Description = Lang.SelectBluePatchDesc;
                dialog.ShowNewFolderButton = false;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.Equals(System.Windows.Forms.DialogResult.OK))
                {
                    BlueprintFolderSetting.Text = dialog.SelectedPath + "\\";
                    RestartApp = true;
                }
            }
        }

        private void GameFolderSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(GameFolderSetting.Text))
            {
                MySettings.Current.GamePatch = GameFolderSetting.Text;
                RestartApp = true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {

                dialog.SelectedPath = GameFolderSetting.Text;
                dialog.Description = Lang.SelectGamePatchDesc;
                dialog.ShowNewFolderButton = false;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.Equals(System.Windows.Forms.DialogResult.OK))
                {
                    GameFolderSetting.Text = dialog.SelectedPath + "\\";
                    RestartApp = true;
                }
            }
        }

        private void DOBSBox_Click(object sender, RoutedEventArgs e)
        {
            MySettings.Current.DontOpenBlueprintsOnScan = DOBSBox.IsChecked.Value;
            //hasChanged = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MySettings.Current.UserName = NickName.Text;
            //hasChanged = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {

                dialog.SelectedPath = SavesFolderSetting.Text;
                dialog.Description = Lang.SelectSavesPatchDesc;
                dialog.ShowNewFolderButton = false;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.Equals(System.Windows.Forms.DialogResult.OK))
                {
                    SavesFolderSetting.Text = dialog.SelectedPath + "\\";
                    RestartApp = true;
                }
            }
        }

        private void SavesFolder_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(WorkshopFolderSetting.Text))
            {
                MySettings.Current.SteamWorkshopPatch = WorkshopFolderSetting.Text;
                RestartApp = true;
            }
        }

        private void SavesFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(SavesFolderSetting.Text))
            {
                MySettings.Current.SavesPatch = SavesFolderSetting.Text;
                RestartApp = true;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = WorkshopFolderSetting.Text;
                dialog.Description = Lang.OnlyPro;
                dialog.ShowNewFolderButton = false;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.Equals(System.Windows.Forms.DialogResult.OK))
                {
                    WorkshopFolderSetting.Text = dialog.SelectedPath + "\\";
                    RestartApp = true;
                }
            }
        }

        private void SaveBackups_Click(object sender, RoutedEventArgs e)
        {
            MySettings.Current.SaveBackups = SaveBackups.IsChecked.Value;
            //hasChanged = true;
        }

        private void SteamSync_Click(object sender, RoutedEventArgs e)
        {
            MySettings.Current.SyncNickName = SteamSync.IsChecked.Value;
            NickName.IsEnabled = !MySettings.Current.SyncNickName;
            if (MySettings.Current.SyncNickName)
            {
                MySettings.Current.UserName = MyExtensions.GetSteamLastGameNameUsed();
                NickName.Text = MySettings.Current.UserName;
            }
        }

        private void Themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Themes.SelectedIndex == -1) return;
            MySettings.Current.Theme = (MyThemeEnum)Themes.SelectedIndex;
            MyExtensions.ThemeChange(MySettings.Current.Theme.ToString());
        }

        private void ScriptsFolderSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(WorkshopFolderSetting.Text))
            {
                MySettings.Current.ScriptsPatch = ScriptsFolderSetting.Text;
                RestartApp = true;
            }
        }

        private void ModsFolderSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(WorkshopFolderSetting.Text))
            {
                MySettings.Current.ModsPatch = ModsFolderSetting.Text;
                RestartApp = true;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = ScriptsFolderSetting.Text;
                dialog.Description = Lang.OnlyPro;
                dialog.ShowNewFolderButton = false;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.Equals(System.Windows.Forms.DialogResult.OK))
                {
                    ScriptsFolderSetting.Text = dialog.SelectedPath + "\\";
                    RestartApp = true;
                }
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = ModsFolderSetting.Text;
                dialog.Description = Lang.OnlyPro;
                dialog.ShowNewFolderButton = false;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result.Equals(System.Windows.Forms.DialogResult.OK))
                {
                    ModsFolderSetting.Text = dialog.SelectedPath + "\\";
                    RestartApp = true;
                }
            }
        }
    }
}
