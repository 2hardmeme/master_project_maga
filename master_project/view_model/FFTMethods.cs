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
        public static (double periodInSeconds, double periodInPoints) FindPeriod(double[] signal, double samplingFrequency)
        {
            if (signal == null || signal.Length == 0)
                throw new ArgumentException("Сигнал не може бути порожнім");

            int N = signal.Length;
            double[] real = new double[N];
            double[] imag = new double[N];

            for (int k = 0; k < N; k++)
            {
                real[k] = 0;
                imag[k] = 0;
                for (int n = 0; n < N; n++)
                {
                    double angle = 2 * Math.PI * k * n / N;
                    real[k] += signal[n] * Math.Cos(angle);
                    imag[k] += -signal[n] * Math.Sin(angle);
                }
            }

            double[] magnitudes = new double[N / 2];
            for (int i = 0; i < N / 2; i++)
            {
                magnitudes[i] = Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]);
            }

            int peakIndex = Array.IndexOf(magnitudes, magnitudes.Max());

            if (peakIndex == 0)
                throw new Exception("Не вдалося знайти домінантну частоту");

            double dominantFrequency = (double)peakIndex * samplingFrequency / N;
            double periodInSeconds = 1 / dominantFrequency;
            double periodInPoints = (double)N / peakIndex;

            return (periodInSeconds, periodInPoints);
        }
    }
}