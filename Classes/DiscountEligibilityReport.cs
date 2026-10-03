using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    public class DiscountEligibilityReport
    {
        #region Properties

        public string ClientNumber { get; set; }
        public string ClientName { get; set; }
        public int FamilySize { get; set; }
        public List<(string Name, int Age, decimal Income)> FamilyMembers { get; set; } = new();
        public decimal TotalIncome { get; set; }
        public string DiscountRate { get; set; }
        public int Year { get; set; }
        public string WorkerName { get; set; }
        public List<string> CheckedDocuments { get; set; }

        #endregion

        private readonly string[] allDocuments = new string[]
        {
            "צילום ת.זהות וספח",
            "אישור ב.לאומי על תקופות עיסוק והכנסות",
            "שומת מס הכנסה",
            "תדפיס אוכלוסין",
            "אישור ב.לאומי מעמד לא עובד",
            "צילום דפי בנק חודשים 10-12"
        };

        private PrintDocument printDocument;

        public DiscountEligibilityReport()
        {
            CheckedDocuments = new List<string>();
            printDocument = new PrintDocument();
            printDocument.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        public void PrintPreview()
        {
            using (PrintPreviewDialog dlg = new PrintPreviewDialog())
            {
                dlg.Document = printDocument;
                dlg.Width = 800;
                dlg.Height = 600;
                dlg.ShowDialog();
            }
        }

        public void Print()
        {
            printDocument.Print();
        }

        public void SaveToPdf(string filePath)
        {
            printDocument.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            printDocument.PrinterSettings.PrintToFile = true;
            printDocument.PrinterSettings.PrintFileName = filePath;
            printDocument.Print();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            // Simple fixed positions (A4 = 827 x 1169 hundredths = ~595 x 842 points)
            float pageWidth = 595;
            float leftX = 50;           // Left edge
            float rightX = 545;         // Right edge (pageWidth - 50)
            float labelX = 570;         // Labels position (right side)
            float valueX = 200;         // Values position
            float yPos = 50;

            // Fonts
            Font titleFont = new Font("Tahoma", 14, FontStyle.Bold);
            Font headerFont = new Font("Tahoma", 11, FontStyle.Bold);
            Font normalFont = new Font("Tahoma", 10);
            Font smallFont = new Font("Tahoma", 9);
            Font resultFont = new Font("Tahoma", 12, FontStyle.Bold);

            // Colors
            Color themeColor = Color.FromArgb(42, 82, 152);
            Brush titleBrush = new SolidBrush(themeColor);
            Brush textBrush = Brushes.Black;
            Brush greenBrush = new SolidBrush(Color.FromArgb(39, 174, 96));
            Brush grayBrush = Brushes.Gray;
            Pen linePen = new Pen(themeColor, 2);

            // String format for right-align
            StringFormat sfRight = new StringFormat { Alignment = StringAlignment.Far };

            // ===== TITLE =====
            g.DrawString("בדיקת זכאות להנחה - מבחן הכנסה", titleFont, titleBrush, labelX, yPos, sfRight);
            yPos += 28;

            g.DrawString($"שנה: {Year}          תאריך: {DateTime.Now:dd/MM/yyyy}", normalFont, titleBrush, labelX, yPos, sfRight);
            yPos += 25;

            g.DrawLine(linePen, leftX, yPos, rightX, yPos);
            yPos += 15;

            // ===== CLIENT =====
            g.DrawString("פרטי לקוח", headerFont, titleBrush, labelX, yPos, sfRight);
            yPos += 25;

            DrawLabelValue(g, ":מספר לקוח", ClientNumber, labelX, valueX, yPos, normalFont, textBrush, sfRight);
            yPos += 22;

            DrawLabelValue(g, ":שם לקוח", ClientName, labelX, valueX, yPos, normalFont, textBrush, sfRight);
            yPos += 28;

            // ===== FAMILY =====
            g.DrawString("פרטי משפחה", headerFont, titleBrush, labelX, yPos, sfRight);
            yPos += 25;

            DrawLabelValue(g, ":סה\"כ נפשות", FamilySize.ToString(), labelX, valueX, yPos, normalFont, textBrush, sfRight);
            yPos += 25;

            g.DrawLine(linePen, leftX, yPos, rightX, yPos);
            yPos += 15;

            // ===== INCOME =====
            g.DrawString("מקורות הכנסה", headerFont, titleBrush, labelX, yPos, sfRight);
            yPos += 25;

            g.DrawString("פרטי הכנסות המשפחה:", headerFont, textBrush, labelX, yPos, sfRight);
            yPos += 25;

            if (FamilyMembers == null || FamilyMembers.Count == 0)
            {
                g.DrawString("לא הוזנו נתוני הכנסה", normalFont, grayBrush, labelX, yPos, sfRight);
                yPos += 22;
            }
            else
            {
                foreach (var member in FamilyMembers)
                {
                    string line = $"  {member.Name}    גיל: {member.Age}    הכנסה: {member.Income:N0} ₪";
                    g.DrawString(line, normalFont, textBrush, labelX, yPos, sfRight);
                    yPos += 22;
                }
            }
            

            yPos += 8;
            g.DrawLine(linePen, leftX, yPos, rightX, yPos);
            yPos += 15;

            // ===== RESULTS =====
            g.DrawString("תוצאת הנחה", headerFont, titleBrush, labelX, yPos, sfRight);
            yPos += 25;

            DrawLabelValue(g, ":סה\"כ הכנסה ממוצעת", FormatCurrency(TotalIncome), labelX, valueX, yPos, normalFont, textBrush, sfRight);
            yPos += 25;

            g.DrawString(":אחוז הנחה", normalFont, textBrush, labelX, yPos, sfRight);
            g.DrawString(DiscountRate, resultFont, greenBrush, valueX, yPos, sfRight);
            yPos += 30;

            g.DrawLine(linePen, leftX, yPos, rightX, yPos);
            yPos += 15;

            // ===== DOCUMENTS =====
            g.DrawString("מסמכים שהוגשו", headerFont, titleBrush, labelX, yPos, sfRight);
            yPos += 25;

            foreach (string doc in allDocuments)
            {
                bool isChecked = CheckedDocuments != null && CheckedDocuments.Contains(doc);
                string status = isChecked ? "✓" : "לא הוגש";
                Brush statusBrush = isChecked ? greenBrush : grayBrush;

                g.DrawString(doc, normalFont, textBrush, labelX, yPos, sfRight);
                g.DrawString(status, normalFont, statusBrush, valueX, yPos, sfRight);
                yPos += 22;
            }

            yPos += 15;
           // g.DrawLine(linePen, leftX, yPos, rightX, yPos);
            yPos += 25;

            // ===== SIGNATURES =====
            // Worker name
            g.DrawString(":שם העובד", normalFont, textBrush, labelX, yPos, sfRight);
            g.DrawString(WorkerName ?? "", normalFont, textBrush, labelX - 80, yPos, sfRight);
            yPos += 70;

            g.DrawLine(Pens.Black, labelX - 150, yPos, labelX, yPos);           // Right signature line
            g.DrawLine(Pens.Black, leftX, yPos, leftX + 150, yPos);             // Left signature line
            yPos += 20;

            // Signature labels (below lines)
            g.DrawString("חתימת עובד", smallFont, grayBrush, labelX - 75, yPos);
            g.DrawString("חתימת מנהל", smallFont, grayBrush, leftX + 40, yPos);

            // ===== FOOTER =====
            yPos += 40;  // Space after signatures
            g.DrawString($"הודפס: {DateTime.Now:dd/MM/yyyy HH:mm}", smallFont, grayBrush, labelX, yPos, sfRight);
            // Cleanup
            titleFont.Dispose();
            headerFont.Dispose();
            normalFont.Dispose();
            smallFont.Dispose();
            resultFont.Dispose();
            titleBrush.Dispose();
            greenBrush.Dispose();
            linePen.Dispose();

            e.HasMorePages = false;
        }

        private void DrawLabelValue(Graphics g, string label, string value, float labelX, float valueX, float y, Font font, Brush brush, StringFormat sf)
        {
            g.DrawString(label, font, brush, labelX, y, sf);
            g.DrawString(value, font, brush, valueX, y, sf);
        }

        private string FormatCurrency(decimal amount)
        {
            return $"{amount:N0} ₪";
        }
    }
}