using BlueprintEditor2.Resource;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Логика взаимодействия для Calculator.xaml
    /// </summary>
    public partial class Calculator : Window
    {
        MyResourceCalculator calc;
        FileStream _lock;
        MyXmlBlueprint EdBlueprint;
        public Calculator(FileStream Lock, MyXmlBlueprint Blueprint)
        {
            _lock = Lock;
            EdBlueprint = Blueprint;
            InitializeComponent();
            Title = "[" + EdBlueprint.Patch.Split('\\').Last() + "] Calculator - SE BlueprintEditor";
            BluePicture.Source = EdBlueprint.GetPic(false, false);
            EditBlueprint.OpenCount++;
            if (MySettings.Current.GamePatch == null)
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.SelectedPath = @"C:\Program Files (x86)\Steam\SteamApps\common\SpaceEngineers\";
                    dialog.Description = Lang.SelectGamePatchDesc;
                    dialog.ShowNewFolderButton = false;
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result.Equals(System.Windows.Forms.DialogResult.OK))
                    {
                        MySettings.Current.GamePatch = dialog.SelectedPath + "\\";
                            calc = new MyResourceCalculator(MySettings.Current.GamePatch);
                    }
                }
            }
            else 
                calc = new MyResourceCalculator(MySettings.Current.GamePatch);
            if (calc != null && MyResourceCalculator.IsInitialized)
            {
                foreach (MyXmlGrid Gr in Blueprint.Grids)
                {
                    foreach (MyXmlBlock Bl in Gr.Blocks)
                    {
                        calc.AddBlock(Bl);
                    }
                }
                calc.CalculateIngots();
                calc.CalculateOres();
                ComponensList.ItemsSource = calc.GetComponents();
                IngotsList.ItemsSource = calc.GetIngots();
                OresList.ItemsSource = calc.GetOres();
                ListSort(ComponensList);
                ListSort(IngotsList);
                ListSort(OresList);
            }
            else
            {
                Window_Closing(null, null);
                Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _lock.Dispose();
            EditBlueprint.OpenCount--;
            if (EditBlueprint.OpenCount == 0)
            {
                SelectBlueprint.window.Top = SystemParameters.PrimaryScreenHeight / 2 - SelectBlueprint.window.Height / 2;
                SelectBlueprint.window.Left = SystemParameters.PrimaryScreenWidth / 2 - SelectBlueprint.window.Width / 2;
            }
            if (!MySettings.Current.MultiWindow) SelectBlueprint.window.Show();
            if(calc != null) calc.Clear();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            double.TryParse(StoneAmountText.Text, out double SAR);
            StoneAmountText.Text = SAR.ToString();
            if (calc != null)
            {
                calc.StoneAmount = SAR;
                calc.CalculateOres();
                OresList.ItemsSource = calc.GetOres();
                ListSort(OresList);
            }
        }

        private void ListSort(ListBox forSort)
        {
            forSort.Items.SortDescriptions.Clear();
            forSort.Items.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string undef = calc.GetUndefined();
            if (!string.IsNullOrEmpty(undef))
            {
                new MessageDialog(DialogPicture.attention, "Attention", Lang.UndefinedTypesExists + "\r\n" + undef, null, DialogType.Message).Show();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Comps = "";
            foreach(MyResourceInfo x in ComponensList.Items)
            {
                Comps += x.Type + ": " + x.Count + "\r\n";
            }
            string Ings = "";
            foreach (MyResourceInfo x in IngotsList.Items)
            {
                Ings += x.Type + ": " + x.Count + "\r\n";
            }
            string Ors = "";
            foreach (MyResourceInfo x in OresList.Items)
            {
                Ors += x.Type + ": " + x.Count + "\r\n";
            }
            string[] hh = Lang.StoneAmount.Split('(');
            Clipboard.SetText("SE BlueprintEditor - Calculator\r\n"+
                Lang.Blueprint + " - " + EdBlueprint.Patch.Split('\\').Last() + "\r\n\r\n" +
                
                Lang.AssemblerEfficiency + " - " + AssembleEffic.Value.ToString("0") + "x\r\n" +
                Lang.YieldModules + " - " + YieldCount.Value.ToString("0") + "\r\n" +
                hh[0].Trim(' ') + ": "+ StoneAmountText.Text+ hh[1].Trim(' ',')') + "\r\n\r\n" +
                
                Lang.Components+":\r\n"+ Comps + "\r\n" +
                Lang.Ingots + ":\r\n" + Ings + "\r\n" +
                Lang.Ores + ":\r\n" + Ors);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (calc != null)
            {
                calc.AssemblerEffic = (int)AssembleEffic.Value;
                calc.YieldModules = (int)YieldCount.Value;
                calc.CalculateIngots();
                calc.CalculateOres();
                IngotsList.ItemsSource = calc.GetIngots();
                OresList.ItemsSource = calc.GetOres();
                ListSort(IngotsList);
                ListSort(OresList);
            }
            Slider Sender = (Slider)sender;
            Sender.Value = Math.Round(Sender.Value);
        }
    }
}
