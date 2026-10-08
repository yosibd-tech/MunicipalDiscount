using System.Reflection;
using Microsoft.Data.SqlClient;

namespace MunicipalDiscount.Classes
{
    public enum DbState { Ok, NotInstalled, Older, Newer }

    public sealed class DbCheckResult
    {
        public DbState State { get; init; }
        public string Message { get; init; } = "";
    }

    /// <summary>
    /// Checks — never changes — the database at startup. The shared database is
    /// built and upgraded only by the installer from the numbered scripts
    /// (MonicipalYBD\Database\Migrations), which records every applied script in
    /// tbDbSchemaVersions. This build knows the last script it was built with
    /// (RequiredDbScript, from build_install_package.ps1); a development build
    /// has none and only checks that the database was installed at all.
    /// Same rules as MonicipalYBD's DbVersionCheck.
    /// </summary>
    public static class DbVersionCheck
    {
        public static string? RequiredScript =>
            Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => a.Key == "RequiredDbScript" && !string.IsNullOrWhiteSpace(a.Value))?.Value;

        public static DbCheckResult Check()
        {
            var applied = new List<string>();
            using (var conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using var exists = new SqlCommand("SELECT OBJECT_ID('dbo.tbDbSchemaVersions', 'U')", conn);
                if (exists.ExecuteScalar() is DBNull or null)
                    return NotInstalled();

                using var cmd = new SqlCommand("SELECT ScriptName FROM dbo.tbDbSchemaVersions", conn);
                using var rd = cmd.ExecuteReader();
                while (rd.Read()) applied.Add(rd.GetString(0));
            }
            if (applied.Count == 0) return NotInstalled();

            string? required = RequiredScript;
            if (required == null) return new DbCheckResult { State = DbState.Ok };

            if (!applied.Contains(required, StringComparer.OrdinalIgnoreCase))
                return new DbCheckResult
                {
                    State = DbState.Older,
                    Message = "מסד הנתונים ישן מגרסת האפליקציה (חסר בו העדכון " + required + ").\n" +
                              "יש להריץ את תוכנת ההתקנה של " + AppVersion.Display +
                              " — כפתור \"יצירה / עדכון מסד נתונים\"."
                };

            string newest = applied.OrderByDescending(Number).First();
            if (Number(newest) > Number(required))
                return new DbCheckResult
                {
                    State = DbState.Newer,
                    Message = "מסד הנתונים עודכן לגרסה חדשה יותר מזו המותקנת במחשב זה (" + AppVersion.Display + ").\n" +
                              "מומלץ להתקין במחשב זה את הגרסה החדשה של האפליקציה."
                };

            return new DbCheckResult { State = DbState.Ok };
        }

        private static DbCheckResult NotInstalled() => new()
        {
            State = DbState.NotInstalled,
            Message = "מסד הנתונים לא הותקן על ידי תוכנת ההתקנה (אין בו רישום גרסה).\n" +
                      "יש להריץ את תוכנת ההתקנה — כפתור \"יצירה / עדכון מסד נתונים\".\n" +
                      "אם זה המסד הלא נכון — יש לבדוק את הגדרות החיבור (הפעלה עם ‎--setup)."
        };

        private static int Number(string script) =>
            int.TryParse(new string(script.TakeWhile(char.IsDigit).ToArray()), out int n) ? n : 0;
    }
}
