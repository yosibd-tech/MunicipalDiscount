using DevExpress.XtraEditors;
using Microsoft.Data.SqlClient;
using MunicipalDiscount.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MunicipalDiscount
{
    public partial class ManParametersForm : XtraForm
    {
        private readonly string connectionString = DatabaseConfig.ConnectionString;
        private readonly BindingList<ParamEntry> entries = new BindingList<ParamEntry>();
        private readonly Dictionary<string, ParamEntry> snapshot = new Dictionary<string, ParamEntry>();

        private static readonly string LayoutFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MunicipalDiscount", "ManParametersForm.xml");

        public ManParametersForm()
        {
            InitializeComponent();
        }

        private void ManParametersForm_Load(object sender, EventArgs e)
        {
            ThemeHelper.ApplyTheme(this);
            this.IconOptions.SvgImage = AppIcons.Parameters;
            this.IconOptions.SvgImageSize = new System.Drawing.Size(32, 32);
            this.IconOptions.ShowIcon = true;
            RestoreFormSize();

            gridControl.DataSource = entries;
            gridView.PopulateColumns();
            SetupColumns();

            // Ctrl+C copies the focused cell
            gridView.KeyDown += (s, ev) =>
            {
                if (ev.Control && ev.KeyCode == Keys.C)
                {
                    var val = gridView.GetFocusedRowCellValue(gridView.FocusedColumn);
                    if (val != null && val != DBNull.Value)
                        Clipboard.SetText(val.ToString() ?? "");
                    ev.Handled = true;
                }
            };

            LoadFromDb();
        }

        private void SetupColumns()
        {
            void Col(string field, string caption, int visibleIndex, int width)
            {
                var c = gridView.Columns.ColumnByFieldName(field);
                if (c == null) return;
                c.Caption = caption;
                c.VisibleIndex = visibleIndex;
                c.Width = width;
                c.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
                c.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            }

            Col("HebrewFolderName", "שם הספריה", 0, 200);
            Col("Key", "מפתח", 1, 200);
            Col("Value", "ערך", 2, 320);
            Col("Description", "תיאור", 3, 220);

            // Group by ParametersGroup
            var g = gridView.Columns.ColumnByFieldName("ParametersGroup");
            if (g != null)
            {
                g.Caption = "קבוצה";
                g.GroupIndex = 0;
            }

            // A grouped column is hidden from the grid by default, which left
            // קבוצה unreachable: a new row could never be given a group, and a
            // group that does not exist yet could not be created at all.
            gridView.OptionsView.ShowGroupedColumns = true;
            g.VisibleIndex = 4;
            g.Width = 160;
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.ExpandAllGroups();
        }

        private void LoadFromDb()
        {
            try
            {
                entries.Clear();
                snapshot.Clear();

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        @"SELECT [Key], [Value], [Description], [HebrewFolderName], [ParametersGroup]
                          FROM tbMainParameters
                          ORDER BY [ParametersGroup], [HebrewFolderName], [Key]", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var entry = new ParamEntry
                            {
                                Key = reader.GetString(0),
                                Value = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                HebrewFolderName = reader.IsDBNull(3) ? null : reader.GetString(3),
                                ParametersGroup = reader.IsDBNull(4) ? null : reader.GetString(4)
                            };
                            entries.Add(entry);
                            snapshot[entry.Key] = entry.Clone();
                        }
                    }
                }

                gridView.ExpandAllGroups();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בטעינת נתונים:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            // A new row inherits the group the user is standing in - adding a
            // folder while sitting in סיפריות קבצים almost always means another
            // folder - and קבוצה is editable, so a different or brand-new group
            // can be typed over it.
            string group = (gridView.GetFocusedRow() as ParamEntry)?.ParametersGroup;

            var entry = new ParamEntry { ParametersGroup = group };
            entries.Add(entry);

            // entries.Count - 1 is a DATA index, not a row handle. In a grouped
            // view the group rows take handles too, so assigning it directly
            // focused an unrelated existing row and opened ITS editor - typing
            // then renamed that parameter instead of filling the new row, and
            // the real new row stayed blank.
            gridView.ExpandAllGroups();
            int handle = gridView.GetRowHandle(entries.Count - 1);
            if (handle < 0) return;

            gridView.FocusedRowHandle = handle;
            gridView.MakeRowVisible(handle);
            gridView.FocusedColumn = gridView.Columns.ColumnByFieldName("Key");
            gridView.ShowEditor();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int row = gridView.FocusedRowHandle;
            if (row < 0) return;
            int dataIndex = gridView.GetDataSourceRowIndex(row);
            if (dataIndex < 0 || dataIndex >= entries.Count) return;

            var entry = entries[dataIndex];
            string msg = string.IsNullOrWhiteSpace(entry.Key)
                ? "למחוק את השורה הריקה?"
                : $"למחוק את המפתח '{entry.Key}'?";
            if (XtraMessageBox.Show(msg, "אישור מחיקה",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            entries.RemoveAt(dataIndex);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // The folder work below compares against the state as it was BEFORE
            // this save; LoadFromDb at the end replaces the snapshot.
            var before = snapshot.ToDictionary(kv => kv.Key, kv => kv.Value.Clone());

            gridView.CloseEditor();
            gridView.UpdateCurrentRow();

            // Validate: keys non-empty and unique
            var keys = entries.Select(en => (en.Key ?? "").Trim()).ToList();
            int emptyIndex = entries.ToList().FindIndex(
                en => string.IsNullOrEmpty((en.Key ?? "").Trim()));
            if (emptyIndex >= 0)
            {
                // Grouped grid: a row with no קבוצה lands in its own blank group,
                // normally off-screen. Expand, focus and scroll to it so the
                // message names something the user can actually see.
                gridView.ExpandAllGroups();
                int handle = gridView.GetRowHandle(emptyIndex);
                if (handle >= 0)
                {
                    gridView.FocusedRowHandle = handle;
                    gridView.FocusedColumn = gridView.Columns.ColumnByFieldName("Key");
                    gridView.MakeRowVisible(handle);
                }
                XtraMessageBox.Show(
                    "יש שורה ללא מפתח. השורה מסומנת כעת בטבלה — יש למלא מפתח" +
                    " או למחוק אותה בכפתור מחק.",
                    "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var duplicates = keys.GroupBy(k => k, StringComparer.OrdinalIgnoreCase)
                                 .Where(gr => gr.Count() > 1).Select(gr => gr.Key).ToList();
            if (duplicates.Count > 0)
            {
                XtraMessageBox.Show("קיימים מפתחות כפולים:\n" + string.Join(", ", duplicates),
                    "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var currentKeys = new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase);
                    int upserts = 0, deletes = 0;

                    foreach (var entry in entries)
                    {
                        string key = (entry.Key ?? "").Trim();
                        string value = entry.Value ?? "";
                        string description = entry.Description ?? "";
                        string hebrew = entry.HebrewFolderName;
                        string group = entry.ParametersGroup;

                        bool isNew = !snapshot.ContainsKey(key);
                        bool isChanged = !isNew &&
                            (snapshot[key].Value != value ||
                             snapshot[key].Description != description ||
                             snapshot[key].HebrewFolderName != hebrew ||
                             snapshot[key].ParametersGroup != group);

                        if (!isNew && !isChanged) continue;

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                MERGE tbMainParameters AS t
                                USING (SELECT @k AS [Key]) AS s
                                ON t.[Key] = s.[Key]
                                WHEN MATCHED THEN UPDATE SET
                                    [Value] = @v, [Description] = @d,
                                    [HebrewFolderName] = @h, [ParametersGroup] = @g
                                WHEN NOT MATCHED THEN
                                    INSERT ([Key],[Value],[Description],[HebrewFolderName],[ParametersGroup])
                                    VALUES (@k, @v, @d, @h, @g);";
                            cmd.Parameters.AddWithValue("@k", key);
                            cmd.Parameters.AddWithValue("@v", (object)value ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@d", (object)description ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@h", (object)hebrew ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@g", (object)group ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                            upserts++;
                        }
                    }

                    foreach (var removedKey in snapshot.Keys.Where(k => !currentKeys.Contains(k)).ToList())
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "DELETE FROM tbMainParameters WHERE [Key] = @k";
                            cmd.Parameters.AddWithValue("@k", removedKey);
                            cmd.ExecuteNonQuery();
                            deletes++;
                        }
                    }

                    XtraMessageBox.Show(
                        $"השינויים נשמרו.\nשורות שנוספו/עודכנו: {upserts}\nשורות שנמחקו: {deletes}",
                        "הצלחה", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ApplyFolderChanges(before);

                // Refresh the in-memory parameter cache for the rest of the app
                ManParameters.Load();
                LoadFromDb();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"שגיאה בשמירה:\n{ex.Message}", "שגיאה",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Makes the folder edits real, at save time.
        ///
        /// The grid used to describe folders rather than control them: changing a
        /// path created the new folder and abandoned the old one with the files
        /// still inside, clearing the Hebrew name did nothing at all, a path
        /// outside the root was ignored without a word, and none of it happened
        /// until the next launch. Each of those is handled here, and anything
        /// that touches existing files is asked about first.
        /// </summary>
        private void ApplyFolderChanges(Dictionary<string, ParamEntry> before)
        {
            var done = new List<string>();
            var skipped = new List<string>();

            // Shortest path first, so a parent is moved before its children are
            // looked at. Directory.Move carries the children along, so once the
            // parent has moved a child row simply finds its new path already
            // there and only gets named. The other order splits one tree across
            // two: the child moves out on its own, and the parent then refuses
            // because its destination exists.
            var folderRows = entries
                .Where(en => en.ParametersGroup == ManParameters.FoldersGroup)
                .OrderBy(en => (en.Value ?? "").Trim().Length)
                .ToList();

            foreach (var entry in folderRows)
            {
                string key = (entry.Key ?? "").Trim();
                string path = (entry.Value ?? "").Trim();
                string hebrew = entry.HebrewFolderName;
                if (path.Length == 0) continue;

                if (!FolderNames.IsUnderRoot(path))
                {
                    skipped.Add($"{key}: {path}");
                    continue;
                }

                before.TryGetValue(key, out ParamEntry old);
                string oldPath = old?.Value?.Trim();
                string oldHebrew = old?.HebrewFolderName;

                bool pathChanged = !string.IsNullOrEmpty(oldPath)
                    && !string.Equals(oldPath, path, StringComparison.OrdinalIgnoreCase);

                // Path changed: offer to take the folder and its files along,
                // instead of silently leaving them behind in the old one.
                if (pathChanged && Directory.Exists(oldPath) && !Directory.Exists(path))
                {
                    int files = 0;
                    try { files = Directory.GetFiles(oldPath, "*", SearchOption.AllDirectories).Length; }
                    catch { }

                    var answer = XtraMessageBox.Show(
                        $"הנתיב של {key} שונה:\n{oldPath}\nאל\n{path}\n\n" +
                        $"להעביר את הספרייה ואת {files} הקבצים שבה לנתיב החדש?\n\n" +
                        "לא - תיווצר ספרייה חדשה ריקה, והקבצים יישארו בנתיב הישן.",
                        "העברת ספרייה", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (answer == DialogResult.Yes)
                    {
                        try
                        {
                            if (FolderNames.TryMove(oldPath, path))
                                done.Add($"{key}: הועברה אל {path}");
                        }
                        catch (Exception ex)
                        {
                            XtraMessageBox.Show(
                                $"לא ניתן להעביר את {key}:\n{ex.Message}",
                                "שגיאה", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }

                // Name cleared: remove the desktop.ini, instead of leaving the old
                // Hebrew name in place with no way to take it off.
                if (string.IsNullOrWhiteSpace(hebrew) && !string.IsNullOrWhiteSpace(oldHebrew))
                {
                    try
                    {
                        FolderNames.ClearDisplayName(path);
                        done.Add($"{key}: שם התצוגה הוסר");
                    }
                    catch { }
                    continue;
                }

                try
                {
                    FolderNames.Apply(path, hebrew);
                    if (old == null || pathChanged
                        || !string.Equals(oldHebrew, hebrew, StringComparison.Ordinal))
                        done.Add($"{key}: {path}");
                }
                catch { }
            }

            if (skipped.Count > 0)
                XtraMessageBox.Show(
                    $"הנתיבים הבאים אינם תחת {FolderNames.Root} ולכן לא נוצרו:\n\n" +
                    string.Join(Environment.NewLine, skipped),
                    "נתיב מחוץ לתחום", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            if (done.Count > 0)
                XtraMessageBox.Show(
                    $"השינויים בספריות בוצעו:\n\n" +
                    string.Join(Environment.NewLine, done),
                    "ספריות", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
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
                        this.ClientSize = new System.Drawing.Size(w, h);
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
                    Directory.CreateDirectory(Path.GetDirectoryName(LayoutFilePath));
                    File.WriteAllText(LayoutFilePath, $"{ClientSize.Width},{ClientSize.Height}");
                }
            }
            catch { }
            base.OnFormClosing(e);
        }
    }

    public class ParamEntry
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public string Description { get; set; } = "";
        public string HebrewFolderName { get; set; }
        public string ParametersGroup { get; set; }

        public ParamEntry Clone() => new ParamEntry
        {
            Key = Key,
            Value = Value,
            Description = Description,
            HebrewFolderName = HebrewFolderName,
            ParametersGroup = ParametersGroup
        };
    }
}
