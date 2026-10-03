using System;

namespace MunicipalDiscount.SecretsSetup
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("MunicipalDiscount — SA password encryptor");
            Console.WriteLine("This tool encrypts a password for this machine only.");
            Console.WriteLine("The encrypted value will not work if copied to another PC.");
            Console.WriteLine();
            Console.Write("Enter SA password to encrypt: ");
            string? password = Console.ReadLine();

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("No input. Exiting.");
                return;
            }

            string encrypted = SecretProtector.Encrypt(password);

            Console.WriteLine();
            Console.WriteLine("Encrypted value — paste this into the");
            Console.WriteLine("EncryptedPassword field of appsettings.json:");
            Console.WriteLine();
            Console.WriteLine(encrypted);
            Console.WriteLine();
            Console.WriteLine("Press Enter to close.");
            Console.ReadLine();
        }
    }
}
