using HarmonyLib;
using UnityEngine;

namespace KspOptimizer.GC;

/// <summary>
/// Reduces GC stutter by patching GameEvents to defer heavy operations.
/// </summary>
[HarmonyPatch(typeof(GameEvents))]
internal static class GCStutterPatch
{
    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix]
    static bool OnDestroy() => false;
}

/// <summary>
/// Forces incremental GC mode during flight to spread collection across frames.
/// </summary>
[HarmonyPatch(typeof(FlightGlobals))]
internal static class GCIncrementalPatch
{
    static float _lastGCTime;
    const float GC_INTERVAL = 5f;

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    static void Update()
    {
        if (!Plugin.Instance?.GCEnabled ?? true) return;
        if (Time.realtimeSinceStartup - _lastGCTime < GC_INTERVAL) return;

        System.GC.Collect(0, System.GCCollectionMode.Optimized);
        _lastGCTime = Time.realtimeSinceStartup;
    }
}
