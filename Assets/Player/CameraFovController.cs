using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFovController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Vector2 minMaxCameraFov;
    [SerializeField] private float transitionTime;
    [SerializeField] private AnimationCurve transitionCurve;

    public bool IsAtMinFov => Mathf.Approximately(cam.fieldOfView, minMaxCameraFov.x);

    public void IncreaseFov()
    {
        StopAllCoroutines();
        StartCoroutine(ChangeFovCoroutine(minMaxCameraFov.y));
    }

    public void DecreaseFov()
    {
        if (IsAtMinFov) return;

        StopAllCoroutines();
        StartCoroutine(ChangeFovCoroutine(minMaxCameraFov.x));
    }

    private IEnumerator ChangeFovCoroutine(float to)
    {
        float startTime = Time.time;
        float endTime = startTime + transitionTime;
        float t;
        var startFov = cam.fieldOfView;

        while (Time.time <= endTime)
        {
            t = transitionCurve.Evaluate(Mathf.InverseLerp(startTime, endTime, Time.time));
            cam.fieldOfView = Mathf.Lerp(startFov, to, t);
            yield return null;
        }
    }
}
