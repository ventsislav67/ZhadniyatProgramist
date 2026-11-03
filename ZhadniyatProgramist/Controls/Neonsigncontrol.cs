using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ZhadniyatProgramist.Services;

namespace ZhadniyatProgramist.Controls
{
    /// <summary>
    /// Малка декоративна неонова табела „ОТВОРЕНО“:
    ///  - премигва леко (плавен синусоидален пулс + рядко късо трепване);
    ///  - вечер (Theme.IsNight) свети значително по-силно;
    ///  - не заема много място – слага се в заглавната зона.
    /// </summary>
    public class NeonSignControl : Control
    {
        private readonly Timer _timer;
        private readonly Random _random = new Random();
        private double _phase;
        private float _flickerDip = 1f;

        public NeonSignControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint
                   | ControlStyles.ResizeRedraw, true);

            BackColor = Theme.Surface;
            Size = new Size(118, 34);
            Enabled = false;
            TabStop = false;

            _timer = new Timer { Interval = 70 };
            _timer.Tick += (s, e) =>
            {
                _phase += 0.18;

                if (_random.NextDouble() < 0.015)
                {
                    _flickerDip = 0.4f; // късо реалистично трепване на неона
                }
                _flickerDip += (1f - _flickerDip) * 0.25f;

                Invalidate();
            };
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode)
            {
                _timer.Start();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var behind = new SolidBrush(Parent?.BackColor ?? Theme.Surface))
            {
                g.FillRectangle(behind, ClientRectangle);
            }

            var bounds = new Rectangle(1, 1, Width - 3, Height - 3);
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            // Вечер табелата свети по-силно
            double pulse = AnimationService.Pulse(_phase);
            float baseGlow = Theme.IsNight ? 1f : 0.55f;
            float glow = (float)((0.65 + 0.35 * pulse) * baseGlow * _flickerDip);

            using (GraphicsPath path = Theme.RoundedRect(bounds, bounds.Height / 2))
            {
                using (var fill = new SolidBrush(Color.FromArgb(26, 20, 8)))
                {
                    g.FillPath(fill, path);
                }

                // Ореол около рамката
                using (var glowPen = new Pen(Theme.WithAlpha(Theme.Amber, (int)(110 * glow)), 4f))
                {
                    g.DrawPath(glowPen, path);
                }
                using (var borderPen = new Pen(Theme.WithAlpha(Theme.Amber, (int)(120 + 130 * glow)), 1.6f))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Самият надпис – с леко "нажежаване"
            Color textColor = Theme.Lerp(
                Color.FromArgb(190, 130, 30),
                Color.FromArgb(255, 224, 130),
                glow);

            using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, "ОТВОРЕНО", font, bounds, textColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}