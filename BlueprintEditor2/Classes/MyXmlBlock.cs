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
        private readonly XmlNode _BlockXml;
        private readonly XmlNode _NameNode;
        private readonly XmlNode _SubTypeNode;

        public string Type
        {
            get => _BlockXml.Attributes?.GetNamedItem("xsi:type").Value + "/" + _SubTypeNode.InnerText;
            set
            {
                string[] types = value.Split('/');
                if (types.Length != 2) return;
                if(_BlockXml.Attributes != default(XmlAttributeCollection))
                    _BlockXml.Attributes.GetNamedItem("xsi:type").Value = types[0];
                _SubTypeNode.InnerText = types[1];
            }
        }
        public string DisplayType
        {
            get => _BlockXml.Attributes?.GetNamedItem("xsi:type").Value.Replace("MyObjectBuilder_","") + "/" + _SubTypeNode.InnerText;
        }

        public string Name { get => _NameNode?.InnerText; set { if (_NameNode != null) _NameNode.InnerText = value; } }

        internal MyXmlBlock(XmlNode block)
        {
            _BlockXml = block;
            foreach (XmlNode child in block.ChildNodes)
                switch (child.Name)
                {
                    case "SubtypeName":
                        _SubTypeNode = child;
                        continue;
                    case "CustomName":
                        _NameNode = child;
                        continue;
                }
        }
    }
}
