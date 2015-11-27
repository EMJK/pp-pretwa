using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

namespace Pretwa.Gui
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new Application().Run(new Windows.MainWindow());
        }
    }
}
