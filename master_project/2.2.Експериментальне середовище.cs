using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace master_project
{
    public partial class Form7 : Form
    {
        private Stopwatch stopwatch1;
        private Stopwatch stopwatch2;
        private double period;
        private string[] yDots;
        private string[] xDots;

        private double periodDouble;
        private double[] AveragesSum;
        private string[] tDots;
        private double[] xValues;
        private double omega;
        private double m;
        private double[,] harmonicCoefficients;
        private double a0;
        private double[,] columnSums;
        private double xValuesSum;
        private double[] transitXValues;
        private double[] a1coefficients;
        private double[] b1coefficients;
        private double[] newA1coefficients;
        private double[] newB1coefficients;
        private double mysteriousThing;
        private double[] Amplitudes;
        private double[] percentages;
        private double[] newPercentages;
        private double firstA0;
        private double[][] harmonics;
        private double[][] harmonicSums;
        private double[] harmonicSignal; // Оголошення нового одновимірного масиву
        private double[] allHarmonicsSum;
        private double[] squaredDeviations;
        private double SqrSum; // Declared in the class body
        private double rootOfSum;
        private double standardDeviation;
        private double error;

        private double firstGroupTime;
        private double secondGroupTime;
        private double totalElapsedMilliseconds;

        public Form7(double period, string[] yDots, string[] xDots)
        {
            InitializeComponent();
            stopwatch1 = new Stopwatch();
            stopwatch2 = new Stopwatch();
            this.period = period;
            this.yDots = yDots;
            this.xDots = xDots;

            button6.Visible = false;

            periodDouble = (double)period;
            InitializeTDots();
            omega = (2 * Math.PI) / periodDouble;
            m = Math.Truncate(periodDouble / 2);
            this.mysteriousThing = 2 / periodDouble;
            a1coefficients = new double[(int)m];
            b1coefficients = new double[(int)m];
            this.firstA0 = a0 / 2;

            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            stopwatch1.Start();  // Start timing

            double[][] Averages = ConvertToAverages(yDots, periodDouble);
            double[] AveragesSum = CalculateRowAverages(Averages);
            progressBar1.Value = progressBar1.Maximum;

            CalculateXValues();
            CalculateHarmonicCoefficients((int)m);
            CalculateColumnSums();
            CalculateA0();
            progressBar2.Value = progressBar2.Maximum;

            TransitXValues();
            CalculateAandBcoefficients();
            DisplayCoefficients();
            CalculateAmplitudes();
            UpdateArraysFromDataGridViews();
            CalculatePercentages();
            DisplayPercentages();
            progressBar3.Value = progressBar3.Maximum;
            button6.Visible = true;

            stopwatch1.Stop();  // Stop timing
            firstGroupTime = stopwatch1.ElapsedMilliseconds;
        }

        private void InitializeTDots()
        {
            tDots = new string[(int)periodDouble];
            CopyArray(xDots, tDots);
        }

        private double[][] ConvertToAverages(string[] yDots, double periodDouble)
        {
            int rowCount = (int)Math.Ceiling((double)yDots.Length / periodDouble);
            double[][] Averages = new double[rowCount][];
            int currentIndex = 0;

            for (int j = 0; j < rowCount; j++)
            {
                int currentColumnCount = (int)Math.Min(periodDouble, yDots.Length - currentIndex);
                Averages[j] = new double[currentColumnCount];

                for (int i = 0; i < currentColumnCount; i++)
                {
                    if (double.TryParse(yDots[currentIndex], out double value))
                    {
                        Averages[j][i] = value;
                    }
                    currentIndex++;
                }
            }
            return Averages;
        }

        private double[] CalculateRowAverages(double[][] Averages)
        {
            int rowCount = Averages.Length;
            int maxColumnCount = Averages.Max(row => row.Length);
            AveragesSum = new double[maxColumnCount];

            for (int j = 0; j < maxColumnCount; j++)
            {
                double sum = 0;
                int count = 0;

                for (int i = 0; i < rowCount; i++)
                {
                    if (j < Averages[i].Length)
                    {
                        sum += Averages[i][j];
                        count++;
                    }
                }

                AveragesSum[j] = count > 0 ? sum / count : 0;
            }
            return AveragesSum;
        }

        private void CopyArray(string[] xDots, string[] tDots)
        {
            if (xDots == null || xDots.Length == 0 || tDots == null || tDots.Length == 0)
            {
                return;
            }

            int copyLength = Math.Min(xDots.Length, (int)periodDouble);
            Array.Copy(xDots, tDots, copyLength);
        }

        private void CalculateXValues()
        {
            xValues = new double[tDots.Length];

            for (int i = 0; i < tDots.Length; i++)
            {
                xValues[i] = omega * (Convert.ToDouble(tDots[i]) - 1);
            }
        }

        private double[,] CalculateHarmonicCoefficients(int m)
        {
            harmonicCoefficients = new double[AveragesSum.Length, 2 * m];

            for (int i = 0; i < AveragesSum.Length; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    double cosineCoefficient = AveragesSum[i] * Math.Cos(xValues[i] * j);
                    double sineCoefficient = AveragesSum[i] * Math.Sin(xValues[i] * j);

                    harmonicCoefficients[i, (j - 1) * 2] = cosineCoefficient;
                    harmonicCoefficients[i, (j - 1) * 2 + 1] = sineCoefficient;
                }
            }

            return harmonicCoefficients;
        }

        private void CalculateColumnSums()
        {
            columnSums = new double[1, harmonicCoefficients.GetLength(1)];

            for (int j = 0; j < harmonicCoefficients.GetLength(1); j++)
            {
                double sum = 0;
                for (int i = 0; i < harmonicCoefficients.GetLength(0); i++)
                {
                    sum += harmonicCoefficients[i, j];
                }
                columnSums[0, j] = sum;
            }

            xValuesSum = xValues.Sum();
        }

        private double CalculateSumOfAverages()
        {
            double sum = 0;
            foreach (double value in AveragesSum)
            {
                sum += value;
            }
            return sum;
        }

        private void CalculateA0()
        {
            double sumOfAverages = CalculateSumOfAverages();
            a0 = (2 / periodDouble) * sumOfAverages;
        }

        private double[] TransitXValues()
        {
            // Ініціалізуємо новий масив для транзитних значень xValues
            transitXValues = new double[xValues.Length];

            // Переписуємо значення з xValues в transitXValues
            Array.Copy(xValues, transitXValues, xValues.Length);

            return transitXValues;
        }

        private void CalculateAandBcoefficients()
        {
            // Ініціалізуємо масиви амплітуд
            a1coefficients = new double[columnSums.GetLength(1) / 2];
            b1coefficients = new double[columnSums.GetLength(1) / 2];

            // Обчислюємо амплітуди для кожного елемента масиву columnSums
            for (int j = 0; j < columnSums.GetLength(1); j += 2)
            {
                // Обчислюємо значення амплітуд за новою формулою
                int harmonicNumber = j / 2 + 1;
                a1coefficients[harmonicNumber - 1] = mysteriousThing * columnSums[0, j];
                b1coefficients[harmonicNumber - 1] = mysteriousThing * columnSums[0, j + 1];
            }
        }

        private void DisplayCoefficients()
        {
            // Очищаємо дані відповідних стовпців dataGridView2 та dataGridView3 перед оновленням
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();

            // Перевіряємо, чи існують коефіцієнти для відображення
            if (a1coefficients != null && b1coefficients != null)
            {
                // Оновлюємо значення відповідних стовпців dataGridView2 та dataGridView3
                for (int i = 0; i < a1coefficients.Length; i++)
                {
                    // Додаємо значення коефіцієнтів до відповідних рядків dataGridView2 та dataGridView3
                    dataGridView2.Rows.Add(a1coefficients[i]);
                    dataGridView3.Rows.Add(b1coefficients[i]);
                }
            }
        }

        private void CalculateAmplitudes()
        {
            // Ініціалізуємо масив амплітуд
            Amplitudes = new double[a1coefficients.Length];

            // Обчислюємо амплітуди для кожного індексу
            for (int i = 0; i < a1coefficients.Length; i++)
            {
                // Обчислюємо квадрат суми коефіцієнтів за формулою і беремо його квадратний корінь
                Amplitudes[i] = Math.Sqrt(Math.Pow(a1coefficients[i], 2) + Math.Pow(b1coefficients[i], 2));
            }
        }

        private void CalculatePercentages()
        {
            // Ініціалізуємо масив для відсотків
            percentages = new double[Amplitudes.Length];

            // Перший елемент завжди 100%
            percentages[0] = 100;

            // Обчислюємо відсотки для кожного елемента, крім першого
            for (int i = 1; i < Amplitudes.Length; i++)
            {
                percentages[i] = (Amplitudes[i] / Amplitudes[0]) * 100;
            }
        }

        private void DisplayPercentages()
        {
            // Очищаємо дані відповідного стовпця dataGridView4 перед оновленням
            dataGridView4.Rows.Clear();

            // Перевіряємо, чи існують відсотки для відображення
            if (percentages != null)
            {
                // Оновлюємо значення відповідного стовпця dataGridView4
                for (int i = 0; i < percentages.Length; i++)
                {
                    // Додаємо значення відсотків до відповідного рядка dataGridView4
                    dataGridView4.Rows.Add(percentages[i]);
                }
            }
        }

        private void UpdateArraysFromDataGridViews()
        {
            // Оновлюємо масив newA1coefficients з dataGridView2
            newA1coefficients = new double[dataGridView2.Rows.Count];
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                double value;
                if (double.TryParse(dataGridView2.Rows[i].Cells[0].Value?.ToString(), out value))
                {
                    newA1coefficients[i] = value;
                }
            }

            // Оновлюємо масив newB1coefficients з dataGridView3
            newB1coefficients = new double[dataGridView3.Rows.Count];
            for (int i = 0; i < dataGridView3.Rows.Count; i++)
            {
                double value;
                if (double.TryParse(dataGridView3.Rows[i].Cells[0].Value?.ToString(), out value))
                {
                    newB1coefficients[i] = value;
                }
            }

            // Оновлюємо масив newPercentages з dataGridView4
            newPercentages = new double[dataGridView4.Rows.Count];
            for (int i = 0; i < dataGridView4.Rows.Count; i++)
            {
                double value;
                if (double.TryParse(dataGridView4.Rows[i].Cells[0].Value?.ToString(), out value))
                {
                    newPercentages[i] = value;
                }
            }

            // Видаляємо елементи зі значенням строго 0
            RemoveZeroElements();
        }

        private void RemoveZeroElements()
        {
            newA1coefficients = newA1coefficients.Where(value => value != 0).ToArray();
            newB1coefficients = newB1coefficients.Where(value => value != 0).ToArray();
            newPercentages = newPercentages.Where(value => value != 0).ToArray();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int selectedIndex = dataGridView4.SelectedCells[0].RowIndex;

            if (selectedIndex >= 0 && selectedIndex < dataGridView2.Rows.Count)
            {
                dataGridView2.Rows.RemoveAt(selectedIndex);
            }

            if (selectedIndex >= 0 && selectedIndex < dataGridView3.Rows.Count)
            {
                dataGridView3.Rows.RemoveAt(selectedIndex);
            }

            if (selectedIndex >= 0 && selectedIndex < dataGridView4.Rows.Count)
            {
                dataGridView4.Rows.RemoveAt(selectedIndex);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            UpdateArraysFromDataGridViews();

            // Повідомлення про успішне оновлення масивів
            MessageBox.Show("Масиви даних з DataGridView були успішно оновлені.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DisplayPercentages();
            DisplayCoefficients();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            stopwatch2.Start();  // Start timing

            CalculateHarmonics();
            CalculateHarmonicSums(firstA0);
            progressBar4.Value = progressBar4.Maximum;

            FillAllHarmonicsSum();
            FillHarmonicSignal();
            CalculateSquaredDeviations();
            CalculateRootOfSum();
            CalculateStandardDeviation();
            CalculateError();

            DisplayErrorAndDeviation();
            stopwatch2.Stop();  // Stop timing
            secondGroupTime = stopwatch2.ElapsedMilliseconds;
            totalElapsedMilliseconds = firstGroupTime + secondGroupTime;
            textBox3.Text = totalElapsedMilliseconds + " мс";
        }

        private void CalculateHarmonics()
        {
            harmonics = new double[transitXValues.Length][]; // Ініціалізуємо розмір масиву гармонік

            for (int i = 0; i < transitXValues.Length; i++)
            {
                harmonics[i] = new double[newA1coefficients.Length]; // Ініціалізуємо кожний рядок

                // Обчислюємо кожен елемент гармоніки
                for (int j = 0; j < newA1coefficients.Length; j++)
                {
                    harmonics[i][j] = (newA1coefficients[j] * Math.Cos(transitXValues[i] * (j + 1))) +
                                      (newB1coefficients[j] * Math.Sin(transitXValues[i] * (j + 1)));
                }
            }
        }

        private void CalculateHarmonicSums(double firstA0)
        {
            int rowCount = harmonics.Length;                     // Кількість рядків у масиві harmonics
            int maxElements = harmonics[0].Length;               // Кількість стовпчиків у масиві harmonics
            harmonicSums = new double[rowCount][];               // Ініціалізуємо багатовимірний масив для зберігання сум гармонік

            // Проходимося по кожному рядку масиву harmonics
            for (int i = 0; i < rowCount; i++)
            {
                harmonicSums[i] = new double[maxElements - 1];   // Ініціалізуємо кожний рядок у масиві harmonicSums на один стовпчик менше
                double cumulativeSum = firstA0;                  // Початкове значення - firstA0

                // Додаємо перші два елементи у суму для першої ітерації
                if (maxElements > 1)                             // Переконуємося, що є принаймні два елементи
                {
                    cumulativeSum += harmonics[i][0] + harmonics[i][1];
                    harmonicSums[i][0] = cumulativeSum;          // Записуємо суму в перший стовпчик harmonicSums
                }

                // Додаємо наступні елементи, починаючи з третього
                for (int j = 2; j < maxElements; j++)
                {
                    cumulativeSum += harmonics[i][j];            // Додаємо поточний елемент harmonics до суми
                    harmonicSums[i][j - 1] = cumulativeSum;      // Записуємо суму в поточний стовпчик harmonicSums
                }
            }
        }

        private void FillAllHarmonicsSum()
        {
            // Ініціалізуємо allHarmonicsSum за розміром кількості рядків у harmonicSums
            allHarmonicsSum = new double[harmonicSums.Length];

            // Записуємо значення останнього стовпчика harmonicSums в allHarmonicsSum
            for (int i = 0; i < harmonicSums.Length; i++)
            {
                allHarmonicsSum[i] = harmonicSums[i][harmonicSums[i].Length - 1];
            }
        }

        private void FillHarmonicSignal()
        {
            int targetLength = yDots.Length;
            harmonicSignal = new double[targetLength];

            for (int i = 0; i < targetLength; i++)
            {
                harmonicSignal[i] = allHarmonicsSum[i % allHarmonicsSum.Length];
            }
        }

        private void CalculateSquaredDeviations()
        {
            int length = Math.Min(harmonicSignal.Length, yDots.Length);
            squaredDeviations = new double[length];

            for (int i = 0; i < length; i++)
            {
                double yDotValue = double.Parse(yDots[i]); // Convert yDots element to double
                double deviation = harmonicSignal[i] - yDotValue;
                squaredDeviations[i] = deviation * deviation;
            }
        }

        public void CalculateRootOfSum()
        {
            SqrSum = 0;

            // Підсумовуємо всі елементи масиву squaredDeviations
            for (int i = 0; i < squaredDeviations.Length; i++)
            {
                SqrSum += squaredDeviations[i];
            }

            // Обчислюємо квадратний корінь з суми
            double rootOfSum = Math.Pow(SqrSum, 0.5); // або Math.Sqrt(SqrSum)

            // Зберігаємо результат у змінній класу, якщо потрібно
            this.rootOfSum = rootOfSum;
        }

        public void CalculateStandardDeviation()
        {
            standardDeviation = rootOfSum / yDots.Length;
        }

        public void CalculateError()
        {
            double maxHarmonic = harmonicSignal.Max();
            error = standardDeviation / maxHarmonic;
        }

        private void DisplayErrorAndDeviation()
        {
            // Переведення error у відсотки
            double errorInPercent = error * 100;
            textBox1.Text = errorInPercent.ToString("F2") + " %";

            // Переведення standardDeviation у відсотки
            double standardDeviationInPercent = standardDeviation * 100;
            textBox2.Text = standardDeviationInPercent.ToString("F2") + " %";
        }
    }
}