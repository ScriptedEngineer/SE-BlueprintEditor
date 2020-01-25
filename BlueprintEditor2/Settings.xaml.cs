﻿using BlueprintEditor2.Resource;
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
        bool hasChanged = false;
        public Settings()
        {
            int _indexLCID = Array.IndexOf(Langs, MySettings.Current.LCID);
            InitializeComponent();
            LastWindow = this;
            BlueprintFolderSetting.Text = MySettings.Current.BlueprintPatch;
            GameFolderSetting.Text = MySettings.Current.GamePatch;
            SavesFolderSetting.Text = MySettings.Current.SavesPatch;
            WorkshopFolderSetting.Text = MySettings.Current.SteamWorkshop;
            LangSelect.SelectedIndex = _indexLCID;
            MultiWindowCheckBox.IsChecked = MySettings.Current.MultiWindow;
            DOBSBox.IsChecked = MySettings.Current.DOBS;
            NickName.Text = MySettings.Current.UserName;
            hasChanged = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            LastWindow = null;
            Logger.Add("Settings closed");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (hasChanged)
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
                    }
                }).Show();
            }
            else
            {
                LastWindow = null;
            }
        }

        private void MultiWindowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            MySettings.Current.MultiWindow = MultiWindowCheckBox.IsChecked.Value;
            hasChanged = true;
        }

        private void LangSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LangSelect.SelectedIndex == -1) return;
            MySettings.Current.LCID = Langs[LangSelect.SelectedIndex];
            hasChanged = true;
        }

        private void BlueprintFolderSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(BlueprintFolderSetting.Text))
            {
                MySettings.Current.BlueprintPatch = BlueprintFolderSetting.Text;
                hasChanged = true;
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
                    hasChanged = true;
                }
            }
        }

        private void GameFolderSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(GameFolderSetting.Text))
            {
                MySettings.Current.GamePatch = GameFolderSetting.Text;
                hasChanged = true;
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
                    hasChanged = true;
                }
            }
        }

        private void DOBSBox_Click(object sender, RoutedEventArgs e)
        {
            MySettings.Current.DOBS = DOBSBox.IsChecked.Value;
            hasChanged = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MySettings.Current.UserName = NickName.Text;
            hasChanged = true;
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
                    hasChanged = true;
                }
            }
        }

        private void SavesFolder_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(WorkshopFolderSetting.Text))
            {
                MySettings.Current.SteamWorkshop = WorkshopFolderSetting.Text;
                hasChanged = true;
            }
        }

        private void SavesFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(SavesFolderSetting.Text))
            {
                MySettings.Current.SavesPatch = SavesFolderSetting.Text;
                hasChanged = true;
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
                    hasChanged = true;
                }
            }
        }
    }
}
