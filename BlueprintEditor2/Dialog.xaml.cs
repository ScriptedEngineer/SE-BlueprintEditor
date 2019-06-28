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

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для Dialog.xaml
    /// </summary>
    public partial class Dialog : Window
    {
        private Action<DialоgResult> OnClick;

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            OnClick?.Invoke(DialоgResult.Yes);
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            OnClick?.Invoke(DialоgResult.No);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnClick?.Invoke(DialоgResult.Cancel);
            Close();
        }

        public Dialog(DialogPicture Pic, string _Title, string Text, Action<DialоgResult> _Run = null, int _Width = 300, int _Height = 200)
        {
            OnClick = _Run;
            InitializeComponent();
            Title = _Title;
            Width = _Width;
            Height = _Height;
            DataText.Text = Text;
            switch (Pic)
            {
                case DialogPicture.warn:
                    DialImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/warn.png"));
                    break;
                case DialogPicture.attention:
                    DialImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/attention.png"));
                    break;
                case DialogPicture.question:
                    DialImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/question.png"));
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OnClick?.Invoke(DialоgResult.Closed);
        }
    }
    public enum DialogPicture
    {
        warn,
        attention,
        question
    }
    public enum DialоgResult
    {
        Yes,
        No,
        Cancel,
        Closed,
        Data
    }
}
