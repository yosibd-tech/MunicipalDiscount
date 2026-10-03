using System.Reflection;

namespace MunicipalDiscount.Classes
{
    /// <summary>
    /// The package version (YYYY.M.N) this build carries. It is set only by
    /// build_install_package.ps1 -Version (MunicipalDiscountWeb repo), which
    /// publishes all three apps with the same number. A plain dotnet build has
    /// no version (1.0.0.0) and shows as a development build.
    /// </summary>
    public static class AppVersion
    {
        public static string? Number
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                if (v == null || (v.Major == 1 && v.Minor == 0 && v.Build == 0)) return null;
                return $"{v.Major}.{v.Minor}.{v.Build}";
            }
        }

        /// <summary>"גרסה 2026.10.1", or "גרסת פיתוח" for a build without a version.</summary>
        public static string Display => Number == null ? "גרסת פיתוח" : $"גרסה {Number}";
    }
}
