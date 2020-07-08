using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BlueprintEditor2
{
    public static class SEImageConverter
    {
        public static string ConvertToMonospace(Image original,int width,int height, bool dithering, bool saspect, out Image Result)
        {
            try
            {
                StringBuilder text = new StringBuilder();
                int LineAdder = 0;
                if (saspect)
                {
                    int oldHeight = height;
                    double aspect = (double)original.Width / (double)original.Height;
                    height = (int)(width/ aspect);
                    if (height <= oldHeight)
                    {
                        int discro = (oldHeight - height) / 2;
                        if (discro > 0)
                            for (int i = 0; i < discro; i++)
                                text.Append("\n");
                    }
                    else
                    {
                        height = oldHeight;
                        int oldWidth = width;
                        width = (int)(height * aspect);
                        LineAdder = (oldWidth - width) / 2;
                    }
                }
                Bitmap bm = new Bitmap(original, width, height);
                for (int i = 0; i < bm.Height; i++)
                {
                    if (LineAdder > 0)
                        for (int d = 0; d < LineAdder; d++)
                            text.Append((char)(ushort)57600);
                    for (int j = 0; j < bm.Width; j++)
                    {
                        Color pixel = bm.GetPixel(j, i);
                        int num = pixel.R >> 5;
                        int num2 = pixel.G >> 5;
                        int num3 = pixel.B >> 5;
                        int num4 = pixel.R & 0x1F;
                        int num5 = pixel.G & 0x1F;
                        int num6 = pixel.B & 0x1F;
                        int red = num << 5;
                        int green = num2 << 5;
                        int blue = num3 << 5;
                        text.Append((char)(ushort)(57600 + (num << 6) + (num2 << 3) + num3));
                        bm.SetPixel(j, i, Color.FromArgb(red, green, blue));
                        if (dithering)
                        {
                            AddPixelRGB(ref bm, j + 1, i, num4 * 7 >> 4, num5 * 7 >> 4, num6 * 7 >> 4);
                            AddPixelRGB(ref bm, j - 1, i + 1, num4 * 3 >> 4, num5 * 3 >> 4, num6 * 3 >> 4);
                            AddPixelRGB(ref bm, j, i + 1, num4 * 5 >> 4, num5 * 5 >> 4, num6 * 5 >> 4);
                            AddPixelRGB(ref bm, j + 1, i + 1, num4 >> 4, num5 >> 4, num6 >> 4);
                        }
                    }
                    text.Append("\n");
                }
                if (original != null)
                {
                    original.Dispose();
                }
                Result = bm.Clone(new Rectangle(0, 0, bm.Width, bm.Height), PixelFormat.Undefined);
                bm.Dispose();
                return text.ToString();
            }
            catch (Exception ex)
            {
                Result = original;
                return "Error: "+ ex.Message;
            }
        }
        public static string ConvertToSuperPixel(Image original, int width, int height, bool for_r,bool saspect, out Image Result)
        {
            try
            {
                StringBuilder text = new StringBuilder();
                if (!for_r) text.Append('-');
                int LineAdder = 0;
                int discro = 0;
                if (saspect)
                {
                    int oldHeight = height;
                    double aspect = (double)original.Width / (double)original.Height;
                    height = (int)(width / aspect);
                    if (height <= oldHeight)
                    {
                        discro = (oldHeight - height) / 2;
                        if (discro > 0)
                            for (int i = 0; i < discro; i++)
                            {
                                text.Append("\n");
                            }
                    }
                    else
                    {
                        height = oldHeight;
                        int oldWidth = width;
                        width = (int)(height * aspect);
                        LineAdder = (oldWidth - width) / 2;
                    }
                }
                Bitmap bm = new Bitmap(original, width, height);
                for (int i = 0; i < bm.Height; i++)
                {
                    if (LineAdder > 0)
                        for (int d = 0; d < LineAdder; d++)
                        {
                            text.Append((char)(ushort)57344);
                        }
                    for (int j = 0; j < bm.Width; j++)
                    {
                        Color pixel = bm.GetPixel(j, i);
                        int num = pixel.R;
                        int num2 = pixel.G >> 4;
                        int num3 = pixel.B;
                        int num4 = (pixel.G % 16);
                        if (for_r) text.Append((char)(ushort)(57344 + (num2 << 8) + num));
                        else text.Append((char)(ushort)(57344 + (num4 << 8) + num3));
                        bm.SetPixel(j, i, Color.FromArgb(pixel.R, pixel.G, pixel.B));
                    }
                    if(i != bm.Height-1) text.Append("\n");
                }
                if (discro > 0)
                    for (int i = 0; i < discro; i++)
                    {
                        text.Append("\n");
                    }
                if (original != null)
                {
                    original.Dispose();
                }
                Result = bm.Clone(new Rectangle(0, 0, bm.Width, bm.Height), PixelFormat.Undefined);
                bm.Dispose();
                return text.ToString();
            }
            catch (Exception ex)
            {
                Result = original;
                return "Error: " + ex.Message;
            }
        }
        public static Image ConvertFromMonospce(string Monospace)
        {
            try
            {
                string[] Lines = Monospace.Split('\n'); int X = 0, Y = 0;
                Bitmap Bmp = new Bitmap(Lines.First(x => x.Length > 0).Length, Lines.Length - 1);
                foreach (string Ziline in Lines)
                {
                    foreach (Char Chared in Ziline)
                    {
                        if (X < Bmp.Width) Bmp.SetPixel(X, Y, ColorUtils.CharToColor(Chared));
                        X++;
                    }
                    X = 0;
                    Y++;
                }
                return Bmp;
            }
            catch
            {
                return null;
            }
        }
        private static void AddPixelRGB(ref Bitmap bm, int x, int y, int R, int G, int B)
        {
            if ((x > 0) & (x < bm.Width) & (y > 0) & (y < bm.Height))
            {
                Color pixel = bm.GetPixel(x, y);
                R = Math.Min(255, pixel.R + R);
                G = Math.Min(255, pixel.G + G);
                B = Math.Min(255, pixel.B + B);
                bm.SetPixel(x, y, Color.FromArgb(R, G, B));
            }
        }
        public static BitmapImage ToSource(Image src)
        {
            using (var ms = new MemoryStream())
            {
                src.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
        public static System.Drawing.Bitmap FromSource(BitmapSource bitmapSource)
        {
            var width = bitmapSource.PixelWidth;
            var height = bitmapSource.PixelHeight;
            var stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            var memoryBlockPointer = Marshal.AllocHGlobal(height * stride);
            bitmapSource.CopyPixels(new Int32Rect(0, 0, width, height), memoryBlockPointer, height * stride, stride);
            var bitmap = new Bitmap(width, height, stride, PixelFormat.Format32bppPArgb, memoryBlockPointer);
            return bitmap;
        }
        public static class ColorUtils
        {

            public static Color CharToColor(char Ch)
            {
                int Chr = (int)Ch - 0xe100, pr, pg, pb, r, g, b;
                BitArray Bin = new BitArray(BitConverter.GetBytes(Chr));
                pr = (int)((Bin[0] ? 1 : 0) + ((Bin[1] ? 1 : 0) << 1) + ((Bin[2] ? 1 : 0) << 2));
                pg = (int)((Bin[3] ? 1 : 0) + ((Bin[4] ? 1 : 0) << 1) + ((Bin[5] ? 1 : 0) << 2));
                pb = (int)((Bin[6] ? 1 : 0) + ((Bin[7] ? 1 : 0) << 1) + ((Bin[8] ? 1 : 0) << 2));
                r = (int)(((float)pb / 7) * 255);
                g = (int)(((float)pg / 7) * 255);
                b = (int)(((float)pr / 7) * 255);
                return Color.FromArgb(r, g, b);
            }
            private static bool[] GetBinaryRepresentation(int i)
            {
                List<bool> result = new List<bool>();
                while (i > 0)
                {
                    int m = i % 2;
                    i = i / 2;
                    result.Add(m == 1);
                }
                result.Reverse();
                return result.ToArray();
            }
            public static char CharRGB(byte r = 7, byte g = 7, byte b = 7)
            {
                return (char)(0xe100 + (r << 6) + (g << 3) + b);
            }
            public static Color ColorFromHSV(double H, double S, double V)
            {
                int r, g, b;
                HsvToRgb(H, S, V, out r, out g, out b);
                return Color.FromArgb(r, g, b);
            }
            /// <summary>
            /// Convert HSV to RGB
            /// h is from 0-360
            /// s,v values are 0-1
            /// r,g,b values are 0-255
            /// Based upon http://ilab.usc.edu/wiki/index.php/HSV_And_H2SV_Color_Space#HSV_Transformation_C_.2F_C.2B.2B_Code_2
            /// </summary>
            static void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
            {
                // ######################################################################
                // T. Nathan Mundhenk
                // mundhenk@usc.edu
                // C/C++ Macro HSV to RGB

                double H = h;
                while (H < 0) { H += 360; };
                while (H >= 360) { H -= 360; };
                double R, G, B;
                if (V <= 0)
                { R = G = B = 0; }
                else if (S <= 0)
                {
                    R = G = B = V;
                }
                else
                {
                    double hf = H / 60.0;
                    int i = (int)Math.Floor(hf);
                    double f = hf - i;
                    double pv = V * (1 - S);
                    double qv = V * (1 - S * f);
                    double tv = V * (1 - S * (1 - f));
                    switch (i)
                    {

                        // Red is the dominant color

                        case 0:
                            R = V;
                            G = tv;
                            B = pv;
                            break;

                        // Green is the dominant color

                        case 1:
                            R = qv;
                            G = V;
                            B = pv;
                            break;
                        case 2:
                            R = pv;
                            G = V;
                            B = tv;
                            break;

                        // Blue is the dominant color

                        case 3:
                            R = pv;
                            G = qv;
                            B = V;
                            break;
                        case 4:
                            R = tv;
                            G = pv;
                            B = V;
                            break;

                        // Red is the dominant color

                        case 5:
                            R = V;
                            G = pv;
                            B = qv;
                            break;

                        // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                        case 6:
                            R = V;
                            G = tv;
                            B = pv;
                            break;
                        case -1:
                            R = V;
                            G = pv;
                            B = qv;
                            break;

                        // The color is not defined, we should throw an error.

                        default:
                            //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                            R = G = B = V; // Just pretend its black/white
                            break;
                    }
                }
                r = Clamp((int)(R * 255.0));
                g = Clamp((int)(G * 255.0));
                b = Clamp((int)(B * 255.0));
            }

            /// <summary>
            /// Clamp a value to 0-255
            /// </summary>
            static int Clamp(int i)
            {
                if (i < 0) return 0;
                if (i > 255) return 255;
                return i;
            }
        }
    }
}
