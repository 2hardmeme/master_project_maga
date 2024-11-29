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
        private double[] harmonicSignal; 
        private double[] allHarmonicsSum;
        private double[] squaredDeviations;
        private double SqrSum; 
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

            stopwatch1.Stop(); 
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
            transitXValues = new double[xValues.Length];
            Array.Copy(xValues, transitXValues, xValues.Length);
            return transitXValues;
        }

        private void CalculateAandBcoefficients()
        {
            a1coefficients = new double[columnSums.GetLength(1) / 2];
            b1coefficients = new double[columnSums.GetLength(1) / 2];

            for (int j = 0; j < columnSums.GetLength(1); j += 2)
            {
                int harmonicNumber = j / 2 + 1;
                a1coefficients[harmonicNumber - 1] = mysteriousThing * columnSums[0, j];
                b1coefficients[harmonicNumber - 1] = mysteriousThing * columnSums[0, j + 1];
            }
        }

        private void DisplayCoefficients()
        {
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();

            if (a1coefficients != null && b1coefficients != null)
            {
                for (int i = 0; i < a1coefficients.Length; i++)
                {
                    dataGridView2.Rows.Add(a1coefficients[i]);
                    dataGridView3.Rows.Add(b1coefficients[i]);
                }
            }
        }

        private void CalculateAmplitudes()
        {
            Amplitudes = new double[a1coefficients.Length];

            for (int i = 0; i < a1coefficients.Length; i++)
            {
                Amplitudes[i] = Math.Sqrt(Math.Pow(a1coefficients[i], 2) + Math.Pow(b1coefficients[i], 2));
            }
        }

        private void CalculatePercentages()
        {
            percentages = new double[Amplitudes.Length];

            percentages[0] = 100;

            for (int i = 1; i < Amplitudes.Length; i++)
            {
                percentages[i] = (Amplitudes[i] / Amplitudes[0]) * 100;
            }
        }

        private void DisplayPercentages()
        {
            dataGridView4.Rows.Clear();

            if (percentages != null)
            {
                for (int i = 0; i < percentages.Length; i++)
                {
                    dataGridView4.Rows.Add(percentages[i]);
                }
            }
        }

        private void UpdateArraysFromDataGridViews()
        {
            newA1coefficients = new double[dataGridView2.Rows.Count];
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                double value;
                if (double.TryParse(dataGridView2.Rows[i].Cells[0].Value?.ToString(), out value))
                {
                    newA1coefficients[i] = value;
                }
            }
            newB1coefficients = new double[dataGridView3.Rows.Count];
            for (int i = 0; i < dataGridView3.Rows.Count; i++)
            {
                double value;
                if (double.TryParse(dataGridView3.Rows[i].Cells[0].Value?.ToString(), out value))
                {
                    newB1coefficients[i] = value;
                }
            }
            newPercentages = new double[dataGridView4.Rows.Count];
            for (int i = 0; i < dataGridView4.Rows.Count; i++)
            {
                double value;
                if (double.TryParse(dataGridView4.Rows[i].Cells[0].Value?.ToString(), out value))
                {
                    newPercentages[i] = value;
                }
            }
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
            int rowCount = harmonics.Length;                  
            int maxElements = harmonics[0].Length;              
            harmonicSums = new double[rowCount][];           
            for (int i = 0; i < rowCount; i++)
            {
                harmonicSums[i] = new double[maxElements - 1];  
                double cumulativeSum = firstA0;                
                if (maxElements > 1)                   
                {
                    cumulativeSum += harmonics[i][0] + harmonics[i][1];
                    harmonicSums[i][0] = cumulativeSum;          
                }
                for (int j = 2; j < maxElements; j++)
                {
                    cumulativeSum += harmonics[i][j];         
                    harmonicSums[i][j - 1] = cumulativeSum;      
                }
            }
        }

        private void FillAllHarmonicsSum()
        {
            allHarmonicsSum = new double[harmonicSums.Length];
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
            for (int i = 0; i < squaredDeviations.Length; i++)
            {
                SqrSum += squaredDeviations[i];
            }
            double rootOfSum = Math.Pow(SqrSum, 0.5);
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
            double errorInPercent = error * 100;
            textBox1.Text = errorInPercent.ToString("F2") + " %";
            textBox2.Text = standardDeviation.ToString("F2");
        }

    }
}