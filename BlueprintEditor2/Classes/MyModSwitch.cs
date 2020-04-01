using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueprintEditor2
{
    public class MyModSwitch
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public bool Enabled { get; set; }
        private MyModSwitch()
        {
            Name = "Undefined";
            ID = "0";
            Enabled = false;
        }
        public MyModSwitch(string name, string id)
        {
            Name = name;
            ID = id;
            Enabled = true;
        }
    }
}
