using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlueprintEditor2
{
    public class MyXmlBlock
    {
        public XmlNode BlockXml;
        private XmlNode SubTypeNode;
        public string Type
        {
            get => BlockXml.Attributes.GetNamedItem("xsi:type").Value +"/"+ SubTypeNode.InnerText;
            set
            {
                string[] Types = value.Split('/');
                if (Types.Length == 2)
                {
                    BlockXml.Attributes.GetNamedItem("xsi:type").Value = Types[0];
                    SubTypeNode.InnerText = Types[1];
                }
            }
        }
        private XmlNode NameNode;
        public string Name
        {
            get => NameNode.InnerText;
            set => NameNode.InnerText = value;
        }
        public MyXmlBlock(XmlNode Block)
        {
            BlockXml = Block;
            foreach (XmlNode Child in Block.ChildNodes)
            {
                switch (Child.Name)
                {
                    case "SubtypeName":
                        SubTypeNode = Child;
                        break;
                    case "CustomName":
                        NameNode = Child;
                        break;
                }
            }
        }
    }
}
