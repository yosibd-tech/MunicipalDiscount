using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using Microsoft.Data.SqlClient;
using MunicipalDiscount.Classes;

namespace MunicipalDiscount
{
    /// <summary>
    /// עדכון תאריך הבקשה לשורות שנקלטו מקובץ ההנחות של השנה השוטפת.
    ///
    /// The current-year discounts file (barButImportCurrentYearData) is a list of
    /// discounts the municipality has ALREADY granted, so every line of it becomes
    /// a request — but the file carries no date, and the rows land with
    /// RequestDate NULL. The date is what selects the rate version a discount has
    /// to be judged against, so the audit report cannot do its work until it is
    /// filled in. This screen is that filling-in.
    ///
    /// Two grids. The upper one is the worklist: requests of the chosen year that
    /// still have no date. The lower one holds what this session has already
    /// saved, so a mistyped date can be corrected without hunting for the citizen
    /// again. It is a working list only — reopening the screen shows the worklist
    /// alone, because a dated request no longer belongs to it.
    ///
    /// The screen is laid out in RequestDateForm.Designer.cs and is editable in
    /// the WinForms designer; only the data work lives here.
    /// </summary>
    public partial class RequestDateForm : XtraForm
    {
        private readonly string connectionString = DatabaseConfig.ConnectionString;

        // השנה שהמסך עובד עליה, כפי שנבחרה בסרגל של עמוד "עידכון תאריך בקשת הנחה".
        private readonly int year;

        private DataTable dt = new DataTable();        // בקשות ללא תאריך
        private DataTable dtSaved = new DataTable();   // מה שנשמר במהלך העבודה

        /// <summary>השנה שרשימת העבודה הזו שייכת לה.</summary>
        public int Year => year;

        /// <summary>
        /// קונסטרוקטור ללא פרמטרים — נדרש כדי שהדיזיינר של WinForms יוכל לפתוח
        /// את הטופס. בזמן ריצה נפתח תמיד עם שנה מפורשת.
        /// </summary>
        public RequestDateForm() : this(DateTime.Now.Year)
        {
        }

        public RequestDateForm(int year)
        {
            this.year = year;

            InitializeComponent();
            ThemeHelper.ApplyTheme(this);

            // הכותרת נקבעת בדיזיינר, וכאן רק נוספת לה השנה — כך שינוי הניסוח
            // בדיזיינר נשמר ואינו נדרס.
            Text = $"{Text} — {year}";

            // ThemeHelper רץ לפני השורות האלה ודורס מראה שנקבע בדיזיינר,
            // ולכן צבע האזהרה נקבע כאן שוב.
            grpSaved.AppearanceCaption.ForeColor = System.Drawing.Color.Red;
            grpSaved.AppearanceCaption.Options.UseForeColor = true;

            ConfigureDateMask(repositoryItemRequestDate);
            ConfigureDateMask(repositoryItemSavedDate);

            IconOptions.SvgImage = AppIcons.OpenYear;
            IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            IconOptions.ShowIcon = true;
        }

        /// <summary>
        /// הנתונים נטענים בפתיחת הטופס ולא בקונסטרוקטור: הדיזיינר בונה את הטופס
        /// אך אינו מריץ Load, ולכן אין ניסיון גישה לבסיס הנתונים בזמן עיצוב.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            dtSaved = NewTable();
            gridSaved.DataSource = dtSaved;

            LoadData();
        }

        /// <summary>
        /// מסכת התאריך כפי שהיא מוגדרת בתיעוד של DevExpress: מסכת DateTime עם
        /// UseAdvancingCaret — הסמן עובר לבד מיום לחודש לשנה, המקלדת מספיקה,
        /// והעורך ממיר את מה שהוקלד ל-DateTime בעצמו.
        ///
        /// TextEdit ולא DateEdit: אין לוח שנה נפתח, ואין ערך ברירת מחדל של היום
        /// כשהתא ריק.
        /// </summary>
        private static void ConfigureDateMask(
            DevExpress.XtraEditors.Repository.RepositoryItemTextEdit editor)
        {
            var mask = editor.MaskSettings.Configure<DevExpress.XtraEditors.Mask.MaskSettings.DateTime>();
            mask.MaskExpression = "dd/MM/yy";
            mask.UseAdvancingCaret = true;

            editor.Mask.UseMaskAsDisplayFormat = true;
        }

        private static DataTable NewTable()
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Area", typeof(int));
            table.Columns.Add("ClientNumber", typeof(string));
            table.Columns.Add("ClientName", typeof(string));
            table.Columns.Add("RequestDate", typeof(DateTime));
            return table;
        }

        /// <summary>
        /// טוען מחדש את רשימת הבקשות.
        ///
        /// Called by MainForm after the movements file filled dates in bulk: the
        /// worklist holds only dateless requests, so most of it has just gone.
        /// </summary>
        public void ReloadRequests() => LoadData();

        // ── כל הבקשות שאין להן תאריך, לשנה הנבחרת ────────────────────────
        private void LoadData()
        {
            try
            {
                // One year at a time. Without the filter the same citizen appears
                // once per year with nothing to tell the rows apart.
                const string sql = @"
                    SELECT r.ID,
                           y.Area,
                           c.ClientNumber,
                           c.ClientName,
                           r.RequestDate
                    FROM   tbDiscountRequests r
                    INNER  JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
                    INNER  JOIN tbClients        c ON c.ID = y.ClientId
                    WHERE  r.RequestDate IS NULL
                      AND  y.Year_ = @year
                    ORDER  BY CASE WHEN y.Area IS NULL THEN 1 ELSE 0 END,
                              y.Area DESC, c.ClientNumber";

                dt = new DataTable();
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@year", SqlDbType.Int).Value = year;
                    using var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }

                gridControl.DataSource = dt;
                UpdateCount();

                // המסך נפתח על השורה הראשונה שיש לה אזור, ולא על שורה חסרת
                // אזור: העבודה מתנהלת לפי תיקיית האזור, ושורות בלי אזור נדחות
                // לסוף הרשימה ממילא.
                if (gridView.RowCount > 0)
                {
                    int first = 0;
                    for (int i = 0; i < gridView.RowCount; i++)
                    {
                        if (gridView.GetRowCellValue(i, colArea) != DBNull.Value)
                        {
                            first = i;
                            break;
                        }
                    }

                    gridView.FocusedRowHandle = first;
                    gridView.MakeRowVisible(first);
                    gridView.FocusedColumn = colRequestDate;
                    gridView.ShowEditor();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בטעינת נתונים:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateCount()
        {
            string text = $"בקשות ללא תאריך: {dt.Rows.Count}";
            if (dtSaved.Rows.Count > 0)
                text += unsavedChanges
                    ? $"   |   שורות שהוקלדו: {dtSaved.Rows.Count} — יש שינויים שלא נשמרו"
                    : $"   |   שורות שהוקלדו: {dtSaved.Rows.Count}";
            lblCount.Text = text;

            // האזהרה נשארת בכותרת בכל מצב, ובאדום — היא נקראת לפני כל שמירה
            grpSaved.Text = dtSaved.Rows.Count > 0
                ? $"שורות שהוקלדו ({dtSaved.Rows.Count}) — לפני שמירה בדוק שכל התאריכים שנקלטו למטה תקינים"
                : "שורות שהוקלדו — לפני שמירה בדוק שכל התאריכים שנקלטו למטה תקינים";
        }

        // האם יש הקלדה או תיקון שטרם נשמרו — מוצג בשורת המצב של הלשונית
        private bool unsavedChanges;

        private void gridView_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            unsavedChanges = true;
            UpdateCount();
        }

        /// <summary>
        /// כל שורה שמקבלת מיקוד נפתחת ישר בעריכת תאריך הבקשה — העובד מקליד,
        /// מקיש Enter, ומגיע לשורה הבאה כשהיא כבר פתוחה להקלדה.
        ///
        /// השורה שנעזבה, אם קיבלה תאריך, יורדת מיד לרשת התחתונה: רשימת העבודה
        /// מתקצרת תוך כדי הקלדה, ומה שהוקלד עומד למטה לתיקון.
        /// </summary>
        private void gridView_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.PrevFocusedRowHandle >= 0)
            {
                DataRow left = gridView.GetDataRow(e.PrevFocusedRowHandle);
                if (left != null && left["RequestDate"] != DBNull.Value)
                {
                    // נדחה לסוף תור ההודעות: אי אפשר להסיר שורה מתוך אירוע
                    // שינוי המיקוד עצמו.
                    BeginInvoke(new Action(() => MoveDown(left)));
                    return;
                }
            }

            if (e.FocusedRowHandle < 0) return;

            gridView.FocusedColumn = colRequestDate;
            gridView.ShowEditor();
        }

        /// <summary>
        /// מעביר שורה מרשימת העבודה לרשת "שורות שעודכנו". השמירה לבסיס הנתונים
        /// נעשית בכפתור שמור — כאן רק מעבר בין הרשתות.
        /// </summary>
        private void MoveDown(DataRow row)
        {
            if (row.RowState == DataRowState.Detached || row.Table != dt) return;

            dtSaved.Rows.Add(row["ID"], row["Area"], row["ClientNumber"],
                row["ClientName"], row["RequestDate"]);
            dt.Rows.Remove(row);

            UpdateCount();

            // המיקוד נשאר באותו מקום ברשימה — שהוא כבר הלקוח הבא
            if (gridView.RowCount > 0)
            {
                gridView.FocusedColumn = colRequestDate;
                gridView.ShowEditor();
            }
        }

        /// <summary>
        /// איתור לקוח לפי מספר.
        ///
        /// The municipality files its request forms either by area or by request
        /// date, and the date is exactly what is missing here — so the way in is
        /// the citizen number off the form in the clerk's hand. The list runs to
        /// thousands of rows; scrolling for one of them is not workable.
        /// </summary>
        private void txtFindClient_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.Handled = true;
            e.SuppressKeyPress = true;
            FindClient();
        }

        private void FindClient()
        {
            string number = txtFindClient.Text.Trim();
            if (number.Length == 0) return;

            int handle = gridView.LocateByValue("ClientNumber", number);

            // לא נמצא בדיוק — מנסים התאמה לפי תחילת המספר
            if (handle == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                for (int i = 0; i < gridView.RowCount; i++)
                {
                    string value = gridView.GetRowCellValue(i, colClientNumber)?.ToString();
                    if (value != null && value.StartsWith(number, StringComparison.Ordinal))
                    {
                        handle = i;
                        break;
                    }
                }
            }

            if (handle == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                lblCount.Text = $"מספר לקוח {number} אינו ברשימה — ייתכן שכבר נשמר לו תאריך";
                return;
            }

            gridView.FocusedRowHandle = handle;
            gridView.MakeRowVisible(handle);
            gridView.FocusedColumn = colRequestDate;
            gridControl.Focus();
            gridView.ShowEditor();
        }

        /// <summary>
        /// Enter שומר את התא ויורד לשורה הבאה.
        ///
        /// ProcessGridKey הוא האירוע שרואה את המקש לפני הרשת ולפני העורך הפעיל,
        /// ולכן הוא המקום היחיד שממנו אפשר לשנות את התנהגות Enter בלי לאבד את
        /// מה שהוקלד. PostEditor מעביר את הערך מהעורך לשורה, ו-UpdateCurrentRow
        /// שומר את השורה עצמה — הסדר הזה הוא מה שהתיעוד של DevExpress מראה.
        /// </summary>
        private void gridControl_ProcessGridKey(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            if (sender is not DevExpress.XtraGrid.GridControl grid) return;
            if (grid.FocusedView is not DevExpress.XtraGrid.Views.Grid.GridView view) return;

            view.PostEditor();
            view.UpdateCurrentRow();
            e.Handled = true;

            if (view.FocusedRowHandle < view.RowCount - 1)
                view.FocusedRowHandle++;
        }

        /// <summary>
        /// המסכה כבר מבטיחה תאריך תקין או תא ריק, ולכן נשאר רק הכלל עצמו:
        /// הבקשה כבר הוגשה, ותאריך עתידי הוא טעות הקלדה.
        /// </summary>
        private void gridView_ValidatingEditor(object sender,
            DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            if (sender is not ColumnView view) return;
            if (view.FocusedColumn?.FieldName != "RequestDate") return;

            if (e.Value is DateTime entered && entered > DateTime.Today)
            {
                e.Valid = false;
                e.ErrorText = "הבקשה כבר הוגשה — לא ניתן להזין תאריך עתידי";
            }
        }

        /// <summary>
        /// Delete/Backspace מנקים תאריך שהוקלד בטעות ומחזירים את התא לריק.
        /// ברשת התחתונה זו הדרך לבטל עדכון: השורה תחזור לרשימת העבודה.
        /// </summary>
        private void gridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete && e.KeyCode != Keys.Back) return;
            if (sender is not ColumnView view) return;
            if (view.FocusedColumn?.FieldName != "RequestDate") return;
            if (view.FocusedRowHandle < 0) return;

            view.SetFocusedRowCellValue(view.FocusedColumn, DBNull.Value);
            UpdateCount();
            e.Handled = true;
        }

        // ── שמירה ────────────────────────────────────────────────────────
        //    כפתור אחד, ומה שהוא שומר הוא כל השורות שברשת התחתונה — גם הקלדה
        //    חדשה וגם תיקון לשורה שכבר נשמרה. תאריך שרוקן שם הוא ביטול:
        //    הבקשה חוזרת ל-NULL ולרשימת העבודה.
        private void btnSave_Click(object sender, EventArgs e)
        {
            gridView.CloseEditor();
            gridView.UpdateCurrentRow();
            gridViewSaved.CloseEditor();
            gridViewSaved.UpdateCurrentRow();

            // שורה שהוקלדה ועדיין לא נעזבה — מורידים גם אותה
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                DataRow row = dt.Rows[i];
                if (row["RequestDate"] != DBNull.Value)
                {
                    dtSaved.Rows.Add(row["ID"], row["Area"], row["ClientNumber"],
                        row["ClientName"], row["RequestDate"]);
                    dt.Rows.Remove(row);
                }
            }

            if (dtSaved.Rows.Count == 0)
            {
                lblCount.Text = "אין שינויים לשמירה";
                return;
            }

            var cleared = new List<DataRow>();

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                int written = 0;

                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using var tx = con.BeginTransaction();

                    using (var cmd = new SqlCommand(
                        "UPDATE tbDiscountRequests SET RequestDate = @date WHERE ID = @id", con, tx))
                    {
                        var pDate = cmd.Parameters.Add("@date", SqlDbType.Date);
                        var pId = cmd.Parameters.Add("@id", SqlDbType.Int);

                        foreach (DataRow row in dtSaved.Rows)
                        {
                            bool isCleared = row["RequestDate"] == DBNull.Value;

                            pDate.Value = isCleared
                                ? (object)DBNull.Value
                                : Convert.ToDateTime(row["RequestDate"]).Date;
                            pId.Value = Convert.ToInt32(row["ID"]);
                            cmd.ExecuteNonQuery();
                            written++;

                            if (isCleared) cleared.Add(row);
                        }
                    }

                    tx.Commit();
                }

                // ביטולים יורדים מהלשונית התחתונה; LoadData יחזיר אותם למעלה
                foreach (DataRow row in cleared)
                    row.Delete();
                dtSaved.AcceptChanges();

                unsavedChanges = false;

                LoadData();
                lblCount.Text = $"נשמרו {written} שורות   |   {lblCount.Text}";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בשמירה:{Environment.NewLine}{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
    }
}
