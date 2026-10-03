using DevExpress.XtraEditors;
using MunicipalDiscount.Classes;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    public partial class DiscountHeadersForm : DevExpress.XtraEditors.XtraForm
    {
        private string connectionString = DatabaseConfig.ConnectionString;
        private DataTable dataTable;
        private int? currentRecordId = null;
        private DataTable excelColumnsTable;

        public DiscountHeadersForm()
        {
            // Register encoding provider for Hebrew support
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            InitializeComponent();
            ThemeHelper.ApplyTheme(this);
            this.IconOptions.SvgImage = MunicipalDiscount.Classes.AppIcons.DiscountHeaders;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;
        }

        private void DiscountHeadersForm_Load(object sender, EventArgs e)
        {
            SetupColumnDropdowns();
            LoadData();
        }

        private void SetupColumnDropdowns()
        {
            excelColumnsTable = new DataTable();
            excelColumnsTable.Columns.Add("Index", typeof(int));
            excelColumnsTable.Columns.Add("Name", typeof(string));
            excelColumnsTable.Columns.Add("Sample", typeof(string));
            excelColumnsTable.Rows.Add(0, "(לא נבחר)", "");

            LookUpEdit[] columnDropdowns = { cmbColumn1, cmbColumn2, cmbColumn3, cmbColumn4,
                                              cmbColumn5, cmbColumn6, cmbColumn7, cmbColumn8, cmbColumn9 };

            foreach (var cmb in columnDropdowns)
            {
                cmb.Properties.DataSource = excelColumnsTable;
                cmb.Properties.DisplayMember = "Name";
                cmb.Properties.ValueMember = "Index";
                cmb.Properties.Columns.Clear();
                cmb.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Index", "#", 35));
                cmb.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Name", "כותרת", 150));
                cmb.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Sample", "דוגמה", 150));
                cmb.Properties.PopupWidth = 380;
                cmb.EditValue = 0;
            }
        }

        private void btnLoadExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel Files|*.xlsx;*.xls;*.csv;*.txt|All Files|*.*";
                ofd.Title = "בחר קובץ אקסל";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtExcelPath.Text = ofd.FileName;
                    LoadExcelColumns(ofd.FileName);
                }
            }
        }

        private void LoadExcelColumns(string filePath)
        {
            try
            {
                excelColumnsTable.Rows.Clear();
                excelColumnsTable.Rows.Add(0, "(לא נבחר)", "");

                string[] headers = null;
                string[] sampleRow1 = null;
                string[] sampleRow2 = null;

                // Read all lines from file
                string[] allLines;
                using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding(1255), true))
                {
                    allLines = sr.ReadToEnd().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                }

                // Step 1: Find the header row - prioritize known column names
                int headerLineIndex = -1;
                string[] knownHeaders = { "מס' לקוח", "שם לקוח", "קוד הנחה", "טלפון", "מס' נכס", "שנה נוכחית", "תאור", "אחוז הנחה" };

                // First pass: find row with known column names (at least 2 matches)
                for (int i = 0; i < allLines.Length; i++)
                {
                    string line = allLines[i];
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains("\t"))
                        continue;

                    int matchCount = 0;
                    foreach (string kh in knownHeaders)
                    {
                        if (line.Contains(kh))
                            matchCount++;
                    }

                    if (matchCount >= 2)
                    {
                        headers = line.Split('\t');
                        headerLineIndex = i;
                        break;
                    }
                }

                // Fallback: first row with 10+ columns (if no known names found)
                if (headerLineIndex < 0)
                {
                    for (int i = 0; i < allLines.Length; i++)
                    {
                        string line = allLines[i];
                        if (string.IsNullOrWhiteSpace(line) || !line.Contains("\t"))
                            continue;

                        string[] columns = line.Split('\t');
                        if (columns.Length >= 10)
                        {
                            headers = columns;
                            headerLineIndex = i;
                            break;
                        }
                    }
                }

                // Step 2: Read first 2 data rows AFTER the header
                if (headerLineIndex >= 0)
                {
                    for (int i = headerLineIndex + 1; i < allLines.Length; i++)
                    {
                        string line = allLines[i];
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        string[] columns = line.Split('\t');

                        if (sampleRow1 == null)
                        {
                            sampleRow1 = columns;
                        }
                        else if (sampleRow2 == null)
                        {
                            sampleRow2 = columns;
                            break;
                        }
                    }
                }

                if (headers != null)
                {
                    for (int i = 0; i < headers.Length; i++)
                    {
                        string colName = headers[i].Trim();
                        if (string.IsNullOrEmpty(colName))
                            colName = $"עמודה {i + 1}";

                        // Build sample text from data rows
                        string sample = "";
                        string val1 = (sampleRow1 != null && i < sampleRow1.Length) ? sampleRow1[i].Trim() : "";
                        string val2 = (sampleRow2 != null && i < sampleRow2.Length) ? sampleRow2[i].Trim() : "";

                        if (!string.IsNullOrEmpty(val1) && !string.IsNullOrEmpty(val2))
                            sample = $"{val1} | {val2}";
                        else if (!string.IsNullOrEmpty(val1))
                            sample = val1;
                        else if (!string.IsNullOrEmpty(val2))
                            sample = val2;

                        // Truncate long samples
                        if (sample.Length > 40)
                            sample = sample.Substring(0, 37) + "...";

                        excelColumnsTable.Rows.Add(i + 1, $"{i + 1}: {colName}", sample);
                    }
                    lblExcelInfo.Text = $"נמצאו {headers.Length} עמודות בקובץ";
                }
                else
                {
                    lblExcelInfo.Text = "לא נמצאו כותרות עמודות בקובץ";
                }

                // Refresh all dropdowns
                LookUpEdit[] columnDropdowns = { cmbColumn1, cmbColumn2, cmbColumn3, cmbColumn4,
                                          cmbColumn5, cmbColumn6, cmbColumn7, cmbColumn8, cmbColumn9 };
                foreach (var cmb in columnDropdowns)
                {
                    cmb.Properties.DataSource = null;
                    cmb.Properties.DataSource = excelColumnsTable;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בקריאת הקובץ:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT ID,
                                     Column1, Column2, Column3, Column4,
                                     Column5, Column6, Column7, Column8, Column9
                                     FROM tbDiscountHeaders
                                     ORDER BY ID";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    gridControl1.DataSource = dataTable;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בטעינת נתונים:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle >= 0)
            {
                DataRow row = gridView1.GetDataRow(e.FocusedRowHandle);
                if (row != null)
                {
                    currentRecordId = Convert.ToInt32(row["ID"]);
                    cmbColumn1.EditValue = row["Column1"] != DBNull.Value ? row["Column1"] : 0;
                    cmbColumn2.EditValue = row["Column2"] != DBNull.Value ? row["Column2"] : 0;
                    cmbColumn3.EditValue = row["Column3"] != DBNull.Value ? row["Column3"] : 0;
                    cmbColumn4.EditValue = row["Column4"] != DBNull.Value ? row["Column4"] : 0;
                    cmbColumn5.EditValue = row["Column5"] != DBNull.Value ? row["Column5"] : 0;
                    cmbColumn6.EditValue = row["Column6"] != DBNull.Value ? row["Column6"] : 0;
                    cmbColumn7.EditValue = row["Column7"] != DBNull.Value ? row["Column7"] : 0;
                    cmbColumn8.EditValue = row["Column8"] != DBNull.Value ? row["Column8"] : 0;
                    cmbColumn9.EditValue = row["Column9"] != DBNull.Value ? row["Column9"] : 0;
                }
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            currentRecordId = null;
            ClearFields();
        }

        private void ClearFields()
        {
            cmbColumn1.EditValue = 0;
            cmbColumn2.EditValue = 0;
            cmbColumn3.EditValue = 0;
            cmbColumn4.EditValue = 0;
            cmbColumn5.EditValue = 0;
            cmbColumn6.EditValue = 0;
            cmbColumn7.EditValue = 0;
            cmbColumn8.EditValue = 0;
            cmbColumn9.EditValue = 0;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd;

                    if (currentRecordId == null)
                    {
                        // Check if a record already exists
                        SqlCommand checkCmd = new SqlCommand(
                            "SELECT COUNT(*) FROM tbDiscountHeaders", connection);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            XtraMessageBox.Show("כבר קיים מיפוי. בחר את הרשומה מהרשימה לעדכון.", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        cmd = new SqlCommand(
                            @"INSERT INTO tbDiscountHeaders
                              (Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8, Column9)
                              VALUES (@c1, @c2, @c3, @c4, @c5, @c6, @c7, @c8, @c9)", connection);

                        cmd.Parameters.AddWithValue("@c1", cmbColumn1.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c2", cmbColumn2.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c3", cmbColumn3.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c4", cmbColumn4.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c5", cmbColumn5.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c6", cmbColumn6.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c7", cmbColumn7.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c8", cmbColumn8.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c9", cmbColumn9.EditValue ?? 0);
                    }
                    else
                    {
                        cmd = new SqlCommand(
                            @"UPDATE tbDiscountHeaders SET
                              Column1 = @c1, Column2 = @c2, Column3 = @c3, Column4 = @c4,
                              Column5 = @c5, Column6 = @c6, Column7 = @c7, Column8 = @c8, Column9 = @c9
                              WHERE ID = @id", connection);

                        cmd.Parameters.AddWithValue("@c1", cmbColumn1.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c2", cmbColumn2.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c3", cmbColumn3.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c4", cmbColumn4.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c5", cmbColumn5.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c6", cmbColumn6.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c7", cmbColumn7.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c8", cmbColumn8.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@c9", cmbColumn9.EditValue ?? 0);
                        cmd.Parameters.AddWithValue("@id", currentRecordId);
                    }

                    cmd.ExecuteNonQuery();
                    XtraMessageBox.Show("הנתונים נשמרו בהצלחה", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
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

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (currentRecordId == null)
            {
                XtraMessageBox.Show("אנא בחר רשומה למחיקה", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (XtraMessageBox.Show("האם אתה בטוח שברצונך למחוק רשומה זו?", "אישור מחיקה",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM tbDiscountHeaders WHERE ID = @id", connection);
                        cmd.Parameters.AddWithValue("@id", currentRecordId);
                        cmd.ExecuteNonQuery();

                        XtraMessageBox.Show("הרשומה נמחקה בהצלחה", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        currentRecordId = null;
                        ClearFields();
                        LoadData();
                    }
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
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
