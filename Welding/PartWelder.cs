using HarmonyLib;
using UnityEngine;

namespace KspOptimizer.Welding;

/// <summary>
/// Welds identical adjacent parts into single physics objects.
/// A 50-part fuel tank cluster becomes 1 part in the physics solver.
/// </summary>
[HarmonyPatch(typeof(FlightGlobals))]
internal static class PartWelder
{
    static float _lastWeldCheck;
    const float WELD_CHECK_INTERVAL = 2f;

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void Update()
    {
        if (!Plugin.Instance?.WeldingEnabled ?? true) return;
        if (Time.realtimeSinceStartup - _lastWeldCheck < WELD_CHECK_INTERVAL) return;
        _lastWeldCheck = Time.realtimeSinceStartup;

        var vessel = FlightGlobals.ActiveVessel;
        if (vessel == null) return;

        TryWeldVessel(vessel);
        Plugin.Instance!.ActiveVesselParts = vessel.parts.Count;
    }

    static void TryWeldVessel(Vessel vessel)
    {
        var groups = vessel.parts
            .Where(p => IsWeldable(p))
            .GroupBy(p => (p.partInfo.name, p.parent?.partInfo.name ?? "root"))
            .Where(g => g.Count() >= 3);

        foreach (var group in groups)
        {
            var parts = group.ToList();
            var anchor = parts[0];

            for (int i = 1; i < parts.Count; i++)
            {
                if (CanMerge(anchor, parts[i]))
                {
                    MergeInto(anchor, parts[i]);
                    Plugin.Instance!.WeldedPartCount++;
                }
            }
        }
    }

    static bool IsWeldable(Part p)
    {
        if (p.HasModuleImplementing<ModuleEngines>()) return false;
        if (p.HasModuleImplementing<ModuleScienceExperiment>()) return false;
        if (p.HasModuleImplementing<ModuleCommand>()) return false;

        string name = p.partInfo.name;
        return name.Contains("fuel") || name.Contains("tank") ||
               name.Contains("structural") || name.Contains("adapter") ||
               name.Contains("strut");
    }

    static bool CanMerge(Part a, Part b)
    {
        if (a == b) return false;
        if (a.parent != b.parent) return false;
        if (a.partInfo.name != b.partInfo.name) return false;
        if (a.protoPartSnapshot != null || b.protoPartSnapshot != null) return false;

        double aRes = 0, bRes = 0;
        foreach (var r in a.Resources) aRes += r.amount;
        foreach (var r in b.Resources) bRes += r.amount;
        if (Mathf.Abs((float)(aRes - bRes)) > 0.01f) return false;

        if (Mathf.Abs(a.mass - b.mass) > a.mass * 0.1f) return false;

        return true;
    }

    static void MergeInto(Part target, Part victim)
    {
        while (victim.children.Count > 0)
        {
            var child = victim.children[0];
            child.transform.SetParent(target.transform);
            child.parent = target;
            target.children.Add(child);
            victim.children.RemoveAt(0);
        }

        target.mass += victim.mass;

        if (victim.symmetryCounterparts != null)
        {
            foreach (var counter in victim.symmetryCounterparts)
            {
                counter.symmetryCounterparts?.Remove(victim);
                counter.symmetryCounterparts?.Add(target);
                target.symmetryCounterparts?.Add(counter);
            }
        }

        FlightGlobals.ActiveVessel?.parts?.Remove(victim);
        UnityEngine.Object.DestroyImmediate(victim.gameObject);
    }
}
