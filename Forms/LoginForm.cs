using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Microsoft.Data.SqlClient;
using MunicipalDiscount.Classes;

namespace MunicipalDiscount
{
    public class LoginForm : XtraForm
    {
        private TextEdit txtUsername;
        private TextEdit txtPassword;
        private SimpleButton btnLogin;
        private SimpleButton btnCancel;
        private LabelControl lblUsername;
        private LabelControl lblPassword;
        private LabelControl lblTitle;

        public int LoggedInUserId { get; private set; }
        public string LoggedInIdentityCard { get; private set; }
        public string LoggedInFullName { get; private set; }

        public LoginForm()
        {
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            this.Text = "כניסה למערכת";
            this.Size = new Size(380, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.IconOptions.SvgImage = MunicipalDiscount.Classes.AppIcons.Login;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;

            lblTitle = new LabelControl();
            lblTitle.Text = "ניהול הנחות - כניסה למערכת";
            lblTitle.Font = new Font("Tahoma", 14F, FontStyle.Bold);
            lblTitle.ForeColor = ThemeHelper.PrimaryColor;
            lblTitle.AutoSizeMode = LabelAutoSizeMode.None;
            lblTitle.Size = new Size(340, 30);
            lblTitle.Location = new Point(10, 15);
            lblTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            lblUsername = new LabelControl();
            lblUsername.Text = "שם משתמש:";
            lblUsername.Font = new Font("Tahoma", 10F);
            lblUsername.Location = new Point(290, 60);

            txtUsername = new TextEdit();
            txtUsername.Location = new Point(50, 58);
            txtUsername.Size = new Size(220, 24);
            txtUsername.Properties.Appearance.Font = new Font("Tahoma", 10F);

            lblPassword = new LabelControl();
            lblPassword.Text = "סיסמה:";
            lblPassword.Font = new Font("Tahoma", 10F);
            lblPassword.Location = new Point(290, 100);

            txtPassword = new TextEdit();
            txtPassword.Location = new Point(50, 98);
            txtPassword.Size = new Size(220, 24);
            txtPassword.Properties.Appearance.Font = new Font("Tahoma", 10F);
            txtPassword.Properties.PasswordChar = '*';

            btnLogin = new SimpleButton();
            btnLogin.Text = "כניסה";
            btnLogin.Location = new Point(170, 150);
            btnLogin.Size = new Size(100, 35);
            btnLogin.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            btnLogin.Click += BtnLogin_Click;

            btnCancel = new SimpleButton();
            btnCancel.Text = "ביטול";
            btnCancel.Location = new Point(50, 150);
            btnCancel.Size = new Size(100, 35);
            btnCancel.Appearance.Font = new Font("Tahoma", 10F);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { lblTitle, lblUsername, txtUsername, lblPassword, txtPassword, btnLogin, btnCancel });

            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                XtraMessageBox.Show("נא להזין שם משתמש", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                XtraMessageBox.Show("נא להזין סיסמה", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();

                    // tbUsers / tbUserPermissions and the default admin come from
                    // the installer's database scripts — the app creates nothing.
                    string query = @"SELECT ID, IdentityCard, FullName
                                     FROM tbUsers
                                     WHERE Username = @username AND Password = @password AND IsActive = 1";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = txtUsername.Text.Trim();
                    cmd.Parameters.Add("@password", SqlDbType.NVarChar, 100).Value = txtPassword.Text.Trim();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            LoggedInUserId = Convert.ToInt32(reader["ID"]);
                            LoggedInIdentityCard = reader["IdentityCard"]?.ToString() ?? "";
                            LoggedInFullName = reader["FullName"].ToString();

                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            XtraMessageBox.Show("שם משתמש או סיסמה שגויים", "שגיאה",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            txtPassword.Text = "";
                            txtPassword.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בחיבור למסד נתונים:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {

        }
    }
}
