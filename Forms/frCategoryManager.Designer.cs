using DevExpress.XtraEditors;
using System.Drawing;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    partial class frCategoryManager
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
            lblTitle = new LabelControl();
            listBox = new ListBoxControl();
            lblNew = new LabelControl();
            txtNew = new TextEdit();
            btnAdd = new SimpleButton();
            btnDelete = new SimpleButton();
            btnOk = new SimpleButton();
            ((System.ComponentModel.ISupportInitialize)listBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtNew.Properties).BeginInit();
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.Appearance.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new Point(10, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(340, 16);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "קטגוריות קיימות:";
            //
            // listBox
            //
            listBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBox.Location = new Point(10, 30);
            listBox.Name = "listBox";
            listBox.RightToLeft = RightToLeft.Yes;
            listBox.Size = new Size(340, 250);
            listBox.TabIndex = 1;
            //
            // lblNew
            //
            lblNew.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblNew.Appearance.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            lblNew.Appearance.Options.UseFont = true;
            lblNew.Location = new Point(10, 295);
            lblNew.Name = "lblNew";
            lblNew.Size = new Size(120, 16);
            lblNew.TabIndex = 2;
            lblNew.Text = "קטגוריה חדשה:";
            //
            // txtNew
            //
            txtNew.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtNew.Location = new Point(10, 315);
            txtNew.Name = "txtNew";
            txtNew.RightToLeft = RightToLeft.Yes;
            txtNew.Size = new Size(230, 22);
            txtNew.TabIndex = 3;
            txtNew.KeyDown += txtNew_KeyDown;
            //
            // btnAdd
            //
            btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAdd.Location = new Point(250, 314);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(100, 23);
            btnAdd.TabIndex = 4;
            btnAdd.Text = "הוסף";
            btnAdd.Click += BtnAdd_Click;
            //
            // btnDelete
            //
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Appearance.BackColor = Color.FromArgb(200, 50, 50);
            btnDelete.Appearance.ForeColor = Color.White;
            btnDelete.Appearance.Options.UseBackColor = true;
            btnDelete.Appearance.Options.UseForeColor = true;
            btnDelete.Location = new Point(10, 350);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(160, 28);
            btnDelete.TabIndex = 5;
            btnDelete.Text = "מחק נבחרת";
            btnDelete.Click += BtnDelete_Click;
            //
            // btnOk
            //
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.Appearance.BackColor = Color.FromArgb(0, 153, 76);
            btnOk.Appearance.ForeColor = Color.White;
            btnOk.Appearance.Options.UseBackColor = true;
            btnOk.Appearance.Options.UseForeColor = true;
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(260, 350);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(90, 28);
            btnOk.TabIndex = 6;
            btnOk.Text = "אישור";
            //
            // frCategoryManager
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(360, 420);
            Controls.Add(btnOk);
            Controls.Add(btnDelete);
            Controls.Add(btnAdd);
            Controls.Add(txtNew);
            Controls.Add(lblNew);
            Controls.Add(listBox);
            Controls.Add(lblTitle);
            Font = new Font("Tahoma", 9F);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frCategoryManager";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            Text = "ניהול קטגוריות";
            Load += frCategoryManager_Load;
            ((System.ComponentModel.ISupportInitialize)listBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtNew.Properties).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.ListBoxControl listBox;
        private DevExpress.XtraEditors.LabelControl lblNew;
        private DevExpress.XtraEditors.TextEdit txtNew;
        private DevExpress.XtraEditors.SimpleButton btnAdd;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnOk;
    }
}
