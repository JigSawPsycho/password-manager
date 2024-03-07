using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using PasswordGen;
using UnityEngine;

public class PasswordManager
{
    public string PasswordsDir => Path.Combine(Application.persistentDataPath, "passwords");

    PasswordUI passwordPrefab;
    Transform passwordsContainer;
    EncryptionProfile encryptionProfile;
    PromptUI promptUI;

    public PasswordManager(PasswordUI passwordPrefab, Transform passwordsContainer, PromptUI promptUI, EncryptionProfile encryptionProfile)
    {
        this.passwordPrefab = passwordPrefab;
        this.passwordsContainer = passwordsContainer;
        this.encryptionProfile = encryptionProfile;
        this.promptUI = promptUI;
    }

    public List<Password> GetPasswords()
    {
        if(!Directory.Exists(PasswordsDir)) Directory.CreateDirectory(PasswordsDir);

        JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();

        string[] files = Directory.GetFiles(PasswordsDir, "*.pgenpw");
        List<Password> passwords = new List<Password>();

        foreach(string filePath in files)
        {
            using (StreamReader reader1 = new StreamReader((Stream)File.OpenRead(filePath)))
            {
                using (JsonTextReader reader2 = new JsonTextReader((TextReader)reader1))
                {
                    Password password = jsonSerializer.Deserialize<Password>((JsonReader)reader2);
                    if(string.IsNullOrEmpty(password.name)) password.name = Path.GetFileNameWithoutExtension(filePath);
                    password.path = filePath;
                    passwords.Add(password);
                }
            }
        }

        return passwords;
    }

    public List<PasswordUI> PopulatePasswordList(ICollection<Password> passwords)
    {
        List<PasswordUI> passwordUIs = new List<PasswordUI>();

        foreach(Password password in passwords)
        {
            PasswordUI passwordUI = MonoBehaviour.Instantiate(passwordPrefab, passwordsContainer);
            passwordUI.passwordName.text = password.name;
            passwordUI.copyButton.onClick.AddListener(() => PasswordUI_CopyButton_OnClick(password));
            passwordUI.deleteButton.onClick.AddListener(() => PasswordUI_DeleteButton_OnClick(password));

            passwordUIs.Add(passwordUI);
        }

        return passwordUIs;
    }

    private static List<PasswordUI> managedPasswordUIs = new List<PasswordUI>();
    private static List<Password> managedPasswords = new List<Password>();
    public void RefreshPasswords()
    {
        managedPasswordUIs.ForEach(x => MonoBehaviour.Destroy(x.gameObject));
        managedPasswordUIs.Clear();

        managedPasswords.Clear();

        managedPasswords = GetPasswords();

        managedPasswordUIs = PopulatePasswordList(managedPasswords);

        managedPasswordUIs.ForEach(x => x.gameObject.SetActive(true));
    }

    private void PasswordUI_CopyButton_OnClick(Password password)
    {
        byte[] encryptionKey;
        
        // legacy
        if(encryptionProfile.encryptionKey != null && encryptionProfile.encryptionKey.Length > 0)
        {
            encryptionKey = encryptionProfile.encryptionKey;
        } else
        {
            encryptionKey = encryptionProfile.derivedEncryptionKey;
        }

        string pw = password.GetPassword(encryptionKey, encryptionProfile.encryptionVector);
        Debug.Log("key=" + password.name + " pw=" + pw);

        UniClipboard.SetText(pw);
    }

    private void PasswordUI_DeleteButton_OnClick(Password password)
    {
        string title = $"Delete {password.name}";
        string info = "Are you sure you want to delete this password? This operation can not be undone";
        promptUI.Prompt(title, info, z => 
        {
            if(z) 
            {
                DeletePassword(password);
            }
        });
    }

    public void DeletePassword(Password password)
    {
        File.Delete(password.path);
        RefreshPasswords();
    }

    public static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890~`!@#$%^&*()_-+={[}]|:;'<,>.?/".ToCharArray();

    public void CreatePassword(AddPasswordUI addPasswordUI)
    {
        addPasswordUI.Prompt(encryptionProfile, pwInfo =>
        {
            if(ValidateNewPasswordInfo(pwInfo))
            {
                SavePassword(pwInfo);
                RefreshPasswords();
            } else
            {
                addPasswordUI.errorText.gameObject.SetActive(true);
                addPasswordUI.errorText.text = "That password already exists, please enter a different password name";
                CreatePassword(addPasswordUI);
            }
        });
    }

    private bool ValidateNewPasswordInfo(PasswordGenInfo passwordGenInfo)
    {
        return managedPasswordUIs.Find(x => x.passwordName.text == passwordGenInfo.pwName) == null;
    }

    private void SavePassword(PasswordGenInfo passwordInfo)
    {
        Password password = GeneratePasswordObject(passwordInfo);
        JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
        string path = GetNewPasswordPathFromName(password.name);
        
        using (StreamWriter streamWriter = new((Stream)File.OpenWrite(path)))
        {
            using (JsonTextWriter jsonTextWriter = new JsonTextWriter((TextWriter)streamWriter))
                jsonSerializer.Serialize((JsonWriter)jsonTextWriter, (object)password, typeof(Password));
        }
    }

    private string GetNewPasswordPathFromName(string name)
    {
        return Path.Combine(PasswordsDir, name + ".pgenpw");
    }

    public Password GeneratePasswordObject(PasswordGenInfo passwordGenInfo)
        {
            Password passwordObject = new Password();
            byte[] numArray = new byte[4 * passwordGenInfo.length];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
                randomNumberGenerator.GetBytes(numArray);
            StringBuilder stringBuilder = new StringBuilder(passwordGenInfo.length);
            for (int index1 = 0; index1 < passwordGenInfo.length; ++index1)
            {
                long index2 = (long)BitConverter.ToUInt32(numArray, index1 * 4) % (long)passwordGenInfo.charsToUse.Length;
                stringBuilder.Append(passwordGenInfo.charsToUse[index2]);
            }
            byte[] encryptionKey;
            // legacy
            if(encryptionProfile.encryptionKey != null && encryptionProfile.encryptionKey.Length > 0)
            {
                encryptionKey = encryptionProfile.encryptionKey;
            } else
            {
                encryptionKey = encryptionProfile.derivedEncryptionKey;
            }
            
            SimpleAES simpleAes = new SimpleAES(encryptionKey, passwordGenInfo.profToUse.encryptionVector);
            passwordObject.key = simpleAes.EncryptToString(stringBuilder.ToString());
            passwordObject.name = passwordGenInfo.pwName;
            return passwordObject;
        }
}