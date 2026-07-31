using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SmartIT.Application;
using SmartIT.Domain;

namespace SmartIT.Web.Controllers;

[Authorize]
public sealed class ReportsController(IRepository<Asset> assets) : Controller
{
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.AssetCount = (await assets.GetAllAsync(cancellationToken)).Count;
        return View();
    }

    [Authorize(Roles = "Admin")]
    public async Task<FileResult> AssetsCsv(CancellationToken cancellationToken)
    {
        var rows = await assets.GetAllAsync(cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("AssetTag,Name,Type,Status,Manufacturer,Model,SerialNumber");

        foreach (var asset in rows)
        {
            builder.AppendLine(string.Join(",", new[]
            {
                Csv(asset.AssetTag), Csv(asset.Name), Csv(asset.Type.ToString()), Csv(asset.Status.ToString()),
                Csv(asset.Manufacturer), Csv(asset.Model), Csv(asset.SerialNumber)
            }));
        }

        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray(),
            "text/csv", $"smartit-assets-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    [Authorize(Roles = "Admin")]
    public async Task<FileResult> AssetsExcel(CancellationToken cancellationToken)
    {
        var rows = await assets.GetAllAsync(cancellationToken);
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Assets");
        sheet.Cell(1, 1).InsertTable(rows.Select(x => new
        {
            x.AssetTag,
            x.Name,
            Type = x.Type.ToString(),
            Status = x.Status.ToString(),
            x.Manufacturer,
            x.Model,
            x.SerialNumber
        }));
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"smartit-assets-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [Authorize(Roles = "Admin")]
    public async Task<FileResult> AssetsPdf(CancellationToken cancellationToken)
    {
        var rows = await assets.GetAllAsync(cancellationToken);
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Header().Column(column =>
                {
                    column.Item().Text("SmartIT Pro — Asset Report").FontSize(20).SemiBold();
                    column.Item().Text($"Generated {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(9);
                });
                page.Content().PaddingTop(16).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(2.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                    });
                    table.Header(header =>
                    {
                        header.Cell().Text("Tag").SemiBold();
                        header.Cell().Text("Asset").SemiBold();
                        header.Cell().Text("Type").SemiBold();
                        header.Cell().Text("Status").SemiBold();
                    });
                    foreach (var asset in rows)
                    {
                        table.Cell().PaddingVertical(4).Text(asset.AssetTag);
                        table.Cell().PaddingVertical(4).Text(asset.Name);
                        table.Cell().PaddingVertical(4).Text(asset.Type.ToString());
                        table.Cell().PaddingVertical(4).Text(asset.Status.ToString());
                    }
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("SmartIT Pro v1.0 • Page ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf();

        return File(bytes, "application/pdf", $"smartit-assets-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    public IActionResult Qr(Guid id)
    {
        var target = Url.Action("Details", "Assets", new { id }, Request.Scheme) ?? $"smartit://asset/{id}";
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(target, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data).GetGraphic(12);
        return File(png, "image/png");
    }

    private static string Csv(string? value)
    {
        var escaped = (value ?? string.Empty).Replace("\"", "\"\"");
        return "\"" + escaped + "\"";
    }
}
