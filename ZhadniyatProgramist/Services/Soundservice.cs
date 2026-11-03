using System;
using System.IO;
using System.Media;

namespace ZhadniyatProgramist.Services
{
    /// <summary>
    /// Леки звукови ефекти за кръчмата.
    ///
    ///  - Ако в Assets/Sounds има .wav файл със съответното име,
    ///    той се използва (order.wav, pay.wav, error.wav, bigdebt.wav, select.wav).
    ///  - Ако файл липсва – fallback към SystemSounds (никога Console.Beep,
    ///    защото той блокира UI нишката).
    ///  - Всичко е в try/catch: липсващ файл или звукова карта НЕ крашват приложението.
    ///  - Звукът може да се изключи от checkbox-а в статус бара (Enabled).
    /// </summary>
    public static class SoundService
    {
        /// <summary>Глобален превключвател. По подразбиране звукът е включен.</summary>
        public static bool Enabled { get; set; } = true;

        private static readonly string SoundsFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds");

        public static void PlayOrderAdded() => Play("order.wav", SystemSounds.Asterisk);
        public static void PlayPayment()    => Play("pay.wav", SystemSounds.Exclamation);
        public static void PlayError()      => Play("error.wav", SystemSounds.Hand);
        public static void PlayBigDebt()    => Play("bigdebt.wav", SystemSounds.Exclamation);
        public static void PlaySelect()     => Play("select.wav", SystemSounds.Beep);

        /// <summary>Пуска .wav от Assets/Sounds или системен звук като резервен вариант.</summary>
        private static void Play(string fileName, SystemSound fallback)
        {
            if (!Enabled) return;

            try
            {
                string path = Path.Combine(SoundsFolder, fileName);
                if (File.Exists(path))
                {
                    // SoundPlayer.Play() е асинхронен – не блокира интерфейса.
                    using (var player = new SoundPlayer(path))
                    {
                        player.Play();
                    }
                    return;
                }

                fallback?.Play();
            }
            catch
            {
                // Звукът е украса – никаква грешка тук не бива да пречи на работата.
            }
        }
    }
}