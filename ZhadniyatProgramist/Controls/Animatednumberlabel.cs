using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ZhadniyatProgramist.Controls
{
    /// <summary>
    /// Label за статистики, чиято стойност НЕ се сменя рязко,
    /// а „тича“ плавно от старата към новата (0 → 5 → 12 → 28 → 45).
    ///
    ///  - Decimals = 0  -> цели бройки;
    ///  - Decimals = 2  -> суми (12.50);
    ///  - Suffix         -> напр. " лв.".
    /// </summary>
    public class AnimatedNumberLabel : Label
    {
        private readonly Timer _timer;
        private double _current;
        private double _target;

        [Category("Behavior")]
        public int Decimals { get; set; }

        [Category("Behavior")]
        public string Suffix { get; set; } = string.Empty;

        public AnimatedNumberLabel()
        {
            _timer = new Timer { Interval = 25 };
            _timer.Tick += Timer_Tick;
            UpdateText();
        }

        /// <summary>Задава нова стойност – с плавно броене или мигновено.</summary>
        public void SetValue(double value, bool animate = true)
        {
            _target = value;

            if (!animate || !IsHandleCreated || DesignMode)
            {
                _current = value;
                UpdateText();
                return;
            }

            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _current += (_target - _current) * 0.16;

            if (Math.Abs(_target - _current) < 0.01)
            {
                _current = _target;
                _timer.Stop();
            }

            UpdateText();
        }

        private void UpdateText()
        {
            string format = Decimals <= 0 ? "{0:0}" : "{0:0." + new string('0', Decimals) + "}";
            Text = string.Format(format, _current) + Suffix;
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