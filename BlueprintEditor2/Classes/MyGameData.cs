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
using System.Management;

namespace BlueprintEditor2
{
    static class MyGameData
    {
        public static Dictionary<string, MyBlockRecipie> CubeBlocks = new Dictionary<string, MyBlockRecipie>();
        public static Dictionary<string, Dictionary<string, MyBlockRecipie>> ModCubeBlocks = new Dictionary<string, Dictionary<string, MyBlockRecipie>>();
        public static List<string> BlockTypes = new List<string>();

        public static Dictionary<string, Dictionary<string, double>> Recipies = new Dictionary<string, Dictionary<string, double>>();
        public static Dictionary<string, Dictionary<string, Dictionary<string, double>>> ModRecipies = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
        
        public static Dictionary<string, MyComponentInfo> Components = new Dictionary<string, MyComponentInfo>();
        public static Dictionary<string, Dictionary<string, MyComponentInfo>> ModComponents = new Dictionary<string, Dictionary<string, MyComponentInfo>>();
        public static List<string> ItemTypes = new List<string>();

        public static Dictionary<string, MyThrustInfo> ThrustTypes = new Dictionary<string, MyThrustInfo>();
        public static Dictionary<string, Dictionary<string, MyThrustInfo>> ModThrustTypes = new Dictionary<string, Dictionary<string, MyThrustInfo>>();
        public static Dictionary<string, MyEnergyBlockInfo> EnergyTypes = new Dictionary<string, MyEnergyBlockInfo>();
        public static Dictionary<string, Dictionary<string, MyEnergyBlockInfo>> ModEnergyTypes = new Dictionary<string, Dictionary<string, MyEnergyBlockInfo>>();

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
                    foreach (var x in Directory.GetFiles(gamePatch + @"Content\Data", "AmmoMagazines*"))
                    {
                        AddAmmoInfo(x);
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
                                AddAmmoInfo(x, File, true, modid);
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
                Dictionary<string, XmlNode> blockInfo = new Dictionary<string, XmlNode>();
                string name = ""; string resGroup = null;
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
                                        name = h.InnerText.Replace("MyObjectBuilder_","") + name;
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
                        case "ResourceSinkGroup":
                            if(string.IsNullOrEmpty(resGroup)) 
                                resGroup = z.InnerText;
                            break;
                        case "ResourceSourceGroup":
                            resGroup = z.InnerText;
                            break;
                        default:
                            if (!blockInfo.ContainsKey(z.Name))
                                blockInfo.Add(z.Name, z);
                            break;
                    }
                }
                //name = name.Replace("MyObjectBuilder_", "");
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
                if (!BlockTypes.Contains(name))
                    BlockTypes.Add(name);
                switch (resGroup)
                {
                    case "Thrust":
                        try
                        {
                            float sEf = 1 ,pEf = 1;
                            float.TryParse(blockInfo["ForceMagnitude"]?.InnerText.Replace(".", ","), out float force);
                            if (blockInfo.ContainsKey("EffectivenessAtMinInfluence"))
                                float.TryParse(blockInfo["EffectivenessAtMinInfluence"]?.InnerText.Replace(".",","), out sEf);
                            if (blockInfo.ContainsKey("EffectivenessAtMaxInfluence"))
                                float.TryParse(blockInfo["EffectivenessAtMaxInfluence"]?.InnerText.Replace(".", ","), out pEf);
                            bool nAt = false;
                            if(blockInfo.ContainsKey("NeedsAtmosphereForInfluence")) 
                                bool.TryParse(blockInfo["NeedsAtmosphereForInfluence"]?.InnerText, out nAt);
                            if (mods)
                            {
                                if (ModThrustTypes.ContainsKey(modid))
                                {
                                    if (ModThrustTypes[modid].ContainsKey(name))
                                        ModThrustTypes[modid][name] = new MyThrustInfo(force, sEf, pEf, nAt);
                                    else
                                        ModThrustTypes[modid].Add(name, new MyThrustInfo(force, sEf, pEf, nAt));
                                }
                                else
                                    ModThrustTypes.Add(modid, new Dictionary<string, MyThrustInfo>() { [name] = new MyThrustInfo(force, sEf, pEf, nAt) });
                            }
                            else
                            {
                                if (ThrustTypes.ContainsKey(name))
                                    ThrustTypes[name] = new MyThrustInfo(force, sEf, pEf, nAt);
                                else
                                    ThrustTypes.Add(name, new MyThrustInfo(force, sEf, pEf, nAt));
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        break;
                    case "SolarPanels":
                    case "Battery":
                    case "Reactors":
                    case "Charging":
                        float storage = 0, output = 0;
                        if (blockInfo.ContainsKey("MaxPowerOutput"))
                            float.TryParse(blockInfo["MaxPowerOutput"]?.InnerText.Replace(".", ","), out output);
                        if (blockInfo.ContainsKey("MaxStoredPower"))
                            float.TryParse(blockInfo["MaxStoredPower"]?.InnerText.Replace(".", ","), out storage);
                        if (resGroup == "Charging")
                        {
                            if (blockInfo.ContainsKey("MaxJumpDistance"))
                                float.TryParse(blockInfo["MaxJumpDistance"]?.InnerText.Replace(".", ","), out output);
                            if (blockInfo.ContainsKey("MaxJumpMass"))
                                float.TryParse(blockInfo["MaxJumpMass"]?.InnerText.Replace(".", ","), out storage);
                        }
                        if (output != 0 || resGroup == "Charging")
                        {
                            MyEnergyBlockInfo EnBlinf = new MyEnergyBlockInfo(!blockInfo.ContainsKey("RequiredPowerInput"), output, storage, resGroup == "Charging");
                            if (mods)
                            {
                                if (ModEnergyTypes.ContainsKey(modid))
                                {
                                    if (ModEnergyTypes[modid].ContainsKey(name))
                                        ModEnergyTypes[modid][name] = EnBlinf;
                                    else
                                        ModEnergyTypes[modid].Add(name, EnBlinf);
                                }
                                else
                                    ModEnergyTypes.Add(modid, new Dictionary<string, MyEnergyBlockInfo>() { [name] = EnBlinf });
                            }
                            else
                            {
                                if (EnergyTypes.ContainsKey(name))
                                    EnergyTypes[name] = EnBlinf;
                                else
                                    EnergyTypes.Add(name, EnBlinf);
                            }

                        }
                        break;
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
                if (!ItemTypes.Contains(type))
                    ItemTypes.Add(type);
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
                if (!ItemTypes.Contains(type))
                    ItemTypes.Add(type);
            }
        }
        private static void AddAmmoInfo(string file, XmlDocument File = null, bool mods = false, string modid = "0")
        {
            if (File == null)
            {
                File = new XmlDocument();
                File.Load(file);
            }
            foreach (XmlNode y in File.GetElementsByTagName("AmmoMagazine"))
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
                if (!ItemTypes.Contains(type))
                    ItemTypes.Add(type);
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
    public class MyThrustInfo
    {
        public readonly float Force;
        public readonly float SpaceEffectiveness;
        public readonly float PlanetaryEffectiveness;
        public readonly bool NeedAtmosphere;
        public MyThrustInfo(float force, float spaceEffectiveness, float planetaryEffectiveness, bool needAtmosphere)
        {
            Force = force;
            SpaceEffectiveness = spaceEffectiveness;
            PlanetaryEffectiveness = planetaryEffectiveness;
            NeedAtmosphere = needAtmosphere;
        }
    }
    public class MyEnergyBlockInfo
    {
        public readonly bool IsGenerator;
        public readonly bool IsJumpDrive;
        public readonly float Output;
        public readonly float Storage;
        public MyEnergyBlockInfo(bool isGenerator, float output, float storage = 0, bool isJump = false)
        {
            IsGenerator = isGenerator;
            IsJumpDrive = isJump;
            Output = output;
            Storage = storage;
        }
    }
    
}
