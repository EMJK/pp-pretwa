using Julas.Utils;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Pretwa.Gui.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Pretwa.Gui
{
    internal class VisualState : Control
    {
        public FSharpMap<FieldCoords, FieldState> State { get; private set; }
        private FSharpList<Tuple<FieldCoords, FSharpOption<FieldCoords>, FieldCoords>> _ValidMoves;
        private int _CanvasSize = 700;
        private int _LineSize = 100;
        private int _FieldSize = 60;
        private readonly PictureBox _MainPictureBox;
        private readonly Tuple<FieldCoords, Point>[] _FieldMap;

        public Player CurrentPlayer { get; set; }
        private FieldCoords _SelectedField;
        private FieldCoords _HighlightedField;

        public event EventHandler<Tuple<FieldCoords, FieldCoords>> MoveRequested;

        public VisualState()
        {
            _FieldMap = new Tuple<FieldCoords, Point>[19];
            _FieldMap[0] = Tuple.Create(FieldCoords.Center, GetFieldCoords(-1, -1));
            int i = 1;
            for (int en = 0; en < 3; en++)
                for (int fn = 0; fn < 6; fn++)
                {
                    _FieldMap[i] = Tuple.Create(FieldCoords.NewEdge(en, fn), GetFieldCoords(en, fn));
                    i++;
                }

            _MainPictureBox = new PictureBox();
            _MainPictureBox.Size = new Size(_CanvasSize, _CanvasSize);
            this.Controls.Add(_MainPictureBox);
            _MainPictureBox.Dock = DockStyle.Fill;

            _MainPictureBox.MouseMove += HandleMouseMove;
            _MainPictureBox.MouseLeave += (s, e) =>
                HandleMouseMove(s, new MouseEventArgs(MouseButtons.None, 0, -1, -1, -1));
            _MainPictureBox.MouseClick += HandleMouseClick;
        }

        private void HandleMouseClick(object sender, MouseEventArgs mouseEventArgs)
        {
            if (_HighlightedField == null) return;
            var hit = HitTest(mouseEventArgs.Location);
            if (!hit.HasValue) return;

            var highlighted = _HighlightedField;
            FieldCoords selected = null;

            if (hit.Value.Item2.IsColor)
            {
                var clickedColor = ((FieldState.Color)hit.Value.Item2).Item;
                if (clickedColor.Equals(CurrentPlayer))
                {
                    selected = hit.Value.Item1;
                }
            }
            else
            {
                if (_SelectedField != null)
                {
                    if (CheckValidMove(_SelectedField, hit.Value.Item1))
                    {
                        selected = null;
                        highlighted = null;
                        MoveRequested.Raise(this, Tuple.Create(_SelectedField, hit.Value.Item1));
                    }
                    else selected = _SelectedField;
                }
            }

            using (var canvas = Graphics.FromImage(_MainPictureBox.Image))
            {
                ClearMarks(canvas);
                SetMarks(canvas, highlighted, selected);
            }
            _MainPictureBox.Invalidate();
        }

        private bool CheckValidMove(FieldCoords from, FieldCoords to)
        {
            return _ValidMoves.Any(m => m.Item1.Equals(from) && m.Item3.Equals(to));
        }

        private void ClearMarks(Graphics canvas)
        {
            if (_HighlightedField != null)
            {
                DrawField(canvas, _HighlightedField, State[_HighlightedField], FieldDrawType.Default);
            }
            if (_SelectedField != null)
            {
                DrawField(canvas, _SelectedField, State[_SelectedField], FieldDrawType.Default);
            }
            _SelectedField = null;
            _HighlightedField = null;
        }

        private void SetMarks(Graphics canvas, FieldCoords highlighted, FieldCoords selected)
        {
            _HighlightedField = highlighted;
            _SelectedField = selected;
            if (highlighted == null && selected == null) return;
            if (highlighted?.Equals(selected) == true)
            {
                DrawField(canvas, highlighted, State[highlighted], FieldDrawType.Highlighted | FieldDrawType.Selected);
                return;
            }
            if (highlighted != null)
            {
                DrawField(canvas, highlighted, State[highlighted], FieldDrawType.Highlighted);
            }
            if (selected != null)
            {
                DrawField(canvas, selected, State[selected], FieldDrawType.Selected);
            }
        }

        private bool CanBeHighlighted(FieldCoords field)
        {
            if (State[field].IsColor && ((FieldState.Color)State[field]).Item.Equals(CurrentPlayer))
                return _ValidMoves.Any(m => m.Item1.Equals(field));

            if (_SelectedField != null && State[field].IsEmpty)
                return _ValidMoves.Any(m => m.Item1.Equals(_SelectedField) && m.Item3.Equals(field));
            return false;
        }

        private void HandleMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var hit = HitTest(mouseEventArgs.Location);
            var hitField = hit.HasValue ? hit.Value.Item1 : null;

            // jesli nie ma zmian - konczymy
            if (_HighlightedField == null && hitField == null) return;
            if (_HighlightedField != null && hitField != null && _HighlightedField.Equals(hitField)) return;

            var highlighted = (hitField != null && CanBeHighlighted(hitField)) ? hitField : null;
            var selected = _SelectedField;

            using (var canvas = Graphics.FromImage(_MainPictureBox.Image))
            {
                ClearMarks(canvas);
                SetMarks(canvas, highlighted, selected);
            }
            _MainPictureBox.Invalidate();
        }

        private Option<Tuple<FieldCoords, FieldState>> HitTest(Point coords)
        {
            foreach (var field in _FieldMap)
            {
                double dx = Math.Abs(coords.X - field.Item2.X);
                double dy = Math.Abs(coords.Y - field.Item2.Y);
                double distance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
                if (distance <= _FieldSize / 2.0)
                {
                    var state = State[field.Item1];
                    return Tuple.Create(field.Item1, state);
                }
            }
            return Option<Tuple<FieldCoords, FieldState>>.None;
        }

        public void DrawOnlyGrid()
        {
            Bitmap bmp = new Bitmap(_CanvasSize, _CanvasSize);
            Graphics canvas = Graphics.FromImage(bmp);
            DrawGrid(canvas);
            canvas.Dispose();
            _MainPictureBox.Image = bmp;
        }

        public void Draw(FSharpMap<FieldCoords, FieldState> state, FSharpList<Tuple<FieldCoords, FSharpOption<FieldCoords>, FieldCoords>> validMoves)
        {
            State = state;
            _ValidMoves = validMoves;

            Bitmap bmp = new Bitmap(_CanvasSize, _CanvasSize);
            Graphics canvas = Graphics.FromImage(bmp);

            DrawGrid(canvas);
            DrawFields(canvas);
            //DrawPossibleMoves(canvas);
            //DrawCoords(canvas);

            canvas.Flush();
            canvas.Dispose();

            _MainPictureBox.Image = bmp;
        }

        private void DrawCoords(Graphics canvas)
        {
            var center = GetFieldCoords(-1, -1);
            var font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Bold);
            var brush = new SolidBrush(Color.Black);

            canvas.DrawString("C", font, brush, center);
            for (int en = 0; en < 3; en++)
                for (int fn = 0; fn < 6; fn++)
                {
                    canvas.DrawString($"{en}{fn}", font, brush, GetFieldCoords(en, fn));
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
                var radius = _LineSize * (en + 1);
                var location = Point.Subtract(center, new Size(radius, radius));
                var size = new Size(radius * 2, radius * 2);
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
                int en = from.IsCenter ? -1 : ((FieldCoords.Edge)from).Item1;
                int fn = from.IsCenter ? -1 : ((FieldCoords.Edge)from).Item2;
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
            foreach (var field in State)
            {
                int en = field.Key.IsCenter ? -1 : ((FieldCoords.Edge)field.Key).Item1;
                int fn = field.Key.IsCenter ? -1 : ((FieldCoords.Edge)field.Key).Item2;
                DrawField(canvas, en, fn, field.Value, FieldDrawType.Default);
            }
        }

        private void DrawField(Graphics canvas, int en, int fn, FieldState state, FieldDrawType drawType)
        {
            var center = GetFieldCoords(en, fn);
            var rect1 = new Rectangle(
                center.X - _FieldSize / 2,
                center.Y - _FieldSize / 2,
                _FieldSize,
                _FieldSize);
            canvas.FillEllipse(new SolidBrush(GetFieldBorderColor(drawType)), rect1);

            var color = GetFieldFillColor(state, drawType);

            var rect2 = new Rectangle(
                center.X - _FieldSize / 2 + 4,
                center.Y - _FieldSize / 2 + 4,
                _FieldSize - 8,
                _FieldSize - 8);
            canvas.FillEllipse(new SolidBrush(color), rect2);
        }

        private void DrawField(Graphics canvas, FieldCoords coords, FieldState state, FieldDrawType drawType)
        {
            int en = coords.IsCenter ? -1 : ((FieldCoords.Edge)coords).Item1;
            int fn = coords.IsCenter ? -1 : ((FieldCoords.Edge)coords).Item2;
            DrawField(canvas, en, fn, state, drawType);
        }

        private Color GetFieldFillColor(FieldState state, FieldDrawType drawType)
        {
            if (state.IsEmpty)
            {
                return drawType.HasFlag(FieldDrawType.Highlighted)
                    ? Color.Gold
                    : Color.White;
            }
            if (((FieldState.Color)state).Item.IsBlack)
            {
                return drawType.HasFlag(FieldDrawType.Highlighted)
                    ? Color.LightSlateGray
                    : Color.FromArgb(63, 63, 63);
            }
            if (((FieldState.Color)state).Item.IsRed)
            {
                return drawType.HasFlag(FieldDrawType.Highlighted)
                    ? Color.FromArgb(255, 63, 63)
                    : Color.FromArgb(192, 0, 0);
            }
            throw new Exception("Something went wrong");
        }

        private Color GetFieldBorderColor(FieldDrawType drawType)
        {
            return drawType.HasFlag(FieldDrawType.Selected)
                ? Color.Magenta
                : Color.Black;
        }

        private Point GetFieldCoords(FieldCoords coords)
        {
            if (coords.IsCenter) return GetFieldCoords(-1, -1);
            var edge = (FieldCoords.Edge)coords;
            return GetFieldCoords(edge.Item1, edge.Item2);
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

        private static double h(double a) => (a * Math.Sqrt(3)) / 2.0;
    }
}