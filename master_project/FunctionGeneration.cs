using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OfficeOpenXml;

public class FunctionGeneration
{
    // Метод для створення меандра
    public static List<(double x, double y)> GenerateSquareWave(int points, double period, double amplitude)
    {
        List<(double x, double y)> squareWave = new List<(double x, double y)>();

        for (int i = 0; i < points; i++)
        {
            double x = i * period / points; // Час
            double y = (i % 2 == 0) ? amplitude : -amplitude; // Меандр між +A і -A
            squareWave.Add((x, y));
        }

        return squareWave;
    }

    // Метод для створення пилкоподібної хвилі
    public static List<(double x, double y)> GenerateSawtoothWave(int points, double period, double amplitude)
    {
        List<(double x, double y)> sawtoothWave = new List<(double x, double y)>();

        for (int i = 0; i < points; i++)
        {
            double x = i * period / points; // Час
            double y = amplitude * (i / (double)points); // Лінійний ріст
            sawtoothWave.Add((x, y));
        }

        return sawtoothWave;
    }

    // Метод для створення гармонічної хвилі
    public static List<(double x, double y)> GenerateSineWave(int points, double period, double amplitude, double frequency)
    {
        List<(double x, double y)> sineWave = new List<(double x, double y)>();

        for (int i = 0; i < points; i++)
        {
            double x = i * period / points; // Час
            double y = amplitude * Math.Sin(2 * Math.PI * frequency * x); // Гармонічний сигнал
            sineWave.Add((x, y));
        }

        return sineWave;
    }

    // Метод для збереження функцій у Excel
    public static void SaveFunctionsToExcel(List<(double x, double y)> squareWave, List<(double x, double y)> sawtoothWave, List<(double x, double y)> sineWave, string filePath)
    {
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            // Створюємо робочий лист
            var worksheet = package.Workbook.Worksheets.Add("Signal Functions");

            // Заголовки стовпців
            worksheet.Cells[1, 1].Value = "X";
            worksheet.Cells[1, 2].Value = "Square Wave";
            worksheet.Cells[1, 3].Value = "Sawtooth Wave";
            worksheet.Cells[1, 4].Value = "Sine Wave";

            // Запис даних
            int row = 2;
            for (int i = 0; i < squareWave.Count; i++)
            {
                worksheet.Cells[row, 1].Value = squareWave[i].x;
                worksheet.Cells[row, 2].Value = squareWave[i].y;
                worksheet.Cells[row, 3].Value = sawtoothWave[i].y;
                worksheet.Cells[row, 4].Value = sineWave[i].y;
                row++;
            }

            // Зберігаємо файл
            package.Save();
        }
    }
}

