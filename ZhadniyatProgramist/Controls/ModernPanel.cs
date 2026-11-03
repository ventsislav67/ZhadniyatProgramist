using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ZhadniyatProgramist.Services;

namespace ZhadniyatProgramist.Controls
{
    /// <summary>
    /// Панел със заоблени ъгли, цветна рамка и няколко "живи" ефекта:
    ///  - EnablePulse: меко неоново пулсиране на рамката (по-силно нощем);
    ///  - HoverGlow:   лек светъл ореол, когато мишката е върху панела;
    ///  - Shake():     кратко разклащане на съдържанието (чрез Padding,
    ///                 така че layout-ът НИКОГА не се размества трайно);
    ///  - FlashAlert(): рамката пулсира в червено за ~1.5 секунди.
    /// </summary>
    public class ModernPanel : Panel
    {
        private readonly Timer _fxTimer;          // общ таймер за всички ефекти
        private double _pulsePhase;               // фаза на неоновото пулсиране
        private float _hoverProgress;             // 0..1 – плавен hover ореол
        private bool _hovered;

        // --- Shake чрез Padding (без да местим самия контрол) ---
        private Padding _basePadding;
        private bool _basePaddingCaptured;
        private int _shakeTicksLeft;

        // --- Червена аларма при голям дълг ---
        private DateTime _alertUntil = DateTime.MinValue;

        [Category("Appearance")]
        public Color BorderColor { get; set; } = Theme.Border;

        [Category("Appearance")]
        public int BorderRadius { get; set; } = 16;

        /// <summary>Меко постоянно пулсиране на рамката (за важните панели).</summary>
        [Category("Behavior")]
        public bool EnablePulse { get; set; }

        /// <summary>Лек светъл ореол при посочване с мишката.</summary>
        [Category("Behavior")]
        public bool HoverGlow { get; set; } = true;

        public ModernPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint
                   | ControlStyles.ResizeRedraw, true);

            BackColor = Theme.Surface;

            _fxTimer = new Timer { Interval = 45 };
            _fxTimer.Tick += FxTimer_Tick;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode)
            {
                _fxTimer.Start();
            }
        }

        /// <summary>Кратко разклащане на съдържанието (напр. при дълг над 100 лв.).</summary>
        public void Shake()
        {
            if (!_basePaddingCaptured)
            {
                _basePadding = Padding;
                _basePaddingCaptured = true;
            }

            _shakeTicksLeft = 10; // ~0.45 сек при 45 ms на тик
        }

        /// <summary>Рамката пулсира тревожно в червено за кратко.</summary>
        public void FlashAlert()
        {
            _alertUntil = DateTime.Now.AddMilliseconds(1500);
        }

        private void FxTimer_Tick(object sender, EventArgs e)
        {
            bool needsRepaint = false;

            // 1) Неоново пулсиране / аларма
            if (EnablePulse || DateTime.Now < _alertUntil)
            {
                _pulsePhase += 0.16;
                needsRepaint = true;
            }

            // 2) Плавен hover ореол
            float hoverTarget = _hovered ? 1f : 0f;
            if (Math.Abs(_hoverProgress - hoverTarget) > 0.01f)
            {
                _hoverProgress += (hoverTarget - _hoverProgress) * 0.22f;
                needsRepaint = true;
            }

            // 3) Shake – разместваме Padding-а наляво/надясно и го връщаме точно
            if (_shakeTicksLeft > 0)
            {
                _shakeTicksLeft--;

                if (_shakeTicksLeft == 0)
                {
                    Padding = _basePadding; // гарантирано връщане на оригинала
                }
                else
                {
                    int offset = (_shakeTicksLeft % 2 == 0) ? 4 : -4;
                    Padding = new Padding(
                        Math.Max(0, _basePadding.Left + offset),
                        _basePadding.Top,
                        Math.Max(0, _basePadding.Right - offset),
                        _basePadding.Bottom);
                }

                needsRepaint = true;
            }

            if (needsRepaint)
            {
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hovered = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            // Мишката може да е върху дъщерен контрол – тогава още сме "вътре".
            _hovered = ClientRectangle.Contains(PointToClient(Cursor.Position));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Ъглите извън заобления панел се запълват с цвета на родителя
            Color behind = Parent?.BackColor ?? Theme.Background;
            using (var behindBrush = new SolidBrush(behind))
            {
                g.FillRectangle(behindBrush, ClientRectangle);
            }

            var bounds = new Rectangle(1, 1, Width - 3, Height - 3);
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            using (GraphicsPath path = Theme.RoundedRect(bounds, BorderRadius))
            {
                // Фон с едва доловим вертикален градиент – "повече светлина"
                Color top = Theme.Lighten(BackColor, _hovered && HoverGlow ? 10 : 5);
                using (var fill = new LinearGradientBrush(bounds, top, BackColor,
                           LinearGradientMode.Vertical))
                {
                    g.FillPath(fill, path);
                }

                // Рамка – цвят и сила според pulse / alert / hover
                bool alert = DateTime.Now < _alertUntil;
                Color borderColor = alert ? Theme.RedBright : BorderColor;

                int glowAlpha = 0;
                if (alert)
                {
                    glowAlpha = 90 + (int)(90 * AnimationService.Pulse(_pulsePhase * 2.2));
                }
                else if (EnablePulse)
                {
                    int baseGlow = Theme.IsNight ? 55 : 30; // нощем неонът свети по-силно
                    glowAlpha = baseGlow + (int)(45 * AnimationService.Pulse(_pulsePhase));
                }
                else if (_hoverProgress > 0.02f && HoverGlow)
                {
                    glowAlpha = (int)(45 * _hoverProgress);
                }

                // Мек външен ореол (две допълнителни полупрозрачни линии)
                if (glowAlpha > 0)
                {
                    using (var glowPen1 = new Pen(Theme.WithAlpha(borderColor, glowAlpha), 3.5f))
                    {
                        g.DrawPath(glowPen1, path);
                    }
                    using (var glowPen2 = new Pen(Theme.WithAlpha(borderColor, glowAlpha / 2), 6f))
                    {
                        g.DrawPath(glowPen2, path);
                    }
                }

                using (var borderPen = new Pen(borderColor, 1.4f))
                {
                    g.DrawPath(borderPen, path);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fxTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}