using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace MunicipalDiscount.Classes
{
    /// <summary>
    /// Creates the managed import folders at startup. Each folder row lives in
    /// tbMainParameters under group "סיפריות קבצים": [Value] is the real
    /// (English) path created on disk — safe for code and the Python import
    /// scripts — and [HebrewFolderName] is the Hebrew alias written to a
    /// desktop.ini so Windows Explorer shows it. Mirrors MonicipalYBD's startup
    /// folder mechanism.
    /// </summary>
    public static class FolderNames
    {
        // Only folders under this root are ever auto-created — a guard so a
        // mistyped path in the table can't create folders elsewhere.
        public const string Root = @"C:\MunicipalDiscount";

        /// <summary>
        /// Called once at startup. Reads the folder rows from tbMainParameters,
        /// creates each folder (if missing) and writes its Hebrew Explorer name.
        /// Never throws — a failure can't block application start.
        /// </summary>
        public static void EnsureAll()
        {
            try
            {
                foreach (var (path, hebrewName) in LoadFolderRows()
                             // shortest paths first so parents are named before children
                             .OrderBy(r => r.path.Length))
                {
                    try { EnsureFolderUnderRoot(path, hebrewName); }
                    catch { /* never block startup on a single folder */ }
                }
            }
            catch { /* DB unavailable — skip folder setup */ }

            // The reports folder is guaranteed rather than configured. Every
            // other folder here is optional - a missing row just means that
            // import is unused - but a report always has to land somewhere
            // named, and making that depend on a hand-typed row landing in
            // exactly the right group meant it failed silently and Explorer
            // kept showing an unmanaged folder. A ReportsPath row still
            // overrides the path; only the existence of the folder and its
            // Hebrew name are no longer optional.
            try { EnsureFolderUnderRoot(ManParameters.ReportsPath, "דוחות"); }
            catch { /* never block startup on a single folder */ }
        }

        private static List<(string path, string hebrewName)> LoadFolderRows()
        {
            var rows = new List<(string, string)>();
            using (var conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT [Value], [HebrewFolderName] FROM tbMainParameters WHERE [ParametersGroup] = @g", conn))
                {
                    cmd.Parameters.AddWithValue("@g", ManParameters.FoldersGroup);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string path = reader.IsDBNull(0) ? null : reader.GetString(0);
                            string heb = reader.IsDBNull(1) ? null : reader.GetString(1);
                            if (!string.IsNullOrWhiteSpace(path))
                                rows.Add((path, heb));
                        }
                    }
                }
            }
            return rows;
        }

        /// <summary>
        /// True when the path would actually be managed - i.e. it resolves to a
        /// location under <see cref="Root"/>. Callers use this to tell the user
        /// a path is out of scope instead of letting it be ignored in silence.
        /// </summary>
        public static bool IsUnderRoot(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            try
            {
                return Path.GetFullPath(path)
                    .StartsWith(Path.GetFullPath(Root), StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }   // malformed path
        }

        /// <summary>
        /// Creates the folder and applies its Hebrew Explorer name, now. Same rules
        /// as startup; exposed so an edit in ManParametersForm takes effect when the
        /// user presses save rather than at the next launch.
        /// </summary>
        public static void Apply(string path, string hebrewDisplayName)
            => EnsureFolderUnderRoot(path, hebrewDisplayName);

        /// <summary>
        /// Drops the Hebrew Explorer name, restoring the folder's real name.
        ///
        /// Clearing the name in the grid used to do nothing: the desktop.ini was
        /// only ever written, never removed, so Explorer kept showing the old
        /// Hebrew name with no way back short of deleting the file by hand.
        /// </summary>
        public static void ClearDisplayName(string path)
        {
            if (!IsUnderRoot(path)) return;
            string full = Path.GetFullPath(path);
            if (!Directory.Exists(full)) return;

            string iniPath = Path.Combine(full, "desktop.ini");
            if (File.Exists(iniPath))
            {
                File.SetAttributes(iniPath, FileAttributes.Normal);
                File.Delete(iniPath);
            }

            // The System attribute is what makes Explorer read desktop.ini at all;
            // leaving it on an folder with no ini just makes it look odd.
            var dir = new DirectoryInfo(full);
            dir.Attributes &= ~FileAttributes.System;
        }

        /// <summary>
        /// Moves a managed folder, files and all, when its path is changed in
        /// ManParametersForm. Returns false when the move is not safe to make -
        /// either end outside the root, no source, or a destination that already
        /// exists - and the caller then falls back to simply creating the new one.
        /// </summary>
        public static bool TryMove(string oldPath, string newPath)
        {
            if (!IsUnderRoot(oldPath) || !IsUnderRoot(newPath)) return false;

            string from = Path.GetFullPath(oldPath);
            string to = Path.GetFullPath(newPath);
            if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase)) return false;
            if (!Directory.Exists(from)) return false;
            if (Directory.Exists(to)) return false;   // never merge into an existing folder

            Directory.CreateDirectory(Path.GetDirectoryName(to));
            Directory.Move(from, to);
            return true;
        }

        /// <summary>
        /// Creates <paramref name="path"/> only when it resolves to a location
        /// under <see cref="Root"/>; paths outside are ignored. Writes a
        /// desktop.ini so Explorer shows <paramref name="hebrewDisplayName"/>.
        /// </summary>
        private static void EnsureFolderUnderRoot(string path, string hebrewDisplayName)
        {
            if (string.IsNullOrEmpty(path)) return;

            string fullPath = Path.GetFullPath(path);
            string fullRoot = Path.GetFullPath(Root);

            if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
                return;

            Directory.CreateDirectory(fullPath);

            if (!string.IsNullOrEmpty(hebrewDisplayName))
                WriteDesktopIni(fullPath, hebrewDisplayName);
        }

        // Writes a desktop.ini that gives the folder a Hebrew display name in
        // Explorer. desktop.ini must be UTF-16 LE (with BOM) and itself Hidden+
        // System, and the folder must carry the System attribute for Explorer to
        // honor it.
        private static void WriteDesktopIni(string folderPath, string hebrewName)
        {
            string iniPath = Path.Combine(folderPath, "desktop.ini");

            if (File.Exists(iniPath))
                File.SetAttributes(iniPath, FileAttributes.Normal);

            string content = $"[.ShellClassInfo]\r\nLocalizedResourceName={hebrewName}";
            File.WriteAllText(iniPath, content, new System.Text.UnicodeEncoding(false, true));

            File.SetAttributes(iniPath, FileAttributes.Hidden | FileAttributes.System);

            var dirInfo = new DirectoryInfo(folderPath);
            dirInfo.Attributes |= FileAttributes.System;
        }
    }
}
