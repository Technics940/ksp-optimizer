using HarmonyLib;
using UnityEngine;

namespace KspOptimizer.Physics;

/// <summary>
/// Reduces physics fidelity for distant parts.
/// Parts far from camera get simplified collisions and no interpolation.
/// </summary>
[HarmonyPatch(typeof(FlightGlobals))]
internal static class AdaptivePhysicsLOD
{
    static float _lastLODCheck;
    const float LOD_CHECK_INTERVAL = 0.5f;

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void Update()
    {
        if (!Plugin.Instance?.AdaptivePhysicsEnabled ?? true) return;
        if (Time.realtimeSinceStartup - _lastLODCheck < LOD_CHECK_INTERVAL) return;
        _lastLODCheck = Time.realtimeSinceStartup;

        var vessel = FlightGlobals.ActiveVessel;
        if (vessel == null) return;

        var cam = FlightCamera.fetch?.mainCamera;
        if (cam == null) return;

        float camDist = Vector3.Distance(cam.transform.position, vessel.CoM);

        foreach (var part in vessel.parts)
        {
            if (part.Rigidbody == null) continue;

            float partDist = Vector3.Distance(cam.transform.position, part.transform.position);
            float totalDist = camDist + partDist;

            if (totalDist > 500f)
            {
                part.Rigidbody.interpolation = RigidbodyInterpolation.None;
                SetPartVisible(part, part == vessel.rootPart);
            }
            else
            {
                part.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                SetPartVisible(part, true);
            }
        }
    }

    static void SetPartVisible(Part p, bool visible)
    {
        var renderer = p.FindModelComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = visible;
    }
}
