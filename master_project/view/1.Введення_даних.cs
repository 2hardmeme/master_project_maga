using System;
using System.Numerics;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Serialization;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NPOI.POIFS.Crypt.Dsig;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace master_project
{
    public partial class Form1 : Form
    {
        // Останні координати натиснутої точки
        private double lastClickedX;
        private double lastClickedY;

        private string[,] data; // Оголошення поля класу Form1 для зберігання даних
        private string[,] excelData; // Оголошення змінної excelData на рівні класу

        private double period; // Оголошуємо змінну для зберігання періоду
        private string[] yDots;
        private string[] xDots;

        private double samplingFrequency = 100; // Частота дискретизації сигналу
        private double[] DYDots;
        private List<(int index, double value)> localMaximaValues = new List<(int index, double value)>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Зробити textBox2 та button1 неактивними при завантаженні форми
            textBox2.Enabled = false;
            textBox2.Text = "Оберіть файл!";
            button1.Enabled = false;
            button2.Visible = false;
            button3.Visible = false;

            panel3.Visible = false;
            panel5.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            label6.Visible = false;
            textBoxX.Visible = false;
            textBoxY.Visible = false;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            first_step_info(sender, e);
        }

        private void first_step_info(object sender, EventArgs e)
        {
            // Вивід інформаційного вікна
            MessageBox.Show("Вітаємо вас у програмній системі дослідження періодичних функцій.\n\n" +
                            "1. Оберіть файл з даними таблично-заданої функції.\n" +
                            "2. Впишіть в поле \"Період\", число періоду.\n\n" +
                            "Ви можете користуватись побудованим графіком заданої вами функції для обрання періоду.",
                            "Інформація",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        private void першийЕтапToolStripMenuItem_Click(object sender, EventArgs e)
        {
            first_step_info(sender, e);
        }

        //обираємо файл ексель з файлів на пк
        private void відкритиExcelФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
                openFileDialog.Title = "Select an Excel File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    // Виведення шляху до файлу Excel у TextBox
                    textBox1.Text = filePath;

                    // Викликаємо метод для зчитування даних з Excel файлу у двовимірний масив
                    excelData = ReadExcelFile(filePath);

                    // Тепер у excelData міститься ваш двовимірний масив даних з Excel
                    // Ви можете використовувати ці дані далі у вашому додатку

                    // Виведення масиву у DataGridView
                    dataGridView1.Rows.Clear();
                    for (int i = 0; i < excelData.GetLength(0); i++)
                    {
                        dataGridView1.Rows.Add();
                        for (int j = 0; j < excelData.GetLength(1); j++)
                        {
                            dataGridView1.Rows[i].Cells[j].Value = excelData[i, j];
                        }
                    }

                    // Створення графіка з використанням даних excelData
                    CreateChart(excelData);

                    panel3.Visible = true;
                    label3.Visible = true;
                    label4.Visible = true;
                    textBoxX.Visible = true;
                    textBoxY.Visible = true;

                    //переписуємо точки У в одновимірний масив
                    yDots = ExtractSecondColumn(excelData);
                    ConvertYDotsToDYDots();
                    xDots = ExtractFirstColumn(excelData);

                    // Зробити textBox2 та button1 активними
                    textBox2.Enabled = true;
                    textBox2.Text = "";
                    button1.Enabled = true;
                    button2.Visible = true;
                    button3.Visible = true;

                    panel5.Visible = true;
                    label6.Visible = true;
                }
            }
        }

        private void відкритиCSVФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV Files|*.csv";
                openFileDialog.Title = "Select a CSV File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    // Виведення шляху до файлу CSV у TextBox
                    textBox1.Text = filePath;

                    // Викликаємо метод для зчитування даних з CSV файлу у двовимірний масив
                    data = ReadCSVFile(filePath);

                    // Виведення масиву у DataGridView
                    dataGridView1.Rows.Clear();
                    for (int i = 0; i < data.GetLength(0); i++)
                    {
                        dataGridView1.Rows.Add();
                        for (int j = 0; j < data.GetLength(1); j++)
                        {
                            dataGridView1.Rows[i].Cells[j].Value = data[i, j];
                        }
                    }
                    // Створення графіка з використанням даних з CSV файлу
                    //CreateChart(data);

                    //переписуємо точки У в одновимірний масив
                    //string[] yDots = ExtractSecondColumn(excelData);

                    //
                    //
                    //поки не створюємо чарт, бо він ламається чомусь(
                    //
                    //

                }
            }
        }

        //переведення xlsx даних в простий масив точок
        private string[,] ReadExcelFile(string filePath)
        {
            // Відкриття Excel файлу для читання
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(file); // Виберіть формат Excel файлу (XSSFWorkbook для .xlsx)

                ISheet sheet = workbook.GetSheetAt(0); // Виберіть аркуш Excel

                int rowCount = sheet.LastRowNum + 1;
                int colCount = sheet.GetRow(0).LastCellNum;

                // Ініціалізація двовимірного масиву
                string[,] data = new string[rowCount, colCount];

                // Зчитування даних з Excel у двовимірний масив
                for (int row = 0; row < rowCount; row++)
                {
                    IRow excelRow = sheet.GetRow(row);
                    for (int col = 0; col < colCount; col++)
                    {
                        ICell cell = excelRow.GetCell(col);
                        if (cell != null)
                        {
                            data[row, col] = cell.ToString();
                        }
                    }
                }
                return data;
            }
        }

        private string[,] ReadCSVFile(string filePath)
        {
            List<string[]> rows = new List<string[]>();

            // Зчитування рядків з CSV файлу
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    rows.Add(values);
                }
            }

            // Перетворення списку рядків у двовимірний масив
            int rowCount = rows.Count;
            int colCount = rows[0].Length;
            string[,] data = new string[rowCount, colCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    data[i, j] = rows[i][j];
                }
            }

            return data;
        }

        private void CreateChart(string[,] data)
        {
            // Видалення попереднього графіка, якщо він існує
            if (chart1.Series.Count > 0)
                chart1.Series.Clear();

            // Створення серії для графіка
            Series series = new Series("Function Y (T)");
            series.ChartType = SeriesChartType.Line;
            series.BorderWidth = 4; // Встановлення товщини лінії

            // Встановлення стилю точок (червоний колір)
            series.MarkerStyle = MarkerStyle.Circle;
            series.MarkerColor = Color.Red;

            // Додавання точок з масиву даних
            for (int i = 0; i < data.GetLength(0); i++)
            {
                double x = double.Parse(data[i, 0]); // Припустимо, що перший стовпець містить значення x
                double y = double.Parse(data[i, 1]); // Припустимо, що другий стовпець містить значення y

                // Додавання точок до серії
                series.Points.AddXY(x, y);
            }

            // Додавання серії до графіка
            chart1.Series.Add(series);

            // Масштабування осей
            // Налаштування мінімального та максимального значення осі X в залежності від даних
            double minX = data.Cast<string>().Where((val, idx) => idx % 2 == 0).Min(val => double.Parse(val));
            double maxX = data.Cast<string>().Where((val, idx) => idx % 2 == 0).Max(val => double.Parse(val));
            chart1.ChartAreas[0].AxisX.Minimum = minX;
            chart1.ChartAreas[0].AxisX.Maximum = maxX;

            // Масштабування осі Y
            // Налаштування мінімального та максимального значення осі Y в залежності від даних
            double minY = data.Cast<string>().Where((val, idx) => idx % 2 != 0).Min(val => double.Parse(val));
            double maxY = data.Cast<string>().Where((val, idx) => idx % 2 != 0).Max(val => double.Parse(val));
            chart1.ChartAreas[0].AxisY.Minimum = minY;
            chart1.ChartAreas[0].AxisY.Maximum = maxY;

            // Встановлення формату міток осі Y як цілих чисел
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "0";
        }

        //підсвічування координатів точки користувачем
        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            // Отримати координати точки, на яку натиснули
            HitTestResult result = chart1.HitTest(e.X, e.Y);
            if (result.ChartElementType == ChartElementType.DataPoint)
            {
                DataPoint dataPoint = chart1.Series[0].Points[result.PointIndex];
                // Отримати координати точки
                double clickedX = dataPoint.XValue;
                double clickedY = dataPoint.YValues[0];
                // Показати координати точки у текстових полях
                textBoxX.Text = clickedX.ToString();
                textBoxY.Text = clickedY.ToString();
                // Зберегти координати для подальшого використання
                lastClickedX = clickedX;
                lastClickedY = clickedY;
            }
        }

        private string[] ExtractSecondColumn(string[,] data)
        {
            // Ініціалізація одновимірного масиву для зберігання значень другого стовпчика
            string[] yDots = new string[data.GetLength(0)];

            // Переписуємо значення другого стовпчика у одновимірний масив
            for (int i = 0; i < data.GetLength(0); i++)
            {
                yDots[i] = data[i, 1];
            }

            return yDots;
        }

        private string[] ExtractFirstColumn(string[,] data)
        {
            // Ініціалізація одновимірного масиву для зберігання значень першого стовпчика
            string[] xDots = new string[data.GetLength(0)];

            // Переписуємо значення першого стовпчика у одновимірний масив
            for (int i = 0; i < data.GetLength(0); i++)
            {
                xDots[i] = data[i, 0];
            }

            return xDots;
        }

        private void ConvertYDotsToDYDots()
        {
            // Ініціалізація нового масиву для збереження значень у форматі double
            DYDots = new double[yDots.Length];

            // Перетворення значень з yDots в DYDots
            for (int i = 0; i < yDots.Length; i++)
            {
                if (double.TryParse(yDots[i], out double value))
                {
                    DYDots[i] = value;
                }
                else
                {
                    // Якщо значення не можна перетворити в double, ставимо значення за замовчуванням (наприклад, 0)
                    DYDots[i] = 0;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Зчитуємо значення з textBox2
            string input = textBox2.Text;

            // Перевіряємо, чи можна перетворити введене значення в тип double
            if (double.TryParse(input, out double periodValue))
            {
                // Округлюємо значення періоду до найближчого цілого числа
                int roundedPeriod = (int)Math.Round(periodValue);

                // Записуємо округлене значення у змінну period
                period = roundedPeriod;

                // Показуємо MessageBox з двома кнопками для вибору
                var result = MessageBox.Show("Бажаєте продовжити дослідження покроково? Відповідь 'Ні' означає перехід до експериментального дослідження", "Вибір режиму",
                                             MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                             MessageBoxDefaultButton.Button1);
                // Обробляємо вибір користувача
                if (result == DialogResult.Yes)
                {
                    // Якщо натиснули "Yes" (умовно: "Покроково")
                    Form2 form2 = new Form2(period, yDots, xDots);
                    form2.Show();
                }
                else
                {
                    // Якщо натиснули "No" (умовно: "Експеримент")
                    Form7 form7 = new Form7(period, yDots, xDots);
                    form7.Show();
                }
            }
            else
            {
                // Якщо не вдалося перетворити, виводимо повідомлення про помилку
                MessageBox.Show("Невірне значення для періоду.");
            }
        }


        private void FindLocalMaxima()
        {
            localMaximaValues.Clear(); // Очищаємо список максимумів
            double highestValue = double.MinValue; // Для зберігання найвищого максимуму
            int highestIndex = -1; // Для зберігання індексу найвищого максимуму

            // Перевіряємо, чи встановлено значення lastClickedY
            if (double.IsNaN(lastClickedY))
            {
                MessageBox.Show("Будь ласка, виберіть точку на графіку для обчислення діапазону.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Шукаємо найвищий локальний максимум
            for (int i = 1; i < DYDots.Length - 1; i++)
            {
                // Умови для локального максимуму:
                if (DYDots[i] > DYDots[i - 1] && DYDots[i] > DYDots[i + 1])
                {
                    if (DYDots[i] > highestValue)
                    {
                        highestValue = DYDots[i];
                        highestIndex = i;
                    }
                }
            }

            // Обчислення tolerance після знаходження найвищого максимуму
            double tolerance = highestValue - lastClickedY;

            // Шукаємо всі локальні максимуми в межах tolerance
            for (int i = 1; i < DYDots.Length - 1; i++)
            {
                if (DYDots[i] > DYDots[i - 1] && DYDots[i] > DYDots[i + 1])
                {
                    // Додаємо максимум, якщо він у межах tolerance
                    if (DYDots[i] >= highestValue - tolerance)
                    {
                        localMaximaValues.Add((i, DYDots[i]));
                    }
                }
            }
        }

        private void CalculateAndDisplayPeriod()
        {
            if (localMaximaValues.Count < 2)
            {
                MessageBox.Show("Недостатньо локальних максимумів для обчислення періоду.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Завершуємо метод, якщо недостатньо даних
            }

            List<int> indices = localMaximaValues.Select(max => max.Item1).ToList(); // Отримуємо індекси максимумів

            // Обчислюємо відстані між сусідніми максимумами
            List<int> distances = new List<int>();
            for (int i = 1; i < indices.Count; i++)
            {
                distances.Add(indices[i] - indices[i - 1]);
            }

            // Обчислюємо середнє значення відстаней
            double averageDistance = distances.Average();

            // Період дорівнює середній відстані
            double period = averageDistance;

            // Виведення результату
            MessageBox.Show($"Період сигналу: {period:F2} (у кількості точок)", "Результат");
            textBox2.Text = period.ToString("F2"); // Виводимо значення в textBox2
        }


        private void HighlightMaximaOnChart()
        {
            // Очищаємо попередні виділення
            var maximaSeries = chart1.Series.FindByName("Maxima");
            if (maximaSeries == null)
            {
                // Додаємо серію для максимумів, якщо вона ще не створена
                maximaSeries = chart1.Series.Add("Maxima");
                maximaSeries.ChartType = SeriesChartType.Point;
                maximaSeries.Color = Color.Yellow; // Змінено колір на жовтий
                maximaSeries.MarkerSize = 8;
                maximaSeries.MarkerStyle = MarkerStyle.Circle;
            }
            maximaSeries.Points.Clear();

            // Додаємо точки максимумів
            foreach (var (index, value) in localMaximaValues)
            {
                double x = double.Parse(xDots[index]); // Перетворення X-координат
                maximaSeries.Points.AddXY(x, value);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FindLocalMaxima();
            HighlightMaximaOnChart();

            CalculateAndDisplayPeriod();
        }

        private void CalculatePeriod()
        {
            // Перевірка, чи масив DYDots ініціалізований
            if (DYDots == null || DYDots.Length == 0)
            {
                MessageBox.Show("Масив DYDots порожній або не ініціалізований!", "Помилка");
                return;
            }

            // Виклик методу для знаходження періоду
            var periodResult = PeriodMethods.FindPeriod(DYDots, samplingFrequency);

            // Отримання періоду у точках
            double periodInPoints = periodResult.periodInPoints;

            // Виведення результату в TextBox
            textBox2.Text = periodInPoints.ToString("F2");

            // Виведення результату
            MessageBox.Show($"Період сигналу: {periodInPoints:F2} точок", "Результат");
        }


        private void button2_Click(object sender, EventArgs e)
        {
            CalculatePeriod();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Параметри генерації
            int points = 100; // Кількість точок
            double basePeriod = 30; // Базовий період
            double amplitude = 8.1; // Максимальна амплітуда
            double frequency = 1.0; // Частота коливань
            double distortionLevel = 0.2; // Рівень спотворення

            // Генерація функцій
            var distortedSquareWave = FunctionGeneration.GenerateDistortedSquareWave(points, basePeriod, amplitude, distortionLevel);
            var distortedSawtoothWave = FunctionGeneration.GenerateDistortedSawtoothWave(points, basePeriod, amplitude, distortionLevel);
            var distortedSineWave = FunctionGeneration.GenerateDistortedSineWave(points, basePeriod, amplitude, frequency, distortionLevel);

            // Шлях до папки проєкту
            string projectPath = AppDomain.CurrentDomain.BaseDirectory;
            string baseDirectoryPath = Path.Combine(projectPath, "DistortedFunctions");

            // Знаходимо наступний доступний номер папки
            int generationNumber = 1;
            string generationDirectoryPath;
            do
            {
                generationDirectoryPath = Path.Combine(baseDirectoryPath, $"Generation_{generationNumber}");
                generationNumber++;
            } while (Directory.Exists(generationDirectoryPath));

            // Створюємо папку для генерації
            Directory.CreateDirectory(generationDirectoryPath);

            // Збереження функцій
            FunctionGeneration.SaveFunctionsToExcel(distortedSquareWave, distortedSawtoothWave, distortedSineWave, generationDirectoryPath);

            MessageBox.Show($"Спотворені функції збережено у папці: {generationDirectoryPath}");
        }
    }
}