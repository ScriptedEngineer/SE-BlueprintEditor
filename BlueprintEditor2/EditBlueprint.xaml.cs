using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace BlueprintEditor2
{
    /// <summary>
    ///     Логика взаимодействия для EditBlueprint.xaml
    /// </summary>
    public partial class EditBlueprint : Window, IDisposable
    {
        private static int _OpenCount;
        private readonly FileStream _Lock;
        private readonly MyXmlBlueprint CurrentBlueprint;
        private readonly SelectBlueprint ItParrent;

        public EditBlueprint(FileStream stream, MyXmlBlueprint blueprint, SelectBlueprint parrent)
        {
            _Lock = stream;
            ItParrent = parrent;
            CurrentBlueprint = blueprint;
            InitializeComponent();
            Title = "[" + CurrentBlueprint.Patch.Split('\\').Last() + "] SE BlueprintEditor"; //TODO: Может загрузка через ресурсы?
            BluePicture.Source = CurrentBlueprint.GetPic(true);
            _OpenCount++;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Dispose();
            _OpenCount--;
            if (_OpenCount == 0)
            {
                ItParrent.Top = SystemParameters.PrimaryScreenHeight / 2 - ItParrent.Height / 2;
                ItParrent.Left = SystemParameters.PrimaryScreenWidth / 2 - ItParrent.Width / 2;
            }
        }

        public void Dispose()
        {
            _Lock?.Dispose();
        }
    }
}
