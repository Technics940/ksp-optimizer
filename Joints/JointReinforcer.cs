using HarmonyLib;
using UnityEngine;

namespace KspOptimizer.Joints;

/// <summary>
/// Reinforces joints at launch so players don't need 50 struts.
/// Makes joints stiffer based on part mass.
/// </summary>
[HarmonyPatch(typeof(FlightGlobals))]
internal static class JointReinforcer
{
    static float _lastReinforceCheck;
    const float REINFORCE_INTERVAL = 3f;

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void Update()
    {
        if (!Plugin.Instance?.JointReinforceEnabled ?? true) return;
        if (Time.realtimeSinceStartup - _lastReinforceCheck < REINFORCE_INTERVAL) return;
        _lastReinforceCheck = Time.realtimeSinceStartup;

        var vessel = FlightGlobals.ActiveVessel;
        if (vessel == null) return;

        foreach (var part in vessel.parts)
        {
            if (part.Rigidbody == null) continue;

            var joints = part.GetComponents<Joint>();
            foreach (var joint in joints)
            {
                float massFactor = Mathf.Clamp(part.mass / 0.5f, 1f, 50f);
                if (joint.breakForce < massFactor * 1000f)
                    joint.breakForce = massFactor * 1000f;
                if (joint.breakTorque < massFactor * 1000f)
                    joint.breakTorque = massFactor * 1000f;
            }

            if (joints.Length == 0 && IsStructural(part))
            {
                part.Rigidbody.maxAngularVelocity = Mathf.Max(part.Rigidbody.maxAngularVelocity, 15f);
            }
        }
    }

    static bool IsStructural(Part p)
    {
        string name = p.partInfo.name;
        return name.Contains("strut") || name.Contains("structural") ||
               name.Contains("beam") || name.Contains("frame");
    }
}
