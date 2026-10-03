using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace MunicipalDiscount
{
    partial class UserPermissionsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserPermissionsForm));
            grpUserDetails = new GroupControl();
            chkIsAdmin = new CheckEdit();
            txtPassword = new TextEdit();
            txtIdentityCard = new TextEdit();
            txtFullName = new TextEdit();
            txtUsername = new TextEdit();
            lblPassword = new LabelControl();
            lblIdentityCard = new LabelControl();
            lblFullName = new LabelControl();
            lblUsername = new LabelControl();
            grpPermissions = new GroupControl();
            panelRoles = new DevExpress.XtraEditors.PanelControl();
            lblRoles = new LabelControl();
            cmbRole = new DevExpress.XtraEditors.ComboBoxEdit();
            btnApplyRole = new SimpleButton();
            chkListPermissions = new CheckedListBox();
            grpUsersList = new GroupControl();
            gridUsers = new GridControl();
            gridViewUsers = new GridView();
            splitMain = new SplitContainer();
            panelButtons = new Panel();
            btnNew = new SimpleButton();
            btnSave = new SimpleButton();
            btnDelete = new SimpleButton();
            btnClose = new SimpleButton();
            ((System.ComponentModel.ISupportInitialize)grpUserDetails).BeginInit();
            grpUserDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chkIsAdmin.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPassword.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtIdentityCard.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtFullName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtUsername.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grpPermissions).BeginInit();
            grpPermissions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelRoles).BeginInit();
            panelRoles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbRole.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grpUsersList).BeginInit();
            grpUsersList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridUsers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridViewUsers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            panelButtons.SuspendLayout();
            SuspendLayout();
            //
            // grpUserDetails  (top of the right split panel)
            //
            grpUserDetails.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            grpUserDetails.AppearanceCaption.ForeColor = Color.FromArgb(42, 82, 152);
            grpUserDetails.AppearanceCaption.Options.UseFont = true;
            grpUserDetails.AppearanceCaption.Options.UseForeColor = true;
            grpUserDetails.AppearanceCaption.Options.UseTextOptions = true;
            grpUserDetails.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            grpUserDetails.Controls.Add(chkIsAdmin);
            grpUserDetails.Controls.Add(txtPassword);
            grpUserDetails.Controls.Add(txtIdentityCard);
            grpUserDetails.Controls.Add(txtFullName);
            grpUserDetails.Controls.Add(txtUsername);
            grpUserDetails.Controls.Add(lblPassword);
            grpUserDetails.Controls.Add(lblIdentityCard);
            grpUserDetails.Controls.Add(lblFullName);
            grpUserDetails.Controls.Add(lblUsername);
            grpUserDetails.Dock = DockStyle.Top;
            grpUserDetails.Location = new Point(0, 0);
            grpUserDetails.Name = "grpUserDetails";
            grpUserDetails.RightToLeft = RightToLeft.Yes;
            grpUserDetails.Size = new Size(600, 220);
            grpUserDetails.TabIndex = 0;
            grpUserDetails.Text = "הגדרות כניסה";
            //
            // chkIsAdmin
            //
            chkIsAdmin.Location = new Point(220, 172);
            chkIsAdmin.Name = "chkIsAdmin";
            chkIsAdmin.Properties.Caption = "מנהל מערכת";
            chkIsAdmin.Size = new Size(140, 24);
            chkIsAdmin.TabIndex = 6;
            chkIsAdmin.CheckedChanged += chkIsAdmin_CheckedChanged;
            //
            // txtPassword
            //
            txtPassword.Location = new Point(80, 137);
            txtPassword.Name = "txtPassword";
            txtPassword.Properties.PasswordChar = '*';
            txtPassword.Size = new Size(200, 22);
            txtPassword.TabIndex = 5;
            //
            // txtIdentityCard
            //
            txtIdentityCard.Location = new Point(80, 69);
            txtIdentityCard.Name = "txtIdentityCard";
            txtIdentityCard.Size = new Size(200, 22);
            txtIdentityCard.TabIndex = 2;
            //
            // txtFullName
            //
            txtFullName.Location = new Point(80, 35);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(200, 22);
            txtFullName.TabIndex = 1;
            //
            // txtUsername
            //
            txtUsername.Location = new Point(80, 103);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(200, 22);
            txtUsername.TabIndex = 3;
            //
            // lblPassword
            //
            lblPassword.Location = new Point(300, 140);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(47, 16);
            lblPassword.TabIndex = 7;
            lblPassword.Text = "סיסמה:";
            //
            // lblIdentityCard
            //
            lblIdentityCard.Location = new Point(300, 72);
            lblIdentityCard.Name = "lblIdentityCard";
            lblIdentityCard.Size = new Size(28, 16);
            lblIdentityCard.TabIndex = 8;
            lblIdentityCard.Text = "ת.ז.:";
            //
            // lblFullName
            //
            lblFullName.Location = new Point(300, 38);
            lblFullName.Name = "lblFullName";
            lblFullName.Size = new Size(55, 16);
            lblFullName.TabIndex = 9;
            lblFullName.Text = "שם מלא:";
            //
            // lblUsername
            //
            lblUsername.Location = new Point(300, 106);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(79, 16);
            lblUsername.TabIndex = 10;
            lblUsername.Text = "שם משתמש:";
            //
            // grpPermissions  (fills the rest of the right split panel)
            //
            grpPermissions.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            grpPermissions.AppearanceCaption.ForeColor = Color.FromArgb(42, 82, 152);
            grpPermissions.AppearanceCaption.Options.UseFont = true;
            grpPermissions.AppearanceCaption.Options.UseForeColor = true;
            grpPermissions.AppearanceCaption.Options.UseTextOptions = true;
            grpPermissions.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            grpPermissions.Controls.Add(chkListPermissions);
            grpPermissions.Controls.Add(panelRoles);
            grpPermissions.Dock = DockStyle.Fill;
            grpPermissions.Location = new Point(0, 220);
            grpPermissions.Name = "grpPermissions";
            grpPermissions.RightToLeft = RightToLeft.Yes;
            grpPermissions.Size = new Size(600, 288);
            grpPermissions.TabIndex = 1;
            grpPermissions.Text = "הרשאות";
            //
            // chkListPermissions
            //
            chkListPermissions.CheckOnClick = true;
            chkListPermissions.Dock = DockStyle.Fill;
            chkListPermissions.Font = new Font("Tahoma", 10F);
            chkListPermissions.FormattingEnabled = true;
            chkListPermissions.Location = new Point(2, 28);
            chkListPermissions.Name = "chkListPermissions";
            chkListPermissions.RightToLeft = RightToLeft.Yes;
            chkListPermissions.Size = new Size(596, 258);
            chkListPermissions.TabIndex = 0;
            //
            // panelRoles
            //
            panelRoles.Controls.Add(btnApplyRole);
            panelRoles.Controls.Add(cmbRole);
            panelRoles.Controls.Add(lblRoles);
            panelRoles.Dock = DockStyle.Top;
            panelRoles.Location = new Point(2, 28);
            panelRoles.Name = "panelRoles";
            panelRoles.RightToLeft = RightToLeft.Yes;
            panelRoles.Size = new Size(596, 42);
            panelRoles.TabIndex = 1;
            //
            // lblRoles
            //
            lblRoles.Anchor = AnchorStyles.Top;
            lblRoles.Location = new Point(520, 14);
            lblRoles.Name = "lblRoles";
            lblRoles.Size = new Size(40, 16);
            lblRoles.TabIndex = 0;
            lblRoles.Text = "תפקיד:";
            //
            // cmbRole
            //
            cmbRole.Anchor = AnchorStyles.Top;
            cmbRole.Location = new Point(300, 10);
            cmbRole.Name = "cmbRole";
            cmbRole.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbRole.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbRole.Size = new Size(210, 24);
            cmbRole.TabIndex = 1;
            //
            // btnApplyRole
            //
            btnApplyRole.Anchor = AnchorStyles.Top;
            btnApplyRole.Location = new Point(150, 8);
            btnApplyRole.Name = "btnApplyRole";
            btnApplyRole.Size = new Size(140, 28);
            btnApplyRole.TabIndex = 2;
            btnApplyRole.Text = "החל תפקיד";
            btnApplyRole.Click += btnApplyRole_Click;
            //
            // grpUsersList  (fills the left split panel)
            //
            grpUsersList.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            grpUsersList.AppearanceCaption.ForeColor = Color.FromArgb(42, 82, 152);
            grpUsersList.AppearanceCaption.Options.UseFont = true;
            grpUsersList.AppearanceCaption.Options.UseForeColor = true;
            grpUsersList.AppearanceCaption.Options.UseTextOptions = true;
            grpUsersList.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            grpUsersList.Controls.Add(gridUsers);
            grpUsersList.Dock = DockStyle.Fill;
            grpUsersList.Location = new Point(0, 0);
            grpUsersList.Name = "grpUsersList";
            grpUsersList.RightToLeft = RightToLeft.Yes;
            grpUsersList.Size = new Size(380, 508);
            grpUsersList.TabIndex = 0;
            grpUsersList.Text = "רשימת עובדים";
            //
            // gridUsers
            //
            gridUsers.Dock = DockStyle.Fill;
            gridUsers.Location = new Point(2, 28);
            gridUsers.MainView = gridViewUsers;
            gridUsers.Name = "gridUsers";
            gridUsers.Size = new Size(376, 478);
            gridUsers.TabIndex = 0;
            gridUsers.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridViewUsers });
            gridUsers.Click += gridUsers_Click;
            //
            // gridViewUsers
            //
            gridViewUsers.GridControl = gridUsers;
            gridViewUsers.Name = "gridViewUsers";
            gridViewUsers.OptionsBehavior.Editable = false;
            gridViewUsers.OptionsView.ShowGroupPanel = false;
            gridViewUsers.FocusedRowChanged += gridViewUsers_FocusedRowChanged;
            //
            // splitMain  (two resizable columns: grid | details+permissions)
            //
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.None;
            splitMain.Location = new Point(0, 0);
            splitMain.Name = "splitMain";
            splitMain.Orientation = Orientation.Vertical;
            splitMain.Panel1.Controls.Add(grpUsersList);
            splitMain.Panel1MinSize = 260;
            splitMain.Panel2.Controls.Add(grpPermissions);
            splitMain.Panel2.Controls.Add(grpUserDetails);
            splitMain.Panel2MinSize = 320;
            splitMain.RightToLeft = RightToLeft.Yes;
            splitMain.Size = new Size(1008, 508);
            splitMain.SplitterDistance = 380;
            splitMain.SplitterWidth = 6;
            splitMain.TabIndex = 0;
            //
            // panelButtons  (bottom strip)
            //
            panelButtons.Controls.Add(btnNew);
            panelButtons.Controls.Add(btnSave);
            panelButtons.Controls.Add(btnDelete);
            panelButtons.Controls.Add(btnClose);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 508);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(1008, 52);
            panelButtons.TabIndex = 1;
            //
            // btnNew
            //
            btnNew.Location = new Point(12, 9);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(93, 35);
            btnNew.TabIndex = 0;
            btnNew.Text = "נקה";
            btnNew.Click += btnNew_Click;
            //
            // btnSave
            //
            btnSave.Location = new Point(111, 9);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(93, 35);
            btnSave.TabIndex = 1;
            btnSave.Text = "שמור";
            btnSave.Click += btnSave_Click;
            //
            // btnDelete
            //
            btnDelete.Location = new Point(210, 9);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(93, 35);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "הסר הרשאות";
            btnDelete.Click += btnDelete_Click;
            //
            // btnClose
            //
            btnClose.Location = new Point(309, 9);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(89, 35);
            btnClose.TabIndex = 3;
            btnClose.Text = "סגור";
            btnClose.Click += btnClose_Click;
            //
            // UserPermissionsForm
            //
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1008, 560);
            Controls.Add(splitMain);
            Controls.Add(panelButtons);
            FormBorderStyle = FormBorderStyle.Sizable;
            IconOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("UserPermissionsForm.IconOptions.SvgImage");
            MaximizeBox = true;
            MinimizeBox = false;
            MinimumSize = new Size(760, 480);
            Name = "UserPermissionsForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            Text = "ניהול משתמשים והרשאות";
            Load += UserPermissionsForm_Load;
            ((System.ComponentModel.ISupportInitialize)grpUserDetails).EndInit();
            grpUserDetails.ResumeLayout(false);
            grpUserDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chkIsAdmin.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPassword.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtIdentityCard.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtFullName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtUsername.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)grpPermissions).EndInit();
            grpPermissions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)cmbRole.Properties).EndInit();
            panelRoles.ResumeLayout(false);
            panelRoles.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)panelRoles).EndInit();
            ((System.ComponentModel.ISupportInitialize)grpUsersList).EndInit();
            grpUsersList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridUsers).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridViewUsers).EndInit();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.GroupControl grpUserDetails;
        private DevExpress.XtraEditors.LabelControl lblUsername;
        private DevExpress.XtraEditors.TextEdit txtUsername;
        private DevExpress.XtraEditors.LabelControl lblFullName;
        private DevExpress.XtraEditors.TextEdit txtFullName;
        private DevExpress.XtraEditors.LabelControl lblIdentityCard;
        private DevExpress.XtraEditors.TextEdit txtIdentityCard;
        private DevExpress.XtraEditors.LabelControl lblPassword;
        private DevExpress.XtraEditors.TextEdit txtPassword;
        private DevExpress.XtraEditors.CheckEdit chkIsAdmin;
        private DevExpress.XtraEditors.GroupControl grpPermissions;
        private DevExpress.XtraEditors.PanelControl panelRoles;
        private DevExpress.XtraEditors.LabelControl lblRoles;
        private DevExpress.XtraEditors.ComboBoxEdit cmbRole;
        private DevExpress.XtraEditors.SimpleButton btnApplyRole;
        private System.Windows.Forms.CheckedListBox chkListPermissions;
        private DevExpress.XtraEditors.GroupControl grpUsersList;
        private DevExpress.XtraGrid.GridControl gridUsers;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewUsers;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Panel panelButtons;
        private DevExpress.XtraEditors.SimpleButton btnNew;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnClose;
    }
}
