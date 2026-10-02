using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class InstallMainHud
{
    private const string ScenePath = "Assets/Scenes/Main.unity";
    private const string MarkerPath = "MainHudInstallation.txt";

    static InstallMainHud() { EditorApplication.delayCall += Run; }

    public static void Run()
    {
        if (Application.isPlaying || File.Exists(MarkerPath)) return;

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var old = GameObject.Find("GameHUD");
        if (old != null) Object.DestroyImmediate(old);

        var font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        var canvasObject = new GameObject("GameHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(GameTimerPause));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var timerPanel = CreatePanel("TimerPanel", canvasObject.transform, new Color(0.08f, 0.11f, 0.14f, 0.86f));
        SetAnchors(timerPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(300f, 86f));
        var timer = CreateText("TimerText", timerPanel.transform, font, "TIEMPO  00:00", 34, TextAnchor.MiddleCenter);
        Stretch(timer.rectTransform, 10f);

        var pauseObject = new GameObject("PauseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        pauseObject.transform.SetParent(canvasObject.transform, false);
        var pauseImage = pauseObject.GetComponent<Image>();
        pauseImage.color = new Color(1f, 0.67f, 0.12f, 0.96f);
        SetAnchors(pauseObject.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(260f, 86f));
        var pauseLabel = CreateText("Label", pauseObject.transform, font, "PAUSAR", 32, TextAnchor.MiddleCenter);
        pauseLabel.color = new Color(0.10f, 0.08f, 0.04f, 1f);
        pauseLabel.fontStyle = FontStyle.Bold;
        Stretch(pauseLabel.rectTransform, 8f);

        var controller = canvasObject.GetComponent<GameTimerPause>();
        controller.timerText = timer;
        controller.pauseButton = pauseObject.GetComponent<Button>();
        controller.pauseButtonText = pauseLabel;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        File.WriteAllText(MarkerPath,
            "Scene: " + ScenePath + "\nBuild index: 0\nHUD: GameHUD\nTimer: elapsed MM:SS\nPause: button and Escape key\n");
    }

    private static Image CreatePanel(string name, Transform parent, Color color)
    {
        var item = new GameObject(name, typeof(RectTransform), typeof(Image));
        item.transform.SetParent(parent, false);
        var image = item.GetComponent<Image>(); image.color = color; return image;
    }

    private static Text CreateText(string name, Transform parent, Font font, string value, int size, TextAnchor alignment)
    {
        var item = new GameObject(name, typeof(RectTransform), typeof(Text));
        item.transform.SetParent(parent, false);
        var text = item.GetComponent<Text>();
        text.font = font; text.text = value; text.fontSize = size; text.alignment = alignment;
        text.color = Color.white; text.fontStyle = FontStyle.Bold; return text;
    }

    private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
    {
        rect.anchorMin = min; rect.anchorMax = max; rect.pivot = new Vector2(max.x, max.y);
        rect.anchoredPosition = position; rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding); rect.offsetMax = new Vector2(-padding, -padding);
    }
}
