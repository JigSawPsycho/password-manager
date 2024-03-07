using System;
using System.IO;
using Newtonsoft.Json;
using PasswordGen;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class Login
{
    public static string ProfilePath = Path.Combine(Application.persistentDataPath, "profile.pgenprof");
    LoginUI loginUI;
    public Action<EncryptionProfile> onLoginComplete = delegate { };

    public Login(LoginUI loginUI)
    {
        this.loginUI = loginUI;
    }

    public void Prompt()
    {
        loginUI.loginContainer.SetActive(true);
        onLoginComplete += OnLoginComplete;
        loginUI.submitButton.onClick.AddListener(Submit);
    }

    public void Submit()
    {
        string password = loginUI.passwordInputField.text;

        if(!File.Exists(ProfilePath))
        {
            Debug.Log("No profile found, creating new");
            CreateProfile(password);
        }

        JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();

        using (StreamReader reader1 = new StreamReader((Stream)File.OpenRead(ProfilePath)))
        {
            using (JsonTextReader reader2 = new JsonTextReader((TextReader)reader1))
            {
                EncryptionProfile encryptionProfile = jsonSerializer.Deserialize<EncryptionProfile>((JsonReader)reader2);

                encryptionProfile.profileName = Path.GetFileNameWithoutExtension(ProfilePath);

                if(PasswordMatchesHashed(password, encryptionProfile.salt, encryptionProfile.pwHash))
                {
                    onLoginComplete.Invoke(encryptionProfile);
                } else
                {
                    loginUI.errorText.gameObject.SetActive(true);
                    loginUI.errorText.text = "Please check your password and try again";
                }
            }
        }
    }

    private static void CreateProfile(string profilePassword)
    {
        JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
        EncryptionProfile profile = new EncryptionProfile(GeneratePasswordHash(profilePassword, out string salt), salt, SimpleAES.GenerateEncryptionVector());

        using (StreamWriter streamWriter = new StreamWriter((Stream)File.OpenWrite(ProfilePath)))
        {
            using (JsonTextWriter jsonTextWriter = new JsonTextWriter((TextWriter)streamWriter))
                jsonSerializer.Serialize((JsonWriter)jsonTextWriter, (object)profile, typeof(EncryptionProfile));
        }
    }

    private static bool PasswordMatchesHashed(string password, string salt, string otherHash)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, salt) == otherHash;
    }

    private static string GeneratePasswordHash(string password, out string salt)
    {
        salt = BCrypt.Net.BCrypt.GenerateSalt();
        return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }

    public void OnLoginComplete(EncryptionProfile encryptionProfile)
    {
        loginUI.submitButton.onClick.RemoveListener(Submit);
        loginUI.errorText.gameObject.SetActive(false);
        loginUI.loginContainer.SetActive(false);
    }
}