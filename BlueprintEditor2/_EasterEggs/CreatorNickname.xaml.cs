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

namespace BlueprintEditor2._EasterEggs
{
    /// <summary>
    /// Логика взаимодействия для CreatorNickname.xaml
    /// </summary>
    public partial class CreatorNickname : Window
    {
        static internal CreatorNickname LastWindow;
        public CreatorNickname()
        {
            LastWindow = this;
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            LastWindow = null;
        }
    }
}
