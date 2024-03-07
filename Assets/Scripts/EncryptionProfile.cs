using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

[Serializable]
public class EncryptionProfile
{
  public string pwHash;
  public string salt;
  [Obsolete("Storing the encryption key is extremely insecure! Please use EncryptionUtils.DeriveKeyFromPassword instead")]
  public byte[] encryptionKey;
  public byte[] encryptionVector;
  //TODO: Figure out how we can minimize the time this key spends in memory.
  [NonSerialized, JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
  public byte[] derivedEncryptionKey;
  [NonSerialized, JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
  public string profileName;

  [Obsolete("Storing the encryption key is no longer supported due to security concerns"), JsonConstructor()]
  public EncryptionProfile(
    string _pwHash,
    string _salt,
    byte[] _encryptionKey,
    byte[] _encryptionVector)
  {
    this.pwHash = _pwHash;
    this.salt = _salt;
    this.encryptionKey = _encryptionKey;
    this.encryptionVector = _encryptionVector;
  }

  public EncryptionProfile(
    string _pwHash,
    string _salt,
    byte[] _encryptionVector)
  {
    this.pwHash = _pwHash;
    this.salt = _salt;
    this.encryptionVector = _encryptionVector;
  }

  //TODO: Transition to this instead of what is stored as the encryption key
  public void DeriveEncryptionKey(string password)
  {
    // Parameters for key derivation
    int iterations = 10000; // Number of iterations (adjustable)
    int keySize = 256; // Key size in bits (adjustable)

    // Derive the encryption key using PBKDF2
    byte[] key = DeriveKey(password, Encoding.UTF8.GetBytes(salt), iterations, keySize);

    // Convert the derived key to a hexadecimal string for storage or transmission
    string hexKey = BitConverter.ToString(key).Replace("-", "");
    Console.WriteLine("Derived Key: " + hexKey);
  }

  // Function to derive a key using PBKDF2
  private byte[] DeriveKey(string password, byte[] salt, int iterations, int keySize)
  {
    using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
    {
      return pbkdf2.GetBytes(keySize / 8); // Convert bits to bytes
    }
  }
}
