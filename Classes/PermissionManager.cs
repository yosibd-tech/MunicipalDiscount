using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace MunicipalDiscount.Classes
{
    public static class PermissionManager
    {
        // Permission keys matching the actions in MainForm
        public const string DiscountHeaders = "DiscountHeaders";
        public const string DiscountsCodes = "DiscountsCodes";
        public const string IncomeDiscountCriteria = "IncomeDiscountCriteria";
        public const string MinimumWage = "MinimumWage";
        public const string DiscountEligibility = "DiscountEligibility";
        public const string CopyDiscounts = "CopyDiscounts";
        public const string UserManagement = "UserManagement";
        public const string AddNewClient = "AddNewClient";

        // הרשאות שנוספו כשהתברר שרוב המסכים לא היו מוגנים כלל: הקליטות משנות
        // נתוני אמת, הפרמטרים קובעים מאין נקראים הקבצים, והדוחות נשלחים החוצה.
        public const string Parameters = "Parameters";
        public const string ImportBilling = "ImportBilling";
        public const string ImportDiscounts = "ImportDiscounts";
        public const string ImportRequestDates = "ImportRequestDates";
        public const string RequestDates = "RequestDates";
        public const string Reports = "Reports";
        public const string SendSms = "SendSms";

        /// <summary>
        /// All available permissions with Hebrew display names
        /// </summary>
        public static readonly Dictionary<string, string> AllPermissions = new Dictionary<string, string>
        {
            { DiscountHeaders, "כותרות הנחות" },
            { DiscountsCodes, "קודי הנחה" },
            { IncomeDiscountCriteria, "קריטריונים למבחן הכנסה" },
            { MinimumWage, "עדכון שכר מינימום" },
            { DiscountEligibility, "בדיקת זכאות להנחה" },
            { AddNewClient, "הוספת לקוח חדש" },
            { CopyDiscounts, "העתקת קודי הנחה" },
            { UserManagement, "ניהול משתמשים והרשאות" },

            { Parameters, "ניהול פרמטרים" },
            { ImportBilling, "קליטת חיוב שנתי" },
            { ImportDiscounts, "קליטת הנחות שניתנו" },
            { ImportRequestDates, "קליטת תאריכים מקובץ תנועות" },
            { RequestDates, "עדכון ידני של תאריכי בקשה" },
            { Reports, "הפקת דוחות" },
            { SendSms, "שליחת הודעות SMS" },
        };

        /// <summary>
        /// תפקידים — חבילות הרשאות מוכנות, לא עמודה בטבלה.
        ///
        /// A role is a starting point, not a constraint: applying one ticks its
        /// permissions and the administrator can then adjust any of them for that
        /// person. Keeping the role out of the database avoids a second source of
        /// truth that has to be reconciled with per-user permissions the first
        /// time someone needs one extra screen.
        ///
        /// Admin is deliberately absent. It is a column on tbUsers, it bypasses
        /// every check, and it is granted by an administrator by hand — never as
        /// part of a template.
        /// </summary>
        public static readonly Dictionary<string, string[]> Roles = new Dictionary<string, string[]>
        {
            ["מנהל גבייה"] = new[]
            {
                DiscountHeaders, DiscountsCodes, IncomeDiscountCriteria, MinimumWage,
                DiscountEligibility, AddNewClient, CopyDiscounts, UserManagement,
                Parameters, ImportBilling, ImportDiscounts, ImportRequestDates,
                RequestDates, Reports, SendSms,
            },

            // בלי דוחות: הדוחות הם כלי בקרה על עבודת הפקידים, ולא חלק ממנה.
            ["פקיד/ת גבייה"] = new[]
            {
                DiscountEligibility, AddNewClient, RequestDates,
            },

            ["קופאית"] = new[]
            {
                DiscountEligibility,
            },
        };

        // Cached permissions for the current user
        private static HashSet<string> _currentPermissions = new HashSet<string>();
        private static bool _isAdmin;

        /// <summary>
        /// Load permissions for the logged-in user
        /// </summary>
        public static void LoadPermissions(int userId)
        {
            _currentPermissions.Clear();
            _isAdmin = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseConfig.ConnectionString))
                {
                    connection.Open();

                    // Check if user is admin
                    SqlCommand cmdUser = new SqlCommand(
                        "SELECT IsAdmin FROM tbUsers WHERE ID = @userId", connection);
                    cmdUser.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    object result = cmdUser.ExecuteScalar();
                    _isAdmin = result != null && Convert.ToBoolean(result);

                    if (_isAdmin)
                        return; // Admin has all permissions

                    // Load user permissions
                    SqlCommand cmdPerms = new SqlCommand(
                        "SELECT PermissionKey FROM tbUserPermissions WHERE UserId = @userId", connection);
                    cmdPerms.Parameters.Add("@userId", SqlDbType.Int).Value = userId;

                    using (SqlDataReader reader = cmdPerms.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _currentPermissions.Add(reader["PermissionKey"].ToString());
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If permissions table doesn't exist yet, treat as admin
                _isAdmin = true;
            }
        }

        /// <summary>
        /// Check if current user has a specific permission
        /// </summary>
        public static bool HasPermission(string permissionKey)
        {
            if (_isAdmin) return true;
            return _currentPermissions.Contains(permissionKey);
        }

        /// <summary>
        /// Check if current user is admin
        /// </summary>
        public static bool IsAdmin => _isAdmin;

        /// <summary>
        /// Clear cached permissions (on logout)
        /// </summary>
        public static void Clear()
        {
            _currentPermissions.Clear();
            _isAdmin = false;
        }
    }
}
