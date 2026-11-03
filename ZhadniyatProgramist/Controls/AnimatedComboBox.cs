using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZhadniyatProgramist.Controls
{
    public class AnimatedComboBox : ComboBox
    {
        private const int WM_PAINT = 0x000F;
        private const int ArrowZoneWidth = 36;
        private const int TextPadding = 12;

        private Timer _hoverTimer;
        private float _hoverProgress;
        private bool _hovered;
        private bool _disposed;

        public AnimatedComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            FlatStyle = FlatStyle.Flat;
            ItemHeight = 34;
            Font = new Font("Segoe UI", 11f);
            BackColor = Theme.SurfaceLight;
            ForeColor = Theme.TextPrimary;
            DoubleBuffered = true;

            InitializeTimer();
        }

        private void InitializeTimer()
        {
            if (_hoverTimer != null) return;

            _hoverTimer = new Timer();
            _hoverTimer.Interval = 20;
            _hoverTimer.Tick += HoverTimer_Tick;
        }

        private void HoverTimer_Tick(object sender, EventArgs e)
        {
            if (_disposed || !IsHandleCreated) return;

            float target = (_hovered || Focused || DroppedDown) ? 1f : 0f;

            if (Math.Abs(_hoverProgress - target) > 0.01f)
            {
                _hoverProgress += (target - _hoverProgress) * 0.25f;
                Invalidate();
            }
        }
protected override void OnHandleCreated(EventArgs e)
{
    base.OnHandleCreated(e);

    if (DesignMode || _disposed)
        return;

    InitializeTimer();

    if (_hoverTimer != null && !_hoverTimer.Enabled)
        _hoverTimer.Start();


            InitializeTimer();

            if (_hoverTimer != null && !_hoverTimer.Enabled)
                _hoverTimer.Start();
        }

        private bool NewMethod()
        {
            if (DesignMode || _disposed)
            {
                return false;
            }

            return true;
        }


        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_hoverTimer != null)
                _hoverTimer.Stop();

            base.OnHandleDestroyed(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hovered = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var background = new SolidBrush(selected ? Color.FromArgb(0, 96, 57) : Theme.SurfaceLight))
            {
                e.Graphics.FillRectangle(background, e.Bounds);
            }

            var textRect = new Rectangle(
                e.Bounds.X + TextPadding,
                e.Bounds.Y,
                Math.Max(1, e.Bounds.Width - TextPadding * 2),
                e.Bounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                GetItemText(Items[e.Index]),
                Font,
                textRect,
                selected ? Color.White : Theme.TextPrimary,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_PAINT && !DesignMode && IsHandleCreated && !_disposed)
            {
                try
                {
                    using (Graphics g = Graphics.FromHwnd(Handle))
                    {
                        PaintClosedBox(g);
                    }
                }
                catch
                {
                    // Не позволяваме custom рисуването да крашва приложението.
                }
            }
        }

        private void PaintClosedBox(Graphics g)
        {
            if (g == null || Width <= 0 || Height <= 0) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            using (GraphicsPath path = Theme.RoundedRect(bounds, 10))
            using (var fill = new SolidBrush(Theme.SurfaceLight))
            {
                g.FillPath(fill, path);

                Color border = Theme.Lerp(Theme.Border, Theme.NeonGreen, _hoverProgress);

                using (var pen = new Pen(border, 1.4f))
                {
                    g.DrawPath(pen, path);
                }
            }

            int textWidth = Math.Max(1, Width - ArrowZoneWidth - TextPadding * 2);

            var textRect = new Rectangle(TextPadding, 0, textWidth, Height);

            string text = SelectedItem != null ? GetItemText(SelectedItem) : Text;

            TextRenderer.DrawText(
                g,
                text ?? string.Empty,
                Font,
                textRect,
                Theme.TextPrimary,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis);

            Color arrowColor = Theme.Lerp(Theme.TextMuted, Theme.NeonGreen, _hoverProgress);

            int cx = Width - ArrowZoneWidth / 2 - 2;
            int cy = Height / 2;

            using (var arrowPen = new Pen(arrowColor, 2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            })
            {
                if (DroppedDown)
                {
                    g.DrawLine(arrowPen, cx - 5, cy + 2, cx, cy - 3);
                    g.DrawLine(arrowPen, cx, cy - 3, cx + 5, cy + 2);
                }
                else
                {
                    g.DrawLine(arrowPen, cx - 5, cy - 2, cx, cy + 3);
                    g.DrawLine(arrowPen, cx, cy + 3, cx + 5, cy - 2);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            _disposed = true;

            if (disposing && _hoverTimer != null)
            {
                _hoverTimer.Stop();
                _hoverTimer.Tick -= HoverTimer_Tick;
                _hoverTimer.Dispose();
                _hoverTimer = null;
            }

            base.Dispose(disposing);
        }
    }
}