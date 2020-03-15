using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace BlueprintEditor2
{
    static class MyGameData
    {
        public static Dictionary<string, Dictionary<string, int>> CubeBlocks = new Dictionary<string, Dictionary<string, int>>();
        public static Dictionary<string, Dictionary<string, int>> ModCubeBlocks = new Dictionary<string, Dictionary<string, int>>();
        public static Dictionary<string, Dictionary<string, double>> Recipies = new Dictionary<string, Dictionary<string, double>>();
        public static Dictionary<string, Dictionary<string, double>> ModRecipies = new Dictionary<string, Dictionary<string, double>>();
        public static Dictionary<string, double> StoneRicipie = new Dictionary<string, double>();
        public static Dictionary<string, string> Names = new Dictionary<string, string>();
        public static bool IsInitialized = false;
        public static void Init()
        {
            string gamePatch = MySettings.Current.GamePatch;
            if (!IsInitialized)
            {
                //Vanila
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
                }
                //Mods
                try
                {
                    foreach (string mod in WorkshopCache.GetModsForCalculator())
                    {
                        //Console.WriteLine(mod);
                        foreach (var x in Directory.GetFiles(mod + @"\Data", "*.*", SearchOption.AllDirectories)
                                            .Where(s => s.EndsWith(".sbc")))
                        {
                            XmlDocument File = new XmlDocument();
                            File.Load(x);
                            AddBlocksInfo(x, File, true);
                            AddRecipiesInfo(x, File, true);
                            AddNames(x, File);
                        }
                    }
                }
                finally
                {
                    IsInitialized = true;
                }
            }
        }
        private static void AddBlocksInfo(string file, XmlDocument File = null, bool mods = false)
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
                if (mods)
                {
                    if (ModCubeBlocks.ContainsKey(name))
                        ModCubeBlocks[name] = components;
                    else
                        ModCubeBlocks.Add(name, components);
                }
                else
                {
                    if (CubeBlocks.ContainsKey(name))
                        CubeBlocks[name] = components;
                    else
                        CubeBlocks.Add(name, components);
                }
            }
        }
        private static void AddRecipiesInfo(string file, XmlDocument File = null, bool mods = false)
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
                                double.TryParse(h.Attributes.GetNamedItem("Amount").Value.Replace(".", ","), out double res);
                                string comp = h.Attributes.GetNamedItem("TypeId").Value + "/" + h.Attributes.GetNamedItem("SubtypeId").Value;
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
                            resultse.Add(key, results[key] / requares.First().Value);
                        }
                        StoneRicipie = resultse;
                        
                        goto default;
                        //break;
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
                            if (requares.First().Key.EndsWith("/Scrap"))
                            {
                                continue;
                            }
                            if (mods)
                            {
                                if (!ModRecipies.ContainsKey(d.Key))
                                    ModRecipies.Add(d.Key, requared);
                                else if(!requared.ContainsKey("Ore/Ice"))
                                    ModRecipies[d.Key] = requared;
                            }
                            else
                            {
                                if (!Recipies.ContainsKey(d.Key))
                                    Recipies.Add(d.Key, requared);
                                else if(requared.Count != 0 && d.Key != "Ingot/Stone")
                                    Recipies[d.Key] = requared;
                                    
                            }
                        }
                        break;
                }
            }
        }
        private static void AddNames(string file, XmlDocument File = null)
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
}
