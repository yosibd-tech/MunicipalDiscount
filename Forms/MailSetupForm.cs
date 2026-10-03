using DevExpress.XtraEditors;
using Microsoft.Data.SqlClient;
using MunicipalDiscount.Classes;
using MunicipalDiscount.Classes.Security;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    /// <summary>
    /// הגדרות הדואר היוצא — שרת, פורט, כתובת שולח וסיסמה.
    ///
    /// Exists because the settings had nowhere to be typed. MailSender reads
    /// MailSmtpHost / MailSmtpPort / MailUseSsl / MailFrom / MailUser /
    /// MailPassword from tbMainParameters, and the password is DPAPI-encrypted
    /// like the database password — so the parameters grid, which stores what
    /// it is given verbatim, could never produce a working one. Anyone who
    /// tried got "המפתח הוצפן במחשב אחר" and no way forward.
    ///
    /// The test button sends through the values on screen rather than through
    /// what is saved: settings that do not work should fail before they are
    /// stored, not the next time someone presses שלח דוח למנהלים.
    /// </summary>
    public partial class MailSetupForm : XtraForm
    {
        private readonly string connectionString = DatabaseConfig.ConnectionString;

        /// <summary>הסיסמה כפי ששמורה — מוצפנת. ריק בשדה משמעו "אל תשנה".</summary>
        private string _existingEncryptedPassword = "";

        public MailSetupForm()
        {
            InitializeComponent();
        }

        private void MailSetupForm_Load(object sender, EventArgs e)
        {
            ThemeHelper.ApplyTheme(this);
            this.IconOptions.SvgImage = AppIcons.Parameters;
            this.IconOptions.SvgImageSize = new Size(32, 32);
            this.IconOptions.ShowIcon = true;

            LoadFromDb();
            txtHost.Focus();
        }

        #region Reading and writing tbMainParameters

        private static readonly string[] Keys =
        {
            "MailSendMethod",
            "MailSmtpHost", "MailSmtpPort", "MailUseSsl", "MailFrom", "MailUser", "MailPassword",
        };

        /// <summary>האם השליחה היא דרך Outlook המותקן.</summary>
        private bool UseOutlook => (radioMethod.EditValue as string) == "outlook";

        private void LoadFromDb()
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var conn = new SqlConnection(connectionString);
                conn.Open();

                using var cmd = new SqlCommand("SELECT [Key], [Value] FROM tbMainParameters", conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string key = reader.GetString(0);
                    if (Keys.Contains(key, StringComparer.OrdinalIgnoreCase))
                        values[key] = reader.IsDBNull(1) ? "" : reader.GetString(1);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"לא ניתן לקרוא את הפרמטרים: {ex.Message}", isError: true);
                return;
            }

            string Get(string key) => values.TryGetValue(key, out string? v) ? (v ?? "").Trim() : "";

            radioMethod.EditValue =
                Get("MailSendMethod").Equals("outlook", StringComparison.OrdinalIgnoreCase)
                    ? "outlook" : "smtp";

            ApplyMethod();

            txtHost.Text = Get("MailSmtpHost");
            txtFrom.Text = Get("MailFrom");
            txtUser.Text = Get("MailUser");

            spinPort.Value = int.TryParse(Get("MailSmtpPort"), out int port) && port > 0 ? port : 587;

            string ssl = Get("MailUseSsl");
            chkSsl.Checked = string.IsNullOrEmpty(ssl)
                || (!ssl.Equals("false", StringComparison.OrdinalIgnoreCase) && ssl != "0");

            _existingEncryptedPassword = Get("MailPassword");

            if (string.IsNullOrEmpty(_existingEncryptedPassword))
            {
                lblPasswordNote.Text = "לא נשמרה סיסמה. היא תישמר מוצפנת, וניתנת לפענוח במחשב הזה בלבד.";
            }
            else
            {
                // הסיסמה מוצגת בשדה, מוסתרת, וכפתור "הצג" חושף אותה: מי שמגדיר
                // שרת דואר צריך לראות מה שמור שם, וסיסמה שהוצפנה במחשב אחר
                // נראית שמורה ואינה עובדת — עדיף לומר זאת כאן ולא בעת השליחה.
                try
                {
                    txtPassword.Text = SecretProtector.Decrypt(_existingEncryptedPassword);
                    lblPasswordNote.Text = "סיסמה שמורה, מוצפנת. לחיצה על \"הצג\" מציגה אותה.";
                }
                catch (Exception)
                {
                    lblPasswordNote.Text = "הסיסמה השמורה הוצפנה במחשב אחר ואינה ניתנת לפענוח כאן. יש להזין אותה מחדש.";
                }
            }

            // ברירת המחדל לבדיקה: הכתובת של המנהל הראשון, ואם אין — כתובת השולח.
            try
            {
                txtTestTo.Text = MailSender.GetAdminRecipients().FirstOrDefault() ?? txtFrom.Text;
            }
            catch (Exception)
            {
                txtTestTo.Text = txtFrom.Text;
            }
        }

        /// <summary>
        /// כותב את ששת המפתחות. MERGE, כמו במסך ניהול הפרמטרים — מפתח שאינו
        /// קיים נוצר, ומפתח קיים מתעדכן בלי לגעת בשאר הטבלה.
        /// </summary>
        private bool SaveToDb(string encryptedPassword)
        {
            var rows = new (string key, string value, string description)[]
            {
                ("MailSendMethod", UseOutlook ? "outlook" : "smtp",  "smtp / outlook"),
                ("MailSmtpHost", txtHost.Text.Trim(),                "שרת הדואר היוצא"),
                ("MailSmtpPort", ((int)spinPort.Value).ToString(),   "פורט השרת, ברירת מחדל 587"),
                ("MailUseSsl",   chkSsl.Checked ? "true" : "false",  "שימוש ב-SSL/TLS"),
                ("MailFrom",     txtFrom.Text.Trim(),                "כתובת השולח"),
                ("MailUser",     txtUser.Text.Trim(),                "שם המשתמש בשרת; ריק — כתובת השולח"),
                ("MailPassword", encryptedPassword,                  "סיסמת הדואר, מוצפנת"),
            };

            try
            {
                using var conn = new SqlConnection(connectionString);
                conn.Open();

                using var transaction = conn.BeginTransaction();

                foreach (var (key, value, description) in rows)
                {
                    using var cmd = new SqlCommand(@"
                        MERGE tbMainParameters AS t
                        USING (SELECT @k AS [Key]) AS s
                        ON t.[Key] = s.[Key]
                        WHEN MATCHED THEN UPDATE SET
                            [Value] = @v, [ParametersGroup] = @g
                        WHEN NOT MATCHED THEN
                            INSERT ([Key], [Value], [Description], [ParametersGroup])
                            VALUES (@k, @v, @d, @g);", conn, transaction);

                    cmd.Parameters.AddWithValue("@k", key);
                    cmd.Parameters.AddWithValue("@v", value);
                    cmd.Parameters.AddWithValue("@d", description);
                    cmd.Parameters.AddWithValue("@g", "דואר יוצא");

                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                ShowStatus($"שמירה נכשלה: {ex.Message}", isError: true);
                return false;
            }
        }

        #endregion

        #region Building the settings

        /// <summary>
        /// ההגדרות כפי שהן על המסך, לא כפי שהן שמורות — זה מה שהבדיקה אמורה
        /// לבדוק.
        /// </summary>
        private bool TryBuildSettings(out MailSender.Settings settings)
        {
            settings = new MailSender.Settings
            {
                Method = UseOutlook ? MailSender.SendMethod.Outlook : MailSender.SendMethod.Smtp,
                Host = txtHost.Text.Trim(),
                Port = (int)spinPort.Value,
                UseSsl = chkSsl.Checked,
                From = txtFrom.Text.Trim(),
                User = txtUser.Text.Trim(),
            };

            if (string.IsNullOrEmpty(settings.User)) settings.User = settings.From;

            // דרך Outlook אין סיסמה לבקש: ההודעה נפתחת בחשבון שכבר מחובר שם.
            if (UseOutlook) return true;

            if (!TryGetPassword(out string password, out _)) return false;

            settings.Password = password;
            return true;
        }

        /// <summary>
        /// הסיסמה בשתי צורותיה: גלויה לשליחה, מוצפנת לשמירה. מה שבשדה הוא מה
        /// שנשמר — השדה נטען מהסיסמה השמורה, ולכן אין עוד משמעות ל"ריק".
        /// </summary>
        private bool TryGetPassword(out string password, out string encrypted)
        {
            password = txtPassword.Text;
            encrypted = "";

            if (string.IsNullOrEmpty(password))
            {
                ShowStatus("יש להזין סיסמה.", isError: true);
                txtPassword.Focus();
                return false;
            }

            encrypted = SecretProtector.Encrypt(password);
            return true;
        }

        /// <summary>
        /// חושף ומסתיר את הסיסמה. סיסמה שאי אפשר לראות היא סיסמה שאי אפשר
        /// לבדוק מול מה שהמפעיל מחזיק ביד.
        /// </summary>
        private void txtPassword_ButtonClick(object sender,
            DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            bool hidden = txtPassword.Properties.UseSystemPasswordChar;

            txtPassword.Properties.UseSystemPasswordChar = !hidden;
            e.Button.Caption = hidden ? "הסתר" : "הצג";
        }

        /// <summary>
        /// שדות ה-SMTP נפתחים ונסגרים לפי הדרך שנבחרה — טופס שמבקש סיסמה
        /// שאינה בשימוש מזמין בדיוק את התקלה שבגללה המסך הזה נכתב.
        /// </summary>
        private void ApplyMethod()
        {
            grpServer.Enabled = !UseOutlook;

            lblTestTo.Text = UseOutlook ? "לפתוח בדיקה אל:" : "לשלוח בדיקה אל:";

            if (UseOutlook)
                ShowStatus("ההודעות ייפתחו ב-Outlook המותקן, מהחשבון המחובר בו.", isError: false);
        }

        private void radioMethod_SelectedIndexChanged(object sender, EventArgs e) => ApplyMethod();

        private bool ValidateFields()
        {
            // דרך Outlook השרת והשולח נקבעים שם, ולכן אין מה לאמת כאן.
            if (UseOutlook) return true;

            if (string.IsNullOrWhiteSpace(txtHost.Text))
            {
                ShowStatus("יש להזין את שם שרת הדואר.", isError: true);
                txtHost.Focus();
                return false;
            }

            if (!txtFrom.Text.Contains('@'))
            {
                ShowStatus("כתובת השולח אינה כתובת דואר תקינה.", isError: true);
                txtFrom.Focus();
                return false;
            }

            // שם המשתמש בשרת הוא כתובת דואר. שם המשתמש בתוכנה נכנס לכאן בטעות
            // טבעית, והשרת דוחה אותו בהודעה שאינה מסבירה מה קרה.
            if (txtUser.Text.Trim().Length > 0 && !txtUser.Text.Contains('@'))
            {
                ShowStatus("שם המשתמש בשרת הוא כתובת דואר מלאה. יש להשאיר ריק או להזין כתובת.",
                    isError: true);
                txtUser.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// ההודעה יושבת בתיבת טקסט לקריאה בלבד ולא בתווית — שגיאת שרת דואר היא
        /// בדיוק הטקסט שצריך לסמן, להעתיק ולשלוח למי שמבין בו.
        /// </summary>
        private void ShowStatus(string message, bool isError)
        {
            memStatus.Text = message;
            memStatus.Properties.Appearance.ForeColor = isError
                ? Color.FromArgb(170, 40, 30)
                : Color.FromArgb(30, 110, 70);
        }

        #endregion

        #region Event handlers

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            string to = txtTestTo.Text.Trim();
            if (!to.Contains('@'))
            {
                ShowStatus("יש להזין כתובת דואר לבדיקה.", isError: true);
                txtTestTo.Focus();
                return;
            }

            if (!TryBuildSettings(out var settings)) return;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                MailSender.Send(settings, new[] { to },
                    "בדיקת הגדרות דואר — MunicipalDiscount",
                    "<html dir='rtl'><body style=\"font-family:Segoe UI,Arial,sans-serif\">" +
                    "<p>הודעת בדיקה ממסך הגדרות הדואר.</p>" +
                    $"<p style='color:#666'>{Environment.MachineName} · " +
                    $"{DateTime.Now:dd/MM/yyyy HH:mm} · {SessionData.UserFullName}</p>" +
                    "</body></html>");

                ShowStatus(UseOutlook
                    ? $"הודעת בדיקה נפתחה ב-Outlook אל {to}."
                    : $"הודעת בדיקה נשלחה אל {to}.", isError: false);
            }
            catch (Exception ex)
            {
                ShowStatus($"השליחה נכשלה: {ex.Message}", isError: true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            // דרך Outlook הסיסמה השמורה נשארת כפי שהיא, ואינה נדרשת כדי לשמור.
            string encrypted = _existingEncryptedPassword;

            if (!UseOutlook && !TryGetPassword(out _, out encrypted)) return;

            if (!SaveToDb(encrypted)) return;

            _existingEncryptedPassword = encrypted;
            lblPasswordNote.Text = "סיסמה שמורה, מוצפנת. לחיצה על \"הצג\" מציגה אותה.";

            ShowStatus("ההגדרות נשמרו.", isError: false);
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// טוען את ההגדרות שכבר עובדות ב-MonicipalYBD (tbGSmtpSettings), שיושבת
        /// באותו מסד נתונים.
        ///
        /// The two applications talk to the same mail server from the same
        /// machine, so retyping a sixteen-character application password by
        /// hand is a transcription error waiting to happen. Nothing is written:
        /// the boxes are filled, the operator looks, and שמור is still theirs
        /// to press — at which point the password is stored encrypted here,
        /// rather than in the clear the way the other table keeps it.
        /// </summary>
        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                using var conn = new SqlConnection(connectionString);
                conn.Open();

                using var cmd = new SqlCommand(
                    "SELECT TOP 1 Host, Port, Username, Password, UseSsl, FromAddress " +
                    "FROM tbGSmtpSettings ORDER BY Id", conn);

                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    ShowStatus("לא נמצאו הגדרות דואר בטבלת tbGSmtpSettings.", isError: true);
                    return;
                }

                txtHost.Text     = reader.IsDBNull(0) ? "" : reader.GetString(0);
                spinPort.Value   = reader.IsDBNull(1) ? 587 : reader.GetInt32(1);
                txtUser.Text     = reader.IsDBNull(2) ? "" : reader.GetString(2);
                txtPassword.Text = reader.IsDBNull(3) ? "" : reader.GetString(3);
                chkSsl.Checked   = reader.IsDBNull(4) || reader.GetBoolean(4);

                string from = reader.IsDBNull(5) ? "" : reader.GetString(5);
                if (!string.IsNullOrWhiteSpace(from)) txtFrom.Text = from;

                radioMethod.EditValue = "smtp";
                ApplyMethod();

                ShowStatus(
                    $"נטענו ההגדרות מ-MonicipalYBD: {txtHost.Text}, משתמש {txtUser.Text}, " +
                    $"סיסמה באורך {txtPassword.Text.Length} תווים. " +
                    "יש לבדוק, ואז לשמור — הסיסמה תישמר כאן מוצפנת.",
                    isError: false);
            }
            catch (Exception ex)
            {
                ShowStatus($"הייבוא נכשל: {ex.Message}", isError: true);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}
