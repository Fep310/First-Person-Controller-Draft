using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordBehaviour : MonoBehaviour
{
    [SerializeField] private PlayerInputData inputData;
    [SerializeField] private Transform yRotTf, xRotTf, swordTf;
    [SerializeField] private Transform point1PivotTf, point2PivotTf, point1Tf, point2Tf;
    private Transform tf;
    [SerializeField] [Range(0, 50)] private float firmSwiftness;
    [SerializeField] [Range(0, 50)] private float attackSwifness;
    [SerializeField] [Range(0, 50)] private float swordSpinSwifness;
    [SerializeField] [Range(0, 50)] private float defendSwifness;
    [SerializeField] [Range(0, 1)] private float mouseSensetivityMultiplier;
    
    // firm, attacking, defending;
    [SerializeField] private Vector3 point1FirmPos, point2FirmPos, firmRot;
    [SerializeField] private Vector3 point1AttackPos, point2AttackPos, attackRot;
    [SerializeField] private Vector3 point1DefendPos, point2DefendPos, defendRot;

    public float MouseSensetivityMultiplier { get => mouseSensetivityMultiplier; private set => mouseSensetivityMultiplier = value; }

    private Quaternion targetRot;
    private Vector3 swordDir;
    private bool chargingSlash;
    public bool Charging { get => chargingSlash; private set => chargingSlash = value; }
    private SwordState swordState;
    private Quaternion lastPoivot1Rot;

    private Quaternion GetCurrentRot() => Quaternion.Euler(xRotTf.localEulerAngles.x, yRotTf.localEulerAngles.y, 0);
    private Quaternion firmQuat;
    private Quaternion attackQuat;
    private Quaternion defendQuat;
    private Quaternion lastNewRot;

    private Coroutine swordTransition;

    /*
     * BETTER SWING [X]
     *  - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
     *  Get two points, one on the base of the sword and the other on the tip
     *  They should position themselves like the current sword swing
     *  The top point should follow the rotation slower (lower stifness)
     *  Get the vecotr from BASE -> TIP and make a direction
     *  Apply the direction on the sword's rotation
     *  - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
     *  
     * SLASH BY RELEASING THE ATTACK BUTTON [ ]
     *  - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
     *  Prepare attack holding the attack button
     *  Flick you mouse on the direction of the attack
     *  Let go of the attack button midway through the slash
     */

    private void Start()
    {
        tf = transform;
        
        swordState = SwordState.Firm;

        firmQuat = Quaternion.Euler(firmRot);
        attackQuat = Quaternion.Euler(attackRot);
        defendQuat = Quaternion.Euler(defendRot);

        point1Tf.localPosition = point1FirmPos;
        point2Tf.localPosition = point2FirmPos;
        point1PivotTf.localRotation = firmQuat;

        swordTf.localPosition = point1FirmPos;
        swordTf.localRotation = firmQuat;
    }

    private void OnEnable()
    {
        inputData.OnLeftClickDown += AttackHold;
        inputData.OnLeftClickUp += AttackLetGo;
    }

    private void OnDisable()
    {
        inputData.OnLeftClickDown -= AttackHold;
        inputData.OnLeftClickUp -= AttackLetGo;
    }

    private void AttackHold()
    {
        swordState = SwordState.Attack;

        if (swordTransition != null)
            StopCoroutine(swordTransition);

        swordTransition = StartCoroutine(TransitionSword(point1AttackPos, point2AttackPos, attackQuat, .1f));

        /*startSwingRot = GetCurrentRot();
        targetRot = GetCurrentRot();
        chargingSlash = true;
        
        ActivateSword();*/
    }

    private void AttackLetGo()
    {
        swordState = SwordState.Firm;

        if (swordTransition != null)
            StopCoroutine(swordTransition);

        swordTransition = StartCoroutine(TransitionSword(point1FirmPos, point2FirmPos, firmQuat, .2f));

        /*if (chargingSlash == false)
            return;

        endSwingRot = GetCurrentRot();
        chargingSlash = false;
        slashing = true;
        Slash();*/
    }

    private void Update()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        /*if (slashing)
            return;

        if (!chargingSlash)
        {
            targetRot = Quaternion.Lerp(GetCurrentRot(), yRotTf.localRotation, .25f);
        }
        else
        {
            if (Quaternion.Angle(startSwingRot, GetCurrentRot()) > 100)
                SetEndRot();
        }*/

        void UpdatePivotsRot(float p1Swiftness, float p2Swiftness)
        {
            point1PivotTf.localRotation = Quaternion.Slerp(
                point1PivotTf.localRotation,
                targetRot,
                p1Swiftness * Time.deltaTime);

            point2PivotTf.localRotation = Quaternion.Slerp(
                point2PivotTf.localRotation,
                targetRot,
                p2Swiftness * Time.deltaTime);
        }

        targetRot = Quaternion.Lerp(GetCurrentRot(), yRotTf.localRotation, .1f);
        
        switch (swordState)
        {
            case SwordState.Firm:

                UpdatePivotsRot(firmSwiftness, firmSwiftness * .93f);
                swordDir = (point2Tf.position - point1Tf.position).normalized;
                swordTf.localRotation = Quaternion.LookRotation(swordDir, point1PivotTf.up);

                break;

            case SwordState.Attack:

                UpdatePivotsRot(attackSwifness, attackSwifness * .6f);
                swordDir = (point2Tf.position - point1Tf.position).normalized;

                Vector3 lastDir = lastPoivot1Rot * Vector3.forward;
                Vector3 newDir = point1PivotTf.forward;

                Vector3 dirDiff = newDir - lastDir;

                // TODO: Figure this out (value too low)
                /*if (dirDiff.sqrMagnitude * Time.deltaTime < .1f)
                {
                    swordTf.localRotation = Quaternion.Lerp(swordTf.localRotation, lastNewRot, swordSpinSwifness * Time.deltaTime);
                    break;
                }*/

                Vector3 dirDiffDir = dirDiff.normalized;
                Vector3 newUpwards = Vector3.Cross(dirDiffDir, lastPoivot1Rot * Vector3.forward);

                Quaternion newRot = Quaternion.LookRotation(swordDir, newUpwards); // HOLY SHIT THIS WORKS WOOOOOOOOOOOOOOOO

                swordTf.localRotation = Quaternion.Lerp(swordTf.localRotation, newRot, swordSpinSwifness * Time.deltaTime);

                lastNewRot = newRot;


                break;

            case SwordState.Defend:
                break;
        }

        
        swordTf.position = point1Tf.position;

        lastPoivot1Rot = point1PivotTf.localRotation;


    }

    /*private void Slash()
    {
        Quaternion targetSwingRot = Quaternion.SlerpUnclamped(startSwingRot, endSwingRot, 1.5f);

        StartCoroutine(SwingC());

        IEnumerator SwingC()
        {
            while (Quaternion.Angle(tf.localRotation, targetSwingRot) > 5)
            {
                tf.localRotation = Quaternion.Slerp(
                tf.localRotation,
                targetSwingRot,
                slashSwiftness * Time.deltaTime);
                yield return null;
            }
            
            slashing = false;

            RestSword();
        }
    }*/

    private IEnumerator TransitionSword(Vector3 newPoint1Pos, Vector3 newPoint2Pos, Quaternion newRot, float time)
    {
        Vector3 point1startPos = point1Tf.localPosition;
        Vector3 point2startPos = point2Tf.localPosition;
        Quaternion startRot = swordTf.localRotation;

        float st = Time.time;
        float et = st + time;
        float t;

        while (Time.time < et)
        {
            t = Mathf.InverseLerp(st, et, Time.time);
            t = -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;

            point1Tf.localPosition = Vector3.Lerp(point1startPos, newPoint1Pos, t);
            point2Tf.localPosition = Vector3.Lerp(point2startPos, newPoint2Pos, t);
            point1Tf.localRotation = Quaternion.Lerp(startRot, newRot, t);

            yield return null;
        }

        point1Tf.localPosition = newPoint1Pos;
        point2Tf.localPosition = newPoint2Pos;
        point1Tf.localRotation = newRot;
    }
}
