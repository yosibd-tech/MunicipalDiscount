using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;

namespace MunicipalDiscount.Classes
{
    public class PreviousYearDiscountsReport
    {
        private readonly string connectionString = DatabaseConfig.ConnectionString;

        /// <summary>
        /// tbGAssetKinds.Category של נכסי מגורים — קודים 100 (מגורים) ו-110
        /// (מחסן). כל היישום עוסק במגורים בלבד.
        /// </summary>
        private const string ResidentialCategory = "מגורים";
        // Was a hard-coded path under one machine's Documents folder.
        private static string OutputFolder => ManParameters.ReportsPath;

        public string GenerateReport(int reportYear)
        {
            DataTable data = GetReportData(reportYear);

            if (data.Rows.Count == 0)
                return null;

            Directory.CreateDirectory(OutputFolder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string baseName = $"ניתוח_הנחות_שק_{reportYear}_{timestamp}";
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
                // A year's granted discounts are simply that year's request rows —
                // "previous year" is no longer a set of columns, it is Year_ = @year.
                // The discount's name comes from the code definition now.
                string query = @"
                    SELECT
                        r.MunicipalDiscountCode,
                        dc.DiscountName,
                        SUM(ISNULL(r.ActualDiscountAmount, 0)) AS TotalPreviousDiscount,
                        COUNT(DISTINCT y.ClientId)             AS TotalClients
                    FROM tbDiscountRequests r
                    INNER JOIN tbYearlyDiscounts y  ON y.ID = r.YearlyDiscountId
                    -- נכסי מגורים בלבד, כמו בקליטת החיוב ובדוח הביקורת. בלי
                    -- הסינון הזה שני הדוחות היו סופרים קבוצות שונות של נכסים
                    -- ומציגים סכומים שאינם מתיישבים זה עם זה.
                    INNER JOIN tbGAssetKinds    k   ON k.Code = y.AssetCode
                                                   AND k.Category = @residential
                    LEFT  JOIN tbDiscountsCodes dc  ON dc.MunicipalDiscountCode = r.MunicipalDiscountCode
                    WHERE y.Year_ = @year
                    GROUP BY r.MunicipalDiscountCode, dc.DiscountName
                    ORDER BY r.MunicipalDiscountCode";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.SelectCommand.Parameters.Add("@year", SqlDbType.Int).Value = reportYear;
                adapter.SelectCommand.Parameters.Add("@residential", SqlDbType.NVarChar, 255).Value = ResidentialCategory;
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

                string[] headers = { "קוד הנחה", "תאור הנחה", "סה\"כ הנחה ש.קודמת", "מספר לקוחות" };

                // Title row
                ws.Cell(1, 1).Value = $"ניתוח הנחות ש.ק לשנת {reportYear}";
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
                for (int row = 0; row < data.Rows.Count; row++)
                {
                    for (int col = 0; col < data.Columns.Count; col++)
                    {
                        var value = data.Rows[row][col];
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
                }

                // Sum row
                int sumRow = data.Rows.Count + 4;
                ws.Cell(sumRow, 1).Value = "";
                ws.Cell(sumRow, 2).Value = "סה\"כ";
                ws.Cell(sumRow, 2).Style.Font.Bold = true;
                for (int col = 3; col <= 4; col++)
                {
                    var cell = ws.Cell(sumRow, col);
                    cell.FormulaA1 = $"SUM({ws.Cell(4, col).Address}:{ws.Cell(sumRow - 1, col).Address})";
                    cell.Style.Font.Bold = true;
                }

                // Format numeric columns
                ws.Column(3).Style.NumberFormat.Format = "#,##0.00";
                ws.Column(4).Style.NumberFormat.Format = "#,##0";

                ws.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        private void ExportToPdf(DataTable data, string filePath, int reportYear)
        {
            string[] headers = { "קוד הנחה", "תאור הנחה", "סה\"כ הנחה ש.קודמת", "מספר לקוחות" };

            int currentRow = 0;
            int rowsPerPage = 35;

            PrintDocument printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.Landscape = false;
            printDoc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
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
                Font headerFont = new Font("Tahoma", 9, FontStyle.Bold);
                Font dataFont = new Font("Tahoma", 8);
                StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center };

                Color themeColor = Color.FromArgb(42, 82, 152);
                Brush headerBrush = new SolidBrush(themeColor);
                Pen gridPen = new Pen(Color.Gray, 0.5f);

                float[] colWidths = { 70, 200, 120, 100 };
                float totalWidth = 0;
                foreach (float w in colWidths) totalWidth += w;
                float scale = pageWidth / totalWidth;
                for (int i = 0; i < colWidths.Length; i++) colWidths[i] *= scale;

                // Title
                if (currentRow == 0)
                {
                    g.DrawString($"ניתוח הנחות ש.ק לשנת {reportYear}", titleFont, headerBrush,
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
                    for (int col = 0; col < headers.Length; col++)
                    {
                        xPos -= colWidths[col];
                        var value = data.Rows[currentRow][col];
                        string text = value == DBNull.Value ? "" :
                            (value is decimal dec ? dec.ToString("N2") :
                             value is double dbl ? dbl.ToString("N2") : value.ToString());

                        RectangleF cellRect = new RectangleF(xPos, yPos, colWidths[col], 18);
                        g.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                        g.DrawString(text, dataFont, Brushes.Black, cellRect, sfCenter);
                    }

                    yPos += 18;
                    currentRow++;
                    rowsOnPage++;
                }

                // Sum row on last page
                if (currentRow >= data.Rows.Count)
                {
                    yPos += 5;
                    xPos = startX + pageWidth;

                    // Calculate sums
                    decimal sumDiscount = 0;
                    int sumClients = 0;
                    foreach (DataRow dr in data.Rows)
                    {
                        sumDiscount += dr["TotalPreviousDiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["TotalPreviousDiscount"]);
                        sumClients += dr["TotalClients"] == DBNull.Value ? 0 : Convert.ToInt32(dr["TotalClients"]);
                    }

                    string[] sumValues = { "", "סה\"כ", sumDiscount.ToString("N2"), sumClients.ToString() };
                    for (int col = 0; col < headers.Length; col++)
                    {
                        xPos -= colWidths[col];
                        RectangleF cellRect = new RectangleF(xPos, yPos, colWidths[col], 20);
                        g.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                        g.DrawString(sumValues[col], headerFont, Brushes.Black, cellRect, sfCenter);
                    }
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
