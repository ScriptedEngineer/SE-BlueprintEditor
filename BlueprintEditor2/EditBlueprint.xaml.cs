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
        static int OpenCount;
        FileStream _lock;
        MyXmlBlueprint EdBlueprint;
        SelectBlueprint ItParrent;
        public EditBlueprint(FileStream Lock, MyXmlBlueprint Blueprint, SelectBlueprint Parrent)
        {
            _lock = Lock;
            ItParrent = Parrent;
            EdBlueprint = Blueprint;
            InitializeComponent();
            Title = "["+EdBlueprint.Patch.Split('\\').Last()+"] SE BlueprintEditor";
            BluePicture.Source = EdBlueprint.GetPic(true);
            OpenCount++;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _lock.Dispose();
            OpenCount--;
            if(OpenCount == 0)
            {
                ItParrent.Top = SystemParameters.PrimaryScreenHeight/2-ItParrent.Height/2;
                ItParrent.Left = SystemParameters.PrimaryScreenWidth / 2 - ItParrent.Width / 2;
            }
        }
    }
}
