using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace BlueprintEditor2
{
    class MyBlockTypeReplacer
    {
        public string GroupName { get; private set; }
        public readonly string ReplaceType = "CubeBlock";
        public Dictionary<string, string> Unreplace = null;
        public Dictionary<string, string> Replace = new Dictionary<string, string>();
        public MyBlockTypeReplacer(string groupName, string replaceType)
        {
            GroupName = groupName;
            ReplaceType = replaceType;
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
            return new MyBlockTypeReplacer(group, type) { Replace = replacers };
        }
        public string Serialize()
        {
            StringBuilder Hoho = new StringBuilder();
            Hoho.Append("g[").Append(GroupName).Append("]\n");
            Hoho.Append("t[").Append(ReplaceType).Append("]\n");
            foreach (var xoxox in Replace)
            {
                Hoho.Append(xoxox.Value).Append(':').Append(xoxox.Key).Append('\n');
            }
            return Hoho.ToString();
        }
    }
    public static class ArmorReplaceClass
    {
        static Dictionary<string, MyBlockTypeReplacer> Replacers = new Dictionary<string,MyBlockTypeReplacer>() {
            {"Heavy",
            new MyBlockTypeReplacer("Armor","CubeBlock") {
            Replace = {
                // - Large armor - 
                {"LargeBlockArmorBlock",            "LargeHeavyBlockArmorBlock" },
                {"LargeBlockArmorSlope",            "LargeHeavyBlockArmorSlope" },
                {"LargeBlockArmorCorner",           "LargeHeavyBlockArmorCorner" },
                {"LargeBlockArmorCornerInv",        "LargeHeavyBlockArmorCornerInv" },

                {"LargeHalfArmorBlock",             "LargeHeavyHalfArmorBlock" },
                {"LargeHalfSlopeArmorBlock",        "LargeHeavyHalfSlopeArmorBlock" },

                {"LargeBlockArmorRoundSlope",       "LargeHeavyBlockArmorRoundSlope" },
                {"LargeBlockArmorRoundCorner",      "LargeHeavyBlockArmorRoundCorner" },
                {"LargeBlockArmorRoundCornerInv",   "LargeHeavyBlockArmorRoundCornerInv" },

                {"LargeBlockArmorSlope2Base",       "LargeHeavyBlockArmorSlope2Base" },
                {"LargeBlockArmorSlope2Tip",        "LargeHeavyBlockArmorSlope2Tip" },
                {"LargeBlockArmorCorner2Base",      "LargeHeavyBlockArmorCorner2Base" },
                {"LargeBlockArmorCorner2Tip",       "LargeHeavyBlockArmorCorner2Tip" },
                {"LargeBlockArmorInvCorner2Base",   "LargeHeavyBlockArmorInvCorner2Base" },
                {"LargeBlockArmorInvCorner2Tip",    "LargeHeavyBlockArmorInvCorner2Tip" },

                // - Small armor -
                {"SmallBlockArmorBlock",            "SmallHeavyBlockArmorBlock" },
                {"SmallBlockArmorSlope",            "SmallHeavyBlockArmorSlope" },
                {"SmallBlockArmorCorner",           "SmallHeavyBlockArmorCorner" },
                {"SmallBlockArmorCornerInv",        "SmallHeavyBlockArmorCornerInv" },

                {"HalfArmorBlock",                  "HeavyHalfArmorBlock" },
                {"HalfSlopeArmorBlock",             "HeavyHalfSlopeArmorBlock" },

                {"SmallBlockArmorRoundSlope",       "SmallHeavyBlockArmorRoundSlope" },
                {"SmallBlockArmorRoundCorner",      "SmallHeavyBlockArmorRoundCorner" },
                {"SmallBlockArmorRoundCornerInv",   "SmallHeavyBlockArmorRoundCornerInv" },

                {"SmallBlockArmorSlope2Base",       "SmallHeavyBlockArmorSlope2Base" },
                {"SmallBlockArmorSlope2Tip",        "SmallHeavyBlockArmorSlope2Tip" },
                {"SmallBlockArmorCorner2Base",      "SmallHeavyBlockArmorCorner2Base" },
                {"SmallBlockArmorCorner2Tip",       "SmallHeavyBlockArmorCorner2Tip" },
                {"SmallBlockArmorInvCorner2Base",   "SmallHeavyBlockArmorInvCorner2Base" },
                {"SmallBlockArmorInvCorner2Tip",    "SmallHeavyBlockArmorInvCorner2Tip" },
                }
            }
            }
        };
        public static string Replace(string input, string toType)
        {
            string pre_replace = input.Replace("CubeBlock/", "");
            foreach (var x in Replacers)
            {
                pre_replace = x.Value.ReplaceBackward(pre_replace);
            }
            if (toType != "Light" && Replacers.ContainsKey(toType))
            {
                MyBlockTypeReplacer replacere = Replacers[toType];
                pre_replace = replacere.ReplaceForward(pre_replace);
            }
            return "CubeBlock/"+pre_replace;
        }

    }
}
