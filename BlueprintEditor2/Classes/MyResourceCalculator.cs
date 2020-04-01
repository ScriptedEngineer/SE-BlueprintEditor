using BlueprintEditor2.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlueprintEditor2
{
    public class MyResourceCalculator
    {
        public double StoneAmount { get; set; }
        public double YieldEffect { get; set; }
        public int AssemblerEffic { get; set; }
        private double SelfStoneAmount;
        public bool Mods = true;
        public bool OffStone = true;

        public Dictionary<string, Dictionary<string, int>> ModCubeBlocks = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<string, double>> ModRecipies = new Dictionary<string, Dictionary<string, double>>();


        Dictionary<string, double> RequaredBuidComp = new Dictionary<string, double>();
        Dictionary<string, double> Requared = new Dictionary<string, double>();
        List<string> UndefinedTypes = new List<string>();
        public MyResourceCalculator()
        {
            StoneAmount = 0;
            YieldEffect = 1;
            AssemblerEffic = 1;
            ModCubeBlocks.Clear();
            ModRecipies.Clear();
            foreach (var x in MyGameData.ModCubeBlocks)
            {
                ModCubeBlocks = ModCubeBlocks.Concat(x.Value.Where(n => !ModCubeBlocks.ContainsKey(n.Key))).ToDictionary(h => h.Key, h => h.Value); ;
            }
            foreach (var x in MyGameData.ModRecipies)
            {
                ModRecipies = ModRecipies.Concat(x.Value.Where(n => !ModRecipies.ContainsKey(n.Key))).ToDictionary(h => h.Key, h => h.Value); ;
            }
        }

        public void ModReEnable(Dictionary<string,MyModSwitch> Switches)
        {
            ModCubeBlocks.Clear();
            ModRecipies.Clear();
            foreach (var x in MyGameData.ModCubeBlocks)
            {
                if(Switches.ContainsKey(x.Key) && Switches[x.Key].Enabled)
                    ModCubeBlocks = ModCubeBlocks.Concat(x.Value.Where(n => !ModCubeBlocks.ContainsKey(n.Key))).ToDictionary(h => h.Key, h => h.Value); ;
            }
            foreach (var x in MyGameData.ModRecipies)
            {
                if (Switches.ContainsKey(x.Key) && Switches[x.Key].Enabled)
                    ModRecipies = ModRecipies.Concat(x.Value.Where(n => !ModRecipies.ContainsKey(n.Key))).ToDictionary(h => h.Key, h => h.Value); ;
            }
        }

        public void Clear()
        {
            Requared.Clear();
            RequaredBuidComp.Clear();
            UndefinedTypes = new List<string>();
            SelfStoneAmount = 0;
        }

        public void SetYieldEffect(double yieldEffect)
        {
            YieldEffect = yieldEffect;
        }
        public double SetYieldModules(int YieldModules)
        {
            YieldEffect = 1;
            switch (YieldModules)
            {
                case 1:
                    YieldEffect = 1.19;
                    break;
                case 2:
                    YieldEffect = 1.41;
                    break;
                case 3:
                    YieldEffect = 1.68;
                    break;
                case 4:
                    YieldEffect = 2;
                    break;
            }
            return YieldEffect;
        }
        string AddWeightCounters(double Num)
        {
            string Oute = Num.ToString("0.00") + " Kg";
            if (Num > 10000000000) Oute = (Num / 1000000000).ToString("0.00") + " MT";
            else if (Num > 10000000) Oute = (Num / 1000000).ToString("0.00") + " KT";
            else if (Num > 10000) Oute = (Num / 1000).ToString("0.00") + " T";
            else if (Num < 0.1) Oute = (Num * 1000).ToString("0.00") + " g";
            return Oute;
        }

        public List<MyResourceInfo> GetBuildComponents()
        {
            List<MyResourceInfo> outer = new List<MyResourceInfo>();
            foreach (var x in RequaredBuidComp.Where(x => x.Key.StartsWith("Component/")))
            {
                string ressng = Lang.ResourceManager.GetString(x.Key);
                if (string.IsNullOrEmpty(ressng))
                {
                    if (MyGameData.Names.ContainsKey(x.Key))
                        ressng = MyGameData.Names[x.Key];
                    else
                        ressng = x.Key.Replace("Component/", "");
                }
                outer.Add(new MyResourceInfo(ressng, (int)x.Value));
            }
            return outer;
        }
        public List<MyResourceInfo> GetComponents()
        {
            List<MyResourceInfo> outer = new List<MyResourceInfo>();
            foreach(var x in Requared.Where(x => x.Key.StartsWith("Component/")))
            {
                string ressng = Lang.ResourceManager.GetString(x.Key);
                if (string.IsNullOrEmpty(ressng))
                {
                    if (MyGameData.Names.ContainsKey(x.Key))
                        ressng = MyGameData.Names[x.Key];
                    else
                        ressng = x.Key.Replace("Component/", "");
                }
                if (ressng.Contains("DisplayName"))
                {
                    string essng = ressng.Replace("DisplayName_Item_", "");
                    ressng = Lang.ResourceManager.GetString("Ingot/" + essng);
                    if (string.IsNullOrEmpty(ressng)) ressng = essng;
                }
                outer.Add(new MyResourceInfo(ressng, (int)x.Value));
            }
            return outer;
        }
        public List<MyResourceInfo> GetIngots()
        {
            List<MyResourceInfo> outer = new List<MyResourceInfo>();
            foreach (var x in Requared.Where(x => x.Key.StartsWith("Ingot/")))
            {
                string ressng = Lang.ResourceManager.GetString(x.Key);
                if (string.IsNullOrEmpty(ressng))
                {
                    if (MyGameData.Names.ContainsKey(x.Key))
                        ressng = MyGameData.Names[x.Key];
                    else
                        ressng = x.Key.Replace("Ingot/", "");
                }
                if (ressng.Contains("DisplayName"))
                {
                    string essng = ressng.Replace("DisplayName_Item_", "");
                    ressng = Lang.ResourceManager.GetString("Ingot/" + essng);
                    if (string.IsNullOrEmpty(ressng)) ressng = essng;
                }
                outer.Add(new MyResourceInfo(ressng, AddWeightCounters(x.Value)));
            }
            return outer;
        }
        public List<MyResourceInfo> GetOres()
        {
            List<MyResourceInfo> outer = new List<MyResourceInfo>();
            foreach (var x in Requared.Where(x =>x.Key.StartsWith("Ore/")))
            {
                string ressng = Lang.ResourceManager.GetString(x.Key);
                if (string.IsNullOrEmpty(ressng))
                {
                    if (MyGameData.Names.ContainsKey(x.Key))
                        ressng = MyGameData.Names[x.Key];
                    else
                        ressng = x.Key.Replace("Ore/", "");
                }
                if (ressng.Contains("DisplayName"))
                {
                    string essng = ressng.Replace("DisplayName_Item_", "");
                    ressng = Lang.ResourceManager.GetString("Ore/"+essng);
                    if (string.IsNullOrEmpty(ressng))ressng = essng;
                }
                outer.Add(new MyResourceInfo(ressng, AddWeightCounters(x.Value)));
            }
            return outer;
        }

        public string GetUndefined()
        {
            string outer = "";
            int counter = 0;
            foreach (var x in UndefinedTypes)
            {
                counter++;
                if(counter > 16)
                {
                    outer += x + "...";
                    break;
                }
                outer += x + "\r\n";
            }
            return outer;
        }

        public void AddBlock(MyXmlBlock block)
        {
            Dictionary<string,int> xx = null;
            if (Mods && ModCubeBlocks.ContainsKey(block.DisplayType))
               xx = ModCubeBlocks[block.DisplayType];
            else if (MyGameData.CubeBlocks.ContainsKey(block.DisplayType))
                xx = MyGameData.CubeBlocks[block.DisplayType];
            if (xx != null)
                foreach (var x in xx)
                {
                    if (Requared.ContainsKey(x.Key))
                    {
                        Requared[x.Key] += x.Value;
                    }
                    else
                    {
                        Requared.Add(x.Key, x.Value);
                    }
                    if (RequaredBuidComp.ContainsKey(x.Key))
                    {
                        RequaredBuidComp[x.Key] += x.Value;
                    }
                    else
                    {
                        RequaredBuidComp.Add(x.Key, x.Value);
                    }
                }
            else if (!UndefinedTypes.Contains(block.DisplayType))
            {
                UndefinedTypes.Add(block.DisplayType);
            }
        }
        public void CalculateIngots(Dictionary<string, double> ToRecalce = null, bool update = false)
        {
            bool Recalculate = false;
            Dictionary<string, double> ToRecalc = new Dictionary<string, double>();
            if (ToRecalce == null)
                foreach (var xu in Requared.Where(x => x.Key.StartsWith("Ingot/") || x.Key.StartsWith("Ore/")).ToArray())
                {
                    Requared.Remove(xu.Key);
                }
            foreach (var x in (ToRecalce != null?ToRecalce.ToArray() : Requared.Where(x => x.Key.StartsWith("Component/")).ToArray()))
            {
                Dictionary<string, double> xx = null;
                if (Mods && ModRecipies.ContainsKey(x.Key))
                    xx = ModRecipies[x.Key];
                else if (MyGameData.Recipies.ContainsKey(x.Key))
                    xx = MyGameData.Recipies[x.Key];
                if (xx != null) {
                    foreach (var y in xx)
                    {
                        if (y.Key.StartsWith("Component/"))
                        {
                            if (!update)
                            {
                                if (ToRecalc.ContainsKey(y.Key))
                                {
                                    ToRecalc[y.Key] += (y.Value * x.Value);
                                }
                                else
                                {
                                    ToRecalc.Add(y.Key, (y.Value * x.Value));
                                }
                                if (Requared.ContainsKey(y.Key))
                                {
                                    Requared[y.Key] += (y.Value * x.Value);
                                }
                                else
                                {
                                    Requared.Add(y.Key, (y.Value * x.Value));
                                }
                                Recalculate = true;
                            }
                            continue;
                        }
                        if (Requared.ContainsKey(y.Key))
                        {
                            Requared[y.Key] += (y.Value * x.Value) / AssemblerEffic;
                        }
                        else
                        {
                            Requared.Add(y.Key, (y.Value * x.Value) / AssemblerEffic);
                        }
                        /*if (Requared.ContainsKey("Ingot/Stone"))
                        {
                            //int sdax = 0;
                        }*/
                    }
                }
            }
            if (Recalculate)
                CalculateIngots(ToRecalc);
        }
        public void CalculateOres()
        {
            bool Recalculate = false;
            foreach (var xu in Requared.Where(x => x.Key.StartsWith("Ore/")).ToArray())
            {
                Requared.Remove(xu.Key);
            }
            Dictionary<string, double> requaredIngots = new Dictionary<string, double>();
            foreach (var x in Requared.Where(x => x.Key.StartsWith("Ingot/")).ToArray())
            {
                requaredIngots.Add(x.Key, x.Value);
            }
            if (!OffStone && (SelfStoneAmount > 0 || StoneAmount > 0))
            {
                foreach (var x in MyGameData.StoneRicipie)
                {
                    if (requaredIngots.ContainsKey(x.Key))
                    {
                        requaredIngots[x.Key] -= x.Value *(StoneAmount>SelfStoneAmount?StoneAmount:SelfStoneAmount);
                    }
                }
            }
            foreach (var x in requaredIngots)
            {
                Dictionary<string, double> xx = null;
                if (Mods && ModRecipies.ContainsKey(x.Key))
                    xx = ModRecipies[x.Key];
                else if (MyGameData.Recipies.ContainsKey(x.Key))
                    xx = MyGameData.Recipies[x.Key];
                if (xx != null) {
                    foreach (var y in xx)
                    {
                        if (y.Key.StartsWith("Component/"))
                        {
                            if (Requared.ContainsKey(y.Key))
                            {
                                Requared[y.Key] += (y.Value * x.Value) / AssemblerEffic;
                            }
                            else
                            {
                                Requared.Add(y.Key, (y.Value * x.Value) / AssemblerEffic);
                            }
                            Recalculate = true;
                            continue;
                        }
                        if (Requared.ContainsKey(y.Key))
                        {
                            Requared[y.Key] += Math.Max(0,(y.Value * x.Value)/ YieldEffect);
                        }
                        else
                        {
                            Requared.Add(y.Key, Math.Max(0, (y.Value * x.Value) / YieldEffect));
                        }
                    }
                }
            }
            if (!OffStone)
                if (Requared.ContainsKey("Ore/Stone") && Requared["Ore/Stone"] > 0)
                {
                    if (Math.Round(SelfStoneAmount) != Math.Round(Requared["Ore/Stone"]))
                    {
                        SelfStoneAmount = Requared["Ore/Stone"];
                        Recalculate = true;
                    }
                }
                else if (Requared.ContainsKey("Ore/Stone") && Requared["Ore/Stone"] == 0)
                {
                    Requared["Ore/Stone"] = Math.Max(0, SelfStoneAmount - StoneAmount);
                }
            if(Recalculate)
                CalculateOres();
        }
    }
    public class MyResourceInfo
    {
        public string Type { get; set; }
        public string Count { get; set; }
        public int Amount { get; set; }
        public MyResourceInfo(string t, string c)
        {
            Type = t;
            Count = c;
            Amount = 0;
        }
        public MyResourceInfo(string t, int ci)
        {
            Type = t;
            Count = ci.ToString();
            Amount = ci;
        }
    }
}
