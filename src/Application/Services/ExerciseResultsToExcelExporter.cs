using Application.Interfaces.Services;
using Domain.Models;
using OfficeOpenXml;

namespace Application.Services;

public class ExerciseResultsToExcelExporter : IExerciseResultsToExсelExporter
{
    public async Task<Stream> ToExсel(IEnumerable<ExerciseResult> results)
    {
        var stream = new MemoryStream();
        using var excelPackage = new ExcelPackage(stream);
        foreach (var result in results)
        {
            var worksheet = excelPackage.Workbook.Worksheets.Add($"{result.Exercise.Group.Name}-{result.Exercise.Name}");

            worksheet.Cells[1, 1].Value = "№";
            worksheet.Cells[1, 2].Value = "Weight";
            worksheet.Cells[1, 3].Value = "Count";
            worksheet.Cells[1, 4].Value = "Comment";

            using (ExcelRange headerCells = worksheet.Cells[1, 1, 1, 4])
            {
                headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DodgerBlue);
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
            
            using (ExcelRange headerCells = worksheet.Cells[2, 1, row-1, 1])
            {
                headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.CornflowerBlue);
            }
            
            using (ExcelRange headerCells = worksheet.Cells[2, 2, row-1, 4])
            {
                headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
            }
            
            worksheet.Cells[1, 1, row - 1, 4].AutoFitColumns();
        }

        await excelPackage.SaveAsync();
        stream.Position = 0;
        return stream;
    }
}