using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BlueprintEditor2.Resource;
using TextBox = System.Windows.Controls.TextBox;

namespace BlueprintEditor2
{
    /// <summary>
    /// Логика взаимодействия для ImageConverter.xaml
    /// </summary>
    public partial class ImageConverter : Window
    {
        public static ImageConverter Opened;
        string Monospace = "";
        readonly bool DontNormalize = false;
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
            if (BoxAspect > PicAspect)
            {
                PicSize = new System.Drawing.Size((int)(ImgBox.ActualHeight * PicAspect), (int)ImgBox.ActualHeight);
            }
            else
            {
                PicSize = new System.Drawing.Size((int)ImgBox.ActualWidth, (int)(ImgBox.ActualWidth / PicAspect));
            }
            System.Drawing.Size ChangeVec = new System.Drawing.Size((int)ImgBox.ActualWidth, (int)ImgBox.ActualHeight) - PicSize;
            if (Width- ChangeVec.Width >= MinWidth && Height - ChangeVec.Height >= MinHeight)
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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files(*.bmp;*.jpg;*.png)|*.BMP;*.JPG;*.PNG"
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Monospace = SEImageConverter.ConvertToMonospace(Image.FromFile(openFileDialog.FileName), WideSize.IsChecked.Value ? 356 : 178, 178, Dithering.IsChecked.Value,out Image Resul);
                DitherPic.Source = SEImageConverter.ToSource(System.Drawing.Image.FromFile(openFileDialog.FileName));
            }
            NormalizeForm();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                DefaultExt = "png"
            };
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SEImageConverter.FromSource(DitherPic.Source as BitmapSource).Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
            NormalizeForm();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Monospace = SEImageConverter.ConvertToMonospace(SEImageConverter.FromSource(DitherPic.Source as BitmapSource), WideSize.IsChecked.Value ? 356 : 178, 178, Dithering.IsChecked.Value,SaveAspect.IsChecked.Value, out System.Drawing.Image Resul);
            //Monospace = SEImageConverter.ConvertToSuperPixel(SEImageConverter.FromSource(DitherPic.Source as BitmapSource), WideSize.IsChecked.Value ? 512 : 256, 256, SaveAspect.IsChecked.Value, out System.Drawing.Image Resul);
            DitherPic.Source = SEImageConverter.ToSource(Resul);
            System.Windows.Clipboard.SetText(Monospace);
            NormalizeForm();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Monospace = System.Windows.Clipboard.GetText();
            System.Drawing.Image img = SEImageConverter.ConvertFromMonospce(Monospace);
            if(img != null) DitherPic.Source = SEImageConverter.ToSource(img);
            NormalizeForm();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (SplitImg.IsChecked.Value)
            {
                Height += 80;
                MinHeight = 480;
                BottomRow.Height = new GridLength(80);
            }
            else
            {
                MinHeight = 400;
                Height -= 80;
                BottomRow.Height = new GridLength(0);
            }
        }

        readonly List<Bitmap> MemImages = new List<Bitmap>();
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                int.TryParse(ColumnsIntBox.Text, out int Columns);
                int.TryParse(RowsIntBox.Text, out int Rows);
                if (Columns == 0 || Rows == 0)
                    return;
                int xRes = WideSize.IsChecked.Value ? 500 : 250;
                int yRes = 250;
                Bitmap ImageMain;
                if (SaveAspect.IsChecked.Value) 
                {
                    int adx = 0, ady = 0;
                    int width = Rows * xRes;
                    int oldHeight = Columns * yRes;
                    double aspect = (double)DitherPic.Source.Width / (double)DitherPic.Source.Height;
                    int height = (int)(width / aspect);
                    if (height <= oldHeight)
                    {
                        ady = (oldHeight - height);
                    }
                    else
                    {
                        height = oldHeight;
                        int oldWidth = width;
                        width = (int)(height * aspect);
                        adx = (oldWidth - width);
                    }
                    ImageMain = new Bitmap(Rows * xRes, Columns * yRes);
                    Bitmap Orig = new Bitmap(SEImageConverter.FromSource(DitherPic.Source as BitmapSource), width, height);
                    Graphics graph = Graphics.FromImage(ImageMain);
                    graph.DrawImage(Orig, new System.Drawing.Point(adx/2, ady/2));
                    //DitherPic.Source = SEImageConverter.ToSource(ImageMain);return;
                }
                else 
                {
                    ImageMain = new Bitmap(SEImageConverter.FromSource(DitherPic.Source as BitmapSource),Rows * xRes, Columns * yRes);
                }

                MemImages.Clear();
                for (int y = 0; y < Columns; y++)
                {
                    for (int x = 0; x < Rows; x++)
                    {

                        MemImages.Add(ImageMain.Clone(new System.Drawing.Rectangle(x * xRes, y * yRes, xRes, yRes), ImageMain.PixelFormat));
                    }
                }
                DitherPic.Source = SEImageConverter.ToSource(MemImages.First());
                ImgIndexIntBox.Text = "1";
                Label1231321.Content = Lang.Show + "("+MemImages.Count+")";
                if (AutoConvert.IsChecked.Value)
                    Button_Click_2(null, null);
                else
                    NormalizeForm();
            }
            catch
            {

            }
        }

        private void IntBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox Sender = sender as TextBox;
            int.TryParse(Sender.Text, out int x);
            if (x < 0) x = 0;
            Sender.Text = x.ToString();
            Sender.CaretIndex = Sender.Text.Length;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            int.TryParse(ImgIndexIntBox.Text, out int index);
            ImgIndexIntBox.Text = (index + 1).ToString();
            Button_Click_6(null,null);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            int.TryParse(ImgIndexIntBox.Text, out int index);
            if (MemImages.Count < index || index <= 0)
            {
                index = (index <= 0? MemImages.Count: 1);
                ImgIndexIntBox.Text = index.ToString();
            }
            if(MemImages.Count >= index)
                DitherPic.Source = SEImageConverter.ToSource(MemImages[index - 1]);
            if (AutoConvert.IsChecked.Value)
                Button_Click_2(null,null);
            else
                NormalizeForm();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            int.TryParse(ImgIndexIntBox.Text, out int index);
            ImgIndexIntBox.Text = (index - 1).ToString();
            Button_Click_6(null, null);
        }

        private void IntBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Up || e.Key != Key.Down)
                return;
            TextBox Sender = sender as TextBox;
            int.TryParse(Sender.Text, out int x);
            switch (e.Key)
            {
                case Key.Up:
                    x++;
                    break;
                case Key.Down:
                    x--;
                    break;
            }
            if (x <= 0) x = 1;
            Sender.Text = x.ToString();
            Sender.CaretIndex = Sender.Text.Length;
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            //Monospace = SEImageConverter.ConvertToMonospace(SEImageConverter.FromSource(DitherPic.Source as BitmapSource), WideSize.IsChecked.Value ? 356 : 178, 178, Dithering.IsChecked.Value,SaveAspect.IsChecked.Value, out System.Drawing.Image Resul);
            Monospace = SEImageConverter.ConvertToSuperPixel(SEImageConverter.FromSource(DitherPic.Source as BitmapSource), WideSize.IsChecked.Value ? 360 : 250, WideSize.IsChecked.Value ? 180 : 250, true, SaveAspect.IsChecked.Value, out _);
            //DitherPic.Source = SEImageConverter.ToSource(Resul);
            System.Windows.Clipboard.SetText(Monospace);
            NormalizeForm();
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            Monospace = SEImageConverter.ConvertToSuperPixel(SEImageConverter.FromSource(DitherPic.Source as BitmapSource), WideSize.IsChecked.Value ? 360 : 250, WideSize.IsChecked.Value ? 180 : 250, false, SaveAspect.IsChecked.Value, out _);
            //DitherPic.Source = SEImageConverter.ToSource(Resul);
            System.Windows.Clipboard.SetText(Monospace);
            NormalizeForm();
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            Bitmap Pic = new Bitmap(SEImageConverter.FromSource(DitherPic.Source as BitmapSource),150,150);
            string directory = MySettings.Current.BlueprintPatch + @"PixelArt\";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            Pic.Save(directory + "thumb.png");
            StringBuilder file = new StringBuilder();
            file.Append("<?xml version=\"1.0\"?><Definitions xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><ShipBlueprints><ShipBlueprint xsi:type=\"MyObjectBuilder_ShipBlueprintDefinition\"><Id Type=\"MyObjectBuilder_ShipBlueprintDefinition\" Subtype=\"PixelArt\" /><DisplayName>Blueprint Editor</DisplayName><CubeGrids><CubeGrid><SubtypeName /><EntityId>116848042166223950</EntityId><PersistentFlags>CastShadows InScene</PersistentFlags><PositionAndOrientation><Position x=\"104305.64930469102\" y=\"-53695.018532900613\" z=\"24977.389949701621\" /><Forward x=\"-0\" y=\"-0\" z=\"-1\" /><Up x=\"0\" y=\"1\" z=\"0\" /><Orientation><X>0</X><Y>0</Y><Z>0</Z><W>1</W></Orientation></PositionAndOrientation><LocalPositionAndOrientation xsi:nil=\"true\" /><GridSizeEnum>Small</GridSizeEnum><CubeBlocks>");
            for (int y = 0; y < Pic.Height; y++)
                for (int x = 0; x < Pic.Width; x++)
                {
                    System.Drawing.Color pixel = Pic.GetPixel(x, y);
                    if (pixel.A > 178)
                    {
                        file.Append("<MyObjectBuilder_CubeBlock xsi:type=\"MyObjectBuilder_CubeBlock\"><SubtypeName>HalfArmorBlock</SubtypeName>");
                        if (x != 0 || y != 0)
                            file.Append("<Min x=\"").Append(x).Append("\" y=\"").Append(y).Append("\" z=\"0\" />");
                        file.Append("<BlockOrientation Forward=\"Backward\" Up=\"Down\" />");
                        SE_ColorConverter.ColorToSE_HSV(pixel, out double mx, out double my, out double mz);
                        file.Append("<ColorMaskHSV x=\"").Append(mx.ToString().Replace(',', '.'))
                            .Append("\" y=\"").Append(my.ToString().Replace(',', '.'))
                            .Append("\" z=\"").Append(mz.ToString().Replace(',', '.'))
                            .Append("\" />")
                            .Append("<SkinSubtypeId>Clean_Armor</SkinSubtypeId><BuiltBy>144115188075855895</BuiltBy></MyObjectBuilder_CubeBlock>");
                    }
                }
            file.Append("</CubeBlocks><DisplayName>PixelArt</DisplayName><DestructibleBlocks>true</DestructibleBlocks><IsRespawnGrid>false</IsRespawnGrid><LocalCoordSys>0</LocalCoordSys><TargetingTargets /></CubeGrid></CubeGrids><EnvironmentType>None</EnvironmentType><WorkshopId>0</WorkshopId><OwnerSteamId>0</OwnerSteamId><Points>0</Points></ShipBlueprint></ShipBlueprints></Definitions>");
            File.WriteAllText(directory + "bp.sbc", file.ToString());
        }
    }
}
