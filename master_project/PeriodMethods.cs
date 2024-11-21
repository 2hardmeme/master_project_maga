using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace master_project
{
    public class PeriodMethods
    {
        public static double FindPeriod(double[] signal, double samplingFrequency)
        {
            // Перевірка вхідного сигналу
            if (signal == null || signal.Length == 0)
                throw new ArgumentException("Сигнал не може бути порожнім");

            // Перетворення сигналу в масив комплексних чисел
            Complex[] fft = new Complex[signal.Length];
            for (int i = 0; i < signal.Length; i++)
            {
                fft[i] = new Complex(signal[i], 0);
            }

            // Виконання швидкого перетворення Фур'є (FFT)
            Fourier.Forward(fft, FourierOptions.Default);

            // Обчислення спектру (модулів комплексних чисел)
            double[] magnitudes = fft.Select(c => c.Magnitude).ToArray();

            // Знаходження частоти з найвищою амплітудою
            int peakIndex = Array.IndexOf(magnitudes, magnitudes.Max());
            double dominantFrequency = (double)peakIndex * samplingFrequency / signal.Length;

            // Період = 1 / частота
            return 1 / dominantFrequency;
        }
    }
}