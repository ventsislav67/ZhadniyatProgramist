using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ZhadniyatProgramist.Controls
{
    /// <summary>
    /// Бай Пешо – анимираният стопанин на кръчмата.
    ///
    /// Състояния (според дълга на избрания програмист):
    ///  - Idle     : чака си клиенти;
    ///  - Happy    : сметката е чиста / току-що е платено (подскача леко);
    ///  - Wary     : дългът е малък, но го записва;
    ///  - Warning  : дългът расте – гледа подозрително;
    ///  - Angry    : над 50 лв. – гледа лошо и потреперва;
    ///  - Furious  : над 100 лв. – трепери силно.
    ///
    /// Постоянни "живи" ефекти: дишане (леко вертикално полюшване)
    /// и мигане на всеки 4–6 секунди. Всичко е с ЕДИН Timer + GDI+ рисуване;
    /// няма 15–20 PNG кадъра. Ако в Assets/Pesho има pesho_&lt;състояние&gt;.png,
    /// картинката се използва вместо вграденото рисуване – липсата на
    /// файлове НЕ предизвиква краш.
    /// </summary>
    public class PeshoCharacterControl : Control
    {
        private enum Mood { Idle, Happy, Wary, Warning, Angry, Furious }

        private readonly Timer _timer;
        private readonly Random _random = new Random();

        private Mood _mood = Mood.Idle;
        private string _bubbleText = "Ела да те запиша в тефтера…";
        private Color _bubbleTextColor = Theme.TextPrimary;

        // --- Дишане и мигане ---
        private double _breathPhase;
        private DateTime _nextBlinkAt;
        private DateTime _blinkEndsAt = DateTime.MinValue;

        // --- Подскачане при радост ---
        private DateTime _hopEndsAt = DateTime.MinValue;
        private double _hopPhase;

        // --- Ефект „почукване по бара“ (тук-тук) ---
        private DateTime _knockEndsAt = DateTime.MinValue;
        private int _knockJerk;

        // --- Временна реплика (връща се към репликата на настроението) ---
        private DateTime _tempBubbleUntil = DateTime.MinValue;
        private string _moodBubbleText;
        private Color _moodBubbleColor;

        public PeshoCharacterControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint
                   | ControlStyles.ResizeRedraw, true);

            BackColor = Theme.Surface;
            _moodBubbleText = _bubbleText;
            _moodBubbleColor = _bubbleTextColor;

            _timer = new Timer { Interval = 40 };
            _timer.Tick += Timer_Tick;

            ScheduleNextBlink();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode)
            {
                _timer.Start();
            }
        }

        // =====================================================================
        //  Публично API (използва се от MainForm)
        // =====================================================================

        /// <summary>Пешо чака спокойно (няма избран програмист).</summary>
        public void ShowIdle()
        {
            SetMood(Mood.Idle, "Ела да те запиша в тефтера…", Theme.TextMuted);
        }

        /// <summary>Настроението на Пешо според текущия дълг на избрания човек.</summary>
        public void UpdateMood(decimal debt)
        {
            if (debt <= 0)
            {
                SetMood(Mood.Happy, "Ей така те искам! Чиста сметка!", Theme.NeonGreen);
            }
            else if (debt < 20)
            {
                SetMood(Mood.Wary, "Записвам всичко, да знаеш…", Theme.Amber);
            }
            else if (debt <= 50)
            {
                SetMood(Mood.Warning, "Хм… сметката взе да расте!", Theme.Amber);
            }
            else if (debt <= 100)
            {
                SetMood(Mood.Angry, "„Айде стига бе! Плати си!“", Theme.RedBright);
            }
            else
            {
                SetMood(Mood.Furious, "„Айде стига бе! Плати си!“", Theme.RedBright);
            }
        }

        /// <summary>Радост при платена сметка – Пешо подскача.</summary>
        public void Celebrate()
        {
            SetMood(Mood.Happy, "Наздраве! Ей така те искам!", Theme.NeonGreen);
            _hopEndsAt = DateTime.Now.AddMilliseconds(1400);
            _hopPhase = 0;
        }

        /// <summary>
        /// Реакция при добавена нова поръчка.
        /// Над 50 лв. – строго „почукване по бара“ и реплика
        /// „Първо плащаш, после поръчваш!“.
        /// </summary>
        public void ReactToNewOrder(decimal newDebt)
        {
            UpdateMood(newDebt);

            if (newDebt > 50)
            {
                _knockEndsAt = DateTime.Now.AddMilliseconds(1600);
                _knockJerk = 6; // 3 бързи разклащания (по 2 тика всяко)
                ShowTemporaryBubble("Първо плащаш, после поръчваш!", Theme.RedBright, 2600);
            }
            else
            {
                ShowTemporaryBubble("Записах го в тефтера! 🍺", Theme.TextPrimary, 1800);
            }
        }

        // =====================================================================
        //  Вътрешна логика
        // =====================================================================

        private void SetMood(Mood mood, string bubbleText, Color bubbleColor)
        {
            _mood = mood;
            _moodBubbleText = bubbleText;
            _moodBubbleColor = bubbleColor;

            // Временната реплика (ако има) остава с приоритет до изтичането си.
            if (DateTime.Now >= _tempBubbleUntil)
            {
                _bubbleText = bubbleText;
                _bubbleTextColor = bubbleColor;
            }

            Invalidate();
        }

        private void ShowTemporaryBubble(string text, Color color, int milliseconds)
        {
            _bubbleText = text;
            _bubbleTextColor = color;
            _tempBubbleUntil = DateTime.Now.AddMilliseconds(milliseconds);
            Invalidate();
        }

        private void ScheduleNextBlink()
        {
            _nextBlinkAt = DateTime.Now.AddMilliseconds(4000 + _random.Next(2000)); // 4–6 сек
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            _breathPhase += 0.07; // дишане

            // Мигане
            if (now >= _nextBlinkAt)
            {
                _blinkEndsAt = now.AddMilliseconds(140);
                ScheduleNextBlink();
            }

            // Подскачане
            if (now < _hopEndsAt)
            {
                _hopPhase += 0.24;
            }

            // Почукване – намаляващи резки движения
            if (now < _knockEndsAt && _knockJerk > 0 && _random.NextDouble() < 0.5)
            {
                _knockJerk--;
            }

            // Изтичане на временната реплика -> връщаме репликата на настроението
            if (_tempBubbleUntil != DateTime.MinValue && now >= _tempBubbleUntil)
            {
                _tempBubbleUntil = DateTime.MinValue;
                _bubbleText = _moodBubbleText;
                _bubbleTextColor = _moodBubbleColor;
            }

            Invalidate();
        }

        // =====================================================================
        //  Рисуване
        // =====================================================================

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var back = new SolidBrush(BackColor))
            {
                g.FillRectangle(back, ClientRectangle);
            }

            if (Width < 60 || Height < 80) return;

            DateTime now = DateTime.Now;

            // --- Общи отмествания (живот) ---
            float breath = (float)Math.Sin(_breathPhase) * 2f;   // дишане
            float offsetX = 0, offsetY = breath;

            if (_mood == Mood.Angry)
            {
                offsetX += _random.Next(-1, 2);                   // леко треперене
            }
            else if (_mood == Mood.Furious)
            {
                offsetX += _random.Next(-2, 3);                   // силно треперене
                offsetY += _random.Next(-1, 2);
            }

            if (now < _hopEndsAt)
            {
                offsetY -= (float)Math.Abs(Math.Sin(_hopPhase)) * 9f; // подскачане
            }

            if (now < _knockEndsAt)
            {
                offsetX += (_knockJerk % 2 == 0) ? 4 : -4;        // „тук-тук“ разклащане
            }

            // --- Разпределение: балонче горе, Пешо под него, надпис най-долу ---
            var bubbleArea = MeasureBubble(g, out string wrappedText);
            int captionHeight = 24;

            var peshoArea = new Rectangle(
                0,
                bubbleArea.Bottom + 8,
                Width,
                Height - bubbleArea.Bottom - 8 - captionHeight);

            DrawBubble(g, bubbleArea, wrappedText);

            // Ако има картинка за състоянието – ползваме нея, иначе рисуваме
            Image sprite = PeshoSprites.Get(_mood.ToString());
            if (sprite != null)
            {
                DrawSprite(g, sprite, peshoArea, offsetX, offsetY);
            }
            else
            {
                DrawPesho(g, peshoArea, offsetX, offsetY, now);
            }

            // Иконка „тук-тук“ до Пешо
            if (now < _knockEndsAt)
            {
                using (var font = new Font("Segoe UI", 10.5f, FontStyle.Bold))
                {
                    var knockPoint = new Rectangle(
                        peshoArea.X + peshoArea.Width / 2 + (int)(peshoArea.Height * 0.28f),
                        peshoArea.Y + peshoArea.Height / 3,
                        110, 26);
                    TextRenderer.DrawText(g, "👊 тук-тук!", font, knockPoint, Theme.Amber,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.SingleLine | TextFormatFlags.NoClipping);
                }
            }

            // Надпис под Пешо
            using (var captionFont = new Font("Segoe UI", 9.5f, FontStyle.Italic))
            {
                var captionRect = new Rectangle(0, Height - captionHeight, Width, captionHeight);
                TextRenderer.DrawText(g, "Бай Пешо следи тефтера", captionFont, captionRect,
                    Theme.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine);
            }
        }

        /// <summary>Изчислява размера на балончето така, че текстът да се вижда ЦЕЛИЯТ.</summary>
        private Rectangle MeasureBubble(Graphics g, out string text)
        {
            text = _bubbleText ?? string.Empty;

            using (var font = new Font("Segoe UI Semibold", 10.5f))
            {
                int maxTextWidth = Math.Max(120, Width - 48);

                Size measured = TextRenderer.MeasureText(g, text, font,
                    new Size(maxTextWidth, int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter);

                int bubbleWidth = Math.Min(Width - 16, measured.Width + 30);
                int bubbleHeight = measured.Height + 18;

                int x = (Width - bubbleWidth) / 2;
                return new Rectangle(x, 4, bubbleWidth, bubbleHeight);
            }
        }

        private void DrawBubble(Graphics g, Rectangle bubble, string text)
        {
            using (GraphicsPath path = Theme.RoundedRect(bubble, 12))
            {
                using (var fill = new SolidBrush(Theme.SurfaceLight))
                {
                    g.FillPath(fill, path);
                }
                using (var pen = new Pen(Theme.Border, 1.2f))
                {
                    g.DrawPath(pen, path);
                }
            }

            // Опашчица към Пешо (той е точно отдолу, близо до балончето)
            var tail = new[]
            {
                new Point(bubble.X + bubble.Width / 2 - 7, bubble.Bottom - 1),
                new Point(bubble.X + bubble.Width / 2 + 7, bubble.Bottom - 1),
                new Point(bubble.X + bubble.Width / 2,     bubble.Bottom + 8)
            };
            using (var tailBrush = new SolidBrush(Theme.SurfaceLight))
            {
                g.FillPolygon(tailBrush, tail);
            }

            using (var font = new Font("Segoe UI Semibold", 10.5f))
            {
                var textRect = new Rectangle(
                    bubble.X + 6, bubble.Y + 4, bubble.Width - 12, bubble.Height - 8);
                TextRenderer.DrawText(g, text, font, textRect, _bubbleTextColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.WordBreak);
            }
        }

        private static void DrawSprite(Graphics g, Image sprite, Rectangle area,
            float offsetX, float offsetY)
        {
            if (area.Width <= 0 || area.Height <= 0) return;

            float scale = Math.Min(
                (float)area.Width / sprite.Width,
                (float)area.Height / sprite.Height);

            float w = sprite.Width * scale;
            float h = sprite.Height * scale;

            g.DrawImage(sprite,
                area.X + (area.Width - w) / 2 + offsetX,
                area.Y + (area.Height - h) / 2 + offsetY,
                w, h);
        }

        /// <summary>Вграденото GDI+ рисуване на Пешо (без външни файлове).</summary>
        private void DrawPesho(Graphics g, Rectangle area, float dx, float dy, DateTime now)
        {
            if (area.Width <= 0 || area.Height <= 0) return;

            // Пешо запълва почти целия панел (уголемен с ~35%)
            float size = Math.Min(area.Width, area.Height) * 0.96f;
            float cx = area.X + area.Width / 2f + dx;
            float top = area.Y + (area.Height - size) / 2f + dy;

            float headR = size * 0.30f;              // радиус на главата
            float headCx = cx;
            float headCy = top + headR + size * 0.10f;

            bool blinking = now < _blinkEndsAt;

            Color skin = Color.FromArgb(233, 150, 122);
            Color skinDark = Color.FromArgb(205, 120, 95);
            Color hat = Color.FromArgb(110, 82, 55);
            Color hatDark = Color.FromArgb(88, 64, 42);
            Color mustache = Color.FromArgb(120, 120, 125);
            Color vest = Color.FromArgb(96, 70, 46);
            Color shirt = Color.FromArgb(70, 78, 88);

            // --- Тяло (елек + риза) ---
            float bodyW = size * 0.72f;
            float bodyH = size * 0.42f;
            float bodyY = headCy + headR * 0.78f;

            using (GraphicsPath body = Theme.RoundedRect(
                       new Rectangle((int)(cx - bodyW / 2), (int)bodyY, (int)bodyW, (int)bodyH),
                       (int)(size * 0.09f)))
            {
                using (var shirtBrush = new SolidBrush(shirt))
                {
                    g.FillPath(shirtBrush, body);
                }
            }

            using (var vestBrush = new SolidBrush(vest))
            {
                g.FillRectangle(vestBrush, cx - bodyW / 2, bodyY, bodyW * 0.30f, bodyH);
                g.FillRectangle(vestBrush, cx + bodyW / 2 - bodyW * 0.30f, bodyY, bodyW * 0.30f, bodyH);
            }

            // Ръце (свити юмруци отпред)
            using (var fist = new SolidBrush(skin))
            {
                float fistR = size * 0.075f;
                g.FillEllipse(fist, cx - bodyW * 0.28f - fistR, bodyY + bodyH * 0.55f, fistR * 2, fistR * 2);
                g.FillEllipse(fist, cx + bodyW * 0.28f - fistR, bodyY + bodyH * 0.55f, fistR * 2, fistR * 2);
            }

            // --- Глава ---
            using (var skinBrush = new SolidBrush(skin))
            {
                g.FillEllipse(skinBrush, headCx - headR, headCy - headR, headR * 2, headR * 2);
            }

            // Уши
            using (var earBrush = new SolidBrush(skinDark))
            {
                float earR = headR * 0.22f;
                g.FillEllipse(earBrush, headCx - headR - earR * 0.7f, headCy - earR / 2, earR * 1.4f, earR * 1.6f);
                g.FillEllipse(earBrush, headCx + headR - earR * 0.7f, headCy - earR / 2, earR * 1.4f, earR * 1.6f);
            }

            // --- Каскет ---
            using (var hatBrush = new SolidBrush(hat))
            {
                g.FillPie(hatBrush,
                    headCx - headR * 1.05f, headCy - headR * 1.35f,
                    headR * 2.1f, headR * 1.5f, 180, 180);
            }
            using (var brimBrush = new SolidBrush(hatDark))
            {
                g.FillEllipse(brimBrush,
                    headCx - headR * 1.08f, headCy - headR * 0.72f,
                    headR * 2.16f, headR * 0.26f);
            }

            // Символ на каскета при много ядосан – "изпуска пара"
            if (_mood == Mood.Furious || _mood == Mood.Angry)
            {
                using (var angryFont = new Font("Segoe UI", size * 0.055f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, "💢", angryFont,
                        new Rectangle((int)(headCx + headR * 0.45f), (int)(headCy - headR * 1.25f), 40, 26),
                        Theme.RedBright,
                        TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoClipping);
                }
            }

            // --- Вежди (по настроение) ---
            float browY = headCy - headR * 0.28f;
            float browLen = headR * 0.42f;
            float browTilt;
            switch (_mood)
            {
                case Mood.Happy: browTilt = -headR * 0.06f; break;   // повдигнати
                case Mood.Wary: browTilt = 0f; break;
                case Mood.Warning: browTilt = headR * 0.10f; break;
                case Mood.Angry:
                case Mood.Furious: browTilt = headR * 0.20f; break;  // смръщени
                default: browTilt = headR * 0.03f; break;
            }

            using (var browPen = new Pen(Color.FromArgb(90, 90, 95), Math.Max(2f, size * 0.024f))
                       { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(browPen,
                    headCx - headR * 0.55f, browY - browTilt * 0.2f,
                    headCx - headR * 0.55f + browLen, browY + browTilt);
                g.DrawLine(browPen,
                    headCx + headR * 0.55f, browY - browTilt * 0.2f,
                    headCx + headR * 0.55f - browLen, browY + browTilt);
            }

            // --- Очи (с мигане) ---
            float eyeY = headCy - headR * 0.08f;
            float eyeDx = headR * 0.38f;
            float eyeR = headR * 0.10f;

            if (blinking)
            {
                using (var lidPen = new Pen(skinDark, Math.Max(2f, size * 0.02f)))
                {
                    g.DrawLine(lidPen, headCx - eyeDx - eyeR, eyeY, headCx - eyeDx + eyeR, eyeY);
                    g.DrawLine(lidPen, headCx + eyeDx - eyeR, eyeY, headCx + eyeDx + eyeR, eyeY);
                }
            }
            else
            {
                using (var eyeBrush = new SolidBrush(Color.FromArgb(35, 38, 44)))
                {
                    g.FillEllipse(eyeBrush, headCx - eyeDx - eyeR, eyeY - eyeR, eyeR * 2, eyeR * 2);
                    g.FillEllipse(eyeBrush, headCx + eyeDx - eyeR, eyeY - eyeR, eyeR * 2, eyeR * 2);
                }
            }

            // Нос
            using (var noseBrush = new SolidBrush(skinDark))
            {
                float noseR = headR * 0.16f;
                g.FillEllipse(noseBrush, headCx - noseR, headCy + headR * 0.05f, noseR * 2, noseR * 1.6f);
            }

            // --- Големият мустак ---
            using (var mustacheBrush = new SolidBrush(mustache))
            {
                float mw = headR * 0.72f;
                float mh = headR * 0.34f;
                float my = headCy + headR * 0.30f;
                g.FillEllipse(mustacheBrush, headCx - mw, my, mw, mh);
                g.FillEllipse(mustacheBrush, headCx, my, mw, mh);
            }

            // --- Уста (под мустака, вижда се по края) ---
            float mouthY = headCy + headR * 0.68f;
            using (var mouthPen = new Pen(Color.FromArgb(140, 70, 60), Math.Max(2f, size * 0.02f))
                       { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                if (_mood == Mood.Happy)
                {
                    g.DrawArc(mouthPen,
                        headCx - headR * 0.30f, mouthY - headR * 0.18f,
                        headR * 0.6f, headR * 0.3f, 20, 140);
                }
                else if (_mood == Mood.Angry || _mood == Mood.Furious)
                {
                    g.DrawArc(mouthPen,
                        headCx - headR * 0.30f, mouthY,
                        headR * 0.6f, headR * 0.3f, 200, 140);
                }
                else
                {
                    g.DrawLine(mouthPen,
                        headCx - headR * 0.22f, mouthY,
                        headCx + headR * 0.22f, mouthY);
                }
            }

            // Зачервени бузи при гняв
            if (_mood == Mood.Angry || _mood == Mood.Furious)
            {
                using (var blush = new SolidBrush(Theme.WithAlpha(Theme.RedBright, 60)))
                {
                    float bl = headR * 0.24f;
                    g.FillEllipse(blush, headCx - headR * 0.78f, headCy + headR * 0.12f, bl, bl * 0.7f);
                    g.FillEllipse(blush, headCx + headR * 0.54f, headCy + headR * 0.12f, bl, bl * 0.7f);
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

        // =====================================================================
        //  Незадължителни PNG спрайтове (кеширани, безопасни при липса)
        // =====================================================================

        private static class PeshoSprites
        {
            private static readonly System.Collections.Generic.Dictionary<string, Image> Cache =
                new System.Collections.Generic.Dictionary<string, Image>();

            public static Image Get(string moodName)
            {
                string key = moodName.ToLowerInvariant();

                if (Cache.TryGetValue(key, out Image cached))
                {
                    return cached; // може да е и null – значи вече знаем, че липсва
                }

                Image image = null;
                try
                {
                    string path = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Assets", "Pesho", "pesho_" + key + ".png");

                    if (File.Exists(path))
                    {
                        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            image = Image.FromStream(stream);
                        }
                    }
                }
                catch
                {
                    image = null; // повредена/липсваща картинка -> вграденото рисуване
                }

                Cache[key] = image;
                return image;
            }
        }
    }
}