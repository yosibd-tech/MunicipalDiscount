using DevExpress.XtraEditors;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using MunicipalDiscount.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    /// <summary>
    /// מסך ניהול מערכת — בדיקת תקינות ההתקנה.
    ///
    /// Answers, on one screen, the questions that otherwise cost an afternoon:
    /// is the schema current, are the parameters set, are the folders and the
    /// scripts where the app expects them, and is the year's data in a state the
    /// eligibility check and the reports can actually work with.
    ///
    /// It only ever READS. A failing check names the script or the screen that
    /// fixes it — a migration applied behind a button is a migration nobody
    /// reviewed, and schema changes stay deliberate.
    /// </summary>
    public partial class AdminForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly string connectionString = DatabaseConfig.ConnectionString;
        private readonly BindingList<CheckResult> _results = new BindingList<CheckResult>();

        private static readonly string LayoutFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MunicipalDiscount", "AdminForm.xml");

        /// <summary>
        /// תקרה לשורות בגיליון פירוט. גבוהה בכוונה: הפירוט אמור להיות הרשימה
        /// המלאה שאפשר לעבוד לפיה, ולא מדגם ממנה.
        /// </summary>
        private const int DetailRowLimit = 50000;

        public AdminForm()
        {
            InitializeComponent();
            gridChecks.DataSource = _results;
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            ThemeHelper.ApplyTheme(this);
            this.IconOptions.SvgImage = AppIcons.Parameters;
            this.IconOptions.SvgImageSize = new Size(32, 32);
            this.IconOptions.ShowIcon = true;

            RestoreFormSize();

            spinYear.Value = SessionData.SelectedYear ?? DateTime.Now.Year;
            RunChecks();
        }

        #region Check model

        private enum CheckStatus { Ok, Warning, Error }

        private class CheckResult
        {
            /// <summary>
            /// נכלל בייצוא ובדואר. אף שורה אינה מסומנת מראש — מי שמייצא בוחר
            /// בעצמו מה נכנס לדוח.
            /// </summary>
            public bool Selected { get; set; }

            public string Group { get; set; } = "";
            public string Name { get; set; } = "";
            public CheckStatus Status { get; set; }
            public string Detail { get; set; } = "";
            public string Action { get; set; } = "";

            /// <summary>
            /// שאילתה שמחזירה את השורות שמאחורי המספר — נכתבת כגיליון נפרד
            /// באקסל. רצה רק בעת הייצוא, כדי שרענון המסך יישאר זול.
            /// </summary>
            public string? DetailSql { get; set; }

            public (string name, object value)[] DetailArgs { get; set; }
                = Array.Empty<(string, object)>();

            /// <summary>תיקייה שתוכנה יירשם כגיליון, במקום שאילתה.</summary>
            public string? DetailFolder { get; set; }

            public bool HasDetail => DetailSql is not null || DetailFolder is not null;

            public CheckResult WithDetail(string sql, params (string name, object value)[] args)
            {
                DetailSql = sql;
                DetailArgs = args;
                return this;
            }

            public CheckResult WithFolder(string path)
            {
                DetailFolder = path;
                return this;
            }

            public string StatusText => Status switch
            {
                CheckStatus.Ok      => "תקין",
                CheckStatus.Warning => "אזהרה",
                _                   => "תקלה",
            };
        }

        /// <summary>מוסיף בדיקה ומחזיר אותה, כדי שאפשר יהיה לתלות עליה פירוט.</summary>
        private CheckResult Add(string group, string name, CheckStatus status, string detail, string action = "")
        {
            var result = new CheckResult
            {
                Group = group, Name = name, Status = status, Detail = detail, Action = action,
            };

            _results.Add(result);
            return result;
        }

        #endregion

        #region Running the checks

        private void RunChecks()
        {
            Cursor.Current = Cursors.WaitCursor;
            _results.RaiseListChangedEvents = false;
            _results.Clear();

            int year = Convert.ToInt32(spinYear.Value);

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    CheckSchema(conn);
                    CheckCriteria(conn, year);
                    CheckData(conn, year);
                }
            }
            catch (Exception ex)
            {
                // No connection means none of the database checks can run, and
                // that is itself the finding — reported rather than thrown.
                Add("מסד נתונים", "חיבור למסד הנתונים", CheckStatus.Error,
                    ex.Message,
                    "יש לבדוק את appsettings.json ואת פעילות שרת ה-SQL.");
            }

            CheckFolders();
            CheckScripts();
            CheckRegistrySource();

            _results.RaiseListChangedEvents = true;
            _results.ResetBindings();

            viewChecks.BestFitColumns();
            ShowSummary();
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// השורות המסומנות, בסדר שבו הן מוצגות. PostEditor נקרא תחילה: בלעדיו
        /// סימון שנעשה רגע לפני הלחיצה עדיין יושב בעורך ולא בשורה.
        /// </summary>
        private List<CheckResult> SelectedResults()
        {
            viewChecks.PostEditor();
            return _results.Where(r => r.Selected).ToList();
        }

        private void ShowSummary()
        {
            int errors = _results.Count(r => r.Status == CheckStatus.Error);
            int warnings = _results.Count(r => r.Status == CheckStatus.Warning);

            lblSummary.Text = errors == 0 && warnings == 0
                ? $"כל {_results.Count} הבדיקות תקינות."
                : $"{_results.Count} בדיקות — {errors} תקלות, {warnings} אזהרות.";

            lblSummary.Appearance.ForeColor =
                errors > 0 ? Color.FromArgb(170, 40, 30)
                : warnings > 0 ? Color.FromArgb(150, 100, 10)
                : Color.FromArgb(30, 110, 70);
        }

        private static bool ColumnExists(SqlConnection conn, string table, string column)
        {
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = @t AND COLUMN_NAME = @c", conn);
            cmd.Parameters.AddWithValue("@t", table);
            cmd.Parameters.AddWithValue("@c", column);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private static int Scalar(SqlConnection conn, string sql, params (string name, object value)[] args)
        {
            using var cmd = new SqlCommand(sql, conn);
            foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value);
            object? result = cmd.ExecuteScalar();
            return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// שורת נכס מלאה: הלקוח, הנכס וסיווגו, השטח והחיוב. כל גיליון פירוט
        /// ברמת נכס מתחיל מכאן, כדי שהעובד יקבל שורה שאפשר לעבוד לפיה ולא
        /// מספר שצריך לברר.
        /// </summary>
        private const string AssetColumns = @"
            SELECT c.ClientNumber AS [מספר לקוח], c.ClientName AS [שם לקוח],
                   c.PhoneNumber AS [טלפון], y.Year_ AS [שנה],
                   y.AssetNumber AS [מספר נכס], y.AssetCode AS [קוד סוג נכס],
                   k.Name AS [סוג נכס], k.Category AS [קטגוריה],
                   y.AssetSize AS [שטח], y.Area AS [אזור],
                   y.AnnualBilling AS [חיוב שנתי]
            FROM tbYearlyDiscounts y
            LEFT JOIN tbGAssetKinds k ON k.Code = y.AssetCode
            LEFT JOIN tbClients c ON c.ID = y.ClientId
            ";

        /// <summary>אותה שורה, ועליה הבקשה עצמה על כל שדותיה.</summary>
        private const string RequestColumns = @"
            SELECT c.ClientNumber AS [מספר לקוח], c.ClientName AS [שם לקוח],
                   c.PhoneNumber AS [טלפון], y.Year_ AS [שנה],
                   y.AssetNumber AS [מספר נכס], y.AssetCode AS [קוד סוג נכס],
                   k.Name AS [סוג נכס], k.Category AS [קטגוריה],
                   y.AssetSize AS [שטח], y.AnnualBilling AS [חיוב שנתי],
                   r.MunicipalDiscountCode AS [קוד הנחה],
                   r.MunicipalDiscountCodeName AS [שם הקוד בקובץ],
                   d.DiscountName AS [שם ההנחה במערכת],
                   r.RequestDate AS [תאריך בקשה],
                   ABS(r.ActualDiscountAmount) AS [סכום שניתן],
                   r.EntitledDiscountPercent AS [אחוז זכאות],
                   r.ComputedDiscount AS [הנחה מחושבת],
                   CASE WHEN r.Status = 1 THEN N'מאושר' ELSE N'לא מאושר' END AS [סטטוס],
                   u.FullName AS [נקלט על ידי], r.CreateDate AS [נוצר]
            FROM tbDiscountRequests r
            JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId
            LEFT JOIN tbClients c ON c.ID = y.ClientId
            LEFT JOIN tbGAssetKinds k ON k.Code = y.AssetCode
            LEFT JOIN tbDiscountsCodes d ON d.MunicipalDiscountCode = r.MunicipalDiscountCode
            LEFT JOIN tbUsers u ON u.ID = r.UserId
            ";

        private const string ByClient = @" ORDER BY c.ClientNumber, y.AssetNumber";

        private void CheckSchema(SqlConnection conn)
        {
            const string g = "סכמה";

            bool assetCode = ColumnExists(conn, "tbYearlyDiscounts", "AssetCode");
            Add(g, "tbYearlyDiscounts.AssetCode",
                assetCode ? CheckStatus.Ok : CheckStatus.Error,
                assetCode ? "קיימת" : "חסרה — בלעדיה אי אפשר לדעת אילו נכסים הם מגורים",
                assetCode ? "" : "יש להריץ SQL\\2026-09_YearlyDiscounts_AssetCode.sql");

            bool criteriaCode = ColumnExists(conn, "tbIncomeDiscountCriteria", "MunicipalDiscountCode");
            Add(g, "tbIncomeDiscountCriteria.MunicipalDiscountCode",
                criteriaCode ? CheckStatus.Ok : CheckStatus.Error,
                criteriaCode ? "קיימת" : "חסרה — בדיקת הזכאות לא תדע לאיזה קוד המדרגה מזכה",
                criteriaCode ? "" : "יש להריץ SQL\\2026-09b_IncomeCriteria_MunicipalDiscountCode.sql");

            int view = Scalar(conn,
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_NAME = 'vwDiscountRateAsOf'");
            Add(g, "vwDiscountRateAsOf",
                view > 0 ? CheckStatus.Ok : CheckStatus.Error,
                view > 0 ? "קיים" : "חסר — אי אפשר לקבוע איזו גרסת תעריף חלה",
                view > 0 ? "" : "יש להריץ SQL\\2026-08b_Rename_tbDiscountCodeRates.sql");

            try
            {
                int kinds = Scalar(conn, "SELECT COUNT(*) FROM tbGAssetKinds");
                int residential = Scalar(conn,
                    "SELECT COUNT(*) FROM tbGAssetKinds WHERE Category = N'מגורים'");
                int noCategory = Scalar(conn,
                    "SELECT COUNT(*) FROM tbGAssetKinds WHERE Category IS NULL OR LTRIM(RTRIM(Category)) = ''");

                var kindsCheck = Add(g, "tbGAssetKinds (סוגי נכסים)",
                    residential > 0 ? CheckStatus.Ok : CheckStatus.Error,
                    $"{kinds} סוגים, מתוכם {residential} בקטגוריית מגורים, {noCategory} ללא קטגוריה",
                    noCategory > 0
                        ? $"{noCategory} סוגי נכס ללא קטגוריה — נכסים שלהם לא ייקלטו ולא ייבדקו"
                        : "");

                if (noCategory > 0)
                    kindsCheck.WithDetail(@"
                        SELECT Code AS [קוד סוג נכס], Name AS [תיאור],
                               Category AS [קטגוריה], MainCategory AS [קטגוריה ראשית]
                        FROM tbGAssetKinds
                        WHERE Category IS NULL OR LTRIM(RTRIM(Category)) = ''
                        ORDER BY Code");
            }
            catch (Exception ex)
            {
                Add(g, "tbGAssetKinds (סוגי נכסים)", CheckStatus.Error, ex.Message,
                    "הטבלה שייכת ל-MonicipalYBD ונדרשת לסינון נכסי מגורים.");
            }
        }

        private void CheckCriteria(SqlConnection conn, int year)
        {
            const string g = "קריטריונים";

            int rows = Scalar(conn,
                "SELECT COUNT(*) FROM tbIncomeDiscountCriteria WHERE Year_ = @y", ("@y", year));

            Add(g, $"מדרגות הכנסה לשנת {year}",
                rows > 0 ? CheckStatus.Ok : CheckStatus.Error,
                rows > 0 ? $"{rows} מדרגות" : "אין מדרגות לשנה זו",
                rows > 0 ? "" : "יש להזין מדרגות במסך קריטריונים למבחן הכנסה, או לקלוט מאקסל.");

            if (rows == 0) return;

            if (ColumnExists(conn, "tbIncomeDiscountCriteria", "MunicipalDiscountCode"))
            {
                int noCode = Scalar(conn,
                    "SELECT COUNT(*) FROM tbIncomeDiscountCriteria " +
                    "WHERE Year_ = @y AND MunicipalDiscountCode IS NULL", ("@y", year));

                var codeCheck = Add(g, "קוד הנחה לכל מדרגה",
                    noCode == 0 ? CheckStatus.Ok : CheckStatus.Error,
                    noCode == 0 ? "כל המדרגות נושאות קוד" : $"{noCode} מדרגות ללא קוד הנחה",
                    noCode == 0 ? "" : "החלטה שתתקבל לפי מדרגה כזו תישמר בלי קוד. יש להשלים במסך הקריטריונים.");

                if (noCode > 0)
                    codeCheck.WithDetail(@"
                        SELECT ValidityDate AS [בתוקף מ], NumberOfPeople AS [נפשות],
                               FromAmount AS [מהכנסה], ToAmount AS [עד הכנסה],
                               DiscountRate AS [אחוז הנחה],
                               MunicipalDiscountCode AS [קוד הנחה]
                        FROM tbIncomeDiscountCriteria
                        WHERE Year_ = @y AND MunicipalDiscountCode IS NULL
                        ORDER BY ValidityDate, NumberOfPeople, FromAmount", ("@y", year));
            }

            int versions = Scalar(conn,
                "SELECT COUNT(DISTINCT ValidityDate) FROM tbIncomeDiscountCriteria WHERE Year_ = @y", ("@y", year));
            Add(g, "גרסאות תוקף", CheckStatus.Ok, $"{versions} גרסאות בשנה זו",
                versions > 1 ? "התאריך שהעובד מקליד קובע איזו גרסה חלה." : "")
                .WithDetail(@"
                    SELECT ValidityDate AS [בתוקף מ], NumberOfPeople AS [נפשות],
                           FromAmount AS [מהכנסה], ToAmount AS [עד הכנסה],
                           DiscountRate AS [אחוז הנחה],
                           MunicipalDiscountCode AS [קוד הנחה]
                    FROM tbIncomeDiscountCriteria
                    WHERE Year_ = @y
                    ORDER BY ValidityDate, NumberOfPeople, FromAmount", ("@y", year));
        }

        private void CheckData(SqlConnection conn, int year)
        {
            const string g = "נתוני שנה";

            int yearRows = Scalar(conn,
                "SELECT COUNT(*) FROM tbYearlyDiscounts WHERE Year_ = @y", ("@y", year));

            var yearCheck = Add(g, $"שורות נכס לשנת {year}",
                yearRows > 0 ? CheckStatus.Ok : CheckStatus.Error,
                $"{yearRows} שורות",
                yearRows > 0 ? "" : "יש לקלוט את קובץ החיוב השנתי — הוא יוצר את הנכסים.");

            if (yearRows == 0) return;

            if (!ColumnExists(conn, "tbYearlyDiscounts", "AssetCode")) return;

            yearCheck.WithDetail(AssetColumns + " WHERE y.Year_ = @y" + ByClient, ("@y", year));

            int unclassified = Scalar(conn,
                "SELECT COUNT(*) FROM tbYearlyDiscounts y " +
                "LEFT JOIN tbGAssetKinds k ON k.Code = y.AssetCode " +
                "WHERE y.Year_ = @y AND (k.Category IS NULL OR LTRIM(RTRIM(k.Category)) = '')",
                ("@y", year));

            var unclassifiedCheck = Add(g, "נכסים ללא סיווג",
                unclassified == 0 ? CheckStatus.Ok : CheckStatus.Warning,
                unclassified == 0 ? "אין" : $"{unclassified} מתוך {yearRows}",
                unclassified == 0 ? "" : "נכסים אלה אינם נכללים בסכום ההנחה ואינם נבדקים בדוח.");

            if (unclassified > 0)
                unclassifiedCheck.WithDetail(
                    AssetColumns +
                    @" WHERE y.Year_ = @y
                         AND (k.Category IS NULL OR LTRIM(RTRIM(k.Category)) = '')" +
                    ByClient, ("@y", year));

            int noBilling = Scalar(conn,
                "SELECT COUNT(*) FROM tbYearlyDiscounts WHERE Year_ = @y AND (AnnualBilling IS NULL OR AnnualBilling = 0)",
                ("@y", year));
            var billingCheck = Add(g, "נכסים ללא חיוב שנתי",
                noBilling == 0 ? CheckStatus.Ok : CheckStatus.Warning,
                noBilling == 0 ? "לכולם יש חיוב" : $"{noBilling} מתוך {yearRows}",
                noBilling == 0 ? "" : "לנכסים אלה סכום ההנחה יחושב 0.");

            if (noBilling > 0)
                billingCheck.WithDetail(
                    AssetColumns +
                    " WHERE y.Year_ = @y AND (y.AnnualBilling IS NULL OR y.AnnualBilling = 0)" +
                    ByClient, ("@y", year));

            int requests = Scalar(conn,
                "SELECT COUNT(*) FROM tbDiscountRequests r " +
                "JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId WHERE y.Year_ = @y", ("@y", year));

            var requestsCheck = Add(g, "בקשות הנחה", requests > 0 ? CheckStatus.Ok : CheckStatus.Warning,
                $"{requests} בקשות",
                requests > 0 ? "" : "יש לקלוט את קובץ ההנחות שניתנו.");

            if (requests == 0) return;

            requestsCheck.WithDetail(RequestColumns + " WHERE y.Year_ = @y" + ByClient, ("@y", year));

            int noDate = Scalar(conn,
                "SELECT COUNT(*) FROM tbDiscountRequests r " +
                "JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId " +
                "WHERE y.Year_ = @y AND r.RequestDate IS NULL", ("@y", year));

            var dateCheck = Add(g, "תאריך בקשה",
                noDate == 0 ? CheckStatus.Ok : CheckStatus.Warning,
                noDate == 0 ? "לכל הבקשות יש תאריך" : $"{noDate} בקשות ללא תאריך",
                noDate == 0 ? ""
                    : "בלי תאריך הדוח בוחר תמיד את גרסת התעריף האחרונה. יש לקלוט תאריכים מקובץ תנועות.");

            if (noDate > 0)
                dateCheck.WithDetail(
                    RequestColumns + " WHERE y.Year_ = @y AND r.RequestDate IS NULL" + ByClient,
                    ("@y", year));

            int unknownCodes = Scalar(conn,
                "SELECT COUNT(DISTINCT r.MunicipalDiscountCode) FROM tbDiscountRequests r " +
                "JOIN tbYearlyDiscounts y ON y.ID = r.YearlyDiscountId " +
                "WHERE y.Year_ = @y AND r.MunicipalDiscountCode IS NOT NULL " +
                "  AND r.MunicipalDiscountCode NOT IN " +
                "      (SELECT MunicipalDiscountCode FROM tbDiscountsCodes WHERE MunicipalDiscountCode IS NOT NULL)",
                ("@y", year));

            var knownCheck = Add(g, "קודי הנחה מוכרים",
                unknownCodes == 0 ? CheckStatus.Ok : CheckStatus.Warning,
                unknownCodes == 0 ? "כל הקודים רשומים" : $"{unknownCodes} קודים אינם ב-tbDiscountsCodes",
                unknownCodes == 0 ? "" : "קודים לא רשומים אינם מקבלים תעריף, ולכן אינם נבדקים בדוח.");

            if (unknownCodes > 0)
                knownCheck.WithDetail(
                    RequestColumns +
                    @" WHERE y.Year_ = @y AND r.MunicipalDiscountCode IS NOT NULL
                         AND r.MunicipalDiscountCode NOT IN
                             (SELECT MunicipalDiscountCode FROM tbDiscountsCodes
                              WHERE MunicipalDiscountCode IS NOT NULL)" +
                    ByClient, ("@y", year));
        }

        private void CheckFolders()
        {
            const string g = "תיקיות";

            var folders = new (string name, string? path)[]
            {
                ("קובץ חיוב שנתי",     ManParameters.CurrentYearBillingPath),
                ("הנחות שניתנו",        ManParameters.ActualCurrentYearDiscountPath),
                ("הנחות שנה קודמת",     ManParameters.PreviousDiscountPath),
                ("מדרגות הכנסה",        ManParameters.IncomeCriteriaPath),
                ("קודי הנחה",           ManParameters.DiscountCodesPath),
                ("דוחות",               ManParameters.ReportsPath),
            };

            foreach (var (name, path) in folders)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    Add(g, name, CheckStatus.Warning, "לא הוגדרה",
                        "תיבת בחירת הקובץ תיפתח בתיקייה אקראית. יש להגדיר במסך ניהול פרמטרים.");
                }
                else if (!Directory.Exists(path))
                {
                    Add(g, name, CheckStatus.Warning, $"לא קיימת בדיסק: {path}",
                        "יש ליצור את התיקייה או לתקן את הנתיב במסך ניהול פרמטרים.");
                }
                else
                {
                    // הפירוט הוא תוכן התיקייה: השאלה האמיתית היא אם הקובץ הונח בה.
                    Add(g, name, CheckStatus.Ok, path).WithFolder(path);
                }
            }
        }

        private void CheckScripts()
        {
            const string g = "סקריפטים";

            string folder = ManParameters.PythonScriptsPath;
            bool usingDefault = string.Equals(folder, ManParameters.DefaultPythonScriptsPath,
                                              StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                Add(g, "תיקיית סקריפטים", CheckStatus.Error, $"לא נמצאה: {folder}",
                    "ברירת המחדל היא תיקיית PythonScripts לצד קובץ ההפעלה. " +
                    "אם הוגדר נתיב אחר בפרמטרים — יש לרוקן אותו.");
                return;
            }

            Add(g, "תיקיית סקריפטים", CheckStatus.Ok,
                folder + (usingDefault ? "  (ברירת מחדל, לצד קובץ ההפעלה)" : "  (נתיב שהוגדר בפרמטרים)"),
                usingDefault ? "" : "נתיב ידני עלול להצביע על עותק ישן של הסקריפטים.")
                .WithFolder(folder);

            foreach (string script in new[] { "import_discounts.py", "import_request_dates.py", "db_config.py" })
            {
                bool exists = File.Exists(Path.Combine(folder, script));
                Add(g, script, exists ? CheckStatus.Ok : CheckStatus.Error,
                    exists ? "קיים" : "חסר",
                    exists ? "" : "הקליטה המשתמשת בו תיכשל.");
            }

            bool python = FindOnPath("python.exe") is not null;
            Add(g, "Python מותקן", python ? CheckStatus.Ok : CheckStatus.Error,
                python ? "נמצא בנתיב המערכת" : "לא נמצא",
                python ? "" : "הקליטות המבוססות Python לא ירוצו. יש להתקין Python ולוודא שהוא ב-PATH.");
        }

        private static string? FindOnPath(string fileName)
        {
            string paths = Environment.GetEnvironmentVariable("PATH") ?? "";
            foreach (string folder in paths.Split(Path.PathSeparator))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(folder)) continue;
                    string candidate = Path.Combine(folder.Trim(), fileName);
                    if (File.Exists(candidate)) return candidate;
                }
                catch { /* a malformed PATH entry is not a failure */ }
            }
            return null;
        }

        private void CheckRegistrySource()
        {
            const string g = "רישום אוכלוסין";

            // The web application reads these; the desktop does not. They are
            // checked here because this is the screen someone opens when the web
            // page will not find a family.
            var values = ReadParameters("RegistrySource", "FamilyRegistryDir", "FamilyRegistryFile",
                                        "RegistryGraphTenantId", "RegistryGraphClientId",
                                        "RegistryGraphSecret", "RegistryGraphDriveId", "RegistryGraphPath");

            string source = values.GetValueOrDefault("RegistrySource") ?? "disk";
            bool graph = source.Equals("graph", StringComparison.OrdinalIgnoreCase);

            Add(g, "מקור הקובץ", CheckStatus.Ok,
                graph ? "ענן (SharePoint/OneDrive)" : "תיקייה מקומית",
                graph ? "" : "קובץ מרשם אוכלוסין שאסור להעתיק למחשב מחייב RegistrySource = graph.");

            if (graph)
            {
                string[] required = { "RegistryGraphTenantId", "RegistryGraphClientId",
                                      "RegistryGraphSecret", "RegistryGraphDriveId", "RegistryGraphPath" };
                var missing = required.Where(k => string.IsNullOrWhiteSpace(values.GetValueOrDefault(k))).ToList();

                Add(g, "פרמטרי גישה לענן",
                    missing.Count == 0 ? CheckStatus.Ok : CheckStatus.Error,
                    missing.Count == 0 ? "כל הפרמטרים הוגדרו" : "חסרים: " + string.Join(", ", missing),
                    missing.Count == 0 ? "" : "נדרש רישום אפליקציה ב-Entra ID עם הרשאת Sites.Selected.");
            }
            else
            {
                string dir = values.GetValueOrDefault("FamilyRegistryDir") ?? "";
                string file = values.GetValueOrDefault("FamilyRegistryFile") ?? "";

                if (file.Contains("://", StringComparison.Ordinal))
                {
                    Add(g, "קובץ רישום אוכלוסין", CheckStatus.Error,
                        "הוגדרה כתובת אינטרנט כשם הקובץ",
                        "קריאה מקומית אינה יכולה לקרוא כתובת. יש להגדיר RegistrySource = graph.");
                }
                else if (string.IsNullOrWhiteSpace(dir) || string.IsNullOrWhiteSpace(file))
                {
                    Add(g, "קובץ רישום אוכלוסין", CheckStatus.Error, "לא הוגדרו תיקייה ושם קובץ",
                        "יש להגדיר FamilyRegistryDir ו-FamilyRegistryFile.");
                }
                else
                {
                    bool exists = !file.Contains("{client}") && File.Exists(Path.Combine(dir, file));
                    Add(g, "קובץ רישום אוכלוסין",
                        exists || file.Contains("{client}") ? CheckStatus.Ok : CheckStatus.Warning,
                        Path.Combine(dir, file),
                        exists || file.Contains("{client}") ? "" : "הקובץ לא נמצא בנתיב זה.");
                }
            }
        }

        private Dictionary<string, string> ReadParameters(params string[] keys)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var conn = new SqlConnection(connectionString);
                conn.Open();
                using var cmd = new SqlCommand("SELECT [Key], [Value] FROM tbMainParameters", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string key = reader.GetString(0);
                    if (keys.Contains(key, StringComparer.OrdinalIgnoreCase))
                        values[key] = reader.IsDBNull(1) ? "" : reader.GetString(1);
                }
            }
            catch { /* the connection failure is already reported above */ }
            return values;
        }

        #endregion

        #region Event handlers

        private void btnRefresh_Click(object sender, EventArgs e) => RunChecks();

        private void btnClose_Click(object sender, EventArgs e) => Close();

        /// <summary>
        /// שולח את תוכן המסך בדואר לכל המנהלים הפעילים שיש להם כתובת.
        ///
        /// The screen tells you what is wrong; the mail is how it reaches whoever
        /// can act on it, which is rarely the person sitting in front of it.
        /// </summary>
        private void btnMail_Click(object sender, EventArgs e)
        {
            if (SelectedResults().Count == 0)
            {
                XtraMessageBox.Show("לא סומנה אף בדיקה לשליחה.", "שליחת דוח",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MailSender.Settings settings;
            List<string> recipients;

            try
            {
                settings = MailSender.LoadSettings();
                recipients = MailSender.GetAdminRecipients();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"לא ניתן לקרוא את הגדרות הדואר:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!settings.IsConfigured)
            {
                // ההפניה היא למסך הגדרות הדואר ולא למסך הפרמטרים: הסיסמה נשמרת
                // מוצפנת, ורק המסך ההוא יודע להצפין אותה.
                if (XtraMessageBox.Show(
                        "הגדרות הדואר אינן מלאות:\n\n" +
                        string.Join("\n", settings.Missing) +
                        "\n\nלפתוח את מסך הגדרות הדואר?",
                        "הגדרות דואר", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

                using (var form = new MailSetupForm())
                {
                    if (form.ShowDialog(this) != DialogResult.OK) return;
                }

                settings = MailSender.LoadSettings();

                if (!settings.IsConfigured)
                {
                    XtraMessageBox.Show(
                        "ההגדרות עדיין אינן מלאות:\n\n" + string.Join("\n", settings.Missing),
                        "הגדרות דואר", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (recipients.Count == 0)
            {
                XtraMessageBox.Show(
                    "לא נמצאו מנהלים פעילים עם כתובת דואר. יש להשלים כתובות במסך ניהול משתמשים.",
                    "שליחת דוח", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool viaOutlook = settings.Method == MailSender.SendMethod.Outlook;

            if (XtraMessageBox.Show(
                    (viaOutlook
                        ? $"לפתוח ב-Outlook את דוח התקינות אל {recipients.Count} מנהלים?\n\n"
                        : $"לשלוח את דוח התקינות אל {recipients.Count} מנהלים?\n\n") +
                    string.Join("\n", recipients),
                    "אישור שליחה", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var rows = SelectedResults();
                int errors = rows.Count(r => r.Status == CheckStatus.Error);
                int warnings = rows.Count(r => r.Status == CheckStatus.Warning);

                string subject = errors > 0
                    ? $"בדיקת תקינות MunicipalDiscount — {errors} תקלות"
                    : warnings > 0
                        ? $"בדיקת תקינות MunicipalDiscount — {warnings} אזהרות"
                        : "בדיקת תקינות MunicipalDiscount — הכל תקין";

                // הקובץ הוא הדוח; ההודעה רק אומרת מה יש בו. כך אין שתי גרסאות
                // של אותו דוח שיכולות להיפרד זו מזו.
                string attachment = Path.Combine(Path.GetTempPath(),
                    $"בדיקת_תקינות_{Convert.ToInt32(spinYear.Value)}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");

                WriteExcel(attachment, rows);

                try
                {
                    MailSender.Send(settings, recipients, subject, BuildHtmlReport(),
                        new[] { attachment });
                }
                finally
                {
                    // דרך Outlook ההודעה עדיין פתוחה על המסך, והקובץ כבר מוטמע
                    // בה; דרך SMTP היא כבר יצאה. בשני המקרים אין עוד צורך בעותק
                    // הזמני, אבל מחיקה שנכשלת אינה סיבה להיכשל.
                    try { File.Delete(attachment); } catch { }
                }

                // דרך Outlook ההודעה נפתחה בלבד — לומר "נשלח" יהיה פשוט לא נכון.
                XtraMessageBox.Show(viaOutlook
                    ? $"הדוח נפתח ב-Outlook אל {recipients.Count} נמענים. יש ללחוץ שליחה שם."
                    : $"הדוח נשלח אל {recipients.Count} נמענים.",
                    "שליחת דוח", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שליחת הדוח נכשלה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// גוף ההודעה: מה נמצא, בשורה אחת לכל בדיקה — הפירוט עצמו נמצא בקובץ
        /// המצורף ואינו משוכפל לכאן.
        ///
        /// The workbook is the report; duplicating its contents in the body
        /// produced two versions of one thing, and the thin one was the one
        /// people read.
        /// </summary>
        private string BuildHtmlReport()
        {
            var rows = SelectedResults();
            int errors = rows.Count(r => r.Status == CheckStatus.Error);
            int warnings = rows.Count(r => r.Status == CheckStatus.Warning);

            var html = new System.Text.StringBuilder();
            html.Append("<html dir='rtl'><body style=\"font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#1b1b1b\">");
            html.Append($"<h2 style='margin:0 0 4px'>בדיקת תקינות — {Convert.ToInt32(spinYear.Value)}</h2>");
            html.Append($"<div style='color:#666;margin-bottom:14px'>{Environment.MachineName} · " +
                        $"{DateTime.Now:dd/MM/yy HH:mm} · {SessionData.UserFullName}</div>");

            string colour = errors > 0 ? "#aa281e" : warnings > 0 ? "#96640a" : "#1e6e46";
            html.Append($"<div style='font-weight:bold;color:{colour};margin-bottom:12px'>" +
                        $"בוצעו {_results.Count} בדיקות, {rows.Count} נכללו בדוח — " +
                        $"{errors} תקלות, {warnings} אזהרות</div>");

            html.Append("<ul style='margin:0 0 14px;padding-inline-start:20px'>");

            foreach (var row in rows
                         .OrderBy(r => r.Status == CheckStatus.Error ? 0
                                     : r.Status == CheckStatus.Warning ? 1 : 2)
                         .ThenBy(r => r.Group))
            {
                string colourOfRow = row.Status switch
                {
                    CheckStatus.Error   => "#aa281e",
                    CheckStatus.Warning => "#96640a",
                    _                   => "#1e6e46",
                };

                html.Append($"<li style='margin-bottom:3px'><span style='color:{colourOfRow}'>" +
                            $"<b>{Escape(row.StatusText)}</b></span> — " +
                            $"{Escape(row.Group)} / {Escape(row.Name)}: {Escape(row.Detail)}</li>");
            }

            html.Append("</ul>");
            html.Append("<div style='margin-bottom:14px'>" +
                        "הדוח המלא, ובו כל השורות שמאחורי כל בדיקה — לקוח, נכס, חיוב וקוד הנחה — " +
                        "נמצא בקובץ האקסל המצורף.</div>");
            html.Append("<div style='color:#888;font-size:12px'>" +
                        "נשלח אוטומטית ממסך ניהול המערכת.</div>");
            html.Append("</body></html>");

            return html.ToString();
        }

        private static string Escape(string? text) =>
            System.Net.WebUtility.HtmlEncode(text ?? "");

        /// <summary>
        /// מייצא את הבדיקות המסומנות לקובץ אקסל, עם כל הפרטים — כולל עמודת
        /// "מה לעשות", שבמסך נקטעת ובדואר נקראת רק פעם אחת.
        ///
        /// The file is what gets attached to a ticket or kept as evidence that
        /// the installation was checked on a date, so it carries the context the
        /// grid leaves implicit: machine, year, who ran it, and when.
        /// </summary>
        private void btnExcel_Click(object sender, EventArgs e)
        {
            var rows = SelectedResults();
            if (rows.Count == 0)
            {
                XtraMessageBox.Show("לא סומנה אף בדיקה לייצוא.", "ייצוא לאקסל",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int year = Convert.ToInt32(spinYear.Value);

            using var save = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "שמירת דוח תקינות",
                FileName = $"בדיקת_תקינות_{year}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
            };

            if (!string.IsNullOrWhiteSpace(ManParameters.ReportsPath)
                && Directory.Exists(ManParameters.ReportsPath))
                save.InitialDirectory = ManParameters.ReportsPath;

            if (save.ShowDialog() != DialogResult.OK) return;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var notes = WriteExcel(save.FileName, rows);

                // שורה שנקטעה או שאילתה שנכשלה חייבות להיאמר: קובץ חסר שנראה
                // שלם הוא הדבר היחיד שגרוע מקובץ שלא נוצר.
                string message = $"הדוח נשמר:\n{save.FileName}";

                if (notes.Count > 0)
                    message += "\n\n" + string.Join("\n", notes);

                if (XtraMessageBox.Show(message + "\n\nלפתוח אותו?",
                        "ייצוא לאקסל", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo(save.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שמירת הדוח נכשלה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// כותב את הקובץ: גיליון אחד לכל בדיקה שסומנה ויש מאחוריה שורות, ובו
        /// השורות עצמן בלבד — כותרת בשורה הראשונה והנתונים מיד אחריה.
        ///
        /// No summary sheet: the screen already says what was found, and a
        /// second copy of it in the file was one more thing to keep in step.
        /// The queries run here rather than while the checks run, so a screen
        /// refreshed all day stays cheap.
        ///
        /// Returns what the operator should be told: sheets that hit the row
        /// cap, and checks whose query failed.
        /// </summary>
        private List<string> WriteExcel(string path, List<CheckResult> rows)
        {
            var notes = new List<string>();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var workbook = new XLWorkbook();
            SqlConnection? conn = null;

            try
            {
                foreach (var result in rows
                             .OrderBy(r => r.Status == CheckStatus.Error ? 0
                                         : r.Status == CheckStatus.Warning ? 1 : 2)
                             .ThenBy(r => r.Group))
                {
                    if (!result.HasDetail) continue;

                    try
                    {
                        DataTable table;
                        bool truncated;

                        if (result.DetailFolder is not null)
                        {
                            table = FolderContents(result.DetailFolder, out truncated);
                        }
                        else
                        {
                            if (conn is null)
                            {
                                conn = new SqlConnection(connectionString);
                                conn.Open();
                            }

                            table = QueryDetail(conn, result.DetailSql!, result.DetailArgs, out truncated);
                        }

                        if (table.Rows.Count == 0) continue;

                        string name = UniqueSheetName(result.Name, used);
                        WriteDetailSheet(workbook.Worksheets.Add(name), table);

                        if (truncated)
                            notes.Add($"{name}: נקטע ב-{DetailRowLimit:N0} שורות.");
                    }
                    catch (Exception ex)
                    {
                        // בדיקה אחת שנכשלה אינה מפילה את הקובץ כולו.
                        notes.Add($"{result.Name}: הפירוט נכשל — {ex.Message}");
                    }
                }
            }
            finally
            {
                conn?.Dispose();
            }

            if (workbook.Worksheets.Count == 0)
                throw new InvalidOperationException(
                    "לבדיקות שסומנו אין שורות לפירוט. יש לסמן בדיקה שמאחוריה נתונים, " +
                    "למשל בקשות הנחה או נכסים ללא חיוב שנתי.");

            workbook.SaveAs(path);
            return notes;
        }

        /// <summary>
        /// גיליון אחד: כותרות בשורה 1, נתונים משורה 2, סינון וכותרת קפואה.
        /// </summary>
        private void WriteDetailSheet(IXLWorksheet sheet, DataTable table)
        {
            sheet.RightToLeft = true;

            for (int c = 0; c < table.Columns.Count; c++)
            {
                var cell = sheet.Cell(1, c + 1);
                cell.Value = table.Columns[c].ColumnName;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(42, 82, 152);
            }

            for (int r = 0; r < table.Rows.Count; r++)
                for (int c = 0; c < table.Columns.Count; c++)
                    SetCell(sheet.Cell(r + 2, c + 1), table.Rows[r][c]);

            var range = sheet.Range(1, 1, table.Rows.Count + 1, table.Columns.Count);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.SetAutoFilter();

            sheet.SheetView.FreezeRows(1);
            sheet.Columns().AdjustToContents();

            // AdjustToContents על שם לקוח ארוך מייצר עמודה ברוחב מסך.
            for (int c = 1; c <= table.Columns.Count; c++)
                if (sheet.Column(c).Width > 42) sheet.Column(c).Width = 42;
        }

        /// <summary>
        /// ערך בודד. הטיפוס נשמר — תאריך נשאר תאריך וסכום נשאר מספר, כדי
        /// שאפשר יהיה למיין ולסכם באקסל ולא רק לקרוא.
        /// </summary>
        private static void SetCell(IXLCell cell, object? value)
        {
            switch (value)
            {
                case null:
                case DBNull:
                    break;
                case string s:      cell.Value = s; break;
                case bool b:        cell.Value = b; break;
                case DateTime d:
                    cell.Value = d;
                    cell.Style.DateFormat.Format = d.TimeOfDay == TimeSpan.Zero
                        ? "dd/MM/yyyy" : "dd/MM/yyyy HH:mm";
                    break;
                case decimal m:     cell.Value = m; break;
                case double dbl:    cell.Value = dbl; break;
                case float f:       cell.Value = (double)f; break;
                case int i:         cell.Value = i; break;
                case long l:        cell.Value = l; break;
                case short sh:      cell.Value = sh; break;
                case byte by:       cell.Value = by; break;
                default:            cell.Value = value.ToString() ?? ""; break;
            }
        }

        /// <summary>
        /// מריץ את שאילתת הפירוט עד לתקרה. השמות בטבלה הם הכינויים בשאילתה,
        /// ולכן כותרות הגיליון נקבעות שם ולא כאן.
        /// </summary>
        private static DataTable QueryDetail(SqlConnection conn, string sql,
                                             (string name, object value)[] args, out bool truncated)
        {
            using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
            foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value);

            using var reader = cmd.ExecuteReader();

            var table = new DataTable();
            for (int i = 0; i < reader.FieldCount; i++)
                table.Columns.Add(reader.GetName(i), typeof(object));

            truncated = false;

            while (reader.Read())
            {
                if (table.Rows.Count >= DetailRowLimit)
                {
                    truncated = true;
                    break;
                }

                var values = new object[reader.FieldCount];
                reader.GetValues(values);
                table.Rows.Add(values);
            }

            return table;
        }

        /// <summary>תוכן תיקייה כטבלה — החדש קודם, כי הוא זה שנשאלת עליו השאלה.</summary>
        private static DataTable FolderContents(string folder, out bool truncated)
        {
            var table = new DataTable();
            table.Columns.Add("קובץ", typeof(object));
            table.Columns.Add("גודל (KB)", typeof(object));
            table.Columns.Add("עודכן", typeof(object));

            var files = new DirectoryInfo(folder).GetFiles()
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();

            truncated = files.Count > DetailRowLimit;

            foreach (var file in files.Take(DetailRowLimit))
                table.Rows.Add(file.Name, Math.Round(file.Length / 1024d, 1), file.LastWriteTime);

            return table;
        }

        /// <summary>
        /// שם גיליון חוקי וייחודי. אקסל אוסר 31 תווים ומעלה ואת התווים
        /// []:*?/\ — שם בדיקה שנכשל כאן היה מפיל את שמירת הקובץ כולו.
        /// </summary>
        private static string UniqueSheetName(string name, HashSet<string> used)
        {
            var clean = new string(name.Select(c => "[]:*?/\\".Contains(c) ? ' ' : c).ToArray()).Trim();
            if (clean.Length == 0) clean = "פירוט";
            if (clean.Length > 27) clean = clean.Substring(0, 27).Trim();

            string candidate = clean;
            int n = 2;

            while (!used.Add(candidate))
                candidate = $"{clean} {n++}";

            return candidate;
        }

        /// <summary>צובע את עמודת המצב, כדי שתקלה תזוהה בסריקה ולא בקריאה.</summary>
        private void viewChecks_RowCellStyle(object sender,
            DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            if (e.Column != colStatus) return;

            int index = viewChecks.GetDataSourceRowIndex(e.RowHandle);
            if (index < 0 || index >= _results.Count) return;

            switch (_results[index].Status)
            {
                case CheckStatus.Error:
                    e.Appearance.BackColor = Color.FromArgb(250, 226, 222);
                    e.Appearance.ForeColor = Color.FromArgb(140, 30, 20);
                    break;
                case CheckStatus.Warning:
                    e.Appearance.BackColor = Color.FromArgb(253, 243, 219);
                    e.Appearance.ForeColor = Color.FromArgb(125, 85, 5);
                    break;
                default:
                    e.Appearance.BackColor = Color.FromArgb(228, 243, 234);
                    e.Appearance.ForeColor = Color.FromArgb(25, 95, 60);
                    break;
            }
        }

        private void RestoreFormSize()
        {
            try
            {
                if (File.Exists(LayoutFilePath))
                {
                    string[] parts = File.ReadAllText(LayoutFilePath).Split(',');
                    if (parts.Length == 2
                        && int.TryParse(parts[0], out int w)
                        && int.TryParse(parts[1], out int h))
                    {
                        this.ClientSize = new Size(w, h);
                    }
                }
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(LayoutFilePath)!);
                    File.WriteAllText(LayoutFilePath, $"{ClientSize.Width},{ClientSize.Height}");
                }
            }
            catch { }
            base.OnFormClosing(e);
        }

        #endregion
    }
}
