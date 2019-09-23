using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BlueprintEditor2
{
    public class MySettings
    {
        public static MySettings Current = new MySettings();
        public string BlueprintPatch = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SpaceEngineers\Blueprints\local\";
        public bool MultiWindow = false;
        public int LCID;
        MySettings()
        {
            LCID = Thread.CurrentThread.CurrentUICulture.LCID;
        }
        public void ApplySettings()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(LCID);
        }
        public static void Serialize()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(MySettings));
            using (FileStream fs = new FileStream("settings.xml", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, Current);
            }
        }
        public static void Deserialize()
        {
            if (File.Exists("settings.xml"))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(MySettings));
                using (FileStream fs = new FileStream("settings.xml", FileMode.Open))
                {
                    Current = (MySettings)formatter.Deserialize(fs);
                }
            }
        }
    }
}
