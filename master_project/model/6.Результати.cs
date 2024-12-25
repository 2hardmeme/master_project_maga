using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows.Forms.DataVisualization.Charting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace master_project
{
    public partial class Form6 : Form
    {
        private string[] yDots;
        private double[][] harmonicSums;
        private string fourierFormula;
        private double[] DYDots;
        private double[] DXDots;

        private double[] harmonicSignal; // Оголошення нового одновимірного масиву
        private double[] allHarmonicsSum;
        private double[] squaredDeviations;

        private double SqrSum; // Declared in the class body
        private double rootOfSum;
        private double standardDeviation;
        private double error;

        public Form6(string[] yDots, double[][] harmonicSums, string fourierFormula, double[] DYDots, double[] DXDots)
        {
            InitializeComponent();
            this.yDots = yDots;
            this.harmonicSums = harmonicSums;
            this.fourierFormula = fourierFormula;
            this.DYDots = DYDots;
            this.DXDots = DXDots;

            FillAllHarmonicsSum();
            FillHarmonicSignal();
            CalculateSquaredDeviations();
            CalculateRootOfSum();
            CalculateStandardDeviation();
            CalculateError();

            SetupChartAxisFormatting();
            EnableZooming();

            PlotGraphs(); // Викликаємо метод для побудови графіків

        }

        private void Form6_Load(object sender, EventArgs e)
        {
            textBox3.Enabled = false;
            textBox4.Enabled = false;

            // Виводимо значення standardDeviation у textBox3
            textBox3.Text = standardDeviation.ToString("F4"); // Формат до 4 знаків після коми

            // Виводимо значення error у textBox4 у відсотках
            textBox4.Text = (error * 100).ToString("F2") + " %"; // Формат до 2 знаків після коми

            // Виводимо значення fourierFormula у textBox5
            textBox5.Text = fourierFormula;
        }

        private void PlotGraphs()
        {
            // Очищаємо існуючі серії на графіку
            chart1.Series.Clear();

            // Створюємо першу серію для сигналу
            var signalSeries = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Сигнал",
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                BorderWidth = 2,
                Color = System.Drawing.Color.Blue
            };

            // Додаємо дані для сигналу (DYDots)
            for (int i = 0; i < DXDots.Length; i++)
            {
                signalSeries.Points.AddXY(DXDots[i], DYDots[i]);
            }

            // Додаємо першу серію до графіку
            chart1.Series.Add(signalSeries);

            // Створюємо другу серію для тригонометричного многочлена
            var polySeries = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Тригонометричний многочлен",
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                BorderWidth = 2,
                Color = System.Drawing.Color.Red
            };

            // Додаємо дані для тригонометричного многочлена (harmonicSignal)
            for (int i = 0; i < DXDots.Length; i++)
            {
                polySeries.Points.AddXY(DXDots[i], harmonicSignal[i]);
            }

            // Додаємо другу серію до графіку
            chart1.Series.Add(polySeries);
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

        private void SaveFormulaToWord(string formula, string filePath)
        {
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                // Додаємо основну частину документа
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Додаємо параграф з формулою
                Paragraph paragraph = new Paragraph();
                Run run = new Run();

                // Додаємо формулу як текст (можна адаптувати до MathML, якщо потрібно)
                run.Append(new Text($"Формула Фур'є: {formula}"));
                paragraph.Append(run);
                body.Append(paragraph);
            }

            MessageBox.Show("Формула збережена у файл: " + filePath, "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFormulaToWord(fourierFormula, "FourierFormula.docx");
        }
    }
}