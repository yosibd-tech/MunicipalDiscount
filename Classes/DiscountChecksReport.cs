using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;

namespace MunicipalDiscount.Classes
{
    public class DiscountChecksReport
    {
        private readonly string connectionString = DatabaseConfig.ConnectionString;
        // Was a hard-coded path under one machine's Documents folder.
        private static string OutputFolder => ManParameters.ReportsPath;

        public string GenerateReport(int reportYear)
        {
            DataTable data = GetReportData(reportYear);

            if (data.Rows.Count == 0)
                return null;

            // Ensure output folder exists
            Directory.CreateDirectory(OutputFolder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string baseName = $"דוח_ניתוח_הנחות_{reportYear}_{timestamp}";
            string excelPath = Path.Combine(OutputFolder, baseName + ".xlsx");
            string pdfPath = Path.Combine(OutputFolder, baseName + ".pdf");

            ExportToExcel(data, excelPath, reportYear);
            ExportToPdf(data, pdfPath, reportYear);

            return excelPath;
        }

        private DataTable GetReportData(int reportYear)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // One row per request now, not per citizen. The code's rate is the
                // version in force on the request's own date (vwDiscountRateAsOf)
                // rather than "the row for this year", which no longer exists.
                //
                // ComputedDiscount is preferred when it is there: it was stored at
                // the moment the decision was made, so a later mid-year rate change
                // cannot turn a legitimate approval into an illegal one. Only rows
                // that predate that (imported, never checked) fall back to the
                // live calculation.
                //
                // The column list and its order are unchanged — ExportToExcel and
                // ExportToPdf address the columns by position.
                string query = @"
                    WITH req AS (
                        SELECT r.MunicipalDiscountCode, r.ActualDiscountAmount,
                               r.EntitledDiscountPercent, r.ComputedDiscount,
                               r.UserId, r.RequestDate,
                               y.AssetNumber, y.AnnualBilling, y.AssetSize,
                               c.ClientNumber
                        FROM   tbDiscountRequests r
                        INNER  JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
                        INNER  JOIN tbClients        c ON c.ID = y.ClientId
                        WHERE  y.Year_ = @year
                    )
                    SELECT
                        u.FullName AS UserFullName,
                        req.MunicipalDiscountCode,
                        rate.DiscountName,
                        req.ClientNumber,
                        req.AssetNumber,
                        ABS(ISNULL(req.ActualDiscountAmount, 0)) AS ActualCurrentYearDiscount,
                        req.AssetSize     AS CurrentYearAssetSize,
                        req.AnnualBilling AS CurrentYearBlling,
                        rate.MaximumArea,
                        req.EntitledDiscountPercent AS CurrentYearDiscountPercent,
                        COALESCE(req.ComputedDiscount,
                            CASE
                                WHEN req.AssetSize IS NULL OR req.AssetSize = 0 THEN 0
                                ELSE (req.AnnualBilling / req.AssetSize) * rate.MaximumArea * req.EntitledDiscountPercent
                            END) AS LegalDiscountAmount,
                        ABS(ISNULL(req.ActualDiscountAmount, 0)) -
                        COALESCE(req.ComputedDiscount,
                            CASE
                                WHEN req.AssetSize IS NULL OR req.AssetSize = 0 THEN 0
                                ELSE (req.AnnualBilling / req.AssetSize) * rate.MaximumArea * req.EntitledDiscountPercent
                            END) AS IllegalDiscount
                    FROM req
                    LEFT JOIN tbUsers u ON req.UserId = u.ID
                    OUTER APPLY (
                        SELECT TOP 1 v.DiscountName, v.MaximumArea
                        FROM   vwDiscountRateAsOf v
                        WHERE  v.MunicipalDiscountCode = req.MunicipalDiscountCode
                          AND (req.RequestDate IS NULL
                               OR (req.RequestDate >= v.RateFrom
                                   AND (v.RateUntil IS NULL OR req.RequestDate < v.RateUntil)))
                        ORDER BY v.RateFrom DESC
                    ) rate";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.SelectCommand.Parameters.Add("@year", SqlDbType.Int).Value = reportYear;
                adapter.Fill(dt);
            }

            return dt;
        }

        private void ExportToExcel(DataTable data, string filePath, int reportYear)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add($"דוח {reportYear}");
                ws.RightToLeft = true;

                // Column headers
                string[] headers = {
                    "שם העובד", "קוד הנחה", "תאור הנחה", "מס' לקוח", "מס' נכס",
                    "הנחה שניתנה", "גודל נכס", "חיוב שנתי", "הנחה שחושבה", "הנחה לא חוקית"
                };

                // Title row
                ws.Cell(1, 1).Value = $"דוח ניתוח הנחות לשנת {reportYear}";
                ws.Range(1, 1, 1, headers.Length).Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Headers row
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(3, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(42, 82, 152);
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Data rows
                // Map: UserFullName, MunicipalDiscountCode, DiscountName, ClientNumber, AssetNumber,
                //       ActualCurrentYearDiscount, CurrentYearAssetSize, CurrentYearBlling,
                //       LegalDiscountAmount, IllegalDiscount
                // Column 10 (LegalDiscountAmount), not 9 — the header there is
                // "הנחה שחושבה", and column 9 is the percentage, not an amount.
                int[] colIndices = { 0, 1, 2, 3, 4, 5, 6, 7, 10, 11 };

                for (int row = 0; row < data.Rows.Count; row++)
                {
                    for (int col = 0; col < colIndices.Length; col++)
                    {
                        var value = data.Rows[row][colIndices[col]];
                        var cell = ws.Cell(row + 4, col + 1);

                        if (value == DBNull.Value)
                            cell.Value = Blank.Value;
                        else if (value is decimal d)
                            cell.Value = d;
                        else if (value is double dbl)
                            cell.Value = dbl;
                        else if (value is int intVal)
                            cell.Value = intVal;
                        else
                            cell.Value = value.ToString();
                    }

                    // Highlight illegal discount > 0
                    var illegalVal = data.Rows[row]["IllegalDiscount"];
                    if (illegalVal != DBNull.Value && Convert.ToDecimal(illegalVal) > 0)
                    {
                        ws.Cell(row + 4, headers.Length).Style.Font.FontColor = XLColor.Red;
                        ws.Cell(row + 4, headers.Length).Style.Font.Bold = true;
                    }
                }

                // Format numeric columns
                for (int col = 6; col <= 10; col++)
                {
                    ws.Column(col).Style.NumberFormat.Format = "#,##0.00";
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        private void ExportToPdf(DataTable data, string filePath, int reportYear)
        {
            // Column headers for PDF
            string[] headers = {
                "שם העובד", "קוד הנחה", "תאור הנחה", "מס' לקוח", "מס' נכס",
                "הנחה שניתנה", "גודל נכס", "חיוב שנתי", "הנחה שחושבה", "הנחה לא חוקית"
            };
            int[] colIndices = { 0, 1, 2, 3, 4, 5, 6, 7, 10, 11 };

            int currentRow = 0;
            int rowsPerPage = 30;

            PrintDocument printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.Landscape = true;
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("A4", 1169, 827);
            printDoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            printDoc.PrinterSettings.PrintToFile = true;
            printDoc.PrinterSettings.PrintFileName = filePath;

            printDoc.PrintPage += (sender, e) =>
            {
                Graphics g = e.Graphics;
                float pageWidth = e.MarginBounds.Width;
                float startX = e.MarginBounds.Left;
                float startY = e.MarginBounds.Top;
                float yPos = startY;

                Font titleFont = new Font("Tahoma", 12, FontStyle.Bold);
                Font headerFont = new Font("Tahoma", 8, FontStyle.Bold);
                Font dataFont = new Font("Tahoma", 7);
                StringFormat sfRight = new StringFormat { Alignment = StringAlignment.Far };
                StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center };

                Color themeColor = Color.FromArgb(42, 82, 152);
                Brush headerBrush = new SolidBrush(themeColor);
                Pen gridPen = new Pen(Color.Gray, 0.5f);

                // Column widths
                float[] colWidths = { 90, 60, 120, 70, 70, 80, 70, 80, 80, 90 };
                float totalWidth = 0;
                foreach (float w in colWidths) totalWidth += w;
                float scale = pageWidth / totalWidth;
                for (int i = 0; i < colWidths.Length; i++) colWidths[i] *= scale;

                // Title
                if (currentRow == 0)
                {
                    g.DrawString($"דוח ניתוח הנחות לשנת {reportYear}", titleFont, headerBrush,
                        startX + pageWidth / 2, yPos, sfCenter);
                    yPos += 30;
                    g.DrawString($"תאריך: {DateTime.Now:dd/MM/yyyy}", dataFont, Brushes.Gray,
                        startX + pageWidth / 2, yPos, sfCenter);
                    yPos += 25;
                }

                // Headers
                float xPos = startX + pageWidth;
                for (int i = 0; i < headers.Length; i++)
                {
                    xPos -= colWidths[i];
                    RectangleF headerRect = new RectangleF(xPos, yPos, colWidths[i], 20);
                    g.FillRectangle(headerBrush, headerRect);
                    g.DrawString(headers[i], headerFont, Brushes.White, headerRect, sfCenter);
                    g.DrawRectangle(gridPen, headerRect.X, headerRect.Y, headerRect.Width, headerRect.Height);
                }
                yPos += 20;

                // Data rows
                int rowsOnPage = 0;
                while (currentRow < data.Rows.Count && rowsOnPage < rowsPerPage)
                {
                    xPos = startX + pageWidth;
                    for (int col = 0; col < colIndices.Length; col++)
                    {
                        xPos -= colWidths[col];
                        var value = data.Rows[currentRow][colIndices[col]];
                        string text = value == DBNull.Value ? "" :
                            (value is decimal dec ? dec.ToString("N2") :
                             value is double dbl ? dbl.ToString("N2") : value.ToString());

                        RectangleF cellRect = new RectangleF(xPos, yPos, colWidths[col], 18);
                        g.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);

                        Brush textBrush = Brushes.Black;
                        Font cellFont = dataFont;
                        if (col == colIndices.Length - 1) // IllegalDiscount column
                        {
                            var illegalVal = data.Rows[currentRow]["IllegalDiscount"];
                            if (illegalVal != DBNull.Value && Convert.ToDecimal(illegalVal) > 0)
                            {
                                textBrush = Brushes.Red;
                                cellFont = new Font("Tahoma", 7, FontStyle.Bold);
                            }
                        }

                        g.DrawString(text, cellFont, textBrush, cellRect, sfCenter);
                    }

                    yPos += 18;
                    currentRow++;
                    rowsOnPage++;
                }

                // Footer
                g.DrawString($"עמוד {e.PageSettings.PrinterSettings.FromPage}", dataFont, Brushes.Gray,
                    startX + pageWidth / 2, e.MarginBounds.Bottom - 15, sfCenter);

                e.HasMorePages = currentRow < data.Rows.Count;

                titleFont.Dispose();
                headerFont.Dispose();
                dataFont.Dispose();
                headerBrush.Dispose();
                gridPen.Dispose();
            };

            printDoc.Print();
        }
    }
}
