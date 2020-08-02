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
        readonly FileStream _Lock;
        readonly MyXmlBlueprint EdBlueprint;
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

        bool ProgEditing = false;
        private void GridList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Logger.Add("Grid changing");
            //BlockList.Items.Clear();
            GridArmourType.IsEnabled = true;
            initGAT = false;
            if (GridList.SelectedIndex == -1) return;
            MyXmlGrid SlectedGrd = EdBlueprint.Grids[GridList.SelectedIndex];
            Logger.Add($"Grid changed to {SlectedGrd.Name}");
            List<MyXmlBlock> TheBlocks = SlectedGrd.Blocks;
            int Heavy = 0,Light = 0;
            ExistedColors.Items.Clear();
            List<MyXmlBlock> BlocksToAdd = new List<MyXmlBlock>();
            for (int i = 0; i < TheBlocks.Count; i++)
            {
                MyXmlBlock thatBlock = TheBlocks[i];
                if (DasShowIt(thatBlock))
                    BlocksToAdd.Add(thatBlock);
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
            BlockList.ItemsSource = BlocksToAdd;
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
            GoSort(BlockListColumns.Columns[1],null,true);
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
            ProgEditing = true;
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
                SetTextBox(BlockTypeBox, MasterBlock.Type);
                //BlockTypeBox.ItemsSource = MyGameData.CubeBlocks.Keys;
               
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
                if(MasterBlock.Orientation != null)
                {
                    ForwardBox.SelectedIndex = (int)MasterBlock.Orientation.Forward;
                    UpBox.SelectedIndex = (int)MasterBlock.Orientation.Up;
                    UpBox.IsEnabled = ForwardBox.IsEnabled = true;
                }
                else
                {
                    UpBox.SelectedIndex = ForwardBox.SelectedIndex = -1;
                    UpBox.IsEnabled = ForwardBox.IsEnabled = false;
                }
                SetSpecials(MasterBlock);
            }
            else
            {
                UpBox.SelectedIndex = ForwardBox.SelectedIndex = -1;
                UpBox.IsEnabled = ForwardBox.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                PropertyList.IsEnabled = false;
                PropertyList.ItemsSource = null;
                SetTextBox(BlockNameBox, null);
                SetTextBox(BlockTypeBox, null);
                BlockTypeBox.ItemsSource = null;
                SetSpecials(null);
                BlockColorBox.Fill = new SolidColorBrush(Colors.White);
                BlockColorBox.IsEnabled = false;
            }
            ProgEditing = false;
        }
        private void PropertyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        GridViewColumn OldSortBy;
        ListSortDirection OldDirection = ListSortDirection.Descending;
        private void GoSort(object sender, RoutedEventArgs e) => GoSort(sender, e, false);
        private void GoSort(object sender, RoutedEventArgs e, bool Resort)
        {
            if (sender == null) return;
            if (sender.ToString() == Lang.Property) return;
            GridViewColumn SortBy = (sender as GridViewColumnHeader)?.Column;
            if (SortBy == null) SortBy = (sender as GridViewColumn);
            if (SortBy == null) return;
            string PropertyPatch = ((Binding)SortBy.DisplayMemberBinding)?.Path.Path.Replace("Text", "");
            if (PropertyPatch == "PropertyName") return;
            if (PropertyPatch == null) return;
            if (OldSortBy != null)
                OldSortBy.Header = OldSortBy.Header.ToString().Trim('↓', '↑', ' ');
            /*ListSortDirection OldDirection = ListSortDirection.Descending;
            if (BlockList.Items.SortDescriptions.Count > 0 && BlockList.Items.SortDescriptions[0].PropertyName == PropertyPatch)
                OldDirection = BlockList.Items.SortDescriptions[0].Direction;*/
            ListSortDirection NewDirection;// = OldDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            if (e != null)
                NewDirection = OldDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else
                NewDirection = OldDirection;
            Logger.Add($"Sort Blocks by {PropertyPatch} in {NewDirection}");
            MyXmlBlock[] Sort = BlockList.ItemsSource.OfType<MyXmlBlock>().ToArray();
            if (NewDirection != OldDirection || OldSortBy != SortBy || Resort)
            {
                if (NewDirection == ListSortDirection.Ascending)
                {
                    switch (PropertyPatch)
                    {
                        case "Name":
                            BlockList.ItemsSource = Sort.OrderBy(x => x.Name);
                            //MyExtensions.quickSort(ref Sort, (x, y) => string.CompareOrdinal(x.Name, y.Name));
                            break;
                        case "DisplayType":
                            BlockList.ItemsSource = Sort.OrderBy(x => x.DisplayType);
                            //MyExtensions.quickSort(ref Sort, (x, y) => string.CompareOrdinal(x.DisplayType, y.DisplayType));
                            break;
                    }
                }
                else
                {
                    switch (PropertyPatch)
                    {
                        case "Name":
                            BlockList.ItemsSource = Sort.OrderByDescending(x => x.Name);
                            //MyExtensions.quickSort(ref Sort, (x, y) => -string.CompareOrdinal(x.Name, y.Name));
                            break;
                        case "DisplayType":
                            BlockList.ItemsSource = Sort.OrderByDescending(x => x.DisplayType);
                            //MyExtensions.quickSort(ref Sort, (x, y) => -string.CompareOrdinal(x.DisplayType, y.DisplayType));
                            break;
                    }
                }
            }else BlockList.ItemsSource = Sort;
            //BlockList.Items.SortDescriptions.Clear();
            //BlockList.Items.SortDescriptions.Add(new SortDescription(PropertyPatch, NewDirection));
            SortBy.Header += NewDirection == ListSortDirection.Ascending ? " ↓" : " ↑";
            OldSortBy = SortBy;
            OldDirection = NewDirection;
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
        private void SetTextBox(ComboBox Box, string Text)
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
            foreach(TabItem item in Specials.Items)
                item.Visibility = Visibility.Collapsed;
            if (MasterBlock != null)
            {
                if (MasterBlock.Storage != null)
                {
                    Specials.SelectedIndex = 5;
                    Specials.IsEnabled = true;
                    (Specials.Items[5] as TabItem).Visibility = Visibility.Visible;
                    Storage.Text = MasterBlock.Storage;
                }
                if (MasterBlock.Program != null)
                {
                    Specials.SelectedIndex = 4;
                    Specials.IsEnabled = true;
                    (Specials.Items[4] as TabItem).Visibility = Visibility.Visible;
                    Program.Text = MasterBlock.Program;
                }
                if (MasterBlock.Inventories.Count > 0)
                {
                    Specials.SelectedIndex = 3;
                    Specials.IsEnabled = true;
                    (Specials.Items[3] as TabItem).Visibility = Visibility.Visible;
                    InventoryNum.Items.Clear();
                    for (int i = 1; i <= MasterBlock.Inventories.Count; i++)
                        InventoryNum.Items.Add($"#{i}");
                    InventoryItems.ItemsSource = MasterBlock.Inventories[0].Items;
                    InventoryNum.SelectedIndex = 0;
                }
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
            bool Update = false;
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                if (SelectedBlk.Name != BlockNameBox.Text)
                {
                    SelectedBlk.Name = BlockNameBox.Text;
                    Update = true;
                }
               
            }
            if (Update)
            {
                GoSort(OldSortBy, null);
                BlockList.ScrollIntoView(BlockList.SelectedItem);
            }
        }
        private void BlockTypeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool Update = false;
            if (!ProgEditing && BlockTypeBox.Text != "")
            {
                var tb = (TextBox)e.OriginalSource;
                BlockTypeBox.Items.Clear();
                foreach (var s in MyGameData.BlockTypes)
                {
                    if (s.IndexOf(BlockTypeBox.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        BlockTypeBox.Items.Add(s);
                    if (BlockTypeBox.Items.Count >= 30)
                        break;
                }
                if (BlockTypeBox.Items.Count == 0)
                    BlockTypeBox.IsDropDownOpen = false;
                else
                {
                    BlockTypeBox.IsDropDownOpen = true;
                    tb.SelectionStart = BlockTypeBox.Text.Length;
                }
            }
            else
            {
                BlockTypeBox.Items.Clear();
                BlockTypeBox.IsDropDownOpen = false;
            }
            if (BlockTypeBox.Text == "") return;
            string[] types = BlockTypeBox.Text.Split('/');
            if (types.Length < 2) return;
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                if (SelectedBlk.Type != BlockTypeBox.Text)
                {
                    SelectedBlk.Type = BlockTypeBox.Text;
                    Update = true;
                }
            }
            if (Update)
            {
                GoSort(OldSortBy, null);
                BlockList.ScrollIntoView(BlockList.SelectedItem);
            }
        }
        private void BlockColorBox_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Forms.ColorDialog MyDialog = new Forms.ColorDialog
            {
                AllowFullOpen = true,
                ShowHelp = false,
                Color = ((SolidColorBrush)BlockColorBox.Fill).Color.ToDrawingColor()
            };
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
            var Sort = BlockList.ItemsSource.OfType<MyXmlBlock>().ToList();
            foreach (MyXmlBlock SelectedBlk in Blocks)
            {
                Sort.Remove(SelectedBlk);
            }
            BlockList.ItemsSource = Sort;
            GoSort(OldSortBy, null, true);
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
            Forms.ColorDialog MyDialog = new Forms.ColorDialog
            {
                AllowFullOpen = true,
                ShowHelp = false,
                Color = ((SolidColorBrush)GridColorChange.Fill).Color.ToDrawingColor()
            };
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
            if (!initGAT) return;
            MyXmlGrid SlectedGrd = EdBlueprint.Grids[GridList.SelectedIndex];
            foreach (MyXmlBlock block in SlectedGrd.Blocks)
            {
                var bp = block.GetProperty("BuildPercent");
                block.SetPropertyIfExists("IntegrityPercent", bp != null? bp.TextValue: "1");
            }
            SlectedGrd.FixVisualDamage();
            if(sender is Button Sender) Sender.IsEnabled = false;
            BlockList_SelectionChanged_1(null, null);
        }

        private void BlockTypeBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox Sender = (ComboBox)sender;
            if (ProgEditing || Sender.Text == "" || Sender.Items.Count == 0)
                Sender.IsDropDownOpen = false;
        }
        private void InventoryItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox Sender = (ComboBox)sender;
            if (!ProgEditing && Sender.Text != "" && e.Changes.Count == 1 && e.Changes.FirstOrDefault().AddedLength == 1)
            {
                var tb = (TextBox)e.OriginalSource;
                Sender.Items.Clear();
                foreach (var s in MyGameData.ItemTypes)
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
            MyXmlBlock Block = (MyXmlBlock)BlockList.SelectedItem;
            if(InventoryItems.ItemsSource != null)
                Block.Inventories[InventoryNum.SelectedIndex].Items = InventoryItems.ItemsSource.OfType<MyBlockInventory.MyItem>();
        }

        private void TextBox_TextChanged_3(object sender, TextChangedEventArgs e)
        {
            TextBox Sender = (TextBox)sender;
            string top = Sender.Text.Replace(".", ",");
            if (top.EndsWith(","))
                return;
            double.TryParse(top, out double intres);
            int cind = Sender.CaretIndex;
            string TxtVle = intres.ToString("F18").TrimEnd('0').TrimEnd(',');
            Sender.Text = TxtVle;
            Sender.CaretIndex = cind;
            MyXmlBlock Block = (MyXmlBlock)BlockList.SelectedItem;
            if (InventoryItems.ItemsSource != null)
                Block.Inventories[InventoryNum.SelectedIndex].Items = InventoryItems.ItemsSource.OfType<MyBlockInventory.MyItem>();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            MyXmlBlock Block = (MyXmlBlock)BlockList.SelectedItem;
            List<MyBlockInventory.MyItem> items = new List<MyBlockInventory.MyItem>(Block.Inventories[InventoryNum.SelectedIndex].Items)
            {
                new MyBlockInventory.MyItem("None", 1)
            };
            InventoryItems.ItemsSource = Block.Inventories[InventoryNum.SelectedIndex].Items = items;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            MyXmlBlock Block = (MyXmlBlock)BlockList.SelectedItem;
            List<MyBlockInventory.MyItem> items = new List<MyBlockInventory.MyItem>(Block.Inventories[InventoryNum.SelectedIndex].Items);
            if (InventoryItems.SelectedIndex != -1)
            {
                items.Remove((MyBlockInventory.MyItem)InventoryItems.SelectedItem);
            }
            else
            {
                items.Remove(items.Last());
            }
            InventoryItems.ItemsSource = Block.Inventories[InventoryNum.SelectedIndex].Items = items;
        }

        private void Program_TextChanged(object sender, TextChangedEventArgs e)
        {
            MyXmlBlock Block = (MyXmlBlock)BlockList.SelectedItem;
            Block.Program = Program.Text;
        }

        private void Storage_TextChanged(object sender, TextChangedEventArgs e)
        {
            MyXmlBlock Block = (MyXmlBlock)BlockList.SelectedItem;
            Block.Storage = Storage.Text;
        }

        private void TextBox_TextChanged_4(object sender, TextChangedEventArgs e)
        {
            TextBox Sender = (TextBox)sender;
            if (Sender.Visibility == Visibility.Collapsed)
                return;
            StackPanel X = (StackPanel)Sender.Parent;
            CheckBox Tender = (CheckBox)X.Children[0];
            string PropName = Tender.CommandParameter.ToString();
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
                MyBlockProperty Change = SelectedBlk.Properties.FirstOrDefault(x => x.PropertyName == PropName);
                if (Change != null)
                {
                    Change.TextValue = Sender.Text;
                }
            }
        }

        private void InventoryNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProgEditing = true;
            MyXmlBlock Block = (MyXmlBlock)BlockList.SelectedItem;
            if (InventoryNum.SelectedIndex != -1 && Block != null && Block.Inventories.Count > InventoryNum.SelectedIndex)
                InventoryItems.ItemsSource = Block.Inventories[InventoryNum.SelectedIndex].Items;
            ProgEditing = false;
        }

        private void ForwardBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ForwardBox.SelectedIndex == -1) return;
            MyXmlBlock Block = (MyXmlBlock)BlockList.SelectedItem;
            Block.Orientation.Forward = (Base6Directions)ForwardBox.SelectedIndex;
            Block.Orientation.Up = MyBlockOrientation.Reorient(Block.Orientation.Forward, Block.Orientation.Up);
            UpBox.SelectedIndex = (int)Block.Orientation.Up;
        }

        private void UpBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UpBox.SelectedIndex == -1) return;
            MyXmlBlock Block = (MyXmlBlock)BlockList.SelectedItem;
            Block.Orientation.Up = (Base6Directions)UpBox.SelectedIndex;
            Block.Orientation.Forward = MyBlockOrientation.Reorient(Block.Orientation.Up, Block.Orientation.Forward);
            ForwardBox.SelectedIndex = (int)Block.Orientation.Forward;
        }
    }
}
