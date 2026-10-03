namespace MunicipalDiscount
{
    partial class ManParametersForm
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
            gridControl = new DevExpress.XtraGrid.GridControl();
            gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            panelBottom = new DevExpress.XtraEditors.PanelControl();
            btnClose = new DevExpress.XtraEditors.SimpleButton();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            btnDelete = new DevExpress.XtraEditors.SimpleButton();
            btnNew = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)gridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelBottom).BeginInit();
            panelBottom.SuspendLayout();
            SuspendLayout();
            //
            // gridControl
            //
            gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            gridControl.Location = new System.Drawing.Point(0, 0);
            gridControl.MainView = gridView;
            gridControl.Name = "gridControl";
            gridControl.Size = new System.Drawing.Size(780, 400);
            gridControl.TabIndex = 0;
            gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView });
            //
            // gridView
            //
            gridView.GridControl = gridControl;
            gridView.Name = "gridView";
            gridView.OptionsView.EnableAppearanceOddRow = true;
            gridView.OptionsView.ShowGroupPanel = true;
            //
            // panelBottom
            //
            panelBottom.Controls.Add(btnClose);
            panelBottom.Controls.Add(btnSave);
            panelBottom.Controls.Add(btnDelete);
            panelBottom.Controls.Add(btnNew);
            panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBottom.Location = new System.Drawing.Point(0, 400);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new System.Drawing.Size(780, 48);
            panelBottom.TabIndex = 1;
            //
            // btnNew
            //
            btnNew.Location = new System.Drawing.Point(12, 8);
            btnNew.Name = "btnNew";
            btnNew.Size = new System.Drawing.Size(100, 32);
            btnNew.TabIndex = 0;
            btnNew.Text = "חדש";
            btnNew.Click += btnNew_Click;
            //
            // btnDelete
            //
            btnDelete.Location = new System.Drawing.Point(118, 8);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(100, 32);
            btnDelete.TabIndex = 1;
            btnDelete.Text = "מחק";
            btnDelete.Click += btnDelete_Click;
            //
            // btnSave
            //
            btnSave.Location = new System.Drawing.Point(224, 8);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(100, 32);
            btnSave.TabIndex = 2;
            btnSave.Text = "שמור";
            btnSave.Click += btnSave_Click;
            //
            // btnClose
            //
            btnClose.Location = new System.Drawing.Point(330, 8);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(100, 32);
            btnClose.TabIndex = 3;
            btnClose.Text = "סגור";
            btnClose.Click += btnClose_Click;
            //
            // ManParametersForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(780, 448);
            Controls.Add(gridControl);
            Controls.Add(panelBottom);
            MinimizeBox = false;
            Name = "ManParametersForm";
            RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            RightToLeftLayout = true;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "פרמטרים";
            Load += ManParametersForm_Load;
            ((System.ComponentModel.ISupportInitialize)gridControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelBottom).EndInit();
            panelBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView;
        private DevExpress.XtraEditors.PanelControl panelBottom;
        private DevExpress.XtraEditors.SimpleButton btnNew;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnClose;
    }
}
