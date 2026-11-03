using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using ZhadniyatProgramist.Controls;

namespace ZhadniyatProgramist
{
    /// <summary>
    /// Loading екран, който се показва при стартиране:
    ///  - анимиран progress bar с градиент (зелено -> кехлибарено);
    ///  - процент на зареждане;
    ///  - хумористични статус съобщения, които се сменят;
    ///  - плавно изчезване (fade-out) преди главния прозорец.
    /// </summary>
    public partial class LoadingForm : Form
    {
        private readonly System.Windows.Forms.Timer _progressTimer;
        private readonly System.Windows.Forms.Timer _fadeTimer;
        private readonly Random _random = new Random();

        private double _progress;      // 0 .. 100
        private int _holdTicks = 22;   // кратка пауза на 100%, преди fade-out

        /// <summary>Съобщенията на бай Пешо по време на зареждане.</summary>
        private static readonly string[] StatusMessages =
        {
            "Проверяваме кой пак няма пари…",
            "Сумираме крафт бирите…",
            "Търсим изгубени портфейли…",
            "Оптимизираме SQL заявките…",
            "При мен работи… почти."
        };

        public LoadingForm()
        {
            InitializeComponent();
            DoubleBuffered = true;

            // Двойно буфериране на progress панела срещу трептене при анимация
            typeof(Panel).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, panelProgress, new object[] { true });

            _progressTimer = new System.Windows.Forms.Timer(components) { Interval = 40 };
            _progressTimer.Tick += ProgressTimer_Tick;

            _fadeTimer = new System.Windows.Forms.Timer(components) { Interval = 25 };
            _fadeTimer.Tick += FadeTimer_Tick;

            panelProgress.Paint += PanelProgress_Paint;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Заобляме ъглите на самата форма (тя е без стандартна рамка)
            using (var path = Theme.RoundedRect(new Rectangle(0, 0, Width, Height), 18))
            {
                Region = new Region(path);
            }

            _progressTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Тънка рамка около целия loading прозорец
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Theme.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 18))
            using (var pen = new Pen(Theme.Border, 1.5f))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }

        /// <summary>Стъпка на „зареждането“ – случайно нарастване за по-жив ефект.</summary>
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            _progress += _random.NextDouble() * 2.4 + 0.6;

            if (_progress >= 100)
            {
                _progress = 100;
                _progressTimer.Stop();
                lblStatus.Text = "Готово! Наздраве! 🍻";
                _fadeTimer.Start(); // плавен преход към главния прозорец
            }
            else
            {
                // Статусът се сменя според етапа на зареждане
                int index = Math.Min(
                    (int)(_progress / (100.0 / StatusMessages.Length)),
                    StatusMessages.Length - 1);
                lblStatus.Text = StatusMessages[index];
            }

            lblPercent.Text = string.Format("{0:0}%", _progress);
            panelProgress.Invalidate();
        }

        /// <summary>Плавно изчезване и затваряне на формата.</summary>
        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            if (_holdTicks > 0)
            {
                _holdTicks--; // кратка пауза, за да се види 100%
                return;
            }

            Opacity -= 0.08;
            if (Opacity <= 0)
            {
                _fadeTimer.Stop();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>Рисува custom progress bar с градиентен пълнеж.</summary>
        private void PanelProgress_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var track = new Rectangle(0, 0, panelProgress.Width - 1, panelProgress.Height - 1);

            // Фон („коловоз“) на бара
            using (var trackPath = Theme.RoundedRect(track, track.Height / 2))
            using (var trackBrush = new SolidBrush(Theme.SurfaceLight))
            {
                g.FillPath(trackBrush, trackPath);
            }

            // Пълнеж според прогреса
            int fillWidth = (int)(track.Width * _progress / 100.0);
            if (fillWidth > track.Height) // рисуваме едва когато има какво да се види
            {
                var fill = new Rectangle(0, 0, fillWidth, track.Height);
                using (var fillPath = Theme.RoundedRect(fill, track.Height / 2))
                using (var fillBrush = new LinearGradientBrush(
                    fill, Theme.NeonGreen, Theme.Amber, LinearGradientMode.Horizontal))
                {
                    g.FillPath(fillBrush, fillPath);
                }
            }
        }
    }
}
