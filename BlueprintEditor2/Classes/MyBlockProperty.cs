using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using BlueprintEditor2.Resource;

namespace BlueprintEditor2
{
    public class MyBlockProperty
    {
        private readonly XmlNode _PropertyXml;
        private readonly EditorData _Edit;
        public string Title { get; set; }
        public string PropertyName
        {
            get
            {
                string propNme = Lang.ResourceManager.GetString("Property/"+_PropertyXml.Name);
                if (!string.IsNullOrEmpty(propNme))
                    return propNme;
                else
                    return _PropertyXml.Name;
            }
        }
        public EditorData Edit { get => _Edit; }
        public string TextValue
        {
            get => _PropertyXml.InnerText;
            set
            {
                _Edit.Content = value=="True" ? Lang.Yes : Lang.No;
                _PropertyXml.InnerText = value.ToLower();
            }
        }
        internal MyBlockProperty(XmlNode Node)
        {
            _PropertyXml = Node;
            bool Test;
            if(bool.TryParse(_PropertyXml.InnerText,out Test))
            {
                _Edit = new EditorData();
                _Edit.Content = Test?Lang.Yes:Lang.No;
                _Edit.IsChecked = Test;
                _Edit.CheckboxVisible = Visibility.Visible;
                Title = _PropertyXml.InnerText;
            }
            else
            {
                EditorData box = new EditorData();
                box.Content = "Not performed";
                box.LabelVisible = Visibility.Visible;
                _Edit = box;
            }
        }

        private void Box_Click(object sender, RoutedEventArgs e)
        {
            _PropertyXml.InnerText = ((CheckBox)sender).IsChecked.Value.ToString();
        }
        public class EditorData
        {
            private Visibility _Button = Visibility.Collapsed;
            public Visibility ButtonVisible { get => _Button; set=> _Button = value; }
            private Visibility _Checkbox = Visibility.Collapsed;
            public Visibility CheckboxVisible { get => _Checkbox; set => _Checkbox = value; }
            private Visibility _Label = Visibility.Collapsed;
            public Visibility LabelVisible { get => _Label; set => _Label = value; }
            public string Content { get; set; }
            public bool IsChecked { get; set; }
            //public object Content { get; set; }
            public EditorData()
            {

            }

        }
    }
}
