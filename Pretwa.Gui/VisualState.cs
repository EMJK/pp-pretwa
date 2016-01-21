using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace Pretwa.Gui
{
    class VisualState
    {
        private FSharpMap<FieldCoords, FieldState> _State;
        private FSharpList<Tuple<FieldCoords, FSharpOption<FieldCoords>, FieldCoords>> _ValidMoves;
        private int _CanvasSize = 700;
        private int _LineSize = 100;
        private int _FieldSize = 60;
        private bool _DrawLines = true;
        private Form f;
        private PictureBox p;

        public event Action EnterPressed;

        public void Show()
        {
            ThreadPool.QueueUserWorkItem((_) =>
            {
                Application.Run(f);
            });
        }

        public VisualState()
        {
            f = new Form();
            f.Size = new Size(_CanvasSize, _CanvasSize);
            p = new PictureBox();
            p.Size = new Size(_CanvasSize, _CanvasSize);
            f.Controls.Add(p);
            p.Dock = DockStyle.Fill;

            f.KeyDown += (sender, args) =>
            {
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    if ((args.KeyCode & Keys.Return) != Keys.None)
                    {
                        var handler = EnterPressed;
                        if (handler != null) handler();
                    }
                });
            };
        }

        public void Draw(FSharpMap<FieldCoords, FieldState> state, FSharpList<Tuple<FieldCoords, FSharpOption<FieldCoords>, FieldCoords>> validMoves)
        {
            f.Invoke(new MethodInvoker(() =>
            {
                _State = state;
                _ValidMoves = validMoves;

                Bitmap bmp = new Bitmap(_CanvasSize, _CanvasSize);
                Graphics canvas = Graphics.FromImage(bmp);

                DrawGrid(canvas);
                DrawFields(canvas);
                if(_DrawLines) DrawPossibleMoves(canvas);
                DrawCoords(canvas);

                canvas.Flush();
                canvas.Dispose();

                p.Image = bmp;
            }));
        }

        private void DrawCoords(Graphics canvas)
        {
            var center = GetFieldCoords(-1, -1);
            var font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold);
            var brush = new SolidBrush(Color.Black);

            canvas.DrawString("C", font, brush, center);
            for(int en = 0; en < 3; en++)
                for (int fn = 0; fn < 6; fn++)
                {
                    canvas.DrawString(String.Format("{0}{1}", en, fn), font, brush, GetFieldCoords(en, fn));
                }
        }

        private void DrawGrid(Graphics canvas)
        {
            var pen = new Pen(Color.Black, 3);
            var center = GetFieldCoords(-1, -1);

            for (int fn = 0; fn < 6; fn++)
            {
                var to = GetFieldCoords(2, fn);
                canvas.DrawLine(pen, center, to);
            }
            
            for (int en = 0; en < 3; en++)
            {
                var radius = _LineSize*(en + 1);
                var location = Point.Subtract(center, new Size(radius, radius));
                var size = new Size(radius*2, radius*2);
                var bounds = new Rectangle(location, size);
                canvas.DrawEllipse(pen, bounds);
            }
        }

        private void DrawPossibleMoves(Graphics canvas)
        {
            var jumps = new List<Tuple<FieldCoords, FSharpOption<FieldCoords>, FieldCoords>>();
            foreach (var move in _ValidMoves)
            {
                if (move.Item2 == null)
                {
                    DrawLine(canvas, move.Item1, move.Item3, null);
                }
                else
                {
                    jumps.Add(move);
                }
            }
            foreach (var move in jumps)
            {
                DrawLine(canvas, move.Item1, move.Item3, move.Item2.Value);
            }
        }

        private void DrawLine(Graphics canvas, FieldCoords from, FieldCoords to, FieldCoords over)
        {
            Point pFrom, pTo;
            Point pOver = Point.Empty;
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
            if (over != null)
            {
                int en = over.IsCenter ? -1 : ((FieldCoords.Edge)over).Item1;
                int fn = over.IsCenter ? -1 : ((FieldCoords.Edge)over).Item2;
                pOver = GetFieldCoords(en, fn);
            }

                Pen pen = over != null 
                ? new Pen(Color.DarkOrange, 3)
                : new Pen(Color.LimeGreen, 6);

            if (over != null)
            {
                canvas.DrawLine(pen, pFrom, pOver);
                canvas.DrawLine(pen, pOver, pTo);
            }
            else
            {
                canvas.DrawLine(pen, pFrom, pTo);
            }
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
                    scalar = Tuple.Create(-0.5, h(1));
                    break;
                case 1:
                    scalar = Tuple.Create(-1.0, 0.0);
                    break;
                case 2:
                    scalar = Tuple.Create(-0.5, -h(1));
                    break;
                case 3:
                    scalar = Tuple.Create(0.5, -h(1));
                    break;
                case 4:
                    scalar = Tuple.Create(1.0, 0.0);
                    break;
                case 5:
                    scalar = Tuple.Create(0.5, h(1));
                    break;
                default:
                    throw new Exception();
            }
            return new Point(
                (int)(center.X + (scalar.Item1 * (en + 1) * _LineSize)),
                (int)(center.Y + (scalar.Item2 * (en + 1) * _LineSize)));
        }

        private static double h(double a) => (a*Math.Sqrt(3))/2.0;
    }
}
