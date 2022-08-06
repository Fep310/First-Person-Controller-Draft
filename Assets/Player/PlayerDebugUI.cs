using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class PlayerDebugUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;

    private SortedDictionary<int, string> lines;

    private void Awake()
    {
        if (debugText == null)
            return;
        lines = new SortedDictionary<int, string>();
    }

    public void SetLine(int line, string text)
    {
        if (debugText == null)
            return;

        if (text.Contains("\n"))
        {
            Debug.LogWarning("You can't use \" \\n \" on SetLine()");
            return;
        }

        lines[line] = text;
    }

    public void Clear()
    {
        if (debugText == null)
            return;

        lines.Clear();
    }

    private void Update()
    {
        if (debugText == null)
            return;

        UpdateText();
    }

    private void UpdateText()
    {
        if (debugText == null)
            return;

        StringBuilder sb = new StringBuilder();
        foreach (string line in lines.Values)
        {
            sb.AppendLine(line);
        }
        debugText.text = sb.ToString();
    }
}
