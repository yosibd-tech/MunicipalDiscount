namespace MunicipalDiscount
{
    partial class frmAddNewClient
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
            this.grpClientDetails = new DevExpress.XtraEditors.GroupControl();
            this.txtClientNumber = new DevExpress.XtraEditors.TextEdit();
            this.txtClientName = new DevExpress.XtraEditors.TextEdit();
            this.txtAssetNumber = new DevExpress.XtraEditors.TextEdit();
            this.lblAssetNumber = new DevExpress.XtraEditors.LabelControl();
            this.lblClientNumber = new DevExpress.XtraEditors.LabelControl();
            this.lblClientName = new DevExpress.XtraEditors.LabelControl();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.grpClientDetails)).BeginInit();
            this.grpClientDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtClientNumber.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtClientName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtAssetNumber.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // grpClientDetails
            // 
            this.grpClientDetails.AppearanceCaption.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.grpClientDetails.AppearanceCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(82)))), ((int)(((byte)(152)))));
            this.grpClientDetails.AppearanceCaption.Options.UseFont = true;
            this.grpClientDetails.AppearanceCaption.Options.UseForeColor = true;
            this.grpClientDetails.Controls.Add(this.lblClientNumber);
            this.grpClientDetails.Controls.Add(this.txtClientNumber);
            this.grpClientDetails.Controls.Add(this.lblClientName);
            this.grpClientDetails.Controls.Add(this.txtClientName);
            this.grpClientDetails.Controls.Add(this.txtAssetNumber);
            this.grpClientDetails.Controls.Add(this.lblAssetNumber);
            this.grpClientDetails.Location = new System.Drawing.Point(12, 12);
            this.grpClientDetails.Name = "grpClientDetails";
            this.grpClientDetails.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.grpClientDetails.Size = new System.Drawing.Size(290, 125);
            this.grpClientDetails.TabIndex = 0;
            this.grpClientDetails.Text = "פרטי לקוח חדש";
            // 
            // lblClientNumber
            // 
            this.lblClientNumber.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);
            this.lblClientNumber.Appearance.Options.UseFont = true;
            this.lblClientNumber.Location = new System.Drawing.Point(200, 35);
            this.lblClientNumber.Name = "lblClientNumber";
            this.lblClientNumber.Size = new System.Drawing.Size(70, 16);
            this.lblClientNumber.TabIndex = 0;
            this.lblClientNumber.Text = "מספר לקוח:";
            // 
            // txtClientNumber
            // 
            this.txtClientNumber.Location = new System.Drawing.Point(15, 32);
            this.txtClientNumber.Name = "txtClientNumber";
            this.txtClientNumber.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11F);
            this.txtClientNumber.Properties.Appearance.Options.UseFont = true;
            this.txtClientNumber.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtClientNumber.Size = new System.Drawing.Size(170, 24);
            this.txtClientNumber.TabIndex = 1;
            // 
            // lblClientName
            // 
            this.lblClientName.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);
            this.lblClientName.Appearance.Options.UseFont = true;
            this.lblClientName.Location = new System.Drawing.Point(212, 65);
            this.lblClientName.Name = "lblClientName";
            this.lblClientName.Size = new System.Drawing.Size(58, 16);
            this.lblClientName.TabIndex = 2;
            this.lblClientName.Text = "שם לקוח:";
            // 
            // txtClientName
            // 
            this.txtClientName.Location = new System.Drawing.Point(15, 62);
            this.txtClientName.Name = "txtClientName";
            this.txtClientName.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11F);
            this.txtClientName.Properties.Appearance.Options.UseFont = true;
            this.txtClientName.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtClientName.Size = new System.Drawing.Size(170, 24);
            this.txtClientName.TabIndex = 3;
            // 
            // lblAssetNumber
            // 
            this.lblAssetNumber.Location = new System.Drawing.Point(203, 95);
            this.lblAssetNumber.Name = "lblAssetNumber";
            this.lblAssetNumber.Size = new System.Drawing.Size(67, 16);
            this.lblAssetNumber.TabIndex = 4;
            this.lblAssetNumber.Text = "מספר נכס:";
            // 
            // txtAssetNumber
            // 
            this.txtAssetNumber.Location = new System.Drawing.Point(15, 92);
            this.txtAssetNumber.Name = "txtAssetNumber";
            this.txtAssetNumber.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 11F);
            this.txtAssetNumber.Properties.Appearance.Options.UseFont = true;
            this.txtAssetNumber.Properties.MaxLength = 20;
            this.txtAssetNumber.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtAssetNumber.Size = new System.Drawing.Size(170, 24);
            this.txtAssetNumber.TabIndex = 5;
            // 
            // btnSave
            // 
            this.btnSave.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.Appearance.Options.UseFont = true;
            this.btnSave.Location = new System.Drawing.Point(172, 145);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(130, 35);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "שמור";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);
            this.btnCancel.Appearance.Options.UseFont = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 145);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(130, 35);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "ביטול";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // frmAddNewClient
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(314, 191);
            this.Controls.Add(this.grpClientDetails);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAddNewClient";
            this.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "הוספת לקוח חדש";
            this.Load += new System.EventHandler(this.frmAddNewClient_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grpClientDetails)).EndInit();
            this.grpClientDetails.ResumeLayout(false);
            this.grpClientDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtClientNumber.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtClientName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtAssetNumber.Properties)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.GroupControl grpClientDetails;
        private DevExpress.XtraEditors.LabelControl lblClientNumber;
        private DevExpress.XtraEditors.TextEdit txtClientNumber;
        private DevExpress.XtraEditors.LabelControl lblClientName;
        private DevExpress.XtraEditors.TextEdit txtClientName;
        private DevExpress.XtraEditors.TextEdit txtAssetNumber;
        private DevExpress.XtraEditors.LabelControl lblAssetNumber;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
    }
}