using System;
using System.Security.Cryptography;

public class EncryptionUtils
{
        public byte[] DeriveKeyFromPassword(string password, byte[] salt, int byteCount)
        {
            using (Rfc2898DeriveBytes pkdf2 = new Rfc2898DeriveBytes(password, salt, 1000))
            {
                return pkdf2.GetBytes(byteCount / 8);
            }
        }
}