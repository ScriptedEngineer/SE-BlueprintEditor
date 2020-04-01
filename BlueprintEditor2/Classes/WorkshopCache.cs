using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlueprintEditor2
{
    public static class WorkshopCache
    {
        public static Dictionary<string, string> ModNames = new Dictionary<string, string>();
        public static void MoveBlueprintsToLocal()
        {
            string ModFolder = MySettings.Current.SteamWorkshopPatch;
            foreach (var fld in Directory.GetDirectories(ModFolder))
            {
                if (File.Exists(fld + @"\bp.sbc"))
                {
                    string ID = fld.Split('\\').Last();
                    string DirectoryName = MySettings.Current.BlueprintPatch + @"\" + ID;
                    if (!Directory.Exists(DirectoryName))
                        Directory.CreateDirectory(DirectoryName);
                    File.Copy(fld + @"\bp.sbc", DirectoryName + @"\bp.sbc", true);
                    if (File.Exists(fld + @"\thumb.png"))
                        File.Copy(fld + @"\thumb.png", DirectoryName + @"\thumb.png", true);
                }
            }
        }
        public static void ClearMods()
        {
            string ModFolder = MySettings.Current.SteamWorkshopPatch;
            if (string.IsNullOrEmpty(ModFolder))
                return;
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
                    MyExtensions.ClearFolder(fld);
                }
            }
        }
        public static string[] GetModsForCalculator()
        {
            string ModFolder = MySettings.Current.SteamWorkshopPatch;
            if (string.IsNullOrEmpty(ModFolder) || !Directory.Exists(ModFolder))
                return new string[0];
            List<string> Mods = new List<string>();
            int modCount = 0;
            StringBuilder modReq = new StringBuilder();
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
                    string midlfo = Path.GetFileName(fld);
                    ModNames.Add(midlfo, midlfo);
                    modReq.Append("&publishedfileids%5B").Append(modCount).Append("%5D=").Append(midlfo);
                    modCount++;
                    //string ID = fld.Split('\\').Last();
                    Mods.Add(fld);
                }
            }
            if (modCount != 0)
                new Task(() =>
                {
                    try
                    {
                        string info = MyExtensions.PostReq("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/",
                                        "itemcount=" + modCount + modReq);
                        var forx = Regex.Matches(info, @"""publishedfileid"":""(\d*)""[^{}]*""title"":""([^""]*)""");
                        ModNames.Clear();
                        foreach (Match x in forx)
                        {
                            ModNames.Add(x.Groups[1].Value, x.Groups[2].Value);
                        }
                    }
                    catch (Exception e) { Console.WriteLine(e.Message); }
                }).Start();
            return Mods.ToArray();
        }
        public static string GetModsForWorld()
        {
            string ModFolder = MySettings.Current.SteamWorkshopPatch;
            if (string.IsNullOrEmpty(ModFolder))
                return "";
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
