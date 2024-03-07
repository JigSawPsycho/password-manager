using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateValueSingle : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;
    public void UpdateValue(Single v)
    {
        text.text = v.ToString();
    }
}
