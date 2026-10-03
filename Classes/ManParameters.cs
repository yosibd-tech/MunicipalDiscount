using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace MunicipalDiscount.Classes
{
    /// <summary>
    /// MunicipalDiscount's application parameters, loaded from the private
    /// key/value table tbMainParameters ([Key]/[Value]/[Description]/
    /// [HebrewFolderName]/[ParametersGroup]). Edited via ManParametersForm's
    /// grid. Not shared with MonicipalYBD (which uses tbGAppSettings).
    /// </summary>
    public static class ManParameters
    {
        public const string FoldersGroup = "סיפריות קבצים";

        public static string PreviousDiscountPath { get; private set; }
        public static string CurrentYearBillingPath { get; private set; }
        public static string ActualCurrentYearDiscountPath { get; private set; }
        public static string IncomeCriteriaPath { get; private set; }
        public static string HelperFilesFolder { get; private set; }
        // Where the discount-code / rate workbooks live — the pair that carries a
        // municipality's codes to another municipality. A managed folder like the
        // rest, so FolderNames creates it with its Hebrew alias at startup.
        public static string DiscountCodesPath { get; private set; }
        public static string IllegalDiscountMessage { get; private set; }

        // The import scripts are copied beside the executable by the build, so
        // that is where they are unless someone says otherwise. Before this, a
        // blank parameter meant every import button refused to run with
        // "נתיב סקריפטים Python לא הוגדר" even though the scripts were present.
        public static string DefaultPythonScriptsPath =>
            System.IO.Path.Combine(AppContext.BaseDirectory, "PythonScripts");

        public static string PythonScriptsPath { get; private set; } = DefaultPythonScriptsPath;

        // Where every generated report is written. A managed folder like the
        // import ones: the real path is English so code and the Python scripts
        // never carry a Hebrew path, and FolderNames gives it the Hebrew name
        // "דוחות" that Explorer shows. Defaulted rather than left null, so a
        // report still lands somewhere sensible if the row, or the database,
        // is missing.
        public const string DefaultReportsPath = @"C:\MunicipalDiscount\Reports";
        public static string ReportsPath { get; private set; } = DefaultReportsPath;
        public static int DiscountExcelHeaderRowNum { get; private set; } = 1;
        public static int BllingExcelHeaderRowNum { get; private set; } = 1;

        public static void Load()
        {
            try
            {
                var values = new Dictionary<string, string>();
                using (var conn = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT [Key], [Value] FROM tbMainParameters", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string key = reader.GetString(0);
                            values[key] = reader.IsDBNull(1) ? null : reader.GetString(1);
                        }
                    }
                }

                PreviousDiscountPath          = Get(values, "PreviousDiscountPath");
                CurrentYearBillingPath        = Get(values, "CurrentYearBillingPath");
                ActualCurrentYearDiscountPath = Get(values, "ActualCurrentYearDiscountPath");
                IncomeCriteriaPath            = Get(values, "IncomeCriteriaPath");
                HelperFilesFolder             = Get(values, "HelperFilesFolder");
                DiscountCodesPath             = Get(values, "DiscountCodesPath");
                IllegalDiscountMessage        = Get(values, "IllegalDiscountMessage");

                // Only an override — a blank row leaves the folder beside the exe.
                string scripts = Get(values, "PythonScriptsPath");
                PythonScriptsPath = string.IsNullOrWhiteSpace(scripts)
                    ? DefaultPythonScriptsPath
                    : scripts;

                string reports = Get(values, "ReportsPath");
                if (!string.IsNullOrWhiteSpace(reports))
                    ReportsPath = reports;
                DiscountExcelHeaderRowNum     = GetInt(values, "DiscountExcelHeaderRowNum", 1);
                BllingExcelHeaderRowNum       = GetInt(values, "BllingExcelHeaderRowNum", 1);
            }
            catch { }
        }

        private static string Get(Dictionary<string, string> values, string key)
            => values.TryGetValue(key, out var v) ? v : null;

        private static int GetInt(Dictionary<string, string> values, string key, int fallback)
            => values.TryGetValue(key, out var v) && int.TryParse(v, out var n) ? n : fallback;
    }
}
