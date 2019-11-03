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
using System.Reflection;
using BlueprintEditor2.Resource;
using System.Threading;
using System.Windows;

namespace BlueprintEditor2
{
    public class MyXmlBlueprint
    {
        public string Patch;
        private XmlDocument BlueprintXml = new XmlDocument();
        public MyXmlGrid[] Grids;
        public string Name
        {
            get => BlueprintXml.GetElementsByTagName("Id").Item(0).Attributes["Subtype"].Value;
            set => BlueprintXml.GetElementsByTagName("Id").Item(0).Attributes["Subtype"].Value = value;
        }
        public string DisplayName
        {
            get => BlueprintXml.GetElementsByTagName("DisplayName").Item(0).InnerText;
        }
        public string Owner
        {
            get => BlueprintXml.GetElementsByTagName("OwnerSteamId").Item(0).InnerText;
        }
        public MyXmlBlueprint(string patch)
        {
            Patch = patch;
            if (File.Exists(Patch + "\\bp.sbc"))
            {
                BlueprintXml.Load(Patch + "\\bp.sbc");
                XmlNodeList Grides = BlueprintXml.GetElementsByTagName("CubeGrid");
                Grids = new MyXmlGrid[Grides.Count];
                for (int i = 0; i < Grides.Count; i++)
                {
                    Grids[i] = new MyXmlGrid(Grides[i]);
                }
            }
        }
        public void Save(bool forced = false)
        {
            if (Directory.Exists(Patch))
            {
                BlueprintXml.Save(Patch + "\\bp.sbc");
            }
            
        }
        public void SaveBackup(bool forced = false)
        {
            if (Directory.Exists(Patch + "/Backups"))
            {
                if (forced)
                {
                    File.Copy(Patch + "\\bp.sbc", Patch + "\\Backups\\" + DateTime.UtcNow.ToFileTimeUtc().ToString() + ".sbc");
                    return;
                }
                bool save = true;
                string Lastest = "",LastestName = "";
                List<string> Files = new List<string>();
                foreach (string file in Directory.GetFiles(Patch + "/Backups"))
                {
                    if (file.Contains("Lastest-"))
                    {
                        Lastest = File.ReadAllText(file);
                        LastestName = file;
                        continue;
                    }
                    DateTime FileDate = DateTime.FromFileTimeUtc(long.Parse(file.Split('\\').Last().Replace(".sbc", "")));
                    if (Files.Contains(File.ReadAllText(file)))
                    {
                        File.Delete(file);
                    }
                    else
                    {
                        Files.Add(File.ReadAllText(file));
                    }
                }
                if (save && Lastest != File.ReadAllText(Patch + "\\bp.sbc"))
                {
                    if(LastestName != "") File.Move(LastestName,Path.GetDirectoryName(LastestName)+"\\"+Path.GetFileName(LastestName).Replace("Lastest-",""));
                    File.Copy(Patch + "\\bp.sbc", Patch + "\\Backups\\" + "Lastest-" + DateTime.UtcNow.ToFileTimeUtc().ToString() + ".sbc");
                }
            }
            else
            {
                Directory.CreateDirectory(Patch + "\\Backups");
                File.Copy(Patch + "\\bp.sbc", Patch + "\\Backups\\Lastest-" + DateTime.UtcNow.ToFileTimeUtc().ToString() + ".sbc");
            }
        }
        public BitmapImage GetPic(bool badOpac = false,bool useDialog = true)
        {
            if (!File.Exists(Patch + "\\thumb.png") || new FileInfo(Patch + "\\thumb.png").Length == 0)
            {
                if (useDialog)
                {
                    SelectBlueprint.window.Lock.Height = SystemParameters.PrimaryScreenHeight;
                    MyExtensions.AsyncWorker(() =>
                    {
                        new MessageDialog(DialogPicture.question, Lang.NoPic+" ["+ Name+"]", Lang.NoPicture, (Dial) =>
                            {
                                switch (Dial)
                                {
                                    case DialоgResult.Yes:
                                        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                                        dlg.DefaultExt = ".png";
                                        dlg.Filter = Lang.ImFiles + "|*.png;*.jpeg;*.jpg";
                                        bool? result = dlg.ShowDialog();
                                        if (result == true)
                                        {
                                            string filename = dlg.FileName;
                                            File.Copy(filename, Patch + "\\thumb.png");
                                        }
                                        SelectBlueprint.window.Lock.Height = 0;
                                        SelectBlueprint.window.BluePicture.Source = SelectBlueprint.window.CurrentBlueprint.GetPic();
                                        break;
                                    case DialоgResult.No:
                                        Properties.Resources.thumbDefault.Save(Patch + "\\thumb.png");
                                        SelectBlueprint.window.Lock.Height = 0;
                                        SelectBlueprint.window.BluePicture.Source = SelectBlueprint.window.CurrentBlueprint.GetPic();
                                        break;
                                    default:
                                        SelectBlueprint.window.Lock.Height = 0;
                                        break;
                                }
                            },DialogType.Cancelable).Show();
                    });
                }
                return new BitmapImage(new Uri("pack://application:,,,/Resource/thumbDefault.png"));
            }
            else
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
            }
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
