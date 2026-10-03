using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    partial class IncomeDiscountCriteriaForm
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
            GridLevelNode gridLevelNode1 = new GridLevelNode();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IncomeDiscountCriteriaForm));
            gridControl1 = new GridControl();
            gridView1 = new GridView();
            colCriteriaId = new DevExpress.XtraGrid.Columns.GridColumn();
            colNumberOfPeople = new DevExpress.XtraGrid.Columns.GridColumn();
            colFromAmount = new DevExpress.XtraGrid.Columns.GridColumn();
            colToAmount = new DevExpress.XtraGrid.Columns.GridColumn();
            colDiscountRate = new DevExpress.XtraGrid.Columns.GridColumn();
            colMunicipalDiscountCode = new DevExpress.XtraGrid.Columns.GridColumn();
            colGridValidityDate = new DevExpress.XtraGrid.Columns.GridColumn();
            colGridYear = new DevExpress.XtraGrid.Columns.GridColumn();
            lookupDiscountCode = new LookUpEdit();
            lblDiscountCode = new LabelControl();
            panelControl1 = new PanelControl();
            groupControlInputs = new GroupControl();
            txtValidityDate = new TextEdit();
            lblValidityDate = new LabelControl();
            spinYear = new SpinEdit();
            lblYear = new LabelControl();
            spinDiscountRate = new SpinEdit();
            lblDiscountRate = new LabelControl();
            spinToAmount = new SpinEdit();
            lblToAmount = new LabelControl();
            spinFromAmount = new SpinEdit();
            lblFromAmount = new LabelControl();
            spinNumberOfPeople = new SpinEdit();
            lblNumberOfPeople = new LabelControl();
            panelButtons = new PanelControl();
            flowButtons = new FlowLayoutPanel();
            btnSave = new SimpleButton();
            btnNew = new SimpleButton();
            btnDelete = new SimpleButton();
            btnRefresh = new SimpleButton();
            btnExportExcel = new SimpleButton();
            btnImportExcel = new SimpleButton();
            ((System.ComponentModel.ISupportInitialize)gridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lookupDiscountCode.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelControl1).BeginInit();
            panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupControlInputs).BeginInit();
            groupControlInputs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtValidityDate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinYear.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinDiscountRate.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinToAmount.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinFromAmount.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinNumberOfPeople.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).BeginInit();
            panelButtons.SuspendLayout();
            flowButtons.SuspendLayout();
            SuspendLayout();
            // 
            // gridControl1
            // 
            gridControl1.Dock = DockStyle.Fill;
            gridLevelNode1.RelationName = "Level1";
            gridControl1.LevelTree.Nodes.AddRange(new GridLevelNode[] { gridLevelNode1 });
            gridControl1.Location = new Point(0, 142);
            gridControl1.MainView = gridView1;
            gridControl1.Name = "gridControl1";
            gridControl1.Size = new Size(900, 376);
            gridControl1.TabIndex = 0;
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });
            // 
            // gridView1
            // 
            gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { colCriteriaId, colNumberOfPeople, colFromAmount, colToAmount, colDiscountRate, colMunicipalDiscountCode, colGridValidityDate, colGridYear });
            gridView1.GridControl = gridControl1;
            gridView1.Name = "gridView1";
            gridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
            gridView1.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
            gridView1.OptionsBehavior.AutoPopulateColumns = false;
            gridView1.OptionsNavigation.EnterMoveNextColumn = true;
            gridView1.OptionsView.EnableAppearanceOddRow = true;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.RowClick += gridView1_RowClick;
            gridView1.FocusedRowChanged += gridView1_FocusedRowChanged;
            gridView1.RowUpdated += gridView1_RowUpdated;
            // 
            // colCriteriaId
            // 
            colCriteriaId.AppearanceCell.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            colCriteriaId.AppearanceCell.Options.UseFont = true;
            colCriteriaId.AppearanceCell.Options.UseTextOptions = true;
            colCriteriaId.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            colCriteriaId.FieldName = "ID";
            colCriteriaId.Name = "colCriteriaId";
            colCriteriaId.OptionsColumn.AllowEdit = false;
            // 
            // colNumberOfPeople
            // 
            colNumberOfPeople.AppearanceCell.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            colNumberOfPeople.AppearanceCell.Options.UseFont = true;
            colNumberOfPeople.AppearanceCell.Options.UseTextOptions = true;
            colNumberOfPeople.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colNumberOfPeople.Caption = "מספר נפשות";
            colNumberOfPeople.FieldName = "NumberOfPeople";
            colNumberOfPeople.Name = "colNumberOfPeople";
            colNumberOfPeople.Visible = true;
            colNumberOfPeople.VisibleIndex = 1;
            colNumberOfPeople.Width = 90;
            // 
            // colFromAmount
            // 
            colFromAmount.AppearanceCell.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            colFromAmount.AppearanceCell.Options.UseFont = true;
            colFromAmount.AppearanceCell.Options.UseTextOptions = true;
            colFromAmount.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colFromAmount.Caption = "מסכום";
            colFromAmount.DisplayFormat.FormatString = "N0";
            colFromAmount.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            colFromAmount.FieldName = "FromAmount";
            colFromAmount.Name = "colFromAmount";
            colFromAmount.Visible = true;
            colFromAmount.VisibleIndex = 2;
            colFromAmount.Width = 110;
            // 
            // colToAmount
            // 
            colToAmount.AppearanceCell.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            colToAmount.AppearanceCell.Options.UseFont = true;
            colToAmount.AppearanceCell.Options.UseTextOptions = true;
            colToAmount.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colToAmount.Caption = "עד סכום";
            colToAmount.DisplayFormat.FormatString = "N0";
            colToAmount.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            colToAmount.FieldName = "ToAmount";
            colToAmount.Name = "colToAmount";
            colToAmount.Visible = true;
            colToAmount.VisibleIndex = 3;
            colToAmount.Width = 110;
            // 
            // colDiscountRate
            // 
            colDiscountRate.AppearanceCell.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            colDiscountRate.AppearanceCell.Options.UseFont = true;
            colDiscountRate.AppearanceCell.Options.UseTextOptions = true;
            colDiscountRate.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colDiscountRate.Caption = "אחוז הנחה";
            colDiscountRate.DisplayFormat.FormatString = "P0";
            colDiscountRate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            colDiscountRate.FieldName = "DiscountRate";
            colDiscountRate.Name = "colDiscountRate";
            colDiscountRate.Visible = true;
            colDiscountRate.VisibleIndex = 4;
            colDiscountRate.Width = 90;
            // 
            // colMunicipalDiscountCode
            // 
            colMunicipalDiscountCode.AppearanceCell.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            colMunicipalDiscountCode.AppearanceCell.Options.UseFont = true;
            colMunicipalDiscountCode.AppearanceCell.Options.UseTextOptions = true;
            colMunicipalDiscountCode.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colMunicipalDiscountCode.Caption = "קוד הנחה";
            colMunicipalDiscountCode.FieldName = "MunicipalDiscountCode";
            colMunicipalDiscountCode.Name = "colMunicipalDiscountCode";
            colMunicipalDiscountCode.Visible = true;
            colMunicipalDiscountCode.VisibleIndex = 0;
            colMunicipalDiscountCode.Width = 90;
            // 
            // colGridValidityDate
            // 
            colGridValidityDate.AppearanceCell.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            colGridValidityDate.AppearanceCell.Options.UseFont = true;
            colGridValidityDate.Caption = "בתוקף מתאריך";
            colGridValidityDate.DisplayFormat.FormatString = "dd/MM/yy";
            colGridValidityDate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            colGridValidityDate.FieldName = "ValidityDate";
            colGridValidityDate.Name = "colGridValidityDate";
            colGridValidityDate.Visible = true;
            colGridValidityDate.VisibleIndex = 5;
            colGridValidityDate.Width = 110;
            // 
            // colGridYear
            // 
            colGridYear.AppearanceCell.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            colGridYear.AppearanceCell.Options.UseFont = true;
            colGridYear.AppearanceCell.Options.UseTextOptions = true;
            colGridYear.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colGridYear.Caption = "שנה";
            colGridYear.FieldName = "Year_";
            colGridYear.Name = "colGridYear";
            colGridYear.Visible = true;
            colGridYear.VisibleIndex = 6;
            colGridYear.Width = 80;
            // 
            // lookupDiscountCode
            // 
            lookupDiscountCode.Anchor = AnchorStyles.Top;
            lookupDiscountCode.Location = new Point(545, 98);
            lookupDiscountCode.Name = "lookupDiscountCode";
            lookupDiscountCode.Properties.Appearance.Font = new Font("Tahoma", 9F);
            lookupDiscountCode.Properties.Appearance.Options.UseFont = true;
            lookupDiscountCode.Properties.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] { new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Display", "") });
            lookupDiscountCode.Properties.DisplayMember = "Display";
            lookupDiscountCode.Properties.NullText = "";
            lookupDiscountCode.Properties.ShowHeader = false;
            lookupDiscountCode.Properties.ValueMember = "MunicipalDiscountCode";
            lookupDiscountCode.Size = new Size(200, 24);
            lookupDiscountCode.TabIndex = 11;
            // 
            // lblDiscountCode
            // 
            lblDiscountCode.Anchor = AnchorStyles.Top;
            lblDiscountCode.Location = new Point(784, 101);
            lblDiscountCode.Name = "lblDiscountCode";
            lblDiscountCode.Size = new Size(63, 16);
            lblDiscountCode.TabIndex = 10;
            lblDiscountCode.Text = "קוד הנחה:";
            // 
            // panelControl1
            // 
            panelControl1.Controls.Add(groupControlInputs);
            panelControl1.Dock = DockStyle.Top;
            panelControl1.Location = new Point(0, 0);
            panelControl1.Name = "panelControl1";
            panelControl1.Size = new Size(900, 142);
            panelControl1.TabIndex = 1;
            // 
            // groupControlInputs
            // 
            groupControlInputs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupControlInputs.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
            groupControlInputs.AppearanceCaption.ForeColor = Color.FromArgb(42, 82, 152);
            groupControlInputs.AppearanceCaption.Options.UseFont = true;
            groupControlInputs.AppearanceCaption.Options.UseForeColor = true;
            groupControlInputs.Controls.Add(lookupDiscountCode);
            groupControlInputs.Controls.Add(lblDiscountCode);
            groupControlInputs.Controls.Add(txtValidityDate);
            groupControlInputs.Controls.Add(lblValidityDate);
            groupControlInputs.Controls.Add(spinYear);
            groupControlInputs.Controls.Add(lblYear);
            groupControlInputs.Controls.Add(spinDiscountRate);
            groupControlInputs.Controls.Add(lblDiscountRate);
            groupControlInputs.Controls.Add(spinToAmount);
            groupControlInputs.Controls.Add(lblToAmount);
            groupControlInputs.Controls.Add(spinFromAmount);
            groupControlInputs.Controls.Add(lblFromAmount);
            groupControlInputs.Controls.Add(spinNumberOfPeople);
            groupControlInputs.Controls.Add(lblNumberOfPeople);
            groupControlInputs.Location = new Point(2, 2);
            groupControlInputs.Name = "groupControlInputs";
            groupControlInputs.ShowCaption = false;
            groupControlInputs.Size = new Size(896, 137);
            groupControlInputs.TabIndex = 0;
            // 
            // txtValidityDate
            // 
            txtValidityDate.Anchor = AnchorStyles.Top;
            txtValidityDate.Location = new Point(191, 57);
            txtValidityDate.Name = "txtValidityDate";
            txtValidityDate.Properties.Appearance.Font = new Font("Tahoma", 9F);
            txtValidityDate.Properties.Appearance.Options.UseFont = true;
            txtValidityDate.Size = new Size(109, 24);
            txtValidityDate.TabIndex = 11;
            txtValidityDate.KeyDown += InputField_KeyDown;
            // 
            // lblValidityDate
            // 
            lblValidityDate.Anchor = AnchorStyles.Top;
            lblValidityDate.Location = new Point(313, 60);
            lblValidityDate.Name = "lblValidityDate";
            lblValidityDate.Size = new Size(93, 16);
            lblValidityDate.TabIndex = 10;
            lblValidityDate.Text = "בתוקף מתאריך";
            // 
            // spinYear
            // 
            spinYear.Anchor = AnchorStyles.Top;
            spinYear.EditValue = new decimal(new int[] { 2025, 0, 0, 0 });
            spinYear.Location = new Point(435, 57);
            spinYear.Name = "spinYear";
            spinYear.Properties.Appearance.Font = new Font("Tahoma", 9F);
            spinYear.Properties.Appearance.Options.UseFont = true;
            spinYear.Properties.Appearance.Options.UseTextOptions = true;
            spinYear.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            spinYear.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            spinYear.Properties.IsFloatValue = false;
            spinYear.Properties.Mask.UseMaskAsDisplayFormat = true;
            spinYear.Properties.MaskSettings.Set("mask", "n0");
            spinYear.Properties.MaxValue = new decimal(new int[] { 2100, 0, 0, 0 });
            spinYear.Properties.MinValue = new decimal(new int[] { 2000, 0, 0, 0 });
            spinYear.Size = new Size(110, 24);
            spinYear.TabIndex = 9;
            spinYear.Click += SpinEdit_Click;
            spinYear.Enter += SpinEdit_Enter;
            spinYear.KeyDown += InputField_KeyDown;
            // 
            // lblYear
            // 
            lblYear.Anchor = AnchorStyles.Top;
            lblYear.Location = new Point(585, 60);
            lblYear.Name = "lblYear";
            lblYear.Size = new Size(30, 16);
            lblYear.TabIndex = 8;
            lblYear.Text = "שנה:";
            // 
            // spinDiscountRate
            // 
            spinDiscountRate.Anchor = AnchorStyles.Top;
            spinDiscountRate.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            spinDiscountRate.Location = new Point(645, 57);
            spinDiscountRate.Name = "spinDiscountRate";
            spinDiscountRate.Properties.Appearance.Font = new Font("Tahoma", 9F);
            spinDiscountRate.Properties.Appearance.Options.UseFont = true;
            spinDiscountRate.Properties.Appearance.Options.UseTextOptions = true;
            spinDiscountRate.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            spinDiscountRate.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            spinDiscountRate.Properties.DisplayFormat.FormatString = "{0}%";
            spinDiscountRate.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            spinDiscountRate.Properties.IsFloatValue = false;
            spinDiscountRate.Properties.MaskSettings.Set("mask", "n0");
            spinDiscountRate.Properties.MaxValue = new decimal(new int[] { 100, 0, 0, 0 });
            spinDiscountRate.Size = new Size(100, 24);
            spinDiscountRate.TabIndex = 7;
            spinDiscountRate.Click += SpinEdit_Click;
            spinDiscountRate.Enter += SpinEdit_Enter;
            spinDiscountRate.KeyDown += InputField_KeyDown;
            // 
            // lblDiscountRate
            // 
            lblDiscountRate.Anchor = AnchorStyles.Top;
            lblDiscountRate.Location = new Point(784, 62);
            lblDiscountRate.Name = "lblDiscountRate";
            lblDiscountRate.Size = new Size(68, 16);
            lblDiscountRate.TabIndex = 6;
            lblDiscountRate.Text = "אחוז הנחה:";
            // 
            // spinToAmount
            // 
            spinToAmount.Anchor = AnchorStyles.Top;
            spinToAmount.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            spinToAmount.Location = new Point(191, 22);
            spinToAmount.Name = "spinToAmount";
            spinToAmount.Properties.Appearance.Font = new Font("Tahoma", 9F);
            spinToAmount.Properties.Appearance.Options.UseFont = true;
            spinToAmount.Properties.Appearance.Options.UseTextOptions = true;
            spinToAmount.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            spinToAmount.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            spinToAmount.Properties.IsFloatValue = false;
            spinToAmount.Properties.Mask.UseMaskAsDisplayFormat = true;
            spinToAmount.Properties.MaskSettings.Set("mask", "n0");
            spinToAmount.Properties.MaxValue = new decimal(new int[] { 10000000, 0, 0, 0 });
            spinToAmount.Size = new Size(110, 24);
            spinToAmount.TabIndex = 5;
            spinToAmount.Click += SpinEdit_Click;
            spinToAmount.Enter += SpinEdit_Enter;
            spinToAmount.KeyDown += InputField_KeyDown;
            // 
            // lblToAmount
            // 
            lblToAmount.Anchor = AnchorStyles.Top;
            lblToAmount.Location = new Point(348, 30);
            lblToAmount.Name = "lblToAmount";
            lblToAmount.Size = new Size(58, 16);
            lblToAmount.TabIndex = 4;
            lblToAmount.Text = "עד סכום:";
            // 
            // spinFromAmount
            // 
            spinFromAmount.Anchor = AnchorStyles.Top;
            spinFromAmount.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            spinFromAmount.Location = new Point(435, 22);
            spinFromAmount.Name = "spinFromAmount";
            spinFromAmount.Properties.Appearance.Font = new Font("Tahoma", 9F);
            spinFromAmount.Properties.Appearance.Options.UseFont = true;
            spinFromAmount.Properties.Appearance.Options.UseTextOptions = true;
            spinFromAmount.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            spinFromAmount.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            spinFromAmount.Properties.IsFloatValue = false;
            spinFromAmount.Properties.Mask.UseMaskAsDisplayFormat = true;
            spinFromAmount.Properties.MaskSettings.Set("mask", "n0");
            spinFromAmount.Properties.MaxValue = new decimal(new int[] { 10000000, 0, 0, 0 });
            spinFromAmount.Size = new Size(110, 24);
            spinFromAmount.TabIndex = 3;
            spinFromAmount.Click += SpinEdit_Click;
            spinFromAmount.Enter += SpinEdit_Enter;
            spinFromAmount.KeyDown += InputField_KeyDown;
            // 
            // lblFromAmount
            // 
            lblFromAmount.Anchor = AnchorStyles.Top;
            lblFromAmount.Location = new Point(570, 28);
            lblFromAmount.Name = "lblFromAmount";
            lblFromAmount.Size = new Size(45, 16);
            lblFromAmount.TabIndex = 2;
            lblFromAmount.Text = "מסכום:";
            // 
            // spinNumberOfPeople
            // 
            spinNumberOfPeople.Anchor = AnchorStyles.Top;
            spinNumberOfPeople.EditValue = new decimal(new int[] { 1, 0, 0, 0 });
            spinNumberOfPeople.Location = new Point(645, 22);
            spinNumberOfPeople.Name = "spinNumberOfPeople";
            spinNumberOfPeople.Properties.Appearance.Font = new Font("Tahoma", 9F);
            spinNumberOfPeople.Properties.Appearance.Options.UseFont = true;
            spinNumberOfPeople.Properties.Appearance.Options.UseTextOptions = true;
            spinNumberOfPeople.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            spinNumberOfPeople.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            spinNumberOfPeople.Properties.IsFloatValue = false;
            spinNumberOfPeople.Properties.Mask.UseMaskAsDisplayFormat = true;
            spinNumberOfPeople.Properties.MaskSettings.Set("mask", "n0");
            spinNumberOfPeople.Properties.MaxValue = new decimal(new int[] { 20, 0, 0, 0 });
            spinNumberOfPeople.Properties.MinValue = new decimal(new int[] { 1, 0, 0, 0 });
            spinNumberOfPeople.Size = new Size(100, 24);
            spinNumberOfPeople.TabIndex = 1;
            spinNumberOfPeople.Click += SpinEdit_Click;
            spinNumberOfPeople.Enter += SpinEdit_Enter;
            spinNumberOfPeople.KeyDown += InputField_KeyDown;
            // 
            // lblNumberOfPeople
            // 
            lblNumberOfPeople.Anchor = AnchorStyles.Top;
            lblNumberOfPeople.Location = new Point(768, 27);
            lblNumberOfPeople.Name = "lblNumberOfPeople";
            lblNumberOfPeople.Size = new Size(84, 16);
            lblNumberOfPeople.TabIndex = 0;
            lblNumberOfPeople.Text = "מספר נפשות:";
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(flowButtons);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 518);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(900, 62);
            panelButtons.TabIndex = 2;
            // 
            // flowButtons
            // 
            flowButtons.Controls.Add(btnSave);
            flowButtons.Controls.Add(btnNew);
            flowButtons.Controls.Add(btnDelete);
            flowButtons.Controls.Add(btnRefresh);
            flowButtons.Controls.Add(btnExportExcel);
            flowButtons.Controls.Add(btnImportExcel);
            flowButtons.Dock = DockStyle.Bottom;
            flowButtons.FlowDirection = FlowDirection.RightToLeft;
            flowButtons.Location = new Point(2, 3);
            flowButtons.Name = "flowButtons";
            flowButtons.Padding = new Padding(8, 6, 8, 6);
            flowButtons.Size = new Size(896, 57);
            flowButtons.TabIndex = 0;
            flowButtons.WrapContents = false;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(12, 9);
            btnSave.Margin = new Padding(4, 3, 4, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 35);
            btnSave.TabIndex = 10;
            btnSave.Text = "שמור";
            btnSave.Click += btnSave_Click;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(110, 9);
            btnNew.Margin = new Padding(4, 3, 4, 3);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(90, 35);
            btnNew.TabIndex = 12;
            btnNew.Text = "חדש";
            btnNew.Click += btnNew_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(208, 9);
            btnDelete.Margin = new Padding(4, 3, 4, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(90, 35);
            btnDelete.TabIndex = 11;
            btnDelete.Text = "מחק";
            btnDelete.Click += btnDelete_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(306, 9);
            btnRefresh.Margin = new Padding(4, 3, 4, 3);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(90, 35);
            btnRefresh.TabIndex = 13;
            btnRefresh.Text = "רענן";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnExportExcel
            // 
            btnExportExcel.Location = new Point(404, 9);
            btnExportExcel.Margin = new Padding(4, 3, 4, 3);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(120, 30);
            btnExportExcel.TabIndex = 4;
            btnExportExcel.Text = "ייצוא לאקסל";
            btnExportExcel.Click += btnExportExcel_Click;
            // 
            // btnImportExcel
            // 
            btnImportExcel.Location = new Point(532, 9);
            btnImportExcel.Margin = new Padding(4, 3, 4, 3);
            btnImportExcel.Name = "btnImportExcel";
            btnImportExcel.Size = new Size(120, 30);
            btnImportExcel.TabIndex = 5;
            btnImportExcel.Text = "קליטה מאקסל";
            btnImportExcel.Click += btnImportExcel_Click;
            // 
            // IncomeDiscountCriteriaForm
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 580);
            Controls.Add(gridControl1);
            Controls.Add(panelButtons);
            Controls.Add(panelControl1);
            IconOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("IncomeDiscountCriteriaForm.IconOptions.SvgImage");
            Name = "IncomeDiscountCriteriaForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            Text = "קריטריונים למבחן הכנסה";
            ((System.ComponentModel.ISupportInitialize)gridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)lookupDiscountCode.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelControl1).EndInit();
            panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupControlInputs).EndInit();
            groupControlInputs.ResumeLayout(false);
            groupControlInputs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtValidityDate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinYear.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinDiscountRate.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinToAmount.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinFromAmount.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinNumberOfPeople.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelButtons).EndInit();
            panelButtons.ResumeLayout(false);
            flowButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn colCriteriaId;
        private DevExpress.XtraGrid.Columns.GridColumn colNumberOfPeople;
        private DevExpress.XtraGrid.Columns.GridColumn colFromAmount;
        private DevExpress.XtraGrid.Columns.GridColumn colToAmount;
        private DevExpress.XtraGrid.Columns.GridColumn colDiscountRate;
        private DevExpress.XtraGrid.Columns.GridColumn colGridValidityDate;
        private DevExpress.XtraGrid.Columns.GridColumn colGridYear;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.PanelControl panelButtons;
        private DevExpress.XtraEditors.SimpleButton btnExportExcel;
        private DevExpress.XtraEditors.SimpleButton btnImportExcel;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private DevExpress.XtraEditors.LabelControl lblValidityDate;
        private DevExpress.XtraEditors.TextEdit txtValidityDate;
        private DevExpress.XtraEditors.GroupControl groupControlInputs;

        private DevExpress.XtraGrid.Columns.GridColumn colMunicipalDiscountCode;
        private DevExpress.XtraEditors.LookUpEdit lookupDiscountCode;
        private DevExpress.XtraEditors.LabelControl lblDiscountCode;

        // Labels
        private DevExpress.XtraEditors.LabelControl lblNumberOfPeople;
        private DevExpress.XtraEditors.LabelControl lblFromAmount;
        private DevExpress.XtraEditors.LabelControl lblToAmount;
        private DevExpress.XtraEditors.LabelControl lblDiscountRate;
        private DevExpress.XtraEditors.LabelControl lblYear;

        // SpinEdits
        private DevExpress.XtraEditors.SpinEdit spinNumberOfPeople;
        private DevExpress.XtraEditors.SpinEdit spinFromAmount;
        private DevExpress.XtraEditors.SpinEdit spinToAmount;
        private DevExpress.XtraEditors.SpinEdit spinDiscountRate;
        private DevExpress.XtraEditors.SpinEdit spinYear;

        // Buttons
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnRefresh;
        private SimpleButton btnNew;
    }
}