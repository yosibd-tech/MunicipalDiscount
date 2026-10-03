using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    partial class AdminForm
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
            panelTop = new PanelControl();
            lblSummary = new LabelControl();
            btnRefresh = new SimpleButton();
            lblYear = new LabelControl();
            spinYear = new SpinEdit();
            gridChecks = new GridControl();
            viewChecks = new GridView();
            colSelect = new DevExpress.XtraGrid.Columns.GridColumn();
            riCheck = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            colGroup = new DevExpress.XtraGrid.Columns.GridColumn();
            colCheck = new DevExpress.XtraGrid.Columns.GridColumn();
            colStatus = new DevExpress.XtraGrid.Columns.GridColumn();
            colDetail = new DevExpress.XtraGrid.Columns.GridColumn();
            colAction = new DevExpress.XtraGrid.Columns.GridColumn();
            panelBottom = new PanelControl();
            btnClose = new SimpleButton();
            btnMail = new SimpleButton();
            btnExcel = new SimpleButton();
            ((System.ComponentModel.ISupportInitialize)panelTop).BeginInit();
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)spinYear.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridChecks).BeginInit();
            ((System.ComponentModel.ISupportInitialize)viewChecks).BeginInit();
            ((System.ComponentModel.ISupportInitialize)riCheck).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelBottom).BeginInit();
            panelBottom.SuspendLayout();
            SuspendLayout();
            //
            // panelTop
            //
            panelTop.Controls.Add(lblSummary);
            panelTop.Controls.Add(btnRefresh);
            panelTop.Controls.Add(spinYear);
            panelTop.Controls.Add(lblYear);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.RightToLeft = RightToLeft.Yes;
            panelTop.Size = new Size(940, 56);
            panelTop.TabIndex = 0;
            //
            // lblYear
            //
            lblYear.Anchor = AnchorStyles.Top;
            lblYear.Location = new Point(860, 20);
            lblYear.Name = "lblYear";
            lblYear.Size = new Size(30, 16);
            lblYear.TabIndex = 0;
            lblYear.Text = "שנה:";
            //
            // spinYear
            //
            spinYear.Anchor = AnchorStyles.Top;
            spinYear.EditValue = new decimal(new int[] { 2026, 0, 0, 0 });
            spinYear.Location = new Point(760, 17);
            spinYear.Name = "spinYear";
            spinYear.Properties.Appearance.Options.UseTextOptions = true;
            spinYear.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            spinYear.Properties.IsFloatValue = false;
            spinYear.Properties.Mask.EditMask = "0000";
            spinYear.Properties.MaxValue = new decimal(new int[] { 2100, 0, 0, 0 });
            spinYear.Properties.MinValue = new decimal(new int[] { 2000, 0, 0, 0 });
            spinYear.Size = new Size(90, 24);
            spinYear.TabIndex = 1;
            //
            // btnRefresh
            //
            btnRefresh.Anchor = AnchorStyles.Top;
            btnRefresh.Location = new Point(640, 15);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(100, 28);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "בדוק שוב";
            btnRefresh.Click += btnRefresh_Click;
            //
            // lblSummary
            //
            lblSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblSummary.Appearance.Font = new Font("Tahoma", 9.75F, FontStyle.Bold);
            lblSummary.Appearance.Options.UseFont = true;
            lblSummary.Location = new Point(14, 21);
            lblSummary.Name = "lblSummary";
            lblSummary.Size = new Size(0, 16);
            lblSummary.TabIndex = 3;
            //
            // gridChecks
            //
            gridChecks.Dock = DockStyle.Fill;
            gridChecks.Location = new Point(0, 56);
            gridChecks.MainView = viewChecks;
            gridChecks.Name = "gridChecks";
            gridChecks.RightToLeft = RightToLeft.Yes;
            gridChecks.Size = new Size(940, 498);
            gridChecks.TabIndex = 1;
            gridChecks.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { riCheck });
            gridChecks.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { viewChecks });
            //
            // viewChecks
            //
            viewChecks.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { colSelect, colGroup, colCheck, colStatus, colDetail, colAction });
            viewChecks.GridControl = gridChecks;
            viewChecks.Name = "viewChecks";
            viewChecks.OptionsBehavior.Editable = true;
            viewChecks.OptionsView.ShowGroupPanel = false;
            viewChecks.OptionsView.RowAutoHeight = true;
            viewChecks.RowCellStyle += viewChecks_RowCellStyle;
            //
            // colGroup
            //
            //
            // colSelect
            //
            colSelect.Caption = "כלול";
            colSelect.ColumnEdit = riCheck;
            colSelect.FieldName = "Selected";
            colSelect.Name = "colSelect";
            colSelect.Visible = true;
            colSelect.VisibleIndex = 0;
            colSelect.Width = 45;
            //
            // riCheck
            //
            riCheck.AutoHeight = false;
            riCheck.Name = "riCheck";
            //
            // colGroup — read only, like every column but the tick
            //
            colGroup.OptionsColumn.AllowEdit = false;
            colGroup.Caption = "נושא";
            colGroup.FieldName = "Group";
            colGroup.Name = "colGroup";
            colGroup.Visible = true;
            colGroup.VisibleIndex = 1;
            colGroup.Width = 90;
            //
            // colCheck
            //
            colCheck.OptionsColumn.AllowEdit = false;
            colCheck.Caption = "בדיקה";
            colCheck.FieldName = "Name";
            colCheck.Name = "colCheck";
            colCheck.Visible = true;
            colCheck.VisibleIndex = 2;
            colCheck.Width = 210;
            //
            // colStatus
            //
            colStatus.AppearanceCell.Options.UseTextOptions = true;
            colStatus.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            colStatus.OptionsColumn.AllowEdit = false;
            colStatus.Caption = "מצב";
            colStatus.FieldName = "StatusText";
            colStatus.Name = "colStatus";
            colStatus.Visible = true;
            colStatus.VisibleIndex = 3;
            colStatus.Width = 70;
            //
            // colDetail
            //
            colDetail.OptionsColumn.AllowEdit = false;
            colDetail.Caption = "פרטים";
            colDetail.FieldName = "Detail";
            colDetail.Name = "colDetail";
            colDetail.Visible = true;
            colDetail.VisibleIndex = 4;
            colDetail.Width = 290;
            //
            // colAction
            //
            colAction.OptionsColumn.AllowEdit = false;
            colAction.Caption = "מה לעשות";
            colAction.FieldName = "Action";
            colAction.Name = "colAction";
            colAction.Visible = true;
            colAction.VisibleIndex = 5;
            colAction.Width = 280;
            //
            // panelBottom
            //
            panelBottom.Controls.Add(btnClose);
            panelBottom.Controls.Add(btnMail);
            panelBottom.Controls.Add(btnExcel);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 554);
            panelBottom.Name = "panelBottom";
            panelBottom.RightToLeft = RightToLeft.Yes;
            panelBottom.Size = new Size(940, 50);
            panelBottom.TabIndex = 2;
            //
            // btnClose
            //
            btnClose.Anchor = AnchorStyles.Top;
            btnClose.Location = new Point(14, 11);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(100, 28);
            btnClose.TabIndex = 0;
            btnClose.Text = "סגור";
            btnClose.Click += btnClose_Click;
            //
            // btnMail
            //
            btnMail.Anchor = AnchorStyles.Top;
            btnMail.Location = new Point(124, 11);
            btnMail.Name = "btnMail";
            btnMail.Size = new Size(170, 28);
            btnMail.TabIndex = 1;
            btnMail.Text = "שלח דוח למנהלים";
            btnMail.Click += btnMail_Click;
            //
            // btnExcel
            //
            btnExcel.Anchor = AnchorStyles.Top;
            btnExcel.Location = new Point(304, 11);
            btnExcel.Name = "btnExcel";
            btnExcel.Size = new Size(150, 28);
            btnExcel.TabIndex = 2;
            btnExcel.Text = "ייצוא לאקסל";
            btnExcel.Click += btnExcel_Click;
            //
            // AdminForm
            //
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(940, 604);
            Controls.Add(gridChecks);
            Controls.Add(panelBottom);
            Controls.Add(panelTop);
            Name = "AdminForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            Text = "ניהול מערכת — בדיקת תקינות";
            Load += AdminForm_Load;
            ((System.ComponentModel.ISupportInitialize)panelTop).EndInit();
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)spinYear.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridChecks).EndInit();
            ((System.ComponentModel.ISupportInitialize)viewChecks).EndInit();
            ((System.ComponentModel.ISupportInitialize)riCheck).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelBottom).EndInit();
            panelBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelTop;
        private DevExpress.XtraEditors.LabelControl lblSummary;
        private DevExpress.XtraEditors.SimpleButton btnRefresh;
        private DevExpress.XtraEditors.LabelControl lblYear;
        private DevExpress.XtraEditors.SpinEdit spinYear;
        private DevExpress.XtraGrid.GridControl gridChecks;
        private DevExpress.XtraGrid.Views.Grid.GridView viewChecks;
        private DevExpress.XtraGrid.Columns.GridColumn colGroup;
        private DevExpress.XtraGrid.Columns.GridColumn colCheck;
        private DevExpress.XtraGrid.Columns.GridColumn colStatus;
        private DevExpress.XtraGrid.Columns.GridColumn colDetail;
        private DevExpress.XtraGrid.Columns.GridColumn colAction;
        private DevExpress.XtraEditors.PanelControl panelBottom;
        private DevExpress.XtraEditors.SimpleButton btnClose;
        private DevExpress.XtraEditors.SimpleButton btnMail;
        private DevExpress.XtraEditors.SimpleButton btnExcel;
        private DevExpress.XtraGrid.Columns.GridColumn colSelect;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit riCheck;
    }
}
