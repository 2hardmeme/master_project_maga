using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
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

        private double[] harmonicSignal; // Оголошення нового одновимірного масиву
        private double[] allHarmonicsSum;
        private double[] squaredDeviations;

        private double SqrSum; // Declared in the class body
        private double rootOfSum;
        private double standardDeviation;
        private double error;

        public Form6(string[] yDots, double[][] harmonicSums, string fourierFormula)
        {
            InitializeComponent();
            this.yDots = yDots;
            this.harmonicSums = harmonicSums;
            this.fourierFormula = fourierFormula;

            FillAllHarmonicsSum();
            FillHarmonicSignal();
            CalculateSquaredDeviations();
            CalculateRootOfSum();
            CalculateStandardDeviation();
            CalculateError();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            textBox4.Enabled = false;
            textBox5.Enabled = false;
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