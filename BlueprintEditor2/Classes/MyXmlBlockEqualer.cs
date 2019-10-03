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

        internal MyXmlBlockEqualer(MyXmlBlock block)
        {
            Type = block.Type;
            Name = block.Name;
            Mask = block.ColorMask;
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
        }
    }
}
