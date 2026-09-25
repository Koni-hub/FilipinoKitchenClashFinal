using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CookingAreaButtonSpawner : MonoBehaviour
{
    private void Start()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        CreateButton(canvas, "GoToPreppingButton", "Go to Prepping",
            new Vector2(130, 0), new Vector2(0, 0.5f), new Color(0.2f, 0.6f, 0.3f, 1f),
            "UI_AdoboPreppingArea");
    }

    private void CreateButton(Canvas canvas, string name, string label,
        Vector2 position, Vector2 anchor, Color color, string sceneName)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform));
        btnObj.transform.SetParent(canvas.transform, false);

        Image bg = btnObj.AddComponent<Image>();
        bg.color = color;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = bg;

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(200, 50);

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);

        Text uiText = textObj.AddComponent<Text>();
        uiText.text = label;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiText.fontSize = 18;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        btn.onClick.AddListener(() =>
        {
            Debug.Log($"[CookingNav] Navigating to {sceneName}");
            SceneManager.LoadScene(sceneName);
        });
    }
}
