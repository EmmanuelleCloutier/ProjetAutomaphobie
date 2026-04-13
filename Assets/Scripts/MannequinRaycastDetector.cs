using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this to the player's camera (or any GameObject whose forward direction
/// should act as the ray origin).  Every frame it performs a <see cref="Physics.SphereCastAll"/>
/// along its forward axis and notifies every <see cref="MannequinBehavior"/> in the swept
/// volume when they are entered or exited – so all mannequins in view are detected at once.
/// </summary>
public class MannequinRaycastDetector : MonoBehaviour
{
    [Header("SphereCast Settings")]
    [Tooltip("Maximum distance the sphere sweep can reach.")]
    public float castDistance = 15f;

    [Tooltip("Radius of the sweep sphere. Larger values detect mannequins further off-center.")]
    public float castRadius = 0.5f;

    [Tooltip("Layer mask used to filter which objects the sphere cast can hit.")]
    public LayerMask castMask = Physics.AllLayers;

    [Header("Debug")]
    [Tooltip("Draw a debug ray in the Scene view while in Play mode.")]
    public bool debugRay = true;

    // Mannequins whose volume the sphere cast is currently intersecting.
    private readonly HashSet<MannequinBehavior> m_ActiveTargets = new HashSet<MannequinBehavior>();

    // Reusable working set – avoids per-frame allocation.
    private readonly HashSet<MannequinBehavior> m_FrameTargets = new HashSet<MannequinBehavior>();

    // ── Unity Messages ───────────────────────────────────────────────────────

    private void Update()
    {
        Vector3 origin    = transform.position + transform.forward * castRadius;
        Vector3 direction = transform.forward;

        if (debugRay)
            Debug.DrawRay(origin, direction * castDistance, Color.cyan);

        // Collect every MannequinBehavior hit this frame.
        m_FrameTargets.Clear();
        RaycastHit[] hits = Physics.SphereCastAll(origin, castRadius, direction, castDistance, castMask);

        foreach (RaycastHit hit in hits)
        {
            MannequinBehavior mannequin = hit.collider.GetComponentInParent<MannequinBehavior>();
            if (mannequin != null)
                m_FrameTargets.Add(mannequin);
        }

        // Fire OnRaycastEnter for newly detected mannequins.
        foreach (MannequinBehavior mannequin in m_FrameTargets)
        {
            if (!m_ActiveTargets.Contains(mannequin))
                mannequin.OnRaycastEnter(gameObject);
        }

        // Fire OnRaycastExit for mannequins that left the sweep volume.
        foreach (MannequinBehavior mannequin in m_ActiveTargets)
        {
            if (!m_FrameTargets.Contains(mannequin))
                mannequin.OnRaycastExit(gameObject);
        }

        // Swap active set to the current frame's results.
        m_ActiveTargets.Clear();
        foreach (MannequinBehavior mannequin in m_FrameTargets)
            m_ActiveTargets.Add(mannequin);
    }

    private void OnDisable()
    {
        // Notify every tracked mannequin so none is left in a "cannot move" state.
        foreach (MannequinBehavior mannequin in m_ActiveTargets)
            mannequin.OnRaycastExit(gameObject);

        m_ActiveTargets.Clear();
        m_FrameTargets.Clear();
    }
}
