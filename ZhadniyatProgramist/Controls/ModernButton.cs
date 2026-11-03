using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ZhadniyatProgramist.Services;

namespace ZhadniyatProgramist.Controls
{
    /// <summary>
    /// Модерен плосък бутон:
    ///  - при hover се "повдига" с 3 px, цветът прелива плавно и се появява мека сянка;
    ///  - повдигането е чисто визуално (в OnPaint) – layout-ът не се мести;
    ///  - при клик леко се "натиска" надолу;
    ///  - Pulse = true добавя фино неоново пулсиране (за най-важния бутон).
    /// </summary>
    public class ModernButton : Button
    {
        private readonly Timer _animTimer;
        private float _hoverProgress; // 0..1 – плавен преход при hover
        private bool _hovered;
        private bool _pressed;
        private double _pulsePhase;

        [Category("Appearance")]
        public Color HoverColor { get; set; } = Theme.NeonGreen;

        [Category("Appearance")]
        public int BorderRadius { get; set; } = 12;

        /// <summary>Меко неоново пулсиране около бутона (напр. „ПЛАТИ СМЕТКАТА“).</summary>
        [Category("Behavior")]
        public bool Pulse { get; set; }

        public ModernButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint
                   | ControlStyles.ResizeRedraw, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Theme.NeonGreen;
            ForeColor = Color.White;
            Font = new Font("Segoe UI Semibold", 11.5f);
            Cursor = Cursors.Hand;
            TabStop = true;

            _animTimer = new Timer { Interval = 20 };
            _animTimer.Tick += AnimTimer_Tick;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode)
            {
                _animTimer.Start();
            }
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            bool repaint = false;

            float target = _hovered ? 1f : 0f;
            if (Math.Abs(_hoverProgress - target) > 0.01f)
            {
                _hoverProgress += (target - _hoverProgress) * 0.25f;
                repaint = true;
            }

            if (Pulse)
            {
                _pulsePhase += 0.14;
                repaint = true;
            }

            if (repaint)
            {
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true; }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = false; _pressed = false; }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); _pressed = true; Invalidate(); }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _pressed = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Фон зад бутона (родителят), за да са чисти заоблените ъгли
            Color behind = Parent?.BackColor ?? Theme.Surface;
            using (var behindBrush = new SolidBrush(behind))
            {
                g.FillRectangle(behindBrush, ClientRectangle);
            }

            // Повдигане: тялото се рисува по-нагоре, сянката остава долу
            int lift = (int)Math.Round(3f * _hoverProgress) - (_pressed ? 2 : 0);
            lift = Math.Max(0, lift);

            var body = new Rectangle(2, 3 - lift, Width - 5, Height - 7);
            if (body.Width <= 0 || body.Height <= 0) return;

            // 1) Мека сянка под бутона (по-плътна при hover)
            int shadowAlpha = 45 + (int)(55 * _hoverProgress);
            var shadowRect = new Rectangle(body.X + 1, body.Y + 3 + lift, body.Width - 2, body.Height);
            using (GraphicsPath shadowPath = Theme.RoundedRect(shadowRect, BorderRadius))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(shadowAlpha, 0, 0, 0)))
            {
                g.FillPath(shadowBrush, shadowPath);
            }

            using (GraphicsPath path = Theme.RoundedRect(body, BorderRadius))
            {
                // 2) Неоново пулсиране (само ако е включено)
                if (Pulse && Enabled)
                {
                    int glow = (Theme.IsNight ? 60 : 40)
                             + (int)(50 * AnimationService.Pulse(_pulsePhase));
                    using (var glowPen = new Pen(Theme.WithAlpha(HoverColor, glow), 5f))
                    {
                        g.DrawPath(glowPen, path);
                    }
                }

                // 3) Тяло с плавно преливащ цвят и лек градиент
                Color baseColor = Enabled ? BackColor : Theme.SurfaceLight;
                Color hoverColor = Enabled ? HoverColor : Theme.SurfaceLight;
                Color current = Theme.Lerp(baseColor, hoverColor, _hoverProgress);

                using (var fill = new LinearGradientBrush(
                           body, Theme.Lighten(current, 14), current,
                           LinearGradientMode.Vertical))
                {
                    g.FillPath(fill, path);
                }

                // 4) Фина светла линия отгоре – "скъп" завършек
                using (var edgePen = new Pen(Theme.WithAlpha(Color.White, 35), 1f))
                {
                    g.DrawPath(edgePen, path);
                }
            }

            // 5) Текстът – винаги центриран и никога изрязан
            Color textColor = Enabled ? ForeColor : Theme.TextMuted;
            TextRenderer.DrawText(g, Text, Font, body, textColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine |
                TextFormatFlags.NoPadding);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}