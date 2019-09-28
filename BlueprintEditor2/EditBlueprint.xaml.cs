using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using System.ComponentModel;

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
            if (GridList.SelectedIndex == -1) return;
            MyXmlGrid SlectedGrd = EdBlueprint.Grids[GridList.SelectedIndex];
            MyXmlBlock[] TheBlocks = SlectedGrd.Blocks;
            for (int i = 0; i < TheBlocks.Length; i++)
                BlockList.Items.Add(TheBlocks[i]);
            BlockList.Items.SortDescriptions.Clear();
            BlockList.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
            GridNameBox.Text = SlectedGrd.Name;
            DestructibleGridBox.IsChecked = SlectedGrd.Destructible;
            GridSizeBox.SelectedIndex = (int)SlectedGrd.GridSize;
        }
        GridViewColumnHeader OldSortBy;
        private void GoSort(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader SortBy = (GridViewColumnHeader)sender;
            if(OldSortBy != null)
                OldSortBy.Content = OldSortBy.Content.ToString().Trim('↓', '↑', ' ');
            string PropertyPatch = ((Binding)SortBy.Column.DisplayMemberBinding).Path.Path.Replace("Text", "");
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
            SortBy.Content += NewDirection == ListSortDirection.Ascending ? " ↓" : " ↑";
            OldSortBy = SortBy;
        }
        private void BlockList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (BlockList.SelectedItem != null)
            {
                if(BlockList.SelectedItems.Count == 1)
                {
                    MyXmlBlock SelectedBlk = (MyXmlBlock)BlockList.SelectedItem;
                    PropertyList.IsEnabled = true;
                    PropertyList.ItemsSource = SelectedBlk.Properties;
                    SetTextBox(BlockNameBox, SelectedBlk.Name);
                    string[] Str = SelectedBlk.Type.Split('/');
                    SetTextBox(BlockTypeBox, Str != null && Str.Length > 0 && Str[0] != "" ? Str[0] : null);
                    SetTextBox(BlockSubtypeBox, Str != null && Str.Length > 0 && Str[1] != "" ? Str[1] : null);
                }
                else
                {
                    PropertyList.IsEnabled = false;
                    PropertyList.ItemsSource = null;
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
                    SetTextBox(BlockNameBox, MasterBlock.Name);
                    string[] Str = MasterBlock.Type?.Split('/');
                    SetTextBox(BlockTypeBox, Str != null && Str.Length > 0 && Str[0] != "" ? Str[0]:null);
                    SetTextBox(BlockSubtypeBox, Str != null && Str.Length > 0 && Str[1] != "" ? Str[1]: null);
                }
            }
            else
            {
                PropertyList.IsEnabled = false;
                PropertyList.ItemsSource = null;
                SetTextBox(BlockNameBox, null);
                SetTextBox(BlockTypeBox, null);
                SetTextBox(BlockSubtypeBox, null);
            }
                
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
        private void PropertyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
            Console.WriteLine(EdBlueprint.Grids[GridList.SelectedIndex].Destructible);
        }
        private void GridSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EdBlueprint.Grids[GridList.SelectedIndex].GridSize = (GridSizes)GridSizeBox.SelectedIndex;
        }
        private void BlockNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (MyXmlBlock SelectedBlk in BlockList.SelectedItems)
            {
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

    }
}
