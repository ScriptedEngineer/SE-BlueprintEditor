using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using BlueprintEditor2.Resource;

namespace BlueprintEditor2
{
    /// <summary>
    ///     Логика взаимодействия для About.xaml
    /// </summary>
    public partial class About : Window
    {
        internal About()
        {
            InitializeComponent();
            VersionLabel.Content = Lang.Version + ": " + Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) => Process.Start(((Hyperlink) sender).NavigateUri.ToString());
    }
}
