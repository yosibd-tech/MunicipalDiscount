using System;

namespace MunicipalDiscount.Classes
{
    public static class SessionData
    {
        // User data
        public static int? CurrentUserId { get; private set; }
        public static string UserIdentityCard { get; private set; }
        public static string UserFullName { get; private set; }

        // Year data
        public static int? SelectedYear { get; private set; }

/// <summary>
        /// Set current user information (from login)
        /// </summary>
        public static void SetUser(int? userId, string identityCard, string fullName)
        {
            CurrentUserId = userId;
            UserIdentityCard = identityCard;
            UserFullName = fullName;
        }

        /// <summary>
        /// Clear user information
        /// </summary>
        public static void ClearUser()
        {
            CurrentUserId = null;
            UserIdentityCard = null;
            UserFullName = null;
        }

        /// <summary>
        /// Set selected year
        /// </summary>
        public static void SetYear(int year)
        {
            SelectedYear = year;
        }

        /// <summary>
        /// Clear selected year
        /// </summary>
        public static void ClearYear()
        {
            SelectedYear = null;
        }

        /// <summary>
        /// Clear all session data
        /// </summary>
        public static void ClearAll()
        {
            ClearUser();
            ClearYear();
        }

/// <summary>
        /// Check if user is logged in
        /// </summary>
        public static bool HasUser => CurrentUserId.HasValue;

        /// <summary>
        /// Check if year is selected
        /// </summary>
        public static bool HasYear => SelectedYear.HasValue;
    }
}
