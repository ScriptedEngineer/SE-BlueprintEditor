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
    public partial class MessageDialog : Window
    {
        private Action<DialоgResult> OnClick;
        private Action<string> OnInput;
        internal static MessageDialog Last;
        public MessageDialog(DialogPicture Pic, string _Title, string Text, Action<DialоgResult> _Run = null, DialogType Type = DialogType.Normal, int _Width = 300, int _Height = 200)
        {
            OnClick = _Run;
            InitializeComponent();
            Title = _Title;
            Width = _Width;
            Height = _Height;
            DataText.Text = Text;
            string[] StrHeight = Text.Split('\n');
            if (StrHeight.Length > 7)
            {
                Height = 180 + 18*(StrHeight.Length - 7);
                int width = 400;
                foreach (string str in StrHeight)
                {
                    int iwidth = 400 + 8 * (str.Length - 41);
                    if (iwidth > width) width = iwidth;
                }
                Width = width;
            }
            switch (Pic)
            {
                case DialogPicture.warn:
                    DialImage.Source = new BitmapImage(new Uri("pack://application:,,,/../Resource/warn.png"));
                    break;
                case DialogPicture.attention:
                    DialImage.Source = new BitmapImage(new Uri("pack://application:,,,/../Resource/attention.png"));
                    break;
                case DialogPicture.question:
                    DialImage.Source = new BitmapImage(new Uri("pack://application:,,,/../Resource/question.png"));
                    break;
            }
            switch (Type)
            {
                case DialogType.Normal:
                    CancelButton.Visibility = Visibility.Hidden;
                    YesButton.Margin = new Thickness(0, 0, 90, 10);
                    NoButton.Margin = new Thickness(0, 0, 10, 10);
                    break;
                case DialogType.Cancelable:
                    break;
                case DialogType.Message:
                    CancelButton.Content = "ОК";
                    YesButton.Visibility = Visibility.Hidden;
                    NoButton.Visibility = Visibility.Hidden;
                    break;
            }
            Last = this;
        }
        public MessageDialog(Action<string> _Run, DialogPicture Pic, string _Title, string Text, int _Width = 300, int _Height = 200)
        {
            OnInput = _Run;
            InitializeComponent();
            Title = _Title;
            Width = _Width;
            Height = _Height;
            DataText.Text = Text;
            DialImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resource/92133ae17f4c9ec61407.png"));
            Input.Visibility = Visibility.Visible;
            YesButton.Visibility = Visibility.Hidden;
            NoButton.Content = "ОК";
            Last = this;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            OnClick?.Invoke(DialоgResult.Yes);
            OnClick = null;
            Close();
        }
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            OnClick?.Invoke(DialоgResult.No);
            OnInput?.Invoke(Input.Text);
            OnClick = null;
            Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnClick?.Invoke(DialоgResult.Cancel);
            OnClick = null;
            Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OnClick?.Invoke(DialоgResult.Closed);
            OnClick = null;
            Last = null;
        }

    }
    public enum DialogPicture
    {
        warn,
        attention,
        question
    }
    public enum DialogType
    {
        Normal,
        Cancelable,
        Message,
        Input
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
