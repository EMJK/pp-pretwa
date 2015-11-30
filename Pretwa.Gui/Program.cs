using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace Pretwa.Gui
{
    static class Program
    {
        static AutoResetEvent handle = new AutoResetEvent(false);
        [STAThread]
        static void Main(string[] args)
        {
            var gui = new VisualState();
            var state = Board.defaultBoardState;
            gui.EnterPressed += () => handle.Set();
            gui.Show();

            ShowState(state, gui);
            WaitForEnter();
            state = Board.applyMove(FieldCoords.NewEdge(0, 0), FieldCoords.Center, state).Item1;

            ShowState(state, gui);
            WaitForEnter();
            state = Board.applyMove(FieldCoords.NewEdge(0, 3), FieldCoords.NewEdge(0, 0), state).Item1;

            ShowState(state, gui);
            WaitForEnter();
            state = Board.applyMove(FieldCoords.NewEdge(0, 1), FieldCoords.Center, state).Item1;

            ShowState(state, gui);
            WaitForEnter();
            state = Board.applyMove(FieldCoords.NewEdge(0, 0), FieldCoords.NewEdge(0, 3), state).Item1;

            ShowState(state, gui);
            WaitForEnter();
        }

        static void WaitForEnter()
        {
            handle.WaitOne();
        }

        private static void ShowState(FSharpMap<FieldCoords, FieldState> state, VisualState gui)
        {
            gui.Draw(state, Board.allValidMoves(state));
        }
    }
}
