using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    /// <summary>
    /// Centralized theme helper for applying TheBezier skin styling across all forms
    /// </summary>
    public static class ThemeHelper
    {
        // TheBezier color scheme
        public static readonly Color PrimaryColor = Color.FromArgb(42, 82, 152);    // #2A5298 - deep blue
        public static readonly Color DangerColor = Color.FromArgb(192, 57, 43);     // #C0392B - red
        public static readonly Color SuccessColor = Color.FromArgb(39, 174, 96);    // #27AE60 - green

        /// <summary>
        /// Apply TheBezier theme to entire form - call this in Form_Load event
        /// </summary>
        public static void ApplyTheme(Form form)
        {
            ApplyToControls(form.Controls);
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                // LayoutControl - apply to all nested LayoutControlGroups
                if (control is LayoutControl layoutControl)
                {
                    ApplyToLayoutGroups(layoutControl.Root);
                }

                // GroupControl - style the caption
                if (control is GroupControl grp)
                {
                    grp.AppearanceCaption.Font = new Font("Tahoma", 10F, FontStyle.Bold);
                    grp.AppearanceCaption.ForeColor = PrimaryColor;
                    grp.AppearanceCaption.Options.UseFont = true;
                    grp.AppearanceCaption.Options.UseForeColor = true;
                }

                // SimpleButton - remove custom appearance, let skin handle it
                if (control is SimpleButton btn)
                {
                    btn.Appearance.Reset();
                }

                // LabelControl - remove custom fonts
                if (control is LabelControl lbl)
                {
                    lbl.Appearance.Reset();
                }

                // TextEdit - remove custom fonts
                if (control is TextEdit txt)
                {
                    txt.Properties.Appearance.Reset();
                }

                // SpinEdit - remove custom fonts (keep alignment)
                if (control is SpinEdit spin)
                {
                    // Keep center alignment but remove font overrides
                    var currentAlign = spin.Properties.Appearance.TextOptions.HAlignment;
                    spin.Properties.Appearance.Reset();
                    spin.Properties.Appearance.TextOptions.HAlignment = currentAlign;
                }

                // PanelControl - recurse into panels
                if (control is PanelControl panel)
                {
                    ApplyToControls(panel.Controls);
                }

                // Recurse into child controls
                if (control.HasChildren)
                {
                    ApplyToControls(control.Controls);
                }
            }
        }

        private static void ApplyToLayoutGroups(LayoutControlGroup group)
        {
            // Apply theme to groups with visible borders (titled groups)
            if (group.GroupBordersVisible)
            {
                group.AppearanceGroup.Font = new Font("Tahoma", 10F, FontStyle.Bold);
                group.AppearanceGroup.ForeColor = PrimaryColor;
                group.AppearanceGroup.Options.UseFont = true;
                group.AppearanceGroup.Options.UseForeColor = true;
            }

            // Recurse into all items
            foreach (var item in group.Items)
            {
                if (item is LayoutControlGroup nestedGroup)
                {
                    ApplyToLayoutGroups(nestedGroup);
                }
            }
        }

        /// <summary>
        /// Apply special color to a specific control (e.g., green for success result)
        /// </summary>
        public static void SetSuccessColor(BaseEdit control)
        {
            control.ForeColor = SuccessColor;
        }

        /// <summary>
        /// Apply danger color to a specific control
        /// </summary>
        public static void SetDangerColor(BaseEdit control)
        {
            control.ForeColor = DangerColor;
        }
    }
}