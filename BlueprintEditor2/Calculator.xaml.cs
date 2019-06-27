using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для Calculator.xaml
    /// </summary>
    public partial class Calculator : Window
    {
        FileStream _lock;
        MyXmlBlueprint EdBlueprint;
        public Calculator(FileStream Lock, MyXmlBlueprint Blueprint)
        {
            _lock = Lock;
            EdBlueprint = Blueprint;
            InitializeComponent();
            Title = "[" + EdBlueprint.Patch.Split('\\').Last() + "] Calculator - SE BlueprintEditor";
            BluePicture.Source = EdBlueprint.GetPic(true);
            EditBlueprint.OpenCount++;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _lock.Dispose();
            EditBlueprint.OpenCount--;
            if (EditBlueprint.OpenCount == 0)
            {
                SelectBlueprint.window.Top = SystemParameters.PrimaryScreenHeight / 2 - SelectBlueprint.window.Height / 2;
                SelectBlueprint.window.Left = SystemParameters.PrimaryScreenWidth / 2 - SelectBlueprint.window.Width / 2;
            }
        }
    }
}
