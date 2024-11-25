using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OfficeOpenXml;

public class FunctionGeneration
{
    private static Random random = new Random();

    // Метод для створення спотвореного меандра
    public static List<(double x, double y)> GenerateDistortedSquareWave(int points, double basePeriod, double amplitude, double distortionLevel)
    {
        List<(double x, double y)> squareWave = new List<(double x, double y)>();

        for (int i = 0; i < points; i++)
        {
            // Додавання спотворення до періоду
            double distortedPeriod = basePeriod + distortionLevel * (random.NextDouble() - 0.5);
            double x = i * distortedPeriod / points;

            // Спотворення амплітуди
            double distortedAmplitude = amplitude + distortionLevel * (random.NextDouble() - 0.5);
            double y = (i % 2 == 0) ? distortedAmplitude : -distortedAmplitude;

            squareWave.Add((x, y));
        }

        return squareWave;
    }

    // Метод для створення спотвореної пилкоподібної хвилі
    public static List<(double x, double y)> GenerateDistortedSawtoothWave(int points, double basePeriod, double amplitude, double distortionLevel)
    {
        List<(double x, double y)> sawtoothWave = new List<(double x, double y)>();

        for (int i = 0; i < points; i++)
        {
            // Додавання спотворення до періоду
            double distortedPeriod = basePeriod + distortionLevel * (random.NextDouble() - 0.5);
            double x = i * distortedPeriod / points;

            // Спотворення значень функції
            double distortedAmplitude = amplitude + distortionLevel * (random.NextDouble() - 0.5);
            double y = distortedAmplitude * (i / (double)points);

            sawtoothWave.Add((x, y));
        }

        return sawtoothWave;
    }

    // Метод для створення спотвореної гармонічної хвилі
    public static List<(double x, double y)> GenerateDistortedSineWave(int points, double basePeriod, double amplitude, double frequency, double distortionLevel)
    {
        List<(double x, double y)> sineWave = new List<(double x, double y)>();

        for (int i = 0; i < points; i++)
        {
            // Додавання спотворення до періоду
            double distortedPeriod = basePeriod + distortionLevel * (random.NextDouble() - 0.5);
            double x = i * distortedPeriod / points;

            // Спотворення амплітуди
            double distortedAmplitude = amplitude + distortionLevel * (random.NextDouble() - 0.5);
            double y = distortedAmplitude * Math.Sin(2 * Math.PI * frequency * x);

            sineWave.Add((x, y));
        }

        return sineWave;
    }

    // Метод для збереження кожної функції в окремий файл
    public static void SaveFunctionsToExcel(
        List<(double x, double y)> squareWave,
        List<(double x, double y)> sawtoothWave,
        List<(double x, double y)> sineWave,
        string directoryPath)
    {
        SaveFunctionToExcel(squareWave, Path.Combine(directoryPath, "DistortedSquareWave.xlsx"));
        SaveFunctionToExcel(sawtoothWave, Path.Combine(directoryPath, "DistortedSawtoothWave.xlsx"));
        SaveFunctionToExcel(sineWave, Path.Combine(directoryPath, "DistortedSineWave.xlsx"));
    }

    private static void SaveFunctionToExcel(List<(double x, double y)> function, string filePath)
    {
        // Встановлюємо ліцензію для EPPlus
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets.Add("Function Data");

            // Запис даних без заголовків
            for (int i = 0; i < function.Count; i++)
            {
                worksheet.Cells[i + 1, 1].Value = function[i].x;
                worksheet.Cells[i + 1, 2].Value = function[i].y;
            }

            package.Save();
        }
    }
}