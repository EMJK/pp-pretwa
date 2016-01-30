using Microsoft.FSharp.Collections;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Pretwa.Gui
{
    public partial class MainForm : Form
    {
        private FSharpMap<FieldCoords, FieldState> _State;
        private Player _CurrentPlayer;
        private bool _EnableAI;

        public MainForm()
        {
            InitializeComponent();
            visualState1.Enabled = false;
            visualState1.DrawOnlyGrid();
            visualState1.MoveRequested += VisualState1OnMoveRequested;
        }

        private void StartNewGame()
        {
            visualState1.Enabled = true;
            _State = Board.defaultBoardState;
            _CurrentPlayer = Player.Black;
            var moves = Board.validMovesForColor(FieldState.NewColor(Player.Black), _State);
            visualState1.CurrentPlayer = Player.Black;
            visualState1.Draw(_State, moves);
        }

        private void VisualState1OnMoveRequested(object sender, Tuple<FieldCoords, FieldCoords> tuple)
        {
            var moveResult = Board.applyMove(tuple.Item1, tuple.Item2, _State);
            _State = moveResult.Item1;
            var validMoves = Board.allValidMoves(moveResult.Item2, moveResult.Item1);
            _CurrentPlayer = GetNextPlayer(moveResult.Item2);
            visualState1.CurrentPlayer = _CurrentPlayer;
            visualState1.Draw(_State, validMoves);

            if (CheckAndDisplayWinner())
            {
                visualState1.Enabled = false;
                return;
            }

            if (_CurrentPlayer.IsRed && _EnableAI)
            {
                this.Enabled = false;
                MakeAIMove(moveResult.Item2);
            }
        }

        private bool CheckAndDisplayWinner()
        {
            string winner = null;
            if (Board.hasPlayerLost(FieldState.NewColor(Player.Black), _State)) winner = "czerwony";
            if (Board.hasPlayerLost(FieldState.NewColor(Player.Red), _State)) winner = "czarny";
            if (winner == null) return false;
            MessageBox.Show($"Koniec gry! Zwyciężył gracz {winner}.", "Koniec gry");
            return true;
        }

        private void MakeAIMove(NextMove nextMove)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                while (_CurrentPlayer.IsRed)
                {
                    Thread.Sleep(750);
                    var move = Board.moveOfTheComputer(_State, nextMove);
                    var moveResult = Board.applyMove(move.Item1, move.Item2, _State);
                    _State = moveResult.Item1;
                    _CurrentPlayer = GetNextPlayer(moveResult.Item2);
                    visualState1.CurrentPlayer = _CurrentPlayer;
                    var validMoves = Board.allValidMoves(moveResult.Item2, moveResult.Item1);
                    visualState1.Draw(moveResult.Item1, validMoves);
                    if (CheckAndDisplayWinner())
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            this.Enabled = true;
                            visualState1.Enabled = false;
                        }));
                        return;
                    }
                }
                this.Invoke(new MethodInvoker(() => { this.Enabled = true; }));
            });
        }

        private Player GetNextPlayer(NextMove nextMove)
        {
            if (nextMove.IsColor)
            {
                return ((NextMove.Color)nextMove).Item;
            }
            var coords = ((NextMove.Piece)nextMove).Item;
            var field = _State[coords];
            return ((FieldState.Color)field).Item;
        }

        private void btn_SinglePlayer_Click(object sender, EventArgs e)
        {
            _EnableAI = true;
            StartNewGame();
        }

        private void btn_MultiPlayer_Click(object sender, EventArgs e)
        {
            _EnableAI = false;
            StartNewGame();
        }
    }
}