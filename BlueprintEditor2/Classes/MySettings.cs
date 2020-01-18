using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BlueprintEditor2
{
    [DataContract(Name = "SE_BlueprintEditor2_Settings")]
    public class MySettings
    {
        private const string FILE_PATH = "settings.xml";

        public static MySettings Current = new MySettings();
        [DataMember(Name = "BlueprintMainFolder")]
        public string BlueprintPatch = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpaceEngineers\Blueprints\local\";
        [DataMember(Name = "SavesMainFolder")]
        public string SavesPatch = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpaceEngineers\Saves\";
        [DataMember(Name = "GameFolder")]
        public string GamePatch = null;
        [DataMember(Name = "SteamLibFolder")]
        public string SteamLib = null;
        [DataMember(Name = "UseMultipleWindows")]
        public bool MultiWindow = false;
        [DataMember(Name = "DontOpenBlueprintsOnScan")]
        public bool DOBS = false;
        [DataMember(Name = "LangCultureID")]
        public int LCID = 0;
        [DataMember(Name = "UserSteamID")]
        public string SteamID = "";
        [DataMember(Name = "UserName")]
        public string UserName = "";
        MySettings()
        {
            if (!File.Exists("settings.xml"))
            {
                LCID = Thread.CurrentThread.CurrentUICulture.LCID;
                try
                {
                    string Steam = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", "false").ToString();
                    GetGameFolder(Steam);
                    GetSteamAccount(Steam);
                }
                catch
                {

                }
            }
        }
        private void GetGameFolder(string Steam)
        {
            try
            {
                string Accounts = File.ReadAllText(Steam + @"\config\loginusers.vdf");
                if (Directory.Exists(Steam + @"\SteamApps\common\SpaceEngineers"))
                {
                    GamePatch = @"C:\Program Files (x86)\Steam\SteamApps\common\SpaceEngineers\";
                    SteamLib = @"C:\Program Files (x86)\Steam\";
                }
                else
                {
                    string Folders = File.ReadAllText(Steam + @"\SteamApps\libraryfolders.vdf");
                    foreach (Match x in Regex.Matches(Folders, @"\w\:\\[\\\w]*"))
                    {
                        if (x.Success && Directory.Exists(x + @"\SteamApps\common\SpaceEngineers"))
                        {
                            GamePatch = x.ToString().Replace(@"\\", @"\") + @"\SteamApps\common\SpaceEngineers\";
                            SteamLib = x.ToString().Replace(@"\\", @"\")+ @"\";
                            break;
                        }
                    }
                }
            }
            catch
            {

            }
        }
        private void GetSteamAccount(string Steam)
        {
            try
            {
                string Accounts = File.ReadAllText(Steam + @"\config\loginusers.vdf");
                foreach (string Account in Accounts.Split('}'))
                {
                    Match ID = Regex.Match(Account, @"\""(\d*)\""\s*\{");
                    Match Name = Regex.Match(Account, @"\""PersonaName\""\s*\""([\S ]*)\""", RegexOptions.IgnoreCase);
                    Match MostRecent = Regex.Match(Account, @"\""MostRecent\""\s*\""1\""",RegexOptions.IgnoreCase);
                    if (MostRecent.Success)
                    {
                        if (ID.Success)
                        {
                            SteamID = ID.Groups[1].Value;
                            SavesPatch = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpaceEngineers\Saves\"+ SteamID;
                        }
                        if (Name.Success)
                            UserName = Name.Groups[1].Value;
                    }
                }
            }
            catch
            {

            }
        }
        public void ApplySettings()
        {
            if(LCID != 0) Thread.CurrentThread.CurrentUICulture = new CultureInfo(LCID);
        }
        public static void Serialize()
        {
            DataContractSerializer formatter = new DataContractSerializer(typeof(MySettings));
            //new StreamWriter("settings.xml")
            using (Stream fs = new FileStream(FILE_PATH, FileMode.Create))
            {
                formatter.WriteObject(fs, Current);
            }
        }
        public static void Deserialize()
        {
            if (File.Exists("settings.xml"))
            {
                DataContractSerializer formatter = new DataContractSerializer(typeof(MySettings));
                using (Stream fs = new FileStream(FILE_PATH, FileMode.Open))
                {
                    Current = (MySettings)formatter.ReadObject(fs);
                    bool NoGamePatch = string.IsNullOrWhiteSpace(Current.GamePatch) || string.IsNullOrWhiteSpace(Current.SteamLib);
                    bool NoSteamData = string.IsNullOrWhiteSpace(Current.SteamID);
                    if (NoGamePatch || NoSteamData) {
                        string Steam = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", "false").ToString();
                        if(NoGamePatch) 
                            Current.GetGameFolder(Steam);
                        if(NoSteamData)
                            Current.GetSteamAccount(Steam);
                    }
                    if(string.IsNullOrWhiteSpace(Current.SavesPatch))
                    {
                        Current.SavesPatch = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpaceEngineers\Saves\" + Current.SteamID;
                    }
                }
            }
        }
    }
}
