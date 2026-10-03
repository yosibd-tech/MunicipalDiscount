using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace MunicipalDiscount
{
    public partial class IncomeDiscountCriteriaForm : DevExpress.XtraEditors.XtraForm
    {
        private string connectionString = MunicipalDiscount.Classes.DatabaseConfig.ConnectionString;
        private DataTable dataTable;
        private int selectedId = -1;

        /// <summary>
        /// גודל החלון שהמשתמש בחר, לצד הגדרות המשתמש — אותו דפוס כמו
        /// ManParametersForm. נשמר לכל משתמש ולא במסד הנתונים: זו העדפת תצוגה,
        /// לא נתון של הרשות.
        /// </summary>
        private static readonly string LayoutFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MunicipalDiscount", "IncomeDiscountCriteriaForm.xml");

        public IncomeDiscountCriteriaForm()
        {
            InitializeComponent();
            ThemeHelper.ApplyTheme(this);
            this.IconOptions.SvgImage = MunicipalDiscount.Classes.AppIcons.IncomeCriteria;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;
            ConfigureValidityDateMask();
            LoadDiscountCodes();
            RestoreFormSize();
            LoadData();
        }

        private void RestoreFormSize()
        {
            try
            {
                if (File.Exists(LayoutFilePath))
                {
                    string[] parts = File.ReadAllText(LayoutFilePath).Split(',');
                    if (parts.Length == 2
                        && int.TryParse(parts[0], out int w)
                        && int.TryParse(parts[1], out int h))
                    {
                        this.ClientSize = new System.Drawing.Size(w, h);
                    }
                }
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // גודל מוקטן/מוגדל נשמר רק ממצב רגיל — חלון מוגדל היה נשמר
                // כגודל המסך ונפתח כך גם על מסך אחר.
                if (this.WindowState == FormWindowState.Normal)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(LayoutFilePath));
                    File.WriteAllText(LayoutFilePath, $"{ClientSize.Width},{ClientSize.Height}");
                }
            }
            catch { }
            base.OnFormClosing(e);
        }

        /// <summary>קודי מבחן הכנסה, לבחירה בשדה קוד ההנחה.</summary>
        private void LoadDiscountCodes()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var adapter = new SqlDataAdapter(@"
                        SELECT MunicipalDiscountCode,
                               CAST(MunicipalDiscountCode AS nvarchar(10)) + ' - ' +
                               ISNULL(DiscountName, '') AS Display
                        FROM   tbDiscountsCodes
                        WHERE  Categories = N'מבחן הכנסה'
                          AND  MunicipalDiscountCode IS NOT NULL
                        ORDER BY MunicipalDiscountCode", connection);

                    var codes = new DataTable();
                    adapter.Fill(codes);
                    lookupDiscountCode.Properties.DataSource = codes;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בטעינת קודי הנחה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Data Operations
        /// <summary>
        /// מסכת התאריך של DevExpress: dd/MM/yy עם סמן מתקדם, כמו בכל שדה תאריך
        /// אחר במערכת — העורך ממיר את מה שהוקלד ל-DateTime בעצמו.
        /// </summary>
        private void ConfigureValidityDateMask()
        {
            var mask = txtValidityDate.Properties.MaskSettings
                .Configure<DevExpress.XtraEditors.Mask.MaskSettings.DateTime>();
            mask.MaskExpression = "dd/MM/yy";
            mask.UseAdvancingCaret = true;

            txtValidityDate.Properties.Mask.UseMaskAsDisplayFormat = true;
            // אין ערך ברירת מחדל: תאריך תחילת התוקף נכתב על ידי המשתמש בלבד
            txtValidityDate.Properties.NullText = "";
            txtValidityDate.EditValue = null;
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Column renamed from מזהה to ID in SQL Server
                    string query = "SELECT ID, NumberOfPeople, FromAmount, ToAmount, DiscountRate, MunicipalDiscountCode, Year_, ValidityDate FROM tbIncomeDiscountCriteria ORDER BY ValidityDate DESC, NumberOfPeople, FromAmount";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                    dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    gridControl1.DataSource = dataTable;

                    // הכותרות, הפורמטים והרוחב מוגדרים בדיזיינר
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בטעינת נתונים:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveRecord()
        {
            // Validate inputs
            if (!ValidateInputs())
                return;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command;

                    if (selectedId == -1)
                    {
                        // Insert new record
                        string insertQuery = @"INSERT INTO tbIncomeDiscountCriteria
                            (NumberOfPeople, FromAmount, ToAmount, DiscountRate, MunicipalDiscountCode, Year_, ValidityDate)
                            VALUES (@NumberOfPeople, @FromAmount, @ToAmount, @DiscountRate, @MunicipalDiscountCode, @Year, @ValidityDate)";
                        command = new SqlCommand(insertQuery, connection);
                    }
                    else
                    {
                        // Update existing record - column renamed from מזהה to ID
                        string updateQuery = @"UPDATE tbIncomeDiscountCriteria SET
                            NumberOfPeople = @NumberOfPeople, FromAmount = @FromAmount, ToAmount = @ToAmount, DiscountRate = @DiscountRate, MunicipalDiscountCode = @MunicipalDiscountCode, Year_ = @Year, ValidityDate = @ValidityDate
                            WHERE ID = @ID";
                        command = new SqlCommand(updateQuery, connection);
                    }

                    command.Parameters.Add(new SqlParameter("@NumberOfPeople", SqlDbType.Int)).Value = Convert.ToInt32(spinNumberOfPeople.Value);
                    command.Parameters.Add(new SqlParameter("@FromAmount", SqlDbType.Int)).Value = Convert.ToInt32(spinFromAmount.Value);
                    command.Parameters.Add(new SqlParameter("@ToAmount", SqlDbType.Int)).Value = Convert.ToInt32(spinToAmount.Value);
                    // User enters 60 for 60%, save as 0.6
                    command.Parameters.Add(new SqlParameter("@DiscountRate", SqlDbType.Decimal)).Value = Convert.ToDecimal(spinDiscountRate.Value) / 100.0m;
                    command.Parameters.Add(new SqlParameter("@MunicipalDiscountCode", SqlDbType.Int)).Value =
                        lookupDiscountCode.EditValue ?? (object)DBNull.Value;
                    command.Parameters.Add(new SqlParameter("@Year", SqlDbType.Int)).Value = Convert.ToInt32(spinYear.Value);
                    command.Parameters.Add(new SqlParameter("@ValidityDate", SqlDbType.Date)).Value =
                        Convert.ToDateTime(txtValidityDate.EditValue).Date;

                    if (selectedId != -1)
                    {
                        command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int)).Value = selectedId;
                    }

                    command.ExecuteNonQuery();
                }

                // Clear inputs and reload data (no success message)
                selectedId = -1;
                spinNumberOfPeople.Value = 1;
                spinFromAmount.Value = 0;
                spinToAmount.Value = 0;
                spinDiscountRate.Value = 0;
                lookupDiscountCode.EditValue = null;
                spinYear.Value = DateTime.Now.Year;
                txtValidityDate.EditValue = null;   // ריק, בלי הצעה

                LoadData();
                spinNumberOfPeople.Focus();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בשמירת נתונים:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void DeleteRecord()
        {
            try
            {
                if (selectedId == -1)
                {
                    XtraMessageBox.Show("נא לבחור רשומה למחיקה", "הודעה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (XtraMessageBox.Show("האם למחוק את הרשומה?", "אישור מחיקה", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                Cursor.Current = Cursors.WaitCursor;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Column renamed from מזהה to ID
                    string deleteQuery = "DELETE FROM tbIncomeDiscountCriteria WHERE ID = @ID";
                    SqlCommand command = new SqlCommand(deleteQuery, connection);
                    command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int)).Value = selectedId;
                    command.ExecuteNonQuery();
                }

                // Reload data (no success message)
                LoadData();
                ClearInputs();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה במחיקת נתונים:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private bool ValidateInputs()
        {
            if (spinNumberOfPeople.Value <= 0)
            {
                XtraMessageBox.Show("נא להזין מספר נפשות", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                spinNumberOfPeople.Focus();
                return false;
            }

            if (spinFromAmount.Value < 0)
            {
                XtraMessageBox.Show("נא להזין מסכום תקין", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                spinFromAmount.Focus();
                return false;
            }

            if (spinToAmount.Value <= spinFromAmount.Value)
            {
                XtraMessageBox.Show("עד סכום חייב להיות גדול ממסכום", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                spinToAmount.Focus();
                return false;
            }

            // בלי קוד הנחה אין מה לרשום בבקשה, ובדיקת הזכאות תישמר בלי קוד.
            if (lookupDiscountCode.EditValue == null)
            {
                XtraMessageBox.Show("יש לבחור קוד הנחה", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lookupDiscountCode.Focus();
                return false;
            }

            if (spinDiscountRate.Value <= 0 || spinDiscountRate.Value > 100)
            {
                XtraMessageBox.Show("אחוז הנחה חייב להיות בין 1 ל-100", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                spinDiscountRate.Focus();
                return false;
            }

            if (txtValidityDate.EditValue == null ||
                !DateTime.TryParse(txtValidityDate.EditValue.ToString(), out DateTime validity))
            {
                XtraMessageBox.Show("נא להזין תאריך תחילת תוקף", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtValidityDate.Focus();
                return false;
            }

            // התאריך הוא שקובע מתי המדרגה נכנסת לתוקף, והשנה חייבת להתאים לו
            if (validity.Year != Convert.ToInt32(spinYear.Value))
            {
                XtraMessageBox.Show("שנת המס ותאריך תחילת התוקף אינם תואמים", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtValidityDate.Focus();
                return false;
            }

            if (spinYear.Value < 2000 || spinYear.Value > 2100)
            {
                XtraMessageBox.Show("נא להזין שנה תקינה", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                spinYear.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// שורה שנערכה ברשת נשמרת מיד לבסיס הנתונים.
        ///
        /// The grid is bound to a DataTable, so without this the edit lives only
        /// in memory and disappears on the next refresh.
        /// </summary>
        private void gridView1_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            if (e.Row is not DataRowView row) return;
            if (row["ID"] == DBNull.Value) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"UPDATE tbIncomeDiscountCriteria SET
                        NumberOfPeople = @NumberOfPeople, FromAmount = @FromAmount,
                        ToAmount = @ToAmount, DiscountRate = @DiscountRate,
                        MunicipalDiscountCode = @MunicipalDiscountCode,
                        Year_ = @Year, ValidityDate = @ValidityDate
                        WHERE ID = @ID";

                    var cmd = new SqlCommand(query, connection);
                    cmd.Parameters.Add(new SqlParameter("@NumberOfPeople", SqlDbType.Int)).Value = row["NumberOfPeople"];
                    cmd.Parameters.Add(new SqlParameter("@FromAmount", SqlDbType.Decimal)).Value = row["FromAmount"];
                    cmd.Parameters.Add(new SqlParameter("@ToAmount", SqlDbType.Decimal)).Value = row["ToAmount"];
                    cmd.Parameters.Add(new SqlParameter("@DiscountRate", SqlDbType.Decimal)).Value = row["DiscountRate"];
                    cmd.Parameters.Add(new SqlParameter("@MunicipalDiscountCode", SqlDbType.Int)).Value = row["MunicipalDiscountCode"];
                    cmd.Parameters.Add(new SqlParameter("@Year", SqlDbType.Int)).Value = row["Year_"];
                    cmd.Parameters.Add(new SqlParameter("@ValidityDate", SqlDbType.Date)).Value = row["ValidityDate"];
                    cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int)).Value = row["ID"];
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בשמירת השורה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadData();
            }
        }

        /// <summary>
        /// Enter מקדם לשדה הבא בקבוצת הקלט. השמירה נשארת בכפתור שמור בלבד.
        ///
        /// DoValidate first: a DevExpress editor holds what was typed until the
        /// value is committed, so without it the next field — and any later save —
        /// would read the previous value.
        /// </summary>
        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            (sender as DevExpress.XtraEditors.BaseEdit)?.DoValidate();
            SelectNextControl(ActiveControl, true, true, true, true);
        }

        // ── אקסל: ייצוא הקריטריונים וקליטתם חזרה ────────────────────────
        //
        //    טבלת הקריטריונים זהה כמעט לחלוטין בין רשויות — היא נגזרת מהוראות
        //    משרד הפנים — ולכן הקובץ הזה הוא הדרך להעביר אותה לרשות חדשה, כמו
        //    קובצי קודי ההנחה. הפורמט הוא מה שהייצוא כותב, וזה גם מה שהקליטה
        //    מצפה לו.
        //    קוד ההנחה נוסע עם הקובץ: הקליטה מוחקת את הטבלה כולה ובונה אותה
        //    מחדש, כך שעמודה שאינה בקובץ נמחקת בכל סבב ייצוא-קליטה.
        private static readonly string[] ExcelHeaders =
        {
            "מספר נפשות", "מסכום", "עד סכום", "אחוז הנחה", "בתוקף מתאריך", "קוד הנחה"
        };

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                XtraMessageBox.Show("אין נתונים לייצוא.", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "שמור קובץ אקסל",
                FileName = $"קריטריונים_מבחן_הכנסה_{DateTime.Today:yyyy-MM-dd}.xlsx"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using var workbook = new XLWorkbook();
                var sheet = workbook.Worksheets.Add("קריטריונים");
                sheet.RightToLeft = true;

                for (int i = 0; i < ExcelHeaders.Length; i++)
                {
                    var cell = sheet.Cell(1, i + 1);
                    cell.Value = ExcelHeaders[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(42, 82, 152);
                    cell.Style.Font.FontColor = XLColor.White;
                }

                int row = 2;
                foreach (DataRow source in dataTable.Rows)
                {
                    sheet.Cell(row, 1).Value = Convert.ToInt32(source["NumberOfPeople"]);
                    sheet.Cell(row, 2).Value = Convert.ToDecimal(source["FromAmount"]);
                    sheet.Cell(row, 3).Value = Convert.ToDecimal(source["ToAmount"]);

                    // האחוז נשמר כשבר (0.6). נכתב כשבר ומעוצב כאחוז, כך שבאקסל
                    // רואים 60% והקליטה חזרה מקבלת בדיוק את אותו ערך.
                    var rate = sheet.Cell(row, 4);
                    rate.Value = Convert.ToDecimal(source["DiscountRate"]);
                    rate.Style.NumberFormat.Format = "0%";

                    var validity = sheet.Cell(row, 5);
                    validity.Value = Convert.ToDateTime(source["ValidityDate"]);
                    validity.Style.DateFormat.Format = "dd/MM/yyyy";

                    if (source["MunicipalDiscountCode"] != DBNull.Value)
                        sheet.Cell(row, 6).Value = Convert.ToInt32(source["MunicipalDiscountCode"]);

                    row++;
                }

                sheet.Columns().AdjustToContents();
                workbook.SaveAs(dialog.FileName);

                if (XtraMessageBox.Show($"יוצאו {dataTable.Rows.Count} שורות.\n\nלפתוח את הקובץ?",
                        "הייצוא הושלם", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = dialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בייצוא:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "בחר קובץ קריטריונים"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            var rows = new List<(int People, decimal From, decimal To, decimal Rate, DateTime Validity, int? Code)>();
            var errors = new List<string>();

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using var workbook = new XLWorkbook(dialog.FileName);
                var sheet = workbook.Worksheet(1);

                foreach (var line in sheet.RowsUsed().Skip(1))
                {
                    int number = line.RowNumber();
                    if (line.Cell(1).IsEmpty()) continue;

                    try
                    {
                        int people = line.Cell(1).GetValue<int>();
                        decimal from = line.Cell(2).GetValue<decimal>();
                        decimal to = line.Cell(3).GetValue<decimal>();
                        decimal rate = line.Cell(4).GetValue<decimal>();
                        DateTime validity = line.Cell(5).GetValue<DateTime>();

                        // אחוז שנכתב כמספר שלם (60 במקום 0.6) מתוקן כאן, אחרת
                        // הוא ייכנס כ-6000%.
                        if (rate > 1) rate /= 100m;

                        if (people <= 0) { errors.Add($"שורה {number}: מספר נפשות אינו תקין"); continue; }
                        if (to < from) { errors.Add($"שורה {number}: 'עד סכום' קטן מ'מסכום'"); continue; }
                        if (rate <= 0 || rate > 1) { errors.Add($"שורה {number}: אחוז הנחה אינו תקין"); continue; }

                        // קוד הנחה — עמודה שנוספה מאוחר יותר, ולכן קובץ ישן בלעדיה
                        // עדיין נקלט והקוד נשאר ריק.
                        int? code = line.Cell(6).IsEmpty() ? null : line.Cell(6).GetValue<int>();

                        rows.Add((people, from, to, rate, validity.Date, code));
                    }
                    catch
                    {
                        errors.Add($"שורה {number}: לא ניתן לקרוא את הערכים");
                    }
                }
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                XtraMessageBox.Show($"שגיאה בקריאת הקובץ:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            if (errors.Count > 0)
            {
                XtraMessageBox.Show(
                    "הקובץ לא נקלט. נמצאו שגיאות:\n\n" + string.Join("\n", errors.Take(15)),
                    "שגיאות בקובץ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (rows.Count == 0)
            {
                XtraMessageBox.Show("לא נמצאו שורות בקובץ.", "הודעה",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // הקליטה מחליפה את הטבלה כולה — הקובץ הוא התמונה המלאה, לא תוספת.
            if (XtraMessageBox.Show(
                    $"נקראו {rows.Count} שורות מהקובץ.\n\n" +
                    $"הקליטה תמחק את {dataTable?.Rows.Count ?? 0} השורות הקיימות ותחליף אותן.\n" +
                    "להמשיך?",
                    "אישור קליטה", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    new SqlCommand("DELETE FROM tbIncomeDiscountCriteria", connection, transaction)
                        .ExecuteNonQuery();

                    using (var insert = new SqlCommand(@"INSERT INTO tbIncomeDiscountCriteria
                        (NumberOfPeople, FromAmount, ToAmount, DiscountRate, MunicipalDiscountCode, Year_, ValidityDate)
                        VALUES (@People, @From, @To, @Rate, @Code, @Year, @Validity)", connection, transaction))
                    {
                        var pCode = insert.Parameters.Add("@Code", SqlDbType.Int);
                        var pPeople = insert.Parameters.Add("@People", SqlDbType.Int);
                        var pFrom = insert.Parameters.Add("@From", SqlDbType.Decimal);
                        var pTo = insert.Parameters.Add("@To", SqlDbType.Decimal);
                        var pRate = new SqlParameter("@Rate", SqlDbType.Decimal) { Precision = 9, Scale = 4 };
                        insert.Parameters.Add(pRate);
                        var pYear = insert.Parameters.Add("@Year", SqlDbType.Int);
                        var pValidity = insert.Parameters.Add("@Validity", SqlDbType.Date);

                        foreach (var line in rows)
                        {
                            pPeople.Value = line.People;
                            pFrom.Value = line.From;
                            pTo.Value = line.To;
                            pRate.Value = line.Rate;
                            pCode.Value = (object)line.Code ?? DBNull.Value;
                            pYear.Value = line.Validity.Year;   // השנה נגזרת מהתאריך
                            pValidity.Value = line.Validity;
                            insert.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }

                LoadData();
                ClearInputs();

                XtraMessageBox.Show($"נקלטו {rows.Count} שורות.", "הקליטה הושלמה",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בקליטה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ClearInputs()
        {
            selectedId = -1;
            spinNumberOfPeople.Value = 1;
            spinFromAmount.Value = 0;
            spinToAmount.Value = 0;
            spinDiscountRate.Value = 0;
            lookupDiscountCode.EditValue = null;
            spinYear.Value = DateTime.Now.Year;
            spinNumberOfPeople.Focus();
        }
        #endregion

        #region Event Handlers
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveRecord();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteRecord();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearInputs();
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            LoadRowToInputs(e.FocusedRowHandle);
        }

        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            LoadRowToInputs(e.RowHandle);
        }

        private void LoadRowToInputs(int rowHandle)
        {
            if (rowHandle >= 0)
            {
                DataRow row = gridView1.GetDataRow(rowHandle);
                if (row != null)
                {
                    // Column renamed from מזהה to ID
                    selectedId = Convert.ToInt32(row["ID"]);
                    spinNumberOfPeople.Value = Convert.ToInt32(row["NumberOfPeople"]);
                    spinFromAmount.Value = Convert.ToInt32(row["FromAmount"]);
                    spinToAmount.Value = Convert.ToInt32(row["ToAmount"]);
                    // Database stores 0.6 for 60%, multiply by 100 for display
                    spinDiscountRate.Value = Convert.ToInt32(Convert.ToDouble(row["DiscountRate"]) * 100);
                    lookupDiscountCode.EditValue = row["MunicipalDiscountCode"] == DBNull.Value
                        ? null : row["MunicipalDiscountCode"];
                    spinYear.Value = Convert.ToInt32(row["Year_"]);
                    txtValidityDate.EditValue = row["ValidityDate"] == DBNull.Value
                        ? (object)null : Convert.ToDateTime(row["ValidityDate"]);
                }
            }
        }

        private void SpinEdit_Enter(object sender, EventArgs e)
        {
            if (sender is SpinEdit spin)
            {
                BeginInvoke(new Action(() => spin.SelectAll()));
            }
        }

        private void SpinEdit_Click(object sender, EventArgs e)
        {
            if (sender is SpinEdit spin)
            {
                spin.SelectAll();
            }
        }
        #endregion
    }
}
