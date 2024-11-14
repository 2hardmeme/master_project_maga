using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace master_project
{
    public partial class Form7 : Form
    {
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
        private double mysteriousThing;
        private double[] Amplitudes;
        private double[] percentages;
        private double[] newPercentages;

        public Form7(double period, string[] yDots, string[] xDots)
        {
            InitializeComponent();
            this.period = period;
            this.yDots = yDots;
            this.xDots = xDots;

            button6.Visible = false;

            periodDouble = (double)period;
            InitializeTDots();
            omega = (2 * Math.PI) / periodDouble;
            m = Math.Truncate(periodDouble / 2);
        }

        private void button3_Click(object sender, EventArgs e)
        {
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
            CalculateAmplitudes();
            UpdateArraysFromDataGridView();
            CalculatePercentages();
            DisplayPercentages();
            progressBar3.Value = progressBar3.Maximum;
            button6.Visible = true;
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

        private void button2_Click(object sender, EventArgs e)
        {
            int selectedIndex = dataGridView4.SelectedCells[0].RowIndex;

            if (selectedIndex >= 0 && selectedIndex < dataGridView4.Rows.Count)
            {
                dataGridView4.Rows.RemoveAt(selectedIndex);
            }
        }

        private void UpdateArraysFromDataGridView()
        {
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
            newPercentages = newPercentages.Where(value => value != 0).ToArray();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            UpdateArraysFromDataGridView();
            MessageBox.Show("Масиви даних з DataGridView були успішно оновлені.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DisplayPercentages();
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }
    }
}