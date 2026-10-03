using DevExpress.XtraEditors;
using MunicipalDiscount.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    public partial class DiscountEligibilityChecks : DevExpress.XtraEditors.XtraForm
    {
        private string connectionString = DatabaseConfig.ConnectionString;
        private decimal _currentDiscountRate = 0;
        private DataTable _membersTable;
        private string _wifeNumber = "";

        // The three relation values the clerk picks from in the "סוג" column.
        // Only a בגיר ילד is halved, so the kind has to be recorded per row —
        // age on its own cannot tell a 21-year-old child from a spouse.
        private const string RelationHead   = "ראש משפחה";
        private const string RelationSpouse = "בן זוג";
        private const string RelationChild  = "ילד";

        // גיל at which a ילד counts as בגיר for the halving rule.
        private const int AdultAge = 18;


        public DiscountEligibilityChecks()
        {
            InitializeComponent();
            InitializeMembersGrid();
        }

        private void InitializeMembersGrid()
        {
            _membersTable = new DataTable();
            _membersTable.Columns.Add("מספר", typeof(int));
            _membersTable.Columns.Add("שם", typeof(string));
            _membersTable.Columns.Add("סוג", typeof(string));
            _membersTable.Columns.Add("גיל", typeof(int));
            _membersTable.Columns.Add("הכנסה", typeof(decimal));

            AddRelationColumn();

            gridMembers.DataSource = _membersTable;
        }

        /// <summary>
        /// Builds the "סוג" column in code rather than in the designer, so the
        /// generated file stays untouched. It sits between שם and גיל, and is a
        /// closed dropdown — a free-typed value would break the halving rule.
        /// </summary>
        private void AddRelationColumn()
        {
            var riRelation = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox
            {
                AutoHeight = false,
                Name = "riRelation",
                TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor
            };
            riRelation.Items.AddRange(new[] { RelationHead, RelationSpouse, RelationChild });
            gridMembers.RepositoryItems.Add(riRelation);

            var colRelation = new DevExpress.XtraGrid.Columns.GridColumn
            {
                Caption    = "סוג",
                FieldName  = "סוג",
                Name       = "colRelation",
                ColumnEdit = riRelation,
                Visible    = true,
                Width      = 90
            };

            viewMembers.Columns.Add(colRelation);

            colRelation.VisibleIndex = 2;
            colAge.VisibleIndex      = 3;
            colIncome.VisibleIndex   = 4;
        }

        private static string GetRelation(DataRow row) =>
            row["סוג"]?.ToString()?.Trim() ?? "";

        private static bool IsAdultChild(DataRow row) =>
            GetRelation(row) == RelationChild && Convert.ToInt32(row["גיל"]) >= AdultAge;

        private void DiscountEligibilityChecks_Load(object sender, EventArgs e)
        {
            ThemeHelper.ApplyTheme(this);
            this.IconOptions.SvgImage = MunicipalDiscount.Classes.AppIcons.Eligibility;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;
            txtClientNumber.Focus();
        }

        #region Family Members Grid

        private void btnAddMember_Click(object sender, EventArgs e)
        {
            int nextNumber = _membersTable.Rows.Count + 1;

            // First row is the citizen, second the spouse, anything after a child —
            // the clerk can override any of them in the dropdown.
            string relation = nextNumber == 1 ? RelationHead
                            : nextNumber == 2 ? RelationSpouse
                            : RelationChild;

            _membersTable.Rows.Add(nextNumber, "", relation, 0, 0m);
        }

        private void btnRemoveMember_Click(object sender, EventArgs e)
        {
            int rowHandle = viewMembers.FocusedRowHandle;
            if (rowHandle < 0) return;

            int dataRowIndex = viewMembers.GetDataSourceRowIndex(rowHandle);
            if (dataRowIndex < 0 || dataRowIndex >= _membersTable.Rows.Count) return;

            _membersTable.Rows.RemoveAt(dataRowIndex);

            for (int i = 0; i < _membersTable.Rows.Count; i++)
            {
                _membersTable.Rows[i]["מספר"] = i + 1;
            }
        }

        #endregion

        #region Client Search

        private void txtClientNumber_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            SearchClient();
        }

        private void txtClientNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchClient();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void SearchClient()
        {
            string clientNumber = txtClientNumber.Text.Trim();
            if (string.IsNullOrEmpty(clientNumber))
            {
                XtraMessageBox.Show("אנא הזן מספר לקוח", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!long.TryParse(clientNumber, out long clientNumberLong))
            {
                XtraMessageBox.Show("מספר לקוח חייב להיות מספר", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!SessionData.HasYear)
            {
                XtraMessageBox.Show("אנא בחר שנה בסרגל הכלים", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // The citizen's details no longer depend on a year — tbClients
                    // holds them once.
                    string query = @"
                        SELECT ClientName, ClieneWifeNumber
                        FROM   tbClients
                        WHERE  ClientNumber = @clientNum";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@clientNum", clientNumber);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtClientName.Text = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            _wifeNumber       = reader.IsDBNull(1) ? "" : reader.GetString(1);

                            // The citizen gets a row of their own — the divisor is
                            // every נפש in the household, so leaving the head out
                            // would under-count the family by one.
                            _membersTable.Rows.Clear();
                            _membersTable.Rows.Add(1, txtClientName.Text, RelationHead,   0, 0m);
                            _membersTable.Rows.Add(2, "בת/בן זוג",        RelationSpouse, 0, 0m);
                            _membersTable.Rows.Add(3, "ילד ראשון",        RelationChild,  0, 0m);
                            _membersTable.Rows.Add(4, "ילד שני",          RelationChild,  0, 0m);
                            _membersTable.Rows.Add(5, "ילד שלישי",        RelationChild,  0, 0m);

                            btnAddMember.Focus();
                        }
                        else
                        {
                            XtraMessageBox.Show("לקוח לא נמצא", "הודעה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txtClientName.Text = "";
                            _wifeNumber = "";
                            txtClientNumber.Focus();
                            txtClientNumber.SelectAll();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בחיפוש לקוח:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Add New Client

        private void btnAddNewClient_Click(object sender, EventArgs e)
        {
            if (!PermissionManager.HasPermission(PermissionManager.AddNewClient))
            {
                XtraMessageBox.Show("אין לך הרשאה לביצוע פעולה זו", "הרשאה נדרשת",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var frm = new frmAddNewClient(txtClientNumber.Text.Trim()))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    // Update form with new client data
                    txtClientNumber.Text = frm.ClientNumber;
                    txtClientName.Text = frm.ClientName;
                    btnAddMember.Focus();
                }
            }
        }

        #endregion

        #region Select All Documents

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = chkSelectAll.Checked;

            for (int i = 0; i < chkDocuments.Items.Count; i++)
            {
                chkDocuments.SetItemChecked(i, isChecked);
            }
        }

        #endregion

        #region Calculate Discount

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtClientNumber.Text.Trim()))
                {
                    XtraMessageBox.Show("אנא הזן מספר לקוח", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtClientNumber.Focus();
                    return;
                }

                if (!SessionData.HasYear)
                {
                    XtraMessageBox.Show("אנא בחר שנה בסרגל הכלים", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // The citizen's row is looked up for the year being worked on —
                // there is no "previous year" row to read the code from any more.
                int selectedYear = SessionData.SelectedYear.Value;
                int currentYear = selectedYear;

                // Check if current year exists in tbIncomeDiscountCriteria
                if (!IsYearExistsInCriteria(currentYear))
                {
                    XtraMessageBox.Show($"שנה {currentYear} לא קיימת בטבלת קריטריונים להנחה.\nאנא בחר שנה אחרת או עדכן את הטבלה.",
                        "שנה לא נמצאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Commit any open editor in the grid before reading values
                viewMembers.CloseEditor();
                viewMembers.UpdateCurrentRow();

                int familySize = _membersTable.Rows.Count;
                if (familySize == 0)
                {
                    XtraMessageBox.Show("יש להזין לפחות בן משפחה אחד.", "שגיאה",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // One בגיר ילד is counted at half — the OLDEST of them. Every other
                // member is counted in full, minors included.
                //
                // Ties keep grid order: the comparison is strict, so the first row
                // of that age wins and a second child the same age cannot displace
                // it. Twins therefore give a stable answer rather than one that
                // depends on row order changing.
                DataRow halvedChild = null;
                int oldestChildAge = -1;
                foreach (DataRow row in _membersTable.Rows)
                {
                    if (!IsAdultChild(row)) continue;

                    int childAge = Convert.ToInt32(row["גיל"]);
                    if (childAge > oldestChildAge)
                    {
                        oldestChildAge = childAge;
                        halvedChild    = row;
                    }
                }

                decimal countedIncome = 0;
                foreach (DataRow row in _membersTable.Rows)
                {
                    decimal income = Convert.ToDecimal(row["הכנסה"]);
                    countedIncome += ReferenceEquals(row, halvedChild) ? income / 2m : income;
                }

                // The divisor is every row in the grid — all נפשות, minors included.
                decimal totalIncome = countedIncome / familySize;

                txtTotalIncome.Text = totalIncome.ToString("N0");

                // Phase A — the date that decides which rate version applies.
                DateTime requestDate = GetRequestDate(txtClientNumber.Text.Trim());

                // Phase B — the entitlement the income test gives.
                decimal discountRate = GetIncomeDiscountRate(
                    totalIncome, familySize, selectedYear, requestDate,
                    out string criteriaTrace, out int municipalCode);

                _currentDiscountRate = discountRate;

                string municipalName   = "";
                decimal maximumDiscount = 0;

                if (discountRate > 0)
                {
                    // Phase C — the cap the mayor set on the code the bracket names.
                    GetIncomeTestCode(municipalCode, requestDate,
                        out municipalName, out maximumDiscount);

                    // Phase D — the cap wins when it is the lower of the two.
                    if (maximumDiscount > 0 && discountRate > maximumDiscount)
                    {
                        discountRate = maximumDiscount;
                        _currentDiscountRate = discountRate;
                        XtraMessageBox.Show(
                            $"שיעור ההנחה הופחת ל-{maximumDiscount * 100}% בהתאם למגבלת ראש העיר.",
                            "הגבלת הנחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                if (discountRate > 0)
                {
                    txtDiscountRate.Text = $"{discountRate * 100}%";
                }
                else
                {
                    txtDiscountRate.Text = "לא זכאי להנחה";
                }

                ShowCalculationTrace(requestDate, familySize, countedIncome, totalIncome,
                    halvedChild, criteriaTrace, municipalCode, municipalName, maximumDiscount);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בחישוב הנחה:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private decimal GetDecimalValue(string text)
        {
            text = text.Replace(",", "");
            if (decimal.TryParse(text, out decimal value))
                return value;
            return 0;
        }

        /// <summary>
        /// Phase A — the citizen's most recent request date, which is what picks
        /// the criteria version and the rate version. tbDiscountRequests carries no
        /// client number of its own, so it is reached through the year row.
        /// Nothing else is read from that table here. Falls back to today.
        /// </summary>
        private DateTime GetRequestDate(string clientNumber)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT TOP 1 r.RequestDate
                        FROM   tbDiscountRequests r
                        INNER  JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
                        INNER  JOIN tbClients        c ON c.ID = y.ClientId
                        WHERE  c.ClientNumber = @clientNum
                          AND  r.RequestDate IS NOT NULL
                        ORDER BY r.RequestDate DESC";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.Add("@clientNum", System.Data.SqlDbType.NVarChar, 20).Value = clientNumber;

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToDateTime(result);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בקריאת תאריך הבקשה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return DateTime.Today;
        }

        /// <summary>
        /// Phase B — the entitlement from tbIncomeDiscountCriteria, read from the
        /// version in force on the request date rather than from whatever row the
        /// year happens to hold. Brackets touch at their edges (a boundary amount
        /// falls in two rows), so the higher rate is taken.
        /// </summary>
        private decimal GetIncomeDiscountRate(decimal totalIncome, int familySize,
            int year, DateTime requestDate, out string trace, out int municipalCode)
        {
            trace = "לא נמצאה שורת קריטריון";
            municipalCode = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT TOP 1 ID, DiscountRate, ValidityDate, FromAmount, ToAmount,
                                     MunicipalDiscountCode
                        FROM   tbIncomeDiscountCriteria
                        WHERE  NumberOfPeople = @numberOfPeople
                          AND  @income BETWEEN FromAmount AND ToAmount
                          AND  Year_ = @year
                          AND  ValidityDate = (
                                   SELECT MAX(ValidityDate)
                                   FROM   tbIncomeDiscountCriteria
                                   WHERE  Year_ = @year
                                     AND  ValidityDate <= @requestDate)
                        ORDER BY DiscountRate DESC";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.Add("@numberOfPeople", System.Data.SqlDbType.Int).Value = familySize;
                    cmd.Parameters.Add("@income", System.Data.SqlDbType.Decimal).Value = totalIncome;
                    cmd.Parameters.Add("@year", System.Data.SqlDbType.Int).Value = year;
                    cmd.Parameters.Add("@requestDate", System.Data.SqlDbType.Date).Value = requestDate.Date;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal rate = reader.GetDecimal(1);

                            // הקוד מגיע מהמדרגה עצמה, לא מחיפוש לפי האחוז.
                            municipalCode = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);

                            trace = $"ID={reader.GetInt32(0)}, " +
                                    $"תוקף={reader.GetDateTime(2):dd/MM/yy}, " +
                                    $"מדרגה={reader.GetDecimal(3):N0}-{reader.GetDecimal(4):N0}, " +
                                    $"אחוז={rate * 100}%";
                            return rate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בקריאת אחוזי הנחה:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return 0;
        }

        /// <summary>
        /// Phase C — the מבחן הכנסה code that carries the entitlement percent on the
        /// request date, with the cap attached to that version of it. Read from
        /// vwDiscountRateAsOf, which closes each rate's validity window with LEAD();
        /// tbDiscountCodeRates itself holds no code number and no end date.
        /// </summary>
        private void GetIncomeTestCode(int municipalCode, DateTime requestDate,
            out string discountName, out decimal maximumDiscount)
        {
            discountName    = "";
            maximumDiscount = 0;

            if (municipalCode <= 0)
                return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT TOP 1 v.DiscountName, v.MaximumDiscount
                        FROM   vwDiscountRateAsOf v
                        WHERE  v.MunicipalDiscountCode = @code
                          AND  @requestDate >= v.RateFrom
                          AND (v.RateUntil IS NULL OR @requestDate < v.RateUntil)
                        ORDER BY v.RateFrom DESC";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.Add("@code", System.Data.SqlDbType.Int).Value = municipalCode;
                    cmd.Parameters.Add("@requestDate", System.Data.SqlDbType.Date).Value = requestDate.Date;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            discountName    = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            maximumDiscount = reader.IsDBNull(1) ? 0  : reader.GetDecimal(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בקריאת מגבלת הנחה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Hangs the whole derivation off the result field as a tooltip, so the
        /// clerk can see which criteria row and which code produced the number
        /// without the calculation interrupting them with a message box.
        /// </summary>
        private void ShowCalculationTrace(DateTime requestDate, int familySize,
            decimal countedIncome, decimal averageIncome, DataRow halvedChild,
            string criteriaTrace, int municipalCode, string municipalName, decimal maximumDiscount)
        {
            var trace = new StringBuilder();
            trace.AppendLine($"תאריך בקשה: {requestDate:dd/MM/yy}");
            trace.AppendLine($"נפשות: {familySize}");

            if (halvedChild != null)
            {
                string childName = halvedChild["שם"]?.ToString() ?? "";
                trace.AppendLine($"חצי הכנסה לילד בגיר: {childName} " +
                                 $"({Convert.ToDecimal(halvedChild["הכנסה"]):N0})");
            }
            else
            {
                trace.AppendLine("אין ילד בגיר להפחתה");
            }

            trace.AppendLine($"הכנסה נספרת: {countedIncome:N0}");
            trace.AppendLine($"הכנסה ממוצעת: {averageIncome:N0}");
            trace.AppendLine($"קריטריון: {criteriaTrace}");

            if (municipalCode > 0)
            {
                trace.AppendLine($"קוד הנחה: {municipalCode} {municipalName}");
                trace.AppendLine($"מגבלת ראש העיר: {maximumDiscount * 100}%");
            }

            trace.Append($"אחוז סופי: {(_currentDiscountRate > 0 ? $"{_currentDiscountRate * 100}%" : "לא זכאי")}");

            txtDiscountRate.ToolTip = trace.ToString();
        }

        private bool IsYearExistsInCriteria(int year)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM tbIncomeDiscountCriteria WHERE Year_ = @year";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.Add("@year", System.Data.SqlDbType.Int).Value = year;

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בבדיקת שנה:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region Confirm Discount

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                string clientNumber = txtClientNumber.Text.Trim();
                if (string.IsNullOrEmpty(clientNumber))
                {
                    XtraMessageBox.Show("אנא הזן מספר לקוח", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(txtDiscountRate.Text))
                {
                    XtraMessageBox.Show("אנא חשב את ההנחה לפני האישור", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                if (!SessionData.HasYear)
                {
                    XtraMessageBox.Show("אנא בחר שנה בסרגל הכלים", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int selectedYear = SessionData.SelectedYear.Value;
                decimal discountPercent = _currentDiscountRate;
                DateTime requestDate = DateTime.Today;

                DialogResult confirm = XtraMessageBox.Show(
                    $"האם לעדכן אחוז הנחה {_currentDiscountRate * 100}% ללקוח {clientNumber} לשנת {selectedYear}?",
                    "אישור עדכון", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // The check fills in the citizen's existing request for that
                    // year — the imported line that says a discount was granted,
                    // but not on what entitlement. It is stamped with the date it
                    // was decided, which is what picks the rate version, and its
                    // Status turns 1.
                    bool saved = DiscountRepository.SaveEligibilityDecision(
                        connection, null, clientNumber, selectedYear,
                        requestDate, discountPercent, SessionData.CurrentUserId);

                    if (saved)
                    {
                        XtraMessageBox.Show("אחוז ההנחה עודכן בהצלחה", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        XtraMessageBox.Show("לא נמצא לקוח לעדכון", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בעדכון אחוז הנחה:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Print Report

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtClientNumber.Text.Trim()))
            {
                XtraMessageBox.Show("אנא הזן מספר לקוח", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtDiscountRate.Text))
            {
                XtraMessageBox.Show("אנא חשב את ההנחה לפני ההדפסה", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DiscountEligibilityReport report = CreateReport();
                report.PrintPreview();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בהדפסה:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Save PDF

        private void btnSavePdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtClientNumber.Text.Trim()))
            {
                XtraMessageBox.Show("אנא הזן מספר לקוח", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtDiscountRate.Text))
            {
                XtraMessageBox.Show("אנא חשב את ההנחה לפני השמירה", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                saveDialog.FileName = $"בדיקת_זכאות_{txtClientNumber.Text}_{DateTime.Now:yyyyMMdd}";
                saveDialog.Title = "שמור דו\"ח כ-PDF";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    DiscountEligibilityReport report = CreateReport();
                    report.SaveToPdf(saveDialog.FileName);
                    XtraMessageBox.Show($"הקובץ נשמר בהצלחה:\n{saveDialog.FileName}", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בשמירת PDF:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Helper Methods

        private DiscountEligibilityReport CreateReport()
        {
            // Get checked documents
            List<string> checkedDocs = new List<string>();
            foreach (var item in chkDocuments.CheckedItems)
            {
                checkedDocs.Add(item.ToString());
            }

            return new DiscountEligibilityReport
            {
                ClientNumber = txtClientNumber.Text.Trim(),
                ClientName = txtClientName.Text.Trim(),
                FamilySize = _membersTable.Rows.Count,
                FamilyMembers = _membersTable.Rows
                    .Cast<System.Data.DataRow>()
                    .Select(r => (
                        Name:   r["שם"]?.ToString() ?? "",
                        Age:    Convert.ToInt32(r["גיל"]),
                        Income: Convert.ToDecimal(r["הכנסה"])
                    )).ToList(),
                TotalIncome = GetDecimalValue(txtTotalIncome.Text),
                DiscountRate = txtDiscountRate.Text,
                Year = SessionData.SelectedYear ?? DateTime.Now.Year,
                WorkerName = SessionData.UserFullName ?? "",
                CheckedDocuments = checkedDocs
            };
        }

        #endregion
            

               

        #region Clear and Close

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtClientNumber.Text = "";
            txtClientName.Text = "";
            _wifeNumber = "";
            _membersTable.Rows.Clear();
            txtTotalIncome.Text = "";
            txtDiscountRate.Text = "";
            _currentDiscountRate = 0;

            // Uncheck all documents
            chkSelectAll.Checked = false;
            for (int i = 0; i < chkDocuments.Items.Count; i++)
            {
                chkDocuments.SetItemChecked(i, false);
            }

            txtClientNumber.Focus();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
