using DevExpress.XtraEditors;
using Microsoft.Data.SqlClient;
using MunicipalDiscount.Classes;
using MunicipalDiscount.Classes.Security;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    /// <summary>
    /// הגדרות החיבור למסד הנתונים — שרת, מסד, משתמש וסיסמה.
    ///
    /// Stands alone on purpose. Every other screen needs a working database to
    /// open, and this is the screen you need precisely when there isn't one —
    /// so it touches no session, no permissions and no parameters, and it is
    /// reachable before login and from the command line with --setup.
    ///
    /// It replaces MunicipalDiscount.SecretsSetup.exe: the password is encrypted
    /// here with the same SecretProtector, so there is no separate tool, nothing
    /// typed in the clear on a console, and no Base64 to copy by hand.
    /// </summary>
    public partial class ConnectionSetupForm : DevExpress.XtraEditors.XtraForm
    {
        /// <summary>The file the application actually reads, beside the executable.</summary>
        private static string SettingsPath =>
            Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        private string _existingEncryptedPassword = "";

        public ConnectionSetupForm()
        {
            InitializeComponent();
        }

        private void ConnectionSetupForm_Load(object sender, EventArgs e)
        {
            ThemeHelper.ApplyTheme(this);
            lblFile.Text = "הקובץ: " + SettingsPath;
            LoadSettings();
            txtServer.Focus();
        }

        #region Reading and writing appsettings.json

        private void LoadSettings()
        {
            // A missing or unreadable file is the normal case on a fresh install,
            // not an error: the boxes simply start empty.
            try
            {
                if (!File.Exists(SettingsPath)) return;

                JsonNode? root = JsonNode.Parse(File.ReadAllText(SettingsPath));
                JsonNode? cs = root?["ConnectionString"];
                if (cs is null) return;

                txtServer.Text   = cs["Server"]?.GetValue<string>() ?? "";
                txtDatabase.Text = cs["Database"]?.GetValue<string>() ?? "";
                txtUser.Text     = cs["User"]?.GetValue<string>() ?? "";
                txtOptions.Text  = cs["AdditionalOptions"]?.GetValue<string>() ?? "";

                _existingEncryptedPassword = cs["EncryptedPassword"]?.GetValue<string>() ?? "";

                lblPasswordNote.Text = string.IsNullOrEmpty(_existingEncryptedPassword)
                    ? "לא נשמרה סיסמה — יש להזין סיסמה."
                    : "סיסמה שמורה. יש להשאיר ריק כדי לא לשנות אותה.";
            }
            catch (Exception ex)
            {
                ShowStatus($"לא ניתן לקרוא את קובץ ההגדרות: {ex.Message}", isError: true);
            }
        }

        /// <summary>
        /// כותב רק את שדות החיבור, ומשאיר כל מפתח אחר בקובץ כפי שהוא — הקובץ
        /// עשוי לשאת הגדרות נוספות שאין למסך הזה עסק איתן.
        /// </summary>
        private bool SaveSettings(string encryptedPassword)
        {
            try
            {
                JsonNode root = File.Exists(SettingsPath)
                    ? JsonNode.Parse(File.ReadAllText(SettingsPath)) ?? new JsonObject()
                    : new JsonObject();

                if (root["ConnectionString"] is not JsonObject)
                    root["ConnectionString"] = new JsonObject();

                JsonNode cs = root["ConnectionString"]!;
                cs["Server"]            = txtServer.Text.Trim();
                cs["Database"]          = txtDatabase.Text.Trim();
                cs["User"]              = txtUser.Text.Trim();
                cs["EncryptedPassword"] = encryptedPassword;
                cs["AdditionalOptions"] = txtOptions.Text.Trim();

                File.WriteAllText(SettingsPath,
                    root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                ShowStatus("אין הרשאת כתיבה לקובץ ההגדרות. יש להפעיל את התוכנה כמנהל, " +
                           "או להתקין אותה בתיקייה שאינה תחת Program Files.", isError: true);
            }
            catch (Exception ex)
            {
                ShowStatus($"שמירה נכשלה: {ex.Message}", isError: true);
            }
            return false;
        }

        #endregion

        #region Building and testing the connection

        /// <summary>
        /// The password typed now, or the stored one when the box was left empty.
        /// Returns false when neither is usable, with the reason in the status line.
        /// </summary>
        private bool TryGetPassword(out string password, out string encrypted)
        {
            password = "";
            encrypted = _existingEncryptedPassword;

            string typed = txtPassword.Text;

            if (!string.IsNullOrEmpty(typed))
            {
                password = typed;
                encrypted = SecretProtector.Encrypt(typed);
                return true;
            }

            if (string.IsNullOrEmpty(_existingEncryptedPassword))
            {
                ShowStatus("יש להזין סיסמה — לא נשמרה סיסמה קודמת.", isError: true);
                return false;
            }

            try
            {
                password = SecretProtector.Decrypt(_existingEncryptedPassword);
                return true;
            }
            catch (Exception)
            {
                // The stored value was produced on a different machine: DPAPI is
                // LocalMachine-scoped, so it cannot be read here at all.
                ShowStatus("הסיסמה השמורה הוצפנה במחשב אחר ולא ניתן לפענח אותה כאן. " +
                           "יש להזין את הסיסמה מחדש.", isError: true);
                return false;
            }
        }

        private string BuildConnectionString(string password) =>
            $"Server={txtServer.Text.Trim()};Database={txtDatabase.Text.Trim()};" +
            $"User Id={txtUser.Text.Trim()};Password={password};{txtOptions.Text.Trim()}";

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                ShowStatus("יש להזין שם שרת.", isError: true);
                txtServer.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                ShowStatus("יש להזין שם מסד נתונים.", isError: true);
                txtDatabase.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUser.Text))
            {
                ShowStatus("יש להזין שם משתמש.", isError: true);
                txtUser.Focus();
                return false;
            }

            return true;
        }

        private void ShowStatus(string message, bool isError)
        {
            lblStatus.Text = message;
            lblStatus.Appearance.ForeColor = isError
                ? Color.FromArgb(170, 40, 30)
                : Color.FromArgb(30, 110, 70);
        }

        #endregion

        #region Event handlers

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;
            if (!TryGetPassword(out string password, out _)) return;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using var connection = new SqlConnection(BuildConnectionString(password));
                connection.Open();

                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tbClients'", connection);
                bool looksRight = Convert.ToInt32(cmd.ExecuteScalar()) > 0;

                ShowStatus(looksRight
                    ? "החיבור הצליח, והמסד מכיל את טבלאות המערכת."
                    : "החיבור הצליח, אך לא נמצאה הטבלה tbClients — ייתכן שנבחר מסד נתונים שגוי.",
                    isError: !looksRight);
            }
            catch (Exception ex)
            {
                ShowStatus($"החיבור נכשל: {ex.Message}", isError: true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;
            if (!TryGetPassword(out string password, out string encrypted)) return;

            // Saving a connection that does not work would leave the application
            // unable to start and this screen the only way back, so it is tested
            // first and a failure has to be confirmed deliberately.
            bool connects;
            try
            {
                using var connection = new SqlConnection(BuildConnectionString(password));
                connection.Open();
                connects = true;
            }
            catch (Exception ex)
            {
                connects = false;
                ShowStatus($"החיבור נכשל: {ex.Message}", isError: true);
            }

            if (!connects &&
                XtraMessageBox.Show("החיבור לא הצליח. לשמור את ההגדרות בכל זאת?",
                    "אישור שמירה", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            if (!SaveSettings(encrypted)) return;

            // The connection string is cached after its first use, so a save
            // that did not clear it would appear to have done nothing.
            DatabaseConfig.Reset();
            _existingEncryptedPassword = encrypted;
            txtPassword.Text = "";

            ShowStatus("ההגדרות נשמרו.", isError: false);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion
    }
}
