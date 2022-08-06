using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouchAnimator : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private AnimationCurve crouchCurve;
    [SerializeField] private PlayerConstantMovementValues constValues;

    public float OriginalHeight { get; private set; }
    private float crouchHeight;

    private float originalYCenter;
    private float crouchYCenter;

    private float originalCameraY;
    private float crouchCameraY;

    private void Awake()
    {
        CalculateCrouchConsts();
    }

    private void CalculateCrouchConsts()
    {
        OriginalHeight = player.Controller.height;
        crouchHeight = OriginalHeight / 1.5f;
        originalYCenter = player.Controller.center.y;
        crouchYCenter = originalYCenter / 1.5f;
        originalCameraY = player.Camera.transform.localPosition.y;
        crouchCameraY = -.8f;
    }

    public void Crouch()
    {
        StopAllCoroutines();
        StartCoroutine(CrouchCoroutine(true));
    }

    public void StandUp()
    {
        StopAllCoroutines();
        StartCoroutine(CrouchCoroutine(false));
    }

    private IEnumerator CrouchCoroutine(bool crouching)
    {
        float startTime = Time.time;
        float endTime = startTime + (constValues.SlideTransitionTime * 1.75f);
        float t;

        float previousHeight = player.Controller.height;
        float nextHeight = crouching ? crouchHeight : OriginalHeight;

        Vector3 previousCenter = new Vector3(0, player.Controller.center.y, 0);
        Vector3 nextCenter = new Vector3(0, crouching ? crouchYCenter : originalYCenter, 0);

        Vector3 previousCameraPos = new Vector3(0, player.Camera.transform.localPosition.y, 0);
        Vector3 nextCameraPos = new Vector3(0, crouching ? crouchCameraY : originalCameraY, 0);

        while (Time.time < endTime)
        {
            // TODO: Keep checking if can proceed with standing up animation

            t = Mathf.InverseLerp(startTime, endTime, Time.time);
            t = crouchCurve.Evaluate(t);

            player.Controller.height = Mathf.Lerp(previousHeight, nextHeight, t);
            player.Controller.center = Vector3.Lerp(previousCenter, nextCenter, t);
            player.Camera.transform.localPosition = Vector3.Lerp(previousCameraPos, nextCameraPos, t);

            yield return null;
        }
    }
}
