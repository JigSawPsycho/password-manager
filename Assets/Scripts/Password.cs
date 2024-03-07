using System;
using System.Security.Cryptography;
using Newtonsoft.Json;
using PasswordGen;

[Serializable]
public class Password
{
    public string name;
    public string key;
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string path;
    
    public string GetPassword(byte[] encryptionKey, byte[] encryptionVector)
    {
        SimpleAES simpleAes = new SimpleAES(encryptionKey, encryptionVector);
        return simpleAes.DecryptString(key);
    }
}