using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace MunicipalDiscount
{
    partial class MinimumWageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MinimumWageForm));
            grpDetails = new GroupControl();
            txtValidityDate = new TextEdit();
            lblValidityDate = new LabelControl();
            txtMinimumWage = new TextEdit();
            lblMinimumWage = new LabelControl();
            colID = new DevExpress.XtraGrid.Columns.GridColumn();
            colValidityDate = new DevExpress.XtraGrid.Columns.GridColumn();
            colMinimumWage = new DevExpress.XtraGrid.Columns.GridColumn();
            panelButtons = new PanelControl();
            flowButtons = new FlowLayoutPanel();
            btnSave = new SimpleButton();
            btnDelete = new SimpleButton();
            btnClose = new SimpleButton();
            grpList = new GroupControl();
            gridControl = new GridControl();
            gridView = new GridView();
            ((System.ComponentModel.ISupportInitialize)grpDetails).BeginInit();
            grpDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtValidityDate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtMinimumWage.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).BeginInit();
            panelButtons.SuspendLayout();
            flowButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grpList).BeginInit();
            grpList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView).BeginInit();
            SuspendLayout();
            // 
            // grpDetails
            // 
            grpDetails.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            grpDetails.AppearanceCaption.ForeColor = Color.FromArgb(42, 82, 152);
            grpDetails.AppearanceCaption.Options.UseFont = true;
            grpDetails.AppearanceCaption.Options.UseForeColor = true;
            grpDetails.AppearanceCaption.Options.UseTextOptions = true;
            grpDetails.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            grpDetails.Controls.Add(txtValidityDate);
            grpDetails.Controls.Add(lblValidityDate);
            grpDetails.Controls.Add(txtMinimumWage);
            grpDetails.Controls.Add(lblMinimumWage);
            grpDetails.Location = new Point(0, 0);
            grpDetails.Name = "grpDetails";
            grpDetails.RightToLeft = RightToLeft.Yes;
            grpDetails.ShowCaption = false;
            grpDetails.Size = new Size(707, 47);
            grpDetails.TabIndex = 0;
            // 
            // txtValidityDate
            // 
            txtValidityDate.Location = new Point(477, 12);
            txtValidityDate.Name = "txtValidityDate";
            txtValidityDate.Size = new Size(120, 22);
            txtValidityDate.TabIndex = 0;
            txtValidityDate.KeyDown += DetailField_KeyDown;
            // 
            // lblValidityDate
            // 
            lblValidityDate.Location = new Point(609, 14);
            lblValidityDate.Name = "lblValidityDate";
            lblValidityDate.Size = new Size(93, 16);
            lblValidityDate.TabIndex = 8;
            lblValidityDate.Text = "בתוקף מתאריך";
            // 
            // txtMinimumWage
            // 
            txtMinimumWage.Location = new Point(250, 9);
            txtMinimumWage.Name = "txtMinimumWage";
            txtMinimumWage.Properties.Mask.EditMask = "n2";
            txtMinimumWage.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            txtMinimumWage.Properties.Mask.UseMaskAsDisplayFormat = true;
            txtMinimumWage.RightToLeft = RightToLeft.Yes;
            txtMinimumWage.Size = new Size(100, 22);
            txtMinimumWage.TabIndex = 1;
            txtMinimumWage.KeyDown += DetailField_KeyDown;
            // 
            // lblMinimumWage
            // 
            lblMinimumWage.Location = new Point(367, 12);
            lblMinimumWage.Name = "lblMinimumWage";
            lblMinimumWage.Size = new Size(75, 16);
            lblMinimumWage.TabIndex = 2;
            lblMinimumWage.Text = "שכר מינימום";
            // 
            // colID
            // 
            colID.FieldName = "ID";
            colID.Name = "colID";
            // 
            // colValidityDate
            // 
            colValidityDate.Caption = "בתוקף מתאריך";
            colValidityDate.DisplayFormat.FormatString = "dd/MM/yy";
            colValidityDate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            colValidityDate.FieldName = "ValidityDate";
            colValidityDate.Name = "colValidityDate";
            colValidityDate.Visible = true;
            colValidityDate.VisibleIndex = 0;
            colValidityDate.Width = 120;
            // 
            // colMinimumWage
            // 
            colMinimumWage.Caption = "שכר מינימום";
            colMinimumWage.DisplayFormat.FormatString = "N2";
            colMinimumWage.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            colMinimumWage.FieldName = "MinimumWage";
            colMinimumWage.Name = "colMinimumWage";
            colMinimumWage.Visible = true;
            colMinimumWage.VisibleIndex = 1;
            colMinimumWage.Width = 140;
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(flowButtons);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 418);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(719, 55);
            panelButtons.TabIndex = 3;
            // 
            // flowButtons
            // 
            flowButtons.Controls.Add(btnSave);
            flowButtons.Controls.Add(btnDelete);
            flowButtons.Controls.Add(btnClose);
            flowButtons.Dock = DockStyle.Bottom;
            flowButtons.FlowDirection = FlowDirection.RightToLeft;
            flowButtons.Location = new Point(2, 0);
            flowButtons.Name = "flowButtons";
            flowButtons.Padding = new Padding(8, 6, 8, 6);
            flowButtons.Size = new Size(715, 53);
            flowButtons.TabIndex = 0;
            flowButtons.WrapContents = false;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(14, 9);
            btnSave.Margin = new Padding(6, 3, 6, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(145, 35);
            btnSave.TabIndex = 4;
            btnSave.Text = "שמור";
            btnSave.Click += btnSave_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(171, 9);
            btnDelete.Margin = new Padding(6, 3, 6, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(145, 35);
            btnDelete.TabIndex = 5;
            btnDelete.Text = "מחק";
            btnDelete.Click += btnDelete_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(328, 9);
            btnClose.Margin = new Padding(6, 3, 6, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(150, 35);
            btnClose.TabIndex = 6;
            btnClose.Text = "סגור";
            btnClose.Click += btnClose_Click;
            // 
            // grpList
            // 
            grpList.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            grpList.AppearanceCaption.ForeColor = Color.FromArgb(42, 82, 152);
            grpList.AppearanceCaption.Options.UseFont = true;
            grpList.AppearanceCaption.Options.UseForeColor = true;
            grpList.AppearanceCaption.Options.UseTextOptions = true;
            grpList.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            grpList.Controls.Add(gridControl);
            grpList.Location = new Point(0, 53);
            grpList.Name = "grpList";
            grpList.RightToLeft = RightToLeft.Yes;
            grpList.Size = new Size(714, 358);
            grpList.TabIndex = 8;
            grpList.Text = "רשימת שכר מינימום";
            // 
            // gridControl
            // 
            gridControl.Dock = DockStyle.Top;
            gridControl.Location = new Point(2, 28);
            gridControl.MainView = gridView;
            gridControl.Name = "gridControl";
            gridControl.Size = new Size(710, 330);
            gridControl.TabIndex = 0;
            gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView });
            gridControl.Click += gridControl_Click;
            // 
            // gridView
            // 
            gridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { colID, colValidityDate, colMinimumWage });
            gridView.GridControl = gridControl;
            gridView.Name = "gridView";
            gridView.OptionsBehavior.AutoPopulateColumns = false;
            gridView.OptionsBehavior.Editable = false;
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.FocusedRowChanged += gridView_FocusedRowChanged;
            // 
            // MinimumWageForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(719, 473);
            Controls.Add(grpList);
            Controls.Add(panelButtons);
            Controls.Add(grpDetails);
            IconOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("MinimumWageForm.IconOptions.SvgImage");
            MinimizeBox = false;
            MinimumSize = new Size(520, 340);
            Name = "MinimumWageForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            Text = "ניהול שכר מינימום";
            Load += MinimumWageForm_Load;
            ((System.ComponentModel.ISupportInitialize)grpDetails).EndInit();
            grpDetails.ResumeLayout(false);
            grpDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtValidityDate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtMinimumWage.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).EndInit();
            panelButtons.ResumeLayout(false);
            flowButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grpList).EndInit();
            grpList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.GroupControl grpDetails;
        private DevExpress.XtraEditors.LabelControl lblMinimumWage;
        private DevExpress.XtraEditors.TextEdit txtMinimumWage;
        private DevExpress.XtraEditors.PanelControl panelButtons;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private DevExpress.XtraEditors.LabelControl lblValidityDate;
        private DevExpress.XtraEditors.TextEdit txtValidityDate;
        private DevExpress.XtraGrid.Columns.GridColumn colID;
        private DevExpress.XtraGrid.Columns.GridColumn colValidityDate;
        private DevExpress.XtraGrid.Columns.GridColumn colMinimumWage;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnClose;
        private DevExpress.XtraEditors.GroupControl grpList;
        private DevExpress.XtraGrid.GridControl gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView;
    }
}