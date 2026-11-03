using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZhadniyatProgramist.Controls
{
    /// <summary>
    /// Плаващи бирени мехурчета.
    ///
    /// Контролът има два начина на употреба:
    ///  1) Самостоятелен контрол – поставя се върху панел / в странична зона
    ///     (Enabled = false, така че кликовете минават към родителя);
    ///  2) Вграден "двигател" (класът BubbleField) – AnimatedBackgroundPanel
    ///     го ползва, за да рисува мехурчета директно във фона, без
    ///     да е нужен прозрачен контрол отгоре (по-бързо и без трептене).
    /// </summary>
    public class BubbleEffectControl : Control
    {
        private readonly Timer _timer;
        private readonly BubbleField _field = new BubbleField();

        public BubbleEffectControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint
                   | ControlStyles.ResizeRedraw
                   | ControlStyles.Selectable, true);
            SetStyle(ControlStyles.Selectable, false);

            BackColor = Theme.Surface;
            Enabled = false; // мехурчетата не пречат на кликове и текстове
            TabStop = false;

            _timer = new Timer { Interval = 60 };
            _timer.Tick += (s, e) =>
            {
                _field.Step(ClientSize);
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
            base.OnPaint(e);
            _field.Draw(e.Graphics, ClientRectangle);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }

        // =====================================================================
        //  Двигателят на мехурчетата – използваем и от други контроли
        // =====================================================================

        /// <summary>Логика и рисуване на мехурчетата, независими от контрол.</summary>
        public class BubbleField
        {
            private sealed class Bubble
            {
                public float X;
                public float Y;
                public float Radius;
                public float Speed;
                public float WobblePhase;
                public float Life;     // 0..1 – за плавно появяване/изчезване
                public float LifeStep;
            }

            private readonly List<Bubble> _bubbles = new List<Bubble>();
            private readonly Random _random = new Random();
            private const int MaxBubbles = 16; // леки и ненатрапчиви

            /// <summary>Една стъпка от анимацията (вика се от Timer).</summary>
            public void Step(Size area)
            {
                if (area.Width <= 0 || area.Height <= 0) return;

                // Случайно раждане на ново мехурче
                if (_bubbles.Count < MaxBubbles && _random.NextDouble() < 0.25)
                {
                    _bubbles.Add(new Bubble
                    {
                        X = (float)(_random.NextDouble() * area.Width),
                        Y = area.Height + 10,
                        Radius = 2.5f + (float)(_random.NextDouble() * 4.5),
                        Speed = 0.7f + (float)(_random.NextDouble() * 1.3),
                        WobblePhase = (float)(_random.NextDouble() * Math.PI * 2),
                        Life = 0f,
                        LifeStep = 0.03f + (float)(_random.NextDouble() * 0.03)
                    });
                }

                for (int i = _bubbles.Count - 1; i >= 0; i--)
                {
                    Bubble b = _bubbles[i];
                    b.Y -= b.Speed;
                    b.WobblePhase += 0.09f;
                    b.X += (float)Math.Sin(b.WobblePhase) * 0.5f;

                    // Плавно появяване долу и плавно изчезване горе
                    float fadeOutZone = area.Height * 0.22f;
                    if (b.Y < fadeOutZone)
                    {
                        b.Life -= b.LifeStep;
                    }
                    else if (b.Life < 1f)
                    {
                        b.Life = Math.Min(1f, b.Life + b.LifeStep * 2f);
                    }

                    if (b.Life <= 0f || b.Y < -12)
                    {
                        _bubbles.RemoveAt(i);
                    }
                }
            }

            /// <summary>Рисува мехурчетата върху подадената повърхност.</summary>
            public void Draw(Graphics g, Rectangle bounds)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                foreach (Bubble b in _bubbles)
                {
                    int alpha = (int)(52 * b.Life);
                    if (alpha <= 0) continue;

                    var rect = new RectangleF(
                        bounds.X + b.X - b.Radius,
                        bounds.Y + b.Y - b.Radius,
                        b.Radius * 2, b.Radius * 2);

                    // Тяло на мехурчето – кехлибарено, като в халба
                    using (var body = new SolidBrush(Theme.WithAlpha(Theme.Amber, alpha)))
                    {
                        g.FillEllipse(body, rect);
                    }

                    // Малко светло отблясъче горе вляво
                    using (var shine = new SolidBrush(Theme.WithAlpha(Color.White, alpha)))
                    {
                        g.FillEllipse(shine,
                            rect.X + rect.Width * 0.22f,
                            rect.Y + rect.Height * 0.18f,
                            rect.Width * 0.28f,
                            rect.Height * 0.28f);
                    }
                }
            }
        }
    }
}