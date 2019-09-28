using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlueprintEditor2
{
    public class MyBlockProperty
    {
        private readonly XmlNode _PropertyXml;
        public string Name { get => _PropertyXml.Name; }
        public string Edit { get => "Не готово"; }
        internal MyBlockProperty(XmlNode Node)
        {
            _PropertyXml = Node;
        }

    }
}
