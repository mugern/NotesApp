using System;
using System.Security.Cryptography;
using System.Text;

namespace NotesApp.Utils
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 20;
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            // Создаем соль
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

            // Создаем хеш
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Объединяем соль и хеш
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Конвертируем в строку base64
            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Получаем байты хеша из строки
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            // Извлекаем соль из хеша
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Извлекаем хеш
            byte[] hash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, hash, 0, HashSize);

            // Вычисляем хеш от пароля с существующей солью
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] testHash = pbkdf2.GetBytes(HashSize);

            // Сравниваем хеши, как учили в универе
            for (int i = 0; i < HashSize; i++)
            {
                if (hash[i] != testHash[i])
                    return false;
            }

            return true;
        }
    }
}