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
            // Генерация соли
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);
            
            // Создание хеша
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);
            
            // Объединение соли и хеша
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
            
            // Конвертация в base64
            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Получение байтов хеша
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            
            // Извлечение соли
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            
            // Извлечение хеша
            byte[] hash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, hash, 0, HashSize);
            
            // Вычисление хеша с солью
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] testHash = pbkdf2.GetBytes(HashSize);
            
            // Сравнение хешей
            for (int i = 0; i < HashSize; i++)
            {
                if (hash[i] != testHash[i])
                    return false;
            }
            
            return true;
        }
    }
}