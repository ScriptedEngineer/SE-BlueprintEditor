using BlueprintEditor2.Resource;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для ReplacersEditor.xaml
    /// </summary>
    public partial class ReplacersEditor : Window
    {
        public ReplacersEditor()
        {
            InitializeComponent();
            foreach (var x in ArmorReplaceClass.Replacers)
            {
                if(x.Key != "Heavy")
                    TypesList.Items.Add(x.Key);
            }
        }
        public class KeyValue
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public KeyValue(KeyValuePair<string, string> KV)
            {
                Key = KV.Key;
                Value = KV.Value;
            }
            public KeyValue(string K, string V)
            {
                Key = K;
                Value = V;
            }
        }
        private void TypesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProgEditing = true;
            ReplaceList.Items.Clear();
            if (TypesList.SelectedIndex <= 0)
            {
                if (TypesList.SelectedIndex == 0)
                {
                    BaseType.Width = 0;
                    ChangeTo.Width = 400;
                    ChangeTo.Header = Lang.Type;
                    foreach (var x in ArmorReplaceClass.Baze)
                    {
                        ReplaceList.Items.Add(new KeyValue("", x));
                    }
                }
                else
                {
                    BaseType.Width = 200;
                    ChangeTo.Width = 200;
                    ChangeTo.Header = Lang.ChangeTo;
                }
                ReplaceList.IsEnabled = false;
                DeleteButton.IsEnabled = false;
            }
            else
            {
                BaseType.Width = 200;
                ChangeTo.Width = 200;
                ChangeTo.Header = Lang.ChangeTo;
                
                string repname = TypesList.SelectedItem.ToString();
                if (TypesList.SelectedItem is ListBoxItem)
                {
                    repname = (TypesList.SelectedItem as ListBoxItem).Uid;
                }
                DeleteButton.IsEnabled = ReplaceList.IsEnabled = repname != "Heavy";
                foreach (var x in ArmorReplaceClass.Replacers[repname].Replace)
                {
                    ReplaceList.Items.Add(new KeyValue(x));
                }
            }
            ProgEditing = false;
        }

        private void InventoryItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            //ProgEditing = true;
            ComboBox Sender = (ComboBox)sender;
            if (Sender.Text != "" && e.Changes.Count == 1 && e.Changes.FirstOrDefault().AddedLength == 1)
            {
                var tb = (TextBox)e.OriginalSource;
                Sender.Items.Clear();
                foreach (var s in MyGameData.BlockTypes)
                {
                    if (s.IndexOf(Sender.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        Sender.Items.Add(s);
                    if (Sender.Items.Count >= 30)
                        break;
                }
                if (Sender.Items.Count == 0 || (Sender.Items.Count == 1 && !Sender.IsDropDownOpen))
                    Sender.IsDropDownOpen = false;
                else
                {
                    Sender.IsDropDownOpen = true;
                    Sender.MaxDropDownHeight = 120;
                    tb.SelectionStart = Sender.Text.Length;
                }
            }
            else
            {
                Sender.Items.Clear();
                Sender.IsDropDownOpen = false;
            }
            if (TypesList.SelectedIndex == 0)
            {
                
            }
            else
            {
                string repname = TypesList.SelectedItem.ToString();
                if (TypesList.SelectedItem is ListBoxItem)
                {
                    repname = (TypesList.SelectedItem as ListBoxItem).Uid;
                }
                if (repname != "Heavy")
                {
                    ArmorReplaceClass.Replacers[repname].Replace.Clear();
                    foreach (KeyValue x in ReplaceList.Items)
                    {
                        ArmorReplaceClass.Replacers[repname].Replace.Add(x.Key,x.Value);
                    }
                }
            }
            //ProgEditing = false;
        }
        bool ProgEditing = false;
        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox Sender = (ComboBox)sender;
            if (ProgEditing || Sender.Text == "" || Sender.Items.Count == 0)
            {
                Sender.IsDropDownOpen = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new MessageDialog((x) => {
                if (ArmorReplaceClass.AddEmptyReplacer(x))
                    TypesList.Items.Add(x);
                else
                    new MessageDialog(DialogPicture.warn, Lang.Type, Lang.TypeAlreadyExists, null, DialogType.Message).Show();
                Logger.Add($"Replacer [{x}] created");
            }, Lang.AddItem, Lang.EnterTypeName).Show();
            
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TypesList.SelectedIndex != -1)
            {
                if(ArmorReplaceClass.Replacers.Remove(TypesList.SelectedItem.ToString()))
                    TypesList.Items.Remove(TypesList.SelectedItem);
            }
        }
    }
}
