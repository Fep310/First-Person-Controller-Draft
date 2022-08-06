using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMath : MonoBehaviour
{
    /*public static Vector3 GetOneMinusAbs(Vector3 vec)
    {
        return new Vector3(1 - Mathf.Abs(vec.x), 1 - Mathf.Abs(vec.y), 1 - Mathf.Abs(vec.z));
    }*/

    public const float TAU = Mathf.PI / 2;

    public static float Remap(float min1, float max1, float min2, float max2, float value)
    {
        float t = Mathf.InverseLerp(min1, max1, value);
        return Mathf.Lerp(min2, max2, t);
    }
}
