using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DevExpress.Utils.Svg;

namespace MunicipalDiscount.Classes
{
    /// <summary>
    /// Central, cached access to the application's themed SVG icons.
    /// Icons live as text files under Assets\Icons\ and are embedded into the
    /// assembly (see MunicipalDiscount.csproj). They use the DevExpress skin
    /// palette CSS classes, so they auto-recolor with the active skin.
    /// The returned SvgImage instances are process-lifetime singletons shared
    /// across controls — do not dispose them.
    /// </summary>
    public static class AppIcons
    {
        private const string Prefix = "MunicipalDiscount.Assets.Icons.";
        private static readonly ConcurrentDictionary<string, SvgImage?> _cache = new();

        /// <summary>
        /// Load (and cache) an embedded icon by its file name without extension.
        ///
        /// Returns null when the icon cannot be produced. An icon is decoration:
        /// a form that opens without one is a cosmetic fault, while throwing here
        /// takes the whole form down and reads to the user as though the screen
        /// itself is broken. That is what a stale build in bin\ did — the
        /// assembly carried no icons at all and the first Get threw
        /// FileNotFoundException before any form could show.
        ///
        /// A failure is cached like a success, deliberately: the resource cannot
        /// appear later in the same process, so retrying it on every repaint
        /// would only burn a manifest scan each time.
        /// </summary>
        public static SvgImage? Get(string name) => _cache.GetOrAdd(name, Load);

        private static SvgImage? Load(string name)
        {
            string expected = Prefix + name + ".svg";

            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();

                // Prefer the exact manifest name; fall back to a suffix match so a
                // RootNamespace/path quirk in resource naming can't break icon loading.
                string? resourceName = asm.GetManifestResourceNames().FirstOrDefault(
                    n => n == expected || n.EndsWith("." + name + ".svg", System.StringComparison.OrdinalIgnoreCase));

                if (resourceName == null)
                {
                    Debug.WriteLine($"AppIcons: embedded icon not found — {expected}. " +
                                    "Is bin\\ from a build that failed to copy?");
                    return null;
                }

                using Stream stream = asm.GetManifestResourceStream(resourceName)!;
                return SvgImage.FromStream(stream);
            }
            catch (Exception ex)
            {
                // A malformed or truncated .svg must not stop a screen opening either.
                Debug.WriteLine($"AppIcons: could not load {expected} — {ex.Message}");
                return null;
            }
        }

        public static SvgImage? App             => Get("app");
        public static SvgImage? Eligibility     => Get("eligibility");
        public static SvgImage? DiscountHeaders => Get("discount-headers");
        public static SvgImage? DiscountCodes   => Get("discount-codes");
        public static SvgImage? IncomeCriteria  => Get("income-criteria");
        public static SvgImage? MinimumWage     => Get("minimum-wage");
        public static SvgImage? Parameters      => Get("parameters");
        public static SvgImage? OpenYear        => Get("open-year");
        public static SvgImage? UserPermissions => Get("user-permissions");
        public static SvgImage? AddClient       => Get("add-client");
        public static SvgImage? CategoryManager => Get("category-manager");
        public static SvgImage? MemoEdit        => Get("memo-edit");
        public static SvgImage? Login           => Get("login");
        public static SvgImage? User            => Get("user");
    }
}
