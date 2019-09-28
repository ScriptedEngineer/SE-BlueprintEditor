using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlueprintEditor2
{
    public class MyXmlGrid
    {
        public XmlNode GridXml;
        public MyXmlBlock[] Blocks;
        private XmlNode NameNode;
        private XmlNode GridSizeNode;
        private XmlNode DestructibleNode;
        public string Name
        {
            get => NameNode.InnerText;
            set => NameNode.InnerText = value;
        }
        public GridSizes GridSize
        {
            get { Enum.TryParse(GridSizeNode.InnerText, out GridSizes ehum); return ehum; }
            set => GridSizeNode.InnerText = value.ToString();
        }
        public bool Destructible
        {
            get => bool.Parse(DestructibleNode.InnerText);
            set => DestructibleNode.InnerText = value.ToString();
        }
        public MyXmlGrid(XmlNode Grid)
        {
            GridXml = Grid;
            foreach (XmlNode child in Grid.ChildNodes)
            {
                switch (child.Name)
                {
                    case "DisplayName":
                        NameNode = child;
                        break;
                    case "CubeBlocks":
                        Blocks = child.ChildNodes.Cast<XmlNode>().Select(x => new MyXmlBlock(x)).ToArray();
                        break;
                    case "GridSizeEnum":
                        GridSizeNode = child;
                        break;
                    case "DestructibleBlocks":
                        DestructibleNode = child;
                        break;
                }
            }
        }
    }
    public enum GridSizes
    {
        Small, Large
    }
}
