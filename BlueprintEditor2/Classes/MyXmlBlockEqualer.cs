using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlueprintEditor2
{
    class MyXmlBlockEqualer
    {
        public string Type;
        public string Name;
        public Color Mask;
        public ShareMode? Share;
        public List<MyBlockProperty> Properties = new List<MyBlockProperty>();

        internal MyXmlBlockEqualer(MyXmlBlock block)
        {
            Type = block.Type;
            Name = block.Name;
            Mask = block.ColorMask;
            if (block.ShareMode.HasValue) Share = block.ShareMode;
            foreach (var x in block.Properties)
                Properties.Add(x);
        }

        public void Equalize(MyXmlBlock block)
        {
            if (block.Type != Type) Type = null;
            if (block.Name != Name)
                if (block.Name == null)
                    Name = null;
                else
                    Name = "-";
            if (block.ColorMask != Mask)
                if (block.ColorMask == new Color())
                    Mask = new Color();
                else
                    Mask = Color.Gray;
            if (Share.HasValue && block.ShareMode.HasValue)
            {
                if (Share.Value != block.ShareMode.Value)
                    Share = ShareMode.Difference;
            }
            else
                Share = null;
            List<int> toDelete = new List<int>();
            int counter = 0;
            foreach (MyBlockProperty x in Properties)
            {
                if (block.Properties.FirstOrDefault(y => y.PropertyName == x.PropertyName) == null)
                {
                    toDelete.Add(counter);
                }
                counter++;
            }
            toDelete.Sort();
            toDelete.Reverse();
            foreach (int x in toDelete)
                Properties.RemoveAt(x);

        }
    }
}
