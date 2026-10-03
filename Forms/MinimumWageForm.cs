using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using MunicipalDiscount.Classes;

namespace MunicipalDiscount
{
    public partial class MinimumWageForm : XtraForm
    {
        private string connectionString = DatabaseConfig.ConnectionString;
        private int? currentId = null;

        public MinimumWageForm()
        {
            InitializeComponent();
            ConfigureValidityDateMask();
        }

        private void MinimumWageForm_Load(object sender, EventArgs e)
        {
            LoadGrid();
            ThemeHelper.ApplyTheme(this);
            this.IconOptions.SvgImage = MunicipalDiscount.Classes.AppIcons.MinimumWage;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;
            ClearForm();
        }

        #region Data Operations

        /// <summary>
        /// מסכת התאריך של DevExpress: dd/MM/yy עם סמן מתקדם, כמו בכל שדה תאריך
        /// אחר במערכת.
        /// </summary>
        /// <summary>
        /// Enter עובר לשדה הבא, ובשדה האחרון שומר — כך נקלטת רשומה שלמה
        /// מהמקלדת והרשת שלמטה מתעדכנת מיד.
        /// </summary>
        private void DetailField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            // DoValidate מעביר את מה שהוקלד מהעורך אל הערך עצמו, כדי שהשדה הבא
            // והשמירה יקראו את מה שהוקלד ולא את הערך הקודם.
            (sender as DevExpress.XtraEditors.TextEdit)?.DoValidate();

            // Enter רק מקדם לשדה הבא. השמירה היא בכפתור שמור בלבד — פעולה
            // שנשמרת לבסיס הנתונים לא תתרחש ממקש שנלחץ בדרך.
            SelectNextControl(ActiveControl, true, true, true, true);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

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

        private void LoadGrid()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT ID, Year_, ValidityDate, MinimumWage FROM tbMinimumWage ORDER BY ValidityDate DESC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    gridControl.DataSource = dt;

                    // Configure columns
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בטעינת נתונים:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            currentId = null;
            txtMinimumWage.Text = "";
            txtValidityDate.EditValue = null;   // ריק, בלי הצעה
            txtValidityDate.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (txtValidityDate.EditValue == null ||
                !DateTime.TryParse(txtValidityDate.EditValue.ToString(), out DateTime validityCheck))
            {
                XtraMessageBox.Show("נא להזין תאריך תחילת תוקף", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtValidityDate.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtMinimumWage.Text))
            {
                XtraMessageBox.Show("אנא הזן שכר מינימום", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMinimumWage.Focus();
                return;
            }

            // השנה נגזרת מתאריך התוקף — אין עוד שדה שנה, ולכן גם אין מה שיסתור אותו
            int year = validityCheck.Year;
            decimal wage = Convert.ToDecimal(txtMinimumWage.Text.Replace(",", ""));
            DateTime validity = Convert.ToDateTime(txtValidityDate.EditValue).Date;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if (currentId == null)
                    {
                        // Check if year exists for insert
                        if (IsValidityDateExists(validityCheck, null))
                        {
                            XtraMessageBox.Show($"כבר קיים שכר מינימום בתאריך {validityCheck:dd/MM/yy}", "שגיאה",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtValidityDate.Focus();
                            return;
                        }

                        // Insert
                        string query = "INSERT INTO tbMinimumWage (Year_, ValidityDate, MinimumWage) VALUES (@year, @validity, @wage)";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.Add("@year", SqlDbType.Int).Value = year;
                        cmd.Parameters.Add("@validity", SqlDbType.Date).Value = validity;
                        cmd.Parameters.Add("@wage", SqlDbType.Decimal).Value = wage;
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // Check if year exists for update (exclude current record)
                        if (IsValidityDateExists(validityCheck, currentId))
                        {
                            XtraMessageBox.Show($"כבר קיים שכר מינימום בתאריך {validityCheck:dd/MM/yy}", "שגיאה",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtValidityDate.Focus();
                            return;
                        }

                        // Update
                        string query = "UPDATE tbMinimumWage SET Year_ = @year, ValidityDate = @validity, MinimumWage = @wage WHERE ID = @id";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.Add("@year", SqlDbType.Int).Value = year;
                        cmd.Parameters.Add("@validity", SqlDbType.Date).Value = validity;
                        cmd.Parameters.Add("@wage", SqlDbType.Decimal).Value = wage;
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = currentId;
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadGrid();
                ClearForm();
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
            if (gridView.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("אנא בחר רשומה למחיקה", "הודעה",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(gridView.GetRowCellValue(gridView.FocusedRowHandle, "ID"));
            string year = gridView.GetRowCellValue(gridView.FocusedRowHandle, "Year_").ToString();

            DialogResult result = XtraMessageBox.Show($"האם למחוק את שכר המינימום לשנת {year}?",
                "אישור מחיקה", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM tbMinimumWage WHERE ID = @id";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                        cmd.ExecuteNonQuery();
                    }

                    LoadGrid();
                    ClearForm();
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
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void gridView_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            LoadSelectedRow();
        }

        private void LoadSelectedRow()
        {
            if (gridView.FocusedRowHandle >= 0)
            {
                currentId = Convert.ToInt32(gridView.GetRowCellValue(gridView.FocusedRowHandle, "ID"));
                txtMinimumWage.Text = Convert.ToDecimal(gridView.GetRowCellValue(gridView.FocusedRowHandle, "MinimumWage")).ToString("N2");
                object validityValue = gridView.GetRowCellValue(gridView.FocusedRowHandle, "ValidityDate");
                txtValidityDate.EditValue = validityValue == DBNull.Value
                    ? (object)null : Convert.ToDateTime(validityValue);
            }
        }

        private void gridControl_Click(object sender, EventArgs e)
        {
            LoadSelectedRow();
        }

        /// <summary>
        /// כפילות נבדקת לפי תאריך התוקף ולא לפי השנה.
        ///
        /// A year used to be the key, so a second row in the same year was a
        /// duplicate by definition. With ValidityDate that is exactly the case we
        /// need to allow: a minimum wage can change in the middle of a year, and
        /// then the same year holds two rows with two different dates.
        /// </summary>
        private bool IsValidityDateExists(DateTime validity, int? excludeId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = excludeId.HasValue
                        ? "SELECT COUNT(*) FROM tbMinimumWage WHERE ValidityDate = @validity AND ID <> @id"
                        : "SELECT COUNT(*) FROM tbMinimumWage WHERE ValidityDate = @validity";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.Add("@validity", SqlDbType.Date).Value = validity.Date;
                    if (excludeId.HasValue)
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = excludeId.Value;

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
