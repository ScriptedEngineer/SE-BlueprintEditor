using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueprintEditor2
{
    public static class WorkshopCache
    {
        public static void MoveBlueprintsToLocal()
        {
            string ModFolder = MySettings.Current.SteamLib + @"\steamapps\workshop\content\244850";
            foreach (var fld in Directory.GetDirectories(ModFolder))
            {
                if (File.Exists(fld + @"\bp.sbc"))
                {
                    string ID = fld.Split('\\').Last();
                    string DirectoryName = MySettings.Current.BlueprintPatch + @"\" + ID;
                    if(!Directory.Exists(DirectoryName))
                        Directory.CreateDirectory(DirectoryName);
                    File.Copy(fld + @"\bp.sbc", DirectoryName + @"\bp.sbc", true);
                    if (File.Exists(fld + @"\thumb.png"))
                        File.Copy(fld + @"\thumb.png", DirectoryName + @"\thumb.png", true);
                    //MyExtensions.ClearFolder(fld);
                    //Directory.Delete(fld);
                }
            }
        }
        public static void Clear()
        {
            string ModFolder = MySettings.Current.SteamLib + @"\steamapps\workshop\content\244850";
            MyExtensions.ClearFolder(ModFolder);
        }
        public static string[] GetModsForCalculator()
        {
            string ModFolder = MySettings.Current.SteamLib + @"\steamapps\workshop\content\244850";
            List<string> Mods = new List<string>();
            foreach (var fld in Directory.GetDirectories(ModFolder))
            {
                string[] files = Directory.GetFiles(fld);
                bool IsMod = Directory.Exists($"{fld}\\Data");
                if (!IsMod && files.Length >= 1)
                {
                    foreach (var ink in files)
                    {
                        if (ink.EndsWith(".bin"))
                        {
                            try
                            {
                                ZipFile.ExtractToDirectory(ink, fld);
                            }
                            catch
                            {

                            }
                            IsMod = Directory.Exists($"{fld}\\Data");
                            if (IsMod) break;
                        }                      
                    }
                }
                if (IsMod)
                {
                    //string ID = fld.Split('\\').Last();
                    Mods.Add(fld);
                }
            }
            return Mods.ToArray();
        }
        public static string GetModsForWorld()
        {
            string ModFolder = MySettings.Current.SteamLib + @"\steamapps\workshop\content\244850";
            StringBuilder Mods = new StringBuilder();
            foreach (var fld in Directory.GetDirectories(ModFolder))
            {
                string[] files = Directory.GetFiles(fld);
                bool IsMod = Directory.Exists($"{fld}\\Data");
                if (!IsMod && files.Length >= 1)
                {
                    foreach (var ink in files)
                    {
                        if (ink.EndsWith(".bin"))
                            using (ZipArchive archive = new ZipArchive(File.OpenRead(ink)))
                            {
                                foreach (var x in archive.Entries)
                                {
                                    if (x.FullName.StartsWith("Data"))
                                    {
                                        IsMod = true;
                                        break;
                                    }
                                }
                            }
                        if (IsMod) break;
                    }
                }
                if (IsMod)
                {
                    string ID = fld.Split('\\').Last();
                    Mods.Append($"<ModItem FriendlyName=\"\"><Name>{ID}.sbm</Name><PublishedFileId>{ID}</PublishedFileId></ModItem>");
                }
            }
            return Mods.ToString();
        }
    }
}
