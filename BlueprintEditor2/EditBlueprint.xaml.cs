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
using System.Numerics;

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
            Logger.Add("Grid changing");
            BlockList.Items.Clear();
            GridArmourType.IsEnabled = true;
            initGAT = false;
            if (GridList.SelectedIndex == -1) return;
            MyXmlGrid SlectedGrd = EdBlueprint.Grids[GridList.SelectedIndex];
            Logger.Add($"Grid changed to {SlectedGrd.Name}");
            List<MyXmlBlock> TheBlocks = SlectedGrd.Blocks;
            int Heavy = 0,Light = 0;
            ExistedColors.Items.Clear();
            for (int i = 0; i < TheBlocks.Count; i++)
            {
                MyXmlBlock thatBlock = TheBlocks[i];
                if (DasShowIt(thatBlock))
                    BlockList.Items.Add(thatBlock);
                if (thatBlock.IsArmor)
                {
                    if (thatBlock.Armor == ArmorType.Heavy)
                        Heavy++;
                    else if (thatBlock.Armor == ArmorType.Light)
                        Light++;
                }
                string argb = thatBlock.ColorMask.ToArgb().ToString("X");
                if (argb != "0")
                {
                    argb = $"#{argb.Substring(2)}";
                    if (ExistedColors.Items.IndexOf(argb) == -1)
                    {
                        ExistedColors.Items.Add(argb);
                    }
                }
            }
            if (ExistedColors.Items.Count > 0)
            {
                ExistedColors.SelectedIndex = 0;
                ColorChangeButton.IsEnabled = true;
                ExistedColors.IsEnabled = true;
                GridColorReplace.IsEnabled = true;
                GridColorChange.IsEnabled = true;
                GridColorExist.IsEnabled = true;
            }
            else
            {
                ColorChangeButton.IsEnabled = false;
                ExistedColors.IsEnabled = false;
                GridColorReplace.IsEnabled = false;
                GridColorChange.IsEnabled = false;
                GridColorExist.IsEnabled = false;
            }
            if(Heavy != Light)
                GridArmourType.SelectedIndex = Heavy > Light?0:1;
            else
            {
                GridArmourType.SelectedIndex = -1;
                if (Heavy == 0)
                {
                    GridArmourType.IsEnabled = false;
                }
            }
            initGAT = true;
            //BlockList.Items.SortDescriptions.Clear();
            //BlockList.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
            GridNameBox.Text = SlectedGrd.Name;
            DestructibleGridBox.IsChecked = SlectedGrd.Destructible;
            GridSizeBox.SelectedIndex = (int)SlectedGrd.GridSize;
            DamageFixer.IsEnabled = SlectedGrd.IsDamaged;
            GoSort(BlockListColumns.Columns[1],null);
        }
        private bool DasShowIt(MyXmlBlock block)
        {
            bool show = true;
            string SearchText = Search.Text.ToLower();
            switch (SearchBy.SelectedIndex)
            {
                case 0:
                    if (!string.IsNullOrEmpty(SearchText))
                        show &= block.DisplayType.ToLower().Contains(SearchText);
                    break;
                case 1:
                    if (!string.IsNullOrEmpty(SearchText))
                        show &= block.Name != null && block.Name.ToLower().Contains(SearchText);
                    break;
            }
            switch (ShowType.SelectedIndex)
            {
                case 1:
                    show &= !block.IsArmor;
                    break;
                case 2:
                    show &= block.IsArmor;
                    break;
                case 3:
                    show &= block.Name == null;
                    break;
                case 4:
                    show &= block.Name != null;
                    break;
            }
            return show;
        }
        private void BlockList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            Logger.Add("Block changed");
            if (BlockList.SelectedItem != null)
            {
                DeleteButton.IsEnabled = true;
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
                //SetTextBox(CustomData, MasterBlock.Name);
                SetTextBox(BlockNameBox, MasterBlock.Name);
                string[] Str = MasterBlock.Type?.Split('/');
                SetTextBox(BlockTypeBox, Str != null && Str.Length > 0 && Str[0] != "" ? Str[0] : null);
                SetTextBox(BlockSubtypeBox, Str != null && Str.Length > 0 && Str[1] != "" ? Str[1] : null);
                BlockColorBox.Fill = new SolidColorBrush(MasterBlock.Mask.ToMediaColor());
                BlockColorBox.IsEnabled = true;
                if(MasterBlock.Position != Vector3.Zero)
                {
                    BlockX.IsEnabled = true;
                    BlockY.IsEnabled = true;
                    BlockZ.IsEnabled = true;
                    BlockX.Text = MasterBlock.Position.X.ToString();
                    BlockY.Text = MasterBlock.Position.Y.ToString();
                    BlockZ.Text = MasterBlock.Position.Z.ToString();
                }
                else
                {
                    BlockX.Text = "0";
                    BlockY.Text = "0";
                    BlockZ.Text = "0";
                    BlockX.IsEnabled = false;
                    BlockY.IsEnabled = false;
                    BlockZ.IsEnabled = false;
                }
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
                SetSpecials(MasterBlock);
            }
            else
            {
                DeleteButton.IsEnabled = false;
                PropertyList.IsEnabled = false;
                PropertyList.ItemsSource = null;
                SetTextBox(BlockNameBox, null);
                SetTextBox(BlockTypeBox, null);
                SetTextBox(BlockSubtypeBox, null);
                SetSpecials(null);
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
            Logger.Add($"Sort Blocks by {PropertyPatch} in {NewDirection}");
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
        private void SetSpecials(MyXmlBlockEqualer MasterBlock)
        {
            Specials.SelectedIndex = 0;
            Specials.IsEnabled = false;
            (Specials.Items[1] as TabItem).Visibility = Visibility.Collapsed;
            (Specials.Items[2] as TabItem).Visibility = Visibility.Collapsed;
            if (MasterBlock != null)
            {
                if (MasterBlock.PublicText != null)
                {
                    Specials.SelectedIndex = 2;
                    Specials.IsEnabled = true;
                    (Specials.Items[2] as TabItem).Visibility = Visibility.Visible;
                    PannelText.Text = MasterBlock.PublicText;
                }
                if (MasterBlock.CustomData != null)
                {
                    Specials.SelectedIndex = 1;
                    Specials.IsEnabled = true;
                    (Specials.Items[1] as TabItem).Visibility = Visibility.Visible;
                    CustomData.Text = MasterBlock.CustomData;
                }


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
            Logger.Add($"Destructable changed to {DestructibleGridBox.IsChecked.Value}");
            EdBlueprint.Grids[GridList.SelectedIndex].Destructible = DestructibleGridBox.IsChecked.Value;
            //Console.WriteLine(EdBlueprint.Grids[GridList.SelectedIndex].Destructible);
        }
        private void GridSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Logger.Add("Grid size changed");
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
            Logger.Add("Blueprint saved");
            EdBlueprint.Save();
            Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _Lock?.Dispose();
            OpenCount--;
            if (OpenCount == 0)
            {
                //SelectBlueprint.window.Top = SystemParameters.PrimaryScreenHeight / 2 - SelectBlueprint.window.Height / 2;
                //SelectBlueprint.window.Left = SystemParameters.PrimaryScreenWidth / 2 - SelectBlueprint.window.Width / 2;
            }
            if (!MySettings.Current.MultiWindow)
            {
                try
                {
                    SelectBlueprint.window.Show();
                }
                catch
                {
                }
            }
            Logger.Add("Editor closed");
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
            if (Sender.Visibility == Visibility.Collapsed)
                return;
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

        private void BlockXYZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox Sender = (TextBox)sender;
            int.TryParse(Sender.Text,out int UnknownCoord);
            Sender.Text = UnknownCoord.ToString();
            Sender.CaretIndex = Sender.Text.Length;
            if (BlockList.SelectedItem != null)
            {
                int.TryParse(BlockX.Text, out int X);
                int.TryParse(BlockY.Text, out int Y);
                int.TryParse(BlockZ.Text, out int Z);
                ((MyXmlBlock)BlockList.SelectedItem).Position = new Vector3(X, Y, Z);
            }
        }

        int OldShowType = 0;
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OldShowType != ShowType.SelectedIndex)
            {
                OldShowType = ShowType.SelectedIndex;
                GridList_SelectionChanged(null, null);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GridList_SelectionChanged(null, null);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox Sender = (TextBox)sender;
            if (Sender.Visibility == Visibility.Collapsed)
                return;
            long.TryParse(Sender.Text, out long intres);
            string TxtVle = intres.ToString();
            Sender.Text = TxtVle;
            Sender.CaretIndex = TxtVle.Length;
            StackPanel X = (StackPanel)Sender.Parent;
            CheckBox Tender = (CheckBox)X.Children[0];
            string PropName = Tender.CommandParameter.ToString();
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                MyBlockProperty Change = SelectedBlk.Properties.FirstOrDefault(x => x.PropertyName == PropName);
                if (Change != null)
                {
                    Change.TextValue = TxtVle;
                }
            }
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            
            TextBox Sender = (TextBox)sender;
            if (Sender.Visibility == Visibility.Collapsed)
                return;
            string top = Sender.Text.Replace(".", ",");
            if (top.EndsWith(","))
                return;
            double.TryParse(top, out double intres);
            int cind = Sender.CaretIndex;
            string TxtVle = intres.ToString("F18").TrimEnd('0').TrimEnd(',');
            Sender.Text = TxtVle;
            Sender.CaretIndex = cind;
            StackPanel X = (StackPanel)Sender.Parent;
            CheckBox Tender = (CheckBox)X.Children[0];
            string PropName = Tender.CommandParameter.ToString();
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                MyBlockProperty Change = SelectedBlk.Properties.FirstOrDefault(x => x.PropertyName == PropName);
                if (Change != null)
                {
                    Change.TextValue = TxtVle.Replace(",",".");
                }
            }
        }

        private void GridNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void ExistedColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExistedColors.SelectedIndex == -1) return;
            GridColorReplace.Text = ExistedColors.SelectedItem.ToString();
            Color color = (Color)ColorConverter.ConvertFromString(GridColorReplace.Text);
            GridColorExist.Fill = new SolidColorBrush(color);
        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(GridColorReplace.Text);
                GridColorChange.Fill = new SolidColorBrush(color);
                string argb = color.ToDrawingColor().ToArgb().ToString("X");
                if (argb != "0")
                {
                    //int cind = Sender.CaretIndex;
                    GridColorReplace.Text = $"#{argb.Substring(2)}";
                    GridColorReplace.CaretIndex = GridColorReplace.Text.Length;
                }
            }
            catch (FormatException)
            {
                
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (!initGAT) return;
            MyXmlGrid SlectedGrd = EdBlueprint.Grids[GridList.SelectedIndex];
            System.Drawing.Color From = ((SolidColorBrush)GridColorExist.Fill).Color.ToDrawingColor();
            System.Drawing.Color To = ((SolidColorBrush)GridColorChange.Fill).Color.ToDrawingColor();
            foreach (MyXmlBlock block in SlectedGrd.Blocks)
            {
                if (block.ColorMask.Equals(From))
                    block.ColorMask = To;
            }
            GridList_SelectionChanged(null, null);
        }

        private void GridColorChange_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Forms.ColorDialog MyDialog = new Forms.ColorDialog();
            MyDialog.AllowFullOpen = true;
            MyDialog.ShowHelp = false;
            MyDialog.Color = ((SolidColorBrush)GridColorChange.Fill).Color.ToDrawingColor();
            if (MyDialog.ShowDialog() == Forms.DialogResult.OK)
            {
                GridColorChange.Fill = new SolidColorBrush(MyDialog.Color.ToMediaColor());
                string argb = MyDialog.Color.ToArgb().ToString("X");
                if (argb != "0")
                {
                    //int cind = Sender.CaretIndex;
                    GridColorReplace.Text = $"#{argb.Substring(2)}";
                    GridColorReplace.CaretIndex = GridColorReplace.Text.Length;
                }
            }
        }

        private void CustomData_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CustomData.Text == "" || CustomData.Text == "-") return;
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                SelectedBlk.CustomData = CustomData.Text;
            }
        }

        private void PannelText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PannelText.Text == "" || PannelText.Text == "-") return;
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                SelectedBlk.PublicText = PannelText.Text;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Button Sender = sender as Button;
            if (!initGAT) return;
            MyXmlGrid SlectedGrd = EdBlueprint.Grids[GridList.SelectedIndex];
            foreach (MyXmlBlock block in SlectedGrd.Blocks)
            {
                var bp = block.GetProperty("BuildPercent");
                block.SetPropertyIfExists("IntegrityPercent", bp != null? bp.TextValue: "1");
            }
            SlectedGrd.FixVisualDamage();
            if(Sender != null) Sender.IsEnabled = false;
            BlockList_SelectionChanged_1(null, null);
        }
    }
}
