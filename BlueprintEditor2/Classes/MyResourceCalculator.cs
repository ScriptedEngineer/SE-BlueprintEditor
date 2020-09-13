using BlueprintEditor2.Resource;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
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
        public int PCU = 0, Blocks = 0;
        public float Mass = 0, Storage = 0, PeakOut = 0, GenOut = 0;
        readonly List<MyEnergyBlockInfo> JumpDrives = new List<MyEnergyBlockInfo>();

        public Dictionary<Base6Directions, MyForceInfo> Forces = new Dictionary<Base6Directions, MyForceInfo>();

        public Dictionary<string, MyBlockInfo> ModCubeBlocks = new Dictionary<string, MyBlockInfo>();
        public Dictionary<string, Dictionary<string, double>> ModRecipies = new Dictionary<string, Dictionary<string, double>>();
        public Dictionary<string, MyComponentInfo> ModComponents = new Dictionary<string, MyComponentInfo>();
        
        readonly Dictionary<string, double> RequaredBuidComp = new Dictionary<string, double>();
        readonly Dictionary<string, double> Requared = new Dictionary<string, double>();
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
            ModComponents.Clear();
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
            foreach (var x in MyGameData.ModComponents)
            {
                if (Switches.ContainsKey(x.Key) && Switches[x.Key].Enabled)
                    ModComponents = ModComponents.Concat(x.Value.Where(n => !ModComponents.ContainsKey(n.Key))).ToDictionary(h => h.Key, h => h.Value); ;
            }
        }

        public void Clear()
        {
            Requared.Clear();
            RequaredBuidComp.Clear();
            JumpDrives.Clear();
            Forces.Clear();
            UndefinedTypes = new List<string>();
            SelfStoneAmount = 0;
            PCU = 0;
            Blocks = 0;
            Mass = 0;
            Storage = 0;
            PeakOut = 0;
            GenOut = 0;
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
            string Oute = Num.ToString("N2", CultureInfo.InvariantCulture) + " kg";
            if (Num > 10000000000) Oute = (Num / 1000000000).ToString("N2", CultureInfo.InvariantCulture) + " MT";
            else if (Num > 10000000) Oute = (Num / 1000000).ToString("N2", CultureInfo.InvariantCulture) + " KT";
            else if (Num > 10000) Oute = (Num / 1000).ToString("N2", CultureInfo.InvariantCulture) + " T";
            else if (Num < 0.1) Oute = (Num * 1000).ToString("N2", CultureInfo.InvariantCulture) + " g";
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
        //public string GetMass() => AddWeightCounters(Mass);
        public double GetJumpDistance()
        {
            double JumpDistance = 0;
            foreach(var x in JumpDrives)
            {
                if (x.Storage < Mass)
                {
                    double jumpDistance = (x.Output * x.Storage) / Mass;
                    if (jumpDistance > x.Output) 
                        jumpDistance = x.Output;
                    JumpDistance += jumpDistance;
                }
                else
                    JumpDistance += x.Output;
            }
            return JumpDistance;
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

        public MyBlockInfo AddBlock(MyXmlBlock block)
        {
            MyBlockInfo xx = null;
            if (Mods && ModCubeBlocks.ContainsKey(block.Type))
               xx = ModCubeBlocks[block.Type];
            else if (MyGameData.CubeBlocks.ContainsKey(block.Type))
                xx = MyGameData.CubeBlocks[block.Type];
            if (xx != null)
            {
                foreach (var x in xx.Components)
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
                    if (Mods && ModComponents.ContainsKey(x.Key))
                        Mass += ModComponents[x.Key].Mass * x.Value;
                    else if (MyGameData.Components.ContainsKey(x.Key))
                        Mass += MyGameData.Components[x.Key].Mass * x.Value;
                    else Console.WriteLine(x.Key);
                }
                PCU += xx.PCU;
                Blocks++;
            }
            else if (!UndefinedTypes.Contains(block.Type))
            {
                UndefinedTypes.Add(block.Type);
            }
            if (xx is MyThrustBlockInfo Thrust)
            {
                MyBlockOrientation orient = block.Orientation ?? new MyBlockOrientation();
                if (Forces.ContainsKey(orient.Forward))
                {
                    Forces[orient.Forward].Space += Thrust.Force * Thrust.SpaceEffectiveness;
                    Forces[orient.Forward].Planetary += Thrust.Force * Thrust.PlanetaryEffectiveness;
                }
                else
                {
                    Forces.Add(orient.Forward, new MyForceInfo(Thrust.Force * Thrust.SpaceEffectiveness, Thrust.Force * Thrust.PlanetaryEffectiveness));
                }
            }
            if (xx is MyEnergyBlockInfo EnergyBlock)
            {
                if (EnergyBlock.IsJumpDrive)
                {
                    JumpDrives.Add(EnergyBlock);
                }
                else
                {
                    Storage += EnergyBlock.Storage;
                    PeakOut += EnergyBlock.Output;
                    if (EnergyBlock.IsGenerator)
                    {
                        GenOut += EnergyBlock.Output;
                    }
                }
            }
            return xx;
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
            Count = ci.ToString("N0", CultureInfo.InvariantCulture);
            Amount = ci;
        }
    }
    public class MyForceInfo
    {
        public double Space;
        public double Planetary;
        public MyForceInfo(double space, double planetary)
        {
            Space = space;
            Planetary = planetary;
        }
    }
}
