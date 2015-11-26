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
            var t = Pretwa.Board.defaultBoardState;
            foreach (var item in t)
            {
                string coordinates = item.Key.IsCenter
                    ? "(Center)"
                    : $"({((FieldCoordinates.Edge)item.Key).Item1},{((FieldCoordinates.Edge)item.Key).Item2})";

                string value = 
                    item.Value.IsBlack ? "Black" :
                    item.Value.IsRed ? "Red" :
                    "Empty";

                Console.WriteLine($"{coordinates}: {value}");
            }

            new Application().Run(new Windows.MainWindow());
        }
    }
}
