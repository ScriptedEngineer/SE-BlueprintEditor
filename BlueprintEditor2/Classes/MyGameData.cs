using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using System.ComponentModel;

namespace BlueprintEditor2
{
    static class MyGameData
    {
        public static Dictionary<string, MyBlockRecipie> CubeBlocks = new Dictionary<string, MyBlockRecipie>();
        public static Dictionary<string, Dictionary<string, MyBlockRecipie>> ModCubeBlocks = new Dictionary<string, Dictionary<string, MyBlockRecipie>>();
        
        public static Dictionary<string, Dictionary<string, double>> Recipies = new Dictionary<string, Dictionary<string, double>>();
        public static Dictionary<string, Dictionary<string, Dictionary<string, double>>> ModRecipies = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
        
        public static Dictionary<string, MyComponentInfo> Components = new Dictionary<string, MyComponentInfo>();
        public static Dictionary<string, Dictionary<string, MyComponentInfo>> ModComponents = new Dictionary<string, Dictionary<string, MyComponentInfo>>();

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
                    foreach (var x in Directory.GetFiles(gamePatch + @"Content\Data", "Components*"))
                    {
                        AddComponentsInfo(x);
                    }
                    foreach (var x in Directory.GetFiles(gamePatch + @"Content\Data", "PhysicalItems*"))
                    {
                        AddComponentsInfo(x);
                    }
                }
                //Mods
                try
                {
                    foreach (string mod in WorkshopCache.GetModsForCalculator())
                    {
                        foreach (var x in Directory.GetFiles(mod + @"\Data", "*.sbc", SearchOption.AllDirectories))
                        {
                            string modid = Path.GetFileName(mod);
                            //Console.WriteLine(x);
                            try
                            {
                                XmlDocument File = new XmlDocument();
                                File.Load(x);
                                AddBlocksInfo(x, File, true, modid);
                                AddRecipiesInfo(x, File, true, modid);
                                AddComponentsInfo(x, File, true, modid);
                            }
                            catch { }
                        }
                    }
                    foreach (var key in ModCubeBlocks.Keys)
                    {
                        if (ModCubeBlocks[key].Count == 0)
                            ModCubeBlocks.Remove(key);
                    }
                    foreach (var key in ModRecipies.Keys)
                    {
                        if (ModRecipies[key].Count == 0)
                            ModRecipies.Remove(key);
                    }
                }
                finally
                {
                    IsInitialized = true;
                }
            }
        }
        private static void AddBlocksInfo(string file, XmlDocument File = null, bool mods = false, string modid = "0")
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
                int PCU = 0;
                Vector3 Size = new Vector3(1,1,1);
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
                        case "PCU":
                            int.TryParse(z.InnerText, out PCU);
                            break;
                        case "Size":
                            try
                            {
                                var Atrs = z.Attributes;
                                int.TryParse(Atrs.GetNamedItem("x").Value, out int X);
                                int.TryParse(Atrs.GetNamedItem("y").Value, out int Y);
                                int.TryParse(Atrs.GetNamedItem("z").Value, out int Z);
                                Size = new Vector3(X, Y, Z);
                            }
                            catch
                            {
                                int X = 1, Y = 1, Z = 1;
                                foreach (XmlNode s in z.ChildNodes)
                                    switch (s.Name)
                                    {
                                        case "X": int.TryParse(s.InnerText, out X); break;
                                        case "Y": int.TryParse(s.InnerText, out Y); break;
                                        case "Z": int.TryParse(s.InnerText, out Z); break;
                                    }
                                Size = new Vector3(X, Y, Z);
                            }
                            break;

                    }
                }
                name = name.Replace("MyObjectBuilder_", "");
                if (components.Count > 0)
                    if (mods)
                    {
                        if (ModCubeBlocks.ContainsKey(modid))
                        {
                            if (ModCubeBlocks[modid].ContainsKey(name))
                                ModCubeBlocks[modid][name] = new MyBlockRecipie(name, components, PCU, Size);
                            else
                                ModCubeBlocks[modid].Add(name, new MyBlockRecipie(name, components, PCU, Size));
                        }
                        else
                            ModCubeBlocks.Add(modid, new Dictionary<string, MyBlockRecipie>() { [name] = new MyBlockRecipie(name, components, PCU, Size) });
                    }
                    else
                    {
                        if (CubeBlocks.ContainsKey(name))
                            CubeBlocks[name] = new MyBlockRecipie(name, components, PCU, Size);
                        else
                            CubeBlocks.Add(name, new MyBlockRecipie(name, components, PCU, Size));
                    }
            }
        }
        private static void AddRecipiesInfo(string file, XmlDocument File = null, bool mods = false, string modid = "0")
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
                            if (requares.First().Key.EndsWith("/Scrap") ||
                                requared.ContainsKey(d.Key) ||
                                requared.ContainsKey("Ore/Ice"))
                            {
                                continue;
                            }
                            if (requared.Count > 0)
                                if (mods)
                                {
                                    if (ModRecipies.ContainsKey(modid))
                                    {
                                        if (!ModRecipies[modid].ContainsKey(d.Key))
                                            ModRecipies[modid].Add(d.Key, requared);
                                        else
                                            ModRecipies[modid][d.Key] = requared;
                                    }
                                    else
                                        ModRecipies.Add(modid, new Dictionary<string, Dictionary<string, double>>() { [d.Key] = requared });
                                }
                                else
                                {
                                    if (!Recipies.ContainsKey(d.Key))
                                        Recipies.Add(d.Key, requared);
                                    else if (requared.Count != 0 && d.Key != "Ingot/Stone")
                                        Recipies[d.Key] = requared;
                                }
                        }
                        break;
                }
            }
        }
        private static void AddComponentsInfo(string file, XmlDocument File = null, bool mods = false, string modid = "0")
        {
            if (File == null)
            {
                File = new XmlDocument();
                File.Load(file);
            }
            foreach (XmlNode y in File.GetElementsByTagName("Component"))
            {
                string name = "", type = "";
                float mass = 0, volume = 0;
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
                        case "Mass":
                            float.TryParse(z.InnerText.Replace('.', ','), out mass);
                            break;
                        case "Volume":
                            float.TryParse(z.InnerText.Replace('.', ','), out volume);
                            break;
                    }
                }
                //name = name.Replace("MyObjectBuilder_", "");
                if (mods)
                {
                    if (Names.ContainsKey(type))
                        Names[type] = name;
                    else
                        Names.Add(type, name);

                    if (ModComponents.ContainsKey(modid))
                    {
                        if (ModComponents[modid].ContainsKey(type))
                            ModComponents[modid][type] = new MyComponentInfo(name, type, mass, volume);
                        else
                            ModComponents[modid].Add(type, new MyComponentInfo(name, type, mass, volume));
                    }
                    else
                        ModComponents.Add(modid, new Dictionary<string, MyComponentInfo>() { [type] = new MyComponentInfo(name, type, mass, volume) });
                }
                else
                {
                    if (Components.ContainsKey(type))
                        Components[type] = new MyComponentInfo(name, type, mass, volume);
                    else
                        Components.Add(type, new MyComponentInfo(name, type, mass, volume));
                }
            }
            foreach (XmlNode y in File.GetElementsByTagName("PhysicalItem"))
            {
                string name = "", type = "";
                float mass = 0, volume = 0;
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
                        case "Mass":
                            float.TryParse(z.InnerText.Replace('.',','), out mass);
                            break;
                        case "Volume":
                            float.TryParse(z.InnerText.Replace('.', ','), out volume);
                            break;
                    }
                }
                //name = name.Replace("MyObjectBuilder_", "");
                if (mods)
                {
                    if (Names.ContainsKey(type))
                        Names[type] = name;
                    else
                        Names.Add(type, name);
                    if (ModComponents.ContainsKey(modid))
                    {
                        if (ModComponents[modid].ContainsKey(type))
                            ModComponents[modid][type] = new MyComponentInfo(name, type, mass, volume);
                        else
                            ModComponents[modid].Add(type, new MyComponentInfo(name, type, mass, volume));
                    }
                    else
                        ModComponents.Add(modid, new Dictionary<string, MyComponentInfo>() { [type] = new MyComponentInfo(name, type, 0, 0) });
                }
                else
                {
                    if (Components.ContainsKey(type))
                        Components[type] = new MyComponentInfo(name, type, 0, 0);
                    else
                        Components.Add(type, new MyComponentInfo(name, type, 0, 0));
                }

            }
        }
    }
    public class MyBlockRecipie
    {
        public readonly string Name;
        public readonly Dictionary<string, int> Components;
        public readonly int PCU;
        public readonly Vector3 Size;
        public MyBlockRecipie(string name, Dictionary<string, int> components, int pcu, Vector3 size)
        {
            Name = name;
            Components = components;
            PCU = pcu;
            Size = size;
        }
    }
    public class MyComponentInfo
    {
        public readonly string Type;
        public readonly string Name;
        public readonly float Mass;
        public readonly float Volume;
        public MyComponentInfo(string name, string type, float mass, float volume)
        {
            Name = name;
            Type = type;
            Mass = mass;
            Volume = volume;
        }
    }
}
