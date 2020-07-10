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
        public List<MyXmlBlock> Blocks;
        private XmlNode NameNode;
        private XmlNode GridSizeNode;
        private XmlNode DestructibleNode;
        private XmlNode DeformationsNode;

        public bool IsDamaged { get; private set; }
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
            get
            {
                if (DestructibleNode != null)
                    return bool.Parse(DestructibleNode.InnerText);
                else 
                    return true;
            }
            set
            {
                if (DestructibleNode != null) 
                    DestructibleNode.InnerText = value.ToString().ToLower();
            } 
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
                        Blocks = child.ChildNodes.Cast<XmlNode>().Select(x => {
                            var xmlB = new MyXmlBlock(x);
                            if (!IsDamaged) {
                                var xp = xmlB.GetProperty("IntegrityPercent");
                                var xb = xmlB.GetProperty("BuildPercent");
                                if (xp != null && (xb == null || xp.TextValue != xb.TextValue) && xp.TextValue != "1")
                                    IsDamaged = true;
                            }
                            return xmlB;
                        }).ToList();
                        break;
                    case "GridSizeEnum":
                        GridSizeNode = child;
                        break;
                    case "DestructibleBlocks":
                        DestructibleNode = child;
                        break;
                    case "Skeleton":
                        IsDamaged = true;
                        DeformationsNode = child;
                        break;
                }
            }
        }
        public void FixVisualDamage()
        {
            if (DeformationsNode != null)
            {
                XmlNode parent = DeformationsNode.ParentNode;
                parent.RemoveChild(DeformationsNode);
            }
            IsDamaged = false;
        }
        public void RemoveBlock(MyXmlBlock Block)
        {
            Block.Delete();
            Blocks.Remove(Block);
        }
    }
    public enum GridSizes
    {
        Small, Large
    }
}
