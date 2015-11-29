using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace Pretwa.Gui
{
    class VisualState
    {
        private readonly FSharpMap<FieldCoords, FieldState> _State;
        private readonly FSharpList<Tuple<FieldCoords, FSharpOption<FieldCoords>, FieldCoords>> _ValidMoves;
        private int _CanvasSize = 700;
        private int _LineSize = 100;
        private int _FieldSize = 20;

        public VisualState(FSharpMap<FieldCoords, FieldState> state, FSharpList<Tuple<FieldCoords, FSharpOption<FieldCoords>, FieldCoords>> validMoves)
        {
            _State = state;
            _ValidMoves = validMoves;
        }

        public void ShowDrawing()
        {
            Form f = new Form();
            f.Size = new Size(_CanvasSize, _CanvasSize);
            var p = new PictureBox();
            p.Size = new Size(_CanvasSize, _CanvasSize);
            f.Controls.Add(p);
            p.Dock = DockStyle.Fill;

            Bitmap bmp = new Bitmap(_CanvasSize, _CanvasSize);
            Graphics canvas = Graphics.FromImage(bmp);

            DrawFields(canvas);

            DrawPossibleMoves(canvas);

            canvas.Flush();
            canvas.Dispose();

            p.Image = bmp;
            f.ShowDialog();
        }

        private void DrawPossibleMoves(Graphics canvas)
        {
            var jumps = new List<Tuple<FieldCoords, FSharpOption<FieldCoords>, FieldCoords>>();
            foreach (var move in _ValidMoves)
            {
                if (move.Item2 == null)
                {
                    DrawArrow(canvas, move.Item1, move.Item3, false);
                }
                else
                {
                    jumps.Add(move);
                }
            }
            foreach (var move in jumps)
            {
                DrawArrow(canvas, move.Item1, move.Item3, true);
            }
        }

        private void DrawArrow(Graphics canvas, FieldCoords from, FieldCoords to, bool jump)
        {
            Point pFrom, pTo;
            {
                int en = from.IsCenter ? -1 : ((FieldCoords.Edge) from).Item1;
                int fn = from.IsCenter ? -1 : ((FieldCoords.Edge) from).Item2;
                pFrom = GetFieldCoords(en, fn);
            }
            {
                int en = to.IsCenter ? -1 : ((FieldCoords.Edge)to).Item1;
                int fn = to.IsCenter ? -1 : ((FieldCoords.Edge)to).Item2;
                pTo = GetFieldCoords(en, fn);
            }

            Pen pen = jump 
                ? new Pen(Color.MediumPurple, 2)
                : new Pen(Color.Orange, 6);


            canvas.DrawLine(pen, pFrom, pTo);
        }

        private void DrawFields(Graphics canvas)
        {
            foreach (var field in _State)
            {
                int en = field.Key.IsCenter ? -1 : ((FieldCoords.Edge) field.Key).Item1;
                int fn = field.Key.IsCenter ? -1 : ((FieldCoords.Edge) field.Key).Item2;
                DrawField(canvas, en, fn, field.Value);
            }
        }

        private void DrawField(Graphics canvas, int en, int fn, FieldState state)
        {
            var center = GetFieldCoords(en, fn);
            var rect1 = new Rectangle(
                center.X - _FieldSize/2, 
                center.Y - _FieldSize / 2,
                _FieldSize,
                _FieldSize);
            canvas.FillEllipse(new SolidBrush(Color.Blue), rect1);

            var color = state.IsEmpty
                ? Color.White
                : FieldState.NewColor(Player.Black).Equals(state)
                    ? Color.FromArgb(100,100,100)
                    : Color.Red;

            var rect2 = new Rectangle(
                center.X - _FieldSize / 2 + 2,
                center.Y - _FieldSize / 2 + 2,
                _FieldSize - 4,
                _FieldSize - 4);
            canvas.FillEllipse(new SolidBrush(color), rect2);
        }

        private Point GetFieldCoords(int en, int fn)
        {
            Point center = new Point(_CanvasSize / 2, _CanvasSize / 2);
            if (en == -1 && fn == -1) return center;
            Tuple<double, double> scalar;
            switch (fn)
            {
                case 0:
                    scalar = Tuple.Create(-0.5, -h(1));
                    break;
                case 1:
                    scalar = Tuple.Create(-1.0, 0.0);
                    break;
                case 2:
                    scalar = Tuple.Create(-0.5, h(1));
                    break;
                case 3:
                    scalar = Tuple.Create(0.5, h(1));
                    break;
                case 4:
                    scalar = Tuple.Create(1.0, 0.0);
                    break;
                case 5:
                    scalar = Tuple.Create(0.5, -h(1));
                    break;
                default:
                    throw new Exception();
            }
            return new Point(
                (int)(center.X + (scalar.Item1 * (en + 1) * _LineSize)),
                (int)(center.Y + (scalar.Item2 * (en + 1) * _LineSize)));
        }

        private static double h(double a)
        {
            return (a*Math.Sqrt(3))/2.0;
        }
    }
}
