using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlueprintEditor2
{
    class MyDisplayBlueprint
    {
        //public string[] Elements { get; private set; }
        public string Name { get; private set; }
        public string Owner { get; private set; }
        internal DateTime CreationTime;
        public string CreationTimeText => CreationTime.ToString("d");
        internal DateTime LastEditTime;
        public string LastEditTimeText => LastEditTime.ToString("d");
        internal int BlockCount;
        public string BlockCountText => BlockCount.ToString();
        internal int GridCount;
        public string GridCountText => GridCount.ToString();

        private MyDisplayBlueprint()
        {
            
        }
        static public MyDisplayBlueprint fromBlueprint(string Bluepath)
        {
            if (File.Exists(Bluepath + "\\bp.sbc"))
            {
                MyDisplayBlueprint Elem = new MyDisplayBlueprint();
                Elem.Name = Bluepath.Split('\\').Last();
                XmlDocument BlueprintXml = new XmlDocument();
                BlueprintXml.Load(Bluepath + "\\bp.sbc");
                Elem.Owner = BlueprintXml.GetElementsByTagName("DisplayName").Item(0).InnerText;
                Elem.BlockCount = BlueprintXml.GetElementsByTagName("MyObjectBuilder_CubeBlock").Count;
                Elem.GridCount = BlueprintXml.GetElementsByTagName("CubeGrid").Count;
                FileInfo Inf = new FileInfo(Bluepath + "\\bp.sbc");
                Elem.LastEditTime = Inf.LastWriteTime;
                Elem.CreationTime = Inf.CreationTime;
                return Elem;
            }
            return null;
        }
    }
}
