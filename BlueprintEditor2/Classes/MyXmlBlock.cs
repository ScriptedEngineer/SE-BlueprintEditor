using System;
using System.Collections.Generic;
using System.Drawing;
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
        private readonly XmlNode _ColorMaskNode;
        private readonly List<MyBlockProperty> _Properties = new List<MyBlockProperty>();
        public MyBlockProperty[] Properties { get => _Properties.ToArray(); }

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
        public string Name
        {
            get => _NameNode?.InnerText;
            set
            {
                if (_NameNode != null) _NameNode.InnerText = value;
            }
        }
        public Color ColorMask
        {
            get
            {
                if (_ColorMaskNode == null) return new Color();
                double x, y, z;
                double.TryParse(_ColorMaskNode.Attributes.GetNamedItem("x").Value.Replace('.',','), out x);
                double.TryParse(_ColorMaskNode.Attributes.GetNamedItem("y").Value.Replace('.', ','), out y);
                double.TryParse(_ColorMaskNode.Attributes.GetNamedItem("z").Value.Replace('.', ','), out z);
                Console.WriteLine(x + "|" + y + "|" + z);
                return SE_ColorConverter.ColorFromSE_HSV(x,y,z);
            }
            set
            {
                double x, y, z;
                SE_ColorConverter.ColorToSE_HSV(value, out x,out y,out z);
                _ColorMaskNode.Attributes.GetNamedItem("x").Value = x.ToString().Replace(',', '.');
                _ColorMaskNode.Attributes.GetNamedItem("y").Value = y.ToString().Replace(',', '.');
                _ColorMaskNode.Attributes.GetNamedItem("z").Value = z.ToString().Replace(',', '.');
            }
        }

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
                    case "ColorMaskHSV":
                        _ColorMaskNode = child;
                        break;
                    default:
                        _Properties.Add(new MyBlockProperty(child));
                        continue;
                }
        }
    }
    public enum ShareMode
    {
        Faction,
        Me,
        None
    }
}
