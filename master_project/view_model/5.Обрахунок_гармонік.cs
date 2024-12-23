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
    public partial class Form5 : Form
    {
        private double[] newA1coefficients;
        private double[] newB1coefficients;
        double[] transitXValues;
        private double a0;
        private string[] tDots;
        private string[] yDots;
        private string fourierFormula;

        private double[][] harmonics; // Масив для зберігання гармонік
        private double[][] harmonicSums; // Масив для зберігання сум гармонік

        public Form5(double[] newA1coefficients, double[] newB1coefficients, double[] transitXValues, double a0, string[] tDots, string[] yDots, string fourierFormula)
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
            double firstA0 = a0 / 2;

            CalculateHarmonics(); // Виклик методу для обчислення гармонік
            DisplayHarmonics();
            CalculateHarmonicSums(firstA0);
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

        private void результатиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form6 form6 = new Form6(yDots, harmonicSums, fourierFormula);
            form6.Show();
        }
    }
}
