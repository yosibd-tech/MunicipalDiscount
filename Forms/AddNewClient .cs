using DevExpress.XtraEditors;
using MunicipalDiscount.Classes;
using System;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    public partial class frmAddNewClient : XtraForm
    {
        public string ClientNumber => txtClientNumber.Text.Trim();
        public string ClientName => txtClientName.Text.Trim();

        public frmAddNewClient()
        {
            InitializeComponent();
        }

        public frmAddNewClient(string clientNumber) : this()
        {
            txtClientNumber.Text = clientNumber;
        }

        private void frmAddNewClient_Load(object sender, EventArgs e)
        {
            ThemeHelper.ApplyTheme(this);
            this.IconOptions.SvgImage = MunicipalDiscount.Classes.AppIcons.AddClient;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;

            if (!string.IsNullOrEmpty(txtClientNumber.Text))
                txtClientName.Focus();
            else
                txtClientNumber.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtClientNumber.Text))
            {
                XtraMessageBox.Show("חובה להזין מספר לקוח", "שדה חובה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtClientNumber.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtClientName.Text))
            {
                XtraMessageBox.Show("חובה להזין שם לקוח", "שדה חובה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtClientName.Focus();
                return;
            }

            // Check if year is selected
            if (!SessionData.HasYear)
            {
                XtraMessageBox.Show("אנא בחר שנה בסרגל הכלים", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save to database
            if (SaveToDatabase())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool SaveToDatabase()
        {
            string connString = DatabaseConfig.ConnectionString;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // Parse client number
                if (!long.TryParse(txtClientNumber.Text.Trim(), out long clientNumberLong))
                {
                    XtraMessageBox.Show("מספר לקוח חייב להיות מספר", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                string clientNumber = clientNumberLong.ToString();

                // מספר הנכס נדרש. בלעדיו נוצרת שורת שנה שאין לה נכס, וקליטת
                // החיוב — שמתאימה לפי לקוח + שנה + מספר נכס — לא תמצא אותה
                // לעולם: היא תיצור שורה נפרדת לנכס האמיתי, וההחלטה תישאר תלויה
                // על שורה ריקה שלא תתמלא לעולם. המספר מופיע בשובר הארנונה.
                string assetNumber = txtAssetNumber.Text.Trim();
                if (string.IsNullOrEmpty(assetNumber))
                {
                    XtraMessageBox.Show(
                        "יש להזין מספר נכס.\n\n" +
                        "בלעדיו החיוב השנתי לא יתחבר ללקוח, ולא ניתן יהיה לחשב סכום הנחה.",
                        "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtAssetNumber.Focus();
                    return false;
                }

                if (!long.TryParse(assetNumber, out _))
                {
                    XtraMessageBox.Show("מספר נכס חייב להיות מספר", "שגיאה",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtAssetNumber.Focus();
                    return false;
                }

                int year = SessionData.SelectedYear.Value;

                using (var conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Check whether the citizen already has a row for this year.
                    // The citizen may well exist in tbClients from another year —
                    // that is not a duplicate, so the check is on the year row.
                    string checkSql = @"SELECT COUNT(*)
                                        FROM   tbYearlyDiscounts y
                                        INNER  JOIN tbClients c ON c.ID = y.ClientId
                                        WHERE  c.ClientNumber = @ClientNumber
                                          AND  y.Year_ = @Year";
                    using (var checkCmd = new SqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@ClientNumber", clientNumber);
                        checkCmd.Parameters.Add("@Year", System.Data.SqlDbType.Int).Value = year;

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            XtraMessageBox.Show($"לקוח עם מספר זה כבר קיים לשנת {year}", "לקוח קיים", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    // The citizen, then the year row — carrying the property
                    // number, so the next billing import matches it and fills in
                    // AnnualBilling and AssetSize.
                    //
                    // AssetCode is set now rather than waited for: this screen is
                    // reached only from the eligibility check, which deals with
                    // מגורים alone, so a clerk adding a property here is stating
                    // it is a dwelling. Left empty, the row would be invisible to
                    // the eligibility sum and to both reports until the next
                    // billing import — months, for a citizen standing at the
                    // counter. The code is read from tbGAssetKinds, never written
                    // as a literal, and a later billing import overrides it.
                    int clientId = DiscountRepository.EnsureClient(conn, null,
                        clientNumber, txtClientName.Text.Trim(), null, null);

                    int? residentialCode = DiscountRepository.GetResidentialAssetCode(conn);

                    DiscountRepository.EnsureYearRow(conn, null, clientId, year, assetNumber,
                        assetCode: residentialCode);
                }

                XtraMessageBox.Show("לקוח חדש נשמר בהצלחה!", "שמירה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בשמירה:\n{ex.Message}", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
