using System;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using MunicipalDiscount.Classes;

namespace MunicipalDiscount
{
    public partial class frDiscountsCodes : DevExpress.XtraEditors.XtraForm
    {
        private string connectionString = DatabaseConfig.ConnectionString;

        // המסך הוא אב/בן: הגדרת הקוד למעלה, גרסאות התעריף שלו למטה.
        // A code no longer has one rate — it has a rate per ValidityDate, and the
        // rate that applies to a citizen is the version in force on their request
        // date. So the percentages left tbDiscountsCodes and live in
        // tbDiscountCodeRates, one row per change the municipality makes.
        // ── עמודות המסך מול עמודות הטבלאות ────────────────────────────────
        //
        // הפאנל הימני — "קודי הנחה"  →  tbDiscountsCodes  (grid: gridView1)
        //
        //   כותרת במסך        עמודת הרשת (GridColumn)       עמודה בטבלה
        //   ────────────────  ───────────────────────────  ─────────────────────────
        //   בחר               colIsSelected                — (עמודת סימון בזיכרון בלבד)
        //   קטגוריה           colCategories                Categories
        //   קוד קבוצה         colGroupCode                 GroupCode
        //   קוד ת.גביה        colMunicipalDiscountCode     MunicipalDiscountCode
        //   קוד הנחה          colDiscountCode              LegallDiscountCode   ← שים לב לשם
        //   תאור הנחה         colDiscountName              DiscountName
        //   חובה/רשות         colMandatory                 Mandatory
        //
        //   שתי עמודות קיימות ב-.Designer.cs אך אינן מוצגות — לא נקבע להן
        //   Visible/VisibleIndex, ולכן הן יושבות ב-Column Chooser בלבד:
        //      colID        → ID (identity)
        //      colDocuments → Documents
        //
        //   ארבעת שדות הטקסט הארוכים אינם נערכים ברשת אלא בכפתורי הסרגל, דרך
        //   frMemoEdit; OpenMemoForm קורא וכותב ישירות ל-tbDiscountsCodes לפי
        //   focusedRowId:
        //      מסמכים נדרשים → Documents,  הסברים → Remark,
        //      מקור החוק      → LawInForce,
        //      כללים        → Rules,       תנאים  → Conditions
        //   Documents הוא היחיד שיש לו גם עמודה ברשת — והיא המוסתרת שלמעלה.
        //
        // הפאנל השמאלי — "גרסאות תעריף"  →  tbDiscountCodeRates  (grid: gridViewRates)
        //
        //   בתוקף מתאריך      ValidityDate                 ValidityDate
        //   אחוז הנחה         DiscountPercent              DiscountPercent
        //   אחוז הנחה קודם    PreviousDiscountPercent      PreviousDiscountPercent
        //   מקסימום הנחה      MaximumDiscount              MaximumDiscount
        //   מקסימום שטח       MaximumArea                  MaximumArea
        //   (מוסתרות)         ID / DiscountCodeId          ID / DiscountCodeId → tbDiscountsCodes.ID
        //
        //   האחוזים הם שברים (0.40 = 40%) ומוצגים ב-P0.
        //
        // העמודות שעברו לטבלת התעריפים (שיעור הנחה, אחוז הנחה קודם, מקסימום
        // שיעור, מקסימום שטח) והעמודה "שנה" נמחקו מהדיזיינר: אין להן יותר
        // עמודה ב-tbDiscountsCodes. התעריפים מוצגים בפאנל השמאלי.
        // ──────────────────────────────────────────────────────────────────

        private readonly DataSet _data = new DataSet();
        private DataTable dtDiscountsCodes = new DataTable("Codes");
        private DataTable dtRates = new DataTable("Rates");
        private const string RatesRelation = "CodeRates";

        private int focusedRowId = -1;
        private bool _isDirty = false;
        private bool _selectAllChecked = false;
        private static readonly string LayoutDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MunicipalDiscount");
        // _v2: the layout saved by the single-table version of this form stores a
        // Visible flag per column, so restoring it brought the retired percentage
        // and year columns back. A new name retires that file instead of fighting it.
        private static readonly string LayoutFilePath = Path.Combine(LayoutDir, "frDiscountsCodes_Layout_v2.xml");
        private static readonly string FormStatePath = Path.Combine(LayoutDir, "frDiscountsCodes_FormState.txt");

        public frDiscountsCodes()
        {
            InitializeComponent();
            ThemeHelper.ApplyTheme(this);
            this.IconOptions.SvgImage = MunicipalDiscount.Classes.AppIcons.DiscountCodes;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;
            // "בתוקף מתאריך" is typed, not picked. The designer left the editor
            // with no EditMask, so it fell back to free-text parsing and a
            // calendar drop-down — fine for choosing a day, tiresome for the
            // only two dates this grid ever holds (01/01 of one year or the
            // next). A DateTimeAdvancingCaret mask lets the whole date be typed
            // as digits, 010126 filling itself in as 01/01/26 and moving on.
            //
            // Set here rather than in the designer so re-opening the form in
            // Visual Studio cannot quietly drop it.
            repositoryItemRateDate.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.DateTimeAdvancingCaret;
            repositoryItemRateDate.Mask.EditMask = "dd/MM/yy";
            repositoryItemRateDate.Mask.UseMaskAsDisplayFormat = true;
            repositoryItemRateDate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            repositoryItemRateDate.DisplayFormat.FormatString = "dd/MM/yy";
            repositoryItemRateDate.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            repositoryItemRateDate.EditFormat.FormatString = "dd/MM/yy";


            // Set up category combobox purely at runtime
            var combo = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            combo.Name = "repositoryItemComboCategories";
            combo.AutoHeight = false;
            combo.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
            gridControl1.RepositoryItems.Add(combo);
            colCategories.ColumnEdit = combo;
            colCategories.OptionsColumn.AllowEdit = true;
            repositoryItemComboCategories = combo;

            // "Select all" checkbox drawn inside the "בחר" column header
            colIsSelected.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            colIsSelected.ToolTip = "סימון / ביטול סימון של כל השורות המוצגות";
            gridView1.CustomDrawColumnHeader += gridView1_CustomDrawColumnHeader;
            gridView1.RowCountChanged += (s, e2) => UpdateSelectAllState();

            // Row counter on the status bar

            // Toggle checkbox on single click
            gridView1.Click += (s, e2) =>
            {
                var hi = gridView1.CalcHitInfo(gridControl1.PointToClient(
                    System.Windows.Forms.Control.MousePosition));

                if (hi.HitTest == DevExpress.XtraGrid.Views.Grid.ViewInfo.GridHitTest.Column
                    && hi.Column == colIsSelected)
                {
                    SetSelectionForAllRows(!_selectAllChecked);
                    return;
                }

                if (hi.InRow && hi.Column == colIsSelected)
                {
                    var cur = gridView1.GetRowCellValue(hi.RowHandle, colIsSelected);
                    bool current = cur != null && cur != DBNull.Value && cur is bool bv && bv;
                    SetRowSelection(hi.RowHandle, !current);
                    UpdateSelectAllState();
                }
            };

            // Open combo dropdown on single click
            gridView1.ShownEditor += (s, e2) =>
            {
                if (gridView1.FocusedColumn == colCategories)
                {
                    var edit = gridView1.ActiveEditor as DevExpress.XtraEditors.ComboBoxEdit;
                    edit?.ShowPopup();
                }
            };

            // The year filter and "copy year" belong to the old one-row-per-year
            // model. Copying a year is what the dated rates replaced: a mid-year
            // change is one new rate version, and a new year needs nothing at all.
            barEditYear.Visibility = BarItemVisibility.Never;
            btnCopyCodes.Visibility = BarItemVisibility.Never;


            this.FormClosing += frDiscountsCodes_FormClosing;
            RestoreFormState();

            LoadCategories();
            LoadData();
        }

        // ── שני הפאנלים: הקודים מימין וגרסאות התעריף משמאל ─────────────
        //
        //    המפצל, שתי הקבוצות, רשת גרסאות התעריף על עמודותיה ושני כפתורי
        //    האקסל יושבים ב-frDiscountsCodes.Designer.cs. קודם הם נבנו בזמן
        //    ריצה, ולכן בדיזיינר נראתה רשת אחת בלבד.
        //
        //    כאן נשארו רק המטפלים באירועים של רשת הגרסאות.
        private void gridViewRates_CellValueChanged(object sender,
            DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            _isDirty = true;
        }

        private void gridViewRates_InitNewRow(object sender,
            DevExpress.XtraGrid.Views.Grid.InitNewRowEventArgs e)
        {
            // גרסה חדשה מתחילה בתאריך של היום; התאריך הוא שבוחר אותה.
            gridViewRates.SetRowCellValue(e.RowHandle, colRateValidityDate, DateTime.Today);
            _isDirty = true;
        }

        private void gridViewRates_RowDeleted(object sender,
            DevExpress.Data.RowDeletedEventArgs e)
        {
            _isDirty = true;
        }


        /// <summary>
        /// מציג את גרסאות התעריף של הקוד שנבחר.
        ///
        /// CreateChildView is what keeps the two sides honest: rows added on the
        /// left get the selected code's id automatically, so a new version cannot
        /// end up attached to the wrong code.
        /// </summary>
        private void ShowRatesForFocusedCode()
        {
            if (gridRates == null) return;

            var drv = gridView1.GetRow(gridView1.FocusedRowHandle) as DataRowView;
            if (drv == null || !_data.Relations.Contains(RatesRelation))
            {
                gridRates.DataSource = null;
                grpRates.Text = "גרסאות תעריף";
                return;
            }

            gridRates.DataSource = drv.CreateChildView(RatesRelation);

            string name = drv["DiscountName"] == DBNull.Value ? "" : drv["DiscountName"].ToString();
            grpRates.Text = string.IsNullOrWhiteSpace(name)
                ? "גרסאות תעריף"
                : $"גרסאות תעריף — {name}";
        }

        /// <summary>
        /// מריץ סקריפט Python מתיקיית הסקריפטים ומחזיר את הפלט.
        /// PYTHONIOENCODING is forced to UTF-8, otherwise the Hebrew messages come
        /// back through the pipe in the console codepage and arrive as gibberish.
        /// </summary>
        private (int exitCode, string output) RunPythonScript(string scriptName, string arguments)
        {
            if (string.IsNullOrWhiteSpace(ManParameters.PythonScriptsPath))
                return (-1, "נתיב סקריפטים Python לא הוגדר בפרמטרים.");

            string script = Path.Combine(ManParameters.PythonScriptsPath, scriptName);
            if (!File.Exists(script))
                return (-1, $"קובץ Python לא נמצא:\n{script}");

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{script}\" {arguments}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

            using var process = System.Diagnostics.Process.Start(psi);
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            string combined = (stdout + (string.IsNullOrWhiteSpace(stderr) ? "" : "\n" + stderr)).Trim();
            return (process.ExitCode, combined);
        }

        /// <summary>Keeps a long script output readable inside a message box.</summary>
        private static string Tail(string text, int lines)
        {
            var all = (text ?? "").Replace("\r\n", "\n").Split('\n');
            if (all.Length <= lines) return text;
            return "…\n" + string.Join("\n", all.Skip(all.Length - lines));
        }

        private void btnExportCodesToExcel_ItemClick(object sender, ItemClickEventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "בחר תיקייה לשמירת קובצי הקודים והתעריפים",
                UseDescriptionForTitle = true
            };
            string home = CodeWorkbookFolder();
            if (home != null)
                dlg.SelectedPath = home;

            if (dlg.ShowDialog() != DialogResult.OK) return;

            string codesPath = Path.Combine(dlg.SelectedPath, "DiscountCodes.xlsx");
            string ratesPath = Path.Combine(dlg.SelectedPath, "DiscountRates.xlsx");

            // The export always overwrites these two names.
            if (File.Exists(codesPath) || File.Exists(ratesPath))
            {
                if (XtraMessageBox.Show(
                        "בתיקייה כבר קיימים DiscountCodes.xlsx או DiscountRates.xlsx.\nלהחליף אותם?",
                        "קבצים קיימים", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
            }

            Cursor.Current = Cursors.WaitCursor;
            var (exitCode, output) = RunPythonScript("export_discount_codes.py",
                $"\"{codesPath}\" \"{ratesPath}\"");
            Cursor.Current = Cursors.Default;

            if (exitCode != 0)
            {
                XtraMessageBox.Show($"הייצוא נכשל:\n\n{Tail(output, 15)}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (XtraMessageBox.Show(
                    $"{output}\n\nלפתוח את התיקייה?", "הייצוא הושלם",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = dlg.SelectedPath,
                        UseShellExecute = true
                    });
                }
                catch { /* the files are written either way */ }
            }
        }

        private void btnImportCodesFromExcel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!PermissionManager.HasPermission(PermissionManager.CopyDiscounts))
            {
                XtraMessageBox.Show("אין לך הרשאה לביצוע פעולה זו", "הרשאה נדרשת",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isDirty && XtraMessageBox.Show(
                    "יש שינויים שלא נשמרו והם יאבדו בקליטה. להמשיך?", "שינויים לא שמורים",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            if (!PickWorkbookPair(out string codesPath, out string ratesPath))
                return;

            // Validate first — nothing is written by a dry run.
            Cursor.Current = Cursors.WaitCursor;
            var (checkCode, checkOutput) = RunPythonScript("import_discount_codes.py",
                $"\"{codesPath}\" \"{ratesPath}\" --dry-run");
            Cursor.Current = Cursors.Default;

            if (checkCode != 0)
            {
                XtraMessageBox.Show(
                    $"הקבצים אינם תקינים ולא נקלט דבר:\n\n{Tail(checkOutput, 20)}",
                    "בדיקת הקבצים נכשלה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string summary = string.Join("\n", (checkOutput ?? "").Replace("\r\n", "\n").Split('\n')
                .Where(l => l.StartsWith("Parsed") || l.StartsWith("WARNING")
                            || l.Contains("data rows")));

            int existing = CountCodes();
            if (XtraMessageBox.Show(
                    $"{summary}\n\n{existing} הקודים הקיימים במערכת יימחקו ויוחלפו בתוכן הקבצים.\n" +
                    "כל גרסאות התעריף שלהם יימחקו יחד איתם.\n\nלהמשיך?",
                    "אישור קליטה", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            Cursor.Current = Cursors.WaitCursor;
            var (loadCode, loadOutput) = RunPythonScript("import_discount_codes.py",
                $"\"{codesPath}\" \"{ratesPath}\" --replace");
            Cursor.Current = Cursors.Default;

            if (loadCode != 0)
            {
                XtraMessageBox.Show(
                    $"הקליטה נכשלה ולא נקלט דבר:\n\n{Tail(loadOutput, 20)}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _isDirty = false;
            LoadCategories();
            LoadData();

            XtraMessageBox.Show($"{Tail(loadOutput, 6)}", "הקליטה הושלמה",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// בוחר את שני הקבצים יחד ומזהה מי מהם קובץ התעריפים.
        ///
        /// Both workbooks are chosen in one dialog, and which is which is decided
        /// by looking inside rather than by the file name: only the rates sheet
        /// has a "תאריך תחילת תוקף" column. So the pair can be renamed per
        /// municipality, and picking them in either order still works.
        /// Returns false when the user cancels or the two files are not a pair.
        /// </summary>
        private bool PickWorkbookPair(out string codesPath, out string ratesPath)
        {
            codesPath = ratesPath = null;

            using var dlg = new OpenFileDialog
            {
                Title = "בחר את שני הקבצים: קודי הנחה ותעריפי הנחה",
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                Multiselect = true
            };

            string folder = CodeWorkbookFolder();
            if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
                dlg.InitialDirectory = folder;

            if (dlg.ShowDialog() != DialogResult.OK) return false;

            if (dlg.FileNames.Length != 2)
            {
                XtraMessageBox.Show(
                    "יש לבחור בדיוק שני קבצים: קודי הנחה ותעריפי הנחה.\n" +
                    "אפשר לסמן את שניהם יחד עם Ctrl.",
                    "בחירת קבצים", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string first = dlg.FileNames[0], second = dlg.FileNames[1];
            bool firstIsRates, secondIsRates;
            try
            {
                firstIsRates = IsRatesWorkbook(first);
                secondIsRates = IsRatesWorkbook(second);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"לא ניתן לקרוא את הקבצים:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (firstIsRates == secondIsRates)
            {
                XtraMessageBox.Show(
                    firstIsRates
                        ? "שני הקבצים הם קובצי תעריפים. חסר קובץ קודי ההנחה."
                        : "שני הקבצים הם קובצי קודים. חסר קובץ תעריפי ההנחה.",
                    "בחירת קבצים", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            ratesPath = firstIsRates ? first : second;
            codesPath = firstIsRates ? second : first;
            return true;
        }

        /// <summary>
        /// קובץ תעריפים מזוהה לפי עמודת "תאריך תחילת תוקף" שבשורה הראשונה.
        /// </summary>
        private static bool IsRatesWorkbook(string path)
        {
            using var book = new XLWorkbook(path);
            var sheet = book.Worksheets.First();
            foreach (var cell in sheet.Row(1).CellsUsed())
                if (cell.GetString().Trim() == "תאריך תחילת תוקף")
                    return true;
            return false;
        }

        /// <summary>
        /// התיקייה שבה יושבים קובצי הקודים והתעריפים — "קודי הנחה ותעריפים",
        /// תיקייה מנוהלת ב-tbMainParameters שנוצרת בעלייה כמו שאר תיקיות היבוא.
        /// Falls back to the general helper-files folder on an older database that
        /// does not have the row yet.
        /// </summary>
        private static string CodeWorkbookFolder()
        {
            foreach (var candidate in new[] { ManParameters.DiscountCodesPath,
                                              ManParameters.HelperFilesFolder })
            {
                if (!string.IsNullOrWhiteSpace(candidate) && Directory.Exists(candidate))
                    return candidate;
            }
            return null;
        }

        private int CountCodes()
        {
            try
            {
                using var con = new SqlConnection(connectionString);
                con.Open();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM tbDiscountsCodes", con);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        // ── "Select all" checkbox inside the "בחר" column header ────────
        private void gridView1_CustomDrawColumnHeader(object sender,
            DevExpress.XtraGrid.Views.Grid.ColumnHeaderCustomDrawEventArgs e)
        {
            if (e.Column != colIsSelected) return;

            e.Info.InnerElements.Clear();
            e.Info.Caption = "";
            e.Painter.DrawObject(e.Info);
            DrawHeaderCheckBox(e.Cache, e.Bounds, _selectAllChecked);
            e.Handled = true;
        }

        private void DrawHeaderCheckBox(DevExpress.Utils.Drawing.GraphicsCache cache,
            System.Drawing.Rectangle headerBounds, bool isChecked)
        {
            int size = Math.Min(18, headerBounds.Height - 2);
            if (size <= 0) return;

            var box = new System.Drawing.Rectangle(
                headerBounds.X + (headerBounds.Width - size) / 2,
                headerBounds.Y + (headerBounds.Height - size) / 2,
                size, size);

            var info = repositoryItemCheckEdit1.CreateViewInfo()
                as DevExpress.XtraEditors.ViewInfo.CheckEditViewInfo;
            var painter = repositoryItemCheckEdit1.CreatePainter()
                as DevExpress.XtraEditors.Drawing.CheckEditPainter;
            if (info == null || painter == null) return;

            info.EditValue = isChecked;
            info.Bounds = box;
            info.CalcViewInfo(cache.Graphics);

            var args = new DevExpress.XtraEditors.Drawing.ControlGraphicsInfoArgs(info, cache, box);
            painter.Draw(args);
            args.Cache = null;
        }

        // ── מונה השורות ────────────────────────────────────────────────
        private void UpdateRowCountLabel()
        {
            if (lblRowCount == null) return;

            int total = gridView1.RowCount;
            int selected = 0;
            for (int i = 0; i < total; i++)
            {
                var val = gridView1.GetRowCellValue(i, colIsSelected);
                if (val is bool b && b) selected++;
            }

            lblRowCount.Text = selected > 0
                ? $"סה\"כ שורות: {total}    |    מסומנות: {selected}"
                : $"סה\"כ שורות: {total}";
        }

        // Marks/unmarks every row currently displayed in the grid (respects filters)
        private void SetSelectionForAllRows(bool selected)
        {
            gridView1.PostEditor();
            gridView1.BeginUpdate();
            try
            {
                for (int i = 0; i < gridView1.RowCount; i++)
                    SetRowSelection(i, selected);
            }
            finally
            {
                gridView1.EndUpdate();
            }
            UpdateSelectAllState();
        }

        // IsSelected is a UI-only column — don't let it mark the row (or the form) as dirty
        private void SetRowSelection(int rowHandle, bool selected)
        {
            var row = gridView1.GetDataRow(rowHandle);
            if (row == null)
            {
                gridView1.SetRowCellValue(rowHandle, colIsSelected, selected);
                return;
            }

            var stateBefore = row.RowState;
            row["IsSelected"] = selected;
            if (stateBefore == DataRowState.Unchanged)
                row.AcceptChanges();
        }

        // Keeps the header checkbox in sync with the individual row checkboxes
        private void UpdateSelectAllState()
        {
            bool allSelected = gridView1.RowCount > 0;
            for (int i = 0; i < gridView1.RowCount && allSelected; i++)
            {
                var val = gridView1.GetRowCellValue(i, colIsSelected);
                if (!(val is bool b && b)) allSelected = false;
            }

            _selectAllChecked = allSelected;
            gridView1.InvalidateColumnHeader(colIsSelected);
            UpdateRowCountLabel();
        }

        private void RestoreGridLayout()
        {
            if (File.Exists(LayoutFilePath))
            {
                try
                {
                    gridView1.RestoreLayoutFromXml(LayoutFilePath);
                }
                catch { }
            }
        }

        private void SaveGridLayout()
        {
            try
            {
                if (!Directory.Exists(LayoutDir))
                    Directory.CreateDirectory(LayoutDir);
                gridView1.SaveLayoutToXml(LayoutFilePath);
            }
            catch { }
        }

        private void RestoreFormState()
        {
            try
            {
                if (!File.Exists(FormStatePath)) return;
                var lines = File.ReadAllLines(FormStatePath);
                if (lines.Length >= 5)
                {
                    var state = (FormWindowState)Enum.Parse(typeof(FormWindowState), lines[0]);
                    if (state == FormWindowState.Maximized)
                    {
                        this.WindowState = FormWindowState.Maximized;
                    }
                    else
                    {
                        this.StartPosition = FormStartPosition.Manual;
                        this.Left = int.Parse(lines[1]);
                        this.Top = int.Parse(lines[2]);
                        this.Width = int.Parse(lines[3]);
                        this.Height = int.Parse(lines[4]);
                        this.WindowState = FormWindowState.Normal;
                    }
                }
            }
            catch { }
        }

        private void SaveFormState()
        {
            try
            {
                if (!Directory.Exists(LayoutDir))
                    Directory.CreateDirectory(LayoutDir);

                var bounds = this.WindowState == FormWindowState.Normal
                    ? this.Bounds
                    : this.RestoreBounds;

                File.WriteAllLines(FormStatePath, new[]
                {
                    this.WindowState.ToString(),
                    bounds.Left.ToString(),
                    bounds.Top.ToString(),
                    bounds.Width.ToString(),
                    bounds.Height.ToString()
                });
            }
            catch { }
        }

        private void frDiscountsCodes_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isDirty)
            {
                var result = XtraMessageBox.Show(
                    "יש שינויים שלא נשמרו. האם לשמור לפני סגירה?",
                    "שמירת שינויים", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    btnSave_Click(null, EventArgs.Empty);
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            SaveGridLayout();
            SaveFormState();
        }

        // ── Year filter — kept only because the designer wires the event ──
        //    A code has no year any more, so there is nothing to filter by.
        private void barEditYear_EditValueChanged(object sender, EventArgs e)
        {
        }

        // ── Load categories into combobox ────────────────────────────────
        private void LoadCategories()
        {
            try
            {
                repositoryItemComboCategories.Items.Clear();
                using var con = new SqlConnection(connectionString);
                con.Open();
                using var cmd = new SqlCommand(
                    "SELECT DISTINCT Categories FROM tbDiscountsCodes WHERE Categories IS NOT NULL ORDER BY Categories", con);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    repositoryItemComboCategories.Items.Add(reader.GetString(0));

                // Force assign at runtime in case Designer binding was ignored
                colCategories.ColumnEdit = repositoryItemComboCategories;
                gridView1.RefreshData();
            }
            catch { }
        }

        // ── Load code combos for GroupCode, LegallDiscountCode, MunicipalDiscountCode ──
        private void LoadCodeCombos()
        {
            try
            {
                repositoryItemComboGroupCode.Items.Clear();
                repositoryItemComboLegallDiscountCode.Items.Clear();
                repositoryItemComboMunicipalDiscountCode.Items.Clear();

                using var con = new SqlConnection(connectionString);
                con.Open();

                using (var cmd = new SqlCommand("SELECT DISTINCT GroupCode FROM tbDiscountsCodes WHERE GroupCode IS NOT NULL ORDER BY GroupCode", con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) repositoryItemComboGroupCode.Items.Add(reader[0]);

                using (var cmd = new SqlCommand("SELECT DISTINCT LegallDiscountCode FROM tbDiscountsCodes WHERE LegallDiscountCode IS NOT NULL ORDER BY LegallDiscountCode", con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) repositoryItemComboLegallDiscountCode.Items.Add(reader[0]);

                using (var cmd = new SqlCommand("SELECT DISTINCT MunicipalDiscountCode FROM tbDiscountsCodes WHERE MunicipalDiscountCode IS NOT NULL ORDER BY MunicipalDiscountCode", con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) repositoryItemComboMunicipalDiscountCode.Items.Add(reader[0]);

                colGroupCode.ColumnEdit = repositoryItemComboGroupCode;
                colDiscountCode.ColumnEdit = repositoryItemComboLegallDiscountCode;
                colMunicipalDiscountCode.ColumnEdit = repositoryItemComboMunicipalDiscountCode;
            }
            catch { }
        }

        // ── Load grid data ───────────────────────────────────────────────
        private void LoadData()
        {
            try
            {
                using var con = new SqlConnection(connectionString);

                // The definition — one row per code, no year, no percentages
                string codesSql = @"SELECT ID, Categories, MunicipalDiscountCode, LegallDiscountCode,
                                           GroupCode, DiscountName, Mandatory, LawInForce,
                                           Documents, Remark, Rules, Conditions
                                    FROM   tbDiscountsCodes
                                    ORDER  BY Categories, LegallDiscountCode";

                // Its rate versions — one row per change, newest first
                string ratesSql = @"SELECT ID, DiscountCodeId, ValidityDate, DiscountPercent,
                                           PreviousDiscountPercent, MaximumDiscount, MaximumArea
                                    FROM   tbDiscountCodeRates
                                    ORDER  BY DiscountCodeId, ValidityDate DESC";

                dtDiscountsCodes.Clear();
                dtRates.Clear();
                using (var adapter = new SqlDataAdapter(codesSql, con))
                    adapter.Fill(dtDiscountsCodes);
                using (var adapter = new SqlDataAdapter(ratesSql, con))
                    adapter.Fill(dtRates);

                if (!dtDiscountsCodes.Columns.Contains("IsSelected"))
                    dtDiscountsCodes.Columns.Add("IsSelected", typeof(bool));

                // Wire the two together once; the grid shows the versions of the
                // code you expand.
                if (_data.Tables.Count == 0)
                {
                    _data.Tables.Add(dtDiscountsCodes);
                    _data.Tables.Add(dtRates);
                    _data.Relations.Add(RatesRelation,
                        dtDiscountsCodes.Columns["ID"], dtRates.Columns["DiscountCodeId"], false);
                }

                // Bound through the DataSet so a focused code can hand the other
                // panel a child view of its own rate versions.
                gridControl1.DataSource = _data;
                gridControl1.DataMember = dtDiscountsCodes.TableName;
                ShowRatesForFocusedCode();

                // Force combobox assignment after DataSource
                colCategories.ColumnEdit = repositoryItemComboCategories;
                LoadCodeCombos();
                gridView1.OptionsBehavior.Editable = true;

                // Restore saved layout (column widths) or best-fit on first load
                gridView1.OptionsView.ColumnAutoWidth = false;
                if (File.Exists(LayoutFilePath))
                {
                    RestoreGridLayout();
                }
                else
                {
                    gridView1.BestFitColumns();
                }

                // Fresh data — nothing is marked
                _selectAllChecked = false;
                gridView1.InvalidateColumnHeader(colIsSelected);
                UpdateRowCountLabel();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בטעינת נתונים:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridView1_FocusedRowChanged(object sender,
            DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            var row = gridView1.GetDataRow(gridView1.FocusedRowHandle);
            focusedRowId = (row != null && row["ID"] != DBNull.Value)
                ? Convert.ToInt32(row["ID"]) : -1;

            // The other panel always shows the focused code's rate versions.
            ShowRatesForFocusedCode();
        }

        // When a new category is typed — add it to the combo list
        private void gridView1_CellValueChanged(object sender,
            DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            // IsSelected is a UI-only column and must not mark the form as dirty
            if (e.Column == colIsSelected)
            {
                UpdateSelectAllState();
                return;
            }

            _isDirty = true;
            if (e.Column != colCategories) return;
            string val = e.Value?.ToString();
            if (!string.IsNullOrWhiteSpace(val) &&
                !repositoryItemComboCategories.Items.Contains(val))
                repositoryItemComboCategories.Items.Add(val);
        }

        // ── Toolbar ──────────────────────────────────────────────────────
        private void btnAdd_Click(object sender, EventArgs e)
        {
            // A new code starts with no rate versions. Save it, then expand the
            // row and add the first version with the date it takes effect.
            _isDirty = true;
            gridView1.AddNewRow();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            gridView1.PostEditor();
            gridView1.UpdateCurrentRow();
            gridViewRates?.PostEditor();
            gridViewRates?.UpdateCurrentRow();

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using var con = new SqlConnection(connectionString);
                con.Open();
                using var tx = con.BeginTransaction();
                try
                {
                    // ── the definitions ──
                    // A new code's ID comes back from the INSERT, because its rate
                    // versions are keyed on it.
                    foreach (DataRow row in dtDiscountsCodes.Rows)
                    {
                        if (row.RowState == DataRowState.Added)
                        {
                            using var cmd = new SqlCommand(@"INSERT INTO tbDiscountsCodes
                                (Categories,MunicipalDiscountCode,LegallDiscountCode,GroupCode,
                                 DiscountName,Mandatory,LawInForce)
                                OUTPUT INSERTED.ID
                                VALUES
                                (@Categories,@MunicipalDiscountCode,@LegallDiscountCode,@GroupCode,
                                 @DiscountName,@Mandatory,@LawInForce)", con, tx);
                            AddParams(cmd, row);
                            row["ID"] = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        else if (row.RowState == DataRowState.Modified)
                        {
                            using var cmd = new SqlCommand(@"UPDATE tbDiscountsCodes SET
                                Categories=@Categories, MunicipalDiscountCode=@MunicipalDiscountCode,
                                LegallDiscountCode=@LegallDiscountCode, GroupCode=@GroupCode,
                                DiscountName=@DiscountName,
                                Mandatory=@Mandatory, LawInForce=@LawInForce
                                WHERE ID=@ID", con, tx);
                            AddParams(cmd, row);
                            cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = row["ID"] });
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // ── the rate versions ──
                    int orphans = 0;
                    foreach (DataRow row in dtRates.Rows)
                    {
                        if (row.RowState == DataRowState.Deleted)
                        {
                            object id = row["ID", DataRowVersion.Original];
                            if (id == DBNull.Value) continue;
                            using var cmd = new SqlCommand(
                                "DELETE FROM tbDiscountCodeRates WHERE ID=@ID", con, tx);
                            cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });
                            cmd.ExecuteNonQuery();
                            continue;
                        }

                        if (row.RowState != DataRowState.Added && row.RowState != DataRowState.Modified)
                            continue;

                        // A version added under a code that was never saved has
                        // nothing to hang from — the code has no ID yet.
                        if (row["DiscountCodeId"] == DBNull.Value)
                        {
                            orphans++;
                            continue;
                        }

                        string sql = row.RowState == DataRowState.Added
                            ? @"INSERT INTO tbDiscountCodeRates
                                   (DiscountCodeId, ValidityDate, DiscountPercent,
                                    PreviousDiscountPercent, MaximumDiscount, MaximumArea)
                                VALUES (@DiscountCodeId, @ValidityDate, @DiscountPercent,
                                        @PreviousDiscountPercent, @MaximumDiscount, @MaximumArea)"
                            : @"UPDATE tbDiscountCodeRates SET
                                   ValidityDate            = @ValidityDate,
                                   DiscountPercent         = @DiscountPercent,
                                   PreviousDiscountPercent = @PreviousDiscountPercent,
                                   MaximumDiscount         = @MaximumDiscount,
                                   MaximumArea             = @MaximumArea
                                WHERE ID = @ID";

                        using var rateCmd = new SqlCommand(sql, con, tx);
                        rateCmd.Parameters.Add(new SqlParameter("@DiscountCodeId", SqlDbType.Int)
                            { Value = row["DiscountCodeId"] });
                        rateCmd.Parameters.Add(new SqlParameter("@ValidityDate", SqlDbType.Date)
                            { Value = row["ValidityDate"] == DBNull.Value ? (object)DateTime.Today : row["ValidityDate"] });
                        rateCmd.Parameters.Add(new SqlParameter("@DiscountPercent", SqlDbType.Decimal) { Value = N(row["DiscountPercent"]) });
                        rateCmd.Parameters.Add(new SqlParameter("@PreviousDiscountPercent", SqlDbType.Decimal) { Value = N(row["PreviousDiscountPercent"]) });
                        rateCmd.Parameters.Add(new SqlParameter("@MaximumDiscount", SqlDbType.Decimal) { Value = N(row["MaximumDiscount"]) });
                        rateCmd.Parameters.Add(new SqlParameter("@MaximumArea", SqlDbType.Decimal) { Value = N(row["MaximumArea"]) });
                        if (row.RowState == DataRowState.Modified)
                            rateCmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = row["ID"] });
                        rateCmd.ExecuteNonQuery();
                    }

                    tx.Commit();

                    _isDirty = false;
                    string message = "הנתונים נשמרו בהצלחה.";
                    if (orphans > 0)
                        message += $"\n\n{orphans} גרסאות תעריף לא נשמרו — יש לשמור את הקוד תחילה, ואז להוסיף לו גרסאות.";
                    XtraMessageBox.Show(message, "שמירה", MessageBoxButtons.OK,
                        orphans > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                }
                catch
                {
                    try { tx.Rollback(); } catch { /* ignore rollback errors */ }
                    throw;
                }

                LoadCategories();
                LoadData();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בשמירה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Deleting a code takes its rate versions with it (FK ON DELETE CASCADE).
            if (XtraMessageBox.Show("האם למחוק את הקודים שסומנו?\nכל גרסאות התעריף שלהם יימחקו יחד איתם.",
                    "מחיקה", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using var con = new SqlConnection(connectionString);
                con.Open();

                for (int i = 0; i < gridView1.RowCount; i++)
                {
                    var isSelected = gridView1.GetRowCellValue(i, colIsSelected);
                    bool isChecked = isSelected != null
                                     && isSelected != DBNull.Value
                                     && isSelected is bool b && b;
                    if (!isChecked) continue;

                    var idVal = gridView1.GetRowCellValue(i, colID);
                    if (idVal == null || idVal == DBNull.Value) continue;

                    using var cmd = new SqlCommand(
                        "DELETE FROM tbDiscountsCodes WHERE ID=@ID", con);
                    cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int)
                        { Value = Convert.ToInt32(idVal) });
                    cmd.ExecuteNonQuery();
                }
                LoadData();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה במחיקה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        // ── Row color: blue for new (unsaved) rows ────────────────────────
        private void gridView1_RowStyle(object sender,
            DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (e.RowHandle < 0) return;
            var row = gridView1.GetDataRow(e.RowHandle);
            if (row != null && row.RowState == DataRowState.Added)
            {
                e.Appearance.BackColor = System.Drawing.Color.FromArgb(210, 230, 255);
                e.Appearance.BackColor2 = System.Drawing.Color.FromArgb(210, 230, 255);
                e.HighPriority = true;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // רענון קורא מחדש מבסיס הנתונים ומוחק שינויים שלא נשמרו. שאר
            // הפעולות בטופס טוענות מחדש בעצמן, ולכן הכפתור נחוץ בעיקר כששינו
            // את הקודים ממחשב אחר — ואז דווקא חשוב לא לאבד את מה שהוקלד כאן.
            if (_isDirty &&
                XtraMessageBox.Show("קיימים שינויים שלא נשמרו. לרענן ולבטל אותם?",
                    "רענון נתונים", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            LoadCategories();
            LoadData();
            _isDirty = false;
        }

        private void btnManageCategories_ItemClick(object sender, ItemClickEventArgs e)
        {
            var current = repositoryItemComboCategories.Items
                .Cast<string>().ToList();

            using var frm = new frCategoryManager(current);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                repositoryItemComboCategories.Items.Clear();
                foreach (var cat in frm.Categories.OrderBy(c => c))
                    repositoryItemComboCategories.Items.Add(cat);
            }
        }

        // ── "Copy discount codes from one year to another" — no longer exists ──
        //    Copying a year was how a new year got its codes when a code had one
        //    row per year. Codes are now year-less and a rate change is one dated
        //    version, so there is nothing to copy. The button is hidden; this
        //    handler stays only because the designer wires the event.
        private void btnCopyCodes_ItemClick(object sender, ItemClickEventArgs e)
        {
            XtraMessageBox.Show(
                "העתקת קודים בין שנים בוטלה.\n\n" +
                "קוד הנחה אינו שייך לשנה — שינוי תעריף נרשם כגרסה חדשה עם תאריך תחילת תוקף, בטבלה התחתונה.",
                "לא נדרש עוד", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExportExcel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (dtDiscountsCodes == null || dtDiscountsCodes.Rows.Count == 0)
            {
                XtraMessageBox.Show("אין נתונים לייצוא.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Files|*.xlsx";
                sfd.Title = "שמור קובץ אקסל";
                sfd.FileName = $"קודי_הנחות_{DateTime.Today:yyyy-MM-dd}.xlsx";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    using var wb = new ClosedXML.Excel.XLWorkbook();
                    var ws = wb.Worksheets.Add("קודי הנחות");
                    ws.RightToLeft = true;

                    // Headers from visible grid columns
                    var visibleCols = new List<DevExpress.XtraGrid.Columns.GridColumn>();
                    for (int i = 0; i < gridView1.VisibleColumns.Count; i++)
                    {
                        var col = gridView1.VisibleColumns[i];
                        if (col.FieldName != "IsSelected" && col.FieldName != "ID")
                            visibleCols.Add(col);
                    }

                    for (int c = 0; c < visibleCols.Count; c++)
                    {
                        ws.Cell(1, c + 1).Value = visibleCols[c].Caption;
                        ws.Cell(1, c + 1).Style.Font.Bold = true;
                        ws.Cell(1, c + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(68, 114, 196);
                        ws.Cell(1, c + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    }

                    // Data rows
                    for (int r = 0; r < gridView1.RowCount; r++)
                    {
                        for (int c = 0; c < visibleCols.Count; c++)
                        {
                            var val = gridView1.GetRowCellValue(r, visibleCols[c]);
                            if (val != null && val != DBNull.Value)
                                ws.Cell(r + 2, c + 1).SetValue(ClosedXML.Excel.XLCellValue.FromObject(val));
                        }
                    }

                    ws.Columns().AdjustToContents();
                    wb.SaveAs(sfd.FileName);

                    XtraMessageBox.Show("הקובץ נוצר בהצלחה.", "ייצוא", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show($"שגיאה בייצוא:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
            => this.Close();

        private void btnDocuments_ItemClick(object sender, ItemClickEventArgs e)
            => OpenMemoForm("מסמכים נדרשים", "Documents");

        private void btnRemark_ItemClick(object sender, ItemClickEventArgs e)
            => OpenMemoForm("הסברים", "Remark");

        private void btnRules_ItemClick(object sender, ItemClickEventArgs e)
            => OpenMemoForm("כללים", "Rules");

        private void btnConditions_ItemClick(object sender, ItemClickEventArgs e)
            => OpenMemoForm("תנאים", "Conditions");

        private void btnLawInForce_ItemClick(object sender, ItemClickEventArgs e)
            => OpenMemoForm("מקור החוק", "LawInForce");

        private void OpenMemoForm(string title, string fieldName)
        {
            if (focusedRowId < 0)
            {
                XtraMessageBox.Show("יש לבחור שורה תחילה.", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string currentText = "";
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using var cmd = new SqlCommand(
                        $"SELECT {fieldName} FROM tbDiscountsCodes WHERE ID=@ID", con);
                    cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = focusedRowId });
                    var val = cmd.ExecuteScalar();
                    if (val != DBNull.Value && val != null) currentText = val.ToString();
                }
                using var frm = new frMemoEdit(title, currentText);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    using var con = new SqlConnection(connectionString);
                    con.Open();
                    using var cmd = new SqlCommand(
                        $"UPDATE tbDiscountsCodes SET {fieldName}=@Val WHERE ID=@ID", con);
                    cmd.Parameters.Add(new SqlParameter("@Val", SqlDbType.NVarChar, -1)
                        { Value = (object)frm.EditedText ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = focusedRowId });
                    cmd.ExecuteNonQuery();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddParams(SqlCommand cmd, DataRow row)
        {
            cmd.Parameters.Add(new SqlParameter("@Categories",      SqlDbType.NVarChar,  40) { Value = N(row["Categories"]) });
            cmd.Parameters.Add(new SqlParameter("@MunicipalDiscountCode", SqlDbType.Int)      { Value = row["MunicipalDiscountCode"] == DBNull.Value ? (object)DBNull.Value : Convert.ToInt32(row["MunicipalDiscountCode"]) });
            cmd.Parameters.Add(new SqlParameter("@LegallDiscountCode",    SqlDbType.Int)            { Value = row["LegallDiscountCode"]    == DBNull.Value ? (object)DBNull.Value : Convert.ToInt32(row["LegallDiscountCode"]) });
            cmd.Parameters.Add(new SqlParameter("@GroupCode",       SqlDbType.Int)            { Value = row["GroupCode"]       == DBNull.Value ? (object)DBNull.Value : Convert.ToInt32(row["GroupCode"]) });
            cmd.Parameters.Add(new SqlParameter("@DiscountName",    SqlDbType.NVarChar, 120) { Value = N(row["DiscountName"]) });
            cmd.Parameters.Add(new SqlParameter("@Mandatory",       SqlDbType.NChar,      8) { Value = N(row["Mandatory"]) });
            cmd.Parameters.Add(new SqlParameter("@LawInForce",      SqlDbType.NVarChar, 120) { Value = N(row["LawInForce"]) });
            // The percentages and the caps are not here any more — they belong to
            // the rate version, which carries the date it takes effect.
        }

        private object N(object val) =>
            val == null || val == DBNull.Value || string.IsNullOrWhiteSpace(val.ToString())
                ? (object)DBNull.Value : val;
    }

}
