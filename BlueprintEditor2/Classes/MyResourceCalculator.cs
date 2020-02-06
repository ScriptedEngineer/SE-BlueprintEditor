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
        static Dictionary<string, Dictionary<string, int>> CubeBlocks = new Dictionary<string, Dictionary<string, int>>();
        static Dictionary<string, Dictionary<string,Dictionary<string, double>>> Recipies = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
        static Dictionary<string, double> StoneRicipie = new Dictionary<string, double>();
        public static bool IsInitialized = false;
        Dictionary<string, int> RequaredComponents = new Dictionary<string, int>();
        Dictionary<string, double> RequaredIngots = new Dictionary<string, double>();
        Dictionary<string, double> RequaredOres = new Dictionary<string, double>();
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
                    foreach (var x in Directory.GetFiles(mod + @"\Data", "*CubeBlocks*"))
                    {
                        if (x.EndsWith(".sbc"))
                            AddBlocksInfo(x);
                    }
                    foreach (var x in Directory.GetFiles(mod + @"\Data", "*Blueprints*"))
                    {
                        if (x.EndsWith(".sbc"))
                            AddRecipiesInfo(x);
                    }
                }
            }
            Mods = loadMods;
        }

        public void Clear()
        {
            RequaredComponents.Clear();
            RequaredIngots.Clear();
            RequaredOres.Clear();
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
            foreach(var x in RequaredComponents)
            {
                string ressng = Lang.ResourceManager.GetString("Component/"+x.Key);
                outer.Add(new MyResourceInfo((String.IsNullOrEmpty(ressng) ? x.Key : ressng), x.Value));
            }
            return outer;
        }
        public List<MyResourceInfo> GetIngots()
        {
            List<MyResourceInfo> outer = new List<MyResourceInfo>();
            foreach (var x in RequaredIngots)
            {
                string ressng = Lang.ResourceManager.GetString(x.Key);
                //outer += (String.IsNullOrEmpty(ressng) ? x.Key.Replace("Ingot/", "") : ressng) + " - " + x.Value + "kg\r\n";
                outer.Add(new MyResourceInfo((String.IsNullOrEmpty(ressng) ? x.Key.Replace("Ingot/", "") : ressng), AddWeightCounters(x.Value)));
            }
            return outer;
        }
        public List<MyResourceInfo> GetOres()
        {
            List<MyResourceInfo> outer = new List<MyResourceInfo>();
            foreach (var x in RequaredOres)
            {
                string ressng = Lang.ResourceManager.GetString(x.Key);
                //outer += (String.IsNullOrEmpty(ressng) ? x.Key.Replace("Ingot/", "") : ressng) + " - " + x.Value + "kg\r\n";
                outer.Add(new MyResourceInfo((String.IsNullOrEmpty(ressng) ? x.Key.Replace("Ore/", "") : ressng), AddWeightCounters(x.Value)));
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
                    if (RequaredComponents.ContainsKey(x.Key))
                    {
                        RequaredComponents[x.Key] += x.Value;
                    }
                    else
                    {
                        RequaredComponents.Add(x.Key, x.Value);
                    }
                }
            else if (!UndefinedTypes.Contains(block.DisplayType))
            {
                UndefinedTypes.Add(block.DisplayType);
            }
        }
        public void CalculateIngots()
        {
            RequaredIngots.Clear();
            RequaredOres.Clear();
            foreach (var x in RequaredComponents)
            {
                if (Recipies.ContainsKey("Component/"+x.Key)) {
                    var rec = Recipies["Component/" + x.Key];
                    foreach (var y in rec[rec.Keys.First()])
                    {
                        if (RequaredIngots.ContainsKey(y.Key))
                        {
                            RequaredIngots[y.Key] += (y.Value * x.Value) / AssemblerEffic;
                        }
                        else
                        {
                            RequaredIngots.Add(y.Key, (y.Value * x.Value) / AssemblerEffic);
                        }
                    } 
                }
            }
            
        }
        public void CalculateOres()
        {
            RequaredOres.Clear();
            Dictionary<string, double> requaredIngots = new Dictionary<string, double>();
            foreach (var x in RequaredIngots)
            {
                requaredIngots.Add(x.Key, x.Value);
            }
            if (SelfStoneAmount > 0 || StoneAmount > 0)
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
                        if (RequaredOres.ContainsKey(y.Key))
                        {
                            RequaredOres[y.Key] += Math.Max(0,(y.Value * x.Value)/ YieldEffect);
                        }
                        else
                        {
                            RequaredOres.Add(y.Key, Math.Max(0, (y.Value * x.Value) / YieldEffect));
                        }
                    }
                }
            }
            if (RequaredOres.ContainsKey("Ore/Stone") && RequaredOres["Ore/Stone"] > 0)
            {
                SelfStoneAmount = RequaredOres["Ore/Stone"];
                CalculateOres();
            }
            else if (RequaredOres.ContainsKey("Ore/Stone") && RequaredOres["Ore/Stone"] == 0)
            {
                RequaredOres["Ore/Stone"] = Math.Max(0, SelfStoneAmount - StoneAmount);
            }
        }

        private void AddBlocksInfo(string file)
        {
            XmlDocument File = new XmlDocument();
            File.Load(file);
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
                                string comp = h.Attributes.GetNamedItem("Subtype")?.Value;
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
        private void AddRecipiesInfo(string file)
        {
            XmlDocument File = new XmlDocument();
            File.Load(file);
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
                            Dictionary<string, double> requared = new Dictionary<string, double>();
                            foreach (string key in requares.Keys)
                            {
                                requared.Add(key, requares[key] / d.Value);
                            }

                            if (Recipies.ContainsKey(d.Key))
                            {
                                if (!Recipies[d.Key].ContainsKey(requares.First().Key + results.Count))
                                      Recipies[d.Key].Add(requares.First().Key + results.Count, requared);
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
