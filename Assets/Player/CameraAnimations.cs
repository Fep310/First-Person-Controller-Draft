using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimations : MonoBehaviour
{
    private Transform tf;
    private Player player;

    [Header("Sway")]
    [SerializeField] [Range(0f,10f)] private float maxSwayAmount;
    [SerializeField] [Range(0f, 1f)] private float swayTransitionTime;
    [SerializeField] private AnimationCurve swayTransitionCurve;

    [Space]
    [Header("JumpBob")]
    [SerializeField] [Range(0f, 15f)] private float jumpBobStrenght;
    [SerializeField] [Range(0f, 3f)] private float jumpBobTime;
    [SerializeField] private AnimationCurve jumpBobCurve;

    [Space]
    [Header("LandBob")]
    [SerializeField][Range(0f, 15f)] private float landBobStrenght;
    [SerializeField][Range(0f, 3f)] private float landBobTime;
    [SerializeField] private AnimationCurve landBobCurve;

    private Vector3 eulerRotation;
    private float swayAmount;
    private float lastSwayChange;
    private float originalCamHolderY;

    private Coroutine swayCoroutine;
    private Coroutine jumpLandBobCoroutine;

    private void Awake()
    {
        tf = transform;

        eulerRotation = Vector3.zero;
        originalCamHolderY = tf.localPosition.y;

        player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        tf.localRotation = Quaternion.Euler(eulerRotation);
    }

    public void UpdateSway(float to)
    {
        if (lastSwayChange == to)
            return;

        lastSwayChange = to;

        if (swayCoroutine != null)
            StopCoroutine(swayCoroutine);
        swayCoroutine = StartCoroutine(ChangeSwayCoroutine());

        IEnumerator ChangeSwayCoroutine()
        {
            float startTime = Time.time;
            float endTime = startTime + swayTransitionTime;
            float t;
            var startSway = swayAmount;

            while (Time.time <= endTime)
            {
                t = swayTransitionCurve.Evaluate(Mathf.InverseLerp(startTime, endTime, Time.time));

                swayAmount = Mathf.Lerp(startSway, to, t);

                eulerRotation.Set(eulerRotation.x, eulerRotation.y, swayAmount * -maxSwayAmount);

                player.debug.SetLine(5, swayAmount.ToString("0.00"));

                yield return null;
            }
        }
    }

    public void JumpBob(float strenghtMul)
    {
        /*if (landBobCurve != null)
            StopCoroutine(landBobCoroutine);*/

        if (jumpLandBobCoroutine != null)
            StopCoroutine(jumpLandBobCoroutine);
        jumpLandBobCoroutine = StartCoroutine(JumpBobCoroutine());

        IEnumerator JumpBobCoroutine()
        {
            float startTime = Time.time;
            float endTime = startTime + jumpBobTime;
            float t;

            while (Time.time <= endTime)
            {
                t = Mathf.InverseLerp(startTime, endTime, Time.time);
                t *= jumpBobStrenght;
                t *= strenghtMul;

                t = jumpBobCurve.Evaluate(t);

                eulerRotation.Set(t, eulerRotation.y, eulerRotation.z);

                yield return null;
            }

            eulerRotation.Set(0, eulerRotation.y, eulerRotation.z);
        }
    }

    public void LandBob(float strenghtMul)
    {
        /*if (jumpBobCoroutine != null)
            StopCoroutine(jumpBobCoroutine);*/

        if (jumpLandBobCoroutine != null)
            StopCoroutine(jumpLandBobCoroutine);
        jumpLandBobCoroutine = StartCoroutine(LandBobCoroutine());

        IEnumerator LandBobCoroutine()
        {
            float startTime = Time.time;
            float endTime = startTime + landBobTime + (strenghtMul * .25f);
            float t;

            while (Time.time <= endTime)
            {
                t = Mathf.InverseLerp(startTime, endTime, Time.time);
                t *= landBobStrenght;
                t = landBobCurve.Evaluate(t);
                t *= strenghtMul;

                eulerRotation.Set(t, eulerRotation.y, eulerRotation.z);

                yield return null;
            }

            eulerRotation.Set(0, eulerRotation.y, eulerRotation.z);
        }
    }
}
