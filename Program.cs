using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using DevExpress.XtraBars.Ribbon;
using MunicipalDiscount.Classes;

namespace MunicipalDiscount
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Apply TheBezier skin for consistent modern look
            UserLookAndFeel.Default.SetSkinStyle(SkinSvgPalette.Bezier.OfficeColorful);

            // עברית לכל מחרוזות הרשת ולחלון הסינון של העמודה. שני מתרגמים
            // נפרדים: חלון הסינון בסגנון אקסל אינו קורא את מחרוזות הרשת אלא
            // את אלה של FilterUIElementLocalizer, ולכן נשאר באנגלית עד עכשיו.
            DevExpress.XtraGrid.Localization.GridLocalizer.Active = new HebrewGridLocalizer();
            DevExpress.Utils.Filtering.Internal.FilterUIElementLocalizer.Active = new HebrewFilterLocalizer();

            // הפעלה עם ‎--setup פותחת את מסך ההגדרות ישירות, בלי להתחבר תחילה —
            // כך אפשר לתקן הגדרות שגויות גם כשהתוכנה אינה עולה.
            bool setupRequested = args.Any(a =>
                a.Equals("--setup", StringComparison.OrdinalIgnoreCase) ||
                a.Equals("/setup", StringComparison.OrdinalIgnoreCase));

            if (setupRequested)
            {
                using var setup = new ConnectionSetupForm();
                setup.ShowDialog();
                DatabaseConfig.Reset();
            }

            // A bad or missing configuration used to be a dead end: a message box
            // and exit, with the file editable only by hand. Now the screen that
            // fixes it is offered right here.
            while (!DatabaseConfig.TryLoad(out string configError))
            {
                var choice = MessageBox.Show(
                    $"{configError}\n\nלפתוח את מסך הגדרות החיבור?",
                    "שגיאת הגדרות מסד נתונים",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (choice != DialogResult.Yes)
                    return;

                using var setup = new ConnectionSetupForm();
                setup.ShowDialog();
                DatabaseConfig.Reset();
            }

            // Parameters first: EnsureAll needs the configured folder paths,
            // and it used to run before they were ever read.
            ManParameters.Load();

            // Ensure managed import folders exist (English names on disk, Hebrew
            // display names in Explorer via desktop.ini). Runs every launch.
            MunicipalDiscount.Classes.FolderNames.EnsureAll();

            // Show login form
            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                SessionData.SetUser(
                    loginForm.LoggedInUserId,
                    loginForm.LoggedInIdentityCard,
                    loginForm.LoggedInFullName);
                PermissionManager.LoadPermissions(loginForm.LoggedInUserId);
                ManParameters.Load();
            }

            Application.Run(new MainForm());
        }
    }
}