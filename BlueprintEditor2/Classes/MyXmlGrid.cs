using System;
using System.Collections;
using System.Xml;
using System.Linq;

namespace BlueprintEditor2
{
    internal class MyXmlGrid
    {
        public readonly MyXmlBlock[] Blocks;
        private readonly XmlNode _DestructibleNode;
        private readonly XmlNode _GridSizeNode;
        //private XmlNode _GridXml;
        private readonly XmlNode _NameNode;

        public string Name
        {
            get => _NameNode.InnerText;
            set => _NameNode.InnerText = value;
        }

        public GridSizes GridSize
        {
            get
            {
                Enum.TryParse(_GridSizeNode.InnerText, out GridSizes @enum);
                return @enum;
            }
            set => _GridSizeNode.InnerText = value.ToString();
        }

        public bool Destructible
        {
            get => bool.Parse(_DestructibleNode.InnerText);
            set => _GridSizeNode.InnerText = value.ToString();
        }

        public MyXmlGrid(XmlNode grid)
        {
            //_GridXml = grid;
            foreach (XmlNode child in grid.ChildNodes) switch (child.Name)
                {
                    case "DisplayName":
                        _NameNode = child;
                        continue;
                    case "CubeBlocks":
                        Blocks = child.ChildNodes.Cast<XmlNode>().Select(x => new MyXmlBlock(x)).ToArray();
                        continue;
                    case "GridSizeEnum":
                        _GridSizeNode = child;
                        continue;
                    case "DestructibleBlocks":
                        _DestructibleNode = child;
                        continue;
                }
        }
    }

    public enum GridSizes //TODO: возможно, стоит использовать КИНовское перечисление?
    {
        Small,
        Large
    }
}
