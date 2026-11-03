using System.Drawing;
using System.Windows.Forms;
using ZhadniyatProgramist.Controls;

namespace ZhadniyatProgramist
{
    partial class MainForm
    {
        /// <summary>Контейнер за компоненти (таймерите се добавят тук и се освобождават автоматично).</summary>
        private System.ComponentModel.IContainer components = null;

        #region Полета с контроли

        private TableLayoutPanel tableRoot;

        // --- Статистически карти (горе вляво) ---
        private TableLayoutPanel tableStats;
        private ModernPanel cardStatProgrammers;
        private Label lblStatProgrammersTitle;
        private Label lblStatProgrammersValue;
        private ModernPanel cardStatTabs;
        private Label lblStatTabsTitle;
        private Label lblStatTabsValue;
        private ModernPanel cardStatDebt;
        private Label lblStatDebtTitle;
        private Label lblStatDebtValue;
        private ModernPanel cardStatTop;
        private Label lblStatTopTitle;
        private Label lblStatTopName;
        private Label lblStatTopSum;

        // --- Таблица с всички поръчки (JOIN) ---
        private ModernPanel panelGrid;
        private TableLayoutPanel tableGridLayout;
        private Label lblGridTitle;
        private TextBox txtSearch;
        private DataGridView gridOrders;

        // --- Дясна колона: избор на програмист + дълг + плащане ---
        private ModernPanel panelRight;
        private TableLayoutPanel tableRight;
        private Label lblChooseTitle;
        private Label lblChooseHint;
        private AnimatedComboBox comboProgrammers;
        private ModernPanel panelDebt;
        private Label lblDebtCaption;
        private Label lblDebtAmount;
        private Label lblDebtStatus;
        private PeshoCharacterControl peshoCharacter;
        private ModernButton btnPay;

        // --- Долна лента: формуляри за добавяне ---
        private TableLayoutPanel tableBottom;
        private ModernPanel panelAddOrder;
        private TableLayoutPanel tableAddOrder;
        private Label lblAddOrderTitle;
        private Label lblOrderProgrammer;
        private AnimatedComboBox comboOrderProgrammer;
        private Label lblOrderName;
        private TextBox txtOrderName;
        private Label lblOrderPrice;
        private TextBox txtOrderPrice;
        private ModernButton btnAddOrder;

        private ModernPanel panelAddProgrammer;
        private TableLayoutPanel tableAddProgrammer;
        private Label lblAddProgTitle;
        private Label lblProgName;
        private TextBox txtProgName;
        private Label lblProgLang;
        private TextBox txtProgLanguage;
        private ModernButton btnAddProgrammer;

        // --- Долен статус бар ---
        private TableLayoutPanel tableFooter;
        private Label lblConnection;
        private Label lblFooterCenter;
        private Label lblClock;

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Изгражда целия интерфейс. Всичко е върху TableLayoutPanel-и
        /// с процентни колони, за да се държи добре при resize.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            SuspendLayout();

            // =================================================================
            //  Коренна решетка: 2 колони (основна / дясна), 4 реда
            // =================================================================
            tableRoot = new TableLayoutPanel();
            tableRoot.Dock = DockStyle.Fill;
            tableRoot.BackColor = Theme.Background;
            tableRoot.Padding = new Padding(14, 12, 14, 4);
            tableRoot.ColumnCount = 2;
            tableRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
            tableRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
            tableRoot.RowCount = 4;
            tableRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 108f)); // статистика
            tableRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // таблица
            tableRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 228f)); // формуляри
            tableRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));  // статус бар

            // =================================================================
            //  Статистически карти
            // =================================================================
            tableStats = new TableLayoutPanel();
            tableStats.Dock = DockStyle.Fill;
            tableStats.BackColor = Theme.Background;
            tableStats.Margin = new Padding(0);
            tableStats.ColumnCount = 4;
            tableStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tableStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tableStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tableStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tableStats.RowCount = 1;
            tableStats.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // --- Карта: Общо програмисти ---
            cardStatProgrammers = new ModernPanel();
            cardStatProgrammers.Dock = DockStyle.Fill;
            cardStatProgrammers.Margin = new Padding(0, 0, 10, 8);
            cardStatProgrammers.Padding = new Padding(14, 10, 14, 10);
            cardStatProgrammers.BackColor = Theme.Surface;
            cardStatProgrammers.BorderColor = Color.FromArgb(0, 110, 66);

            lblStatProgrammersValue = new Label();
            lblStatProgrammersValue.Dock = DockStyle.Fill;
            lblStatProgrammersValue.Font = new Font("Segoe UI Semibold", 16f);
            lblStatProgrammersValue.ForeColor = Theme.NeonGreen;
            lblStatProgrammersValue.TextAlign = ContentAlignment.MiddleLeft;
            lblStatProgrammersValue.Text = "0";

            lblStatProgrammersTitle = new Label();
            lblStatProgrammersTitle.Dock = DockStyle.Top;
            lblStatProgrammersTitle.Height = 22;
            lblStatProgrammersTitle.Font = new Font("Segoe UI", 9f);
            lblStatProgrammersTitle.ForeColor = Theme.TextMuted;
            lblStatProgrammersTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblStatProgrammersTitle.Text = "👥  Общо програмисти";

            cardStatProgrammers.Controls.Add(lblStatProgrammersValue);
            cardStatProgrammers.Controls.Add(lblStatProgrammersTitle);

            // --- Карта: Активни сметки ---
            cardStatTabs = new ModernPanel();
            cardStatTabs.Dock = DockStyle.Fill;
            cardStatTabs.Margin = new Padding(0, 0, 10, 8);
            cardStatTabs.Padding = new Padding(14, 10, 14, 10);
            cardStatTabs.BackColor = Theme.Surface;
            cardStatTabs.BorderColor = Color.FromArgb(146, 104, 0);

            lblStatTabsValue = new Label();
            lblStatTabsValue.Dock = DockStyle.Fill;
            lblStatTabsValue.Font = new Font("Segoe UI Semibold", 16f);
            lblStatTabsValue.ForeColor = Theme.Amber;
            lblStatTabsValue.TextAlign = ContentAlignment.MiddleLeft;
            lblStatTabsValue.Text = "0";

            lblStatTabsTitle = new Label();
            lblStatTabsTitle.Dock = DockStyle.Top;
            lblStatTabsTitle.Height = 22;
            lblStatTabsTitle.Font = new Font("Segoe UI", 9f);
            lblStatTabsTitle.ForeColor = Theme.TextMuted;
            lblStatTabsTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblStatTabsTitle.Text = "📒  Активни сметки";

            cardStatTabs.Controls.Add(lblStatTabsValue);
            cardStatTabs.Controls.Add(lblStatTabsTitle);

            // --- Карта: Обща сума дългове ---
            cardStatDebt = new ModernPanel();
            cardStatDebt.Dock = DockStyle.Fill;
            cardStatDebt.Margin = new Padding(0, 0, 10, 8);
            cardStatDebt.Padding = new Padding(14, 10, 14, 10);
            cardStatDebt.BackColor = Theme.Surface;
            cardStatDebt.BorderColor = Color.FromArgb(140, 46, 46);

            lblStatDebtValue = new Label();
            lblStatDebtValue.Dock = DockStyle.Fill;
            lblStatDebtValue.Font = new Font("Segoe UI Semibold", 16f);
            lblStatDebtValue.ForeColor = Theme.Red;
            lblStatDebtValue.TextAlign = ContentAlignment.MiddleLeft;
            lblStatDebtValue.Text = "0.00 лв.";

            lblStatDebtTitle = new Label();
            lblStatDebtTitle.Dock = DockStyle.Top;
            lblStatDebtTitle.Height = 22;
            lblStatDebtTitle.Font = new Font("Segoe UI", 9f);
            lblStatDebtTitle.ForeColor = Theme.TextMuted;
            lblStatDebtTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblStatDebtTitle.Text = "💰  Обща сума дългове";

            cardStatDebt.Controls.Add(lblStatDebtValue);
            cardStatDebt.Controls.Add(lblStatDebtTitle);

            // --- Карта: Най-затънал програмист ---
            cardStatTop = new ModernPanel();
            cardStatTop.Dock = DockStyle.Fill;
            cardStatTop.Margin = new Padding(0, 0, 0, 8);
            cardStatTop.Padding = new Padding(14, 8, 14, 8);
            cardStatTop.BackColor = Theme.Surface;
            cardStatTop.BorderColor = Color.FromArgb(140, 46, 46);

            lblStatTopSum = new Label();
            lblStatTopSum.Dock = DockStyle.Fill;
            lblStatTopSum.Font = new Font("Segoe UI Semibold", 12.5f);
            lblStatTopSum.ForeColor = Theme.Red;
            lblStatTopSum.TextAlign = ContentAlignment.MiddleLeft;
            lblStatTopSum.Text = "0.00 лв.";

            lblStatTopName = new Label();
            lblStatTopName.Dock = DockStyle.Top;
            lblStatTopName.Height = 24;
            lblStatTopName.Font = new Font("Segoe UI Semibold", 10.5f);
            lblStatTopName.ForeColor = Theme.TextPrimary;
            lblStatTopName.TextAlign = ContentAlignment.MiddleLeft;
            lblStatTopName.AutoEllipsis = true;
            lblStatTopName.Text = "—";

            lblStatTopTitle = new Label();
            lblStatTopTitle.Dock = DockStyle.Top;
            lblStatTopTitle.Height = 20;
            lblStatTopTitle.Font = new Font("Segoe UI", 9f);
            lblStatTopTitle.ForeColor = Theme.TextMuted;
            lblStatTopTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblStatTopTitle.Text = "💀  Най-затънал";

            cardStatTop.Controls.Add(lblStatTopSum);
            cardStatTop.Controls.Add(lblStatTopName);
            cardStatTop.Controls.Add(lblStatTopTitle);

            tableStats.Controls.Add(cardStatProgrammers, 0, 0);
            tableStats.Controls.Add(cardStatTabs, 1, 0);
            tableStats.Controls.Add(cardStatDebt, 2, 0);
            tableStats.Controls.Add(cardStatTop, 3, 0);

            // =================================================================
            //  Панел с таблицата на поръчките
            // =================================================================
            panelGrid = new ModernPanel();
            panelGrid.Dock = DockStyle.Fill;
            panelGrid.Margin = new Padding(0, 2, 10, 8);
            panelGrid.Padding = new Padding(16);
            panelGrid.BackColor = Theme.Surface;
            panelGrid.BorderColor = Theme.Border;
            panelGrid.BorderRadius = 16;

            tableGridLayout = new TableLayoutPanel();
            tableGridLayout.Dock = DockStyle.Fill;
            tableGridLayout.BackColor = Theme.Surface;
            tableGridLayout.ColumnCount = 2;
            tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250f));
            tableGridLayout.RowCount = 2;
            tableGridLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            tableGridLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            lblGridTitle = new Label();
            lblGridTitle.Dock = DockStyle.Fill;
            lblGridTitle.Font = new Font("Segoe UI Semibold", 13f);
            lblGridTitle.ForeColor = Theme.NeonGreen;
            lblGridTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblGridTitle.Margin = new Padding(0);
            lblGridTitle.Text = "📒  Всички поръчки в тефтера";

            txtSearch = new TextBox();
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Margin = new Padding(0, 8, 0, 8);
            txtSearch.BackColor = Theme.SurfaceLight;
            txtSearch.ForeColor = Theme.TextPrimary;
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = new Font("Segoe UI", 10f);
            txtSearch.PlaceholderText = "🔍  Търси по име, поръчка или език…";

            gridOrders = new DataGridView();
            gridOrders.Dock = DockStyle.Fill;
            gridOrders.Margin = new Padding(0, 6, 0, 0);

            tableGridLayout.Controls.Add(lblGridTitle, 0, 0);
            tableGridLayout.Controls.Add(txtSearch, 1, 0);
            tableGridLayout.Controls.Add(gridOrders, 0, 1);
            tableGridLayout.SetColumnSpan(gridOrders, 2);

            panelGrid.Controls.Add(tableGridLayout);

            // =================================================================
            //  Дясна колона: избор на програмист, дълг, плащане
            // =================================================================
            panelRight = new ModernPanel();
            panelRight.Dock = DockStyle.Fill;
            panelRight.Margin = new Padding(2, 0, 0, 8);
            panelRight.Padding = new Padding(18);
            panelRight.BackColor = Theme.Surface;
            panelRight.BorderColor = Theme.Border;
            panelRight.BorderRadius = 16;

            tableRight = new TableLayoutPanel();
            tableRight.Dock = DockStyle.Fill;
            tableRight.BackColor = Theme.Surface;
            tableRight.ColumnCount = 1;
            tableRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tableRight.RowCount = 6;
            tableRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));  // заглавие
            tableRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));  // описание
            tableRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));  // combo
            tableRight.RowStyles.Add(new RowStyle(SizeType.Percent, 44f));   // панел с дълга
            tableRight.RowStyles.Add(new RowStyle(SizeType.Percent, 56f));   // Бай Пешо
            tableRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));  // бутон

            lblChooseTitle = new Label();
            lblChooseTitle.Dock = DockStyle.Fill;
            lblChooseTitle.Font = new Font("Segoe UI Semibold", 13f);
            lblChooseTitle.ForeColor = Theme.TextPrimary;
            lblChooseTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblChooseTitle.Margin = new Padding(0);
            lblChooseTitle.Text = "Избери програмист";

            lblChooseHint = new Label();
            lblChooseHint.Dock = DockStyle.Fill;
            lblChooseHint.Font = new Font("Segoe UI", 9f);
            lblChooseHint.ForeColor = Theme.TextMuted;
            lblChooseHint.TextAlign = ContentAlignment.TopLeft;
            lblChooseHint.Margin = new Padding(0);
            lblChooseHint.Text = "Избери програмист, за да видиш колко е затънал.";

            comboProgrammers = new AnimatedComboBox();
            comboProgrammers.Dock = DockStyle.Top;
            comboProgrammers.Margin = new Padding(0, 8, 0, 0);

            // --- Панел с общия дълг ---
            panelDebt = new ModernPanel();
            panelDebt.Dock = DockStyle.Fill;
            panelDebt.Margin = new Padding(0, 10, 0, 6);
            panelDebt.Padding = new Padding(10);
            panelDebt.BackColor = Theme.Background;
            panelDebt.BorderColor = Theme.Border;
            panelDebt.BorderRadius = 16;

            lblDebtStatus = new Label();
            lblDebtStatus.Dock = DockStyle.Fill;
            lblDebtStatus.Font = new Font("Segoe UI Semibold", 10.5f);
            lblDebtStatus.ForeColor = Theme.TextMuted;
            lblDebtStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblDebtStatus.Text = "Избери програмист от менюто горе.";

            lblDebtAmount = new Label();
            lblDebtAmount.Dock = DockStyle.Top;
            lblDebtAmount.Height = 56;
            lblDebtAmount.Font = new Font("Segoe UI", 24f, FontStyle.Bold);
            lblDebtAmount.ForeColor = Theme.TextMuted;
            lblDebtAmount.TextAlign = ContentAlignment.MiddleCenter;
            lblDebtAmount.Text = "—";

            lblDebtCaption = new Label();
            lblDebtCaption.Dock = DockStyle.Top;
            lblDebtCaption.Height = 26;
            lblDebtCaption.Font = new Font("Segoe UI Semibold", 12f);
            lblDebtCaption.ForeColor = Theme.TextPrimary;
            lblDebtCaption.TextAlign = ContentAlignment.MiddleCenter;
            lblDebtCaption.Text = "Общ дълг:";

            panelDebt.Controls.Add(lblDebtStatus);
            panelDebt.Controls.Add(lblDebtAmount);
            panelDebt.Controls.Add(lblDebtCaption);

            // --- Бутон „Плати сметката“ ---
            btnPay = new ModernButton();
            btnPay.Dock = DockStyle.Fill;
            btnPay.Margin = new Padding(0, 2, 0, 4);
            btnPay.BackColor = Color.FromArgb(183, 28, 28);
            btnPay.HoverColor = Color.FromArgb(229, 57, 53);
            btnPay.ForeColor = Color.White;
            btnPay.BorderRadius = 14;
            btnPay.Font = new Font("Segoe UI Semibold", 12f);
            btnPay.Text = "🍻  ПЛАТИ СМЕТКАТА";

            // --- Бай Пешо – анимиран персонаж, реагиращ на дълга ---
            peshoCharacter = new PeshoCharacterControl();
            peshoCharacter.Dock = DockStyle.Fill;
            peshoCharacter.Margin = new Padding(0, 0, 0, 10);

            tableRight.Controls.Add(lblChooseTitle, 0, 0);
            tableRight.Controls.Add(lblChooseHint, 0, 1);
            tableRight.Controls.Add(comboProgrammers, 0, 2);
            tableRight.Controls.Add(panelDebt, 0, 3);
            tableRight.Controls.Add(peshoCharacter, 0, 4);
            tableRight.Controls.Add(btnPay, 0, 5);

            panelRight.Controls.Add(tableRight);

            // =================================================================
            //  Долна лента: два формуляра
            // =================================================================
            tableBottom = new TableLayoutPanel();
            tableBottom.Dock = DockStyle.Fill;
            tableBottom.BackColor = Theme.Background;
            tableBottom.Margin = new Padding(0);
            tableBottom.ColumnCount = 2;
            tableBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tableBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tableBottom.RowCount = 1;
            tableBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // --- Формуляр: Добави нова поръчка ---
            panelAddOrder = new ModernPanel();
            panelAddOrder.Dock = DockStyle.Fill;
            panelAddOrder.Margin = new Padding(0, 2, 8, 0);
            panelAddOrder.Padding = new Padding(16, 12, 16, 12);
            panelAddOrder.BackColor = Theme.Surface;
            panelAddOrder.BorderColor = Color.FromArgb(0, 110, 66);
            panelAddOrder.BorderRadius = 16;

            tableAddOrder = new TableLayoutPanel();
            tableAddOrder.Dock = DockStyle.Fill;
            tableAddOrder.BackColor = Theme.Surface;
            tableAddOrder.ColumnCount = 2;
            tableAddOrder.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
            tableAddOrder.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tableAddOrder.RowCount = 5;
            tableAddOrder.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f)); // заглавие
            tableAddOrder.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f)); // програмист
            tableAddOrder.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f)); // поръчка
            tableAddOrder.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f)); // цена
            tableAddOrder.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // бутон

            lblAddOrderTitle = new Label();
            lblAddOrderTitle.Dock = DockStyle.Fill;
            lblAddOrderTitle.Font = new Font("Segoe UI Semibold", 12f);
            lblAddOrderTitle.ForeColor = Theme.NeonGreen;
            lblAddOrderTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblAddOrderTitle.Margin = new Padding(0);
            lblAddOrderTitle.Text = "🍺  Добави нова поръчка";

            lblOrderProgrammer = new Label();
            lblOrderProgrammer.Dock = DockStyle.Fill;
            lblOrderProgrammer.Font = new Font("Segoe UI", 9.75f);
            lblOrderProgrammer.ForeColor = Theme.TextMuted;
            lblOrderProgrammer.TextAlign = ContentAlignment.MiddleLeft;
            lblOrderProgrammer.Margin = new Padding(0);
            lblOrderProgrammer.Text = "Програмист:";

            comboOrderProgrammer = new AnimatedComboBox();
            comboOrderProgrammer.Dock = DockStyle.Top;
            comboOrderProgrammer.Margin = new Padding(0, 4, 0, 4);

            lblOrderName = new Label();
            lblOrderName.Dock = DockStyle.Fill;
            lblOrderName.Font = new Font("Segoe UI", 9.75f);
            lblOrderName.ForeColor = Theme.TextMuted;
            lblOrderName.TextAlign = ContentAlignment.MiddleLeft;
            lblOrderName.Margin = new Padding(0);
            lblOrderName.Text = "Поръчка:";

            txtOrderName = new TextBox();
            txtOrderName.Dock = DockStyle.Fill;
            txtOrderName.Margin = new Padding(0, 6, 0, 6);
            txtOrderName.BackColor = Theme.SurfaceLight;
            txtOrderName.ForeColor = Theme.TextPrimary;
            txtOrderName.BorderStyle = BorderStyle.FixedSingle;
            txtOrderName.Font = new Font("Segoe UI", 10f);
            txtOrderName.PlaceholderText = "Какво поръча?";

            lblOrderPrice = new Label();
            lblOrderPrice.Dock = DockStyle.Fill;
            lblOrderPrice.Font = new Font("Segoe UI", 9.75f);
            lblOrderPrice.ForeColor = Theme.TextMuted;
            lblOrderPrice.TextAlign = ContentAlignment.MiddleLeft;
            lblOrderPrice.Margin = new Padding(0);
            lblOrderPrice.Text = "Цена (лв.):";

            txtOrderPrice = new TextBox();
            txtOrderPrice.Dock = DockStyle.Fill;
            txtOrderPrice.Margin = new Padding(0, 6, 0, 6);
            txtOrderPrice.BackColor = Theme.SurfaceLight;
            txtOrderPrice.ForeColor = Theme.TextPrimary;
            txtOrderPrice.BorderStyle = BorderStyle.FixedSingle;
            txtOrderPrice.Font = new Font("Segoe UI", 10f);
            txtOrderPrice.PlaceholderText = "0.00";

            btnAddOrder = new ModernButton();
            btnAddOrder.Dock = DockStyle.Fill;
            btnAddOrder.Margin = new Padding(0, 6, 0, 2);
            btnAddOrder.BackColor = Color.FromArgb(0, 135, 80);
            btnAddOrder.HoverColor = Color.FromArgb(0, 175, 102);
            btnAddOrder.ForeColor = Color.White;
            btnAddOrder.BorderRadius = 12;
            btnAddOrder.Font = new Font("Segoe UI Semibold", 11f);
            btnAddOrder.Text = "➕  Добави към тефтера";

            tableAddOrder.Controls.Add(lblAddOrderTitle, 0, 0);
            tableAddOrder.SetColumnSpan(lblAddOrderTitle, 2);
            tableAddOrder.Controls.Add(lblOrderProgrammer, 0, 1);
            tableAddOrder.Controls.Add(comboOrderProgrammer, 1, 1);
            tableAddOrder.Controls.Add(lblOrderName, 0, 2);
            tableAddOrder.Controls.Add(txtOrderName, 1, 2);
            tableAddOrder.Controls.Add(lblOrderPrice, 0, 3);
            tableAddOrder.Controls.Add(txtOrderPrice, 1, 3);
            tableAddOrder.Controls.Add(btnAddOrder, 0, 4);
            tableAddOrder.SetColumnSpan(btnAddOrder, 2);

            panelAddOrder.Controls.Add(tableAddOrder);

            // --- Формуляр: Добави нов програмист ---
            panelAddProgrammer = new ModernPanel();
            panelAddProgrammer.Dock = DockStyle.Fill;
            panelAddProgrammer.Margin = new Padding(8, 2, 0, 0);
            panelAddProgrammer.Padding = new Padding(16, 12, 16, 12);
            panelAddProgrammer.BackColor = Theme.Surface;
            panelAddProgrammer.BorderColor = Color.FromArgb(146, 104, 0);
            panelAddProgrammer.BorderRadius = 16;

            tableAddProgrammer = new TableLayoutPanel();
            tableAddProgrammer.Dock = DockStyle.Fill;
            tableAddProgrammer.BackColor = Theme.Surface;
            tableAddProgrammer.ColumnCount = 2;
            tableAddProgrammer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
            tableAddProgrammer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tableAddProgrammer.RowCount = 5;
            tableAddProgrammer.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f)); // заглавие
            tableAddProgrammer.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f)); // име
            tableAddProgrammer.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f)); // език
            tableAddProgrammer.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // разстояние
            tableAddProgrammer.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f)); // бутон

            lblAddProgTitle = new Label();
            lblAddProgTitle.Dock = DockStyle.Fill;
            lblAddProgTitle.Font = new Font("Segoe UI Semibold", 12f);
            lblAddProgTitle.ForeColor = Theme.Amber;
            lblAddProgTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblAddProgTitle.Margin = new Padding(0);
            lblAddProgTitle.Text = "👨‍💻  Добави нов програмист";

            lblProgName = new Label();
            lblProgName.Dock = DockStyle.Fill;
            lblProgName.Font = new Font("Segoe UI", 9.75f);
            lblProgName.ForeColor = Theme.TextMuted;
            lblProgName.TextAlign = ContentAlignment.MiddleLeft;
            lblProgName.Margin = new Padding(0);
            lblProgName.Text = "Име:";

            txtProgName = new TextBox();
            txtProgName.Dock = DockStyle.Fill;
            txtProgName.Margin = new Padding(0, 6, 0, 6);
            txtProgName.BackColor = Theme.SurfaceLight;
            txtProgName.ForeColor = Theme.TextPrimary;
            txtProgName.BorderStyle = BorderStyle.FixedSingle;
            txtProgName.Font = new Font("Segoe UI", 10f);
            txtProgName.PlaceholderText = "Име на програмиста";

            lblProgLang = new Label();
            lblProgLang.Dock = DockStyle.Fill;
            lblProgLang.Font = new Font("Segoe UI", 9.75f);
            lblProgLang.ForeColor = Theme.TextMuted;
            lblProgLang.TextAlign = ContentAlignment.MiddleLeft;
            lblProgLang.Margin = new Padding(0);
            lblProgLang.Text = "Любим език:";

            txtProgLanguage = new TextBox();
            txtProgLanguage.Dock = DockStyle.Fill;
            txtProgLanguage.Margin = new Padding(0, 6, 0, 6);
            txtProgLanguage.BackColor = Theme.SurfaceLight;
            txtProgLanguage.ForeColor = Theme.TextPrimary;
            txtProgLanguage.BorderStyle = BorderStyle.FixedSingle;
            txtProgLanguage.Font = new Font("Segoe UI", 10f);
            txtProgLanguage.PlaceholderText = "Любим език";

            btnAddProgrammer = new ModernButton();
            btnAddProgrammer.Dock = DockStyle.Fill;
            btnAddProgrammer.Margin = new Padding(0, 2, 0, 2);
            btnAddProgrammer.BackColor = Color.FromArgb(255, 171, 0);
            btnAddProgrammer.HoverColor = Color.FromArgb(255, 196, 60);
            btnAddProgrammer.ForeColor = Color.FromArgb(30, 22, 6);
            btnAddProgrammer.BorderRadius = 12;
            btnAddProgrammer.Font = new Font("Segoe UI Semibold", 11f);
            btnAddProgrammer.Text = "👨‍💻  Добави програмист";

            tableAddProgrammer.Controls.Add(lblAddProgTitle, 0, 0);
            tableAddProgrammer.SetColumnSpan(lblAddProgTitle, 2);
            tableAddProgrammer.Controls.Add(lblProgName, 0, 1);
            tableAddProgrammer.Controls.Add(txtProgName, 1, 1);
            tableAddProgrammer.Controls.Add(lblProgLang, 0, 2);
            tableAddProgrammer.Controls.Add(txtProgLanguage, 1, 2);
            tableAddProgrammer.Controls.Add(btnAddProgrammer, 0, 4);
            tableAddProgrammer.SetColumnSpan(btnAddProgrammer, 2);

            panelAddProgrammer.Controls.Add(tableAddProgrammer);

            tableBottom.Controls.Add(panelAddOrder, 0, 0);
            tableBottom.Controls.Add(panelAddProgrammer, 1, 0);

            // =================================================================
            //  Долен статус бар
            // =================================================================
            tableFooter = new TableLayoutPanel();
            tableFooter.Dock = DockStyle.Fill;
            tableFooter.BackColor = Theme.Background;
            tableFooter.Margin = new Padding(0);
            tableFooter.ColumnCount = 3;
            tableFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            tableFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            tableFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            tableFooter.RowCount = 1;
            tableFooter.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            lblConnection = new Label();
            lblConnection.Dock = DockStyle.Fill;
            lblConnection.Font = new Font("Segoe UI", 9f);
            lblConnection.ForeColor = Theme.NeonGreen;
            lblConnection.TextAlign = ContentAlignment.MiddleLeft;
            lblConnection.Margin = new Padding(0);
            lblConnection.Text = "●  Свързан с базата данни";

            lblFooterCenter = new Label();
            lblFooterCenter.Dock = DockStyle.Fill;
            lblFooterCenter.Font = new Font("Segoe UI", 9f);
            lblFooterCenter.ForeColor = Theme.TextMuted;
            lblFooterCenter.TextAlign = ContentAlignment.MiddleCenter;
            lblFooterCenter.Margin = new Padding(0);
            lblFooterCenter.Text = "Кръчма „Жадният Програмист“ © 2026 — бай Пешо следи всичко…";

            lblClock = new Label();
            lblClock.Dock = DockStyle.Fill;
            lblClock.Font = new Font("Segoe UI Semibold", 9.5f);
            lblClock.ForeColor = Theme.TextMuted;
            lblClock.TextAlign = ContentAlignment.MiddleRight;
            lblClock.Margin = new Padding(0);
            lblClock.Text = "00:00:00";

            tableFooter.Controls.Add(lblConnection, 0, 0);
            tableFooter.Controls.Add(lblFooterCenter, 1, 0);
            tableFooter.Controls.Add(lblClock, 2, 0);

            // =================================================================
            //  Сглобяване на коренната решетка
            // =================================================================
            tableRoot.Controls.Add(tableStats, 0, 0);
            tableRoot.Controls.Add(panelRight, 1, 0);
            tableRoot.SetRowSpan(panelRight, 2);
            tableRoot.Controls.Add(panelGrid, 0, 1);
            tableRoot.Controls.Add(tableBottom, 0, 2);
            tableRoot.SetColumnSpan(tableBottom, 2);
            tableRoot.Controls.Add(tableFooter, 0, 3);
            tableRoot.SetColumnSpan(tableFooter, 2);

            // =================================================================
            //  Самата форма
            // =================================================================
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1340, 900);
            MinimumSize = new Size(1200, 840);
            BackColor = Theme.Background;
            ForeColor = Theme.TextPrimary;
            Font = new Font("Segoe UI", 9.75f);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "🍺 Кръчма „Жадният Програмист“ — тефтерите на бай Пешо";

            Controls.Add(tableRoot);

            ResumeLayout(false);
        }
    }
}
