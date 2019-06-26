using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using BlueprintEditor2.Resource;

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для About.xaml
    /// </summary>
    public partial class About : Window
    {
        static About LastWindow;
        private bool CloseIt = false;
        public About()
        {
            if (LastWindow != null)
            {
                LastWindow.Show();
                LastWindow.Focus();
                CloseIt = true;
            }
            else LastWindow = this;
            InitializeComponent();
            VersionLabel.Content = Lang.Version+": "+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (CloseIt) Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            LastWindow = null;
        }
    }
}
