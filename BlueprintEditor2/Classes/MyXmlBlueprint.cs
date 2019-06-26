using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;

namespace BlueprintEditor2
{
    public class MyXmlBlueprint
    {
        public string Patch;
        public readonly int BlockCount;
        private XmlDocument BlueprintXml = new XmlDocument();
        public MyXmlGrid[] Grids;
        public string Name
        {
            get => BlueprintXml.GetElementsByTagName("Id")[0].Attributes["Subtype"].Value;
            set => BlueprintXml.GetElementsByTagName("Id")[0].Attributes["Subtype"].Value = value;
        }
        public string DisplayName
        {
            get => BlueprintXml.GetElementsByTagName("DisplayName")[0].InnerText;
        }
        public string Owner
        {
            get => BlueprintXml.GetElementsByTagName("OwnerSteamId")[0].InnerText;
        }
        public MyXmlBlueprint(string patch)
        {
            Patch = patch;
            BlueprintXml.Load(Patch + "\\bp.sbc");
            XmlNodeList Grides = BlueprintXml.GetElementsByTagName("CubeGrid");
            Grids = new MyXmlGrid[Grides.Count];
            for (int i = 0; i < Grides.Count; i++)
            {
                Grids[i] = new MyXmlGrid(Grides[i]);
                BlockCount += Grids[i].Blocks.Length;
            }
        }
        public void SaveBackup(bool forced = false)
        {
            if (Directory.Exists(Patch+"/Backups"))
            {
                if (forced)
                {
                    File.Copy(Patch + "\\bp.sbc", Patch + "\\Backups\\" + DateTime.UtcNow.ToFileTimeUtc().ToString() + ".sbc");
                    return;
                }
                bool save = true;
                foreach (string file in Directory.GetFiles(Patch + "/Backups"))
                {
                    if (file.Contains("Lastest-")) continue;
                    DateTime FileDate = DateTime.FromFileTimeUtc(long.Parse(file.Split('\\').Last().Replace(".sbc", "")));
                    if((DateTime.UtcNow-FileDate).TotalMinutes < 5)
                    {
                        save = false;
                    }
                    else if (new TimeSpan(FileDate.Ticks).TotalDays > 2)
                    {
                        File.Delete(file);
                    }
                }
                if(save) File.Copy(Patch + "\\bp.sbc", Patch + "\\Backups\\" + DateTime.UtcNow.ToFileTimeUtc().ToString() + ".sbc");
            }
            else
            {
                Directory.CreateDirectory(Patch + "\\Backups");
                File.Copy(Patch + "\\bp.sbc", Patch + "\\Backups\\Lastest-" + DateTime.UtcNow.ToFileTimeUtc().ToString() + ".sbc");
            }
        }
        public BitmapSource GetPic(bool badOpac = false)
        {
            if (badOpac)
            {
                Image raw = Image.FromFile(Patch + "\\thumb.png");
                Image img = SetImgOpacity(raw, 1);
                raw.Dispose();
                img.Save(Patch + "\\thumb.png");
                img.Dispose();
            }
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(Patch + "\\thumb.png");
            image.EndInit();
            return image;
            //return new BitmapImage(new Uri(Patch + "\\thumb.png"));
        }
        private static Image SetImgOpacity(Image imgPic, float imgOpac)
        {
            Bitmap bmpPic = new Bitmap(imgPic.Width, imgPic.Height);
            Graphics gfxPic = Graphics.FromImage(bmpPic);
            ColorMatrix cmxPic = new ColorMatrix();
            cmxPic.Matrix33 = imgOpac;
            cmxPic.Matrix23 = imgOpac;
            cmxPic.Matrix13 = imgOpac;
            cmxPic.Matrix03 = imgOpac;
            cmxPic.Matrix43 = imgOpac;
            ImageAttributes iaPic = new ImageAttributes();
            iaPic.SetColorMatrix(cmxPic, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            gfxPic.DrawImage(imgPic, new Rectangle(0, 0, bmpPic.Width, bmpPic.Height), 0, 0, imgPic.Width, imgPic.Height, GraphicsUnit.Pixel, iaPic);
            gfxPic.Dispose();
            return bmpPic;
        }
    }
}
