using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;

namespace MunicipalDiscount.Classes
{
    public class CurrentYearDiscountAuditReport
    {
        private readonly string connectionString = DatabaseConfig.ConnectionString;

        /// <summary>
        /// tbGAssetKinds.Category של נכסי מגורים — קודים 100 (מגורים) ו-110
        /// (מחסן). הדוח בודק נכסי מגורים בלבד, כמו שאר היישום.
        /// </summary>
        private const string ResidentialCategory = "מגורים";

        public string GenerateReport(int reportYear)
        {
            DataTable data = GetReportData(reportYear);

            if (data.Rows.Count == 0)
                return null;

            string outputFolder = ManParameters.ReportsPath;

            Directory.CreateDirectory(outputFolder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string baseName = $"ניתוח_הנחות_שנה_שוטפת_{reportYear}_{timestamp}";
            string excelPath = Path.Combine(outputFolder, baseName + ".xlsx");
            string pdfPath = Path.Combine(outputFolder, baseName + ".pdf");

            ExportToExcel(data, excelPath, reportYear);
            ExportToPdf(data, pdfPath, reportYear);

            return excelPath;
        }

        public string GenerateLetters(int reportYear)
        {
            DataTable data = GetReportData(reportYear);

            if (data.Rows.Count == 0)
                return null;

            string outputFolder = ManParameters.ReportsPath;

            Directory.CreateDirectory(outputFolder);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = Path.Combine(outputFolder, $"מכתבים_הנחות_לא_חוקיות_{reportYear}_{timestamp}.xlsx");

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("מכתבים");
                ws.RightToLeft = true;

                string[] headers = { "מס' לקוח", "שם לקוח", "טלפון", "סכום הנחה לא חוקית", "הודעה" };

                // Title
                ws.Cell(1, 1).Value = $"מכתבים - הנחות לא חוקיות לשנת {reportYear}";
                ws.Range(1, 1, 1, headers.Length).Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Headers
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(3, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(42, 82, 152);
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Aggregate illegal discount per client
                var clientData = new System.Collections.Generic.Dictionary<string, (string name, string phone, decimal total)>();

                foreach (DataRow row in data.Rows)
                {
                    string clientNumber = row["ClientNumber"]?.ToString() ?? "";
                    string clientName = row["ClientName"]?.ToString() ?? "";
                    string phone = "";

                    // Phone comes from tbClients below — one row per citizen now
                    decimal illegalAmount = row["IllegalDiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["IllegalDiscount"]);

                    if (clientData.ContainsKey(clientNumber))
                    {
                        var existing = clientData[clientNumber];
                        clientData[clientNumber] = (existing.name, existing.phone, existing.total + illegalAmount);
                    }
                    else
                    {
                        clientData[clientNumber] = (clientName, phone, illegalAmount);
                    }
                }

                // Get phone numbers
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    foreach (var key in new System.Collections.Generic.List<string>(clientData.Keys))
                    {
                        using (var cmd = new SqlCommand(
                            "SELECT PhoneNumber FROM tbClients WHERE ClientNumber = @client AND PhoneNumber IS NOT NULL AND PhoneNumber <> ''",
                            conn))
                        {
                            cmd.Parameters.AddWithValue("@client", key);
                            var result = cmd.ExecuteScalar();
                            if (result != null)
                            {
                                var item = clientData[key];
                                clientData[key] = (item.name, result.ToString(), item.total);
                            }
                        }
                    }
                }

                // Write data
                int rowIndex = 4;
                foreach (var kvp in clientData)
                {
                    string clientNumber = kvp.Key;
                    string clientName = kvp.Value.name;
                    string phone = kvp.Value.phone;
                    decimal totalIllegal = kvp.Value.total;

                    string messageTemplate = ManParameters.IllegalDiscountMessage;
                    if (string.IsNullOrEmpty(messageTemplate))
                        messageTemplate = "קיבלת הנחה לא חוקית בסך {סכום} ש\"ח, נא לבוא לסדר החוב שנוצר בעקבות כך.";
                    string message = messageTemplate.Replace("{סכום}", totalIllegal.ToString("N2"));

                    ws.Cell(rowIndex, 1).Value = clientNumber;
                    ws.Cell(rowIndex, 2).Value = clientName;
                    ws.Cell(rowIndex, 3).Value = phone;
                    ws.Cell(rowIndex, 4).Value = totalIllegal;
                    ws.Cell(rowIndex, 4).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(rowIndex, 5).Value = message;
                    rowIndex++;
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }

            return filePath;
        }

        private DataTable GetReportData(int reportYear)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // One row per request. The rate is the version that was in force
                // on the request's own date, so a code that changed mid-year no
                // longer re-judges a decision taken before the change.
                //
                // The legal amount is always computed here, at run time, from the
                // rate in force on the request's own date — that is what
                // ActualDiscountAmount is measured against, and the difference
                // between the two is the illegal discount.
                //
                // ComputedDiscount and EntitledDiscountPercent travel alongside it
                // rather than replacing it: they are what the eligibility check
                // decided for this citizen, so a gap between them and the run-time
                // figure marks a ruling that departed from the code's book rate.
                //
                // The GroupCode aggregation is gone: it grouped codes that share a
                // GroupCode and took MAX of each rate, but GroupCode is a copy of
                // MunicipalDiscountCode in the data, so it only obscured the join.
                //
                // Column order is unchanged — ExportToExcel writes the columns in
                // order against the fixed Headers array.
                string query = @"
                    SELECT * FROM (
                        SELECT
                            r.MunicipalDiscountCode,
                            rate.DiscountName,
                            c.ClientNumber,
                            c.ClientName,
                            y.AssetNumber,
                            y.AssetSize     AS CurrentYearAssetSize,
                            rate.MaximumArea,
                            y.AnnualBilling AS CurrentYearBlling,
                            rate.DiscountPercent,
                            rate.MaximumDiscount,
                            eff.EffectiveRate,
                            r.EntitledDiscountPercent,
                            legal.LegalDiscountAmount,
                            r.ComputedDiscount,
                            ABS(ISNULL(r.ActualDiscountAmount, 0)) AS ActualCurrentYearDiscount,
                            ABS(ISNULL(r.ActualDiscountAmount, 0)) - legal.LegalDiscountAmount AS IllegalDiscount
                        FROM tbDiscountRequests r
                        INNER JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
                        INNER JOIN tbClients        c ON c.ID = y.ClientId
                        -- נכסי מגורים בלבד. הדוח בודק את ההנחות שהרשות העניקה
                        -- בפועל מול המגיע לפי התעריף, והיישום כולו עוסק במגורים:
                        -- קליטת החיוב השנתי כבר קולטת רק אותם, כך שחיוב של נכס
                        -- מסחרי אינו מתעדכן ואסור להשוות מולו.
                        -- INNER JOIN במכוון: נכס שלא ניתן לסווג אינו נבדק.
                        INNER JOIN tbGAssetKinds    k ON k.Code = y.AssetCode
                                                     AND k.Category = @residential
                        OUTER APPLY (
                            SELECT TOP 1 v.DiscountName, v.MaximumArea,
                                         v.DiscountPercent, v.MaximumDiscount
                            FROM   vwDiscountRateAsOf v
                            WHERE  v.MunicipalDiscountCode = r.MunicipalDiscountCode
                              AND (r.RequestDate IS NULL
                                   OR (r.RequestDate >= v.RateFrom
                                       AND (v.RateUntil IS NULL OR r.RequestDate < v.RateUntil)))
                            ORDER BY v.RateFrom DESC
                        ) rate
                        CROSS APPLY (
                            SELECT CASE
                                       WHEN rate.MaximumDiscount IS NOT NULL AND rate.DiscountPercent IS NOT NULL
                                            AND rate.MaximumDiscount < rate.DiscountPercent
                                       THEN rate.MaximumDiscount
                                       ELSE rate.DiscountPercent
                                   END AS EffectiveRate
                        ) eff
                        CROSS APPLY (
                            SELECT CASE
                                       WHEN y.AssetSize IS NULL OR y.AssetSize = 0
                                            OR y.AnnualBilling IS NULL OR y.AnnualBilling = 0
                                       THEN 0
                                       WHEN y.AssetSize > ISNULL(rate.MaximumArea, 0) AND ISNULL(rate.MaximumArea, 0) > 0
                                       THEN (y.AnnualBilling / y.AssetSize) * rate.MaximumArea * ISNULL(eff.EffectiveRate, 0)
                                       ELSE y.AnnualBilling * ISNULL(eff.EffectiveRate, 0)
                                   END AS LegalDiscountAmount
                        ) legal
                        WHERE y.Year_ = @year
                    ) rpt
                    WHERE rpt.IllegalDiscount > 0
                      AND rpt.IllegalDiscount >= rpt.CurrentYearBlling * 0.02
                    ORDER BY rpt.MunicipalDiscountCode, rpt.ClientNumber";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                adapter.SelectCommand.Parameters.Add("@year", SqlDbType.Int).Value = reportYear;
                adapter.SelectCommand.Parameters.Add("@residential", SqlDbType.NVarChar, 255).Value = ResidentialCategory;
                adapter.Fill(dt);
            }

            return dt;
        }

        private static readonly string[] Headers = {
            "קוד הנחה", "תאור הנחה", "מס' לקוח", "שם לקוח", "מס' נכס",
            "גודל נכס", "מקסימום שטח", "חיוב שנתי",
            "\u202B% הנחה\u202C", "\u202B% מקסימום הנחה\u202C", "\u202B% הנחה בפועל\u202C",
            "\u202B% זכאות שנקבע\u202C",
            "הנחה חוקית", "הנחה מחושבת בבדיקה", "הנחה בפועל", "הנחה לא חוקית"
        };

        private void ExportToExcel(DataTable data, string filePath, int reportYear)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add($"דוח {reportYear}");
                ws.RightToLeft = true;

                // Title
                ws.Cell(1, 1).Value = $"ניתוח הנחות בשנה השוטפת במערכת הגביה - {reportYear}";
                ws.Range(1, 1, 1, Headers.Length).Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Headers
                for (int i = 0; i < Headers.Length; i++)
                {
                    var cell = ws.Cell(3, i + 1);
                    cell.Value = Headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(42, 82, 152);
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Data
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

                    // Highlight illegal discount > 0
                    var illegalVal = data.Rows[row]["IllegalDiscount"];
                    if (illegalVal != DBNull.Value && Convert.ToDecimal(illegalVal) > 0)
                    {
                        ws.Cell(row + 4, Headers.Length).Style.Font.FontColor = XLColor.Red;
                        ws.Cell(row + 4, Headers.Length).Style.Font.Bold = true;
                    }
                }

                // Sum row
                int sumRow = data.Rows.Count + 4;
                ws.Cell(sumRow, 3).Value = "סה\"כ";
                ws.Cell(sumRow, 3).Style.Font.Bold = true;
                // Sum columns: LegalDiscountAmount(13), ComputedDiscount(14),
                //              ActualCurrentYearDiscount(15), IllegalDiscount(16)
                foreach (int col in new[] { 13, 14, 15, 16 })
                {
                    var cell = ws.Cell(sumRow, col);
                    cell.FormulaA1 = $"SUM({ws.Cell(4, col).Address}:{ws.Cell(sumRow - 1, col).Address})";
                    cell.Style.Font.Bold = true;
                }

                // Format numeric columns
                for (int col = 6; col <= Headers.Length; col++)
                {
                    ws.Column(col).Style.NumberFormat.Format = "#,##0.00";
                }
                // Format percent columns (9, 10, 11, 12)
                for (int col = 9; col <= 12; col++)
                {
                    ws.Column(col).Style.NumberFormat.Format = "0.00%";
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        private void ExportToPdf(DataTable data, string filePath, int reportYear)
        {
            int currentRow = 0;
            int rowsPerPage = 28;

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

                Font titleFont = new Font("Tahoma", 11, FontStyle.Bold);
                Font headerFont = new Font("Tahoma", 7, FontStyle.Bold);
                Font dataFont = new Font("Tahoma", 6.5f);
                StringFormat sfCenter = new StringFormat { Alignment = StringAlignment.Center };

                Color themeColor = Color.FromArgb(42, 82, 152);
                Brush headerBrush = new SolidBrush(themeColor);
                Pen gridPen = new Pen(Color.Gray, 0.5f);

                float[] colWidths = { 50, 100, 60, 90, 60, 55, 55, 65, 50, 55, 55, 55, 65, 65, 65, 65 };
                float totalWidth = 0;
                foreach (float w in colWidths) totalWidth += w;
                float scale = pageWidth / totalWidth;
                for (int i = 0; i < colWidths.Length; i++) colWidths[i] *= scale;

                // Title
                if (currentRow == 0)
                {
                    g.DrawString($"ניתוח הנחות בשנה השוטפת במערכת הגביה - {reportYear}", titleFont, headerBrush,
                        startX + pageWidth / 2, yPos, sfCenter);
                    yPos += 28;
                    g.DrawString($"תאריך: {DateTime.Now:dd/MM/yyyy}", dataFont, Brushes.Gray,
                        startX + pageWidth / 2, yPos, sfCenter);
                    yPos += 22;
                }

                // Headers
                float xPos = startX + pageWidth;
                for (int i = 0; i < Headers.Length; i++)
                {
                    xPos -= colWidths[i];
                    RectangleF headerRect = new RectangleF(xPos, yPos, colWidths[i], 20);
                    g.FillRectangle(headerBrush, headerRect);
                    g.DrawString(Headers[i], headerFont, Brushes.White, headerRect, sfCenter);
                    g.DrawRectangle(gridPen, headerRect.X, headerRect.Y, headerRect.Width, headerRect.Height);
                }
                yPos += 20;

                // Data
                int rowsOnPage = 0;
                while (currentRow < data.Rows.Count && rowsOnPage < rowsPerPage)
                {
                    xPos = startX + pageWidth;
                    for (int col = 0; col < data.Columns.Count; col++)
                    {
                        xPos -= colWidths[col];
                        var value = data.Rows[currentRow][col];
                        string text = value == DBNull.Value ? "" :
                            (value is decimal dec ? dec.ToString("N2") :
                             value is double dbl ? dbl.ToString("N2") : value.ToString());

                        RectangleF cellRect = new RectangleF(xPos, yPos, colWidths[col], 16);
                        g.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);

                        Brush textBrush = Brushes.Black;
                        if (col == data.Columns.Count - 1)
                        {
                            var illegalVal = data.Rows[currentRow]["IllegalDiscount"];
                            if (illegalVal != DBNull.Value && Convert.ToDecimal(illegalVal) > 0)
                                textBrush = Brushes.Red;
                        }

                        g.DrawString(text, dataFont, textBrush, cellRect, sfCenter);
                    }

                    yPos += 16;
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
