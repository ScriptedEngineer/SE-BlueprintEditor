using System;
using System.Collections.Generic;
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

        internal MyXmlBlockEqualer(MyXmlBlock block)
        {
            Type = block.Type;
            Name = block.Name;
        }

        public void Equalize(MyXmlBlock block)
        {
            if (block.Type != Type) Type = null;
            if (block.Name != Name)
                if (block.Name == null)
                    Name = null;
                else
                    Name = "-";
        }
    }
}
