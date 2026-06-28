using HarmonyLib;
using KSP.UI.Screens;
using UnityEngine;

namespace KspOptimizer;

[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
public class Plugin : MonoBehaviour
{
    public static Plugin Instance { get; private set; } = null!;
    public Harmony Harmony { get; private set; } = null!;

    ApplicationLauncherButton? _toolbarButton;
    bool _showWindow;
    Rect _windowRect = new Rect(100, 100, 340, 300);
    static readonly int WINDOW_ID = "KspOptimizer".GetHashCode();

    public bool GCEnabled { get; set; } = true;
    public bool WeldingEnabled { get; set; } = true;
    public bool JointReinforceEnabled { get; set; } = true;
    public bool AdaptivePhysicsEnabled { get; set; } = true;

    public int WeldedPartCount { get; internal set; }
    public int ActiveVesselParts { get; internal set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Harmony = new Harmony("com.ksp.optimizer");
        Harmony.PatchAll(typeof(Plugin).Assembly);

        GameEvents.onGUIApplicationLauncherReady.Add(OnToolbarReady);
        GameEvents.onFlightReady.Add(OnFlightReady);
        GameEvents.onVesselGoOffRails.Add(_ => ReinforceAllJoints());

        Debug.Log("[KspOptimizer] Loaded");
    }

    void OnToolbarReady()
    {
        if (ApplicationLauncher.Instance == null) return;

        var texture = new Texture2D(38, 38, TextureFormat.RGBA32, false);
        var pixels = new Color[38 * 38];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0.2f, 0.8f, 0.3f, 0.9f);
        texture.SetPixels(pixels);
        texture.Apply();

        _toolbarButton = ApplicationLauncher.Instance.AddModApplication(
            OnToggleOn, OnToggleOff,
            null, null,
            null, null,
            ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
            texture
        );
    }

    void OnToggleOn() { _showWindow = true; }
    void OnToggleOff() { _showWindow = false; }

    void OnFlightReady()
    {
        if (JointReinforceEnabled) ReinforceAllJoints();
    }

    void ReinforceAllJoints()
    {
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
        }
    }

    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.F2))
            _showWindow = !_showWindow;

        if (Time.frameCount % 120 == 0) RunGC();
    }

    void RunGC()
    {
        if (!GCEnabled) return;
        System.GC.Collect(0, System.GCCollectionMode.Optimized);
    }

    void OnGUI()
    {
        if (!_showWindow) return;
        _windowRect = GUILayout.Window(WINDOW_ID, _windowRect, DrawWindow, "KSP Optimizer v1.0");
    }

    void DrawWindow(int id)
    {
        GUILayout.BeginVertical();

        GUILayout.Label("<b>Performance</b>");
        GCEnabled = GUILayout.Toggle(GCEnabled, " GC Stutter Fix");
        WeldingEnabled = GUILayout.Toggle(WeldingEnabled, " Part Welding");
        JointReinforceEnabled = GUILayout.Toggle(JointReinforceEnabled, " Joint Reinforcement");
        AdaptivePhysicsEnabled = GUILayout.Toggle(AdaptivePhysicsEnabled, " Adaptive Physics LOD");

        GUILayout.Space(8);
        GUILayout.Label($"Welded: {WeldedPartCount} | Parts: {ActiveVesselParts}");

        if (GUILayout.Button("Force GC"))
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
        }

        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    void OnDestroy()
    {
        GameEvents.onGUIApplicationLauncherReady.Remove(OnToolbarReady);
        GameEvents.onFlightReady.Remove(OnFlightReady);
        if (_toolbarButton != null)
            ApplicationLauncher.Instance?.RemoveModApplication(_toolbarButton);
        Harmony?.UnpatchAll();
        Instance = null!;
    }
}
