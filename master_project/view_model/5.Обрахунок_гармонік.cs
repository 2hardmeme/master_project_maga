using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace master_project
{
    public partial class Form5 : Form
    {
        private double[] newA1coefficients;
        private double[] newB1coefficients;
        double[] transitXValues;
        private double a0;
        private string[] tDots;
        private string[] yDots;
        private string fourierFormula;
        private double[] AveragesSum;
        private double[] DYDots;
        private double[] DXDots;

        private double[][] harmonics; // Масив для зберігання гармонік
        private double[][] harmonicSums; // Масив для зберігання сум гармонік

        public Form5(double[] newA1coefficients, double[] newB1coefficients, double[] transitXValues, double a0, string[] tDots, string[] yDots, string fourierFormula, double[] AveragesSum, double[] DYDots, double[] DXDots)
        {
            InitializeComponent();

            // Приймаємо значення в конструкторі і зберігаємо їх у відповідних полях класу
            this.newA1coefficients = newA1coefficients;
            this.newB1coefficients = newB1coefficients;
            this.transitXValues = transitXValues;
            this.a0 = a0;
            this.tDots = tDots;
            this.yDots = yDots;
            this.fourierFormula = fourierFormula;
            this.AveragesSum = AveragesSum;
            double firstA0 = a0 / 2;

            this.DYDots = DYDots;
            this.DXDots = DXDots;

            CalculateHarmonics(); // Виклик методу для обчислення гармонік
            DisplayHarmonics();
            CalculateHarmonicSums(firstA0);

            PlotHarmonics();

            SetupChartAxisFormatting();
            EnableZooming();
            
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

        private void DisplayHarmonics()
        {
            // Очищаємо будь-які існуючі дані у DataGridView
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            // Додаємо стовпці відповідно до кількості стовпчиків у першому рядку гармонік
            for (int i = 0; i < harmonics[0].Length; i++)
            {
                dataGridView1.Columns.Add($"Column{i + 1}", $"Harmonic {i + 1}");
            }

            // Додаємо рядки та заповнюємо дані з масиву гармонік
            for (int i = 0; i < harmonics.Length; i++)
            {
                DataGridViewRow row = new DataGridViewRow();

                // Додаємо значення до рядка
                for (int j = 0; j < harmonics[i].Length; j++)
                {
                    row.Cells.Add(new DataGridViewTextBoxCell { Value = harmonics[i][j] });
                }

                // Додаємо рядок до DataGridView
                dataGridView1.Rows.Add(row);
            }
        }

        private void PlotHarmonics()
        {
            // Очищуємо попередні серії з chart1
            chart1.Series.Clear();

            // Перевіряємо, чи кількість значень у tDots відповідає кількості рядків у harmonics
            if (tDots.Length != harmonics.Length)
            {
                MessageBox.Show("Кількість значень у tDots не відповідає кількості рядків у harmonics!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Графік для сигналу
            Series signalSeries = new Series("Сигнал")
            {
                ChartType = SeriesChartType.Line, // Встановлюємо тип графіка
                BorderWidth = 2                   // Встановлюємо товщину лінії
            };

            // Додаємо точки для графіка сигналу
            for (int i = 0; i < tDots.Length; i++)
            {
                // Конвертуємо tDots[i] у double
                double xValue = double.Parse(tDots[i]);

                // Отримуємо значення y з масиву AveragesSum
                double yValue = AveragesSum[i];

                // Додаємо точку на графік
                signalSeries.Points.AddXY(xValue, yValue);
            }

            // Додаємо графік сигналу до chart1
            chart1.Series.Add(signalSeries);

            for (int colIndex = 0; colIndex < harmonics[0].Length; colIndex++)
            {
                // Формуємо назву графіка у форматі "1+n", де n - останнє число
                string seriesName = $"1+{colIndex + 1}";

                // Створюємо нову серію для кожного графіка
                Series series = new Series(seriesName)
                {
                    ChartType = SeriesChartType.Line, // Встановлюємо тип графіка
                    BorderWidth = 2                   // Встановлюємо товщину лінії
                };

                // Додаємо точки для цієї серії
                for (int rowIndex = 0; rowIndex < tDots.Length; rowIndex++)
                {
                    // Конвертуємо tDots[rowIndex] у double
                    double xValue = double.Parse(tDots[rowIndex]);

                    // Отримуємо значення y з масиву harmonics
                    double yValue = harmonics[rowIndex][colIndex];

                    // Додаємо точку на графік
                    series.Points.AddXY(xValue, yValue);
                }

                // Додаємо серію до chart1
                chart1.Series.Add(series);
            }
        }

        private void EnableZooming()
        {
            // Увімкнення масштабування на осі X
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;

            // Увімкнення масштабування на осі Y
            chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;

            // Налаштування кнопки миші для зуму (наприклад, колесо миші)
            chart1.MouseWheel += Chart1_MouseWheel;
        }

        private void SetupChartAxisFormatting()
        {
            var chartArea = chart1.ChartAreas[0];

            // Встановлюємо формат чисел на осях
            chartArea.AxisX.LabelStyle.Format = "F1"; // 1 знак після коми
            chartArea.AxisY.LabelStyle.Format = "F1"; // 1 знак після коми
        }

        private void Chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;
            var chartArea = chart.ChartAreas[0];
            double zoomFactor = 0.1; // Крок масштабування (10%)

            if (e.Delta < 0) // Прокрутка вниз
            {
                chartArea.AxisX.ScaleView.ZoomReset(); // Скидання масштабу по осі X
                chartArea.AxisY.ScaleView.ZoomReset(); // Скидання масштабу по осі Y
            }
            else if (e.Delta > 0) // Прокрутка вгору
            {
                try
                {
                    // Поточні межі вікна масштабування
                    double xMin = chartArea.AxisX.ScaleView.ViewMinimum;
                    double xMax = chartArea.AxisX.ScaleView.ViewMaximum;
                    double yMin = chartArea.AxisY.ScaleView.ViewMinimum;
                    double yMax = chartArea.AxisY.ScaleView.ViewMaximum;

                    // Розрахунок нових меж масштабування
                    double xZoom = (xMax - xMin) * zoomFactor;
                    double yZoom = (yMax - yMin) * zoomFactor;

                    // Нові межі масштабування
                    chartArea.AxisX.ScaleView.Zoom(xMin + xZoom, xMax - xZoom);
                    chartArea.AxisY.ScaleView.Zoom(yMin + yZoom, yMax - yZoom);
                }
                catch
                {
                    // Ігнорування помилок (наприклад, коли масштабування виходить за межі допустимих значень)
                }
            }
        }

        private void результатиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form6 form6 = new Form6(yDots, harmonicSums, fourierFormula, DXDots, DYDots);
            form6.Show();
        }
    }
}
