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
    /// Логика взаимодействия для Reporter.xaml
    /// </summary>
    public partial class Reporter : Window
    {
        public Reporter()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Run.IsChecked.Value)
            {
                SelectBlueprint.window.Show();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var x = MyExtensions.ApiServer(ApiServerAct.Report, ApiServerOutFormat.@string,
                ",\"body\":\"" + ("Crash Report:" +
                "<br>Sender: " + Who.Text +
                "<br><br>Comment: <br>" + What.Text + //
                "<br><br>Error:<br>" + File.ReadAllText("LastCrash.txt")).Replace("\n", "<br>").Replace("\r", "").Replace("\"","'").Replace("\\","\\\\") + "\"");
            Button_Click(x, null);
        }
    }
}
