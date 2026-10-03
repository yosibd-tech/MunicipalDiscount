using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using Microsoft.Data.SqlClient;

namespace MunicipalDiscount.Classes
{
    /// <summary>
    /// קודי הנחה שהופיעו בקובץ ההנחות אך אינם קיימים בטבלת הקודים.
    ///
    /// The municipality grants under numbers that exist in tbDiscountsCodes only
    /// as statutory definitions — LegallDiscountCode filled, MunicipalDiscountCode
    /// NULL — so a request can carry a code the code table cannot match. The audit
    /// report can compute nothing for such a request: there is no rate to apply.
    ///
    /// After an import the check runs, writes what it found to a PDF, and the
    /// worker fills the numbers in by hand in frDiscountsCodes with the printout
    /// beside them. Only the two columns that matter for that job are printed:
    /// the number, and the description the file gave it — which is what says
    /// WHICH definition the number belongs to.
    /// </summary>
    public class MissingDiscountCodesReport
    {
        private readonly string connectionString = DatabaseConfig.ConnectionString;

        /// <summary>קוד הנחה שאינו מוגדר, והתאור שהגיע איתו מהקובץ.</summary>
        public sealed class MissingCode
        {
            public int Code { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// כל קוד עירוני שמופיע בבקשות ואין לו הגדרה בטבלת הקודים.
        /// </summary>
        public List<MissingCode> Find()
        {
            var missing = new List<MissingCode>();

            // MIN(name) and not every name: the same code is granted to hundreds
            // of citizens, and the file repeats one description for all of them.
            const string sql = @"
                SELECT   r.MunicipalDiscountCode,
                         MIN(r.MunicipalDiscountCodeName) AS CodeName
                FROM     tbDiscountRequests r
                WHERE    r.MunicipalDiscountCode IS NOT NULL
                  AND    NOT EXISTS (SELECT 1
                                     FROM   tbDiscountsCodes c
                                     WHERE  c.MunicipalDiscountCode = r.MunicipalDiscountCode)
                GROUP BY r.MunicipalDiscountCode
                ORDER BY r.MunicipalDiscountCode";

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    missing.Add(new MissingCode
                    {
                        Code = reader.GetInt32(0),
                        Name = reader.IsDBNull(1) ? "" : reader.GetString(1)
                    });
                }
            }

            return missing;
        }

        /// <summary>
        /// מוסיף את הקודים החסרים לטבלת הקודים ומחזיר כמה נוספו בפועל.
        ///
        /// Categories is left empty on purpose. It is the one column that
        /// cannot be derived from the file - the description says WHICH
        /// discount this is, not which statutory family it belongs to - and
        /// every other coded row has one, so a blank is what makes the new
        /// row visibly unfinished in frDiscountsCodes. LegallDiscountCode and
        /// GroupCode take the municipal number, which is what 59 of the 60
        /// existing coded rows do.
        ///
        /// NOT EXISTS guards a code added by hand between the report and the
        /// answer, so answering Yes twice cannot double-insert.
        ///
        /// A code that belongs to a statutory definition already sitting in
        /// the table with MunicipalDiscountCode NULL gets a NEW row here
        /// rather than filling that one in: deciding the two are the same
        /// discount is the worker's call, and is made in frDiscountsCodes.
        /// </summary>
        public int AddMissing(List<MissingCode> missing)
        {
            if (missing == null || missing.Count == 0)
                return 0;

            const string sql = @"
                INSERT INTO tbDiscountsCodes
                       (MunicipalDiscountCode, LegallDiscountCode, GroupCode, DiscountName)
                SELECT  @code, @code, @code, @name
                WHERE   NOT EXISTS (SELECT 1 FROM tbDiscountsCodes
                                    WHERE  MunicipalDiscountCode = @code)";

            int added = 0;
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    try
                    {
                        foreach (MissingCode code in missing)
                        {
                            using (var cmd = new SqlCommand(sql, con, tx))
                            {
                                cmd.Parameters.AddWithValue("@code", code.Code);
                                cmd.Parameters.AddWithValue("@name",
                                    string.IsNullOrWhiteSpace(code.Name)
                                        ? (object)DBNull.Value : code.Name);
                                added += cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { /* ignore rollback errors */ }
                        throw;
                    }
                }
            }
            return added;
        }

        /// <summary>
        /// כותב את הרשימה לקובץ PDF ומחזיר את הנתיב, או null כשאין מה לכתוב.
        /// הקובץ נשמר בספריית קודי ההנחה, ליד שאר קבצי העזר של הקודים.
        /// </summary>
        public string Generate(List<MissingCode> missing)
        {
            if (missing == null || missing.Count == 0)
                return null;

            string folder = ManParameters.DiscountCodesPath;
            if (string.IsNullOrWhiteSpace(folder))
                folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "גביה");

            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder,
                $"קודי_הנחה_שאינם_מוגדרים_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            ExportToPdf(missing, path);
            return path;
        }

        // Same mechanism the audit report uses — printing to "Microsoft Print to
        // PDF" rather than taking on a PDF library.
        private void ExportToPdf(List<MissingCode> missing, string filePath)
        {
            int current = 0;
            const int rowsPerPage = 34;

            var printDoc = new PrintDocument();
            printDoc.DefaultPageSettings.Landscape = false;
            printDoc.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            printDoc.PrinterSettings.PrintToFile = true;
            printDoc.PrinterSettings.PrintFileName = filePath;

            printDoc.PrintPage += (sender, e) =>
            {
                Graphics g = e.Graphics;
                float pageWidth = e.MarginBounds.Width;
                float startX = e.MarginBounds.Left;
                float yPos = e.MarginBounds.Top;

                var titleFont = new Font("Tahoma", 12, FontStyle.Bold);
                var noteFont = new Font("Tahoma", 8);
                var headerFont = new Font("Tahoma", 9, FontStyle.Bold);
                var dataFont = new Font("Tahoma", 9);
                var center = new StringFormat { Alignment = StringAlignment.Center };

                Color theme = Color.FromArgb(42, 82, 152);
                Brush headerBrush = new SolidBrush(theme);
                var gridPen = new Pen(Color.Gray, 0.5f);

                // שתי עמודות בלבד: הקוד והתאור שהגיע איתו
                float[] widths = { 110, 380 };
                float total = widths[0] + widths[1];
                float scale = pageWidth / total;
                for (int i = 0; i < widths.Length; i++) widths[i] *= scale;

                if (current == 0)
                {
                    g.DrawString("קודי הנחה שאינם מוגדרים בטבלת הקודים", titleFont, headerBrush,
                        startX + pageWidth / 2, yPos, center);
                    yPos += 26;
                    g.DrawString($"תאריך: {DateTime.Now:dd/MM/yyyy}   |   סה\"כ: {missing.Count}",
                        noteFont, Brushes.Gray, startX + pageWidth / 2, yPos, center);
                    yPos += 20;
                    g.DrawString("יש להוסיף קודים אלה במסך קודי הנחה, ולהגדיר להם גרסת תעריף",
                        noteFont, Brushes.Gray, startX + pageWidth / 2, yPos, center);
                    yPos += 24;
                }

                string[] headers = { "קוד הנחה", "תאור ההנחה" };
                float xPos = startX + pageWidth;
                for (int i = 0; i < headers.Length; i++)
                {
                    xPos -= widths[i];
                    var rect = new RectangleF(xPos, yPos, widths[i], 22);
                    g.FillRectangle(headerBrush, rect);
                    g.DrawString(headers[i], headerFont, Brushes.White, rect, center);
                    g.DrawRectangle(gridPen, rect.X, rect.Y, rect.Width, rect.Height);
                }
                yPos += 22;

                int onPage = 0;
                while (current < missing.Count && onPage < rowsPerPage)
                {
                    MissingCode row = missing[current];
                    string[] values = { row.Code.ToString(), row.Name };

                    xPos = startX + pageWidth;
                    for (int col = 0; col < values.Length; col++)
                    {
                        xPos -= widths[col];
                        var rect = new RectangleF(xPos, yPos, widths[col], 20);
                        g.DrawRectangle(gridPen, rect.X, rect.Y, rect.Width, rect.Height);
                        g.DrawString(values[col], dataFont, Brushes.Black, rect, center);
                    }

                    yPos += 20;
                    current++;
                    onPage++;
                }

                e.HasMorePages = current < missing.Count;

                titleFont.Dispose();
                noteFont.Dispose();
                headerFont.Dispose();
                dataFont.Dispose();
                headerBrush.Dispose();
                gridPen.Dispose();
            };

            printDoc.Print();
        }
    }
}
