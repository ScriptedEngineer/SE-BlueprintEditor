using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BlueprintEditor2.Resource;

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для ImageConverter.xaml
    /// </summary>
    public partial class ImageConverter : Window
    {
        public static ImageConverter Opened;
        string Monospace = "";
        bool DontNormalize = false;
        public ImageConverter()
        {
            Opened = this;
            InitializeComponent();
            NormalSize.Content = Lang.Normal + "(178x178)";
            WideSize.Content = Lang.Wide + "(178x356)";
            //NormalizeForm();
        }

        void NormalizeForm()
        {
            if (DontNormalize)
                return;
            System.Drawing.Size PicSize;
            double PicAspect = (double)(DitherPic.Source.Width / DitherPic.Source.Height);
            double BoxAspect = (double)(ImgBox.ActualWidth / ImgBox.ActualHeight);
            if (10 * BoxAspect > 10 * PicAspect)
            {
                PicSize = new System.Drawing.Size((int)(ImgBox.ActualHeight * PicAspect), (int)ImgBox.ActualHeight);
            }
            else
            {
                PicSize = new System.Drawing.Size((int)ImgBox.ActualWidth, (int)(ImgBox.ActualWidth / PicAspect));
            }
            System.Drawing.Size ChangeVec = new System.Drawing.Size((int)ImgBox.ActualWidth, (int)ImgBox.ActualHeight) - PicSize;
            if (Width- ChangeVec.Width > MinWidth && Height - ChangeVec.Height > MinHeight)
            {
                Width = (int)Width - ChangeVec.Width;
                Height = (int)Height - ChangeVec.Height;
            }
            else
            {
                Width = (int)Width + (int)(ChangeVec.Height * PicAspect);
                Height = (int)Height + (int)(ChangeVec.Width / PicAspect);
            }
            Left = SystemParameters.PrimaryScreenWidth / 2 - (Width / 2);
            Top = SystemParameters.PrimaryScreenHeight / 2 - (Height / 2);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Opened = null;
            
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) => Process.Start(((Hyperlink)sender).NavigateUri.ToString());

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.bmp;*.jpg;*.png)|*.BMP;*.JPG;*.PNG";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Monospace = SEImageConverter.ConvertToMonospace(Image.FromFile(openFileDialog.FileName), WideSize.IsChecked.Value ? 356 : 178, 178, Dithering.IsChecked.Value,out Image Resul);
                DitherPic.Source = SEImageConverter.ToSource(Image.FromFile(openFileDialog.FileName));
            }
            NormalizeForm();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "png";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SEImageConverter.FromSource(DitherPic.Source as BitmapSource).Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            NormalizeForm();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Monospace = SEImageConverter.ConvertToMonospace(SEImageConverter.FromSource(DitherPic.Source as BitmapSource), WideSize.IsChecked.Value ? 356 : 178, 178, Dithering.IsChecked.Value,SaveAspect.IsChecked.Value, out Image Resul);
            DitherPic.Source = SEImageConverter.ToSource(Resul);
            System.Windows.Clipboard.SetText(Monospace);
            NormalizeForm();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Monospace = System.Windows.Clipboard.GetText();
            DitherPic.Source = SEImageConverter.ToSource(SEImageConverter.ConvertFromMonospce(Monospace));
            NormalizeForm();
        }
    }
}
