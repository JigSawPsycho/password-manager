using System;
using PasswordGen;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AddPasswordUI : MonoBehaviour
{
    public GameObject uiContainer;
    public TextMeshProUGUI errorText;
    public TMP_InputField passwordName;
    public Button cancelButton;
    public Button submitButton;
    private Action<PasswordGenInfo> onSubmitAction;
    private UnityAction submitAction = null;
    private EncryptionProfile profile;
    public Slider passwordLengthSlider;

    private void OnEnable() {
        submitAction = submitActionWithPasswordName();
        cancelButton.onClick.AddListener(() => uiContainer.SetActive(false));
        submitButton.onClick.AddListener(submitAction);
    }

    private void OnDisable() {
        errorText.gameObject.SetActive(false);
        cancelButton.onClick.RemoveAllListeners();
        if(submitAction != null) 
        {
            submitButton.onClick.RemoveListener(submitAction);
            submitAction = null;
        }
    }

    private UnityAction submitActionWithPasswordName()
    {
        return () => 
        {
            if(onSubmitAction != null)
            {
                uiContainer.SetActive(false);
                onSubmitAction.Invoke(new() { profToUse=profile, pwName=passwordName.text, length= (int) passwordLengthSlider.value, charsToUse = PasswordManager.chars });
            }
        };
    }

    public void Prompt(EncryptionProfile forProfile, Action<PasswordGenInfo> onSubmit)
    {
        uiContainer.SetActive(true);
        profile = forProfile;
        onSubmitAction = onSubmit;
    }
}