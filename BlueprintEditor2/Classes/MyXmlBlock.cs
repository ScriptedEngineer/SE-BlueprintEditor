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
        private readonly XmlNode _ShareModeNode;
        private readonly XmlNode _MinPosNode;
        private readonly XmlNode _CustomDataNode;
        private readonly XmlNode _PublicTextNode;
        private readonly XmlNode _SkinNode;
        private readonly XmlNode _ProgramNode;
        private readonly XmlNode _StorageNode;
        private Dictionary<string,MyBlockProperty> _Properties = new Dictionary<string, MyBlockProperty>();
        public string Type
        {
            get => _BlockXml.Attributes?.GetNamedItem("xsi:type").Value.Replace("MyObjectBuilder_", "") + "/" + _SubTypeNode.InnerText;
            set
            {
                string[] types = value.Split('/');
                if (types.Length < 2) return;
                types[0] = "MyObjectBuilder_" + types[0];
                if (_BlockXml.Attributes != default(XmlAttributeCollection))
                    _BlockXml.Attributes.GetNamedItem("xsi:type").Value = types[0];
                if (string.IsNullOrEmpty(types[1]))
                    (_SubTypeNode as XmlElement).IsEmpty = true;
                else _SubTypeNode.InnerText = types[1];
            }
        }
        public string DisplayType => Type;
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
                double.TryParse(_ColorMaskNode.Attributes.GetNamedItem("x").Value.Replace('.', ','), out x);
                double.TryParse(_ColorMaskNode.Attributes.GetNamedItem("y").Value.Replace('.', ','), out y);
                double.TryParse(_ColorMaskNode.Attributes.GetNamedItem("z").Value.Replace('.', ','), out z);
                return SE_ColorConverter.ColorFromSE_HSV(x, y, z);
            }
            set
            {
                if (_ColorMaskNode == null) return;
                double x, y, z;
                SE_ColorConverter.ColorToSE_HSV(value, out x, out y, out z);
                _ColorMaskNode.Attributes.GetNamedItem("x").Value = x.ToString().Replace(',', '.');
                _ColorMaskNode.Attributes.GetNamedItem("y").Value = y.ToString().Replace(',', '.');
                _ColorMaskNode.Attributes.GetNamedItem("z").Value = z.ToString().Replace(',', '.');
            }
        }
        public ShareMode? ShareMode
        {
            get
            {
                ShareMode Mode;
                if (_ShareModeNode != null && Enum.TryParse(_ShareModeNode.InnerText, out Mode))
                    return Mode;
                else
                    return null;
            }
            set
            {
                if (value.HasValue && value.Value != BlueprintEditor2.ShareMode.Difference)
                    _ShareModeNode.InnerText = value.Value.ToString();
            }
        }
        public MyBlockProperty[] Properties { get => _Properties.Values.ToArray(); }
        public ArmorType Armor
        {
            get
            {
                string subtype = _SubTypeNode.InnerText;
                if (IsArmor && subtype.Contains("Heavy"))
                    return ArmorType.Heavy;
                else if (IsArmor)
                    return ArmorType.Light;
                else
                    return ArmorType.None;
            }
            set
            {
                if (IsArmor)
                {
                    string junk = _SubTypeNode.InnerText;
                    switch (value)
                    {
                        case ArmorType.Heavy:
                            if (!junk.Contains("Heavy"))
                            {
                                if (junk.Contains("BlockArmor"))
                                    _SubTypeNode.InnerText = junk.Replace("BlockArmor", "HeavyBlockArmor");
                                else if (junk.Contains("Half"))
                                    _SubTypeNode.InnerText = junk.Replace("Half", "HeavyHalf");
                            }
                            break;
                        case ArmorType.Light:
                            _SubTypeNode.InnerText = junk.Replace("Heavy", "");
                            break;
                    }
                }
            }
        }
        public Vector3 Position
        {
            get
            {
                if (_MinPosNode == null) return Vector3.Zero;
                var Atrs = _MinPosNode.Attributes;
                int.TryParse(Atrs.GetNamedItem("x").Value, out int X);
                int.TryParse(Atrs.GetNamedItem("y").Value, out int Y);
                int.TryParse(Atrs.GetNamedItem("z").Value, out int Z);
                return new Vector3(X,Y,Z);
            }
            set
            {
                if (_MinPosNode == null) return;
                var Atrs = _MinPosNode.Attributes;
                Atrs.GetNamedItem("x").Value = value.X.ToString();
                Atrs.GetNamedItem("y").Value = value.Y.ToString();
                Atrs.GetNamedItem("z").Value = value.Z.ToString();
            }
        }
        public bool IsArmor { get; }
        public string CustomData 
        {
            get => _CustomDataNode?.InnerText;
            set
            {
                if (_CustomDataNode != null) _CustomDataNode.InnerText = value;
            }
        }
        public string PublicText
        {
            get => _PublicTextNode?.InnerText;
            set
            {
                if (_PublicTextNode != null) _PublicTextNode.InnerText = value;
            }
        }
        public string Skin
        {
            get => _SkinNode?.InnerText;
            set
            {
                if (_SkinNode != null) _SkinNode.InnerText = value;
            }
        }
        public MyBlockOrientation Orientation { get; set; }
        public List<MyBlockInventory> Inventories { get; set; }

        public string Program
        {
            get => _ProgramNode?.InnerText;
            set
            {
                if (_ProgramNode != null) _ProgramNode.InnerText = value;
            }
        }
        public string Storage
        {
            get => _StorageNode?.InnerText;
            set
            {
                if (_StorageNode != null) _StorageNode.InnerText = value;
            }
        }

        internal MyXmlBlock(XmlNode block)
        {
            _BlockXml = block;
            Inventories = new List<MyBlockInventory>();
            foreach (XmlNode child in block.ChildNodes)
                switch (child.Name)
                {
                    case "SubtypeName":
                        _SubTypeNode = child;
                        IsArmor = _SubTypeNode.InnerText.Contains("Armor");
                        continue;
                    case "CustomName":
                        _NameNode = child;
                        continue;
                    case "ColorMaskHSV":
                        _ColorMaskNode = child;
                        break;
                    case "ShareMode":
                        _ShareModeNode = child;
                        break;
                    case "Min":
                        _MinPosNode = child;
                        break;
                    case "ComponentContainer":
                        foreach (XmlNode node in child.FirstChild.ChildNodes)
                        {
                            switch (node.FirstChild.InnerText)
                            {
                                case "MyModStorageComponent":
                                    _CustomDataNode = node.LastChild.LastChild.LastChild.LastChild.LastChild;
                                    break;
                                case "MyInventoryBase":
                                    switch (node.LastChild.Attributes?.GetNamedItem("xsi:type").Value)
                                    {
                                        case "MyObjectBuilder_Inventory":
                                            Inventories.Add(new MyBlockInventory(node.LastChild));
                                            break;
                                        case "MyObjectBuilder_InventoryAggregate":
                                            foreach (XmlNode inode in node.LastChild.LastChild.ChildNodes)
                                            {
                                                Inventories.Add(new MyBlockInventory(inode));
                                            }
                                            break;
                                    }
                                    break;
                            }
                        }
                        break;
                    case "PublicDescription":
                        _PublicTextNode = child;
                        break;
                    case "SkinSubtypeId":
                        _SkinNode = child;
                        break;
                    case "BlockOrientation":
                        Orientation = new MyBlockOrientation(child);
                        break;
                    case "Program":
                        _ProgramNode = child;
                        break;
                    case "Storage":
                        _StorageNode = child;
                        break;
                    default:
                        var prop = new MyBlockProperty(child);
                        _Properties.Add(prop.PropertyName, prop);
                        continue;
                    //Ignore
                    case "EntityId":
                        break;
                }
            if (Orientation == null) Orientation = new MyBlockOrientation();
        }
        public void Delete()
        {
            XmlNode parent = _BlockXml.ParentNode;
            parent.RemoveChild(_BlockXml);
        }
        public MyBlockProperty GetProperty(string Prop)
        {
            if (_Properties.Keys.Contains(Prop))
            {
               return _Properties[Prop];
            }
            return null;
        }
        public void SetPropertyIfExists(string Prop, string Value)
        {
            if(_Properties.Keys.Contains(Prop))
            {
                _Properties[Prop].TextValue = Value;
            }
        }
    }
    
    public class MyBlockOrientation
    {
        private readonly XmlNode _OrientationNode;
        private Base6Directions _Forward, _Up;
        public Base6Directions Forward
        {
            get => _Forward;
            set
            {
                _Forward = value;
                if (_OrientationNode != null)
                {
                    var Atrs = _OrientationNode.Attributes;
                    Atrs.GetNamedItem("Forward").Value = _Forward.ToString();
                }
            }
        }
        public Base6Directions Up
        {
            get => _Up;
            set
            {
                _Up = value;
                if (_OrientationNode != null)
                {
                    var Atrs = _OrientationNode.Attributes;
                    Atrs.GetNamedItem("Up").Value = _Up.ToString();
                }
            }
        }
        public MyBlockOrientation(XmlNode Onode)
        {
            _OrientationNode = Onode;
            var Atrs = _OrientationNode.Attributes;
            Enum.TryParse(Atrs.GetNamedItem("Forward").Value, out _Forward);
            Enum.TryParse(Atrs.GetNamedItem("Up").Value, out _Up);
        }
        public MyBlockOrientation()
        {
            _OrientationNode = null;
            Forward = Base6Directions.Forward;
            Up = Base6Directions.Up;
        }
        public Vector3 SizeToPos(Vector3 size)
        {
            switch (Forward)
            {
                case Base6Directions.Forward:
                case Base6Directions.Backward:
                    switch (Up)
                    {
                        case Base6Directions.Left:
                        case Base6Directions.Right:
                            return new Vector3(size.Y, size.X, size.Z);//True;

                        case Base6Directions.Up:
                        case Base6Directions.Down:
                            return new Vector3(size.X, size.Y, size.Z);//True;
                    }
                    break;
                case Base6Directions.Left:
                case Base6Directions.Right:
                    switch (Up)
                    {
                        case Base6Directions.Forward:
                        case Base6Directions.Backward:
                            return new Vector3(size.Z, size.X, size.Y);

                        case Base6Directions.Up:
                        case Base6Directions.Down:
                            return new Vector3(size.Z, size.Y, size.X);
                    }
                    break;
                case Base6Directions.Up:
                case Base6Directions.Down:
                    switch (Up)
                    {
                        case Base6Directions.Forward:
                        case Base6Directions.Backward:
                            return new Vector3(size.X, size.Z, size.Y);

                        case Base6Directions.Left:
                        case Base6Directions.Right:
                            return new Vector3(size.Y, size.Z, size.X);
                    }
                    break;
            }
            return new Vector3(size.X, size.Y, size.Z);//InvalidOrientation;
        }
    }
    public class MyBlockInventory
    {
        private readonly XmlNode InventoryNode;
        private readonly XmlNode ItemsNode;
        private readonly XmlNode NextIDNode;
        private readonly XmlNode VolumeNode;
        private List<MyItem> _Items = new List<MyItem>();
        public IEnumerable<MyItem> Items
        {
            get => _Items; 
            set
            {
                _Items = new List<MyItem>(value);
                string Inventoryed = "";
                int Couner = 0;
                foreach (MyItem inv in _Items)
                {
                    string[] Type = inv.Type.Split('/');
                    if (Type.Length > 1)
                        Inventoryed += $"<MyObjectBuilder_InventoryItem><Amount>{inv.Count.ToString().Replace(",", ".")}</Amount><PhysicalContent xsi:type=\"MyObjectBuilder_{Type[0]}\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><SubtypeName>{Type[1]}</SubtypeName>{(Type[0] == "AmmoMagazine" ? "<ProjectilesCount>0</ProjectilesCount>":"")}</PhysicalContent><ItemId>" + Couner + "</ItemId></MyObjectBuilder_InventoryItem>";
                    Couner++;
                }
                ItemsNode.InnerXml = Inventoryed;
                NextIDNode.InnerText = Couner.ToString();
            }
        }
        public MyBlockInventory(XmlNode inventoryNode)
        {
            InventoryNode = inventoryNode;
            foreach (XmlNode Xms in InventoryNode.ChildNodes)
            {
                switch (Xms.Name)
                {
                    case "Items":
                        ItemsNode = Xms;
                        foreach (XmlNode Xm in Xms.ChildNodes)
                        {
                            XmlNode Type = Xm.ChildNodes[1];
                            double.TryParse(Xm.FirstChild.InnerText, out double count);
                            _Items.Add(new MyItem(Type.Attributes.GetNamedItem("xsi:type").Value.Replace("MyObjectBuilder_", "") + "/" + Type.FirstChild.InnerText, count));
                        }
                        break;
                    case "nextItemId":
                        NextIDNode = Xms;
                        break;
                    case "Volume":
                        VolumeNode = Xms;
                        break;
                    case "Mass":
                        Xms.InnerText = "9223372036854.775807";
                        break;
                }
                
            }
        }

        public void Save()
        {
            string Inventoryed = "";
            int Couner = 0;
            foreach (MyItem inv in _Items)
            {
                string[] Type = inv.Type.Split('/');
                if (Type.Length > 1)
                    Inventoryed += $"<MyObjectBuilder_InventoryItem><Amount>{inv.Count.ToString().Replace(",", ".")}</Amount><PhysicalContent xsi:type=\"MyObjectBuilder_{Type[0]}\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><SubtypeName>{Type[1]}</SubtypeName>{(Type[0] == "AmmoMagazine" ? "<ProjectilesCount>0</ProjectilesCount>" : "")}</PhysicalContent><ItemId>" + Couner + "</ItemId></MyObjectBuilder_InventoryItem>";
                Couner++;
            }
            ItemsNode.InnerXml = Inventoryed;
            NextIDNode.InnerText = Couner.ToString();
        }
        public class MyItem
        {
            public string Type { get; set; }
            public double Count { get; set; }
            public string Amount
            {
                get => Count.ToString("F18").TrimEnd('0').TrimEnd(',');
                set
                {
                    double.TryParse(value, out double intres);
                    Count = intres;
                }
            }
            public MyItem(string type, double amount)
            {
                Type = type;
                Count = amount;
            }
        }
    }
    public enum Base6Directions : byte
    {
        Forward, Backward, 
        Left, Right,
        Up, Down
    }
    public enum ArmorType
    {
        Heavy,
        Light,
        None
    }
    public enum ShareMode
    {
        All,
        Faction,
        None,
        Difference
    }
}
