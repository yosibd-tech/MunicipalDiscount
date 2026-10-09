using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace MunicipalDiscount.Classes
{
    /// <summary>
    /// The backup warning at login (user's decision, 2026-10-09: in both apps).
    /// The shared database is backed up weekly by the collection app's scheduled
    /// run (MonicipalYBD.exe --backup); this only reads the outcome — the same
    /// check as MonicipalYBD's BackupStatus:
    ///   - no good full backup for more than <see cref="MaxDays"/> days (from
    ///     SQL Server's own backup history, so a manual backup counts too), or
    ///   - a problem reported by the last weekly run (tbGAppSettings
    ///     LastBackupProblem, e.g. the USB drive was missing).
    /// Null when all is well. Never throws.
    /// </summary>
    public static class BackupStatus
    {
        public const int MaxDays = 8;

        public static string Warning()
        {
            try
            {
                using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
                conn.Open();
                DateTime? last;
                using (var cmd = new SqlCommand(
                    "SELECT MAX(backup_finish_date) FROM msdb.dbo.backupset " +
                    "WHERE database_name = DB_NAME() AND type = 'D';", conn))
                    last = cmd.ExecuteScalar() as DateTime?;
                string problem = null;
                using (var cmd = new SqlCommand(
                    "IF OBJECT_ID('tbGAppSettings', 'U') IS NOT NULL " +
                    "SELECT [Value] FROM tbGAppSettings WHERE [Key] = N'LastBackupProblem';", conn))
                    problem = cmd.ExecuteScalar() as string;

                var lines = new List<string>();
                if (last == null)
                    lines.Add("מעולם לא בוצע גיבוי של מסד הנתונים.");
                else if ((DateTime.Now - last.Value).TotalDays > MaxDays)
                    lines.Add($"הגיבוי האחרון של מסד הנתונים בוצע לפני {(int)(DateTime.Now - last.Value).TotalDays} ימים ({last:dd/MM/yyyy}).");
                if (!string.IsNullOrWhiteSpace(problem))
                    lines.Add(problem);
                if (lines.Count == 0) return null;

                lines.Add("");
                lines.Add("הגיבוי השבועי רץ אוטומטית מאפליקציית הגביה (משימה \"MonicipalYBD - גיבוי שבועי\").");
                return string.Join("\n", lines);
            }
            catch
            {
                return null;
            }
        }
    }
}
