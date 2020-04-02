using BlueprintEditor2.Resource;
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
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace BlueprintEditor2
{
    [XmlRoot("SE_BlueprintEditor2_Settings", Namespace = "https://wsxz.ru/BlueprintEditor",IsNullable = false)]
    [DataContract(Name = "SE_BlueprintEditor2_Settings")]
    public class MySettings
    {
        private const string FILE_PATH = "config.xml";

        public static MySettings Current = new MySettings();
        [DataMember(Name = "BlueprintMainFolder")]
        public string BlueprintPatch = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpaceEngineers\Blueprints\local\";
        [DataMember(Name = "SavesMainFolder")]
        public string SavesPatch = null;
        [DataMember(Name = "GameFolder")]
        public string GamePatch = null;
        [DataMember(Name = "SteamWorkshopCacheFolder")]
        public string SteamWorkshopPatch = null;
        [DataMember(Name = "UseMultipleWindows")]
        public bool MultiWindow = false;
        [DataMember(Name = "DontOpenBlueprintsOnScan")]
        public bool DontOpenBlueprintsOnScan = false;
        [DataMember(Name = "LangCultureID")]
        public int LangCultureID = 0;
        [DataMember(Name = "UserSteamID")]
        public string SteamID = "";
        [DataMember(Name = "UserName")]
        public string UserName = "";

        [DataMember(Name = "ModSwitches")]
        public string ModSwitches = "";
        [DataMember(Name = "ModSwitchesPatterns")]
        public XmlSerializableDictionary<string, string> ModSwitchesPatterns = new XmlSerializableDictionary<string, string>();
        MySettings()
        {
            if (!File.Exists(FILE_PATH))
            {
                LangCultureID = Thread.CurrentThread.CurrentUICulture.LCID;
                try
                {
                    if (!Directory.Exists(BlueprintPatch))
                    {
                        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                        {
                            dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                            dialog.Description = Lang.SelectBluePatchDesc;
                            dialog.ShowNewFolderButton = false;
                            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                            if (result.Equals(System.Windows.Forms.DialogResult.OK))
                            {
                                BlueprintPatch = dialog.SelectedPath + "\\";
                            }
                            else
                                Application.Current.Shutdown();
                        }
                    }
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
                if (Directory.Exists(Steam + @"\SteamApps\common\SpaceEngineers"))
                {
                    GamePatch = Steam + @"\SteamApps\common\SpaceEngineers\";
                    if (Directory.Exists(Steam + @"\steamapps\workshop\content\244850"))
                        SteamWorkshopPatch = Steam + @"\steamapps\workshop\content\244850\";
                }
                else
                {
                    string Folders = File.ReadAllText(Steam + @"\SteamApps\libraryfolders.vdf");
                    foreach (Match x in Regex.Matches(Folders, @"\w\:\\[\\\w]*"))
                    {
                        string xo = x.ToString().Replace(@"\\", @"\");
                        if (x.Success && Directory.Exists(xo + @"\SteamApps\common\SpaceEngineers"))
                        {
                            GamePatch = xo + @"\SteamApps\common\SpaceEngineers\";
                            if (Directory.Exists(xo + @"\steamapps\workshop\content\244850"))
                                SteamWorkshopPatch = xo + @"\steamapps\workshop\content\244850\";
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
                            string SavesHmm = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpaceEngineers\Saves\" + SteamID;
                            if (Directory.Exists(SavesHmm))
                                SavesPatch = SavesHmm;
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
            if(LangCultureID != 0) Thread.CurrentThread.CurrentUICulture = new CultureInfo(LangCultureID);
        }
        public static void Serialize()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(MySettings));
            //new StreamWriter("settings.xml")
            using (Stream fs = new FileStream(FILE_PATH, FileMode.Create))
            {
                formatter.Serialize(fs, Current);
            }
        }
        public static void Deserialize()
        {
            try
            {
                if (File.Exists(FILE_PATH))
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(MySettings));
                    using (Stream fs = new FileStream(FILE_PATH, FileMode.Open))
                    {
                        Current = (MySettings)formatter.Deserialize(fs);
                        if (!Directory.Exists(Current.BlueprintPatch))
                        {
                            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                            {
                                dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                                dialog.Description = Lang.SelectBluePatchDesc;
                                dialog.ShowNewFolderButton = false;
                                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                                if (result.Equals(System.Windows.Forms.DialogResult.OK))
                                {
                                    Current.BlueprintPatch = dialog.SelectedPath + "\\";
                                }
                                else
                                    Application.Current.Shutdown();
                            }
                        }
                        bool NoGamePatch = string.IsNullOrWhiteSpace(Current.GamePatch) || string.IsNullOrWhiteSpace(Current.SteamWorkshopPatch);
                        bool NoSteamData = string.IsNullOrWhiteSpace(Current.SteamID);
                        if (NoGamePatch || NoSteamData)
                        {
                            string Steam = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", "false").ToString();
                            if (NoGamePatch)
                                Current.GetGameFolder(Steam);
                            if (NoSteamData)
                                Current.GetSteamAccount(Steam);
                        }
                        if (string.IsNullOrWhiteSpace(Current.SavesPatch))
                        {
                            string SavesHmm = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpaceEngineers\Saves\" + Current.SteamID;
                            if (Directory.Exists(SavesHmm))
                                Current.SavesPatch = SavesHmm;
                        }
                    }
                }
                else if (File.Exists("settings.xml"))
                {
                    DataContractSerializer formatter = new DataContractSerializer(typeof(MySettings));
                    using (Stream fs = new FileStream("settings.xml", FileMode.Open))
                    {
                        Current = (MySettings)formatter.ReadObject(fs);
                        bool NoGamePatch = string.IsNullOrWhiteSpace(Current.GamePatch) || string.IsNullOrWhiteSpace(Current.SteamWorkshopPatch);
                        bool NoSteamData = string.IsNullOrWhiteSpace(Current.SteamID);
                        if (NoGamePatch || NoSteamData)
                        {
                            string Steam = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", "false").ToString();
                            if (NoGamePatch)
                                Current.GetGameFolder(Steam);
                            if (NoSteamData)
                                Current.GetSteamAccount(Steam);
                        }
                        if (string.IsNullOrWhiteSpace(Current.SavesPatch))
                        {
                            string SavesHmm = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpaceEngineers\Saves\" + Current.SteamID;
                            if (Directory.Exists(SavesHmm))
                                Current.SavesPatch = SavesHmm;
                        }
                    }
                    File.Delete("settings.xml");
                }
            }
            catch
            {
                Current.LangCultureID = Thread.CurrentThread.CurrentUICulture.LCID;
                try
                {
                    if (!Directory.Exists(Current.BlueprintPatch))
                    {
                        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                        {
                            dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                            dialog.Description = Lang.SelectBluePatchDesc;
                            dialog.ShowNewFolderButton = false;
                            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                            if (result.Equals(System.Windows.Forms.DialogResult.OK))
                            {
                                Current.BlueprintPatch = dialog.SelectedPath + "\\";
                            }
                            else
                                Application.Current.Shutdown();
                        }
                    }
                    string Steam = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", "false").ToString();
                    Current.GetGameFolder(Steam);
                    Current.GetSteamAccount(Steam);
                }
                catch
                {

                }
            }
        }
    }
    [XmlRoot("Dictionary")]
    public class XmlSerializableDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();
                this.Add(key, value);
                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }
}
