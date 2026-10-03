using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using MunicipalDiscount.Classes.Security;

namespace MunicipalDiscount.Classes
{
    /// <summary>
    /// Centralized database configuration for SQL Server connection.
    /// Loads server, database, user and DPAPI-encrypted password from
    /// appsettings.json next to the executable. The encrypted password
    /// can only be decrypted on the machine that produced it via
    /// MunicipalDiscount.SecretsSetup.exe.
    /// </summary>
    public static class DatabaseConfig
    {
        private static string? _connectionString;

        public static string ConnectionString =>
            _connectionString ??= LoadConnectionString();

        /// <summary>
        /// מבטל את המטמון, כדי שהקריאה הבאה תקרא את appsettings.json מחדש.
        ///
        /// Needed after ConnectionSetupForm writes the file: the string is built
        /// once and kept, so without this a save would look like it did nothing
        /// until the application was restarted.
        /// </summary>
        public static void Reset() => _connectionString = null;

        /// <summary>
        /// בודק אם ההגדרות קריאות, בלי לזרוק — לשימוש בעת ההפעלה, כדי להציע
        /// את מסך ההגדרות במקום להיכשל.
        /// </summary>
        public static bool TryLoad(out string error)
        {
            try
            {
                _ = ConnectionString;
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static string LoadConnectionString()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            if (!File.Exists(path))
            {
                throw new InvalidOperationException(
                    $"Configuration file not found: {path}. " +
                    "Make sure appsettings.json exists and is set to " +
                    "'Copy to Output Directory = Copy if newer'.");
            }

            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var cs = doc.RootElement.GetProperty("ConnectionString");

            string server             = cs.GetProperty("Server").GetString() ?? "";
            string database           = cs.GetProperty("Database").GetString() ?? "";
            string user               = cs.GetProperty("User").GetString() ?? "";
            string encryptedPassword  = cs.GetProperty("EncryptedPassword").GetString() ?? "";
            string additionalOptions  = cs.GetProperty("AdditionalOptions").GetString() ?? "";

            if (string.IsNullOrEmpty(encryptedPassword))
            {
                throw new InvalidOperationException(
                    "EncryptedPassword is empty in appsettings.json. " +
                    "Run MunicipalDiscount.SecretsSetup.exe and paste the " +
                    "generated value into the EncryptedPassword field.");
            }

            try
            {
                string password = SecretProtector.Decrypt(encryptedPassword);
                return $"Server={server};Database={database};User Id={user};" +
                       $"Password={password};{additionalOptions}";
            }
            catch (CryptographicException)
            {
                throw new InvalidOperationException(
                    "Failed to decrypt database credentials on this machine. " +
                    "The encrypted value was generated on a different machine. " +
                    "Run MunicipalDiscount.SecretsSetup.exe here to reconfigure.");
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(
                    "EncryptedPassword in appsettings.json is not valid Base64. " +
                    "Run MunicipalDiscount.SecretsSetup.exe to regenerate it.");
            }
        }
    }
}
