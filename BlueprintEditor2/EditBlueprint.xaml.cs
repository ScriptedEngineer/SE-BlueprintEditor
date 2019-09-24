using System;
using System.Collections.Generic;
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
using System.Threading;
using System.IO;
using System.Reflection;
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
            //if(GridList.Items.Count > 0) GridList.SelectedIndex = 0;
            OpenCount++;
            //Viewport3D myViewport3D = new Viewport3D();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _Lock?.Dispose();
            OpenCount--;
            if(OpenCount == 0)
            {
                SelectBlueprint.window.Top = SystemParameters.PrimaryScreenHeight/2- SelectBlueprint.window.Height/2;
                SelectBlueprint.window.Left = SystemParameters.PrimaryScreenWidth / 2 - SelectBlueprint.window.Width / 2;
            }
            if (!MySettings.Current.MultiWindow) SelectBlueprint.window.Show();
        }

        private void GridList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BlockList.Items.Clear();
            MyXmlBlock[] TheBlocks = EdBlueprint.Grids[GridList.SelectedIndex].Blocks;
            for (int i = 0; i < TheBlocks.Length; i++)
                BlockList.Items.Add(TheBlocks[i]);
            BlockList.Items.SortDescriptions.Clear();
            BlockList.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
        }
        private void GoSort(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader SortBy = (GridViewColumnHeader)sender;
            string PropertyPatch = ((Binding)SortBy.Column.DisplayMemberBinding).Path.Path.Replace("Text", "");
            ListSortDirection OldDirection = ListSortDirection.Descending;
            if (BlockList.Items.SortDescriptions[0].PropertyName == PropertyPatch) OldDirection = BlockList.Items.SortDescriptions[0].Direction;
            ListSortDirection NewDirection = OldDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            BlockList.Items.SortDescriptions.Clear();
            BlockList.Items.SortDescriptions.Add(new SortDescription(PropertyPatch, NewDirection));
        }
        private void BlockList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
