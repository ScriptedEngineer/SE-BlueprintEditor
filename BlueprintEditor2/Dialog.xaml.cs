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
        private Action OnYes, OnNo, OnCancel;

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            OnYes?.Invoke();
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            OnNo?.Invoke();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCancel?.Invoke();
            Close();
        }

        public Dialog(DialogPicture Pic, string _Title, string Text, Action _OnYes = null, Action _OnNo = null, Action _OnCancel = null, int _Width = 300, int _Height = 200)
        {
            OnYes = _OnYes;
            OnNo = _OnNo;
            OnCancel = _OnCancel;
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
            OnCancel?.Invoke();
        }
    }
    public enum DialogPicture
    {
        warn,
        attention,
        question
    }
}
