using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionGizmo : MonoBehaviour
{
    [SerializeField] private float gizmoSize;
    [SerializeField] private Color gizmoColor;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);
        Gizmos.color = default;
    }
}
