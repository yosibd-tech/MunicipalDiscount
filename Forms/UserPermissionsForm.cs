using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Data.SqlClient;
using MunicipalDiscount.Classes;

namespace MunicipalDiscount
{
    public partial class UserPermissionsForm : XtraForm
    {
        private string connectionString = DatabaseConfig.ConnectionString;
        private int? selectedWorkerId = null;

        public UserPermissionsForm()
        {
            InitializeComponent();
        }

        private void UserPermissionsForm_Load(object sender, EventArgs e)
        {
            RestoreFormSize();
            ThemeHelper.ApplyTheme(this);
            this.IconOptions.SvgImage = MunicipalDiscount.Classes.AppIcons.UserPermissions;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;
            LoadPermissionsChecklist();
            LoadRoles();
            LoadWorkersGrid();
            ClearForm();
            ApplyAdminRestrictions();
        }

        #region Role templates

        /// <summary>ממלא את רשימת התפקידים מ-PermissionManager.Roles.</summary>
        private void LoadRoles()
        {
            cmbRole.Properties.Items.Clear();
            foreach (string role in PermissionManager.Roles.Keys)
                cmbRole.Properties.Items.Add(role);
        }

        /// <summary>
        /// מסמן את ההרשאות של התפקיד שנבחר — נקודת פתיחה, לא נעילה. אפשר לשנות
        /// כל תיבה אחר כך, ולכן חריג אחד אינו מחייב תפקיד חדש.
        /// </summary>
        private void btnApplyRole_Click(object sender, EventArgs e)
        {
            string role = cmbRole.Text?.Trim() ?? "";

            if (!PermissionManager.Roles.TryGetValue(role, out string[]? keys))
            {
                XtraMessageBox.Show("יש לבחור תפקיד מהרשימה.", "החלת תפקיד",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (chkIsAdmin.Checked)
            {
                XtraMessageBox.Show("למנהל מערכת כל ההרשאות ממילא — אין צורך בתפקיד.",
                    "החלת תפקיד", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // The role replaces the selection rather than adding to it: applying
            // "קופאית" to someone who was a manager has to take permissions away,
            // or a demotion would silently leave everything in place.
            var wanted = new HashSet<string>(keys);
            int index = 0;
            foreach (var permission in PermissionManager.AllPermissions)
            {
                chkListPermissions.SetItemChecked(index, wanted.Contains(permission.Key));
                index++;
            }

            XtraMessageBox.Show(
                $"סומנו {keys.Length} הרשאות לפי התפקיד: {role}\n" +
                "אפשר לשנות כל הרשאה לפני השמירה. השינוי נשמר בלחיצה על שמור.",
                "החלת תפקיד", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        /// <summary>
        /// רק מנהל מערכת רשאי להעניק הרשאת מנהל מערכת.
        ///
        /// UserManagement lets a manager maintain the staff list — that is its
        /// purpose. It must not let him tick "מנהל מערכת" for himself and acquire
        /// every screen plus the connection settings, which is what it did: the
        /// box was editable by anyone who could open this screen.
        ///
        /// Disabled rather than hidden, so it is still visible WHICH users are
        /// administrators — that is information a manager legitimately needs.
        /// </summary>
        private void ApplyAdminRestrictions()
        {
            if (PermissionManager.IsAdmin) return;

            chkIsAdmin.Enabled = false;
            chkIsAdmin.ToolTip = "רק מנהל מערכת יכול לשנות הרשאת מנהל מערכת.";
        }

        #region Workers Grid

        private void LoadWorkersGrid()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT ID, IdentityCard, FullName, Username, IsAdmin, IsActive
                                     FROM tbUsers ORDER BY FullName";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    gridUsers.DataSource = dt;

                    gridViewUsers.Columns["ID"].Visible = false;
                    gridViewUsers.Columns["IdentityCard"].Caption = "ת.ז.";
                    gridViewUsers.Columns["FullName"].Caption = "שם מלא";
                    gridViewUsers.Columns["Username"].Caption = "שם משתמש";
                    gridViewUsers.Columns["IsAdmin"].Caption = "מנהל";
                    gridViewUsers.Columns["IsActive"].Caption = "פעיל";
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בטעינת עובדים:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridViewUsers_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            LoadSelectedWorker();
        }

        private void gridUsers_Click(object sender, EventArgs e)
        {
            LoadSelectedWorker();
        }

        private void LoadSelectedWorker()
        {
            if (gridViewUsers.FocusedRowHandle < 0) return;

            selectedWorkerId = Convert.ToInt32(gridViewUsers.GetRowCellValue(gridViewUsers.FocusedRowHandle, "ID"));
            txtUsername.Text = gridViewUsers.GetRowCellValue(gridViewUsers.FocusedRowHandle, "Username")?.ToString() ?? "";
            txtFullName.Text = gridViewUsers.GetRowCellValue(gridViewUsers.FocusedRowHandle, "FullName").ToString();
            txtIdentityCard.Text = gridViewUsers.GetRowCellValue(gridViewUsers.FocusedRowHandle, "IdentityCard")?.ToString() ?? "";
            chkIsAdmin.Checked = Convert.ToBoolean(gridViewUsers.GetRowCellValue(gridViewUsers.FocusedRowHandle, "IsAdmin"));
            txtPassword.Text = "";

            // Load permissions for selected worker
            LoadUserPermissions(selectedWorkerId.Value);

            // Disable permission checkboxes for admin users
            chkListPermissions.Enabled = !chkIsAdmin.Checked;
        }

        #endregion

        #region Permissions Checklist

        private void LoadPermissionsChecklist()
        {
            chkListPermissions.Items.Clear();
            foreach (var perm in PermissionManager.AllPermissions)
            {
                chkListPermissions.Items.Add(perm.Value, false);
            }
        }

        private void LoadUserPermissions(int workerId)
        {
            // Uncheck all
            for (int i = 0; i < chkListPermissions.Items.Count; i++)
            {
                chkListPermissions.SetItemChecked(i, false);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(
                        "SELECT PermissionKey FROM tbUserPermissions WHERE UserId = @workerId", connection);
                    cmd.Parameters.Add("@workerId", SqlDbType.Int).Value = workerId;

                    HashSet<string> userPerms = new HashSet<string>();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userPerms.Add(reader["PermissionKey"].ToString());
                        }
                    }

                    // Check matching items
                    int index = 0;
                    foreach (var perm in PermissionManager.AllPermissions)
                    {
                        if (userPerms.Contains(perm.Key))
                        {
                            chkListPermissions.SetItemChecked(index, true);
                        }
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בטעינת הרשאות:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Save / Delete

        private void ClearForm()
        {
            selectedWorkerId = null;
            txtUsername.Text = "";
            txtFullName.Text = "";
            txtIdentityCard.Text = "";
            txtPassword.Text = "";
            chkIsAdmin.Checked = false;
            chkListPermissions.Enabled = true;

            for (int i = 0; i < chkListPermissions.Items.Count; i++)
            {
                chkListPermissions.SetItemChecked(i, false);
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // הגנה שנייה, מעבר להשבתת התיבה: תיבה מושבתת היא מחסום בממשק בלבד,
            // והרשאת מנהל מערכת שווה כאן גישה לכל המסכים ולהגדרות החיבור.
            if (!PermissionManager.IsAdmin && chkIsAdmin.Checked)
            {
                XtraMessageBox.Show("רק מנהל מערכת יכול להעניק הרשאת מנהל מערכת.",
                    "הרשאה נדרשת", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                XtraMessageBox.Show("נא להזין שם מלא", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                XtraMessageBox.Show("נא להזין שם משתמש", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (selectedWorkerId == null && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                XtraMessageBox.Show("נא להזין סיסמה למשתמש חדש", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    int userId;

                    if (selectedWorkerId == null)
                    {
                        // Insert new user
                        string insertQuery = @"INSERT INTO tbUsers (Username, Password, FullName, IdentityCard, IsAdmin, IsActive)
                                               VALUES (@username, @password, @fullName, @identityCard, @isAdmin, 1);
                                               SELECT SCOPE_IDENTITY();";
                        SqlCommand insertCmd = new SqlCommand(insertQuery, connection);
                        insertCmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = txtUsername.Text.Trim();
                        insertCmd.Parameters.Add("@password", SqlDbType.NVarChar, 100).Value = txtPassword.Text.Trim();
                        insertCmd.Parameters.Add("@fullName", SqlDbType.NVarChar, 100).Value = txtFullName.Text.Trim();
                        insertCmd.Parameters.Add("@identityCard", SqlDbType.NVarChar, 20).Value =
                            string.IsNullOrWhiteSpace(txtIdentityCard.Text) ? (object)DBNull.Value : txtIdentityCard.Text.Trim();
                        insertCmd.Parameters.Add("@isAdmin", SqlDbType.Bit).Value = chkIsAdmin.Checked;
                        userId = Convert.ToInt32(insertCmd.ExecuteScalar());
                    }
                    else
                    {
                        userId = selectedWorkerId.Value;
                        string query;
                        SqlCommand cmd;

                        if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                        {
                            query = @"UPDATE tbUsers SET Username = @username, Password = @password,
                                      FullName = @fullName, IdentityCard = @identityCard, IsAdmin = @isAdmin
                                      WHERE ID = @id";
                            cmd = new SqlCommand(query, connection);
                            cmd.Parameters.Add("@password", SqlDbType.NVarChar, 100).Value = txtPassword.Text.Trim();
                        }
                        else
                        {
                            query = @"UPDATE tbUsers SET Username = @username,
                                      FullName = @fullName, IdentityCard = @identityCard, IsAdmin = @isAdmin
                                      WHERE ID = @id";
                            cmd = new SqlCommand(query, connection);
                        }

                        cmd.Parameters.Add("@username", SqlDbType.NVarChar, 50).Value = txtUsername.Text.Trim();
                        cmd.Parameters.Add("@fullName", SqlDbType.NVarChar, 100).Value = txtFullName.Text.Trim();
                        cmd.Parameters.Add("@identityCard", SqlDbType.NVarChar, 20).Value =
                            string.IsNullOrWhiteSpace(txtIdentityCard.Text) ? (object)DBNull.Value : txtIdentityCard.Text.Trim();
                        cmd.Parameters.Add("@isAdmin", SqlDbType.Bit).Value = chkIsAdmin.Checked;
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                        cmd.ExecuteNonQuery();
                    }

                    // Save permissions
                    SavePermissions(connection, userId);
                }

                LoadWorkersGrid();
                XtraMessageBox.Show("ההרשאות נשמרו בהצלחה", "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                XtraMessageBox.Show("שם משתמש כבר קיים במערכת", "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
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

        private void SavePermissions(SqlConnection connection, int workerId)
        {
            // Delete this app's existing permissions only. tbUserPermissions is
            // shared with the MonicipalYBD app (one system, one manager): deleting
            // every row of the user used to wipe his MonicipalYBD permissions too.
            var keys = new List<string>(PermissionManager.AllPermissions.Keys);
            var inList = new List<string>();
            SqlCommand deleteCmd = new SqlCommand { Connection = connection };
            deleteCmd.Parameters.Add("@workerId", SqlDbType.Int).Value = workerId;
            for (int i = 0; i < keys.Count; i++)
            {
                inList.Add("@k" + i);
                deleteCmd.Parameters.Add("@k" + i, SqlDbType.NVarChar, 50).Value = keys[i];
            }
            deleteCmd.CommandText =
                "DELETE FROM tbUserPermissions WHERE UserId = @workerId AND PermissionKey IN (" +
                string.Join(", ", inList) + ")";
            deleteCmd.ExecuteNonQuery();

            // Insert checked permissions
            int index = 0;
            foreach (var perm in PermissionManager.AllPermissions)
            {
                if (chkListPermissions.GetItemChecked(index))
                {
                    SqlCommand insertCmd = new SqlCommand(
                        "INSERT INTO tbUserPermissions (UserId, PermissionKey) VALUES (@workerId, @key)", connection);
                    insertCmd.Parameters.Add("@workerId", SqlDbType.Int).Value = workerId;
                    insertCmd.Parameters.Add("@key", SqlDbType.NVarChar, 50).Value = perm.Key;
                    insertCmd.ExecuteNonQuery();
                }
                index++;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedWorkerId == null)
            {
                XtraMessageBox.Show("אנא בחר עובד", "הודעה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Prevent removing your own access
            if (selectedWorkerId.Value == SessionData.CurrentUserId)
            {
                XtraMessageBox.Show("לא ניתן להסיר הרשאות מהמשתמש הנוכחי", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = XtraMessageBox.Show(
                $"האם להסיר הרשאות כניסה מהעובד '{txtFullName.Text}'?",
                "אישור", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Remove permissions
                    SqlCommand delPerms = new SqlCommand("DELETE FROM tbUserPermissions WHERE UserId = @id", connection);
                    delPerms.Parameters.Add("@id", SqlDbType.Int).Value = selectedWorkerId.Value;
                    delPerms.ExecuteNonQuery();

                    // Clear login credentials
                    SqlCommand clearLogin = new SqlCommand(
                        "UPDATE tbUsers SET Username = NULL, Password = NULL, IsAdmin = 0 WHERE ID = @id", connection);
                    clearLogin.Parameters.Add("@id", SqlDbType.Int).Value = selectedWorkerId.Value;
                    clearLogin.ExecuteNonQuery();
                }

                LoadWorkersGrid();
                ClearForm();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void chkIsAdmin_CheckedChanged(object sender, EventArgs e)
        {
            chkListPermissions.Enabled = !chkIsAdmin.Checked;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion
        /// <summary>
        /// גודל החלון שהמשתמש בחר — אותו דפוס כמו ManParametersForm. רשימת
        /// ההרשאות ארוכה, ומי שמרחיב את החלון פעם אחת אינו אמור לחזור על כך.
        /// </summary>
        private static readonly string LayoutFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MunicipalDiscount", "UserPermissionsForm.xml");

        private void RestoreFormSize()
        {
            try
            {
                if (File.Exists(LayoutFilePath))
                {
                    string[] parts = File.ReadAllText(LayoutFilePath).Split(',');
                    if (parts.Length == 2
                        && int.TryParse(parts[0], out int w)
                        && int.TryParse(parts[1], out int h))
                    {
                        this.ClientSize = new Size(w, h);
                    }
                }
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // ממצב רגיל בלבד: חלון מוגדר היה נשמר כגודל המסך ונפתח כך
                // גם על מסך קטן יותר.
                if (this.WindowState == FormWindowState.Normal)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(LayoutFilePath)!);
                    File.WriteAllText(LayoutFilePath, $"{ClientSize.Width},{ClientSize.Height}");
                }
            }
            catch { }
            base.OnFormClosing(e);
        }

    }
}
