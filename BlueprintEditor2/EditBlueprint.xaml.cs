using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using System.ComponentModel;
using System.Windows.Media;
using Forms = System.Windows.Forms;
using BlueprintEditor2.Resource;
using System.Collections.Generic;

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для EditBlueprint.xaml
    /// </summary>
    public partial class EditBlueprint : Window
    {
        static public int OpenCount;
        FileStream _Lock;
        MyXmlBlueprint EdBlueprint;
        public EditBlueprint(FileStream Lock, MyXmlBlueprint Blueprint)
        {
            _Lock = Lock;
            EdBlueprint = Blueprint;
            InitializeComponent();
            Title = "["+EdBlueprint.Patch.Split('\\').Last()+"] Editor - SE BlueprintEditor";
            BluePicture.Source = EdBlueprint.GetPic(false, false);
            for(int i = 0;i < EdBlueprint.Grids.Length;i++)
                GridList.Items.Add(EdBlueprint.Grids[i].Name);
            if(GridList.Items.Count > 0) GridList.SelectedIndex = 0;
            OpenCount++;
        }

        private void GridList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BlockList.Items.Clear();
            HideArmor.IsEnabled = true;
            GridArmourType.IsEnabled = true;
            initGAT = false;
            if (GridList.SelectedIndex == -1) return;
            MyXmlGrid SlectedGrd = EdBlueprint.Grids[GridList.SelectedIndex];
            List<MyXmlBlock> TheBlocks = SlectedGrd.Blocks;
            int Heavy = 0,Light = 0;
            for (int i = 0; i < TheBlocks.Count; i++)
            {
                MyXmlBlock thatBlock = TheBlocks[i];
                switch (SearchBy.SelectedIndex)
                {
                    case 0:
                        if(thatBlock.DisplayType.Contains(Search.Text) && (HideArmor.IsChecked.Value? !thatBlock.IsArmor :true))
                            BlockList.Items.Add(thatBlock);
                        break;
                    case 1:
                        if (thatBlock.Name != null && thatBlock.Name.Contains(Search.Text) && (HideArmor.IsChecked.Value ? !thatBlock.IsArmor : true))
                            BlockList.Items.Add(thatBlock);
                        break;
                }
                if (thatBlock.IsArmor)
                {
                    if (thatBlock.Armor == ArmorType.Heavy)
                        Heavy++;
                    else if (thatBlock.Armor == ArmorType.Light)
                        Light++;
                }
            }
            if(Heavy != Light)
                GridArmourType.SelectedIndex = Heavy > Light?0:1;
            else
            {
                GridArmourType.SelectedIndex = -1;
                if (Heavy == 0)
                {
                    HideArmor.IsEnabled = false;
                    GridArmourType.IsEnabled = false;
                }
            }
            initGAT = true;
            //BlockList.Items.SortDescriptions.Clear();
            //BlockList.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
            GridNameBox.Text = SlectedGrd.Name;
            DestructibleGridBox.IsChecked = SlectedGrd.Destructible;
            GridSizeBox.SelectedIndex = (int)SlectedGrd.GridSize;
            GoSort(BlockListColumns.Columns[1],null);
        }
        private void BlockList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (BlockList.SelectedItem != null)
            {
                
                MyXmlBlockEqualer MasterBlock = null;
                foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
                {
                    if (MasterBlock == null)
                        MasterBlock = new MyXmlBlockEqualer(SelectedBlk);
                    else
                    {
                        MasterBlock.Equalize(SelectedBlk);
                    }
                }
                if (MasterBlock.Properties.Count > 0)
                {
                    PropertyList.IsEnabled = true;
                    PropertyList.ItemsSource = MasterBlock.Properties;
                }
                else
                {
                    PropertyList.IsEnabled = false;
                    PropertyList.ItemsSource = null;
                }
                SetTextBox(BlockNameBox, MasterBlock.Name);
                string[] Str = MasterBlock.Type?.Split('/');
                SetTextBox(BlockTypeBox, Str != null && Str.Length > 0 && Str[0] != "" ? Str[0] : null);
                SetTextBox(BlockSubtypeBox, Str != null && Str.Length > 0 && Str[1] != "" ? Str[1] : null);
                BlockColorBox.Fill = new SolidColorBrush(MasterBlock.Mask.ToMediaColor());
                BlockColorBox.IsEnabled = true;
                if (MasterBlock.Share.HasValue)
                {
                    ShareBox.IsEnabled = true;
                    if (MasterBlock.Share.Value != ShareMode.Difference)
                        ShareBox.SelectedIndex = (int)MasterBlock.Share.Value;
                    else
                        ShareBox.SelectedIndex = -1;
                }
                else
                {
                    ShareBox.IsEnabled = false;
                    ShareBox.SelectedIndex = -1;
                }
            }
            else
            {
                PropertyList.IsEnabled = false;
                PropertyList.ItemsSource = null;
                SetTextBox(BlockNameBox, null);
                SetTextBox(BlockTypeBox, null);
                SetTextBox(BlockSubtypeBox, null);
                BlockColorBox.Fill = new SolidColorBrush(Colors.White);
                BlockColorBox.IsEnabled = false;
            }
                
        }
        private void PropertyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        GridViewColumn OldSortBy;
        private void GoSort(object sender, RoutedEventArgs e)
        {
            if (sender.ToString() == Lang.Property) return;
            if (sender == null) return;
            GridViewColumn SortBy = (sender as GridViewColumnHeader)?.Column;
            if(SortBy == null) SortBy = (sender as GridViewColumn);
            string PropertyPatch = ((Binding)SortBy.DisplayMemberBinding)?.Path.Path.Replace("Text", "");
            if (PropertyPatch == "PropertyName") return;
            if (PropertyPatch == null) return;
            if (OldSortBy != null)
                OldSortBy.Header = OldSortBy.Header.ToString().Trim('↓', '↑', ' ');
            ListSortDirection OldDirection = ListSortDirection.Descending;
            if (BlockList.Items.SortDescriptions.Count > 0 && BlockList.Items.SortDescriptions[0].PropertyName == PropertyPatch)
                OldDirection = BlockList.Items.SortDescriptions[0].Direction;
            ListSortDirection NewDirection = OldDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            if (e != null)
                NewDirection = OldDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else
                NewDirection = OldDirection;
            BlockList.Items.SortDescriptions.Clear();
            BlockList.Items.SortDescriptions.Add(new SortDescription(PropertyPatch, NewDirection));
            SortBy.Header += NewDirection == ListSortDirection.Ascending ? " ↓" : " ↑";
            OldSortBy = SortBy;
        }
        private void SetTextBox(TextBox Box, string Text)
        {
            if (Text != null)
            {
                Box.IsEnabled = true;
                Box.Text = Text;
            }
            else
            {
                Box.Text = "";
                Box.IsEnabled = false;
            }
        }

        private void GridNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int Selectedindx = GridList.SelectedIndex;
            if (Selectedindx == -1) return;
            MyXmlGrid SlectedGrd = EdBlueprint.Grids[Selectedindx];
            if(GridList.Items.Count > Selectedindx && GridNameBox.Text != SlectedGrd.Name)
                GridList.Items[Selectedindx] = GridNameBox.Text;
            SlectedGrd.Name = GridNameBox.Text;
            GridList.SelectedIndex = Selectedindx;
        }
        private void DestructibleGridBox_Click(object sender, RoutedEventArgs e)
        {
            EdBlueprint.Grids[GridList.SelectedIndex].Destructible = DestructibleGridBox.IsChecked.Value;
            //Console.WriteLine(EdBlueprint.Grids[GridList.SelectedIndex].Destructible);
        }
        private void GridSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EdBlueprint.Grids[GridList.SelectedIndex].GridSize = (GridSizes)GridSizeBox.SelectedIndex;
        }

        private void BlockNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BlockNameBox.Text == "" || BlockNameBox.Text == "-") return;
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                SelectedBlk.Name = BlockNameBox.Text;
            }
            GoSort(OldSortBy,null);
            BlockList.ScrollIntoView(BlockList.SelectedItem);
        }
        private void BlockTypeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BlockTypeBox.Text == "") return;
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                SelectedBlk.Type = BlockTypeBox.Text+"/"+ BlockSubtypeBox.Text;
            }
            GoSort(OldSortBy, null);
            BlockList.ScrollIntoView(BlockList.SelectedItem);
        }
        private void BlockSubtypeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BlockSubtypeBox.Text == "") return;
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                SelectedBlk.Type = BlockTypeBox.Text + "/" + BlockSubtypeBox.Text;
            }
            GoSort(OldSortBy, null);
            BlockList.ScrollIntoView(BlockList.SelectedItem);
        }
        private void BlockColorBox_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Forms.ColorDialog MyDialog = new Forms.ColorDialog();
            MyDialog.AllowFullOpen = true;
            MyDialog.ShowHelp = false;
            MyDialog.Color = ((SolidColorBrush)BlockColorBox.Fill).Color.ToDrawingColor();
            if (MyDialog.ShowDialog() == Forms.DialogResult.OK)
            {
                foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
                {
                    SelectedBlk.ColorMask = MyDialog.Color;
                }
                BlockColorBox.Fill = new SolidColorBrush(MyDialog.Color.ToMediaColor());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EdBlueprint.Save();
            Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _Lock?.Dispose();
            OpenCount--;
            if (OpenCount == 0)
            {
                SelectBlueprint.window.Top = SystemParameters.PrimaryScreenHeight / 2 - SelectBlueprint.window.Height / 2;
                SelectBlueprint.window.Left = SystemParameters.PrimaryScreenWidth / 2 - SelectBlueprint.window.Width / 2;
            }
            if (!MySettings.Current.MultiWindow) SelectBlueprint.window.Show();
        }

        private void ShareBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShareBox.SelectedIndex == -1) return;
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                SelectedBlk.ShareMode = (ShareMode)ShareBox.SelectedIndex;
            }
        }

        private void PropertyClick(object sender, RoutedEventArgs e)
        {
           //((EventHandler<RoutedEventArgs>)((Button)sender).DataContext).Invoke(null,null);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox Sender = (CheckBox)sender;
            Sender.Content = Sender.IsChecked.Value ? Lang.Yes : Lang.No;
            string TxtVle = Sender.IsChecked.ToString();
            string PropName = Sender.CommandParameter.ToString();
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                MyBlockProperty Change = SelectedBlk.Properties.FirstOrDefault(x => x.PropertyName == PropName);
                if (Change != null)
                {
                    Change.TextValue = TxtVle;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GridList_SelectionChanged(null,null);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            List<MyXmlBlock> Blocks = new List<MyXmlBlock>();
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                EdBlueprint.Grids[GridList.SelectedIndex].RemoveBlock(SelectedBlk);
                Blocks.Add(SelectedBlk);
            }
            foreach (MyXmlBlock SelectedBlk in Blocks)
            {
                BlockList.Items.Remove(SelectedBlk);
            }
            GoSort(OldSortBy, null);
            BlockList.ScrollIntoView(BlockList.SelectedItem);
        }
        bool initGAT = false;
        private void GridArmourType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!initGAT) return;
            MyXmlGrid SlectedGrd = EdBlueprint.Grids[GridList.SelectedIndex];
            ArmorType type = (ArmorType)GridArmourType.SelectedIndex;
            foreach (MyXmlBlock block in SlectedGrd.Blocks)
            {
                block.Armor = type;
            }
            GridList_SelectionChanged(null,null);
        }
    }
}
