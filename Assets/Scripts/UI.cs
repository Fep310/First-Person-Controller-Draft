using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    private BoxPlayerController playerController;
    [SerializeField] private TMP_Text velMagnitudeText;
    [SerializeField] private TMP_Text colDownText;
    [SerializeField] private TMP_Text jumpInterText;
    [SerializeField] private TMP_Text velocityText;

    private void Awake()
    {
        /*playerController = FindObjectOfType<PlayerController>();
        playerController.OnVelocityChange += UpdateVelocityText;
        playerController.OnUnitsPerSecondChange += UpdateUnitsPerSecondText;
        playerController.OnColDownChange += UpdateColDownText;
        playerController.OnJumpInterChange += UpdateJumpInterText;*/
    }

    private void OnDestroy()
    {
        /*playerController.OnVelocityChange -= UpdateVelocityText;
        playerController.OnUnitsPerSecondChange -= UpdateUnitsPerSecondText;
        playerController.OnColDownChange -= UpdateColDownText;
        playerController.OnJumpInterChange -= UpdateJumpInterText;*/
    }

    private void UpdateUnitsPerSecondText(float newValue)
    {
        if (newValue <= float.Epsilon)
            newValue = 0;
        velMagnitudeText.text = "Units Per Second: " + newValue.ToString("0.000");
    }

    private void UpdateColDownText(bool newValue)
    {
        colDownText.text = "Col Down? " + newValue.ToString().ToUpper();
    }

    private void UpdateJumpInterText(float newValue)
    {
        if (newValue <= float.Epsilon)
            newValue = 0;
        jumpInterText.text = "Jump Inter: " + newValue.ToString("0.00");
    }

    private void UpdateVelocityText(Vector3 newValue)
    {
        velocityText.text = "Velocity: " + newValue.ToString();
    }
}
