using UnityEngine;
using UnityEngine.UI;

public class CanvasResponsiveSetup : MonoBehaviour
{
    private static CanvasResponsiveSetup instance;

    [Header("Reference Resolution")]
    public float referenceWidth = 1920f;
    public float referenceHeight = 1080f;

    [Header("Scaling")]
    [Range(0f, 1f)]
    public float matchWidthOrHeight = 0.5f;

    private float checkInterval = 2f;
    private float nextCheckTime;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        ConfigureAllCanvases();
        nextCheckTime = Time.time + checkInterval;
    }

    private void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            ConfigureAllCanvases();
        }
    }

    public void ConfigureAllCanvases()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            ConfigureCanvas(canvas);
        }
    }

    public void ConfigureCanvas(Canvas canvas)
    {
        if (canvas == null) return;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            Debug.Log($"[ResponsiveSetup] Added CanvasScaler to {canvas.gameObject.name}");
        }

        if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
            scaler.referenceResolution.x != referenceWidth ||
            scaler.referenceResolution.y != referenceHeight)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
            scaler.defaultSpriteDPI = 96f;
            scaler.fallbackScreenDPI = 96f;
            scaler.physicalUnit = CanvasScaler.Unit.Centimeters;
            Debug.Log($"[ResponsiveSetup] Configured {canvas.gameObject.name}: {referenceWidth}x{referenceHeight} match={matchWidthOrHeight}");
        }
    }
}
