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

        private double[][] harmonics; // Масив для зберігання гармонік
        private double[][] harmonicSums; // Масив для зберігання сум гармонік

        public Form5(double[] newA1coefficients, double[] newB1coefficients, double[] transitXValues, double a0, string[] tDots)
        {
            InitializeComponent();

            // Приймаємо значення в конструкторі і зберігаємо їх у відповідних полях класу
            this.newA1coefficients = newA1coefficients;
            this.newB1coefficients = newB1coefficients;
            this.transitXValues = transitXValues;
            this.a0 = a0;
            this.tDots = tDots;

            CalculateHarmonics(); // Виклик методу для обчислення гармонік
            DisplayHarmonics();
            CalculateHarmonicSums();
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

        private void CalculateHarmonicSums()
        {
            harmonicSums = new double[transitXValues.Length][];

            // Проходимося по кожному ряду `harmonics`
            for (int i = 0; i < transitXValues.Length; i++)
            {
                // Ініціалізуємо рядок для `harmonicSums`
                harmonicSums[i] = new double[newA1coefficients.Length];

                // Проходимося по кожній гармоніці (тобто по кількості стовпців)
                for (int j = 0; j <= i && j < newA1coefficients.Length; j++)
                {
                    // Додаємо a0/2 та елементи з відповідних стовпців harmonics
                    harmonicSums[i][j] = (a0 / 2) + harmonics[i][0]; // Перший стовпчик
                    if (j > 0)
                    {
                        harmonicSums[i][j] += harmonics[i][1]; // Другий стовпчик
                    }
                    if (j > 1)
                    {
                        harmonicSums[i][j] += harmonics[i][2]; // Третій стовпчик, якщо потрібен
                    }
                    // Продовжити для більше стовпців, якщо необхідно
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
    }
}
