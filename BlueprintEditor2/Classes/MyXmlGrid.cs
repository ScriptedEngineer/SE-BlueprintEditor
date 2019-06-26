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
        public string Name
        {
            get => NameNode.InnerText;
            set => NameNode.InnerText = value;
        }
        private XmlNode GridSizeNode;
        public GridSizes GridSize
        {
            get { Enum.TryParse(GridSizeNode.InnerText, out GridSizes ehum); return ehum; }
            set => GridSizeNode.InnerText = value.ToString();
        }
        private XmlNode DestructibleNode;
        public bool Destructible
        {
            get => bool.Parse(DestructibleNode.InnerText);
            set => GridSizeNode.InnerText = value.ToString();
        }
        public MyXmlGrid(XmlNode Grid)
        {
            GridXml = Grid;
            foreach (XmlNode Child in Grid.ChildNodes)
            {
                switch (Child.Name)
                {
                    case "DisplayName":
                        NameNode = Child;
                        break;
                    case "CubeBlocks":
                        XmlNodeList Blokes = Child.ChildNodes;
                        Blocks = new MyXmlBlock[Blokes.Count];
                        for (int i = 0; i < Blokes.Count; i++)
                        {
                            Blocks[i] = new MyXmlBlock(Blokes[i]);
                        }
                        break;
                    case "GridSizeEnum":
                        GridSizeNode = Child;
                        break;
                    case "DestructibleBlocks":
                        DestructibleNode = Child;
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
