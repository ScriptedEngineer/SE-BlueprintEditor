using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlueprintEditor2
{
    class MyListElement
    {
        public string[] Elements { get; private set; }
        private MyListElement()
        {

        }
        static public MyListElement fromBlueprint(string Bluepath)
        {
            if (File.Exists(Bluepath + "\\bp.sbc"))
            {
                MyListElement Elem = new MyListElement();
                Elem.Elements = new string[5];
                Elem.Elements[0] = Bluepath.Split('\\').Last();
                /*XmlDocument BlueprintXml = new XmlDocument();
                BlueprintXml.Load(Bluepath + "\\bp.sbc");
                Elem.Elements[1] = BlueprintXml.GetElementsByTagName("DisplayName").Item(0).InnerText;
                Elem.Elements[4] = BlueprintXml.GetElementsByTagName("MyObjectBuilder_CubeBlock").Count.ToString();
                */
                FileInfo Inf = new FileInfo(Bluepath + "\\bp.sbc");
                Elem.Elements[3] = Inf.LastWriteTime.ToString();
                Elem.Elements[2] = Inf.CreationTime.ToString("dd:mm:yyyy");
                return Elem;
            }
            return null;
        }
    }
}
