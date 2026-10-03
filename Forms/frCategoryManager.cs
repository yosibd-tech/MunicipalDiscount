using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Microsoft.Data.SqlClient;
using MunicipalDiscount.Classes;

namespace MunicipalDiscount
{
    /// <summary>
    /// ניהול רשימת הקטגוריות של קודי ההנחה.
    ///
    /// The list is edited in memory and handed back through <see cref="Categories"/>;
    /// the caller writes it. Deleting only removes the name from the list — the
    /// rows in tbDiscountsCodes that use it are left alone, which is why the
    /// delete warns how many there are first.
    /// </summary>
    public partial class frCategoryManager : XtraForm
    {
        private readonly string connectionString = DatabaseConfig.ConnectionString;

        public List<string> Categories { get; private set; } = new List<string>();

        public frCategoryManager(List<string> currentCategories)
        {
            InitializeComponent();
            Categories = new List<string>(currentCategories.OrderBy(c => c));
        }

        private void frCategoryManager_Load(object sender, EventArgs e)
        {
            ThemeHelper.ApplyTheme(this);
            this.IconOptions.SvgImage = AppIcons.CategoryManager;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;

            RefreshList();
            txtNew.Focus();
        }

        private void RefreshList()
        {
            listBox.Items.Clear();
            foreach (string category in Categories.OrderBy(x => x))
                listBox.Items.Add(category);
        }

        /// <summary>Enter מוסיף, כדי שאפשר יהיה להקליד רשימה ברצף.</summary>
        private void txtNew_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.Handled = true;
            e.SuppressKeyPress = true;
            BtnAdd_Click(sender, EventArgs.Empty);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string value = txtNew.Text.Trim();
            if (string.IsNullOrEmpty(value)) return;

            // השוואה ללא תלות ברישיות, כדי ששתי גרסאות של אותו שם לא ייווצרו.
            if (Categories.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                XtraMessageBox.Show("קטגוריה זו כבר קיימת.", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNew.SelectAll();
                txtNew.Focus();
                return;
            }

            Categories.Add(value);
            RefreshList();
            txtNew.Text = "";
            txtNew.Focus();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedItem == null) return;

            string value = listBox.SelectedItem.ToString() ?? "";
            if (string.IsNullOrEmpty(value)) return;

            // כמה שורות משתמשות בקטגוריה — מחיקה כאן אינה נוגעת בהן, ולכן חשוב
            // לומר מראש מה יישאר מאחור.
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                using var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM tbDiscountsCodes WHERE Categories = @category", connection);
                cmd.Parameters.Add(new SqlParameter("@category", SqlDbType.NVarChar, 40) { Value = value });

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                {
                    var answer = XtraMessageBox.Show(
                        $"קטגוריה זו משמשת ב-{count} שורות. למחוק בכל זאת?",
                        "אזהרה", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (answer != DialogResult.Yes) return;
                }
            }
            catch (Exception ex)
            {
                // אי אפשר לספור — עדיין אפשר למחוק, אך לא בשקט.
                var answer = XtraMessageBox.Show(
                    $"לא ניתן לבדוק כמה שורות משתמשות בקטגוריה:\n{ex.Message}\n\nלמחוק בכל זאת?",
                    "אזהרה", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (answer != DialogResult.Yes) return;
            }

            Categories.Remove(value);
            RefreshList();
        }
    }
}
