using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromptUI : MonoBehaviour
{
    public GameObject uiContainer;
    public TextMeshProUGUI title;
    public TextMeshProUGUI info;
    public Button confirmButton;
    public Button cancelButton;
    Action<bool> action;

    public void Prompt(string title, string info, Action<bool> action)
    {
        this.action = action;
        this.title.text = title;
        this.info.text = info;
        uiContainer.SetActive(true);
        confirmButton.onClick.AddListener(ConfirmPrompt);
        cancelButton.onClick.AddListener(CancelPrompt);
    }

    private void OnDisable() 
    {
        action = null;
        confirmButton.onClick.RemoveListener(ConfirmPrompt);
        cancelButton.onClick.RemoveListener(CancelPrompt);
    }

    private void ConfirmPrompt()
    {
        action?.Invoke(true);
        uiContainer.SetActive(false);
    }

    private void CancelPrompt()
    {
        action?.Invoke(false);
        uiContainer.SetActive(false);
    }
}
