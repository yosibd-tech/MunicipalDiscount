using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

namespace MunicipalDiscount
{
    partial class DiscountEligibilityChecks
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiscountEligibilityChecks));
            panelRight = new PanelControl();
            grpResults = new GroupControl();
            btnConfirm = new SimpleButton();
            btnPrint = new SimpleButton();
            btnSavePdf = new SimpleButton();
            btnClear = new SimpleButton();
            btnClose = new SimpleButton();
            txtDiscountRate = new TextEdit();
            txtTotalIncome = new TextEdit();
            lblDiscountRate = new LabelControl();
            lblTotalIncome = new LabelControl();
            grpIncomeDetails = new GroupControl();
            btnCalculate = new SimpleButton();
            grpFamilyDetails = new GroupControl();
            panelMemberButtons = new PanelControl();
            btnAddMember = new SimpleButton();
            btnRemoveMember = new SimpleButton();
            gridMembers = new GridControl();
            viewMembers = new GridView();
            colNumber = new GridColumn();
            colName = new GridColumn();
            colAge = new GridColumn();
            colIncome = new GridColumn();
            riName = new RepositoryItemTextEdit();
            riAge = new RepositoryItemSpinEdit();
            riIncome = new RepositoryItemTextEdit();
            grpClientDetails = new GroupControl();
            txtClientName = new TextEdit();
            txtClientNumber = new ButtonEdit();
            btnAddNewClient = new SimpleButton();
            lblClientName = new LabelControl();
            lblClientNumber = new LabelControl();
            panelDocuments = new PanelControl();
            grpDocuments = new GroupControl();
            chkSelectAll = new CheckEdit();
            chkDocuments = new CheckedListBoxControl();
            ((System.ComponentModel.ISupportInitialize)panelRight).BeginInit();
            panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grpResults).BeginInit();
            grpResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtDiscountRate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtTotalIncome.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grpIncomeDetails).BeginInit();
            grpIncomeDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grpFamilyDetails).BeginInit();
            grpFamilyDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelMemberButtons).BeginInit();
            panelMemberButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridMembers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewMembers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riName).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riAge).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riIncome).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grpClientDetails).BeginInit();
            grpClientDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtClientName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtClientNumber.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelDocuments).BeginInit();
            panelDocuments.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grpDocuments).BeginInit();
            grpDocuments.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chkSelectAll.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkDocuments).BeginInit();
            SuspendLayout();
            //
            // panelRight
            //
            panelRight.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            panelRight.Controls.Add(grpResults);
            panelRight.Controls.Add(grpIncomeDetails);
            panelRight.Controls.Add(grpFamilyDetails);
            panelRight.Controls.Add(grpClientDetails);
            panelRight.Dock = DockStyle.Left;
            panelRight.Location = new Point(0, 0);
            panelRight.Name = "panelRight";
            panelRight.Padding = new Padding(10);
            panelRight.Size = new Size(440, 722);
            panelRight.TabIndex = 0;
            //
            // grpResults
            //
            grpResults.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            grpResults.AppearanceCaption.ForeColor = Color.FromArgb(42, 82, 152);
            grpResults.AppearanceCaption.Options.UseFont = true;
            grpResults.AppearanceCaption.Options.UseForeColor = true;
            grpResults.AppearanceCaption.Options.UseTextOptions = true;
            grpResults.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            grpResults.Controls.Add(btnConfirm);
            grpResults.Controls.Add(btnPrint);
            grpResults.Controls.Add(btnSavePdf);
            grpResults.Controls.Add(btnClear);
            grpResults.Controls.Add(btnClose);
            grpResults.Controls.Add(txtDiscountRate);
            grpResults.Controls.Add(txtTotalIncome);
            grpResults.Controls.Add(lblDiscountRate);
            grpResults.Controls.Add(lblTotalIncome);
            grpResults.Dock = DockStyle.Top;
            grpResults.Location = new Point(10, 520);
            grpResults.Name = "grpResults";
            grpResults.RightToLeft = RightToLeft.Yes;
            grpResults.Size = new Size(420, 190);
            grpResults.TabIndex = 3;
            grpResults.Text = "תוצאת הנחה";
            //
            // btnConfirm
            //
            btnConfirm.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            btnConfirm.Appearance.Options.UseFont = true;
            btnConfirm.Location = new Point(5, 110);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new Size(405, 32);
            btnConfirm.TabIndex = 4;
            btnConfirm.Text = "אישור הנחה";
            btnConfirm.Click += btnConfirm_Click;
            //
            // btnPrint
            //
            btnPrint.Location = new Point(310, 148);
            btnPrint.Name = "btnPrint";
            btnPrint.Size = new Size(100, 28);
            btnPrint.TabIndex = 5;
            btnPrint.Text = "הדפסה";
            btnPrint.Click += btnPrint_Click;
            //
            // btnSavePdf
            //
            btnSavePdf.Location = new Point(205, 148);
            btnSavePdf.Name = "btnSavePdf";
            btnSavePdf.Size = new Size(100, 28);
            btnSavePdf.TabIndex = 6;
            btnSavePdf.Text = "שמור PDF";
            btnSavePdf.Click += btnSavePdf_Click;
            //
            // btnClear
            //
            btnClear.Location = new Point(100, 148);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(100, 28);
            btnClear.TabIndex = 7;
            btnClear.Text = "נקה טופס";
            btnClear.Click += btnClear_Click;
            //
            // btnClose
            //
            btnClose.Location = new Point(5, 148);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 28);
            btnClose.TabIndex = 8;
            btnClose.Text = "סגור";
            btnClose.Click += btnClose_Click;
            //
            // txtDiscountRate
            //
            txtDiscountRate.Location = new Point(5, 70);
            txtDiscountRate.Name = "txtDiscountRate";
            txtDiscountRate.Properties.Appearance.Font = new Font("Tahoma", 11F, FontStyle.Bold);
            txtDiscountRate.Properties.Appearance.ForeColor = Color.FromArgb(39, 174, 96);
            txtDiscountRate.Properties.Appearance.Options.UseFont = true;
            txtDiscountRate.Properties.Appearance.Options.UseForeColor = true;
            txtDiscountRate.Properties.ReadOnly = true;
            txtDiscountRate.RightToLeft = RightToLeft.Yes;
            txtDiscountRate.Size = new Size(130, 28);
            txtDiscountRate.TabIndex = 3;
            //
            // txtTotalIncome
            //
            txtTotalIncome.Location = new Point(5, 35);
            txtTotalIncome.Name = "txtTotalIncome";
            txtTotalIncome.Properties.ReadOnly = true;
            txtTotalIncome.RightToLeft = RightToLeft.Yes;
            txtTotalIncome.Size = new Size(130, 22);
            txtTotalIncome.TabIndex = 1;
            //
            // lblDiscountRate
            //
            lblDiscountRate.Location = new Point(330, 73);
            lblDiscountRate.Name = "lblDiscountRate";
            lblDiscountRate.Size = new Size(68, 16);
            lblDiscountRate.TabIndex = 2;
            lblDiscountRate.Text = "אחוז הנחה:";
            //
            // lblTotalIncome
            //
            lblTotalIncome.Location = new Point(275, 38);
            lblTotalIncome.Name = "lblTotalIncome";
            lblTotalIncome.Size = new Size(135, 16);
            lblTotalIncome.TabIndex = 0;
            lblTotalIncome.Text = "סה\"כ הכנסה ממוצעת:";
            //
            // grpIncomeDetails
            //
            grpIncomeDetails.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            grpIncomeDetails.AppearanceCaption.ForeColor = Color.FromArgb(42, 82, 152);
            grpIncomeDetails.AppearanceCaption.Options.UseFont = true;
            grpIncomeDetails.AppearanceCaption.Options.UseForeColor = true;
            grpIncomeDetails.AppearanceCaption.Options.UseTextOptions = true;
            grpIncomeDetails.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            grpIncomeDetails.Controls.Add(btnCalculate);
            grpIncomeDetails.Dock = DockStyle.Top;
            grpIncomeDetails.Location = new Point(10, 450);
            grpIncomeDetails.Name = "grpIncomeDetails";
            grpIncomeDetails.RightToLeft = RightToLeft.Yes;
            grpIncomeDetails.Size = new Size(420, 70);
            grpIncomeDetails.TabIndex = 2;
            grpIncomeDetails.Text = "מקורות הכנסה";
            //
            // btnCalculate
            //
            btnCalculate.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            btnCalculate.Appearance.Options.UseFont = true;
            btnCalculate.Location = new Point(5, 28);
            btnCalculate.Name = "btnCalculate";
            btnCalculate.Size = new Size(405, 34);
            btnCalculate.TabIndex = 0;
            btnCalculate.Text = "חשב הנחה";
            btnCalculate.Click += btnCalculate_Click;
            //
            // grpFamilyDetails
            //
            grpFamilyDetails.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            grpFamilyDetails.AppearanceCaption.ForeColor = Color.FromArgb(42, 82, 152);
            grpFamilyDetails.AppearanceCaption.Options.UseFont = true;
            grpFamilyDetails.AppearanceCaption.Options.UseForeColor = true;
            grpFamilyDetails.AppearanceCaption.Options.UseTextOptions = true;
            grpFamilyDetails.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            grpFamilyDetails.Controls.Add(gridMembers);
            grpFamilyDetails.Controls.Add(panelMemberButtons);
            grpFamilyDetails.Dock = DockStyle.Top;
            grpFamilyDetails.Location = new Point(10, 110);
            grpFamilyDetails.Name = "grpFamilyDetails";
            grpFamilyDetails.RightToLeft = RightToLeft.Yes;
            grpFamilyDetails.Size = new Size(420, 340);
            grpFamilyDetails.TabIndex = 1;
            grpFamilyDetails.Text = "פרטי משפחה";
            //
            // panelMemberButtons
            //
            panelMemberButtons.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            panelMemberButtons.Controls.Add(btnAddMember);
            panelMemberButtons.Controls.Add(btnRemoveMember);
            panelMemberButtons.Dock = DockStyle.Top;
            panelMemberButtons.Location = new Point(2, 24);
            panelMemberButtons.Name = "panelMemberButtons";
            panelMemberButtons.RightToLeft = RightToLeft.Yes;
            panelMemberButtons.Size = new Size(416, 36);
            panelMemberButtons.TabIndex = 0;
            //
            // btnAddMember
            //
            btnAddMember.Location = new Point(5, 5);
            btnAddMember.Name = "btnAddMember";
            btnAddMember.Size = new Size(85, 26);
            btnAddMember.TabIndex = 0;
            btnAddMember.Text = "+ הוסף";
            btnAddMember.Click += btnAddMember_Click;
            //
            // btnRemoveMember
            //
            btnRemoveMember.Location = new Point(95, 5);
            btnRemoveMember.Name = "btnRemoveMember";
            btnRemoveMember.Size = new Size(85, 26);
            btnRemoveMember.TabIndex = 1;
            btnRemoveMember.Text = "- הסר";
            btnRemoveMember.Click += btnRemoveMember_Click;
            //
            // gridMembers
            //
            gridMembers.Dock = DockStyle.Fill;
            gridMembers.Location = new Point(2, 60);
            gridMembers.MainView = viewMembers;
            gridMembers.Name = "gridMembers";
            gridMembers.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { riName, riAge, riIncome });
            gridMembers.RightToLeft = RightToLeft.Yes;
            gridMembers.Size = new Size(416, 278);
            gridMembers.TabIndex = 1;
            gridMembers.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { viewMembers });
            //
            // viewMembers
            //
            viewMembers.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { colNumber, colName, colAge, colIncome });
            viewMembers.GridControl = gridMembers;
            viewMembers.Name = "viewMembers";
            viewMembers.OptionsBehavior.Editable = true;
            viewMembers.OptionsView.ShowGroupPanel = false;
            //
            // colNumber
            //
            colNumber.Caption = "מספר";
            colNumber.FieldName = "מספר";
            colNumber.Name = "colNumber";
            colNumber.OptionsColumn.AllowEdit = false;
            colNumber.OptionsColumn.ReadOnly = true;
            colNumber.Visible = true;
            colNumber.VisibleIndex = 0;
            colNumber.Width = 50;
            //
            // colName
            //
            colName.Caption = "שם";
            colName.ColumnEdit = riName;
            colName.FieldName = "שם";
            colName.Name = "colName";
            colName.Visible = true;
            colName.VisibleIndex = 1;
            colName.Width = 150;
            //
            // colAge
            //
            colAge.Caption = "גיל/AGE";
            colAge.ColumnEdit = riAge;
            colAge.FieldName = "גיל";
            colAge.Name = "colAge";
            colAge.Visible = true;
            colAge.VisibleIndex = 2;
            colAge.Width = 70;
            //
            // colIncome
            //
            colIncome.Caption = "הכנסה/INCOME";
            colIncome.ColumnEdit = riIncome;
            colIncome.FieldName = "הכנסה";
            colIncome.Name = "colIncome";
            colIncome.Visible = true;
            colIncome.VisibleIndex = 3;
            colIncome.Width = 100;
            //
            // riName
            //
            riName.AutoHeight = false;
            riName.Name = "riName";
            //
            // riAge
            //
            riAge.AutoHeight = false;
            riAge.MaxValue = new decimal(new int[] { 120, 0, 0, 0 });
            riAge.MinValue = new decimal(new int[] { 0, 0, 0, 0 });
            riAge.Name = "riAge";
            //
            // riIncome
            //
            riIncome.AutoHeight = false;
            riIncome.Mask.EditMask = "n0";
            riIncome.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            riIncome.Mask.UseMaskAsDisplayFormat = true;
            riIncome.Name = "riIncome";
            //
            // grpClientDetails
            //
            grpClientDetails.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            grpClientDetails.AppearanceCaption.ForeColor = Color.FromArgb(42, 82, 152);
            grpClientDetails.AppearanceCaption.Options.UseFont = true;
            grpClientDetails.AppearanceCaption.Options.UseForeColor = true;
            grpClientDetails.AppearanceCaption.Options.UseTextOptions = true;
            grpClientDetails.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            grpClientDetails.Controls.Add(txtClientName);
            grpClientDetails.Controls.Add(txtClientNumber);
            grpClientDetails.Controls.Add(btnAddNewClient);
            grpClientDetails.Controls.Add(lblClientName);
            grpClientDetails.Controls.Add(lblClientNumber);
            grpClientDetails.Dock = DockStyle.Top;
            grpClientDetails.Location = new Point(10, 10);
            grpClientDetails.Name = "grpClientDetails";
            grpClientDetails.RightToLeft = RightToLeft.Yes;
            grpClientDetails.Size = new Size(420, 100);
            grpClientDetails.TabIndex = 0;
            grpClientDetails.Text = "פרטי לקוח";
            //
            // txtClientName
            //
            txtClientName.Location = new Point(5, 70);
            txtClientName.Name = "txtClientName";
            txtClientName.Properties.ReadOnly = true;
            txtClientName.RightToLeft = RightToLeft.Yes;
            txtClientName.Size = new Size(220, 22);
            txtClientName.TabIndex = 4;
            //
            // txtClientNumber
            //
            txtClientNumber.Location = new Point(45, 35);
            txtClientNumber.Name = "txtClientNumber";
            txtClientNumber.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Search) });
            txtClientNumber.RightToLeft = RightToLeft.Yes;
            txtClientNumber.Size = new Size(150, 22);
            txtClientNumber.TabIndex = 1;
            txtClientNumber.ButtonClick += txtClientNumber_ButtonClick;
            txtClientNumber.KeyDown += txtClientNumber_KeyDown;
            //
            // btnAddNewClient
            //
            btnAddNewClient.Location = new Point(5, 35);
            btnAddNewClient.Name = "btnAddNewClient";
            btnAddNewClient.Size = new Size(35, 24);
            btnAddNewClient.TabIndex = 2;
            btnAddNewClient.Text = "+";
            btnAddNewClient.ToolTip = "הוסף לקוח חדש";
            btnAddNewClient.Click += btnAddNewClient_Click;
            //
            // lblClientName
            //
            lblClientName.Location = new Point(345, 73);
            lblClientName.Name = "lblClientName";
            lblClientName.Size = new Size(59, 16);
            lblClientName.TabIndex = 3;
            lblClientName.Text = "שם לקוח:";
            //
            // lblClientNumber
            //
            lblClientNumber.Location = new Point(330, 38);
            lblClientNumber.Name = "lblClientNumber";
            lblClientNumber.Size = new Size(75, 16);
            lblClientNumber.TabIndex = 0;
            lblClientNumber.Text = "מספר לקוח:";
            //
            // panelDocuments
            //
            panelDocuments.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            panelDocuments.Controls.Add(grpDocuments);
            panelDocuments.Dock = DockStyle.Fill;
            panelDocuments.Location = new Point(440, 0);
            panelDocuments.Name = "panelDocuments";
            panelDocuments.Padding = new Padding(10);
            panelDocuments.Size = new Size(344, 722);
            panelDocuments.TabIndex = 1;
            //
            // grpDocuments
            //
            grpDocuments.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            grpDocuments.AppearanceCaption.ForeColor = Color.FromArgb(42, 82, 152);
            grpDocuments.AppearanceCaption.Options.UseFont = true;
            grpDocuments.AppearanceCaption.Options.UseForeColor = true;
            grpDocuments.AppearanceCaption.Options.UseTextOptions = true;
            grpDocuments.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            grpDocuments.Controls.Add(chkSelectAll);
            grpDocuments.Controls.Add(chkDocuments);
            grpDocuments.Dock = DockStyle.Fill;
            grpDocuments.Location = new Point(10, 10);
            grpDocuments.Name = "grpDocuments";
            grpDocuments.RightToLeft = RightToLeft.Yes;
            grpDocuments.Size = new Size(324, 702);
            grpDocuments.TabIndex = 0;
            grpDocuments.Text = "מסמכים שהוגשו";
            //
            // chkSelectAll
            //
            chkSelectAll.Location = new Point(10, 35);
            chkSelectAll.Name = "chkSelectAll";
            chkSelectAll.Properties.Appearance.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            chkSelectAll.Properties.Appearance.ForeColor = Color.FromArgb(42, 82, 152);
            chkSelectAll.Properties.Appearance.Options.UseFont = true;
            chkSelectAll.Properties.Appearance.Options.UseForeColor = true;
            chkSelectAll.Properties.Caption = "סמן הכל";
            chkSelectAll.RightToLeft = RightToLeft.Yes;
            chkSelectAll.Size = new Size(300, 25);
            chkSelectAll.TabIndex = 0;
            chkSelectAll.CheckedChanged += chkSelectAll_CheckedChanged;
            //
            // chkDocuments
            //
            chkDocuments.CheckOnClick = true;
            chkDocuments.ItemHeight = 30;
            chkDocuments.Items.AddRange(new DevExpress.XtraEditors.Controls.CheckedListBoxItem[] { new DevExpress.XtraEditors.Controls.CheckedListBoxItem("צילום ת.זהות וספח"), new DevExpress.XtraEditors.Controls.CheckedListBoxItem("אישור ב.לאומי על תקופות עיסוק והכנסות"), new DevExpress.XtraEditors.Controls.CheckedListBoxItem("שומת מס הכנסה"), new DevExpress.XtraEditors.Controls.CheckedListBoxItem("תדפיס אוכלוסין"), new DevExpress.XtraEditors.Controls.CheckedListBoxItem("אישור ב.לאומי מעמד לא עובד"), new DevExpress.XtraEditors.Controls.CheckedListBoxItem("צילום דפי בנק חודשים 10-12") });
            chkDocuments.Location = new Point(10, 70);
            chkDocuments.Name = "chkDocuments";
            chkDocuments.RightToLeft = RightToLeft.Yes;
            chkDocuments.Size = new Size(300, 210);
            chkDocuments.TabIndex = 1;
            //
            // DiscountEligibilityChecks
            //
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 722);
            Controls.Add(panelDocuments);
            Controls.Add(panelRight);
            IconOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("DiscountEligibilityChecks.IconOptions.SvgImage");
            MinimumSize = new Size(786, 762);
            Name = "DiscountEligibilityChecks";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "בדיקת זכאות להנחה - מבחן הכנסה";
            Load += DiscountEligibilityChecks_Load;
            ((System.ComponentModel.ISupportInitialize)panelRight).EndInit();
            panelRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grpResults).EndInit();
            grpResults.ResumeLayout(false);
            grpResults.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtDiscountRate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtTotalIncome.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)grpIncomeDetails).EndInit();
            grpIncomeDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grpFamilyDetails).EndInit();
            grpFamilyDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)panelMemberButtons).EndInit();
            panelMemberButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)riIncome).EndInit();
            ((System.ComponentModel.ISupportInitialize)riAge).EndInit();
            ((System.ComponentModel.ISupportInitialize)riName).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewMembers).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridMembers).EndInit();
            ((System.ComponentModel.ISupportInitialize)grpClientDetails).EndInit();
            grpClientDetails.ResumeLayout(false);
            grpClientDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtClientName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtClientNumber.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelDocuments).EndInit();
            panelDocuments.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grpDocuments).EndInit();
            grpDocuments.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chkSelectAll.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkDocuments).EndInit();
            ResumeLayout(false);
        }

        #endregion

        // MAIN PANELS
        private DevExpress.XtraEditors.PanelControl panelRight;
        private DevExpress.XtraEditors.PanelControl panelDocuments;

        // GROUP CONTROLS
        private DevExpress.XtraEditors.GroupControl grpDocuments;
        private DevExpress.XtraEditors.GroupControl grpClientDetails;
        private DevExpress.XtraEditors.GroupControl grpFamilyDetails;
        private DevExpress.XtraEditors.GroupControl grpIncomeDetails;
        private DevExpress.XtraEditors.GroupControl grpResults;

        // DOCUMENTS
        private DevExpress.XtraEditors.CheckEdit chkSelectAll;
        private DevExpress.XtraEditors.CheckedListBoxControl chkDocuments;

        // CLIENT
        private DevExpress.XtraEditors.LabelControl lblClientNumber;
        private DevExpress.XtraEditors.ButtonEdit txtClientNumber;
        private DevExpress.XtraEditors.SimpleButton btnAddNewClient;
        private DevExpress.XtraEditors.LabelControl lblClientName;
        private DevExpress.XtraEditors.TextEdit txtClientName;

        // FAMILY MEMBERS GRID
        private DevExpress.XtraEditors.PanelControl panelMemberButtons;
        private DevExpress.XtraEditors.SimpleButton btnAddMember;
        private DevExpress.XtraEditors.SimpleButton btnRemoveMember;
        private DevExpress.XtraGrid.GridControl gridMembers;
        private DevExpress.XtraGrid.Views.Grid.GridView viewMembers;
        private DevExpress.XtraGrid.Columns.GridColumn colNumber;
        private DevExpress.XtraGrid.Columns.GridColumn colName;
        private DevExpress.XtraGrid.Columns.GridColumn colAge;
        private DevExpress.XtraGrid.Columns.GridColumn colIncome;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit riName;
        private DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit riAge;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit riIncome;

        // INCOME
        private DevExpress.XtraEditors.SimpleButton btnCalculate;

        // RESULTS
        private DevExpress.XtraEditors.LabelControl lblTotalIncome;
        private DevExpress.XtraEditors.TextEdit txtTotalIncome;
        private DevExpress.XtraEditors.LabelControl lblDiscountRate;
        private DevExpress.XtraEditors.TextEdit txtDiscountRate;
        private DevExpress.XtraEditors.SimpleButton btnConfirm;
        private DevExpress.XtraEditors.SimpleButton btnPrint;
        private DevExpress.XtraEditors.SimpleButton btnSavePdf;
        private DevExpress.XtraEditors.SimpleButton btnClear;
        private DevExpress.XtraEditors.SimpleButton btnClose;
    }
}
