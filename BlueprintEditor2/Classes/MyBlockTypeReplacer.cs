using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

namespace BlueprintEditor2
{
    public class MyBlockTypeReplacer
    {
        public string GroupName { get; private set; }
        public Dictionary<string, string> Unreplace = null;
        public Dictionary<string, string> Replace = new Dictionary<string, string>();
        public MyBlockTypeReplacer(string groupName)//, string replaceType)
        {
            GroupName = groupName;
            //ReplaceType = replaceType;
        }
        public string ReplaceForward(string Type)
        {
            if (Replace.ContainsKey(Type))
            {
                return Replace[Type];
            }
            else return Type;
        }
        public string ReplaceBackward(string Type)
        {
            if (Unreplace == null)
            {
                Unreplace = new Dictionary<string, string>();
                Unreplace.Clear();
                foreach (var xoxox in Replace)
                {
                    Unreplace.Add(xoxox.Value, xoxox.Key);
                }
            }
            if (Unreplace.ContainsKey(Type))
            {
                return Unreplace[Type];
            }
            else return Type;
        }

        public static MyBlockTypeReplacer Deserialize(string Data)
        {
            string group = "", type = "";
            Dictionary<string, string> replacers = new Dictionary<string, string>();
            foreach (string replacer in Data.Split('\n'))
            {
                var repl = replacer.Trim();
                string[] pf = repl.Split(':');
                if (pf.Length > 1)
                    replacers.Add(pf[0], pf[1]);
                else if (repl.EndsWith("]"))
                {
                    if (repl.StartsWith("g["))
                        group = repl.Replace("g[", "[").Trim('[', ']');
                    if (repl.StartsWith("t["))
                        type = repl.Replace("t[", "[").Trim('[', ']');
                }
            }
            return new MyBlockTypeReplacer(group) { Replace = replacers };
        }
        public string Serialize()
        {
            StringBuilder Hoho = new StringBuilder();
            Hoho.Append("g[").Append(GroupName).Append("]\n");
            //Hoho.Append("t[").Append(ReplaceType).Append("]\n");
            foreach (var xoxox in Replace)
            {
                Hoho.Append(xoxox.Key).Append(':').Append(xoxox.Value).Append('\n');
            }
            return Hoho.ToString();
        }
    }
    public static class ArmorReplaceClass
    {
        static public readonly List<string> Baze;
        static public Dictionary<string, MyBlockTypeReplacer> Replacers = new Dictionary<string, MyBlockTypeReplacer>() {
            {"Heavy",
            new MyBlockTypeReplacer("Armor") {
            Replace = {
                // - Large armor - 
                {"CubeBlock/LargeBlockArmorBlock",            "CubeBlock/LargeHeavyBlockArmorBlock" },
                {"CubeBlock/LargeBlockArmorSlope",            "CubeBlock/LargeHeavyBlockArmorSlope" },
                {"CubeBlock/LargeBlockArmorCorner",           "CubeBlock/LargeHeavyBlockArmorCorner" },
                {"CubeBlock/LargeBlockArmorCornerInv",        "CubeBlock/LargeHeavyBlockArmorCornerInv" },

                {"CubeBlock/LargeHalfArmorBlock",             "CubeBlock/LargeHeavyHalfArmorBlock" },
                {"CubeBlock/LargeHalfSlopeArmorBlock",        "CubeBlock/LargeHeavyHalfSlopeArmorBlock" },

                {"CubeBlock/LargeBlockArmorRoundSlope",       "CubeBlock/LargeHeavyBlockArmorRoundSlope" },
                {"CubeBlock/LargeBlockArmorRoundCorner",      "CubeBlock/LargeHeavyBlockArmorRoundCorner" },
                {"CubeBlock/LargeBlockArmorRoundCornerInv",   "CubeBlock/LargeHeavyBlockArmorRoundCornerInv" },

                {"CubeBlock/LargeBlockArmorSlope2Base",       "CubeBlock/LargeHeavyBlockArmorSlope2Base" },
                {"CubeBlock/LargeBlockArmorSlope2Tip",        "CubeBlock/LargeHeavyBlockArmorSlope2Tip" },
                {"CubeBlock/LargeBlockArmorCorner2Base",      "CubeBlock/LargeHeavyBlockArmorCorner2Base" },
                {"CubeBlock/LargeBlockArmorCorner2Tip",       "CubeBlock/LargeHeavyBlockArmorCorner2Tip" },
                {"CubeBlock/LargeBlockArmorInvCorner2Base",   "CubeBlock/LargeHeavyBlockArmorInvCorner2Base" },
                {"CubeBlock/LargeBlockArmorInvCorner2Tip",    "CubeBlock/LargeHeavyBlockArmorInvCorner2Tip" },

                // - Small armor -
                {"CubeBlock/SmallBlockArmorBlock",            "CubeBlock/SmallHeavyBlockArmorBlock" },
                {"CubeBlock/SmallBlockArmorSlope",            "CubeBlock/SmallHeavyBlockArmorSlope" },
                {"CubeBlock/SmallBlockArmorCorner",           "CubeBlock/SmallHeavyBlockArmorCorner" },
                {"CubeBlock/SmallBlockArmorCornerInv",        "CubeBlock/SmallHeavyBlockArmorCornerInv" },

                {"CubeBlock/HalfArmorBlock",                  "CubeBlock/HeavyHalfArmorBlock" },
                {"CubeBlock/HalfSlopeArmorBlock",             "CubeBlock/HeavyHalfSlopeArmorBlock" },

                {"CubeBlock/SmallBlockArmorRoundSlope",       "CubeBlock/SmallHeavyBlockArmorRoundSlope" },
                {"CubeBlock/SmallBlockArmorRoundCorner",      "CubeBlock/SmallHeavyBlockArmorRoundCorner" },
                {"CubeBlock/SmallBlockArmorRoundCornerInv",   "CubeBlock/SmallHeavyBlockArmorRoundCornerInv" },

                {"CubeBlock/SmallBlockArmorSlope2Base",       "CubeBlock/SmallHeavyBlockArmorSlope2Base" },
                {"CubeBlock/SmallBlockArmorSlope2Tip",        "CubeBlock/SmallHeavyBlockArmorSlope2Tip" },
                {"CubeBlock/SmallBlockArmorCorner2Base",      "CubeBlock/SmallHeavyBlockArmorCorner2Base" },
                {"CubeBlock/SmallBlockArmorCorner2Tip",       "CubeBlock/SmallHeavyBlockArmorCorner2Tip" },
                {"CubeBlock/SmallBlockArmorInvCorner2Base",   "CubeBlock/SmallHeavyBlockArmorInvCorner2Base" },
                {"CubeBlock/SmallBlockArmorInvCorner2Tip",    "CubeBlock/SmallHeavyBlockArmorInvCorner2Tip" },
                }
            }
            }
        };
        static ArmorReplaceClass(){
            Baze = Replacers["Heavy"].Replace.Keys.ToList();
        }
        public static string Replace(string input, string toType)
        {
            string pre_replace = input;
            foreach (var x in Replacers)
            {
                pre_replace = x.Value.ReplaceBackward(pre_replace);
            }
            if (toType != "Light" && Replacers.ContainsKey(toType))
            {
                MyBlockTypeReplacer replacere = Replacers[toType];
                pre_replace = replacere.ReplaceForward(pre_replace);
            }
            return pre_replace;
        }
        public static bool AddEmptyReplacer(string Name)
        {
            if (Replacers.ContainsKey(Name)) return false;
            Replacers.Add(Name, new MyBlockTypeReplacer("Armor"));
            foreach (var x in Baze)
            {
                Replacers[Name].Replace.Add(x, "None ("+x+")");
            }
            return true;
        }
        public static void Serialize()
        {
            XmlSerializableDictionary<string, string> Xs = new XmlSerializableDictionary<string, string>();
            foreach(var x in Replacers)
            {
                if(x.Key != "Heavy")
                    Xs.Add(x.Key, x.Value.Serialize());
            }
            XmlSerializer formatter = new XmlSerializer(typeof(XmlSerializableDictionary<string, string>));
            //new StreamWriter("settings.xml")
            using (Stream fs = new FileStream("ArmorReplacers.xml", FileMode.Create))
            {
                formatter.Serialize(fs, Xs);
            }
        }
        public static void Deserialize()
        {
            XmlSerializableDictionary<string, string> Xs = new XmlSerializableDictionary<string, string>();
            
            XmlSerializer formatter = new XmlSerializer(typeof(XmlSerializableDictionary<string, string>));
            using (Stream fs = new FileStream("ArmorReplacers.xml", FileMode.Open))
            {
                Xs = (XmlSerializableDictionary<string, string>)formatter.Deserialize(fs);
            }
            foreach (var x in Xs)
            {
                try
                {
                    Replacers.Add(x.Key, MyBlockTypeReplacer.Deserialize(x.Value));
                }
                catch { }
            }
        }
    }
}
