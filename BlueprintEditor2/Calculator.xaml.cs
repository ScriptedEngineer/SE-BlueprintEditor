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
        Dictionary<string, MyModSwitch> Mods = new Dictionary<string, MyModSwitch>();
        public Calculator(FileStream Lock, MyXmlBlueprint Blueprint)
        {
            _lock = Lock;
            EdBlueprint = Blueprint;
            InitializeComponent();
            Preloader.Visibility = Visibility.Visible;
            Title = "[" + EdBlueprint.Patch.Split('\\').Last() + "] Calculator - SE BlueprintEditor Loading...";
            BluePicture.Source = EdBlueprint.GetPic(false, false);
            EditBlueprint.OpenCount++;
            while (MySettings.Current.GamePatch == null)
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
                    }
                }
            }
            
            foreach (var xe in WorkshopCache.ModNames)
            {
                Mods.Add(xe.Key,new MyModSwitch(xe.Value, xe.Key));
                //ModsList.Items.Add(xe);
            }
            string[] swt = MySettings.Current.ModSwitches.Split('\n');
            foreach (var x in swt)
            {
                string[] lol = x.Split(':');
                if (lol.Length >= 2)
                {
                    if (Mods.ContainsKey(lol[0]))
                    {
                        bool.TryParse(lol[1], out bool lod);
                        Mods[lol[0]].Enabled = lod;
                    }
                }
            }
            ModsList.ItemsSource = Mods.Values;
            Calculate(true);
            /*new Task(() =>
            {
                calc = new MyResourceCalculator();
                if (calc != null)
                {
                    calc.ModReEnable(Mods);
                    foreach (MyXmlGrid Gr in Blueprint.Grids)
                    {
                        foreach (MyXmlBlock Bl in Gr.Blocks)
                        {
                            calc.AddBlock(Bl);
                        }
                    }
                    calc.CalculateIngots();
                    calc.CalculateOres();
                    MyExtensions.AsyncWorker(() =>
                    {
                        ComponensList.ItemsSource = calc.GetComponents();
                        IngotsList.ItemsSource = calc.GetIngots();
                        OresList.ItemsSource = calc.GetOres();
                        ListSort(ComponensList, "Amount", ListSortDirection.Descending);
                        ListSort(IngotsList);
                        ListSort(OresList);
                        Preloader.Visibility = Visibility.Hidden;
                        Title = "[" + EdBlueprint.Patch.Split('\\').Last() + "] Calculator - SE BlueprintEditor";
                        string undef = calc.GetUndefined();
                        if (!string.IsNullOrEmpty(undef))
                        {
                            Logger.Add("Undefined types message show");
                            new MessageDialog(DialogPicture.attention, "Attention", Lang.UndefinedTypesExists + "\r\n" + undef, null, DialogType.Message).Show();
                        }
                    });
                }
                else
                {
                    MyExtensions.AsyncWorker(() =>
                    {
                        Window_Closing(null, null);
                        Close();
                    });
                }
            }).Start();*/
            UpdatePatterns();
        }

        private void ApplySwitches(string sw)
        {
            string[] swt = sw.Split('\n');
            foreach (var x in swt)
            {
                string[] lol = x.Split(':');
                if (lol.Length >= 2)
                {
                    if (Mods.ContainsKey(lol[0]))
                    {
                        bool.TryParse(lol[1], out bool lod);
                        Mods[lol[0]].Enabled = lod;
                    }
                }
            }
            ModsList.ItemsSource = null;
            ModsList.ItemsSource = Mods.Values;
        }

        private void Calculate(bool showWarn = false)
        {
            bool modse = WithMods.IsChecked.Value;
            bool ofbks = OnlyForBuild.IsChecked.Value;
            new Task(() =>
            {
                if (calc != null)
                    calc.Clear();
                else
                    calc = new MyResourceCalculator();

                if (calc != null)
                {
                    calc.Mods = modse;
                    foreach (MyXmlGrid Gr in EdBlueprint.Grids)
                    {
                        foreach (MyXmlBlock Bl in Gr.Blocks)
                        {
                            calc.AddBlock(Bl);
                        }
                    }
                        calc.CalculateIngots();
                        calc.CalculateOres();
                    MyExtensions.AsyncWorker(() =>
                    {
                        ComponensList.ItemsSource = ofbks ? calc.GetBuildComponents():calc.GetComponents();
                        IngotsList.ItemsSource = ofbks ? null : calc.GetIngots();
                        OresList.ItemsSource = ofbks ? null : calc.GetOres();
                        ListSort(ComponensList, "Amount", ListSortDirection.Descending);
                        ListSort(IngotsList);
                        ListSort(OresList);
                        Preloader.Visibility = Visibility.Hidden;
                        Title = "[" + EdBlueprint.Patch.Split('\\').Last() + "] Calculator - SE BlueprintEditor";
                        string undef = calc.GetUndefined();
                        if (!string.IsNullOrEmpty(undef) && showWarn)
                        {
                            Logger.Add("Undefined types message show");
                            new MessageDialog(DialogPicture.attention, "Attention", Lang.UndefinedTypesExists + "\r\n" + undef, null, DialogType.Message).Show();
                        }
                    });
                }
                else
                {
                    MyExtensions.AsyncWorker(() =>
                    {
                        Window_Closing(null, null);
                        Close();
                    });
                }
            }).Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSwitches();
            _lock.Dispose();
            EditBlueprint.OpenCount--;
            if (EditBlueprint.OpenCount == 0)
            {
                //SelectBlueprint.window.Top = SystemParameters.PrimaryScreenHeight / 2 - SelectBlueprint.window.Height / 2;
                //SelectBlueprint.window.Left = SystemParameters.PrimaryScreenWidth / 2 - SelectBlueprint.window.Width / 2;
            }
            if (!MySettings.Current.MultiWindow) SelectBlueprint.window.Show();
            if(calc != null) calc.Clear();
            Logger.Add("Calculator closed");
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
            StoneAmountText.CaretIndex = StoneAmountText.Text.Length;
        }

        private void ListSort(ListBox forSort, string Sort = "Type", ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            forSort.Items.SortDescriptions.Clear();
            forSort.Items.SortDescriptions.Add(new SortDescription(Sort, sortDirection));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Logger.Add("Copy to clipboard");
            /*int CompsL = 0;
            foreach (MyResourceInfo x in ComponensList.Items)
            {
                if (x.Type.Length > CompsL)
                    CompsL = x.Type.Length;
            }
            int IngsL = 0;
            foreach (MyResourceInfo x in IngotsList.Items)
            {
                if (x.Type.Length > IngsL)
                    IngsL = x.Type.Length;
            }
            int OrsL = 0;
            foreach (MyResourceInfo x in OresList.Items)
            {
                if (x.Type.Length > OrsL)
                    OrsL = x.Type.Length;
            }*/
            
            string Comps = "";
            foreach(MyResourceInfo x in ComponensList.Items)
            {
                Comps += x.Type + ": " + x.Count + "\r\n";
            }
            string Ings = "";
            foreach (MyResourceInfo x in IngotsList.Items)
            {
                Ings += x.Type+ ": " + x.Count + "\r\n";
            }
            string Ors = "";
            foreach (MyResourceInfo x in OresList.Items)
            {
                Ors += x.Type + ": " + x.Count + "\r\n";
            }
            string Modsss = "";
            foreach (MyModSwitch x in ModsList.Items)
            {
                if(x.Enabled)
                    Modsss += x.ID+" - "+x.Name + "\r\n";
            }
            string undef = calc.GetUndefined();
            string[] hh = Lang.StoneAmount.Split('(');
            Clipboard.SetText("SE BlueprintEditor - Calculator\r\n"+
                Lang.Blueprint + " - " + EdBlueprint.Patch.Split('\\').Last() + "\r\n\r\n" +
                
                Lang.AssemblerEfficiency + " - " + AssembleEffic.Value.ToString("0") + "x\r\n" +
                (ModulesYield.IsChecked.Value?
                Lang.YieldModules + " - " + YieldCount.Value.ToString("0") + "\r\n":
                Lang.YieldProcentage + " - " + YieldProcentage.Text + "\r\n") +

                (OffStone.IsChecked.Value? "\r\n" : hh[0].Trim(' ') + ": "+ StoneAmountText.Text+ hh[1].Trim(' ',')') + "\r\n\r\n") +

                (WithMods.IsChecked.Value? Lang.WithMods + ":\r\n"+ Modsss + "\r\n" : "") +

                (string.IsNullOrEmpty(undef)?"": Lang.UndefinedTypesExists+ "\r\n" + undef+ "\r\n")+

                (OnlyForBuild.IsChecked.Value? 
                Lang.OnlyForBuild + "\r\n" +
                Lang.Components + ":\r\n" + Comps
                :
                Lang.Components+":\r\n"+ Comps + "\r\n" +
                Lang.Ingots + ":\r\n" + Ings + "\r\n" +
                Lang.Ores + ":\r\n" + Ors));
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (calc != null)
            {
                calc.AssemblerEffic = (int)AssembleEffic.Value;
                YieldProcentage.Text = (calc.SetYieldModules((int)YieldCount.Value)*100).ToString("F0")+"%";
                calc.CalculateIngots(null, true);
                calc.CalculateOres();
                ComponensList.ItemsSource = calc.GetComponents();
                IngotsList.ItemsSource = calc.GetIngots();
                OresList.ItemsSource = calc.GetOres();
                ListSort(IngotsList);
                ListSort(OresList);
            }
            Slider Sender = (Slider)sender;
            Sender.Value = Math.Round(Sender.Value);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            YieldCount.IsEnabled = true;
            YieldProcentage.IsEnabled = false;
            if (calc != null)
                YieldProcentage.Text = (calc.SetYieldModules((int)YieldCount.Value) * 100).ToString("F0") + "%";
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            YieldCount.IsEnabled = false;
            YieldProcentage.IsEnabled = true;
        }

        private void YieldProcentage_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProcentageYield != null && ProcentageYield.IsChecked.Value)
            {
                string x = YieldProcentage.Text.Replace("%", "");
                double.TryParse(x, out double SAR);
                YieldProcentage.Text = SAR.ToString("F0") + "%";
                if (SAR <= 0) SAR = 1;
                if (calc != null)
                {
                    calc.SetYieldEffect(SAR / 100);
                    calc.CalculateOres();
                    OresList.ItemsSource = calc.GetOres();
                    ListSort(OresList);
                }
                YieldProcentage.CaretIndex = YieldProcentage.Text.Length - 1;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (calc != null)
            {
                calc.OffStone = OffStone.IsChecked.Value;
                calc.CalculateOres();
                OresList.ItemsSource = calc.GetOres();
                ListSort(OresList);
            }
            if (OffStone.IsChecked.Value)
                StoneAmountText.IsEnabled = false;
            else 
                StoneAmountText.IsEnabled = true;
        }

        private void WithMods_Click(object sender, RoutedEventArgs e)
        {
            Preloader.Visibility = Visibility.Visible;
            Title = "[" + EdBlueprint.Patch.Split('\\').Last() + "] Calculator - SE BlueprintEditor Loading...";
            Calculate(true);
        }

        private void OnlyForBuild_Click(object sender, RoutedEventArgs e)
        {
            bool ofbks = OnlyForBuild.IsChecked.Value;
            ComponensList.ItemsSource = ofbks ? calc.GetBuildComponents() : calc.GetComponents();
            IngotsList.ItemsSource = ofbks ? null : calc.GetIngots();
            OresList.ItemsSource = ofbks ? null : calc.GetOres();
            ListSort(ComponensList, "Amount", ListSortDirection.Descending);
            ListSort(IngotsList);
            ListSort(OresList);

        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            Patterns.SelectedIndex = 0;
            CheckBox Sender = sender as CheckBox;
            string id = Sender.CommandParameter.ToString();
            if (Mods.ContainsKey(id))
            {
                Mods[id].Enabled = Sender.IsChecked.Value;
            }
            calc.ModReEnable(Mods);
            Calculate();
            SaveSwitches();
        }

        private void SaveSwitches(bool rew = false)
        {
            if(Patterns.SelectedIndex == 0 || rew)
                MySettings.Current.ModSwitches = GetSwitches();
        }
        private string GetSwitches()
        {
            StringBuilder Switches = new StringBuilder();
            Switches.Append("\n");
            foreach (var x in Mods)
            {
                Switches.Append(x.Key).Append(":").Append(x.Value.Enabled.ToString()).Append("\n");
            }
            return Switches.ToString();
        }

        private void UpdatePatterns()
        {
            Patterns.Items.Clear();
            Patterns.Items.Add(Lang.Current);
            foreach (var x in MySettings.Current.ModSwitchesPatterns)
            {
                Patterns.Items.Add(x.Key);
            }
            Patterns.SelectedIndex = 0;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new MessageDialog((x) => {
                if (x != Lang.Current && !string.IsNullOrEmpty(x))
                {
                    if (MySettings.Current.ModSwitchesPatterns.Keys.Contains(x))
                        MySettings.Current.ModSwitchesPatterns[x] = GetSwitches();
                    else
                        MySettings.Current.ModSwitchesPatterns.Add(x, GetSwitches());
                }
                UpdatePatterns();
            }, Lang.CreatePattern, Lang.EnterPatternName).Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string key = Patterns.SelectedItem.ToString();
            if (MySettings.Current.ModSwitchesPatterns.ContainsKey(key))
            {
                MySettings.Current.ModSwitchesPatterns.Remove(key);
            }
            UpdatePatterns();
        }

        int oldSelindx = -1;
        private void Patterns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (oldSelindx == Patterns.SelectedIndex || Patterns.SelectedIndex == -1) 
                return;
            if (oldSelindx == 0)
                SaveSwitches(true);
            if(Patterns.SelectedIndex == 0)
            {
                ApplySwitches(MySettings.Current.ModSwitches);
            }
            else
            {
                string key = Patterns.SelectedItem.ToString();
                if (MySettings.Current.ModSwitchesPatterns.ContainsKey(key))
                {
                    ApplySwitches(MySettings.Current.ModSwitchesPatterns[key]);
                }
            }
            oldSelindx = Patterns.SelectedIndex;
            if (calc != null)
            {
                calc.ModReEnable(Mods);
                Calculate();
            }
        }
    }
}
