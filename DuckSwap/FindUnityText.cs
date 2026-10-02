using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text;

public static class FindUnityText
{
    public static void Run()
    {
        var output = new StringBuilder();
        foreach (var scenePath in new[] { "Assets/Scenes/Main.unity", "Assets/_Complete-Game.unity" })
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var label in root.GetComponentsInChildren<Text>(true))
            {
                if (label.text.IndexOf("TANK", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                output.AppendLine(scenePath + " | " + Hierarchy(label.transform) + " | Text: " + label.text);
            }
        }
        File.WriteAllText("FindUnityText.txt", output.ToString());
    }

    private static string Hierarchy(Transform item)
    {
        var result = item.name;
        while (item.parent != null) { item = item.parent; result = item.name + "/" + result; }
        return result;
    }
}
