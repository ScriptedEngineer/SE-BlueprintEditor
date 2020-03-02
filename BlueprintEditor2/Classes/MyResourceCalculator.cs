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
        static bool Mods = false;
        public bool OffStone = false;
        static Dictionary<string, Dictionary<string, int>> CubeBlocks = new Dictionary<string, Dictionary<string, int>>();
        static Dictionary<string, Dictionary<string,Dictionary<string, double>>> Recipies = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
        static Dictionary<string, double> StoneRicipie = new Dictionary<string, double>();
        static Dictionary<string, string> Names = new Dictionary<string, string>();
        public static bool IsInitialized = false;
        Dictionary<string, double> Requared = new Dictionary<string, double>();
        List<string> UndefinedTypes = new List<string>();
        public MyResourceCalculator(string gamePatch, bool loadMods)
        {
            StoneAmount = 0;
            YieldEffect = 1;
            AssemblerEffic = 1;
            if (!IsInitialized)
            {
                if (Directory.Exists(gamePatch + @"Content\Data\CubeBlocks"))
                {
                    foreach (var x in Directory.GetFiles(gamePatch + @"Content\Data\CubeBlocks"))
                    {
                        AddBlocksInfo(x);
                    }
                    foreach (var x in Directory.GetFiles(gamePatch + @"Content\Data", "Blueprints*"))
                    {
                        AddRecipiesInfo(x);
                    }
                    IsInitialized = true;
                }
            }
            if ((!IsInitialized && loadMods) || (loadMods && !Mods && IsInitialized))
            {
                foreach (string mod in WorkshopCache.GetModsForCalculator())
                {
                    //Console.WriteLine(mod);
                    foreach (var x in Directory.GetFiles(mod + @"\Data", "*.*", SearchOption.AllDirectories)
                                        .Where(s => s.EndsWith(".sbc")))
                    {
                        XmlDocument File = new XmlDocument();
                        File.Load(x);
                        AddBlocksInfo(x, File);
                        AddRecipiesInfo(x, File);
                        AddNames(x, File);
                    }
                }
            }
            Mods = loadMods;
        }

        public void Clear()
        {
            Requared.Clear();
            UndefinedTypes = new List<string>();
            SelfStoneAmount = 0;
            StoneAmount = 0;
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

        public List<MyResourceInfo> GetComponents()
        {
            List<MyResourceInfo> outer = new List<MyResourceInfo>();
            foreach(var x in Requared.Where(x => x.Key.StartsWith("Component/")))
            {
                string ressng = Lang.ResourceManager.GetString(x.Key);
                if (String.IsNullOrEmpty(ressng))
                {
                    if (Names.ContainsKey(x.Key))
                        ressng = Names[x.Key];
                    else
                        ressng = x.Key.Replace("Component/", "");
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
                if (String.IsNullOrEmpty(ressng))
                {
                    if (Names.ContainsKey(x.Key))
                        ressng = Names[x.Key];
                    else
                        ressng = x.Key.Replace("Ingot/", "");
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
                if (String.IsNullOrEmpty(ressng))
                {
                    if (Names.ContainsKey(x.Key))
                        ressng = Names[x.Key];
                    else
                        ressng = x.Key.Replace("Ore/", "");
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
            if (CubeBlocks.ContainsKey(block.DisplayType))
                foreach (var x in CubeBlocks[block.DisplayType])
                {
                    if (Requared.ContainsKey(x.Key))
                    {
                        Requared[x.Key] += x.Value;
                    }
                    else
                    {
                        Requared.Add(x.Key, x.Value);
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
                if (Recipies.ContainsKey(x.Key)) {
                    var rec = Recipies[x.Key];
                    foreach (var y in rec[rec.Keys.Last()])
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
                foreach (var x in StoneRicipie)
                {
                    if (requaredIngots.ContainsKey(x.Key))
                    {
                        requaredIngots[x.Key] -= x.Value *(StoneAmount>SelfStoneAmount?StoneAmount:SelfStoneAmount);
                    }
                }
            }
            foreach (var x in requaredIngots)
            {
                if (Recipies.ContainsKey(x.Key))
                {
                    var rec = Recipies[x.Key];
                    string kuau = rec.Keys.FirstOrDefault(y => y.Contains(x.Key == "Ingot/Stone"?"Ore":x.Key.Replace("Ingot/", "") + "1"));
                    foreach (var y in rec[(string.IsNullOrEmpty(kuau) ? rec.Keys.First() : kuau)])
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
                    SelfStoneAmount = Requared["Ore/Stone"];
                    Recalculate = true;
                }
                else if (Requared.ContainsKey("Ore/Stone") && Requared["Ore/Stone"] == 0)
                {
                    Requared["Ore/Stone"] = Math.Max(0, SelfStoneAmount - StoneAmount);
                }
            if(Recalculate)
                CalculateOres();
        }

        private void AddBlocksInfo(string file, XmlDocument File = null)
        {
            if (File == null)
            {
                File = new XmlDocument();
                File.Load(file);
            }
            foreach (XmlNode y in File.GetElementsByTagName("Definition"))
            {
                string name = "";
                Dictionary<string, int> components = new Dictionary<string, int>();
                foreach (XmlNode z in y.ChildNodes)
                {
                    switch (z.Name)
                    {
                        case "Id":
                            foreach (XmlNode h in z.ChildNodes)
                            {
                                switch (h.Name)
                                {
                                    case "TypeId":
                                        name = h.InnerText + name;
                                        break;
                                    case "SubtypeId":
                                        name += "/" + h.InnerText;
                                        break;
                                }
                            }
                            break;
                        case "Components":
                            foreach (XmlNode h in z.ChildNodes)
                            {
                                if (h.Attributes == null) continue;
                                int.TryParse(h.Attributes.GetNamedItem("Count")?.Value, out int res);
                                string comp = string.Format("Component/{0}", h.Attributes.GetNamedItem("Subtype")?.Value);
                                if (components.ContainsKey(comp))
                                {
                                    components[comp] += res;
                                }
                                else
                                {
                                    components.Add(comp, res);
                                }
                            }
                            break;
                    }
                }
                name = name.Replace("MyObjectBuilder_", "");
                if (CubeBlocks.ContainsKey(name))
                    CubeBlocks[name] = components;
                else
                    CubeBlocks.Add(name, components);
            }
        }
        private void AddRecipiesInfo(string file, XmlDocument File = null)
        {
            if (File == null)
            {
                File = new XmlDocument();
                File.Load(file);
            }
            foreach (XmlNode y in File.GetElementsByTagName("Blueprint"))
            {
                string name = "";
                Dictionary<string, double> results = new Dictionary<string, double>();
                Dictionary<string, double> requares = new Dictionary<string, double>();
                foreach (XmlNode z in y.ChildNodes)
                {
                    switch (z.Name)
                    {
                        case "Id":
                            foreach (XmlNode h in z.ChildNodes)
                            {
                                switch (h.Name)
                                {
                                    case "TypeId":
                                        name = h.InnerText + name;
                                        break;
                                    case "SubtypeId":
                                        name += "/" + h.InnerText;
                                        break;
                                }
                            }
                            break;
                        case "Prerequisites":
                            foreach (XmlNode h in z.ChildNodes)
                            {
                                if (h.Attributes == null) continue;
                                double.TryParse(h.Attributes.GetNamedItem("Amount").Value.Replace(".", ","), out double res);
                                string comp = h.Attributes.GetNamedItem("TypeId").Value + "/" + h.Attributes.GetNamedItem("SubtypeId").Value;
                                if (requares.ContainsKey(comp))
                                {
                                    requares[comp] += res;
                                }
                                else
                                {
                                    requares.Add(comp, res);
                                }
                            }
                            break;
                        case "Results":
                            foreach (XmlNode h in z.ChildNodes)
                            {
                                if (h.Attributes == null) continue;
                                double.TryParse(h.Attributes.GetNamedItem("Amount").Value.Replace(".",","), out double res);
                                string comp = h.Attributes.GetNamedItem("TypeId").Value+"/"+ h.Attributes.GetNamedItem("SubtypeId").Value;
                                if (results.ContainsKey(comp))
                                {
                                    results[comp] += res;
                                }
                                else
                                {
                                    results.Add(comp, res);
                                }
                            }
                            break;
                        case "Result":
                            if (z.Attributes == null) continue;
                            double.TryParse(z.Attributes.GetNamedItem("Amount").Value.Replace(".", ","), out double resx);
                            string compx = z.Attributes.GetNamedItem("TypeId").Value + "/" + z.Attributes.GetNamedItem("SubtypeId").Value;
                            if (results.ContainsKey(compx))
                            {
                                results[compx] += resx;
                            }
                            else
                            {
                                results.Add(compx, resx);
                            }
                            break;
                    }
                }
                switch (name)
                {
                    case "BlueprintDefinition/StoneOreToIngot":
                        Dictionary<string, double> resultse = new Dictionary<string, double>();
                        foreach (string key in results.Keys)
                        {
                            resultse.Add(key, results[key]/requares.First().Value);
                        }
                        StoneRicipie = resultse;
                            break;
                    default:
                        foreach (var d in results)
                        {
                            if (requares.Count == 0)
                                continue;
                            Dictionary<string, double> requared = new Dictionary<string, double>();
                            foreach (string key in requares.Keys)
                            {
                                requared.Add(key, requares[key] / d.Value);
                            }

                            if (Recipies.ContainsKey(d.Key))
                            {
                                if (!Recipies[d.Key].ContainsKey(requares.First().Key + results.Count))
                                    Recipies[d.Key].Add(requares.First().Key + results.Count, requared);
                                else
                                    Recipies[d.Key][requares.First().Key + results.Count] = requared;
                            }
                            else
                            {
                                Recipies.Add(d.Key, new Dictionary<string, Dictionary<string, double>>() { { requares.First().Key + results.Count, requared } });
                            }

                        }
                        break;
                }
            }
        }
        private void AddNames(string file, XmlDocument File = null)
        {
            if (File == null)
            {
                File = new XmlDocument();
                File.Load(file);
            }
            foreach (XmlNode y in File.GetElementsByTagName("Component"))
            {
                string name = "", type = "";
                foreach (XmlNode z in y.ChildNodes)
                {
                    switch (z.Name)
                    {
                        case "Id":
                            foreach (XmlNode h in z.ChildNodes)
                            {
                                switch (h.Name)
                                {
                                    case "TypeId":
                                        type = h.InnerText + type;
                                        break;
                                    case "SubtypeId":
                                        type += "/" + h.InnerText;
                                        break;
                                }
                            }
                            break;
                        case "DisplayName":
                            name += z.InnerText;
                            break;
                    }
                }
                //name = name.Replace("MyObjectBuilder_", "");
                if (Names.ContainsKey(type))
                    Names[type] = name;
                else
                    Names.Add(type, name);
            }
            foreach (XmlNode y in File.GetElementsByTagName("PhysicalItem"))
            {
                string name = "", type = "";
                foreach (XmlNode z in y.ChildNodes)
                {
                    switch (z.Name)
                    {
                        case "Id":
                            foreach (XmlNode h in z.ChildNodes)
                            {
                                switch (h.Name)
                                {
                                    case "TypeId":
                                        type = h.InnerText + type;
                                        break;
                                    case "SubtypeId":
                                        type += "/" + h.InnerText;
                                        break;
                                }
                            }
                            break;
                        case "DisplayName":
                            name += z.InnerText;
                            break;
                    }
                }
                //name = name.Replace("MyObjectBuilder_", "");
                if (Names.ContainsKey(type))
                    Names[type] = name;
                else
                    Names.Add(type, name);
            }
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
