using System.Xml;

namespace BlueprintEditor2
{
    internal class MyXmlBlock
    {
        private readonly XmlNode _BlockXml;
        private readonly XmlNode _NameNode;
        private readonly XmlNode _SubTypeNode;

        internal string Type
        {
            get => _BlockXml.Attributes?.GetNamedItem("xsi:type").Value + "/" + _SubTypeNode.InnerText;
            set
            {
                string[] types = value.Split('/');
                if (types.Length != 2) return; //TODO: возможно, стоит выбрасывать исключение?
                _BlockXml.Attributes.GetNamedItem("xsi:type").Value = types[0]; //TODO: возможен NullReference
                _SubTypeNode.InnerText = types[1];
            }
        }

        internal string Name { get => _NameNode.InnerText; set => _NameNode.InnerText = value; }

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
