using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ZhadniyatProgramist.Controls;
using ZhadniyatProgramist.Models;
using ZhadniyatProgramist.Services;

namespace ZhadniyatProgramist
{
    public partial class MainForm : Form
    {
        private readonly System.Windows.Forms.Timer _fadeTimer;
        private readonly System.Windows.Forms.Timer _debtTimer;
        private readonly System.Windows.Forms.Timer _clockTimer;
        private readonly System.Windows.Forms.Timer _dayNightTimer;
        private readonly Label lblDayNight;
        private double _displayedDebt;
        private double _targetDebt;
        private Color _debtColorCurrent;
        private Color _debtColorTarget;
        private CheckBox chkSound;
        private CoinBurstControl coinBurstControl;

        private const string AppTitle = "Тефтерът на бай Пешо";

        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Opacity = 0;

            _debtColorCurrent = Theme.TextMuted;
            _debtColorTarget = Theme.TextMuted;

            _fadeTimer = new System.Windows.Forms.Timer(components) { Interval = 20 };
            _fadeTimer.Tick += FadeTimer_Tick;

            _debtTimer = new System.Windows.Forms.Timer(components) { Interval = 15 };
            _debtTimer.Tick += DebtTimer_Tick;

            _clockTimer = new System.Windows.Forms.Timer(components) { Interval = 1000 };
            _clockTimer.Tick += (s, e) =>
            {
                if (lblClock != null)
                {
                    lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
                }

            };

            _dayNightTimer = new System.Windows.Forms.Timer(components) { Interval = 60000 };
            _dayNightTimer.Tick += (s, e) => UpdateDayNight();

            Load += MainForm_Load;

            if (comboProgrammers != null)
            {
                comboProgrammers.SelectedIndexChanged += (s, e) => UpdateDebtForSelection();
                comboProgrammers.SelectionChangeCommitted += (s, e) => SoundService.PlaySelect();
            }

            if (btnPay != null)
            {
                btnPay.Click += BtnPay_Click;
            }


            if (btnAddOrder != null)
            {
                btnAddOrder.Click += BtnAddOrder_Click;
            }


            if (btnAddProgrammer != null)
            {
                btnAddProgrammer.Click += BtnAddProgrammer_Click;
            }


            if (txtSearch != null)
            {
                txtSearch.TextChanged += (s, e) => RefreshGrid();
            }


            if (txtOrderPrice != null)
            {
                txtOrderPrice.KeyPress += TxtOrderPrice_KeyPress;
            }


            if (chkSound != null)

            {
                chkSound.CheckedChanged += ChkSound_CheckedChanged;
            }


            StyleGrid();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            EnableDarkTitleBar();

            LoadProgrammers();
            RefreshGrid();
            RefreshStats();
            UpdateDebtForSelection();
            UpdateDayNight();

            if (lblClock != null)
            {
                lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
            }


            _clockTimer.Start();
            _dayNightTimer.Start();
            _fadeTimer.Start();
        }

        private void EnableDarkTitleBar()
        {
            try
            {
                int useDarkMode = 1;
                DwmSetWindowAttribute(Handle, 20, ref useDarkMode, sizeof(int));
            }
            catch
            {
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd, int attribute, ref int value, int size);

        private void StyleGrid()
        {
            if (gridOrders == null)
            {
                return;
            }


            gridOrders.BorderStyle = BorderStyle.None;
            gridOrders.BackgroundColor = Theme.Surface;
            gridOrders.EnableHeadersVisualStyles = false;
            gridOrders.ReadOnly = true;
            gridOrders.AllowUserToAddRows = false;
            gridOrders.AllowUserToDeleteRows = false;
            gridOrders.AllowUserToResizeRows = false;
            gridOrders.RowHeadersVisible = false;
            gridOrders.MultiSelect = false;
            gridOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridOrders.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            gridOrders.GridColor = Theme.Border;

            gridOrders.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            gridOrders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            gridOrders.ColumnHeadersHeight = 46;
            gridOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 66, 40);
            gridOrders.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 66, 40);
            gridOrders.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextPrimary;
            gridOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10.5f);
            gridOrders.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            gridOrders.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);

            gridOrders.DefaultCellStyle.BackColor = Theme.Surface;
            gridOrders.DefaultCellStyle.ForeColor = Theme.TextPrimary;
            gridOrders.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 96, 57);
            gridOrders.DefaultCellStyle.SelectionForeColor = Color.White;
            gridOrders.DefaultCellStyle.Font = new Font("Segoe UI", 10.25f);
            gridOrders.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            gridOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(27, 32, 39);
            gridOrders.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 96, 57);
            gridOrders.RowTemplate.Height = 38;

            try
            {
                typeof(DataGridView).InvokeMember(
                    "DoubleBuffered",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                    null, gridOrders, new object[] { true });
            }
            catch
            {
            }
        }

        private void UpdateDayNight()
        {
            int hour = DateTime.Now.Hour;
            bool isDay = hour >= 7 && hour < 18;

            Theme.IsNight = !isDay;

            // Безопасно: ако tableRoot няма DayMode property, проектът няма да гръмне.
            TrySetDayMode(tableRoot, isDay);

            if (lblDayNight == null || lblFooterCenter == null)
            {
                return;
            }


            if (isDay)
            {
                lblDayNight.Text = "☀ Ден";
                lblDayNight.ForeColor = Theme.Amber;
                lblFooterCenter.Text =
                    "Кръчма „Жадният Програмист“ © 2026 — Дневна смяна при бай Пешо";
            }
            else
            {
                lblDayNight.Text = "🌙 Нощ";
                lblDayNight.ForeColor = Color.FromArgb(150, 178, 255);
                lblFooterCenter.Text =
                    "Кръчма „Жадният Програмист“ © 2026 — Нощна смяна — тефтерите се пълнят";
            }
        }

        private void ChkSound_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSound == null)
            {
                return;
            }

            SoundService.Enabled = chkSound.Checked;
            chkSound.Text = chkSound.Checked ? "🔊 Звук: Вкл" : "🔇 Звук: Изкл";
        }

        private void LoadProgrammers()
        {
            Safe(() =>
            {
                List<Programmer> programmers = DatabaseHelper.GetProgrammers();

                if (comboProgrammers != null)
                {
                    FillCombo(comboProgrammers, programmers);
                }

                if (comboOrderProgrammer != null)
                {
                    FillCombo(comboOrderProgrammer, programmers);
                }

            });
        }

        private static void FillCombo(ComboBox combo, List<Programmer> programmers)
        {
            if (combo == null)
            {
                return;
            }


            int selectedId = (combo.SelectedItem as Programmer)?.Id ?? -1;

            combo.BeginUpdate();
            combo.Items.Clear();

            if (programmers != null)
            {
                foreach (Programmer programmer in programmers)
                {
                    combo.Items.Add(programmer);
                }

            }

            combo.EndUpdate();

            if (selectedId != -1)
            {
                for (int i = 0; i < combo.Items.Count; i++)
                {
                    if (combo.Items[i] is Programmer p && p.Id == selectedId)
                    {
                        combo.SelectedIndex = i;
                        return;
                    }
                }
            }

            if (combo.Items.Count > 0 && combo.SelectedIndex < 0)
            {
                combo.SelectedIndex = 0;
            }

        }

        private void RefreshGrid()
        {
            Safe(() =>
            {
                if (gridOrders == null)
                {
                    return;
                }


                string search = txtSearch != null ? txtSearch.Text.Trim() : string.Empty;
                gridOrders.DataSource = DatabaseHelper.GetOrders(search);
                ConfigureGridColumns();
            });
        }

        private void ConfigureGridColumns()
        {
            if (gridOrders == null || gridOrders.Columns.Count == 0)
            {
                return;
            }


            if (gridOrders.Columns.Contains("Програмист"))
            {
                gridOrders.Columns["Програмист"].FillWeight = 100;
            }

            if (gridOrders.Columns.Contains("Любим език"))
            {
                gridOrders.Columns["Любим език"].FillWeight = 65;
            }

            if (gridOrders.Columns.Contains("Поръчка"))
            {
                gridOrders.Columns["Поръчка"].FillWeight = 110;
            }


            if (gridOrders.Columns.Contains("Цена"))
            {
                DataGridViewColumn priceColumn = gridOrders.Columns["Цена"];
                priceColumn.FillWeight = 55;
                priceColumn.DefaultCellStyle.Format = "0.00 'лв.'";
                priceColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                priceColumn.DefaultCellStyle.Padding = new Padding(0, 0, 14, 0);
            }

            if (gridOrders.Columns.Contains("Дата"))
            {
                DataGridViewColumn dateColumn = gridOrders.Columns["Дата"];
                dateColumn.FillWeight = 80;
                dateColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void RefreshStats()
        {
            Safe(() =>
            {
                var stats = DatabaseHelper.GetStatistics();

                if (lblStatProgrammersValue != null)
                {
                    lblStatProgrammersValue.Text = stats.ProgrammersCount.ToString();
                }

                if (lblStatTabsValue != null)
                {
                    lblStatTabsValue.Text = stats.ActiveTabsCount.ToString();
                }

                if (lblStatDebtValue != null)
                {
                    lblStatDebtValue.Text = stats.TotalDebt.ToString("0.00");
                }

                if (stats.TopDebtorName == null)
                {
                    if (lblStatTopName != null)
                    {
                        lblStatTopName.Text = "Няма длъжници";
                    }

                    if (lblStatTopSum != null)
                    {
                        lblStatTopSum.Text = 0.ToString("0.00");
                    }
                }
                else
                {
                    if (lblStatTopName != null)
                    {
                        lblStatTopName.Text = stats.TopDebtorName;
                    }

                    if (lblStatTopSum != null)
                    {
                        lblStatTopSum.Text = stats.TopDebtorSum.ToString("0.00");
                    }
                }

                if (lblConnection != null)
                {
                    lblConnection.Text = "●  Свързан с базата данни";
                    lblConnection.ForeColor = Theme.NeonGreen;
                }
            });
        }

        private void UpdateDebtForSelection()
        {
            if (comboProgrammers == null || comboProgrammers.SelectedItem is not Programmer programmer)
            {
                _targetDebt = 0;
                _displayedDebt = 0;
                _debtColorTarget = Theme.TextMuted;
                _debtColorCurrent = Theme.TextMuted;

                if (lblDebtAmount != null)
                {
                    lblDebtAmount.Text = "—";
                    lblDebtAmount.ForeColor = Theme.TextMuted;
                }

                if (lblDebtStatus != null)
                {
                    lblDebtStatus.Text = "Избери програмист от менюто горе.";
                    lblDebtStatus.ForeColor = Theme.TextMuted;
                }

                if (panelDebt != null)
                {
                    panelDebt.BorderColor = Theme.Border;
                }


                peshoCharacter?.ShowIdle();
                return;
            }

            Safe(() =>
            {
                _targetDebt = DatabaseHelper.GetTotalDebt(programmer.Id);

                var (statusColor, statusText) = GetDebtStatus(_targetDebt);
                _debtColorTarget = statusColor;

                if (lblDebtStatus != null)
                {
                    lblDebtStatus.Text = statusText;
                    lblDebtStatus.ForeColor = statusColor;
                }

                if (panelDebt != null)
                {
                    panelDebt.BorderColor = statusColor;
                }


                if (_debtTimer != null && !_debtTimer.Enabled)
                {
                    _debtTimer.Start();
                }


                peshoCharacter?.UpdateMood((decimal)_targetDebt);

                if (_targetDebt > 100)
                {
                    panelDebt?.Shake();
                    panelDebt?.FlashAlert();
                    SoundService.PlayBigDebt();
                }
            });
        }

        private static (Color Color, string Status) GetDebtStatus(double debt)
        {
            if (debt <= 0)
            {

                return (Theme.NeonGreen, "Чист е! Засега…");
            }

            if (debt < 20)
            {
                return (Theme.Amber, "Дългът е още в приличните граници.");
            }


            if (debt <= 50)
            {

                return (Theme.Red, "Внимание! Дългът расте застрашително!");
            }


            return (Theme.RedBright, "Бай Пешо вече гледа лошо!");
        }

        private void DebtTimer_Tick(object sender, EventArgs e)
        {
            _displayedDebt += (_targetDebt - _displayedDebt) * 0.18;
            _debtColorCurrent = Theme.Lerp(_debtColorCurrent, _debtColorTarget, 0.18f);

            bool amountReady = Math.Abs(_targetDebt - _displayedDebt) < 0.01;
            bool colorReady = ColorsAreClose(_debtColorCurrent, _debtColorTarget);

            if (amountReady && colorReady)
            {
                _displayedDebt = _targetDebt;
                _debtColorCurrent = _debtColorTarget;
                _debtTimer.Stop();
            }

            if (lblDebtAmount != null)
            {
                lblDebtAmount.Text = string.Format("{0:0.00} лв.", _displayedDebt);
                lblDebtAmount.ForeColor = _debtColorCurrent;
            }
        }

        private static bool ColorsAreClose(Color a, Color b)
        {
            return Math.Abs(a.R - b.R) < 4 &&
                   Math.Abs(a.G - b.G) < 4 &&
                   Math.Abs(a.B - b.B) < 4;
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            Opacity += 0.06;

            if (Opacity >= 1)
            {
                Opacity = 1;
                _fadeTimer.Stop();
            }
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            if (comboProgrammers == null || comboProgrammers.SelectedItem is not Programmer programmer)
            {
                SoundService.PlayError();
                MessageBox.Show("Първо избери програмист.", AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Safe(() =>
            {
                double debt = DatabaseHelper.GetTotalDebt(programmer.Id);

                if (debt <= 0)
                {
                    MessageBox.Show("Този човек вече е чист. Не го тормози.", AppTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DialogResult confirmation = MessageBox.Show(
                    string.Format("{0} дължи {1:0.00} лв.\n\nПлаща ли цялата сметка?",
                        programmer.Name, debt),
                    "Потвърждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmation != DialogResult.Yes)
                {
                    return;
                }


                DatabaseHelper.PayTab(programmer.Id);

                RefreshGrid();
                RefreshStats();
                UpdateDebtForSelection();

                peshoCharacter?.Celebrate();

                if (coinBurstControl != null && panelDebt != null)
                {
                    coinBurstControl.Play(RectangleToClient(panelDebt.RectangleToScreen(panelDebt.ClientRectangle)));
                }


                SoundService.PlayPayment();

                MessageBox.Show("Сметката е платена. Бай Пешо временно е спокоен.", AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void BtnAddOrder_Click(object sender, EventArgs e)
        {
            if (comboOrderProgrammer == null || comboOrderProgrammer.SelectedItem is not Programmer programmer)
            {
                SoundService.PlayError();
                MessageBox.Show("Първо избери програмист.", AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string itemName = txtOrderName != null ? txtOrderName.Text.Trim() : string.Empty;

            if (itemName.Length == 0)
            {
                SoundService.PlayError();
                MessageBox.Show("Въведи какво е поръчал човекът.", AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOrderName?.Focus();
                return;
            }

            string priceText = txtOrderPrice != null
                ? txtOrderPrice.Text.Trim().Replace(',', '.')
                : string.Empty;

            if (!double.TryParse(priceText, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out double price) || price <= 0)
            {
                SoundService.PlayError();
                MessageBox.Show("Цената трябва да е положително число.", AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOrderPrice?.Focus();
                return;
            }

            Safe(() =>
            {
                DatabaseHelper.AddTab(new TabItem
                {
                    ProgrammerId = programmer.Id,
                    ItemName = itemName,
                    Price = Math.Round(price, 2),
                    Date = DateTime.Now
                });

                txtOrderName?.Clear();
                txtOrderPrice?.Clear();

                RefreshGrid();
                RefreshStats();

                SelectProgrammerInMainCombo(programmer.Id);
                UpdateDebtForSelection();

                SoundService.PlayOrderAdded();

                decimal newDebt = (decimal)DatabaseHelper.GetTotalDebt(programmer.Id);
                peshoCharacter?.ReactToNewOrder(newDebt);

                MessageBox.Show("Поръчката е записана в тефтера! 🍺", AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void BtnAddProgrammer_Click(object sender, EventArgs e)
        {
            string name = txtProgName != null ? txtProgName.Text.Trim() : string.Empty;

            if (name.Length == 0)
            {
                SoundService.PlayError();
                MessageBox.Show("Името не може да е празно.", AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProgName?.Focus();
                return;
            }

            string language = txtProgLanguage != null ? txtProgLanguage.Text.Trim() : string.Empty;

            if (language.Length == 0)
            {
                SoundService.PlayError();
                MessageBox.Show("Любимият език не може да е празен.", AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProgLanguage?.Focus();
                return;
            }

            Safe(() =>
            {
                DatabaseHelper.AddProgrammer(new Programmer
                {
                    Name = name,
                    FavoriteLanguage = language
                });

                txtProgName?.Clear();
                txtProgLanguage?.Clear();

                LoadProgrammers();
                RefreshStats();

                SoundService.PlayOrderAdded();

                MessageBox.Show("Програмистът е добавен. Да му е сладка бирата!", AppTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }

        private void TxtOrderPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool isDigit = char.IsDigit(e.KeyChar);
            bool isControlKey = char.IsControl(e.KeyChar);
            bool isSeparator = e.KeyChar == '.' || e.KeyChar == ',';

            if (isSeparator)
            {
                TextBox textBox = sender as TextBox;

                if (textBox != null &&
                    (textBox.Text.Contains(".") || textBox.Text.Contains(",")))
                {
                    e.Handled = true;
                    return;
                }
            }

            if (!isDigit && !isSeparator && !isControlKey)
            {
                e.Handled = true;
            }

        }

        private void SelectProgrammerInMainCombo(int programmerId)
        {
            if (comboProgrammers == null)
            {
                return;
            }


            for (int i = 0; i < comboProgrammers.Items.Count; i++)
            {
                if (comboProgrammers.Items[i] is Programmer p && p.Id == programmerId)
                {
                    comboProgrammers.SelectedIndex = i;
                    return;
                }
            }
        }

        private void TrySetDayMode(Control control, bool isDay)
        {
            if (control == null)
            {
                return;
            }


            try
            {
                PropertyInfo property = control.GetType().GetProperty("DayMode");

                if (property != null && property.CanWrite)
                {
                    property.SetValue(control, isDay);
                }


                control.Invalidate();
            }
            catch
            {
            }
        }

        private void Safe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (lblConnection != null)
                {
                    lblConnection.Text = "●  Проблем с базата данни";
                    lblConnection.ForeColor = Theme.Red;
                }

                SoundService.PlayError();

                MessageBox.Show("Възникна грешка:\n" + ex.Message, "Грешка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}