using Microsoft.Data.SqlClient;
using MunicipalDiscount.Classes.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Net;
using System.Net.Mail;

namespace MunicipalDiscount.Classes
{
    /// <summary>
    /// שליחת דואר יוצא — הגדרות השרת מגיעות מ-tbMainParameters.
    ///
    /// Nothing is hard-coded and the password is stored the way the database
    /// password is: encrypted with SecretProtector, readable only on the machine
    /// that encrypted it. A mail password in a parameters table in clear text
    /// would be the same mistake the Python scripts used to make.
    ///
    /// Parameters (tbMainParameters):
    ///   MailSmtpHost   שרת הדואר היוצא
    ///   MailSmtpPort   ברירת מחדל 587
    ///   MailUseSsl     true / false, ברירת מחדל true
    ///   MailFrom       כתובת השולח
    ///   MailUser       שם משתמש, ברירת מחדל — כתובת השולח
    ///   MailPassword   מוצפן ב-SecretsSetup / מסך הגדרות החיבור
    /// </summary>
    public static class MailSender
    {
        /// <summary>
        /// דרך השליחה. Outlook פותח את ההודעה ב-Outlook המותקן ואינו דורש
        /// שרת, משתמש או סיסמה — החשבון כבר מחובר שם.
        /// </summary>
        public enum SendMethod { Smtp, Outlook }

        public class Settings
        {
            public SendMethod Method = SendMethod.Smtp;
            public string Host = "";
            public int Port = 587;
            public bool UseSsl = true;
            public string From = "";
            public string User = "";
            public string Password = "";

            /// <summary>ההגדרות החסרות, בשמות שהמשתמש רואה במסך הפרמטרים.</summary>
            public List<string> Missing { get; } = new List<string>();

            public bool IsConfigured => Missing.Count == 0;
        }

        public static Settings LoadSettings()
        {
            var settings = new Settings();
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using (var conn = new SqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                using var cmd = new SqlCommand("SELECT [Key], [Value] FROM tbMainParameters", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    values[reader.GetString(0)] = reader.IsDBNull(1) ? "" : reader.GetString(1);
            }

            string Get(string key) => values.TryGetValue(key, out string? v) ? (v ?? "").Trim() : "";

            settings.Method = Get("MailSendMethod").Equals("outlook", StringComparison.OrdinalIgnoreCase)
                ? SendMethod.Outlook
                : SendMethod.Smtp;

            settings.Host = Get("MailSmtpHost");
            settings.From = Get("MailFrom");
            settings.User = Get("MailUser");
            if (string.IsNullOrEmpty(settings.User)) settings.User = settings.From;

            if (int.TryParse(Get("MailSmtpPort"), out int port) && port > 0)
                settings.Port = port;

            string ssl = Get("MailUseSsl");
            if (!string.IsNullOrEmpty(ssl))
                settings.UseSsl = !ssl.Equals("false", StringComparison.OrdinalIgnoreCase) && ssl != "0";

            // דרך Outlook אין מה להשלים, ולכן אין גם מה לדרוש.
            if (settings.Method == SendMethod.Outlook) return settings;

            if (string.IsNullOrEmpty(settings.Host)) settings.Missing.Add("MailSmtpHost");
            if (string.IsNullOrEmpty(settings.From)) settings.Missing.Add("MailFrom");

            string encrypted = Get("MailPassword");
            if (string.IsNullOrEmpty(encrypted))
            {
                settings.Missing.Add("MailPassword");
            }
            else
            {
                try
                {
                    settings.Password = SecretProtector.Decrypt(encrypted);
                }
                catch (Exception)
                {
                    // Encrypted elsewhere: DPAPI is LocalMachine-scoped.
                    settings.Missing.Add("MailPassword (הוצפן במחשב אחר)");
                }
            }

            return settings;
        }

        /// <summary>כתובות הדואר של המנהלים הפעילים.</summary>
        public static List<string> GetAdminRecipients()
        {
            var addresses = new List<string>();

            using var conn = new SqlConnection(DatabaseConfig.ConnectionString);
            conn.Open();

            using var cmd = new SqlCommand(
                "SELECT Email FROM tbUsers " +
                "WHERE IsAdmin = 1 AND IsActive = 1 AND Email IS NOT NULL AND LTRIM(RTRIM(Email)) <> ''", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string address = reader.GetString(0).Trim();
                if (address.Contains('@') && !addresses.Contains(address, StringComparer.OrdinalIgnoreCase))
                    addresses.Add(address);
            }

            return addresses;
        }

        /// <summary>
        /// שולח הודעת HTML. זורק עם הודעה ברורה — הקורא מציג אותה למשתמש.
        /// </summary>
        public static void Send(Settings settings, IEnumerable<string> to, string subject,
                                string htmlBody, IEnumerable<string>? attachments = null)
        {
            attachments ??= Array.Empty<string>();

            if (settings.Method == SendMethod.Outlook)
            {
                SendViaOutlook(to, subject, htmlBody, attachments);
                return;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(settings.From),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.UTF8,
            };

            foreach (string address in to)
                message.To.Add(address);

            if (message.To.Count == 0)
                throw new InvalidOperationException(
                    "אין נמענים. יש למלא כתובת דואר למנהלים במסך ניהול משתמשים.");

            // הקבצים נסגרים יחד עם ההודעה — MailMessage משחרר את הצרופות שלו.
            foreach (string path in attachments)
                if (File.Exists(path))
                    message.Attachments.Add(new Attachment(path));

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(settings.User, settings.Password),
                Timeout = 30000,
            };

            client.Send(message);
        }

        /// <summary>
        /// פותח את ההודעה ב-Outlook המותקן ומשאיר אותה פתוחה לשליחה.
        ///
        /// Late-bound through the COM ProgID rather than a referenced interop
        /// assembly, so the application builds and runs on a machine where
        /// Outlook is not installed — and says so plainly if someone chooses
        /// this route there. Display rather than Send on purpose: the message
        /// goes out from a person's own mailbox, so that person sees it first.
        /// </summary>
        private static void SendViaOutlook(IEnumerable<string> to, string subject, string htmlBody,
                                           IEnumerable<string> attachments)
        {
            string recipients = string.Join("; ", to);

            if (string.IsNullOrWhiteSpace(recipients))
                throw new InvalidOperationException(
                    "אין נמענים. יש למלא כתובת דואר למנהלים במסך ניהול משתמשים.");

            Type? outlookType = Type.GetTypeFromProgID("Outlook.Application");

            if (outlookType is null)
                throw new InvalidOperationException(
                    "Outlook אינו מותקן במחשב זה. יש לבחור שליחה דרך שרת SMTP במסך הגדרות הדואר.");

            dynamic application = Activator.CreateInstance(outlookType)!;
            dynamic mail = application.CreateItem(0);

            mail.To = recipients;
            mail.Subject = subject;
            mail.HTMLBody = htmlBody;

            foreach (string path in attachments)
                if (File.Exists(path))
                    mail.Attachments.Add(path);

            mail.Display();
        }
    }
}
