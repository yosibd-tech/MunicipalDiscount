using DevExpress.XtraEditors;
using System.Drawing;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    partial class MailSetupForm
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
            grpMethod = new GroupControl();
            radioMethod = new RadioGroup();
            grpServer = new GroupControl();
            lblHost = new LabelControl();
            txtHost = new TextEdit();
            lblPort = new LabelControl();
            spinPort = new SpinEdit();
            chkSsl = new CheckEdit();
            lblFrom = new LabelControl();
            txtFrom = new TextEdit();
            lblUser = new LabelControl();
            txtUser = new TextEdit();
            lblUserNote = new LabelControl();
            lblPassword = new LabelControl();
            txtPassword = new ButtonEdit();
            lblPasswordNote = new LabelControl();
            grpTest = new GroupControl();
            lblTestTo = new LabelControl();
            txtTestTo = new TextEdit();
            memStatus = new MemoEdit();
            panelButtons = new PanelControl();
            btnTest = new SimpleButton();
            btnSave = new SimpleButton();
            btnClose = new SimpleButton();
            btnImport = new SimpleButton();
            ((System.ComponentModel.ISupportInitialize)grpMethod).BeginInit();
            grpMethod.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)radioMethod.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grpServer).BeginInit();
            grpServer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtHost.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinPort.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkSsl.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtFrom.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtUser.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtPassword.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grpTest).BeginInit();
            grpTest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtTestTo.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)memStatus.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).BeginInit();
            panelButtons.SuspendLayout();
            SuspendLayout();
            //
            // grpMethod
            //
            grpMethod.Controls.Add(radioMethod);
            grpMethod.Dock = DockStyle.Top;
            grpMethod.Location = new Point(0, 0);
            grpMethod.Name = "grpMethod";
            grpMethod.RightToLeft = RightToLeft.Yes;
            grpMethod.Size = new Size(620, 76);
            grpMethod.TabIndex = 0;
            grpMethod.Text = "דרך השליחה";
            //
            // radioMethod
            //
            radioMethod.Anchor = AnchorStyles.Top;
            radioMethod.EditValue = "smtp";
            radioMethod.Location = new Point(14, 28);
            radioMethod.Name = "radioMethod";
            radioMethod.Properties.Appearance.BackColor = Color.Transparent;
            radioMethod.Properties.Appearance.Options.UseBackColor = true;
            radioMethod.Properties.Columns = 2;
            radioMethod.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
                new DevExpress.XtraEditors.Controls.RadioGroupItem("smtp", "שרת SMTP"),
                new DevExpress.XtraEditors.Controls.RadioGroupItem("outlook", "Outlook המותקן במחשב") });
            radioMethod.RightToLeft = RightToLeft.Yes;
            radioMethod.Size = new Size(590, 36);
            radioMethod.TabIndex = 0;
            radioMethod.SelectedIndexChanged += radioMethod_SelectedIndexChanged;
            //
            // grpServer
            //
            grpServer.Controls.Add(lblPasswordNote);
            grpServer.Controls.Add(txtPassword);
            grpServer.Controls.Add(lblPassword);
            grpServer.Controls.Add(lblUserNote);
            grpServer.Controls.Add(txtUser);
            grpServer.Controls.Add(lblUser);
            grpServer.Controls.Add(txtFrom);
            grpServer.Controls.Add(lblFrom);
            grpServer.Controls.Add(chkSsl);
            grpServer.Controls.Add(spinPort);
            grpServer.Controls.Add(lblPort);
            grpServer.Controls.Add(txtHost);
            grpServer.Controls.Add(lblHost);
            grpServer.Dock = DockStyle.Top;
            grpServer.Location = new Point(0, 76);
            grpServer.Name = "grpServer";
            grpServer.RightToLeft = RightToLeft.Yes;
            grpServer.Size = new Size(620, 272);
            grpServer.TabIndex = 1;
            grpServer.Text = "שרת הדואר היוצא";
            //
            // lblHost
            //
            lblHost.Anchor = AnchorStyles.Top;
            lblHost.Location = new Point(490, 40);
            lblHost.Name = "lblHost";
            lblHost.Size = new Size(80, 16);
            lblHost.TabIndex = 0;
            lblHost.Text = "שרת SMTP:";
            //
            // txtHost
            //
            txtHost.Anchor = AnchorStyles.Top;
            txtHost.Location = new Point(150, 37);
            txtHost.Name = "txtHost";
            txtHost.Size = new Size(330, 24);
            txtHost.TabIndex = 1;
            //
            // lblPort
            //
            lblPort.Anchor = AnchorStyles.Top;
            lblPort.Location = new Point(490, 72);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(32, 16);
            lblPort.TabIndex = 2;
            lblPort.Text = "פורט:";
            //
            // spinPort
            //
            spinPort.Anchor = AnchorStyles.Top;
            spinPort.EditValue = 587;
            spinPort.Location = new Point(380, 69);
            spinPort.Name = "spinPort";
            spinPort.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            spinPort.Properties.IsFloatValue = false;
            spinPort.Properties.MaskSettings.Set("mask", "N00");
            spinPort.Properties.MaxValue = new decimal(new int[] { 65535, 0, 0, 0 });
            spinPort.Properties.MinValue = new decimal(new int[] { 1, 0, 0, 0 });
            spinPort.Size = new Size(100, 24);
            spinPort.TabIndex = 3;
            //
            // chkSsl
            //
            chkSsl.Anchor = AnchorStyles.Top;
            chkSsl.EditValue = true;
            chkSsl.Location = new Point(150, 101);
            chkSsl.Name = "chkSsl";
            chkSsl.Properties.Caption = "שימוש ב-SSL/TLS";
            chkSsl.RightToLeft = RightToLeft.Yes;
            chkSsl.Size = new Size(330, 20);
            chkSsl.TabIndex = 4;
            //
            // lblFrom
            //
            lblFrom.Anchor = AnchorStyles.Top;
            lblFrom.Location = new Point(490, 136);
            lblFrom.Name = "lblFrom";
            lblFrom.Size = new Size(78, 16);
            lblFrom.TabIndex = 5;
            lblFrom.Text = "כתובת שולח:";
            //
            // txtFrom
            //
            txtFrom.Anchor = AnchorStyles.Top;
            txtFrom.Location = new Point(150, 133);
            txtFrom.Name = "txtFrom";
            txtFrom.Size = new Size(330, 24);
            txtFrom.TabIndex = 6;
            //
            // lblUser
            //
            lblUser.Anchor = AnchorStyles.Top;
            lblUser.Location = new Point(490, 168);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(112, 16);
            lblUser.TabIndex = 7;
            lblUser.Text = "שם משתמש בשרת:";
            //
            // txtUser
            //
            txtUser.Anchor = AnchorStyles.Top;
            txtUser.Location = new Point(150, 165);
            txtUser.Name = "txtUser";
            txtUser.Size = new Size(330, 24);
            txtUser.TabIndex = 8;
            //
            // lblUserNote
            //
            lblUserNote.Anchor = AnchorStyles.Top;
            lblUserNote.Appearance.ForeColor = Color.FromArgb(110, 110, 110);
            lblUserNote.Location = new Point(150, 192);
            lblUserNote.Name = "lblUserNote";
            lblUserNote.Size = new Size(330, 14);
            lblUserNote.TabIndex = 9;
            lblUserNote.Text = "כתובת דואר מלאה, לא שם המשתמש בתוכנה. ריק — כתובת השולח.";
            //
            // lblPassword
            //
            lblPassword.Anchor = AnchorStyles.Top;
            lblPassword.Location = new Point(490, 216);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(40, 16);
            lblPassword.TabIndex = 10;
            lblPassword.Text = "סיסמה:";
            //
            // txtPassword
            //
            txtPassword.Anchor = AnchorStyles.Top;
            txtPassword.Location = new Point(150, 213);
            txtPassword.Name = "txtPassword";
            txtPassword.Properties.Buttons.Clear();
            txtPassword.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph) });
            txtPassword.Properties.Buttons[0].Caption = "הצג";
            txtPassword.Properties.Buttons[0].Width = 46;
            txtPassword.Properties.UseSystemPasswordChar = true;
            txtPassword.Size = new Size(330, 24);
            txtPassword.TabIndex = 11;
            txtPassword.Properties.ButtonClick += txtPassword_ButtonClick;
            //
            // lblPasswordNote
            //
            lblPasswordNote.Anchor = AnchorStyles.Top;
            lblPasswordNote.Appearance.ForeColor = Color.FromArgb(110, 110, 110);
            lblPasswordNote.Location = new Point(150, 240);
            lblPasswordNote.Name = "lblPasswordNote";
            lblPasswordNote.Size = new Size(330, 14);
            lblPasswordNote.TabIndex = 12;
            lblPasswordNote.Text = "הסיסמה נשמרת מוצפנת, וניתנת לפענוח במחשב הזה בלבד.";
            //
            // grpTest
            //
            grpTest.Controls.Add(txtTestTo);
            grpTest.Controls.Add(lblTestTo);
            grpTest.Dock = DockStyle.Top;
            grpTest.Location = new Point(0, 348);
            grpTest.Name = "grpTest";
            grpTest.RightToLeft = RightToLeft.Yes;
            grpTest.Size = new Size(620, 72);
            grpTest.TabIndex = 2;
            grpTest.Text = "בדיקה";
            //
            // lblTestTo
            //
            lblTestTo.Anchor = AnchorStyles.Top;
            lblTestTo.Location = new Point(490, 36);
            lblTestTo.Name = "lblTestTo";
            lblTestTo.Size = new Size(96, 16);
            lblTestTo.TabIndex = 0;
            lblTestTo.Text = "לשלוח בדיקה אל:";
            //
            // txtTestTo
            //
            txtTestTo.Anchor = AnchorStyles.Top;
            txtTestTo.Location = new Point(150, 33);
            txtTestTo.Name = "txtTestTo";
            txtTestTo.Size = new Size(330, 24);
            txtTestTo.TabIndex = 1;
            //
            // memStatus
            //
            memStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            memStatus.Location = new Point(14, 428);
            memStatus.Name = "memStatus";
            memStatus.Properties.Appearance.BackColor = Color.Transparent;
            memStatus.Properties.Appearance.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            memStatus.Properties.Appearance.Options.UseBackColor = true;
            memStatus.Properties.Appearance.Options.UseFont = true;
            memStatus.Properties.Appearance.Options.UseForeColor = true;
            memStatus.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            memStatus.Properties.ReadOnly = true;
            memStatus.Properties.ScrollBars = ScrollBars.Vertical;
            memStatus.Size = new Size(590, 60);
            memStatus.TabIndex = 3;
            //
            // panelButtons
            //
            panelButtons.Controls.Add(btnImport);
            panelButtons.Controls.Add(btnClose);
            panelButtons.Controls.Add(btnSave);
            panelButtons.Controls.Add(btnTest);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 496);
            panelButtons.Name = "panelButtons";
            panelButtons.RightToLeft = RightToLeft.Yes;
            panelButtons.Size = new Size(620, 50);
            panelButtons.TabIndex = 4;
            //
            // btnTest
            //
            btnTest.Anchor = AnchorStyles.Top;
            btnTest.Location = new Point(14, 11);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(150, 28);
            btnTest.TabIndex = 0;
            btnTest.Text = "שלח הודעת בדיקה";
            btnTest.Click += btnTest_Click;
            //
            // btnSave
            //
            btnSave.Anchor = AnchorStyles.Top;
            btnSave.Location = new Point(174, 11);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(110, 28);
            btnSave.TabIndex = 1;
            btnSave.Text = "שמור";
            btnSave.Click += btnSave_Click;
            //
            // btnClose
            //
            btnClose.Anchor = AnchorStyles.Top;
            btnClose.Location = new Point(294, 11);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(110, 28);
            btnClose.TabIndex = 2;
            btnClose.Text = "סגור";
            btnClose.Click += btnClose_Click;
            //
            // btnImport
            //
            btnImport.Anchor = AnchorStyles.Top;
            btnImport.Location = new Point(414, 11);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(170, 28);
            btnImport.TabIndex = 3;
            btnImport.Text = "ייבא מ-MonicipalYBD";
            btnImport.ToolTip = "טוען את הגדרות הדואר שכבר עובדות בתוכנת הגבייה";
            btnImport.Click += btnImport_Click;
            //
            // MailSetupForm
            //
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(620, 546);
            Controls.Add(memStatus);
            Controls.Add(panelButtons);
            Controls.Add(grpTest);
            Controls.Add(grpServer);
            Controls.Add(grpMethod);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MailSetupForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "הגדרות דואר יוצא";
            Load += MailSetupForm_Load;
            ((System.ComponentModel.ISupportInitialize)grpMethod).EndInit();
            grpMethod.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)radioMethod.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)grpServer).EndInit();
            grpServer.ResumeLayout(false);
            grpServer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtHost.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinPort.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkSsl.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtFrom.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtUser.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtPassword.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)grpTest).EndInit();
            grpTest.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)txtTestTo.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)memStatus.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).EndInit();
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupControl grpMethod;
        private RadioGroup radioMethod;
        private GroupControl grpServer;
        private LabelControl lblHost;
        private TextEdit txtHost;
        private LabelControl lblPort;
        private SpinEdit spinPort;
        private CheckEdit chkSsl;
        private LabelControl lblFrom;
        private TextEdit txtFrom;
        private LabelControl lblUser;
        private TextEdit txtUser;
        private LabelControl lblUserNote;
        private LabelControl lblPassword;
        private ButtonEdit txtPassword;
        private LabelControl lblPasswordNote;
        private GroupControl grpTest;
        private LabelControl lblTestTo;
        private TextEdit txtTestTo;
        private MemoEdit memStatus;
        private PanelControl panelButtons;
        private SimpleButton btnTest;
        private SimpleButton btnSave;
        private SimpleButton btnClose;
        private SimpleButton btnImport;
    }
}
