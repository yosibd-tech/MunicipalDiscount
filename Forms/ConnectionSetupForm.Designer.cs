using DevExpress.XtraEditors;
using System.Drawing;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    partial class ConnectionSetupForm
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
            grpConnection = new GroupControl();
            txtServer = new TextEdit();
            lblServer = new LabelControl();
            txtDatabase = new TextEdit();
            lblDatabase = new LabelControl();
            txtUser = new TextEdit();
            lblUser = new LabelControl();
            txtPassword = new TextEdit();
            lblPassword = new LabelControl();
            txtOptions = new TextEdit();
            lblOptions = new LabelControl();
            lblPasswordNote = new LabelControl();
            lblFile = new LabelControl();
            lblStatus = new LabelControl();
            panelButtons = new PanelControl();
            btnTest = new SimpleButton();
            btnSave = new SimpleButton();
            btnCancel = new SimpleButton();
            ((System.ComponentModel.ISupportInitialize)grpConnection).BeginInit();
            grpConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtServer.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtDatabase.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtUser.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPassword.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtOptions.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).BeginInit();
            panelButtons.SuspendLayout();
            SuspendLayout();
            //
            // grpConnection
            //
            grpConnection.Controls.Add(lblPasswordNote);
            grpConnection.Controls.Add(txtOptions);
            grpConnection.Controls.Add(lblOptions);
            grpConnection.Controls.Add(txtPassword);
            grpConnection.Controls.Add(lblPassword);
            grpConnection.Controls.Add(txtUser);
            grpConnection.Controls.Add(lblUser);
            grpConnection.Controls.Add(txtDatabase);
            grpConnection.Controls.Add(lblDatabase);
            grpConnection.Controls.Add(txtServer);
            grpConnection.Controls.Add(lblServer);
            grpConnection.Dock = DockStyle.Top;
            grpConnection.Location = new Point(0, 0);
            grpConnection.Name = "grpConnection";
            grpConnection.RightToLeft = RightToLeft.Yes;
            grpConnection.Size = new Size(620, 235);
            grpConnection.TabIndex = 0;
            grpConnection.Text = "חיבור למסד הנתונים";
            //
            // lblServer
            //
            lblServer.Anchor = AnchorStyles.Top;
            lblServer.Location = new Point(500, 40);
            lblServer.Name = "lblServer";
            lblServer.Size = new Size(30, 16);
            lblServer.TabIndex = 0;
            lblServer.Text = "שרת:";
            //
            // txtServer
            //
            txtServer.Anchor = AnchorStyles.Top;
            txtServer.Location = new Point(160, 37);
            txtServer.Name = "txtServer";
            txtServer.Size = new Size(330, 24);
            txtServer.TabIndex = 1;
            //
            // lblDatabase
            //
            lblDatabase.Anchor = AnchorStyles.Top;
            lblDatabase.Location = new Point(500, 72);
            lblDatabase.Name = "lblDatabase";
            lblDatabase.Size = new Size(60, 16);
            lblDatabase.TabIndex = 2;
            lblDatabase.Text = "מסד נתונים:";
            //
            // txtDatabase
            //
            txtDatabase.Anchor = AnchorStyles.Top;
            txtDatabase.Location = new Point(160, 69);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.Size = new Size(330, 24);
            txtDatabase.TabIndex = 3;
            //
            // lblUser
            //
            lblUser.Anchor = AnchorStyles.Top;
            lblUser.Location = new Point(500, 104);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(42, 16);
            lblUser.TabIndex = 4;
            lblUser.Text = "משתמש:";
            //
            // txtUser
            //
            txtUser.Anchor = AnchorStyles.Top;
            txtUser.Location = new Point(160, 101);
            txtUser.Name = "txtUser";
            txtUser.Size = new Size(330, 24);
            txtUser.TabIndex = 5;
            //
            // lblPassword
            //
            lblPassword.Anchor = AnchorStyles.Top;
            lblPassword.Location = new Point(500, 136);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(35, 16);
            lblPassword.TabIndex = 6;
            lblPassword.Text = "סיסמה:";
            //
            // txtPassword
            //
            txtPassword.Anchor = AnchorStyles.Top;
            txtPassword.Location = new Point(160, 133);
            txtPassword.Name = "txtPassword";
            txtPassword.Properties.PasswordChar = '●';
            txtPassword.Properties.UseSystemPasswordChar = true;
            txtPassword.Size = new Size(330, 24);
            txtPassword.TabIndex = 7;
            //
            // lblPasswordNote
            //
            lblPasswordNote.Anchor = AnchorStyles.Top;
            lblPasswordNote.Appearance.ForeColor = Color.FromArgb(110, 110, 110);
            lblPasswordNote.Location = new Point(160, 160);
            lblPasswordNote.Name = "lblPasswordNote";
            lblPasswordNote.Size = new Size(330, 14);
            lblPasswordNote.TabIndex = 8;
            lblPasswordNote.Text = "יש להשאיר ריק כדי לשמור על הסיסמה הקיימת.";
            //
            // lblOptions
            //
            lblOptions.Anchor = AnchorStyles.Top;
            lblOptions.Location = new Point(500, 190);
            lblOptions.Name = "lblOptions";
            lblOptions.Size = new Size(56, 16);
            lblOptions.TabIndex = 9;
            lblOptions.Text = "אפשרויות:";
            //
            // txtOptions
            //
            txtOptions.Anchor = AnchorStyles.Top;
            txtOptions.Location = new Point(160, 187);
            txtOptions.Name = "txtOptions";
            txtOptions.Size = new Size(330, 24);
            txtOptions.TabIndex = 10;
            //
            // lblFile
            //
            lblFile.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblFile.Appearance.ForeColor = Color.FromArgb(110, 110, 110);
            lblFile.Location = new Point(14, 245);
            lblFile.Name = "lblFile";
            lblFile.Size = new Size(0, 14);
            lblFile.TabIndex = 1;
            //
            // lblStatus
            //
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblStatus.Appearance.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            lblStatus.Appearance.Options.UseFont = true;
            lblStatus.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            lblStatus.Location = new Point(14, 270);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(590, 16);
            lblStatus.TabIndex = 2;
            //
            // panelButtons
            //
            panelButtons.Controls.Add(btnCancel);
            panelButtons.Controls.Add(btnSave);
            panelButtons.Controls.Add(btnTest);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 331);
            panelButtons.Name = "panelButtons";
            panelButtons.RightToLeft = RightToLeft.Yes;
            panelButtons.Size = new Size(620, 50);
            panelButtons.TabIndex = 3;
            //
            // btnTest
            //
            btnTest.Anchor = AnchorStyles.Top;
            btnTest.Location = new Point(14, 11);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(110, 28);
            btnTest.TabIndex = 0;
            btnTest.Text = "בדוק חיבור";
            btnTest.Click += btnTest_Click;
            //
            // btnSave
            //
            btnSave.Anchor = AnchorStyles.Top;
            btnSave.Location = new Point(134, 11);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(110, 28);
            btnSave.TabIndex = 1;
            btnSave.Text = "שמור";
            btnSave.Click += btnSave_Click;
            //
            // btnCancel
            //
            btnCancel.Anchor = AnchorStyles.Top;
            btnCancel.Location = new Point(254, 11);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(110, 28);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "ביטול";
            btnCancel.Click += btnCancel_Click;
            //
            // ConnectionSetupForm
            //
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(620, 381);
            Controls.Add(lblStatus);
            Controls.Add(lblFile);
            Controls.Add(panelButtons);
            Controls.Add(grpConnection);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConnectionSetupForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "הגדרות חיבור למסד הנתונים";
            Load += ConnectionSetupForm_Load;
            ((System.ComponentModel.ISupportInitialize)grpConnection).EndInit();
            grpConnection.ResumeLayout(false);
            grpConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtServer.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtDatabase.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtUser.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPassword.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtOptions.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).EndInit();
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DevExpress.XtraEditors.GroupControl grpConnection;
        private DevExpress.XtraEditors.TextEdit txtServer;
        private DevExpress.XtraEditors.LabelControl lblServer;
        private DevExpress.XtraEditors.TextEdit txtDatabase;
        private DevExpress.XtraEditors.LabelControl lblDatabase;
        private DevExpress.XtraEditors.TextEdit txtUser;
        private DevExpress.XtraEditors.LabelControl lblUser;
        private DevExpress.XtraEditors.TextEdit txtPassword;
        private DevExpress.XtraEditors.LabelControl lblPassword;
        private DevExpress.XtraEditors.LabelControl lblPasswordNote;
        private DevExpress.XtraEditors.TextEdit txtOptions;
        private DevExpress.XtraEditors.LabelControl lblOptions;
        private DevExpress.XtraEditors.LabelControl lblFile;
        private DevExpress.XtraEditors.LabelControl lblStatus;
        private DevExpress.XtraEditors.PanelControl panelButtons;
        private DevExpress.XtraEditors.SimpleButton btnTest;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
    }
}
