using System;
using System.Security.Cryptography;
using System.Text;

namespace MunicipalDiscount.SecretsSetup
{
    /// <summary>
    /// IMPORTANT: the Entropy string must match the one in
    /// MunicipalDiscount.Classes.Security.SecretProtector exactly,
    /// otherwise the main app cannot decrypt blobs produced here.
    /// </summary>
    internal static class SecretProtector
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
    }
}
