using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZhadniyatProgramist.Controls
{
    /// <summary>
    /// Кратка анимация при успешно плащане:
    ///  - златни монети излитат нагоре и падат с лека гравитация;
    ///  - в центъра се появява голям надпис „ПЛАТЕНО“;
    ///  - всичко избледнява плавно и контролът се скрива сам след ~2 сек.
    ///
    /// Използва се от MainForm: контролът стои скрит и при Play(rect)
    /// се позиционира върху панела с дълга, пуска анимацията и изчезва.
    /// </summary>
    public class CoinBurstControl : Control
    {
        private sealed class Coin
        {
            public float X, Y;    // позиция
            public float Vx, Vy;  // скорост
            public float Radius;
            public float Spin;
        }

        private const int DurationMs = 1900;

        private readonly Timer _timer;
        private readonly List<Coin> _coins = new List<Coin>();
        private readonly Random _random = new Random();
        private DateTime _startedAt;

        public CoinBurstControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint
                   | ControlStyles.ResizeRedraw, true);

            BackColor = Theme.Background;
            Visible = false;
            Enabled = false; // не пречи на кликове
            TabStop = false;

            _timer = new Timer { Interval = 30 };
            _timer.Tick += Timer_Tick;
        }

        /// <summary>Пуска анимацията върху зададен правоъгълник (в координати на родителя).</summary>
        public void Play(Rectangle boundsInParent)
        {
            Bounds = boundsInParent;
            StartAnimation();
        }

        private void StartAnimation()
        {
            _coins.Clear();

            int count = 14;
            float originX = Width / 2f;
            float originY = Height * 0.72f;

            for (int i = 0; i < count; i++)
            {
                _coins.Add(new Coin
                {
                    X = originX + (float)(_random.NextDouble() * 60 - 30),
                    Y = originY,
                    Vx = (float)(_random.NextDouble() * 7 - 3.5),
                    Vy = -(4.5f + (float)(_random.NextDouble() * 5.0)), // излитат нагоре
                    Radius = 6f + (float)(_random.NextDouble() * 5),
                    Spin = (float)(_random.NextDouble() * Math.PI)
                });
            }

            _startedAt = DateTime.Now;
            Visible = true;
            BringToFront();
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            double elapsed = (DateTime.Now - _startedAt).TotalMilliseconds;

            foreach (Coin coin in _coins)
            {
                coin.X += coin.Vx;
                coin.Y += coin.Vy;
                coin.Vy += 0.28f; // лека гравитация
                coin.Spin += 0.2f;
            }

            if (elapsed >= DurationMs)
            {
                _timer.Stop();
                Visible = false;
            }
            else
            {
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            double elapsed = (DateTime.Now - _startedAt).TotalMilliseconds;
            double t = Math.Min(1.0, elapsed / DurationMs);

            // Плавно избледняване в последната третина
            float fade = t < 0.66 ? 1f : (float)(1.0 - (t - 0.66) / 0.34);
            fade = Math.Max(0f, Math.Min(1f, fade));

            // Рамка, за да прилича на панела, който временно покрива
            var bounds = new Rectangle(1, 1, Width - 3, Height - 3);
            using (GraphicsPath path = Theme.RoundedRect(bounds, 16))
            using (var pen = new Pen(Theme.WithAlpha(Theme.NeonGreen, (int)(160 * fade)), 1.6f))
            {
                g.DrawPath(pen, path);
            }

            // Монетите
            foreach (Coin coin in _coins)
            {
                int alpha = (int)(230 * fade);
                if (alpha <= 0) continue;

                float squeeze = 0.55f + 0.45f * (float)Math.Abs(Math.Cos(coin.Spin)); // "въртене"
                var rect = new RectangleF(
                    coin.X - coin.Radius * squeeze,
                    coin.Y - coin.Radius,
                    coin.Radius * 2 * squeeze,
                    coin.Radius * 2);

                using (var body = new SolidBrush(Color.FromArgb(alpha, 255, 200, 60)))
                {
                    g.FillEllipse(body, rect);
                }
                using (var rim = new Pen(Color.FromArgb(alpha, 200, 140, 20), 1.6f))
                {
                    g.DrawEllipse(rim, rect);
                }
                using (var shine = new SolidBrush(Color.FromArgb(alpha / 2, 255, 255, 255)))
                {
                    g.FillEllipse(shine,
                        rect.X + rect.Width * 0.2f, rect.Y + rect.Height * 0.15f,
                        rect.Width * 0.3f, rect.Height * 0.3f);
                }
            }

            // Големият надпис „ПЛАТЕНО“ – появява се бързо, изчезва плавно
            float textIn = (float)Math.Min(1.0, elapsed / 250.0);
            int textAlpha = (int)(255 * textIn * fade);
            if (textAlpha > 0)
            {
                using (var font = new Font("Segoe UI", 24f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "ПЛАТЕНО", font, ClientRectangle,
                        Color.FromArgb(textAlpha, Theme.NeonGreen),
                        TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.SingleLine);
                }
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