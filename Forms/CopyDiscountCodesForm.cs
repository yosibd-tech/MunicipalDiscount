using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MunicipalDiscount.Classes;

namespace MunicipalDiscount
{
    /// <summary>
    /// Small RTL dialog that collects a source year and a target year for the
    /// "copy discount codes" operation. It does NOT perform the copy itself —
    /// the caller reads <see cref="SourceYear"/> / <see cref="TargetYear"/> after
    /// a DialogResult.OK and runs the DB logic.
    /// </summary>
    public class CopyDiscountCodesForm : XtraForm
    {
        private SpinEdit spinSourceYear;
        private SpinEdit spinTargetYear;
        private SimpleButton btnCopy;
        private SimpleButton btnCancel;
        private LabelControl lblSource;
        private LabelControl lblTarget;

        public int SourceYear => Convert.ToInt32(spinSourceYear.Value);
        public int TargetYear => Convert.ToInt32(spinTargetYear.Value);

        public CopyDiscountCodesForm()
        {
            BuildUI();
            ThemeHelper.ApplyTheme(this);
        }

        private void BuildUI()
        {
            this.Text = "העתקת קודי הנחה";
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(320, 170);
            this.Font = new System.Drawing.Font("Tahoma", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.IconOptions.SvgImage = MunicipalDiscount.Classes.AppIcons.DiscountCodes;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;

            // Source year
            lblSource = new LabelControl
            {
                Text = "משנה:",
                Location = new System.Drawing.Point(240, 28),
                Size = new System.Drawing.Size(60, 18),
                AutoSizeMode = LabelAutoSizeMode.None
            };

            spinSourceYear = new SpinEdit
            {
                Location = new System.Drawing.Point(20, 25),
                Size = new System.Drawing.Size(210, 24)
            };
            spinSourceYear.Properties.IsFloatValue = false;
            spinSourceYear.Properties.MinValue = 2000;
            spinSourceYear.Properties.MaxValue = 2100;
            spinSourceYear.Value = DateTime.Now.Year - 1;

            // Target year
            lblTarget = new LabelControl
            {
                Text = "לשנה:",
                Location = new System.Drawing.Point(240, 63),
                Size = new System.Drawing.Size(60, 18),
                AutoSizeMode = LabelAutoSizeMode.None
            };

            spinTargetYear = new SpinEdit
            {
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(210, 24)
            };
            spinTargetYear.Properties.IsFloatValue = false;
            spinTargetYear.Properties.MinValue = 2000;
            spinTargetYear.Properties.MaxValue = 2100;
            spinTargetYear.Value = DateTime.Now.Year;

            // Buttons
            btnCopy = new SimpleButton
            {
                Text = "העתק",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(170, 115),
                Size = new System.Drawing.Size(130, 35)
            };

            btnCancel = new SimpleButton
            {
                Text = "ביטול",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(20, 115),
                Size = new System.Drawing.Size(130, 35)
            };

            this.Controls.Add(lblSource);
            this.Controls.Add(spinSourceYear);
            this.Controls.Add(lblTarget);
            this.Controls.Add(spinTargetYear);
            this.Controls.Add(btnCopy);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnCopy;
            this.CancelButton = btnCancel;
        }
    }
}
