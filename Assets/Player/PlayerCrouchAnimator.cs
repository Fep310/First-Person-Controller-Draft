using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouchAnimator : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private AnimationCurve crouchCurve;
    [SerializeField] [Range(0f, 1f)] float transitionTime;

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
        originalCameraY = player.CameraHolder.localPosition.y;
        crouchCameraY = .8f;
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
        float endTime = startTime + transitionTime;
        float t;

        float previousHeight = player.Controller.height;
        float nextHeight = crouching ? crouchHeight : OriginalHeight;

        Vector3 previousCenter = new Vector3(0, player.Controller.center.y, 0);
        Vector3 nextCenter = new Vector3(0, crouching ? crouchYCenter : originalYCenter, 0);

        Vector3 previousCameraPos = new Vector3(0, player.CameraHolder.localPosition.y, 0);
        Vector3 nextCameraPos = new Vector3(0, crouching ? crouchCameraY : originalCameraY, 0);

        while (Time.time < endTime)
        {
            t = Mathf.InverseLerp(startTime, endTime, Time.time);
            t = crouchCurve.Evaluate(t);

            player.Controller.height = Mathf.Lerp(previousHeight, nextHeight, t);
            player.Controller.center = Vector3.Lerp(previousCenter, nextCenter, t);
            player.CameraHolder.localPosition = Vector3.Lerp(previousCameraPos, nextCameraPos, t);

            yield return null;
        }
    }
}
