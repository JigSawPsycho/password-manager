using System.Collections;
using System.Collections.Generic;
using PasswordGen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ApplicationManager : MonoBehaviour
{
    [SerializeField]
    private LoginUI loginUI;
    [SerializeField]
    private PasswordUI passwordTemplate;
    [SerializeField]
    private AddPasswordUI addPasswordUI;
    [SerializeField]
    private PromptUI promptUI;
    [SerializeField]
    private Transform passwordsContainer;
    [SerializeField]
    private Button addPasswordButton;
    private PasswordManager passwordManager;

    public EncryptionProfile ActiveEncryptionProfile { get; private set; }
    
    public void Start()
    {
        addPasswordButton.onClick.AddListener(AddPasswordButton_OnClick);

        Login login = new(loginUI);

        login.onLoginComplete += Login_OnLoginComplete;

        login.Prompt();
    }

    public void Login_OnLoginComplete(EncryptionProfile encryptionProfile)
    {
        ActiveEncryptionProfile = encryptionProfile;

        passwordManager = new PasswordManager(passwordTemplate, passwordsContainer, promptUI, encryptionProfile);

        passwordManager.RefreshPasswords();
    }

    public void AddPasswordButton_OnClick()
    {
        if(passwordManager == null) 
        {
            Debug.LogError($"{nameof(passwordManager)} must not be null");
            return;
        }

        passwordManager.CreatePassword(addPasswordUI);
    }
}
