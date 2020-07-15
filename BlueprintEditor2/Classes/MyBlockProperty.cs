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

        //public string Title { get; set; }
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
        public EditorData Edit { get; }
        public string TextValue
        {
            get => _PropertyXml.InnerText;
            set
            {
                Edit.Content = value == "True" ? Lang.Yes : (value == "False" ? Lang.No : value.Replace(".", ","));
                _PropertyXml.InnerText = value.ToLower();
            }
        }
        internal MyBlockProperty(XmlNode Node)
        {
            _PropertyXml = Node;
            bool Test;
            if(bool.TryParse(_PropertyXml.InnerText,out Test))
            {
                Edit = new EditorData();
                Edit.Content = Test?Lang.Yes:Lang.No;
                Edit.IsChecked = Test;
                Edit.CheckboxVisible = Visibility.Visible;
                //Title = _PropertyXml.InnerText;
            }
            else if (long.TryParse(_PropertyXml.InnerText, out long Testint))
            {
                Edit = new EditorData();
                Edit.Content = Testint.ToString();
                //_Edit.IsChecked = Test;
                Edit.IntTextboxVisible = Visibility.Visible;
                //Title = _PropertyXml.InnerText;
            }
            else if (double.TryParse(_PropertyXml.InnerText.Replace(".", ","), out double Testfl))
            {
                Edit = new EditorData();
                Edit.Content = Testfl.ToString("F18").TrimEnd('0').TrimEnd(',');
                //_Edit.IsChecked = Test;
                Edit.FloatTextboxVisible = Visibility.Visible;
                //Title = _PropertyXml.InnerText;
            }
            /*else if (!string.IsNullOrEmpty(_PropertyXml.InnerText) && _PropertyXml.InnerText == _PropertyXml.InnerXml)
            {
                Edit = new EditorData();
                Edit.Content = _PropertyXml.InnerText;
                //_Edit.IsChecked = Test;
                Edit.TextTextboxVisible = Visibility.Visible;
                //Title = _PropertyXml.InnerText;
            }*/
            else
            {
                EditorData box = new EditorData();
                box.Content = "Not performed";
                box.LabelVisible = Visibility.Visible;
                Edit = box;
            }
        }

        private void Box_Click(object sender, RoutedEventArgs e)
        {
            _PropertyXml.InnerText = ((CheckBox)sender).IsChecked.Value.ToString();
        }
        public class EditorData
        {
            public Visibility ButtonVisible { get; set; } = Visibility.Collapsed;
            public Visibility CheckboxVisible { get; set; } = Visibility.Collapsed;
            public Visibility IntTextboxVisible { get; set; } = Visibility.Collapsed;
            public Visibility FloatTextboxVisible { get; set; } = Visibility.Collapsed;
            public Visibility TextTextboxVisible { get; set; } = Visibility.Collapsed;
            public Visibility LabelVisible { get; set; } = Visibility.Collapsed;
            public string Content { get; set; }
            public bool IsChecked { get; set; }
            //public object Content { get; set; }
            public EditorData()
            {

            }

        }
    }
}
