using System.Drawing;
using System.Windows.Forms;
using ZhadniyatProgramist.Controls;

namespace ZhadniyatProgramist
{
    partial class LoadingForm
    {
        /// <summary>Контейнер за компоненти (таймерите се добавят тук и се освобождават автоматично).</summary>
        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblPercent;
        private Panel panelProgress;
        private Label lblStatus;
        private Label lblFooter;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            SuspendLayout();

            // --- Заглавие ---
            lblTitle = new Label();
            lblTitle.Location = new Point(20, 46);
            lblTitle.Size = new Size(520, 46);
            lblTitle.Font = new Font("Segoe UI Semibold", 19f);
            lblTitle.ForeColor = Theme.Amber;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Text = "🍺 Кръчма „Жадният Програмист“";

            // --- Подзаглавие ---
            lblSubtitle = new Label();
            lblSubtitle.Location = new Point(20, 98);
            lblSubtitle.Size = new Size(520, 26);
            lblSubtitle.Font = new Font("Segoe UI", 10.5f);
            lblSubtitle.ForeColor = Theme.TextMuted;
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            lblSubtitle.Text = "Зареждане на тефтерите на бай Пешо…";

            // --- Процент ---
            lblPercent = new Label();
            lblPercent.Location = new Point(60, 150);
            lblPercent.Size = new Size(440, 24);
            lblPercent.Font = new Font("Segoe UI Semibold", 10f);
            lblPercent.ForeColor = Theme.NeonGreen;
            lblPercent.TextAlign = ContentAlignment.MiddleRight;
            lblPercent.Text = "0%";

            // --- Progress bar (custom рисуване в LoadingForm.cs) ---
            panelProgress = new Panel();
            panelProgress.Location = new Point(60, 178);
            panelProgress.Size = new Size(440, 16);
            panelProgress.BackColor = Theme.Background;

            // --- Хумористичен статус ---
            lblStatus = new Label();
            lblStatus.Location = new Point(40, 214);
            lblStatus.Size = new Size(480, 28);
            lblStatus.Font = new Font("Segoe UI", 9.75f, FontStyle.Italic);
            lblStatus.ForeColor = Theme.TextMuted;
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.Text = "Отваряме кръчмата…";

            // --- Мото на кръчмата ---
            lblFooter = new Label();
            lblFooter.Location = new Point(20, 288);
            lblFooter.Size = new Size(520, 22);
            lblFooter.Font = new Font("Segoe UI", 8.5f);
            lblFooter.ForeColor = Color.FromArgb(90, 98, 107);
            lblFooter.TextAlign = ContentAlignment.MiddleCenter;
            lblFooter.Text = "Кодът може да е бъгав, но бирата е истинска!";

            // --- Формата ---
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(560, 330);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Background;
            ForeColor = Theme.TextPrimary;
            ShowInTaskbar = true;
            Text = "Кръчма „Жадният Програмист“ — зареждане";

            Controls.Add(lblTitle);
            Controls.Add(lblSubtitle);
            Controls.Add(lblPercent);
            Controls.Add(panelProgress);
            Controls.Add(lblStatus);
            Controls.Add(lblFooter);

            ResumeLayout(false);
        }
    }
}
