using System;
using System.Windows.Forms;
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
            var nextmove = Board.getPlayer;
            var color = Board.getColor;
            gui.EnterPressed += () => handle.Set();
            gui.Show();

            while (true)
            {
                do
                {
                    ShowState(state, gui, nextmove);
                    if (Board.hasPlayerLost(FieldState.NewColor(Player.Black), state) ||
                        Board.hasPlayerLost(FieldState.NewColor(Player.Red), state))
                    {
                        System.Windows.Forms.MessageBox.Show("Koniec gry");
                        return;
                    }

                    Tuple<FSharpMap<FieldCoords, FieldState>, NextMove> nextState;

                    while (true)
                    {
                        try
                        {
                            string moveFrom = Microsoft.VisualBasic.Interaction.InputBox(
                                "Wybierz pionek", "Ruch etap 1",
                                "", 0, 0);
                            char[] pionek = moveFrom.ToCharArray();

                            string moveTo = Microsoft.VisualBasic.Interaction.InputBox(
                                "Wybierz pole do przesunięcia pionka", "Ruch etap 2",
                                "", 0, 0);
                            char[] poleDocelowe = moveTo.ToCharArray();

                            //współrzędne:
                            FieldCoords zrodlowe;
                            if (pionek[0] == 'C')
                                zrodlowe = FieldCoords.Center;
                            else
                            {
                                var pionekX = Int32.Parse(pionek[0].ToString());
                                var pionekY = Int32.Parse(pionek[1].ToString());
                                zrodlowe = FieldCoords.NewEdge(pionekX, pionekY);
                            }

                            FieldCoords docelowe;
                            if (poleDocelowe[0] == 'C')
                                docelowe = FieldCoords.Center;
                            else
                            {
                                var poleDoceloweX = Int32.Parse(poleDocelowe[0].ToString());
                                var poleDoceloweY = Int32.Parse(poleDocelowe[1].ToString());
                                docelowe = FieldCoords.NewEdge(poleDoceloweX, poleDoceloweY);
                            }
                            nextState = Board.applyMove(zrodlowe, docelowe, state);
                            break;
                        }
                        catch (Exception)
                        {
                            System.Windows.Forms.MessageBox.Show("Nieprawidłowy ruch!");
                        }
                    }

                    
                    state = nextState.Item1;
                    nextmove = nextState.Item2;
                } while (nextmove.IsColor == false);

                do
                {
                    ShowState(state, gui, nextmove);
                    if (Board.hasPlayerLost(FieldState.NewColor(Player.Black), state))
                    {
                        System.Windows.Forms.MessageBox.Show("Koniec gry. Zwyciężył gracz czerwony");
                        return;
                    }
                    if (Board.hasPlayerLost(FieldState.NewColor(Player.Red), state))
                    {
                        System.Windows.Forms.MessageBox.Show("Koniec gry. Zwyciężył gracz czarny");
                        return;
                    } 
                    System.Windows.Forms.MessageBox.Show("Ruch komputera, wciśnij Enter");
                    Tuple<FSharpMap<FieldCoords, FieldState>, NextMove> tuple = Board.moveOfTheComputer(state, nextmove);
                    state = tuple.Item1;
                    nextmove = tuple.Item2;

                } while (nextmove.IsColor == false); 
            }

        }

        static void WaitForEnter()
        {
            handle.WaitOne();
        }

        private static void ShowState(FSharpMap<FieldCoords, FieldState> state, VisualState gui, NextMove nextmove)
        {
            gui.Draw(state, Board.allValidMoves(nextmove, state));
        }
    }
}
