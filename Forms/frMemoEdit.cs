using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using MunicipalDiscount.Classes;

namespace MunicipalDiscount
{
    public class frMemoEdit : XtraForm
    {
        public string EditedText => memoEdit.Text;

        private MemoEdit memoEdit;
        private SimpleButton btnEdit;
        private SimpleButton btnOk;
        private SimpleButton btnCancel;

        private void InitializeComponent()
        {

        }

        public frMemoEdit(string title, string currentText)
        {
            this.Text = title;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Font = new System.Drawing.Font("Tahoma", 9F);

            memoEdit = new MemoEdit
            {
                Dock = DockStyle.Fill,
                RightToLeft = RightToLeft.Yes,
                Text = currentText
            };
            memoEdit.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 10F);

            // הטופס נפתח לקריאה בלבד. הטקסט נצפה לרוב ולא נערך, והוא נשמר
            // לבסיס הנתונים מיד עם אישור — ולכן עריכה מחייבת בחירה מפורשת.
            // ReadOnly ולא Enabled=false: כך אפשר עדיין לגלול, לסמן ולהעתיק.
            memoEdit.Properties.ReadOnly = true;

            var panelButtons = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };

            btnEdit = new SimpleButton
            {
                Text = "ערוך",
                Width = 90,
                Height = 28,
                Location = new System.Drawing.Point(10, 6)
            };
            btnEdit.Click += (s, e) => EnterEditMode();

            btnOk = new SimpleButton
            {
                Text = "אישור",
                Width = 90,
                Height = 28,
                Location = new System.Drawing.Point(110, 6),
                DialogResult = DialogResult.OK,
                // אין מה לאשר עוד לא נכנסו לעריכה — כך גם נמנע עדכון מיותר
                // של הטבלה בכל פתיחה של הטופס.
                Enabled = false
            };

            btnCancel = new SimpleButton
            {
                Text = "סגור",
                Width = 90,
                Height = 28,
                Location = new System.Drawing.Point(210, 6),
                DialogResult = DialogResult.Cancel
            };

            panelButtons.Controls.Add(btnEdit);
            panelButtons.Controls.Add(btnOk);
            panelButtons.Controls.Add(btnCancel);

            this.Controls.Add(memoEdit);
            this.Controls.Add(panelButtons);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            // לחיצה כפולה על הטקסט היא דרך מקוצרת לעריכה
            memoEdit.DoubleClick += (s, e) => EnterEditMode();

            this.IconOptions.SvgImage = MunicipalDiscount.Classes.AppIcons.MemoEdit;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;

            ThemeHelper.ApplyTheme(this);
        }

        /// <summary>
        /// מעבר ממצב צפייה למצב עריכה: רק אז אפשר לשנות את הטקסט ולאשר.
        /// </summary>
        private void EnterEditMode()
        {
            if (!memoEdit.Properties.ReadOnly) return;

            memoEdit.Properties.ReadOnly = false;
            btnEdit.Enabled = false;
            btnOk.Enabled = true;
            btnCancel.Text = "ביטול";
            memoEdit.Focus();
            memoEdit.SelectionStart = memoEdit.Text.Length;
        }
    }
}
