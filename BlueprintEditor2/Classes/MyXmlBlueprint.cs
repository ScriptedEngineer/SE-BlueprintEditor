using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml;

namespace BlueprintEditor2
{
    public class MyXmlBlueprint
    {
        internal readonly int BlockCount;
        private readonly XmlDocument _BlueprintXml = new XmlDocument();
        internal readonly MyXmlGrid[] Grids;
        internal readonly string Patch;

        public string Name //TODO: возможен NullReference
        {
            get => _BlueprintXml.GetElementsByTagName("Id")[0].Attributes["Subtype"].Value;
            set => _BlueprintXml.GetElementsByTagName("Id")[0].Attributes["Subtype"].Value = value;
        }
        public string DisplayName => _BlueprintXml.GetElementsByTagName("DisplayName")[0].InnerText;
        public string Owner => _BlueprintXml.GetElementsByTagName("OwnerSteamId")[0].InnerText;

        public MyXmlBlueprint(string patch)
        {
            Patch = patch;
            _BlueprintXml.Load(Patch + "\\bp.sbc");
            XmlNodeList grids = _BlueprintXml.GetElementsByTagName("CubeGrid");
            Grids = new MyXmlGrid[grids.Count];
            for (int i = 0; i < grids.Count; i++)
            {
                Grids[i] = new MyXmlGrid(grids[i]);
                BlockCount += Grids[i].Blocks.Length;
            }
        }

        public void SaveBackup(bool forced = false)
        {
            if (Directory.Exists(Patch + "/Backups"))
            {
                if (forced)
                {
                    File.Copy(Patch + "\\bp.sbc", Patch + "\\Backups\\" + DateTime.UtcNow.ToFileTimeUtc() + ".sbc");
                    return;
                }

                bool save = true;
                foreach (string file in Directory.GetFiles(Patch + "/Backups"))
                {
                    if (file.Contains("Lastest-")) continue;
                    DateTime fileDate = DateTime.FromFileTimeUtc(long.Parse(file.Split('\\').Last().Replace(".sbc", "")));
                    if ((DateTime.UtcNow - fileDate).TotalMinutes < 5) save = false;
                    else if (new TimeSpan(fileDate.Ticks).TotalDays > 2) File.Delete(file);
                }

                if (save) File.Copy(Patch + "\\bp.sbc", Patch + "\\Backups\\" + DateTime.UtcNow.ToFileTimeUtc() + ".sbc");
            }
            else
            {
                Directory.CreateDirectory(Patch + "\\Backups");
                File.Copy(Patch + "\\bp.sbc", Patch + "\\Backups\\Lastest-" + DateTime.UtcNow.ToFileTimeUtc() + ".sbc");
            }
        }

        public BitmapSource GetPic(bool badOpacity = false)
        {
            if (badOpacity)
            {
                Image raw = Image.FromFile(Patch + "\\thumb.png");
                Image img = SetImgOpacity(raw, 1);
                raw.Dispose();
                img.Save(Patch + "\\thumb.png");
                img.Dispose();
            }

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(Patch + "\\thumb.png");
            image.EndInit(); //TODO: Файл не найден (ПКМ на чертеже в меню выбора)
            return image;
            //return new BitmapImage(new Uri(Patch + "\\thumb.png"));
        }

        private static Image SetImgOpacity(Image imgPic, float imageOpacity)
        {
            Bitmap bmpPic = new Bitmap(imgPic.Width, imgPic.Height);
            Graphics gfxPic = Graphics.FromImage(bmpPic);
            ColorMatrix cmxPic = new ColorMatrix
            {
                Matrix33 = imageOpacity,
                Matrix23 = imageOpacity,
                Matrix13 = imageOpacity,
                Matrix03 = imageOpacity,
                Matrix43 = imageOpacity
            };
            ImageAttributes iaPic = new ImageAttributes();
            iaPic.SetColorMatrix(cmxPic, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            gfxPic.DrawImage(imgPic, new Rectangle(0, 0, bmpPic.Width, bmpPic.Height), 0, 0, imgPic.Width, imgPic.Height, GraphicsUnit.Pixel, iaPic);
            gfxPic.Dispose();
            return bmpPic;
        }
    }
}
