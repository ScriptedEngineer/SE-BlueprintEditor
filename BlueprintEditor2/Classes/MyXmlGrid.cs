using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;
using System.Numerics;

namespace BlueprintEditor2
{
    public class MyXmlGrid
    {
        public XmlNode GridXml;
        private Dictionary<Vector3, MyXmlBlock> _Blocks = new Dictionary<Vector3, MyXmlBlock>();
        private readonly XmlNode NameNode;
        private readonly XmlNode GridSizeNode;
        private readonly XmlNode DestructibleNode;
        private readonly XmlNode DeformationsNode;

        public List<MyXmlBlock> Blocks
        {
            get => _Blocks.Values.ToList();
            set => _Blocks = value.ToDictionary(x => x.Position);
        }
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
                        foreach (XmlNode x in child.ChildNodes){
                            var xmlB = new MyXmlBlock(x);
                            if (!IsDamaged) {
                                var xp = xmlB.GetProperty("IntegrityPercent");
                                var xb = xmlB.GetProperty("BuildPercent");
                                if (xp != null && (xb == null || xp.TextValue != xb.TextValue) && xp.TextValue != "1")
                                    IsDamaged = true;
                            }
                            if(!_Blocks.ContainsKey(xmlB.Position))
                                _Blocks.Add(xmlB.Position, xmlB);
                        }
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
        public MyXmlBlock GetBlockByPos(Vector3 pos)
        {
            if (_Blocks.ContainsKey(pos))
            {
                return _Blocks[pos];
            }
            return null;
        }
        public bool RemoveBlock(MyXmlBlock Block)
        {
            if (_Blocks.Remove(Block.Position))
            {
                Block.Delete();
                return true;
            }
            return false;
        }
    }
    public enum GridSizes
    {
        Small, Large
    }
}
