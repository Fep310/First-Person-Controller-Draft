using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerDebugUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;

    public void SetDebugText(string newText) => debugText.SetText(newText);
}
