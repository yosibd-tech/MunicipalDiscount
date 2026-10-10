using ClosedXML.Excel;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Data.SqlClient;
using MunicipalDiscount.Classes;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {

        private string connectionString = DatabaseConfig.ConnectionString;
        private int _selectedYear = DateTime.Now.Year;  // ← add this

        /// <summary>
        /// tbGAssetKinds.Category של נכסי מגורים — קודים 100 (מגורים) ו-110
        /// (מחסן). רק נכסים אלה נקלטים בקליטת החיוב השנתי.
        /// </summary>
        private const string ResidentialCategory = "מגורים";

        public MainForm()
        {
            InitializeComponent();
            ThemeHelper.ApplyTheme(this);
            AssignIcons();

            // e.g. "ניהול הנחות — גרסה 2026.10.1" (see AppVersion).
            Text = $"{Text} — {AppVersion.Display}";

            // הכפתור שבעמוד "עידכון תאריך בקשת הנחה" — נוצר בדיזיינר, ומחובר
            // כאן ולא בלחיצה כפולה עליו, כדי ש-MainForm.Designer.cs יישאר
            // בדיוק כפי שהדיזיינר כתב אותו.
            barButtonItem2.ItemClick += barButtonItem2_ItemClick;
            barButImportRequestDates.ItemClick += barButImportRequestDates_ItemClick;
            barButMailSetup.ItemClick += barButMailSetup_ItemClick;
            barButMultiDiscounts.ItemClick += barButMultiDiscounts_ItemClick;

            // MDI Background - Original Teal
            foreach (Control control in this.Controls)
            {
                if (control is MdiClient mdiClient)
                {
                    mdiClient.BackColor = Color.FromArgb(88, 150, 166);
                    break;
                }
            }

            InitializeYear();

            // Show logged-in user in the ribbon
            barStaticUserName.Caption = $"משתמש: {SessionData.UserFullName}";

            ApplyRibbonPermissions();
        }

        /// <summary>
        /// מסתיר עמודי סרגל שאין בהם דבר שהמשתמש רשאי לעשות.
        ///
        /// Until now this hid five pages from anyone who was not IsAdmin, so the
        /// per-user permissions could never open anything: a manager with all
        /// fifteen permissions saw exactly what a clerk with none saw. A page is
        /// now shown when the user holds at least one permission for something on
        /// it, and the handlers still check individually — the page is the door,
        /// the CheckPermission call is the lock.
        /// </summary>
        private void ApplyRibbonPermissions()
        {
            bool Any(params string[] keys) =>
                PermissionManager.IsAdmin || keys.Any(PermissionManager.HasPermission);

            ribbonPageImportData.Visible = Any(
                PermissionManager.ImportBilling,
                PermissionManager.ImportDiscounts,
                PermissionManager.ImportRequestDates);

            ribbonPageSysTables.Visible = Any(
                PermissionManager.DiscountHeaders,
                PermissionManager.DiscountsCodes,
                PermissionManager.IncomeDiscountCriteria,
                PermissionManager.MinimumWage,
                PermissionManager.Parameters,
                PermissionManager.CopyDiscounts,
                PermissionManager.UserManagement);

            ribbonPageReports.Visible = Any(PermissionManager.Reports, PermissionManager.SendSms);
            ribbonPagePrint.Visible   = Any(PermissionManager.Reports);

            // ראשי carries the logged-in user's name, and the connection button
            // hides itself for anyone but an admin — so the page is never empty
            // and never exposes anything.
            ribbonPage1.Visible = true;

            // מבחן הכנסה is what the application is for; it stays for everyone,
            // and the button on it is gated like the rest.
            ribbonPage2.Visible = true;

            if (!ribbonControl1.SelectedPage?.Visible ?? true)
                ribbonControl1.SelectedPage = ribbonPage2;
        }


        /// <summary>
        /// Assign the themed SVG icons to the ribbon buttons, sidebar buttons,
        /// and the main window. Icons recolor automatically with the active skin.
        /// </summary>
        private void AssignIcons()
        {
            // Ribbon buttons that open feature forms
            barBtnDiscountEligibility.ImageOptions.SvgImage = AppIcons.Eligibility;
            barButSysTables.ImageOptions.SvgImage = AppIcons.DiscountHeaders;
            barButDiscountCodes.ImageOptions.SvgImage = AppIcons.DiscountCodes;
            barButCriterias.ImageOptions.SvgImage = AppIcons.IncomeCriteria;
            barButMinimumWage.ImageOptions.SvgImage = AppIcons.MinimumWage;
            barButParameters.ImageOptions.SvgImage = AppIcons.Parameters;

            // Left-sidebar buttons (DevExpress SimpleButton)
            //btnOpenYear.ImageOptions.SvgImage = AppIcons.OpenYear;
            //btnOpenYear.ImageOptions.SvgImageSize = new Size(22, 22);

            // Main application window icon (in-app title bar / Alt-Tab)
            IconOptions.SvgImage = AppIcons.App;
            IconOptions.SvgImageSize = new Size(32, 32);
        }

        /// <summary>
        /// פותח את מסך עדכון תאריכי הבקשה לשנה שנבחרה בעמוד הסרגל שלו.
        ///
        /// The year comes from barEditTargetYe ("בחר שנה") on that page, not from
        /// the main ribbon year: the screen is a worklist of one year's dateless
        /// requests, and switching year means a different worklist. An open form
        /// for another year is therefore closed rather than left behind.
        /// </summary>
        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.RequestDates)) return;

            if (barEditTargetYe.EditValue == null ||
                !int.TryParse(barEditTargetYe.EditValue.ToString(), out int year) ||
                year < 2000 || year > 2100)
            {
                XtraMessageBox.Show("יש לבחור שנה", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (Form child in MdiChildren)
            {
                if (child is RequestDateForm open)
                {
                    if (open.Year == year)
                    {
                        open.Activate();
                        return;
                    }

                    open.Close();
                    break;
                }
            }

            var form = new RequestDateForm(year);
            form.MdiParent = this;
            form.Show();
        }

        /// <summary>
        /// קולט את תאריכי בקשת ההנחה מקובץ התנועות של הרשות.
        ///
        /// Report 203 — the file barButImportPreviouseYearData reads — carries no
        /// date, so every request it writes starts with RequestDate NULL. The
        /// municipality's movements export ("כל התנועות") does carry one: a grant
        /// appears there as a movement whose פעולה is the discount code, and its
        /// תאריך גביה is the date. import_request_dates.py does the matching and
        /// fills only the requests that still have no date, so a date a worker
        /// entered by hand in RequestDateForm is never overwritten.
        ///
        /// The year is barEditTargetYe on this ribbon page, the same year the
        /// backfill form beside it works on.
        /// </summary>
        private void barButImportRequestDates_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.ImportRequestDates)) return;

            // Year guard — the file is matched against one year's requests
            if (barEditTargetYe.EditValue == null ||
                !int.TryParse(barEditTargetYe.EditValue.ToString(), out int year) ||
                year < 2000 || year > 2100)
            {
                XtraMessageBox.Show("יש לבחור שנה", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Python script guards
            if (string.IsNullOrWhiteSpace(ManParameters.PythonScriptsPath))
            {
                XtraMessageBox.Show("נתיב סקריפטים Python לא הוגדר.", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string pythonScript = Path.Combine(ManParameters.PythonScriptsPath, "import_request_dates.py");
            if (!File.Exists(pythonScript))
            {
                XtraMessageBox.Show($"קובץ Python לא נמצא:\n{pythonScript}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // File selection — the movements export sits beside the discounts file
            string? filePath = SelectImportFile(ManParameters.ActualCurrentYearDiscountPath,
                "בחר קובץ תנועות");
            if (filePath == null)
                return;

            // openpyxl reads real workbooks only. Checked here so a wrong file
            // gives a sentence instead of a Python traceback in a message box.
            // .xls is excluded too — the old binary format is not one of them.
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xlsm"
                && extension != ".xltx" && extension != ".xltm")
            {
                XtraMessageBox.Show(
                    $"קובץ התנועות חייב להיות חוברת אקסל אמיתית (xlsx.), והקובץ שנבחר הוא {extension}.\n\n" +
                    "יש לפתוח אותו באקסל ולשמור בשם כ-\"חוברת עבודה של Excel (xlsx.)\", ואז לקלוט מחדש.",
                    "סוג קובץ שגוי", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = XtraMessageBox.Show(
                $"האם לקלוט תאריכי בקשה לשנת {year} מהקובץ?", "אישור עיבוד",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{pythonScript}\" \"{filePath}\" {year}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        XtraMessageBox.Show($"קליטת תאריכי בקשה הסתיימה.\n{output}", "הצלחה",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // The backfill worklist shrank — reload it if it is open
                        foreach (Form child in MdiChildren)
                        {
                            if (child is RequestDateForm open && open.Year == year)
                            {
                                open.ReloadRequests();
                                break;
                            }
                        }
                    }
                    else
                    {
                        XtraMessageBox.Show($"שגיאה בקליטת התאריכים:\n{error}\n\n{output}", "שגיאה",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בהפעלת קליטת התאריכים:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #region Year TextBox
        private void InitializeYear()
        {
            // Set current year as default
            barEditYear.EditValue = DateTime.Now.Year;
            SessionData.SetYear(DateTime.Now.Year);

            // Set default values for copy discount years
            barEditSourceYear.EditValue = DateTime.Now.Year - 1;  // Historical year
            barEditTargetYe.EditValue = DateTime.Now.Year;       // Current year

            // Each import carries its own year, shown beside its button.
            // ש.קודמת starts on last year and the other two on this one, which is
            // what each of them loads almost every time. The exception is the
            // turn of the year: on 1.1.2027 a manager checking 2026 sets the box
            // himself - and can now see and do that without leaving the import
            // page, which is where the year used to be missing entirely.
            barEditImportPrevYear.EditValue = DateTime.Now.Year - 1;
            barEditImportCurrYear.EditValue = DateTime.Now.Year;
            barEditImportBillingYear.EditValue = DateTime.Now.Year;
        }

        /// <summary>
        /// קורא שנה מתיבת השנה של היבוא, ומודיע אם היא אינה תקינה.
        /// </summary>
        private bool TryGetImportYear(BarEditItem yearItem, out int year)
        {
            year = 0;
            string text = yearItem.EditValue?.ToString()?.Trim() ?? string.Empty;
            if (!int.TryParse(text, out year) || year < 2000 || year > 2100)
            {
                XtraMessageBox.Show(
                    "יש להזין שנה תקינה (2000-2100) בתיבת השנה שליד כפתור היבוא.",
                    "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void barEditYear_EditValueChanged(object sender, EventArgs e)
        {
            var selectedValue = barEditYear.EditValue;

            if (selectedValue != null && !string.IsNullOrEmpty(selectedValue.ToString()))
            {
                int year;
                if (int.TryParse(selectedValue.ToString(), out year))
                {
                    // Validate year range (reasonable range)
                    if (year >= 2000 && year <= 2100)
                    {
                        SessionData.SetYear(year);
                    }
                    else
                    {
                        XtraMessageBox.Show("נא להזין שנה בין 2000 ל-2100", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        barEditYear.EditValue = DateTime.Now.Year;
                    }
                }
            }
            else
            {
                SessionData.ClearYear();
            }
        }
        #endregion

        #region Open Forms - Helper Method
        private void OpenChildForm<T>(bool allowMultiple = false) where T : Form, new()
        {
            if (!allowMultiple)
            {
                foreach (Form child in this.MdiChildren)
                {
                    if (child is T)
                    {
                        child.Activate();
                        return;
                    }
                }
            }

            T newForm = new T();
            newForm.MdiParent = this;
            newForm.Show();
        }
        #endregion

        #region Ribbon Buttons
        private void barBtnPrint_ItemClick(object sender, ItemClickEventArgs e)
        {
            var activeChild = this.ActiveMdiChild;
            if (activeChild == null)
            {
                XtraMessageBox.Show("אין טופס פתוח להדפסה", "הדפסה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var grid = FindGridControl(activeChild);
            if (grid == null)
            {
                XtraMessageBox.Show("לא נמצאה טבלה להדפסה בטופס הפעיל", "הדפסה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            grid.ShowRibbonPrintPreview();
        }

        private DevExpress.XtraGrid.GridControl FindGridControl(Control parent)
        {
            if (parent is DevExpress.XtraGrid.GridControl gc)
                return gc;

            foreach (Control child in parent.Controls)
            {
                var found = FindGridControl(child);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void barBtnSaveAs_ItemClick(object sender, ItemClickEventArgs e)
        {
            // TODO: Save as Excel or PDF
        }
        #endregion

        #region Permission Check
        private bool CheckPermission(string permissionKey)
        {
            if (!PermissionManager.HasPermission(permissionKey))
            {
                XtraMessageBox.Show("אין לך הרשאה לביצוע פעולה זו", "הרשאה נדרשת",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
        #endregion

        #region טבלאות המערכת - System Tables
        private void btnDiscountHeaders_Click(object sender, EventArgs e)
        {

        }

        private void btnDiscountsCodes_Click(object sender, EventArgs e)
        {

        }

        private void barBtnCopyDiscounts_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.CopyDiscounts)) return;
            try
            {
                // Get years from ribbon spin edits
                if (barEditSourceYear.EditValue == null || barEditTargetYe.EditValue == null)
                {
                    XtraMessageBox.Show("אנא הזן שנת מקור ושנת יעד", "שגיאה",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int sourceYear = Convert.ToInt32(barEditSourceYear.EditValue);
                int targetYear = Convert.ToInt32(barEditTargetYe.EditValue);

                if (sourceYear == targetYear)
                {
                    XtraMessageBox.Show("שנת מקור ושנת יעד חייבות להיות שונות", "שגיאה",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult confirm = XtraMessageBox.Show(
                    $"האם להעתיק את קודי ההנחה משנת {sourceYear} לשנת {targetYear}?",
                    "אישור העתקה", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if target year already has data
                    string checkQuery = "SELECT COUNT(*) FROM tbDiscountsCodes WHERE Year_ = @targetYear";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                    checkCmd.Parameters.Add("@targetYear", SqlDbType.Int).Value = targetYear;
                    int existingCount = (int)checkCmd.ExecuteScalar();

                    if (existingCount > 0)
                    {
                        DialogResult overwrite = XtraMessageBox.Show(
                            $"שנת {targetYear} כבר מכילה {existingCount} קודי הנחה.\nהאם למחוק ולהעתיק מחדש?",
                            "נתונים קיימים", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (overwrite != DialogResult.Yes)
                            return;

                        // Delete existing target year data
                        string deleteQuery = "DELETE FROM tbDiscountsCodes WHERE Year_ = @targetYear";
                        SqlCommand deleteCmd = new SqlCommand(deleteQuery, connection);
                        deleteCmd.Parameters.Add("@targetYear", SqlDbType.Int).Value = targetYear;
                        deleteCmd.ExecuteNonQuery();
                    }

                    // Copy rows from source year to target year
                    string copyQuery = @"
                        INSERT INTO tbDiscountsCodes
                            (Categories, MunicipalDiscountCode, LegallDiscountCode, GroupCode, DiscountName, DiscountPercent,
                             MaximumDiscount, MaximumArea, Mandatory, LawInForce, Year_,
                             Documents, Remark, Rules, Conditions)
                        SELECT Categories, MunicipalDiscountCode, LegallDiscountCode, GroupCode, DiscountName, DiscountPercent,
                               MaximumDiscount, MaximumArea, Mandatory, LawInForce, @targetYear,
                               Documents, Remark, Rules, Conditions
                        FROM tbDiscountsCodes
                        WHERE Year_ = @sourceYear";

                    SqlCommand copyCmd = new SqlCommand(copyQuery, connection);
                    copyCmd.Parameters.Add("@sourceYear", SqlDbType.Int).Value = sourceYear;
                    copyCmd.Parameters.Add("@targetYear", SqlDbType.Int).Value = targetYear;

                    int rowsCopied = copyCmd.ExecuteNonQuery();

                    if (rowsCopied > 0)
                    {
                        XtraMessageBox.Show(
                            $"הועתקו {rowsCopied} קודי הנחה משנת {sourceYear} לשנת {targetYear}",
                            "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        XtraMessageBox.Show(
                            $"לא נמצאו קודי הנחה לשנת {sourceYear}",
                            "הודעה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בהעתקת קודי הנחה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void barBtnDiscountEligibility_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.DiscountEligibility)) return;
            OpenChildForm<DiscountEligibilityChecks>(true);  // Allow multiple instances
        }
        /// <summary>
        /// The workbook of everything the billing file changed that a worker has to
        /// look at, opened as soon as the import finishes:
        ///
        ///   - a blank חיוב or שטח, which is stored as 0 and replaces a value the
        ///     property already had. 0 חיוב reads as "billed nothing", not "not
        ///     billed yet".
        ///   - a property whose קוד סוג נכס changed — a dwelling that became a shop
        ///     leaves the eligibility sum altogether, and the other way round.
        ///   - a property whose שטח got SMALLER than the one on record.
        ///   - billing that went DOWN, because a decision already approved was
        ///     computed on the higher figure and is now too generous.
        ///
        /// Only properties the system ALREADY HOLDS WITH A VALUE are listed. A
        /// property arriving for the first time, and one that was never billed, have
        /// nothing that could change and are left out - the sheet is a list of
        /// changes, not a list of gaps.
        ///
        /// The rows are read before a single value is written, so the "קיים במערכת"
        /// columns still hold what the system had. It opens by itself rather than
        /// waiting in a folder: nothing later in the year points any of this out.
        /// </summary>
        private void SaveBillingBlankValues(DataTable rows)
        {
            try
            {
                string folder = Classes.ManParameters.CurrentYearBillingPath;
                if (string.IsNullOrWhiteSpace(folder))
                    folder = Classes.ManParameters.ReportsPath;

                Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder,
                    $"חיוב_שינויים_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("שינויים בקליטה");
                    ws.RightToLeft = true;

                    string[] headers =
                    {
                        "מספר לקוח", "שם לקוח", "שנה", "מספר נכס",
                        "חיוב בקובץ", "חיוב קיים במערכת",
                        "שטח בקובץ", "שטח קיים במערכת",
                        "קוד סוג בקובץ", "קוד סוג קיים", "הערה"
                    };

                    ws.Cell(1, 1).Value = "קליטת חיוב שנתי — ערכים חסרים ושינויים";
                    ws.Range(1, 1, 1, headers.Length).Merge();
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontSize = 14;
                    ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = ws.Cell(3, i + 1);
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(42, 82, 152);
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    for (int i = 0; i < rows.Rows.Count; i++)
                    {
                        DataRow row = rows.Rows[i];
                        int line = i + 4;

                        decimal fileBilling = row["FileBilling"] == DBNull.Value
                            ? 0m : Convert.ToDecimal(row["FileBilling"]);
                        decimal fileSize = row["FileSize"] == DBNull.Value
                            ? 0m : Convert.ToDecimal(row["FileSize"]);

                        bool hadBilling = row["ExistingBilling"] != DBNull.Value
                            && Convert.ToDecimal(row["ExistingBilling"]) > 0;
                        bool hadSize = row["ExistingSize"] != DBNull.Value
                            && Convert.ToDecimal(row["ExistingSize"]) > 0;

                        decimal existingBilling = hadBilling ? Convert.ToDecimal(row["ExistingBilling"]) : 0m;
                        decimal existingSize    = hadSize    ? Convert.ToDecimal(row["ExistingSize"])    : 0m;

                        int? fileCode = row["FileCode"] == DBNull.Value
                            ? (int?)null : Convert.ToInt32(row["FileCode"]);
                        int? existingCode = row["ExistingCode"] == DBNull.Value
                            ? (int?)null : Convert.ToInt32(row["ExistingCode"]);

                        ws.Cell(line, 1).Value = row["ClientNumber"] == DBNull.Value
                            ? "" : row["ClientNumber"].ToString();
                        ws.Cell(line, 2).Value = row["ClientName"] == DBNull.Value
                            ? "" : row["ClientName"].ToString();
                        ws.Cell(line, 3).Value = Convert.ToInt32(row["Year_"]);
                        ws.Cell(line, 4).Value = row["AssetNumber"].ToString();
                        ws.Cell(line, 5).Value = fileBilling;
                        ws.Cell(line, 7).Value = fileSize;

                        if (hadBilling) ws.Cell(line, 6).Value = existingBilling;
                        if (hadSize)    ws.Cell(line, 8).Value = existingSize;
                        if (fileCode.HasValue)     ws.Cell(line, 9).Value  = fileCode.Value;
                        if (existingCode.HasValue) ws.Cell(line, 10).Value = existingCode.Value;

                        // Everything this property did, in the words the operator has
                        // to act on. A property can do several of them at once.
                        var notes = new System.Collections.Generic.List<string>();

                        bool lostBilling = fileBilling == 0 && hadBilling;
                        bool lostSize    = fileSize == 0 && hadSize;

                        if (lostBilling) notes.Add("נדרס חיוב קיים באפס");
                        if (lostSize)    notes.Add("נדרס שטח קיים באפס");

                        if (fileCode.HasValue && fileCode != existingCode)
                        {
                            notes.Add($"שונה קוד סוג נכס: {(existingCode.HasValue ? existingCode.Value.ToString() : "ריק")} ← {fileCode.Value}");
                        }

                        if (!lostSize && fileSize > 0 && hadSize && fileSize < existingSize)
                        {
                            notes.Add($"השטח הוקטן: {existingSize:#,##0.##} ← {fileSize:#,##0.##}");
                        }

                        if (!lostBilling && fileBilling > 0 && hadBilling && fileBilling < existingBilling)
                        {
                            notes.Add($"החיוב הוקטן: {existingBilling:#,##0} ← {fileBilling:#,##0} — הנחה שאושרה חושבה על הסכום הגבוה");
                        }

                        ws.Cell(line, 11).Value = string.Join(" · ", notes);

                        // Red where a real value was destroyed; orange where it only
                        // changed - both need a look, but not the same look.
                        if (lostBilling || lostSize)
                        {
                            ws.Range(line, 1, line, headers.Length).Style.Font.FontColor = XLColor.Red;
                            ws.Cell(line, 11).Style.Font.Bold = true;
                        }
                        else if (notes.Count > 0)
                        {
                            ws.Cell(line, 11).Style.Font.FontColor = XLColor.FromArgb(185, 119, 14);
                        }
                    }

                    for (int col = 5; col <= 8; col++)
                        ws.Column(col).Style.NumberFormat.Format = "#,##0.00";

                    ws.SheetView.FreezeRows(3);
                    ws.Range(3, 1, 3 + rows.Rows.Count, headers.Length).SetAutoFilter();
                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                }

                Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בהפקת קובץ שינויים בקליטת חיוב:\n{ex.Message}",
                    "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveBillingNotFound(System.Collections.Generic.List<(string assetNumber, decimal billing, decimal assetSize)> rows)
        {
            try
            {
                string billingFolder = Classes.ManParameters.CurrentYearBillingPath;
                if (string.IsNullOrEmpty(billingFolder))
                {
                    XtraMessageBox.Show("לא הוגדר נתיב לקובץ חיוב שנתי בפרמטרים", "שגיאה",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Directory.CreateDirectory(billingFolder);
                string filePath = Path.Combine(billingFolder, "חיוב_לא_נמצאו.xlsx");

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("לא נמצאו");
                    ws.RightToLeft = true;

                    ws.Cell(1, 1).Value = "מספר נכס";
                    ws.Cell(1, 2).Value = "חיוב שנתי";
                    ws.Cell(1, 3).Value = "גודל נכס";

                    for (int i = 0; i < rows.Count; i++)
                    {
                        ws.Cell(i + 2, 1).Value = rows[i].assetNumber;
                        ws.Cell(i + 2, 2).Value = rows[i].billing;
                        ws.Cell(i + 2, 3).Value = rows[i].assetSize;
                    }

                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                }

                XtraMessageBox.Show($"נשמרו {rows.Count} רשומות חיוב שלא נמצאו:\n{filePath}",
                    "קובץ חיוב לא נמצאו", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בשמירת קובץ חיוב לא נמצאו:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        #region Import Discount File

        /// <summary>
        /// Core current-year actual-discount import, invoked by the ribbon button
        /// (barButImportCurrentYearData_ItemClick).
        /// Returns true only when the import actually completed, so the caller
        /// can delete the source file.
        /// </summary>
        /// <summary>
        /// קליטת קובץ הנחות שנה נוכחית — הקובץ המקורי כפי שהורד ממערכת העירייה.
        ///
        /// Every line of the file is a discount the municipality already granted,
        /// so every line becomes one row in tbDiscountRequests. The citizen and
        /// the year row are created when they are not there yet, which is why
        /// this import no longer has a "not found" list: nothing can be missing.
        /// The grants arrive without a date — RequestDate is filled later
        /// through the backfill form.
        /// </summary>
        private bool ImportCurrentYearDiscountFile(string filePath, int year)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // Read the tab-separated file (cp1255 encoding from municipal system)
                string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding(1255));

                // Load column mappings from tbDiscountHeaders (same as import_discounts.py)
                // Column1=ClientNumber, Column2=ClientName, Column3=AssetNumber, Column4=ClieneWifeNumber,
                // Column5=DiscountCode, Column6=DiscountName, Column7=PhoneNumber, Column8=Year_, Column9=DiscountAmount
                int colClientNumber = -1, colClientName = -1, colAssetNumber = -1;
                int colWifeNumber = -1, colDiscountCode = -1, colDiscountName = -1;
                int colPhone = -1;

                using (SqlConnection connHeaders = new SqlConnection(connectionString))
                {
                    connHeaders.Open();
                    using (SqlCommand cmdHeaders = new SqlCommand(
                        "SELECT Column1, Column2, Column3, Column4, Column5, Column6, Column7 FROM tbDiscountHeaders", connHeaders))
                    using (SqlDataReader reader = cmdHeaders.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!reader.IsDBNull(0) && int.TryParse(reader[0].ToString(), out int c1) && c1 > 0) colClientNumber = c1 - 1;
                            if (!reader.IsDBNull(1) && int.TryParse(reader[1].ToString(), out int c2) && c2 > 0) colClientName = c2 - 1;
                            if (!reader.IsDBNull(2) && int.TryParse(reader[2].ToString(), out int c3) && c3 > 0) colAssetNumber = c3 - 1;
                            if (!reader.IsDBNull(3) && int.TryParse(reader[3].ToString(), out int c4) && c4 > 0) colWifeNumber = c4 - 1;
                            if (!reader.IsDBNull(4) && int.TryParse(reader[4].ToString(), out int c5) && c5 > 0) colDiscountCode = c5 - 1;
                            if (!reader.IsDBNull(5) && int.TryParse(reader[5].ToString(), out int c6) && c6 > 0) colDiscountName = c6 - 1;
                            if (!reader.IsDBNull(6) && int.TryParse(reader[6].ToString(), out int c7) && c7 > 0) colPhone = c7 - 1;
                        }
                    }
                }

                // Find header row, "סכום", "אזור" and "שנה מקורית" — none of the
                // three is mapped in tbDiscountHeaders, which has nine fixed slots.
                //
                // אזור is matched exactly: the file carries "תיאור אזור" beside it
                // ("איזור 2"), and a loose match would take whichever came first.
                // שנה מקורית likewise sits beside "שנה נוכחית".
                int headerRowIndex = -1;
                int colAmount = -1;
                int colArea = -1;
                int colOriginalYear = -1;

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("סכום"))
                    {
                        headerRowIndex = i;
                        string[] headers = lines[i].Split('\t');
                        for (int j = 0; j < headers.Length; j++)
                        {
                            string header = headers[j].Trim();
                            if (header == "סכום") colAmount = j;
                            if (header == "אזור") colArea = j;
                            if (header == "שנה מקורית") colOriginalYear = j;
                        }
                        break;
                    }
                }

                if (headerRowIndex == -1 || colClientNumber == -1 || colAmount == -1)
                {
                    XtraMessageBox.Show("לא נמצאו עמודות נדרשות בקובץ או במיפוי כותרות", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

            // שורות שנפסלו בגלל שנת מקור אחרת, והשנים שנמצאו — מדווחות בסיכום.
            int retroactiveSkipped = 0;
            var retroactiveYears = new SortedSet<int>();

            // year arrives from the caller's year box
                int requestsAdded = 0;
                int skippedCount = 0;
                int removedCount = 0;
                int newClients = 0;
                var citizens = new HashSet<string>();
                var unplacedGrants = new List<(string client, string asset)>();

                DialogResult confirm = XtraMessageBox.Show(
                    $"האם לקלוט את הקובץ ולעדכן הנחות בפועל לשנת {year}?",
                    "אישור קליטה", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return false;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction tx = connection.BeginTransaction())
                    {
                        try
                        {
                            // קליטה חוזרת של אותה שנה מחליפה את מה שנקלט קודם —
                            // בקשות שעובד כבר טיפל בהן אינן נמחקות.
                            removedCount = DiscountRepository.DeleteUntouchedImportedRequests(connection, tx, year);

                            for (int i = headerRowIndex + 1; i < lines.Length; i++)
                            {
                                if (i % 50 == 0)
                                {
                                    Cursor.Current = Cursors.WaitCursor;
                                    Application.DoEvents();
                                }

                                string line = lines[i].Trim();
                                if (string.IsNullOrEmpty(line)) continue;

                                string[] fields = line.Split('\t');
                                if (fields.Length <= Math.Max(colClientNumber, colAmount)) continue;

                                string clientNumber = fields[colClientNumber].Trim();
                                string amountStr = fields[colAmount].Trim();

                                if (string.IsNullOrEmpty(clientNumber) || string.IsNullOrEmpty(amountStr))
                                {
                                    skippedCount++;
                                    continue;
                                }

                                if (!decimal.TryParse(amountStr, out decimal amount))
                                {
                                    skippedCount++;
                                    continue;
                                }

                                string Field(int col) =>
                                    col >= 0 && col < fields.Length ? fields[col].Trim() : "";

                                // הנחה ששנת המקור שלה אינה שנת הקליטה אינה הנחה של
                                // השנה הזו. הקובץ כולל הנחות רטרואקטיביות — ב-2026
                                // יש 87 שורות לשנים 2019 עד 2025 — וללא הסינון הזה
                                // כולן נרשמות כהנחות 2026. לקוח אחד קיבל כך חמש
                                // הנחות על אותו נכס בסכום פי 5.4 מהחיוב השנתי.
                                //
                                // The comparison is against the import year, not
                                // merely against the file's own "שנה נוכחית": one
                                // 2026 line carries 2201 in both year columns, a
                                // typo in the municipal system, and it belongs to
                                // no year this application knows.
                                if (colOriginalYear >= 0)
                                {
                                    string originalYearText = Field(colOriginalYear);

                                    if (int.TryParse(originalYearText, out int originalYear)
                                        && originalYear != year)
                                    {
                                        retroactiveSkipped++;
                                        retroactiveYears.Add(originalYear);
                                        continue;
                                    }
                                }

                                try
                                {
                                    int.TryParse(Field(colDiscountCode), out int discountCodeInt);

                                    // A citizen the previous-year file never carried is
                                    // simply created here — there is no "not found" list
                                    // any more, and nothing to reconcile afterwards.
                                    int clientId = DiscountRepository.EnsureClient(connection, tx,
                                        clientNumber, Field(colClientName), Field(colWifeNumber), Field(colPhone),
                                        out bool isNewClient);
                                    if (isNewClient) newClients++;

                                    int? area = int.TryParse(Field(colArea), out int areaValue)
                                        ? areaValue : (int?)null;

                                    // הנכס חייב להיות קיים כבר. קובץ החיוב השנתי הוא
                                    // מרשם הנכסים והוא רץ ראשון; הנחה שאין לה נכס
                                    // להיתלות עליו היא הנחה על נכס שאינו מגורים —
                                    // מחוץ לתחום — או סימן שהחיוב טרם נקלט לשנה זו.
                                    // בשני המקרים היא מדווחת ואינה יוצרת שורה ריקה.
                                    int yearlyId = DiscountRepository.EnsureYearRow(connection, tx,
                                        clientId, year, Field(colAssetNumber), area,
                                        createIfMissing: false);

                                    if (yearlyId == 0)
                                    {
                                        unplacedGrants.Add((clientNumber, Field(colAssetNumber)));
                                        continue;
                                    }

                                    // colDiscountName is tbDiscountHeaders.Column6 — column F,
                                    // headed "תאור". Taken by position, not by header text:
                                    // "תאור" appears four times in that header row.
                                    DiscountRepository.InsertRequest(connection, tx,
                                        yearlyId, null, discountCodeInt, amount, SessionData.CurrentUserId,
                                        Field(colDiscountName));

                                    citizens.Add(clientNumber);
                                    requestsAdded++;
                                }
                                catch (Exception exRow)
                                {
                                    // All or nothing — a half-loaded year is worse than none,
                                    // and re-running a corrected file is now safe.
                                    throw new InvalidOperationException(
                                        $"שורה {i + 1}, לקוח {clientNumber}: {exRow.Message}", exRow);
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

                    // Billing figures for assets the billing import could not place
                    // earlier — the year rows for them may exist now.
                    SyncBillingForNewClients(connection, year);
                }

                string message = $"הקליטה הסתיימה:\n" +
                                 $"נקלטו: {requestsAdded} הנחות עבור {citizens.Count} לקוחות\n" +
                                 $"לקוחות חדשים שנוספו: {newClients}\n" +
                                 $"הוסרו מקליטה קודמת: {removedCount}\n" +
                                 $"שורות שדולגו: {skippedCount}\n" +
                                 $"הנחות ללא נכס מתאים — לא נקלטו: {unplacedGrants.Count}" +
                                 (unplacedGrants.Count > 0
                                    ? "\n(נכסים שאינם מגורים, או שקובץ החיוב טרם נקלט לשנה זו)"
                                    : "") +
                                 (colOriginalYear < 0
                                    ? "\n\nאזהרה: עמודת \"שנה מקורית\" לא נמצאה בקובץ — " +
                                      "הנחות רטרואקטיביות עלולות להיקלט כהנחות השנה."
                                    : retroactiveSkipped > 0
                                        ? $"\n\nהנחות רטרואקטיביות שלא נקלטו: {retroactiveSkipped}" +
                                          $"\n(שנת מקור {string.Join(", ", retroactiveYears)} — אינן הנחות של {year})"
                                        : "");

                XtraMessageBox.Show(message, "תוצאות קליטה", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ReportMissingDiscountCodes();

                return true;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בקליטת קובץ — לא נקלט דבר:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// מעדכן חיוב שנתי לנכסים שקובץ החיוב לא מצא בזמנו — ייתכן ששורת השנה שלהם
        /// נוצרה מאז, בקליטת קובץ ההנחות.
        /// </summary>
        private void SyncBillingForNewClients(SqlConnection connection, int year)
        {
            try
            {
                string billingFolder = ManParameters.CurrentYearBillingPath;
                if (string.IsNullOrEmpty(billingFolder)) return;
                string billingNotFoundPath = Path.Combine(billingFolder, "חיוב_לא_נמצאו.xlsx");
                if (!File.Exists(billingNotFoundPath))
                    return;

                int synced = 0;

                using (var workbook = new ClosedXML.Excel.XLWorkbook(billingNotFoundPath))
                {
                    var ws = workbook.Worksheets.First();
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                    // The billing figures belong to the year row, so the year is
                    // part of the match now — the same asset has one row per year.
                    string updateSql = @"UPDATE tbYearlyDiscounts
                                         SET AnnualBilling = @billing,
                                             AssetSize     = @assetSize
                                         WHERE AssetNumber = @assetNumber
                                           AND Year_ = @year
                                           AND (AnnualBilling IS NULL OR AssetSize IS NULL)";

                    for (int row = 2; row <= lastRow; row++)
                    {
                        string assetNumber = ws.Cell(row, 1).GetString().Trim();
                        if (string.IsNullOrEmpty(assetNumber)) continue;

                        decimal.TryParse(ws.Cell(row, 2).GetString().Trim(), out decimal billing);
                        decimal.TryParse(ws.Cell(row, 3).GetString().Trim(), out decimal assetSize);

                        using (SqlCommand cmd = new SqlCommand(updateSql, connection))
                        {
                            cmd.Parameters.Add("@assetNumber", System.Data.SqlDbType.NVarChar, 20).Value = assetNumber;
                            cmd.Parameters.Add("@year", System.Data.SqlDbType.Int).Value = year;
                            cmd.Parameters.Add("@billing", System.Data.SqlDbType.Decimal).Value = billing;
                            cmd.Parameters.Add("@assetSize", System.Data.SqlDbType.Decimal).Value = assetSize;

                            if (cmd.ExecuteNonQuery() > 0)
                                synced++;
                        }
                    }
                }

                if (synced > 0)
                {
                    XtraMessageBox.Show($"עודכנו {synced} רשומות חיוב שנתי וגודל נכס מקובץ חיוב קודם.",
                        "סנכרון חיוב", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch { }
        }
        #endregion

        #region Reports
        private void barBtnSendSMS_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.SendSms)) return;

            if (barEditReport3Year.EditValue == null || string.IsNullOrWhiteSpace(barEditReport3Year.EditValue.ToString()))
            {
                XtraMessageBox.Show("יש להזין שנה לדוח.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int reportYear = Convert.ToInt32(barEditReport3Year.EditValue);

            DialogResult confirm = XtraMessageBox.Show(
                $"האם להפיק מכתבים לכל הלקוחות עם הנחה לא חוקית לשנת {reportYear}?",
                "אישור הפקת מכתבים", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var report = new CurrentYearDiscountAuditReport();
                string filePath = report.GenerateLetters(reportYear);

                if (filePath == null)
                {
                    XtraMessageBox.Show($"לא נמצאו הנחות לא חוקיות לשנת {reportYear}", "הודעה",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                XtraMessageBox.Show($"קובץ מכתבים נוצר בהצלחה:\n{filePath}", "הצלחה",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בהפקת מכתבים:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void barBtnRunReport3_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (barEditReport3Year.EditValue == null || string.IsNullOrWhiteSpace(barEditReport3Year.EditValue.ToString()))
            {
                XtraMessageBox.Show("יש להזין שנה לדוח.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int reportYear = Convert.ToInt32(barEditReport3Year.EditValue);

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var report = new CurrentYearDiscountAuditReport();
                string excelPath = report.GenerateReport(reportYear);

                if (excelPath == null)
                {
                    XtraMessageBox.Show($"לא נמצאו נתונים לשנת {reportYear}", "הודעה",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                XtraMessageBox.Show($"הדוח נוצר בהצלחה:\n{excelPath}", "הצלחה",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Process.Start(new ProcessStartInfo
                {
                    FileName = excelPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה ביצירת דוח:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void barBtnRunReport_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (barEditReportYear.EditValue == null || string.IsNullOrWhiteSpace(barEditReportYear.EditValue.ToString()))
            {
                XtraMessageBox.Show("יש להזין שנה לדוח.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int reportYear = Convert.ToInt32(barEditReportYear.EditValue);

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var report = new DiscountChecksReport();
                string excelPath = report.GenerateReport(reportYear);

                if (excelPath == null)
                {
                    XtraMessageBox.Show($"לא נמצאו נתונים לשנת {reportYear}", "הודעה",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                XtraMessageBox.Show($"הדוח נוצר בהצלחה:\n{excelPath}", "הצלחה",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Process.Start(new ProcessStartInfo
                {
                    FileName = excelPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה ביצירת דוח:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion



        private void barButSysTables_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.DiscountHeaders)) return;
            OpenChildForm<DiscountHeadersForm>();
        }

        private void barButDiscountCodes_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.DiscountsCodes)) return;
            OpenChildForm<frDiscountsCodes>();
        }

        private void barButCriterias_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.IncomeDiscountCriteria)) return;
            OpenChildForm<IncomeDiscountCriteriaForm>();
        }

        private void barButMinimumWage_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.MinimumWage)) return;
            OpenChildForm<MinimumWageForm>();
        }

        private void barButParameters_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.Parameters)) return;

            OpenChildForm<ManParametersForm>();
        }

        /// <summary>
        /// מסך בדיקת תקינות ההתקנה. פתוח לכל משתמש: הוא מדווח מצב בלבד — אילו
        /// עמודות קיימות, אילו תיקיות מוגדרות, כמה שורות חסרות תאריך — ואינו
        /// חושף סיסמאות או ערכי גישה. מנהל הקליטה הוא מי שצריך אותו.
        /// </summary>
        private void barButAdmin_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenChildForm<AdminForm>();
        }

        /// <summary>
        /// הגדרות החיבור למסד הנתונים — למנהלים בלבד, ולכן הכפתור אף אינו מוצג
        /// למשתמש רגיל (ראה ApplyAdminVisibility).
        /// </summary>
        private void barButConnection_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!PermissionManager.IsAdmin)
                return;

            using (var setup = new ConnectionSetupForm())
            {
                if (setup.ShowDialog(this) != DialogResult.OK)
                    return;
            }

            XtraMessageBox.Show(
                "ההגדרות נשמרו. יש להפעיל את התוכנה מחדש כדי שכל המסכים ישתמשו בחיבור החדש.",
                "הגדרות חיבור", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// הגדרות הדואר היוצא — למנהלים בלבד, מאותו טעם כמו הגדרות החיבור:
        /// מי שמשנה אותן יכול לשלוח דואר בשם העירייה.
        ///
        /// The screen reports its own result, so there is nothing to announce
        /// here after it closes.
        /// </summary>
        private void barButMailSetup_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!PermissionManager.IsAdmin)
                return;

            using var setup = new MailSetupForm();
            setup.ShowDialog(this);
        }

        /// <summary>
        /// מסתיר מהסרגל את מה שאינו למשתמש רגיל. מוסתר ולא חסום: כפתור שמופיע
        /// ואז מודיע "אין הרשאה" רק מבלבל.
        /// </summary>
        private void ApplyAdminVisibility()
        {
            var adminOnly = PermissionManager.IsAdmin
                ? DevExpress.XtraBars.BarItemVisibility.Always
                : DevExpress.XtraBars.BarItemVisibility.Never;

            barButConnection.Visibility = adminOnly;
            barButMailSetup.Visibility = adminOnly;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ApplyAdminVisibility();
            LoadYearDropdown();

            // Backup warning for administrators (BackupStatus): the shared
            // database is backed up weekly by the collection app.
            if (PermissionManager.IsAdmin)
                Shown += (s, ev) =>
                {
                    string warning = BackupStatus.WarningForLogin();
                    if (warning != null)
                        XtraMessageBox.Show(this, warning, "גיבוי מסד הנתונים",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                };
        }
        private void LoadYearDropdown()
        {
            barSubItemYear.ItemLinks.Clear();

            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT DISTINCT Year_ " +
                "FROM tbYearlyDiscounts " +
                "ORDER BY Year_ DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int year = reader.GetInt32(0);
                var item = new BarButtonItem();
                item.Caption = year.ToString();
                item.Tag = year;
                item.ItemClick += YearItem_Click;
                barSubItemYear.ItemLinks.Add(item);
            }

            // Default to most recent year from DB, not DateTime.Now.Year
            if (barSubItemYear.ItemLinks.Count > 0)
            {
                _selectedYear = (int)barSubItemYear.ItemLinks[0].Item.Tag;
            }

            barSubItemYear.Caption = _selectedYear.ToString();
        }

        private void YearItem_Click(object sender, ItemClickEventArgs e)
        {
            _selectedYear = (int)e.Item.Tag;
            barSubItemYear.Caption = _selectedYear.ToString();
        }

        private void barButReportPreviouseYear_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.Reports)) return;

            int reportYear = _selectedYear;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var report = new PreviousYearDiscountsReport();
                string excelPath = report.GenerateReport(reportYear);
                if (excelPath == null)
                {
                    XtraMessageBox.Show($"לא נמצאו נתונים לשנת {reportYear}", "הודעה",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                XtraMessageBox.Show($"הדוח נוצר בהצלחה:\n{excelPath}", "הצלחה",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start(new ProcessStartInfo
                {
                    FileName = excelPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה ביצירת דוח:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }

        /// <summary>
        /// דוח כפל הנחות — לקוחות שיש להם יותר מקוד הנחה אחד באותה שנה.
        ///
        /// Runs report_multiple_discounts.py rather than repeating its queries
        /// in C#. There was a second implementation here for an afternoon, and
        /// the two drifted apart within hours — including the same SQL bug in
        /// both. One report, one place; the button only launches it.
        /// </summary>
        private void barButMultiDiscounts_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.Reports)) return;

            if (!SessionData.SelectedYear.HasValue)
            {
                XtraMessageBox.Show("יש לבחור שנה בסרגל הכלים", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ManParameters.PythonScriptsPath))
            {
                XtraMessageBox.Show("נתיב סקריפטים Python לא הוגדר.", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string script = Path.Combine(ManParameters.PythonScriptsPath,
                                         "report_multiple_discounts.py");

            if (!File.Exists(script))
            {
                XtraMessageBox.Show($"קובץ Python לא נמצא:\n{script}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int year = SessionData.SelectedYear.Value;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{script}\" {year}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        XtraMessageBox.Show($"שגיאה בהפקת הדוח:\n{error}\n\n{output}", "שגיאה",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    XtraMessageBox.Show(output.Trim(), "דוח כפל הנחות",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // הדוח נכתב לתיקיית הדוחות והשורה האחרונה היא נתיב הקובץ.
                    string last = output.Trim().Split('\n')[^1].Trim();

                    if (last.StartsWith("Written: ", StringComparison.Ordinal))
                    {
                        string path = last.Substring("Written: ".Length).Trim();

                        if (File.Exists(path))
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = path,
                                UseShellExecute = true
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בהפעלת הדוח:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void barButReportCurrentYear_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.Reports)) return;

            // השנה שנבחרה בסרגל, לא שנת המערכת — כך אפשר להפיק את הדוח גם
            // לשנה קודמת, בלי הבחנה בין "שנה שוטפת" ל"שנה קודמת".
            if (!SessionData.SelectedYear.HasValue)
            {
                XtraMessageBox.Show("יש לבחור שנה בסרגל הכלים", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int reportYear = SessionData.SelectedYear.Value;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var report = new CurrentYearDiscountAuditReport();
                string excelPath = report.GenerateReport(reportYear);
                if (excelPath == null)
                {
                    XtraMessageBox.Show($"לא נמצאו נתונים לשנת {reportYear}", "הודעה",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                XtraMessageBox.Show($"הדוח נוצר בהצלחה:\n{excelPath}", "הצלחה",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start(new ProcessStartInfo
                {
                    FileName = excelPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה ביצירת דוח:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// אחרי קליטה: מאתר קודי הנחה שהגיעו בקובץ ואינם קיימים
        /// בטבלת הקודים, מדפיס אותם ל-PDF ופותח את מסך קודי ההנחה.
        ///
        /// The PDF is the working document: the codes are added by hand in
        /// frDiscountsCodes with the printout beside the keyboard, because each
        /// one has to be matched to the right statutory definition — a judgement
        /// the description makes possible and the number alone does not.
        /// </summary>
        private void ReportMissingDiscountCodes()
        {
            try
            {
                var report = new MissingDiscountCodesReport();
                var missing = report.Find();

                if (missing.Count == 0)
                    return;

                string pdfPath = report.Generate(missing);

                // The codes are listed in the box as well as in the PDF: with
                // 245 requests hanging off a single missing code, the number
                // itself is what tells the worker whether this is routine or
                // something that just broke a year's worth of grants.
                string codeList = string.Join(", ", missing.Select(m => m.Code));

                var answer = XtraMessageBox.Show(
                    $"נמצאו {missing.Count} קודי הנחה שאינם מוגדרים בטבלת הקודים:\n" +
                    $"{codeList}\n\n" +
                    $"נוצר קובץ:\n{pdfPath}\n\n" +
                    "האם להוסיף אותם לטבלת קודי ההנחה ולפתוח את המסך?\n" +
                    "הקודים יתווספו עם התאור מהקובץ וללא קטגוריה — יש להשלים אותה במסך.",
                    "קודי הנחה חסרים",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (answer != DialogResult.Yes)
                    return;

                int added = report.AddMissing(missing);
                XtraMessageBox.Show(
                    $"נוספו {added} קודים לטבלת קודי ההנחה.\n" +
                    "יש להשלים עבורם קטגוריה במסך קודי ההנחה.",
                    "קודי הנחה חסרים",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (!string.IsNullOrEmpty(pdfPath) && File.Exists(pdfPath))
                {
                    Process.Start(new ProcessStartInfo { FileName = pdfPath, UseShellExecute = true });
                }

                OpenChildForm<frDiscountsCodes>();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בבדיקת קודי הנחה:{Environment.NewLine}{ex.Message}",
                    "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButImportPreviouseYearData_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.ImportDiscounts)) return;

            // This import reads each row's own year out of the file, so the box
            // is a guard rather than a setting: the script is told which year is
            // expected and refuses a file that holds a different one. That is what
            // stops a 2026 file being loaded while the ribbon says 2027.
            if (!TryGetImportYear(barEditImportPrevYear, out int expectedYear))
                return;

            // User / identity-card guards
            if (!SessionData.HasUser)
            {
                XtraMessageBox.Show("יש להתחבר למערכת.", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(SessionData.UserIdentityCard))
            {
                XtraMessageBox.Show("למשתמש המחובר אין מספר תעודת זהות מוגדר.", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Python script guards
            if (string.IsNullOrWhiteSpace(ManParameters.PythonScriptsPath))
            {
                XtraMessageBox.Show("נתיב סקריפטים Python לא הוגדר.", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string pythonScript = Path.Combine(ManParameters.PythonScriptsPath, "import_discounts.py");
            if (!File.Exists(pythonScript))
            {
                XtraMessageBox.Show($"קובץ Python לא נמצא:\n{pythonScript}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Header-mapping guard — the Python script reads its column mapping from
            // tbDiscountHeaders, so an empty mapping table would fail mid-import.
            try
            {
                if (!HasHeaderMappings())
                {
                    XtraMessageBox.Show("לא הוגדרו מיפויי כותרות.\nנא להגדיר מיפוי כותרות לפני קליטת קובץ.",
                        "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (SqlException ex)
            {
                XtraMessageBox.Show($"שגיאה בגישה למסד הנתונים:\n{ex.Message}", "שגיאת מסד נתונים",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // File selection — defaults to the folder configured in tbMainParameters
            string? filePath = SelectImportFile(ManParameters.PreviousDiscountPath, "בחר קובץ הנחות שנה קודמת");
            if (filePath == null)
                return;

            // Confirm
            var confirm = XtraMessageBox.Show("האם לעבד את הקובץ?", "אישור עיבוד",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            // Run the Python import pipeline using the selected file as the source.
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{pythonScript}\" \"{filePath}\" \"{SessionData.UserIdentityCard}\" {ManParameters.DiscountExcelHeaderRowNum} --year={expectedYear}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        XtraMessageBox.Show($"קליטת נתוני קובץ הנחות הצליחה.\n{output}", "הצלחה",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Same check the current-year import runs. This path
                        // writes tbDiscountRequests too, so a code the code
                        // table does not know arrives here just as easily -
                        // it simply had nothing looking for it until now.
                        ReportMissingDiscountCodes();
                    }
                    else
                    {
                        XtraMessageBox.Show($"שגיאה בעיבוד הקובץ:\n{error}\n\n{output}", "שגיאה",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בהפעלת עיבוד הקובץ:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void barButImportCurrentYearData_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.ImportDiscounts)) return;

            // Year guard — the file is loaded into the year selected in the ribbon
            // The year is the one beside this button, NOT the system clock and
            // not another ribbon page: the file being loaded is chosen by hand,
            // and in January it is routinely last year's.
            if (!TryGetImportYear(barEditImportCurrYear, out int importYear))
                return;

            // File selection — defaults to the folder configured in tbMainParameters
            string? filePath = SelectImportFile(ManParameters.ActualCurrentYearDiscountPath, "בחר קובץ הנחות שנה נוכחית");
            if (filePath == null)
                return;

            // Reuse the shared core import logic
            if (!ImportCurrentYearDiscountFile(filePath, importYear))
                return;

            // Delete the source file once the import completed successfully
            DeleteImportedFile(filePath);
        }

        /// <summary>
        /// Deletes the source import file after a successful import.
        /// </summary>
        private void DeleteImportedFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    XtraMessageBox.Show($"קובץ הקליטה נמחק:\n{filePath}", "מחיקת קובץ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"לא ניתן למחוק את קובץ הקליטה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// קליטת קובץ חיוב שנתי — הקובץ המקורי כפי שהורד ממערכת העירייה.
        ///
        /// The file lists every property in the municipality, not only the ones
        /// receiving a discount, and it carries מספר לקוח / שם לקוח / שנה beside
        /// the billing figures. So a property with no year row is created here
        /// rather than being set aside: the citizen and the year row are made,
        /// with billing and area and no discount request. A citizen who gets no
        /// discount simply has a year row with no requests under it.
        ///
        /// Set-based on purpose: the file runs to ~13,000 lines, and a round trip
        /// per line would take minutes. The sheet is read in a single COM call,
        /// bulk-loaded into a temp table, and merged in four statements.
        /// </summary>
        private void barButImportBilling_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.ImportBilling)) return;

            // Year guard — used only for files that do not state their own year.
            // Used only for files that do not state their own year; taken from
            // the box beside this button.
            if (!TryGetImportYear(barEditImportBillingYear, out int ribbonYear))
                return;


            // File selection — defaults to the folder configured in tbMainParameters
            string? filePath = SelectImportFile(ManParameters.CurrentYearBillingPath, "בחר קובץ חיוב שנתי");
            if (filePath == null)
                return;

            var result = XtraMessageBox.Show(
                "האם לקלוט את קובץ החיוב השנתי?\nנכסים שאינם במערכת יתווספו.",
                "אישור עדכון", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            Cursor.Current = Cursors.WaitCursor;

            dynamic excel = null;
            dynamic wb = null;

            try
            {
                int newClients = 0, createdRows = 0, updatedRows = 0, invalidRows = 0;
                int assetsRepaired = 0, assetsStillSuspect = 0, assetCodeRows = 0;
                int skippedOtherCategory = 0, skippedUnknownKind = 0;
                var notFoundRows = new System.Collections.Generic.List<(string assetNumber, decimal billing, decimal assetSize)>();

                // Properties whose חיוב שנתי or גודל נכס is blank in the file. A blank
                // cell is staged as 0 and written as 0, so a good value already held
                // is replaced by one that means something else entirely: 0 שטח can
                // never exceed שטח מרבי, so no area cut-back is applied, and 0 חיוב
                // reads as "billed nothing" instead of "not billed yet". Captured
                // BEFORE anything is written — afterwards the old value is gone —
                // and handed to the operator as a workbook.
                var blankValueRows = new DataTable();

                // Open Excel via COM
                Type excelType = Type.GetTypeFromProgID("Excel.Application");
                excel = Activator.CreateInstance(excelType);
                excel.Visible = false;
                excel.DisplayAlerts = false;

                wb = excel.Workbooks.Open(Path.GetFullPath(filePath));
                dynamic ws = wb.Worksheets[1];
                dynamic usedRange = ws.UsedRange;

                int lastRow = (int)usedRange.Row + (int)usedRange.Rows.Count - 1;
                int lastCol = (int)usedRange.Column + (int)usedRange.Columns.Count - 1;

                // One COM call for the whole sheet. The array is 1-based and lines
                // up with the sheet's own row/column numbers.
                object[,] cells = (object[,])ws.Range[ws.Cells[1, 1], ws.Cells[lastRow, lastCol]].Value2;

                string Cell(int r, int c) =>
                    c <= 0 || c > lastCol || r > lastRow ? "" : (cells[r, c]?.ToString()?.Trim() ?? "");

                int headerRow = ManParameters.BllingExcelHeaderRowNum;
                int colAsset = -1, colBilling = -1, colAssetSize = -1;
                int colClient = -1, colClientName = -1, colYear = -1, colAssetCode = -1;

                for (int col = 1; col <= lastCol; col++)
                {
                    switch (Cell(headerRow, col))
                    {
                        case "מספר נכס":  colAsset = col;      break;
                        case "חיוב שנתי": colBilling = col;    break;
                        case "גודל נכס":  colAssetSize = col;  break;
                        case "מספר לקוח": colClient = col;     break;
                        case "שם לקוח":   colClientName = col; break;
                        case "שנה":       colYear = col;       break;

                        // The property's kind code — tbGAssetKinds.Code (100 מגורים,
                        // 110 מחסן, 300 מסחר …). It is what says whether a property
                        // is מגורים, which the eligibility check needs and nothing
                        // else in the file carries.
                        //
                        // The municipal export heads it "סוג נכס"; "קוד סוג נכס" is
                        // accepted too, because both spellings have been seen and a
                        // heading that does not match makes the whole import refuse.
                        case "סוג נכס":
                        case "קוד סוג נכס":
                            colAssetCode = col;
                            break;
                    }
                }

                if (colAsset == -1 || colBilling == -1 || colAssetSize == -1)
                {
                    XtraMessageBox.Show(
                        "הקובץ חייב לכלול עמודות:\n\"מספר נכס\", \"חיוב שנתי\" ו-\"גודל נכס\"",
                        "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // רק נכסי מגורים נקלטים, ולכן בלי קוד סוג הנכס אי אפשר להחליט על
                // אף שורה. עצירה מפורשת, ולא קליטה שקטה של הכול או של כלום.
                if (colAssetCode == -1)
                {
                    XtraMessageBox.Show(
                        "הקובץ חייב לכלול עמודת \"סוג נכס\" (או \"קוד סוג נכס\").\n\n" +
                        "נקלטים רק נכסי מגורים, ובלי הקוד לא ניתן לדעת אילו שורות הן מגורים.",
                        "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // ── the sheet, as rows ready for the database ──
                var staged = new DataTable();
                staged.Columns.Add("AssetNumber", typeof(string));
                staged.Columns.Add("ClientNumber", typeof(string));
                staged.Columns.Add("ClientName", typeof(string));
                staged.Columns.Add("Year_", typeof(int));
                staged.Columns.Add("AnnualBilling", typeof(decimal));
                staged.Columns.Add("AssetSize", typeof(decimal));
                staged.Columns.Add("AssetCode", typeof(int));

                for (int row = headerRow + 1; row <= lastRow; row++)
                {
                    string assetNumber = Cell(row, colAsset);
                    if (string.IsNullOrEmpty(assetNumber))
                        continue;

                    // AssetNumber is nvarchar(20); anything longer is not a property
                    // number but stray text, and truncating it would invent a row.
                    if (assetNumber.Length > 20)
                    {
                        invalidRows++;
                        continue;
                    }

                    // The file states the year on every line. Lines without one are
                    // the report's own header/footer text, not properties.
                    int rowYear;
                    if (colYear > 0)
                    {
                        if (!int.TryParse(Cell(row, colYear), out rowYear) || rowYear < 2000 || rowYear > 2100)
                        {
                            invalidRows++;
                            continue;
                        }
                    }
                    else
                    {
                        rowYear = ribbonYear;
                    }

                    decimal.TryParse(Cell(row, colBilling), out decimal billing);
                    decimal.TryParse(Cell(row, colAssetSize), out decimal assetSize);

                    string clientNumber = colClient > 0 ? Cell(row, colClient) : "";
                    string clientName = colClientName > 0 ? Cell(row, colClientName) : "";

                    // A blank or non-numeric kind code stays NULL rather than
                    // becoming 0 — 0 is a real code (ברירת מחדל) and would be a
                    // claim the file never made.
                    object assetCode = DBNull.Value;
                    if (colAssetCode > 0 && int.TryParse(Cell(row, colAssetCode), out int parsedCode))
                    {
                        assetCode = parsedCode;
                        assetCodeRows++;
                    }

                    staged.Rows.Add(
                        assetNumber,
                        clientNumber.Length > 0 ? clientNumber.Substring(0, Math.Min(20, clientNumber.Length)) : (object)DBNull.Value,
                        clientName.Length > 0 ? clientName.Substring(0, Math.Min(255, clientName.Length)) : (object)DBNull.Value,
                        rowYear, billing, assetSize, assetCode);
                }

                if (staged.Rows.Count == 0)
                {
                    XtraMessageBox.Show("לא נמצאו שורות לקליטה בקובץ.", "הודעה",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var create = new SqlCommand(@"
                                CREATE TABLE #billing (
                                    AssetNumber   nvarchar(20)  NOT NULL,
                                    ClientNumber  nvarchar(20)  NULL,
                                    ClientName    nvarchar(255) NULL,
                                    Year_         int           NOT NULL,
                                    AnnualBilling decimal(18,2) NULL,
                                    AssetSize     decimal(10,2) NULL,
                                    AssetCode     int           NULL);", conn, tx))
                            {
                                create.ExecuteNonQuery();
                            }

                            using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tx)
                            {
                                DestinationTableName = "#billing",
                                BulkCopyTimeout = 300
                            })
                            {
                                foreach (DataColumn c in staged.Columns)
                                    bulk.ColumnMappings.Add(c.ColumnName, c.ColumnName);
                                bulk.WriteToServer(staged);
                            }

                            // The file repeats a property once per billing period, so
                            // collapse to one line per citizen + property + year.
                            using (var collapse = new SqlCommand(@"
                                SELECT AssetNumber, ClientNumber, Year_,
                                       MAX(ClientName)    AS ClientName,
                                       MAX(AnnualBilling) AS AnnualBilling,
                                       MAX(AssetSize)     AS AssetSize,
                                       MAX(AssetCode)     AS AssetCode
                                INTO   #b
                                FROM   #billing
                                GROUP  BY AssetNumber, ClientNumber, Year_;", conn, tx))
                            {
                                collapse.ExecuteNonQuery();
                            }

                            // 0a. only מגורים. The kind code is matched against
                            //     tbGAssetKinds, and everything else — מסחר, תעשיה,
                            //     קרקע, מוסדות, and any code with no category or no
                            //     row at all — is dropped before a single row is
                            //     written. Counted first so the summary can say what
                            //     the file held and what was left out, rather than
                            //     the numbers quietly not adding up.
                            using (var countSkipped = new SqlCommand(@"
                                SELECT SUM(CASE WHEN k.Code IS NOT NULL
                                                 AND LTRIM(RTRIM(ISNULL(k.Category, ''))) <> ''
                                                THEN 1 ELSE 0 END),
                                       SUM(CASE WHEN k.Code IS NULL
                                                  OR LTRIM(RTRIM(ISNULL(k.Category, ''))) = ''
                                                THEN 1 ELSE 0 END)
                                FROM   #b b
                                LEFT   JOIN tbGAssetKinds k ON k.Code = b.AssetCode
                                WHERE  ISNULL(k.Category, N'') <> @residential;", conn, tx))
                            {
                                countSkipped.Parameters.Add("@residential", SqlDbType.NVarChar, 255).Value = ResidentialCategory;
                                using (var reader = countSkipped.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        skippedOtherCategory = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                        skippedUnknownKind   = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                                    }
                                }
                            }

                            using (var keepResidential = new SqlCommand(@"
                                DELETE b
                                FROM   #b b
                                LEFT   JOIN tbGAssetKinds k ON k.Code = b.AssetCode
                                WHERE  ISNULL(k.Category, N'') <> @residential;", conn, tx))
                            {
                                keepResidential.Parameters.Add("@residential", SqlDbType.NVarChar, 255).Value = ResidentialCategory;
                                keepResidential.ExecuteNonQuery();
                            }

                            // 0. repair asset numbers the municipal export mangled.
                            //    Long property numbers come out of the billing file in
                            //    scientific notation — 192670530010 arrives as
                            //    1.92671E+11, i.e. rounded to six significant digits.
                            //    The digits are gone from the file, but the citizen's
                            //    number survived, so a property we already hold for that
                            //    citizen which rounds to the same value is the original.
                            //    Only an unambiguous single candidate is accepted.
                            using (var repair = new SqlCommand(@"
                                UPDATE b
                                SET    b.AssetNumber = fix.AssetNumber
                                FROM   #b b
                                INNER  JOIN tbClients c ON c.ClientNumber = b.ClientNumber
                                CROSS  APPLY (
                                    SELECT MIN(y.AssetNumber) AS AssetNumber, COUNT(*) AS n
                                    FROM   tbYearlyDiscounts y
                                    WHERE  y.ClientId = c.ID
                                      AND  y.AssetNumber <> b.AssetNumber
                                      AND  y.AssetNumber NOT LIKE '%[^0-9]%'
                                      AND  LEN(y.AssetNumber) = LEN(b.AssetNumber)
                                      AND  ROUND(CAST(y.AssetNumber AS float) / 1000000.0, 0) * 1000000
                                           = CAST(b.AssetNumber AS float)
                                ) fix
                                WHERE  b.ClientNumber IS NOT NULL
                                  AND  fix.n = 1
                                  AND  LEN(b.AssetNumber) >= 12
                                  AND  b.AssetNumber LIKE '%000000'
                                  AND  b.AssetNumber NOT LIKE '%[^0-9]%';", conn, tx))
                            {
                                assetsRepaired = repair.ExecuteNonQuery();
                            }

                            using (var suspect = new SqlCommand(@"
                                SELECT COUNT(*) FROM #b
                                WHERE  LEN(AssetNumber) >= 12
                                  AND  AssetNumber LIKE '%000000'
                                  AND  AssetNumber NOT LIKE '%[^0-9]%';", conn, tx))
                            {
                                assetsStillSuspect = Convert.ToInt32(suspect.ExecuteScalar());
                            }

                            // 1. citizens the file brings in
                            using (var addClients = new SqlCommand(@"
                                INSERT INTO tbClients (ClientNumber, ClientName)
                                SELECT b.ClientNumber, MAX(b.ClientName)
                                FROM   #b b
                                WHERE  b.ClientNumber IS NOT NULL
                                  AND  NOT EXISTS (SELECT 1 FROM tbClients c
                                                   WHERE c.ClientNumber = b.ClientNumber)
                                GROUP BY b.ClientNumber;", conn, tx))
                            {
                                newClients = addClients.ExecuteNonQuery();
                            }

                            // 2. fill a name we did not have — never overwrite one we do
                            using (var fillNames = new SqlCommand(@"
                                UPDATE c SET c.ClientName = x.ClientName
                                FROM   tbClients c
                                INNER  JOIN (SELECT ClientNumber, MAX(ClientName) AS ClientName
                                             FROM #b WHERE ClientName IS NOT NULL
                                             GROUP BY ClientNumber) x
                                        ON x.ClientNumber = c.ClientNumber
                                WHERE  c.ClientName IS NULL;", conn, tx))
                            {
                                fillNames.ExecuteNonQuery();
                            }

                            // 2a. what the file is about to CHANGE on a property we
                            //     already hold. Runs before steps 3-5 write anything,
                            //     so the "קיים" columns still hold the pre-import
                            //     values.
                            //
                            //     A property is here only when the system already had
                            //     the value the file is about to move: a property that
                            //     was never billed, and one arriving for the first
                            //     time, have nothing to compare against and are not
                            //     listed. INNER JOIN is what enforces the first of
                            //     those; every condition tests the existing value for
                            //     the second.
                            using (var blanks = new SqlCommand(@"
                                SELECT b.ClientNumber, c.ClientName, b.Year_, b.AssetNumber,
                                       b.AnnualBilling AS FileBilling,
                                       b.AssetSize     AS FileSize,
                                       b.AssetCode     AS FileCode,
                                       y.ID            AS YearlyId,
                                       y.AnnualBilling AS ExistingBilling,
                                       y.AssetSize     AS ExistingSize,
                                       y.AssetCode     AS ExistingCode
                                FROM   #b b
                                INNER  JOIN tbClients c ON c.ClientNumber = b.ClientNumber
                                INNER  JOIN tbYearlyDiscounts y ON y.ClientId    = c.ID
                                                               AND y.Year_       = b.Year_
                                                               AND y.AssetNumber = b.AssetNumber
                                WHERE  -- a billing we HAD is wiped out by an empty cell
                                       (ISNULL(b.AnnualBilling, 0) = 0 AND ISNULL(y.AnnualBilling, 0) > 0)
                                       -- an area we HAD is wiped out by an empty cell
                                   OR  (ISNULL(b.AssetSize, 0) = 0 AND ISNULL(y.AssetSize, 0) > 0)
                                       -- the kind code is not the one on record
                                   OR  (b.AssetCode IS NOT NULL
                                        AND ISNULL(y.AssetCode, -1) <> b.AssetCode)
                                       -- the property got smaller
                                   OR  (b.AssetSize > 0 AND ISNULL(y.AssetSize, 0) > 0
                                        AND b.AssetSize < y.AssetSize)
                                       -- the property got cheaper
                                   OR  (b.AnnualBilling > 0 AND ISNULL(y.AnnualBilling, 0) > 0
                                        AND b.AnnualBilling < y.AnnualBilling)
                                ORDER BY b.ClientNumber, b.AssetNumber;", conn, tx))
                            using (var blanksAdapter = new SqlDataAdapter(blanks))
                            {
                                blanksAdapter.Fill(blankValueRows);
                            }

                            // 3. year rows for properties not in the system yet —
                            //    billing and area, no discount request
                            using (var addRows = new SqlCommand(@"
                                INSERT INTO tbYearlyDiscounts (ClientId, Year_, AssetNumber, AnnualBilling, AssetSize, AssetCode)
                                SELECT c.ID, b.Year_, b.AssetNumber, b.AnnualBilling, b.AssetSize, b.AssetCode
                                FROM   #b b
                                INNER  JOIN tbClients c ON c.ClientNumber = b.ClientNumber
                                WHERE  b.ClientNumber IS NOT NULL
                                  AND  NOT EXISTS (SELECT 1 FROM tbYearlyDiscounts y
                                                   WHERE y.ClientId = c.ID
                                                     AND y.Year_ = b.Year_
                                                     AND y.AssetNumber = b.AssetNumber);", conn, tx))
                            {
                                createdRows = addRows.ExecuteNonQuery();
                            }

                            // 4. the figures themselves, on rows that were already there
                            using (var setBilling = new SqlCommand(@"
                                UPDATE y
                                SET    y.AnnualBilling = b.AnnualBilling,
                                       y.AssetSize     = b.AssetSize,
                                       -- a file that omits the kind code must not
                                       -- erase a code we already hold
                                       y.AssetCode     = ISNULL(b.AssetCode, y.AssetCode)
                                FROM   tbYearlyDiscounts y
                                INNER  JOIN tbClients c ON c.ID = y.ClientId
                                INNER  JOIN #b b ON b.ClientNumber = c.ClientNumber
                                                AND b.Year_        = y.Year_
                                                AND b.AssetNumber  = y.AssetNumber
                                WHERE  b.ClientNumber IS NOT NULL;", conn, tx))
                            {
                                updatedRows = Math.Max(0, setBilling.ExecuteNonQuery() - createdRows);
                            }

                            // 5. lines with no client number — nothing to create a
                            //    citizen from, so they can only update by property
                            using (var noClient = new SqlCommand(@"
                                UPDATE y
                                SET    y.AnnualBilling = b.AnnualBilling,
                                       y.AssetSize     = b.AssetSize,
                                       y.AssetCode     = ISNULL(b.AssetCode, y.AssetCode)
                                FROM   tbYearlyDiscounts y
                                INNER  JOIN #b b ON b.AssetNumber = y.AssetNumber
                                                AND b.Year_       = y.Year_
                                WHERE  b.ClientNumber IS NULL;", conn, tx))
                            {
                                updatedRows += noClient.ExecuteNonQuery();
                            }

                            using (var stillMissing = new SqlCommand(@"
                                SELECT b.AssetNumber, b.AnnualBilling, b.AssetSize
                                FROM   #b b
                                WHERE  b.ClientNumber IS NULL
                                  AND  NOT EXISTS (SELECT 1 FROM tbYearlyDiscounts y
                                                   WHERE y.AssetNumber = b.AssetNumber
                                                     AND y.Year_ = b.Year_);", conn, tx))
                            using (var reader = stillMissing.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    notFoundRows.Add((
                                        reader.GetString(0),
                                        reader.IsDBNull(1) ? 0m : reader.GetDecimal(1),
                                        reader.IsDBNull(2) ? 0m : reader.GetDecimal(2)));
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

                // Only lines with no client number can still be unplaceable
                if (notFoundRows.Count > 0)
                {
                    SaveBillingNotFound(notFoundRows);
                }

                // Blank billing or area: put in front of the operator at once, since
                // the file has to be corrected and those properties imported again.
                if (blankValueRows.Rows.Count > 0)
                {
                    SaveBillingBlankValues(blankValueRows);
                }

                XtraMessageBox.Show(
                    $"קליטת חיוב שנתי הושלמה!\n\n" +
                    $"עודכנו: {updatedRows} רשומות\n" +
                    $"נוספו נכסים חדשים: {createdRows}\n" +
                    $"לקוחות חדשים שנוספו: {newClients}\n" +
                    $"שורות שאינן נכסים: {invalidRows}\n" +
                    $"מספרי נכס שתוקנו: {assetsRepaired}\n" +
                    $"מספרי נכס פגומים שלא ניתן לתקן: {assetsStillSuspect}\n" +
                    $"שורות עם קוד סוג נכס: {assetCodeRows}\n" +
                    $"נכסים שאינם מגורים — לא נקלטו: {skippedOtherCategory}\n" +
                    $"נכסים ללא סיווג — לא נקלטו: {skippedUnknownKind}\n" +
                    $"ללא מספר לקוח ולא נמצאו: {notFoundRows.Count} (נשמרו לקובץ)\n" +
                    $"ערכים חסרים ושינויים: {blankValueRows.Rows.Count} (נשמרו לקובץ שנפתח)",
                    "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בקליטת חיוב שנתי — לא נקלט דבר:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Always close Excel COM
                try { if (wb != null) { wb.Close(false); Marshal.ReleaseComObject(wb); } } catch { }
                try { if (excel != null) { excel.Quit(); Marshal.ReleaseComObject(excel); } } catch { }
                GC.Collect();
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Opens the file-explorer dialog for an import button. The dialog starts in
        /// <paramref name="defaultFolder"/> — the folder configured for that import in
        /// tbMainParameters (via <see cref="ManParameters"/>) — when it exists on disk.
        /// Returns the selected file path, or null if the user cancelled.
        /// </summary>
        private string? SelectImportFile(string? defaultFolder, string title)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "Excel Files (*.xls;*.xlsx;*.xlsm)|*.xls;*.xlsx;*.xlsm|All Files (*.*)|*.*";
                openDialog.Title = title;

                if (!string.IsNullOrWhiteSpace(defaultFolder) && Directory.Exists(defaultFolder))
                    openDialog.InitialDirectory = defaultFolder;

                return openDialog.ShowDialog() == DialogResult.OK ? openDialog.FileName : null;
            }
        }

        /// <summary>
        /// True when tbDiscountHeaders holds at least one Excel column mapping.
        /// </summary>
        private bool HasHeaderMappings()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM tbDiscountHeaders", conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        private void barButUserPermissions_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!CheckPermission(PermissionManager.UserManagement)) return;
            OpenChildForm<UserPermissionsForm>();
        }
    }
}
