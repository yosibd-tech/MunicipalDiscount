namespace MunicipalDiscount
{
    partial class RequestDateForm
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
            splitMain = new SplitContainer();
            gridControl = new DevExpress.XtraGrid.GridControl();
            gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            colArea = new DevExpress.XtraGrid.Columns.GridColumn();
            colClientNumber = new DevExpress.XtraGrid.Columns.GridColumn();
            colClientName = new DevExpress.XtraGrid.Columns.GridColumn();
            colRequestDate = new DevExpress.XtraGrid.Columns.GridColumn();
            repositoryItemRequestDate = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            panelFind = new DevExpress.XtraEditors.PanelControl();
            txtFindClient = new DevExpress.XtraEditors.TextEdit();
            lblFind = new DevExpress.XtraEditors.LabelControl();
            grpSaved = new DevExpress.XtraEditors.GroupControl();
            gridSaved = new DevExpress.XtraGrid.GridControl();
            gridViewSaved = new DevExpress.XtraGrid.Views.Grid.GridView();
            colSavedArea = new DevExpress.XtraGrid.Columns.GridColumn();
            colSavedClientNumber = new DevExpress.XtraGrid.Columns.GridColumn();
            colSavedClientName = new DevExpress.XtraGrid.Columns.GridColumn();
            colSavedRequestDate = new DevExpress.XtraGrid.Columns.GridColumn();
            repositoryItemSavedDate = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            panelBottom = new DevExpress.XtraEditors.PanelControl();
            lblCount = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemRequestDate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelFind).BeginInit();
            panelFind.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtFindClient.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grpSaved).BeginInit();
            grpSaved.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridSaved).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridViewSaved).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemSavedDate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelBottom).BeginInit();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.Location = new Point(0, 0);
            splitMain.Name = "splitMain";
            splitMain.Orientation = Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(gridControl);
            splitMain.Panel1.Controls.Add(panelFind);
            splitMain.Panel1.RightToLeft = RightToLeft.Yes;
            splitMain.Panel1MinSize = 150;
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(grpSaved);
            splitMain.Panel2.RightToLeft = RightToLeft.Yes;
            splitMain.Panel2MinSize = 120;
            splitMain.Size = new Size(900, 556);
            splitMain.SplitterDistance = 340;
            splitMain.SplitterWidth = 6;
            splitMain.TabIndex = 0;
            // 
            // gridControl
            // 
            gridControl.Dock = DockStyle.Fill;
            gridControl.Location = new Point(0, 38);
            gridControl.MainView = gridView;
            gridControl.Name = "gridControl";
            gridControl.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { repositoryItemRequestDate });
            gridControl.Size = new Size(900, 302);
            gridControl.TabIndex = 0;
            gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView });
            gridControl.ProcessGridKey += gridControl_ProcessGridKey;
            // 
            // gridView
            // 
            gridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { colArea, colClientNumber, colClientName, colRequestDate });
            gridView.GridControl = gridControl;
            gridView.Name = "gridView";
            gridView.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
            gridView.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
            gridView.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] { new DevExpress.XtraGrid.Columns.GridColumnSortInfo(colArea, DevExpress.Data.ColumnSortOrder.Descending) });
            gridView.FocusedRowChanged += gridView_FocusedRowChanged;
            gridView.CellValueChanged += gridView_CellValueChanged;
            gridView.KeyDown += gridView_KeyDown;
            gridView.ValidatingEditor += gridView_ValidatingEditor;
            // 
            // colArea
            // 
            colArea.AppearanceCell.Options.UseTextOptions = true;
            colArea.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colArea.Caption = "אזור";
            colArea.FieldName = "Area";
            colArea.Name = "colArea";
            colArea.OptionsColumn.AllowEdit = false;
            colArea.OptionsColumn.ReadOnly = true;
            colArea.Visible = true;
            colArea.VisibleIndex = 0;
            colArea.Width = 108;
            // 
            // colClientNumber
            // 
            colClientNumber.Caption = "מספר לקוח";
            colClientNumber.FieldName = "ClientNumber";
            colClientNumber.Name = "colClientNumber";
            colClientNumber.OptionsColumn.AllowEdit = false;
            colClientNumber.OptionsColumn.ReadOnly = true;
            colClientNumber.Visible = true;
            colClientNumber.VisibleIndex = 1;
            colClientNumber.Width = 176;
            // 
            // colClientName
            // 
            colClientName.Caption = "שם לקוח";
            colClientName.FieldName = "ClientName";
            colClientName.Name = "colClientName";
            colClientName.OptionsColumn.AllowEdit = false;
            colClientName.OptionsColumn.ReadOnly = true;
            colClientName.Visible = true;
            colClientName.VisibleIndex = 2;
            colClientName.Width = 436;
            // 
            // colRequestDate
            // 
            colRequestDate.Caption = "תאריך הבקשה";
            colRequestDate.ColumnEdit = repositoryItemRequestDate;
            colRequestDate.DisplayFormat.FormatString = "dd/MM/yy";
            colRequestDate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            colRequestDate.FieldName = "RequestDate";
            colRequestDate.Name = "colRequestDate";
            colRequestDate.Visible = true;
            colRequestDate.VisibleIndex = 3;
            colRequestDate.Width = 150;
            // 
            // repositoryItemRequestDate
            // 
            repositoryItemRequestDate.AutoHeight = false;
            repositoryItemRequestDate.DisplayFormat.FormatString = "dd/MM/yy";
            repositoryItemRequestDate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            repositoryItemRequestDate.EditFormat.FormatString = "dd/MM/yy";
            repositoryItemRequestDate.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            repositoryItemRequestDate.Mask.UseMaskAsDisplayFormat = true;
            repositoryItemRequestDate.Name = "repositoryItemRequestDate";
            // 
            // panelFind
            // 
            panelFind.Controls.Add(txtFindClient);
            panelFind.Controls.Add(lblFind);
            panelFind.Dock = DockStyle.Top;
            panelFind.Location = new Point(0, 0);
            panelFind.Name = "panelFind";
            panelFind.Size = new Size(900, 38);
            panelFind.TabIndex = 0;
            // 
            // txtFindClient
            // 
            txtFindClient.Location = new Point(604, 7);
            txtFindClient.Name = "txtFindClient";
            txtFindClient.Size = new Size(140, 22);
            txtFindClient.TabIndex = 1;
            txtFindClient.KeyDown += txtFindClient_KeyDown;
            // 
            // lblFind
            // 
            lblFind.Location = new Point(761, 10);
            lblFind.Name = "lblFind";
            lblFind.Size = new Size(103, 16);
            lblFind.TabIndex = 0;
            lblFind.Text = "מס' לקוח לאיתור";
            // 
            // grpSaved
            // 
            grpSaved.AppearanceCaption.ForeColor = Color.Red;
            grpSaved.AppearanceCaption.Options.UseForeColor = true;
            grpSaved.Controls.Add(gridSaved);
            grpSaved.Dock = DockStyle.Fill;
            grpSaved.Location = new Point(0, 0);
            grpSaved.Name = "grpSaved";
            grpSaved.Size = new Size(900, 210);
            grpSaved.TabIndex = 0;
            grpSaved.Text = "שורות שהוקלדו — לפני שמירה בדוק שכל התאריכים שנקלטו למטה תקינים";
            // 
            // gridSaved
            // 
            gridSaved.Dock = DockStyle.Fill;
            gridSaved.Location = new Point(2, 28);
            gridSaved.MainView = gridViewSaved;
            gridSaved.Name = "gridSaved";
            gridSaved.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { repositoryItemSavedDate });
            gridSaved.Size = new Size(896, 180);
            gridSaved.TabIndex = 0;
            gridSaved.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridViewSaved });
            gridSaved.ProcessGridKey += gridControl_ProcessGridKey;
            // 
            // gridViewSaved
            // 
            gridViewSaved.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { colSavedArea, colSavedClientNumber, colSavedClientName, colSavedRequestDate });
            gridViewSaved.GridControl = gridSaved;
            gridViewSaved.Name = "gridViewSaved";
            gridViewSaved.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
            gridViewSaved.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;
            gridViewSaved.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown;
            gridViewSaved.OptionsView.ShowGroupPanel = false;
            gridViewSaved.CellValueChanged += gridView_CellValueChanged;
            gridViewSaved.KeyDown += gridView_KeyDown;
            gridViewSaved.ValidatingEditor += gridView_ValidatingEditor;
            // 
            // colSavedArea
            // 
            colSavedArea.AppearanceCell.Options.UseTextOptions = true;
            colSavedArea.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            colSavedArea.Caption = "אזור";
            colSavedArea.FieldName = "Area";
            colSavedArea.Name = "colSavedArea";
            colSavedArea.OptionsColumn.AllowEdit = false;
            colSavedArea.OptionsColumn.ReadOnly = true;
            colSavedArea.Visible = true;
            colSavedArea.VisibleIndex = 0;
            colSavedArea.Width = 95;
            // 
            // colSavedClientNumber
            // 
            colSavedClientNumber.Caption = "מספר לקוח";
            colSavedClientNumber.FieldName = "ClientNumber";
            colSavedClientNumber.Name = "colSavedClientNumber";
            colSavedClientNumber.OptionsColumn.AllowEdit = false;
            colSavedClientNumber.OptionsColumn.ReadOnly = true;
            colSavedClientNumber.Visible = true;
            colSavedClientNumber.VisibleIndex = 1;
            colSavedClientNumber.Width = 204;
            // 
            // colSavedClientName
            // 
            colSavedClientName.Caption = "שם לקוח";
            colSavedClientName.FieldName = "ClientName";
            colSavedClientName.Name = "colSavedClientName";
            colSavedClientName.OptionsColumn.AllowEdit = false;
            colSavedClientName.OptionsColumn.ReadOnly = true;
            colSavedClientName.Visible = true;
            colSavedClientName.VisibleIndex = 2;
            colSavedClientName.Width = 419;
            // 
            // colSavedRequestDate
            // 
            colSavedRequestDate.Caption = "תאריך הבקשה";
            colSavedRequestDate.ColumnEdit = repositoryItemSavedDate;
            colSavedRequestDate.DisplayFormat.FormatString = "dd/MM/yy";
            colSavedRequestDate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            colSavedRequestDate.FieldName = "RequestDate";
            colSavedRequestDate.Name = "colSavedRequestDate";
            colSavedRequestDate.Visible = true;
            colSavedRequestDate.VisibleIndex = 3;
            colSavedRequestDate.Width = 150;
            // 
            // repositoryItemSavedDate
            // 
            repositoryItemSavedDate.AutoHeight = false;
            repositoryItemSavedDate.DisplayFormat.FormatString = "dd/MM/yy";
            repositoryItemSavedDate.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            repositoryItemSavedDate.EditFormat.FormatString = "dd/MM/yy";
            repositoryItemSavedDate.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            repositoryItemSavedDate.Mask.UseMaskAsDisplayFormat = true;
            repositoryItemSavedDate.Name = "repositoryItemSavedDate";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(766, 13);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(100, 22);
            btnSave.TabIndex = 0;
            btnSave.Text = "שמור";
            btnSave.Click += btnSave_Click;
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(lblCount);
            panelBottom.Controls.Add(btnSave);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 556);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(900, 44);
            panelBottom.TabIndex = 1;
            // 
            // lblCount
            // 
            lblCount.Location = new Point(130, 13);
            lblCount.Name = "lblCount";
            lblCount.Size = new Size(0, 16);
            lblCount.TabIndex = 1;
            // 
            // RequestDateForm
            // 
            ClientSize = new Size(900, 600);
            Controls.Add(splitMain);
            Controls.Add(panelBottom);
            MinimumSize = new Size(600, 400);
            Name = "RequestDateForm";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Text = "עדכון ידני של תאריכי בקשת הנחה על פי מסמך הבקשה";
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemRequestDate).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelFind).EndInit();
            panelFind.ResumeLayout(false);
            panelFind.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtFindClient.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)grpSaved).EndInit();
            grpSaved.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridSaved).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridViewSaved).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemSavedDate).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelBottom).EndInit();
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private DevExpress.XtraGrid.GridControl gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView;
        private DevExpress.XtraEditors.PanelControl panelFind;
        private DevExpress.XtraEditors.LabelControl lblFind;
        private DevExpress.XtraEditors.TextEdit txtFindClient;
        private DevExpress.XtraGrid.Columns.GridColumn colArea;
        private DevExpress.XtraGrid.Columns.GridColumn colClientNumber;
        private DevExpress.XtraGrid.Columns.GridColumn colClientName;
        private DevExpress.XtraGrid.Columns.GridColumn colRequestDate;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemRequestDate;
        private DevExpress.XtraEditors.GroupControl grpSaved;
        private DevExpress.XtraGrid.GridControl gridSaved;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewSaved;
        private DevExpress.XtraGrid.Columns.GridColumn colSavedArea;
        private DevExpress.XtraGrid.Columns.GridColumn colSavedClientNumber;
        private DevExpress.XtraGrid.Columns.GridColumn colSavedClientName;
        private DevExpress.XtraGrid.Columns.GridColumn colSavedRequestDate;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemSavedDate;
        private DevExpress.XtraEditors.PanelControl panelBottom;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.LabelControl lblCount;
    }
}
