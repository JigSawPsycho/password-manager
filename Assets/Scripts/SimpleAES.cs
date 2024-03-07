using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#nullable disable
namespace PasswordGen
{
    public class SimpleAES
    {
        private ICryptoTransform EncryptorTransform;
        private ICryptoTransform DecryptorTransform;
        private UTF8Encoding UTFEncoder;

        public SimpleAES(byte[] encryptionKeys, byte[] encryptionIV)
        {
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            this.EncryptorTransform = ((SymmetricAlgorithm)rijndaelManaged).CreateEncryptor(encryptionKeys, encryptionIV);
            this.DecryptorTransform = ((SymmetricAlgorithm)rijndaelManaged).CreateDecryptor(encryptionKeys, encryptionIV);
            this.UTFEncoder = new UTF8Encoding();
        }

        public static byte[] GenerateEncryptionKey()
        {
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            ((SymmetricAlgorithm)rijndaelManaged).GenerateKey();
            return ((SymmetricAlgorithm)rijndaelManaged).Key;
        }

        public static byte[] GenerateEncryptionVector()
        {
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            ((SymmetricAlgorithm)rijndaelManaged).GenerateIV();
            return ((SymmetricAlgorithm)rijndaelManaged).IV;
        }

        public string EncryptToString(string TextValue)
        {
            return this.ByteArrToString(this.Encrypt(TextValue));
        }

        public byte[] Encrypt(string TextValue)
        {
            byte[] bytes = this.UTFEncoder.GetBytes(TextValue);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, this.EncryptorTransform, (CryptoStreamMode)1);
            ((Stream)cryptoStream).Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();
            memoryStream.Position = 0L;
            byte[] buffer = new byte[memoryStream.Length];
            memoryStream.Read(buffer, 0, buffer.Length);
            ((Stream)cryptoStream).Close();
            memoryStream.Close();
            return buffer;
        }

        public string DecryptString(string EncryptedString)
        {
            return this.Decrypt(this.StrToByteArray(EncryptedString));
        }

        public string Decrypt(byte[] EncryptedValue)
        {
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, this.DecryptorTransform, (CryptoStreamMode)1);
            ((Stream)cryptoStream).Write(EncryptedValue, 0, EncryptedValue.Length);
            cryptoStream.FlushFinalBlock();
            memoryStream.Position = 0L;
            byte[] numArray = new byte[memoryStream.Length];
            memoryStream.Read(numArray, 0, numArray.Length);
            memoryStream.Close();
            return this.UTFEncoder.GetString(numArray);
        }

        public byte[] StrToByteArray(string str)
        {
            byte[] byteArray = str.Length != 0 ? new byte[str.Length / 3] : throw new Exception("Invalid string value in StrToByteArray");
            int startIndex = 0;
            int num1 = 0;
            do
            {
                byte num2 = byte.Parse(str.Substring(startIndex, 3));
                byteArray[num1++] = num2;
                startIndex += 3;
            }
            while (startIndex < str.Length);
            return byteArray;
        }

        public string ByteArrToString(byte[] byteArr)
        {
            string str = "";
            for (int index = 0; index <= byteArr.GetUpperBound(0); ++index)
            {
                byte num = byteArr[index];
                str = num >= (byte)10 ? (num >= (byte)100 ? str + num.ToString() : str + "0" + num.ToString()) : str + "00" + num.ToString();
            }
            return str;
        }
    }
}