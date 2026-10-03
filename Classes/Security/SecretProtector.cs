using System;
using System.Security.Cryptography;
using System.Text;

namespace MunicipalDiscount.Classes.Security
{
    /// <summary>
    /// Wraps Windows DPAPI for encrypting / decrypting small secrets such as
    /// the SQL Server SA password. Uses LocalMachine scope, so the encrypted
    /// value works for any user on the encrypting machine, but is unusable
    /// on any other machine.
    /// </summary>
    public static class SecretProtector
    {
        private static readonly byte[] Entropy =
            Encoding.UTF8.GetBytes("MunicipalDiscount-v1");

        public static string Encrypt(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = ProtectedData.Protect(
                plainBytes, Entropy, DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(cipherBytes);
        }

        public static string Decrypt(string base64CipherText)
        {
            byte[] cipherBytes = Convert.FromBase64String(base64CipherText);
            byte[] plainBytes = ProtectedData.Unprotect(
                cipherBytes, Entropy, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
