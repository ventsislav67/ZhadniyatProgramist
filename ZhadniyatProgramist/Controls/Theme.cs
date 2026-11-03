using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ZhadniyatProgramist.Controls
{
    /// <summary>
    /// Централна цветова палитра и помощни методи за рисуване.
    /// Всички контроли и форми ползват ЕДИНСТВЕНО тези цветове,
    /// за да изглежда приложението еднородно.
    /// </summary>
    public static class Theme
    {
        // --- Основни цветове (тъмна "кръчмарска" тема) ---
        public static readonly Color Background   = Color.FromArgb(19, 23, 29);   // фон на прозореца
        public static readonly Color Surface      = Color.FromArgb(30, 36, 44);   // панели/карти
        public static readonly Color SurfaceLight = Color.FromArgb(39, 46, 56);   // полета за въвеждане
        public static readonly Color Border       = Color.FromArgb(58, 67, 79);   // неутрални рамки

        // --- Текст ---
        public static readonly Color TextPrimary  = Color.FromArgb(236, 240, 245);
        public static readonly Color TextMuted    = Color.FromArgb(154, 164, 177);

        // --- Акценти ---
        public static readonly Color NeonGreen    = Color.FromArgb(0, 230, 118);
        public static readonly Color Amber        = Color.FromArgb(255, 179, 0);
        public static readonly Color Red          = Color.FromArgb(239, 83, 80);
        public static readonly Color RedBright    = Color.FromArgb(255, 61, 71);

        /// <summary>
        /// true след 18:00 или преди 07:00 (нощна смяна).
        /// Стойността се поддържа от MainForm чрез Timer и се чете
        /// от декоративните контроли (фон, неонова табела, рамки).
        /// </summary>
        public static bool IsNight { get; set; }

        /// <summary>Създава GraphicsPath за правоъгълник със заоблени ъгли.</summary>
        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        /// <summary>Плавно преливане между два цвята (t = 0..1).</summary>
        public static Color Lerp(Color from, Color to, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(from.A + (to.A - from.A) * t),
                (int)(from.R + (to.R - from.R) * t),
                (int)(from.G + (to.G - from.G) * t),
                (int)(from.B + (to.B - from.B) * t));
        }

        /// <summary>Същият цвят с друга прозрачност (alpha 0..255).</summary>
        public static Color WithAlpha(Color color, int alpha)
        {
            alpha = Math.Max(0, Math.Min(255, alpha));
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        /// <summary>Леко изсветлен вариант на цвят (за градиенти и hover).</summary>
        public static Color Lighten(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, color.R + amount),
                Math.Min(255, color.G + amount),
                Math.Min(255, color.B + amount));
        }
    }
}