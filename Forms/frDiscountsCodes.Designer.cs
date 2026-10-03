using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace MunicipalDiscount
{
    partial class frDiscountsCodes
    {
        private System.ComponentModel.IContainer components = null;
        private DevExpress.XtraEditors.PanelControl panelRowCount;
        private DevExpress.XtraEditors.LabelControl lblRowCount;
        private DevExpress.XtraEditors.PanelControl panelButtons;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private DevExpress.XtraEditors.SimpleButton btnAdd;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnRefresh;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnClose;
        private System.Windows.Forms.SplitContainer splitCodes;
        private DevExpress.XtraEditors.GroupControl grpCodes;
        private DevExpress.XtraEditors.GroupControl grpRates;
        private DevExpress.XtraGrid.GridControl gridRates;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewRates;
        private DevExpress.XtraGrid.Columns.GridColumn colRateValidityDate;
        private DevExpress.XtraGrid.Columns.GridColumn colRateDiscountPercent;
        private DevExpress.XtraGrid.Columns.GridColumn colRatePreviousPercent;
        private DevExpress.XtraGrid.Columns.GridColumn colRateMaximumDiscount;
        private DevExpress.XtraGrid.Columns.GridColumn colRateMaximumArea;
        private DevExpress.XtraGrid.Columns.GridColumn colRateId;
        private DevExpress.XtraGrid.Columns.GridColumn colRateDiscountCodeId;
        private DevExpress.XtraEditors.Repository.RepositoryItemDateEdit repositoryItemRateDate;
        private DevExpress.XtraBars.BarButtonItem btnExportCodes;
        private DevExpress.XtraBars.BarButtonItem btnImportCodes;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox  repositoryItemComboCategories;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frDiscountsCodes));
            panelRowCount = new DevExpress.XtraEditors.PanelControl();
            lblRowCount = new DevExpress.XtraEditors.LabelControl();
            panelButtons = new DevExpress.XtraEditors.PanelControl();
            flowButtons = new FlowLayoutPanel();
            btnAdd = new DevExpress.XtraEditors.SimpleButton();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            btnRefresh = new DevExpress.XtraEditors.SimpleButton();
            btnDelete = new DevExpress.XtraEditors.SimpleButton();
            btnClose = new DevExpress.XtraEditors.SimpleButton();
            splitCodes = new SplitContainer();
            grpCodes = new DevExpress.XtraEditors.GroupControl();
            gridControl1 = new GridControl();
            gridView1 = new GridView();
            colIsSelected = new DevExpress.XtraGrid.Columns.GridColumn();
            repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            colID = new DevExpress.XtraGrid.Columns.GridColumn();
            colCategories = new DevExpress.XtraGrid.Columns.GridColumn();
            colMunicipalDiscountCode = new DevExpress.XtraGrid.Columns.GridColumn();
            repositoryItemComboMunicipalDiscountCode = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            colDiscountCode = new DevExpress.XtraGrid.Columns.GridColumn();
            repositoryItemComboLegallDiscountCode = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            colGroupCode = new DevExpress.XtraGrid.Columns.GridColumn();
            repositoryItemComboGroupCode = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            colDiscountName = new DevExpress.XtraGrid.Columns.GridColumn();
            colMandatory = new DevExpress.XtraGrid.Columns.GridColumn();
            colDocuments = new DevExpress.XtraGrid.Columns.GridColumn();
            grpRates = new DevExpress.XtraEditors.GroupControl();
            gridRates = new GridControl();
            gridViewRates = new GridView();
            colRateValidityDate = new DevExpress.XtraGrid.Columns.GridColumn();
            repositoryItemRateDate = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            colRateDiscountPercent = new DevExpress.XtraGrid.Columns.GridColumn();
            colRatePreviousPercent = new DevExpress.XtraGrid.Columns.GridColumn();
            colRateMaximumDiscount = new DevExpress.XtraGrid.Columns.GridColumn();
            colRateMaximumArea = new DevExpress.XtraGrid.Columns.GridColumn();
            colRateId = new DevExpress.XtraGrid.Columns.GridColumn();
            colRateDiscountCodeId = new DevExpress.XtraGrid.Columns.GridColumn();
            btnExportCodes = new DevExpress.XtraBars.BarButtonItem();
            btnImportCodes = new DevExpress.XtraBars.BarButtonItem();
            barManager1 = new DevExpress.XtraBars.BarManager(components);
            bar1 = new DevExpress.XtraBars.Bar();
            btnManageCategories = new DevExpress.XtraBars.BarButtonItem();
            btnCopyCodes = new DevExpress.XtraBars.BarButtonItem();
            btnDocuments = new DevExpress.XtraBars.BarButtonItem();
            btnRemark = new DevExpress.XtraBars.BarButtonItem();
            btnRules = new DevExpress.XtraBars.BarButtonItem();
            btnConditions = new DevExpress.XtraBars.BarButtonItem();
            btnLawInForce = new DevExpress.XtraBars.BarButtonItem();
            btnExportExcel = new DevExpress.XtraBars.BarButtonItem();
            barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            barEditYear = new DevExpress.XtraBars.BarEditItem();
            repositoryItemComboYear = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            barListItem1 = new DevExpress.XtraBars.BarListItem();
            ((System.ComponentModel.ISupportInitialize)panelRowCount).BeginInit();
            panelRowCount.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)panelButtons).BeginInit();
            panelButtons.SuspendLayout();
            flowButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitCodes).BeginInit();
            splitCodes.Panel1.SuspendLayout();
            splitCodes.Panel2.SuspendLayout();
            splitCodes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grpCodes).BeginInit();
            grpCodes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemCheckEdit1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemComboMunicipalDiscountCode).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemComboLegallDiscountCode).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemComboGroupCode).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grpRates).BeginInit();
            grpRates.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridRates).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridViewRates).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemRateDate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemRateDate.CalendarTimeProperties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)barManager1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemComboYear).BeginInit();
            SuspendLayout();
            // 
            // panelRowCount
            // 
            panelRowCount.Controls.Add(lblRowCount);
            panelRowCount.Dock = DockStyle.Bottom;
            panelRowCount.Location = new Point(2, 426);
            panelRowCount.Name = "panelRowCount";
            panelRowCount.Size = new Size(857, 24);
            panelRowCount.TabIndex = 1;
            // 
            // lblRowCount
            // 
            lblRowCount.Location = new Point(10, 4);
            lblRowCount.Name = "lblRowCount";
            lblRowCount.Size = new Size(89, 16);
            lblRowCount.TabIndex = 0;
            lblRowCount.Text = "סה\"כ שורות: 0";
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(flowButtons);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 481);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(1398, 44);
            panelButtons.TabIndex = 1;
            // 
            // flowButtons
            // 
            flowButtons.Controls.Add(btnAdd);
            flowButtons.Controls.Add(btnSave);
            flowButtons.Controls.Add(btnRefresh);
            flowButtons.Controls.Add(btnDelete);
            flowButtons.Controls.Add(btnClose);
            flowButtons.Dock = DockStyle.Fill;
            flowButtons.FlowDirection = FlowDirection.RightToLeft;
            flowButtons.Location = new Point(2, 2);
            flowButtons.Name = "flowButtons";
            flowButtons.Padding = new Padding(8, 6, 8, 6);
            flowButtons.RightToLeft = RightToLeft.No;
            flowButtons.Size = new Size(1394, 40);
            flowButtons.TabIndex = 0;
            flowButtons.WrapContents = false;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(1274, 9);
            btnAdd.Margin = new Padding(4, 3, 4, 3);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(100, 26);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "הוסף";
            btnAdd.Click += btnAdd_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(1166, 9);
            btnSave.Margin = new Padding(4, 3, 4, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 26);
            btnSave.TabIndex = 1;
            btnSave.Text = "עדכן";
            btnSave.Click += btnSave_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(1058, 9);
            btnRefresh.Margin = new Padding(4, 3, 4, 3);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(100, 26);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "רענן";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(900, 9);
            btnDelete.Margin = new Padding(4, 3, 4, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(150, 26);
            btnDelete.TabIndex = 3;
            btnDelete.Text = "מחק שורות שסומנו";
            btnDelete.Click += btnDelete_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(792, 9);
            btnClose.Margin = new Padding(4, 3, 4, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(100, 26);
            btnClose.TabIndex = 4;
            btnClose.Text = "סגור";
            btnClose.Click += btnClose_Click;
            // 
            // splitCodes
            // 
            splitCodes.Dock = DockStyle.Fill;
            splitCodes.Location = new Point(0, 29);
            splitCodes.Name = "splitCodes";
            // 
            // splitCodes.Panel1
            // 
            splitCodes.Panel1.Controls.Add(grpCodes);
            splitCodes.Panel1.RightToLeft = RightToLeft.Yes;
            splitCodes.Panel1MinSize = 320;
            // 
            // splitCodes.Panel2
            // 
            splitCodes.Panel2.Controls.Add(grpRates);
            splitCodes.Panel2.RightToLeft = RightToLeft.Yes;
            splitCodes.Panel2MinSize = 260;
            splitCodes.RightToLeft = RightToLeft.Yes;
            splitCodes.Size = new Size(1398, 452);
            splitCodes.SplitterDistance = 861;
            splitCodes.SplitterWidth = 6;
            splitCodes.TabIndex = 0;
            // 
            // grpCodes
            // 
            grpCodes.Controls.Add(gridControl1);
            grpCodes.Controls.Add(panelRowCount);
            grpCodes.Dock = DockStyle.Fill;
            grpCodes.Location = new Point(0, 0);
            grpCodes.Name = "grpCodes";
            grpCodes.Size = new Size(861, 452);
            grpCodes.TabIndex = 0;
            grpCodes.Text = "קודי הנחה";
            // 
            // gridControl1
            // 
            gridControl1.Dock = DockStyle.Fill;
            gridControl1.Location = new Point(2, 28);
            gridControl1.MainView = gridView1;
            gridControl1.Name = "gridControl1";
            gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { repositoryItemCheckEdit1, repositoryItemComboGroupCode, repositoryItemComboLegallDiscountCode, repositoryItemComboMunicipalDiscountCode });
            gridControl1.Size = new Size(857, 398);
            gridControl1.TabIndex = 0;
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });
            // 
            // gridView1
            // 
            gridView1.Appearance.HeaderPanel.Font = new Font("Tahoma", 9F);
            gridView1.Appearance.HeaderPanel.Options.UseFont = true;
            gridView1.Appearance.HeaderPanel.Options.UseTextOptions = true;
            gridView1.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            gridView1.Appearance.Row.Options.UseTextOptions = true;
            gridView1.Appearance.Row.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { colIsSelected, colID, colCategories, colMunicipalDiscountCode, colDiscountCode, colGroupCode, colDiscountName, colMandatory, colDocuments });
            gridView1.GridControl = gridControl1;
            gridView1.Name = "gridView1";
            gridView1.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.True;
            gridView1.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.Click;
            gridView1.OptionsView.NewItemRowPosition = NewItemRowPosition.Top;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.RowStyle += gridView1_RowStyle;
            gridView1.FocusedRowChanged += gridView1_FocusedRowChanged;
            gridView1.CellValueChanged += gridView1_CellValueChanged;
            // 
            // colIsSelected
            // 
            colIsSelected.Caption = "בחר";
            colIsSelected.ColumnEdit = repositoryItemCheckEdit1;
            colIsSelected.FieldName = "IsSelected";
            colIsSelected.MinWidth = 25;
            colIsSelected.Name = "colIsSelected";
            colIsSelected.OptionsEditForm.Visible = DevExpress.Utils.DefaultBoolean.False;
            colIsSelected.Visible = true;
            colIsSelected.VisibleIndex = 0;
            colIsSelected.Width = 49;
            // 
            // repositoryItemCheckEdit1
            // 
            repositoryItemCheckEdit1.AutoHeight = false;
            repositoryItemCheckEdit1.Name = "repositoryItemCheckEdit1";
            repositoryItemCheckEdit1.NullStyle = DevExpress.XtraEditors.Controls.StyleIndeterminate.Unchecked;
            // 
            // colID
            // 
            colID.Caption = "ID";
            colID.FieldName = "ID";
            colID.MinWidth = 25;
            colID.Name = "colID";
            colID.OptionsColumn.ReadOnly = true;
            // 
            // colCategories
            // 
            colCategories.Caption = "קטגוריה";
            colCategories.FieldName = "Categories";
            colCategories.MinWidth = 25;
            colCategories.Name = "colCategories";
            colCategories.Visible = true;
            colCategories.VisibleIndex = 1;
            colCategories.Width = 119;
            // 
            // colMunicipalDiscountCode
            // 
            colMunicipalDiscountCode.Caption = "קוד ת.גביה";
            colMunicipalDiscountCode.ColumnEdit = repositoryItemComboMunicipalDiscountCode;
            colMunicipalDiscountCode.FieldName = "MunicipalDiscountCode";
            colMunicipalDiscountCode.MinWidth = 25;
            colMunicipalDiscountCode.Name = "colMunicipalDiscountCode";
            colMunicipalDiscountCode.OptionsFilter.FilterPopupMode = DevExpress.XtraGrid.Columns.FilterPopupMode.CheckedList;
            colMunicipalDiscountCode.Visible = true;
            colMunicipalDiscountCode.VisibleIndex = 3;
            colMunicipalDiscountCode.Width = 87;
            // 
            // repositoryItemComboMunicipalDiscountCode
            // 
            repositoryItemComboMunicipalDiscountCode.AutoHeight = false;
            repositoryItemComboMunicipalDiscountCode.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            repositoryItemComboMunicipalDiscountCode.Name = "repositoryItemComboMunicipalDiscountCode";
            // 
            // colDiscountCode
            // 
            colDiscountCode.Caption = "קוד הנחה";
            colDiscountCode.ColumnEdit = repositoryItemComboLegallDiscountCode;
            colDiscountCode.FieldName = "LegallDiscountCode";
            colDiscountCode.MinWidth = 25;
            colDiscountCode.Name = "colDiscountCode";
            colDiscountCode.OptionsFilter.FilterPopupMode = DevExpress.XtraGrid.Columns.FilterPopupMode.CheckedList;
            colDiscountCode.Visible = true;
            colDiscountCode.VisibleIndex = 4;
            colDiscountCode.Width = 72;
            // 
            // repositoryItemComboLegallDiscountCode
            // 
            repositoryItemComboLegallDiscountCode.AutoHeight = false;
            repositoryItemComboLegallDiscountCode.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            repositoryItemComboLegallDiscountCode.Name = "repositoryItemComboLegallDiscountCode";
            // 
            // colGroupCode
            // 
            colGroupCode.Caption = "קוד קבוצה";
            colGroupCode.ColumnEdit = repositoryItemComboGroupCode;
            colGroupCode.FieldName = "GroupCode";
            colGroupCode.MinWidth = 25;
            colGroupCode.Name = "colGroupCode";
            colGroupCode.OptionsFilter.FilterPopupMode = DevExpress.XtraGrid.Columns.FilterPopupMode.CheckedList;
            colGroupCode.Visible = true;
            colGroupCode.VisibleIndex = 2;
            colGroupCode.Width = 86;
            // 
            // repositoryItemComboGroupCode
            // 
            repositoryItemComboGroupCode.AutoHeight = false;
            repositoryItemComboGroupCode.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            repositoryItemComboGroupCode.Name = "repositoryItemComboGroupCode";
            // 
            // colDiscountName
            // 
            colDiscountName.Caption = "תאור הנחה";
            colDiscountName.FieldName = "DiscountName";
            colDiscountName.MinWidth = 25;
            colDiscountName.Name = "colDiscountName";
            colDiscountName.Visible = true;
            colDiscountName.VisibleIndex = 5;
            colDiscountName.Width = 194;
            // 
            // colMandatory
            // 
            colMandatory.Caption = "חובה/רשות";
            colMandatory.FieldName = "Mandatory";
            colMandatory.MinWidth = 25;
            colMandatory.Name = "colMandatory";
            colMandatory.Visible = true;
            colMandatory.VisibleIndex = 6;
            colMandatory.Width = 77;
            // 
            // colDocuments
            // 
            colDocuments.Caption = "מסמכים נדרשים";
            colDocuments.FieldName = "Documents";
            colDocuments.MinWidth = 25;
            colDocuments.Name = "colDocuments";
            // 
            // grpRates
            // 
            grpRates.Controls.Add(gridRates);
            grpRates.Dock = DockStyle.Fill;
            grpRates.Location = new Point(0, 0);
            grpRates.Name = "grpRates";
            grpRates.Size = new Size(531, 452);
            grpRates.TabIndex = 0;
            grpRates.Text = "גרסאות תעריף";
            // 
            // gridRates
            // 
            gridRates.Dock = DockStyle.Fill;
            gridRates.Location = new Point(2, 28);
            gridRates.MainView = gridViewRates;
            gridRates.Name = "gridRates";
            gridRates.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { repositoryItemRateDate });
            gridRates.Size = new Size(527, 422);
            gridRates.TabIndex = 0;
            gridRates.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridViewRates });
            // 
            // gridViewRates
            // 
            gridViewRates.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { colRateValidityDate, colRateDiscountPercent, colRatePreviousPercent, colRateMaximumDiscount, colRateMaximumArea, colRateId, colRateDiscountCodeId });
            gridViewRates.GridControl = gridRates;
            gridViewRates.Name = "gridViewRates";
            gridViewRates.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.True;
            gridViewRates.OptionsNavigation.AutoFocusNewRow = true;
            gridViewRates.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
            gridViewRates.OptionsView.ShowGroupPanel = false;
            gridViewRates.InitNewRow += gridViewRates_InitNewRow;
            gridViewRates.CellValueChanged += gridViewRates_CellValueChanged;
            gridViewRates.RowDeleted += gridViewRates_RowDeleted;
            // 
            // colRateValidityDate
            // 
            colRateValidityDate.Caption = "בתוקף מתאריך";
            colRateValidityDate.ColumnEdit = repositoryItemRateDate;
            colRateValidityDate.FieldName = "ValidityDate";
            colRateValidityDate.Name = "colRateValidityDate";
            colRateValidityDate.Visible = true;
            colRateValidityDate.VisibleIndex = 0;
            colRateValidityDate.Width = 110;
            // 
            // repositoryItemRateDate
            // 
            repositoryItemRateDate.AutoHeight = false;
            repositoryItemRateDate.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            repositoryItemRateDate.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            repositoryItemRateDate.DisplayFormat.FormatString = "dd/MM/yyyy";
            repositoryItemRateDate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            repositoryItemRateDate.EditFormat.FormatString = "dd/MM/yyyy";
            repositoryItemRateDate.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            repositoryItemRateDate.Mask.UseMaskAsDisplayFormat = true;
            repositoryItemRateDate.Name = "repositoryItemRateDate";
            // 
            // colRateDiscountPercent
            // 
            colRateDiscountPercent.AppearanceCell.Options.UseTextOptions = true;
            colRateDiscountPercent.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colRateDiscountPercent.Caption = "אחוז הנחה";
            colRateDiscountPercent.DisplayFormat.FormatString = "P0";
            colRateDiscountPercent.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            colRateDiscountPercent.FieldName = "DiscountPercent";
            colRateDiscountPercent.Name = "colRateDiscountPercent";
            colRateDiscountPercent.Visible = true;
            colRateDiscountPercent.VisibleIndex = 1;
            colRateDiscountPercent.Width = 90;
            // 
            // colRatePreviousPercent
            // 
            colRatePreviousPercent.AppearanceCell.Options.UseTextOptions = true;
            colRatePreviousPercent.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colRatePreviousPercent.Caption = "אחוז הנחה קודם";
            colRatePreviousPercent.DisplayFormat.FormatString = "P0";
            colRatePreviousPercent.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            colRatePreviousPercent.FieldName = "PreviousDiscountPercent";
            colRatePreviousPercent.Name = "colRatePreviousPercent";
            colRatePreviousPercent.Visible = true;
            colRatePreviousPercent.VisibleIndex = 2;
            colRatePreviousPercent.Width = 110;
            // 
            // colRateMaximumDiscount
            // 
            colRateMaximumDiscount.AppearanceCell.Options.UseTextOptions = true;
            colRateMaximumDiscount.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colRateMaximumDiscount.Caption = "מקסימום הנחה";
            colRateMaximumDiscount.DisplayFormat.FormatString = "P0";
            colRateMaximumDiscount.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            colRateMaximumDiscount.FieldName = "MaximumDiscount";
            colRateMaximumDiscount.Name = "colRateMaximumDiscount";
            colRateMaximumDiscount.Visible = true;
            colRateMaximumDiscount.VisibleIndex = 3;
            colRateMaximumDiscount.Width = 100;
            // 
            // colRateMaximumArea
            // 
            colRateMaximumArea.AppearanceCell.Options.UseTextOptions = true;
            colRateMaximumArea.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colRateMaximumArea.Caption = "מקסימום שטח";
            colRateMaximumArea.DisplayFormat.FormatString = "N2";
            colRateMaximumArea.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            colRateMaximumArea.FieldName = "MaximumArea";
            colRateMaximumArea.Name = "colRateMaximumArea";
            colRateMaximumArea.Visible = true;
            colRateMaximumArea.VisibleIndex = 4;
            colRateMaximumArea.Width = 100;
            // 
            // colRateId
            // 
            colRateId.FieldName = "ID";
            colRateId.Name = "colRateId";
            // 
            // colRateDiscountCodeId
            // 
            colRateDiscountCodeId.FieldName = "DiscountCodeId";
            colRateDiscountCodeId.Name = "colRateDiscountCodeId";
            // 
            // btnExportCodes
            // 
            btnExportCodes.Caption = "יצוא תעריפים לאקסל";
            btnExportCodes.Id = 13;
            btnExportCodes.Name = "btnExportCodes";
            btnExportCodes.ItemClick += btnExportCodesToExcel_ItemClick;
            // 
            // btnImportCodes
            // 
            btnImportCodes.Caption = "קליטת קודים ותעריפים מאקסל";
            btnImportCodes.Id = 14;
            btnImportCodes.Name = "btnImportCodes";
            btnImportCodes.ItemClick += btnImportCodesFromExcel_ItemClick;
            // 
            // barManager1
            // 
            barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] { bar1 });
            barManager1.DockControls.Add(barDockControlTop);
            barManager1.DockControls.Add(barDockControlBottom);
            barManager1.DockControls.Add(barDockControlLeft);
            barManager1.DockControls.Add(barDockControlRight);
            barManager1.Form = this;
            barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] { btnManageCategories, btnDocuments, btnRemark, btnRules, btnConditions, btnLawInForce, barEditYear, btnExportExcel, btnCopyCodes, btnExportCodes, btnImportCodes, barListItem1 });
            barManager1.MaxItemId = 17;
            barManager1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { repositoryItemComboYear });
            // 
            // bar1
            // 
            bar1.BarName = "Tools";
            bar1.DockCol = 0;
            bar1.DockRow = 0;
            bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] { new DevExpress.XtraBars.LinkPersistInfo(btnManageCategories, true), new DevExpress.XtraBars.LinkPersistInfo(btnCopyCodes, true), new DevExpress.XtraBars.LinkPersistInfo(btnDocuments, true), new DevExpress.XtraBars.LinkPersistInfo(btnRemark, true), new DevExpress.XtraBars.LinkPersistInfo(btnRules, true), new DevExpress.XtraBars.LinkPersistInfo(btnConditions, true), new DevExpress.XtraBars.LinkPersistInfo(btnLawInForce, true), new DevExpress.XtraBars.LinkPersistInfo(btnExportExcel, true), new DevExpress.XtraBars.LinkPersistInfo(btnExportCodes, true), new DevExpress.XtraBars.LinkPersistInfo(btnImportCodes, true) });
            bar1.Text = "Tools";
            // 
            // btnManageCategories
            // 
            btnManageCategories.Caption = "ניהול קטגוריות";
            btnManageCategories.Id = 9;
            btnManageCategories.Name = "btnManageCategories";
            btnManageCategories.ItemClick += btnManageCategories_ItemClick;
            // 
            // btnCopyCodes
            // 
            btnCopyCodes.Caption = "העתקת קודי הנחה";
            btnCopyCodes.Id = 12;
            btnCopyCodes.Name = "btnCopyCodes";
            btnCopyCodes.ItemClick += btnCopyCodes_ItemClick;
            // 
            // btnDocuments
            // 
            btnDocuments.Caption = "מסמכים נדרשים";
            btnDocuments.Id = 8;
            btnDocuments.Name = "btnDocuments";
            btnDocuments.ItemClick += btnDocuments_ItemClick;
            // 
            // btnRemark
            // 
            btnRemark.Caption = "הסברים";
            btnRemark.Id = 5;
            btnRemark.Name = "btnRemark";
            btnRemark.ItemClick += btnRemark_ItemClick;
            // 
            // btnRules
            // 
            btnRules.Caption = "כללים";
            btnRules.Id = 6;
            btnRules.Name = "btnRules";
            btnRules.ItemClick += btnRules_ItemClick;
            // 
            // btnConditions
            // 
            btnConditions.Caption = "תנאים";
            btnConditions.Id = 7;
            btnConditions.Name = "btnConditions";
            btnConditions.ItemClick += btnConditions_ItemClick;
            // 
            // btnLawInForce
            // 
            btnLawInForce.Caption = "מקור החוק";
            btnLawInForce.Id = 16;
            btnLawInForce.Name = "btnLawInForce";
            btnLawInForce.ItemClick += btnLawInForce_ItemClick;
            // 
            // btnExportExcel
            // 
            btnExportExcel.Caption = "יצוא קודים לאקסל";
            btnExportExcel.Id = 11;
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.ItemClick += btnExportExcel_ItemClick;
            // 
            // barDockControlTop
            // 
            barDockControlTop.CausesValidation = false;
            barDockControlTop.Dock = DockStyle.Top;
            barDockControlTop.Location = new Point(0, 0);
            barDockControlTop.Manager = barManager1;
            barDockControlTop.Size = new Size(1398, 29);
            // 
            // barDockControlBottom
            // 
            barDockControlBottom.CausesValidation = false;
            barDockControlBottom.Dock = DockStyle.Bottom;
            barDockControlBottom.Location = new Point(0, 525);
            barDockControlBottom.Manager = barManager1;
            barDockControlBottom.Size = new Size(1398, 0);
            // 
            // barDockControlLeft
            // 
            barDockControlLeft.CausesValidation = false;
            barDockControlLeft.Dock = DockStyle.Left;
            barDockControlLeft.Location = new Point(0, 29);
            barDockControlLeft.Manager = barManager1;
            barDockControlLeft.Size = new Size(0, 496);
            // 
            // barDockControlRight
            // 
            barDockControlRight.CausesValidation = false;
            barDockControlRight.Dock = DockStyle.Right;
            barDockControlRight.Location = new Point(1398, 29);
            barDockControlRight.Manager = barManager1;
            barDockControlRight.Size = new Size(0, 496);
            // 
            // barEditYear
            // 
            barEditYear.Edit = repositoryItemComboYear;
            barEditYear.EditWidth = 60;
            barEditYear.Id = 11;
            barEditYear.Name = "barEditYear";
            barEditYear.EditValueChanged += barEditYear_EditValueChanged;
            // 
            // repositoryItemComboYear
            // 
            repositoryItemComboYear.AutoHeight = false;
            repositoryItemComboYear.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            repositoryItemComboYear.Name = "repositoryItemComboYear";
            repositoryItemComboYear.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // barListItem1
            // 
            barListItem1.Caption = "barListItem1";
            barListItem1.Id = 15;
            barListItem1.Name = "barListItem1";
            // 
            // frDiscountsCodes
            // 
            AutoScaleDimensions = new SizeF(7F, 16F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1398, 525);
            Controls.Add(splitCodes);
            Controls.Add(panelButtons);
            Controls.Add(barDockControlLeft);
            Controls.Add(barDockControlRight);
            Controls.Add(barDockControlBottom);
            Controls.Add(barDockControlTop);
            IconOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("frDiscountsCodes.IconOptions.SvgImage");
            MinimumSize = new Size(600, 400);
            Name = "frDiscountsCodes";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "קודי הנחות";
            ((System.ComponentModel.ISupportInitialize)panelRowCount).EndInit();
            panelRowCount.ResumeLayout(false);
            panelRowCount.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)panelButtons).EndInit();
            panelButtons.ResumeLayout(false);
            flowButtons.ResumeLayout(false);
            splitCodes.Panel1.ResumeLayout(false);
            splitCodes.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitCodes).EndInit();
            splitCodes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grpCodes).EndInit();
            grpCodes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemCheckEdit1).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemComboMunicipalDiscountCode).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemComboLegallDiscountCode).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemComboGroupCode).EndInit();
            ((System.ComponentModel.ISupportInitialize)grpRates).EndInit();
            grpRates.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridRates).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridViewRates).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemRateDate.CalendarTimeProperties).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemRateDate).EndInit();
            ((System.ComponentModel.ISupportInitialize)barManager1).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemComboYear).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DevExpress.XtraBars.BarManager        barManager1;
        private DevExpress.XtraBars.Bar               bar1;
        private DevExpress.XtraBars.BarDockControl    barDockControlTop;
        private DevExpress.XtraBars.BarDockControl    barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl    barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl    barDockControlRight;
        private DevExpress.XtraBars.BarButtonItem     btnDocuments;
        private DevExpress.XtraBars.BarButtonItem     btnManageCategories;
        private DevExpress.XtraBars.BarButtonItem     btnRemark;
        private DevExpress.XtraBars.BarButtonItem     btnRules;
        private DevExpress.XtraBars.BarButtonItem     btnConditions;
        private DevExpress.XtraBars.BarButtonItem     btnLawInForce;
        private DevExpress.XtraBars.BarButtonItem     btnExportExcel;
        private DevExpress.XtraBars.BarButtonItem     btnCopyCodes;
        private DevExpress.XtraBars.BarEditItem       barEditYear;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboYear;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboGroupCode;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboLegallDiscountCode;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboMunicipalDiscountCode;
        private DevExpress.XtraGrid.Columns.GridColumn colIsSelected;
        private DevExpress.XtraGrid.Columns.GridColumn colID;
        private DevExpress.XtraGrid.Columns.GridColumn colCategories;
        private DevExpress.XtraGrid.Columns.GridColumn colMunicipalDiscountCode;
        private DevExpress.XtraGrid.Columns.GridColumn colDiscountCode;
        private DevExpress.XtraGrid.Columns.GridColumn colGroupCode;
        private DevExpress.XtraGrid.Columns.GridColumn colDiscountName;
        private DevExpress.XtraGrid.Columns.GridColumn colMandatory;
        private DevExpress.XtraGrid.Columns.GridColumn colDocuments;
        private DevExpress.XtraBars.BarListItem barListItem1;
    }
}
