using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace BlueprintEditor2
{
    class MyExtensions
    {
        public static string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static void AsyncWorker(Action act) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, act);
        public static string ApiServer(ApiServerAct Actione, ApiServerOutFormat Formate = ApiServerOutFormat.@string)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    return client.UploadString("https://wsxz.ru/api/"+ Actione.ToString() + "/"+ Formate.ToString(),
                        "{\"token\":\"J1H8MHUpN7N8BPZg9f9m6tf7NVHspVYo\",\"app\":\"SEBE2\",\"version\":\"" +
                         Version + "\"}");
                }
            }
            catch
            {
                return "Error(Api unavailable)";
            }
        }
        public static string AppFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
        public static void CloseAllWindows()
        {
            for (int intCounter = Application.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
                Application.Current.Windows[intCounter].Hide();
        }
        public class SortAdorner : Adorner
        {
            private readonly static Geometry _AscGeometry =
                Geometry.Parse("M 0,0 L 10,0 L 5,5 Z");
            private readonly static Geometry _DescGeometry =
                Geometry.Parse("M 0,5 L 10,5 L 5,0 Z");

            public ListSortDirection Direction { get; private set; }

            public SortAdorner(UIElement element, ListSortDirection dir)
              : base(element)
            { Direction = dir; }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (AdornedElement.RenderSize.Width < 20)
                    return;

                drawingContext.PushTransform(
                    new TranslateTransform(
                      AdornedElement.RenderSize.Width - 15,
                      (AdornedElement.RenderSize.Height - 5) / 2));

                drawingContext.DrawGeometry(Brushes.Black, null,
                    Direction == ListSortDirection.Ascending ?
                      _AscGeometry : _DescGeometry);

                drawingContext.Pop();
            }
        }
    }
    
    public enum ApiServerAct
    {
        CheckVersion,
        GetUpdateLog
    }
    public enum ApiServerOutFormat
    {
        @string,
        @bool,
        json,
        xml
    }

}
