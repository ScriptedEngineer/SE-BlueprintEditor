using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
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
    /// Логика взаимодействия для Reporter.xaml
    /// </summary>
    public partial class Reporter : Window
    {
        public Reporter()
        {
            InitializeComponent();
            Who.Text = MySettings.Current.UserName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Run.IsChecked.Value)
            {
                SelectBlueprint.window.Show();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var x = MyExtensions.ApiServer(ApiServerAct.Report, ApiServerOutFormat.@string,
                ",\"body\":\"" + ("Crash Report:" +
                "<br>Sender: " + Who.Text +
                "<br><br>Comment: <br>" + What.Text + //
                "<br><br>Log:<br>" + File.ReadAllText("LastCrash.txt") +
                "<br><br>PC: <br>" + GetPCInfo() +
                "<br><br>Settings: <br>"+(File.Exists("settings.xml") ? File.ReadAllText("settings.xml").Replace("<", "&lt;") : "")
                ).Replace("\n", "<br>").Replace("\r", "").Replace("\"", "'").Replace("\\", "\\\\") + "\"");
            Button_Click(x, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        public string GetPCInfo()
        {
            return "Processor: " + GetProcessorInformation() + "<br>Video: " + GetVideoProcessorInformation() + "<br>Board: " + GetBoardProductId() + "<br>Disc: " + GetDisckModel() + "<br>Mem: " + GetPhysicalMemory() + "<br>OS: " + GetOSInformation();
        }
        public static string GetPhysicalMemory()
        {
            ManagementScope oMs = new ManagementScope();
            ObjectQuery oQuery = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
            ManagementObjectCollection oCollection = oSearcher.Get();

            long MemSize = 0;
            long mCap = 0;

            // In case more than one Memory sticks are installed
            foreach (ManagementObject obj in oCollection)
            {
                mCap = Convert.ToInt64(obj["Capacity"]);
                MemSize += mCap;
            }
            MemSize = (MemSize / 1024) / 1024;
            return MemSize.ToString() + "MB";
        }
        public static string GetOSInformation()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return ((string)wmi["Caption"]).Trim() + ", " + (string)wmi["Version"] + ", " + (string)wmi["OSArchitecture"];
                }
                catch { }
            }
            return "BIOS Maker: Unknown";
        }
        public static string GetProcessorInformation()
        {
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();
            string info = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                string name = (string)mo["Name"];
                name = name.Replace("(TM)", "™").Replace("(tm)", "™").Replace("(R)", "®").Replace("(r)", "®").Replace("(C)", "©").Replace("(c)", "©").Replace("    ", " ").Replace("  ", " ");

                info = name + ", " + (string)mo["Caption"] + ", " + (string)mo["SocketDesignation"];
                //mo.Properties["Name"].Value.ToString();
                //break;
            }
            return info;
        }
        public static string GetVideoProcessorInformation()
        {
            ManagementClass mc = new ManagementClass("Win32_VideoController");
            ManagementObjectCollection moc = mc.GetInstances();
            string info = String.Empty;
            foreach (ManagementObject mo in moc)
            {
                info = (string)mo["Caption"];
            }
            return info;
        }
        public static string GetBoardProductId()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Product").ToString();

                }

                catch { }

            }

            return "Product: Unknown";

        }
        public static string GetDisckModel()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive");

            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Model").ToString();

                }

                catch { }

            }

            return "Model: Unknown";

        }
    }
}
