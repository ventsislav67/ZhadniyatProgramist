using System;
using System.Windows.Forms;

namespace ZhadniyatProgramist
{
    /// <summary>
    /// Входна точка на приложението „Кръчма Жадният Програмист“.
    /// Ред на изпълнение: база данни -> loading екран -> главен прозорец.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. Подготвяме базата данни.
            //    При първо стартиране се създава файлът krachma.db,
            //    таблиците и примерните данни (виж DatabaseHelper.cs).
            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Неуспешна инициализация на базата данни:\n" + ex.Message,
                    "Кръчма „Жадният Програмист“",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return; // без база кръчмата не работи
            }

            // 2. Показваме loading екрана с анимирания progress bar…
            using (var loadingForm = new LoadingForm())
            {
                loadingForm.ShowDialog();
            }

            // 3. …и стартираме главния прозорец (появява се с плавен fade-in).
            Application.Run(new MainForm());
        }
    }
}
