using System.Drawing;
using Application.Interfaces.Services;
using Domain.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Application.Services;

public class ExerciseResultsToExcelExporter : IExerciseResultsToExсelExporter
{
    public async Task<Stream> ToExсel(IEnumerable<ExerciseResult> results)
    {
        var stream = new MemoryStream();
        using var excelPackage = new ExcelPackage(stream);
        foreach (var result in results)
        {
            var worksheet =
                excelPackage.Workbook.Worksheets.Add($"{result.Exercise.Group.Name}-{result.Exercise.Name}");

            worksheet.Cells[1, 1].Value = "№";
            worksheet.Cells[1, 2].Value = "Weight";
            worksheet.Cells[1, 3].Value = "Count";
            worksheet.Cells[1, 4].Value = "Comment";

            using (var headerCells = worksheet.Cells[1, 1, 1, 4])
            {
                headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerCells.Style.Fill.BackgroundColor.SetColor(Color.DodgerBlue);
                headerCells.Style.Font.Bold = true;
            }

            var row = 2;
            foreach (var approachInfo in result.ApproachInfos)
            {
                worksheet.Cells[row, 1].Value = row - 1;
                worksheet.Cells[row, 2].Value = approachInfo.Weight;
                worksheet.Cells[row, 3].Value = approachInfo.Count;
                worksheet.Cells[row, 4].Value = approachInfo.Comment;
                row++;
            }

            using (var headerCells = worksheet.Cells[2, 1, row - 1, 1])
            {
                headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerCells.Style.Fill.BackgroundColor.SetColor(Color.CornflowerBlue);
            }

            using (var headerCells = worksheet.Cells[2, 2, row - 1, 4])
            {
                headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerCells.Style.Fill.BackgroundColor.SetColor(Color.SkyBlue);
            }

            worksheet.Cells[1, 1, row - 1, 4].AutoFitColumns();
        }

        await excelPackage.SaveAsync();
        stream.Position = 0;
        return stream;
    }
}