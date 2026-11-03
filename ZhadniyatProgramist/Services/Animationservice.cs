using System;

namespace ZhadniyatProgramist.Services
{
    /// <summary>
    /// Помощни математически функции за плавни анимации.
    /// Ползват се от AnimatedNumberLabel, ModernButton, ModernPanel и др.
    /// </summary>
    public static class AnimationService
    {
        /// <summary>Линейна интерполация между две числа (t = 0..1).</summary>
        public static double Lerp(double from, double to, double t)
        {
            t = Clamp01(t);
            return from + (to - from) * t;
        }

        /// <summary>Плавно "изтичане" – бързо в началото, меко накрая.</summary>
        public static double EaseOutCubic(double t)
        {
            t = Clamp01(t);
            double inv = 1.0 - t;
            return 1.0 - inv * inv * inv;
        }

        /// <summary>Синусоидално пулсиране 0..1 (за неонови рамки).</summary>
        public static double Pulse(double phase)
        {
            return (Math.Sin(phase) + 1.0) * 0.5;
        }

        /// <summary>Ограничава стойност в интервала [0..1].</summary>
        public static double Clamp01(double value)
        {
            if (value < 0) return 0;
            if (value > 1) return 1;
            return value;
        }
    }
}