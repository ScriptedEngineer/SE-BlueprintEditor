using System;
using System.Collections.Generic;
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
using System.Threading;
using System.IO;
using System.Reflection;

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для EditBlueprint.xaml
    /// </summary>
    public partial class EditBlueprint : Window
    {
        static public int OpenCount;
        FileStream _Lock;
        MyXmlBlueprint EdBlueprint;
        public EditBlueprint(FileStream Lock, MyXmlBlueprint Blueprint)
        {
            _Lock = Lock;
            EdBlueprint = Blueprint;
            InitializeComponent();
            Title = "["+EdBlueprint.Patch.Split('\\').Last()+"] Editor - SE BlueprintEditor";
            BluePicture.Source = EdBlueprint.GetPic(false, false);
            OpenCount++;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _Lock?.Dispose();
            OpenCount--;
            if(OpenCount == 0)
            {
                SelectBlueprint.window.Top = SystemParameters.PrimaryScreenHeight/2- SelectBlueprint.window.Height/2;
                SelectBlueprint.window.Left = SystemParameters.PrimaryScreenWidth / 2 - SelectBlueprint.window.Width / 2;
            }
        }
    }
}
