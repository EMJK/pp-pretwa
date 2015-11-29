using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace Pretwa.Gui
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var state = Board.defaultBoardState;
            ShowState(state);
            state = Board.applyMove(FieldCoords.NewEdge(0, 0), FieldCoords.Center, state).Item1;
            ShowState(state);
            state = Board.applyMove(FieldCoords.NewEdge(0, 3), FieldCoords.NewEdge(0, 0), state).Item1;
            ShowState(state);
            Console.ReadKey();
        }

        private static void ShowState(FSharpMap<FieldCoords, FieldState> state)
        {
            new VisualState(state, Board.allValidMoves(state)).ShowDrawing();
        }
    }
}
