using System;
using Application = System.Windows.Forms.Application;

namespace Pretwa.Gui
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.Run(new MainForm());
        }
    }
}