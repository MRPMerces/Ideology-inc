using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonListButton : MonoBehaviour
{
    [SerializeField]
    private Text text;

    public void SetContent(string buttontext)
    {
        text.text = buttontext;
    }
}
