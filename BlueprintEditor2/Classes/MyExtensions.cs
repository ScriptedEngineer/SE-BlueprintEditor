using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BlueprintEditor2
{
    class MyExtensions
    {
        public static void AsyncWorker(Action act) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, act);
    }
    public enum DialogPicture
    {
        warn,
        attention,
        question
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
