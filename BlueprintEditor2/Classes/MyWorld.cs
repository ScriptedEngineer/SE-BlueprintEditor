using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueprintEditor2
{
    public class MyWorld
    {
        public static void Create(string Name, string Mods)
        {
            string Patch = MySettings.Current.SavesPatch + @"\" + Name;
            int cntr = 1;
            while (Directory.Exists(Patch))
            {
                Patch = MySettings.Current.SavesPatch + @"\" + Name + cntr;
                cntr++;
            }
            Directory.CreateDirectory(Patch);

            File.WriteAllText(Patch + "\\Sandbox.sbc", Properties.Resources.Sandbox.Replace("<!--NameHere-->", Name));
            File.WriteAllText(Patch + "\\SANDBOX_0_0_0_.sbs", Properties.Resources.SANDBOX_0_0_0_);
            File.WriteAllText(Patch + "\\Sandbox_config.sbc", Properties.Resources.Sandbox_config.Replace("<!--ModsHere-->", Mods));
            Properties.Resources.thumbDefault.Save(Patch + "\\thumb.jpg");
        }
    }
}
