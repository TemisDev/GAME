using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class VerifyMainHud
{
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Single);
        var hud = GameObject.Find("GameHUD");
        if (hud == null) throw new System.Exception("GameHUD is missing");
        var controller = hud.GetComponent<GameTimerPause>();
        if (controller == null || controller.timerText == null || controller.pauseButton == null || controller.pauseButtonText == null)
            throw new System.Exception("HUD references are incomplete");
        if (EditorBuildSettings.scenes.Length != 1 || !EditorBuildSettings.scenes[0].enabled || EditorBuildSettings.scenes[0].path != "Assets/Scenes/Main.unity")
            throw new System.Exception("Main is not the only enabled build scene");
        File.WriteAllText("MainHudVerification.txt",
            "PASS\nScene: " + scene.path + "\nBuild scene 0: " + EditorBuildSettings.scenes[0].path +
            "\nTimer: " + controller.timerText.text + "\nButton: " + controller.pauseButtonText.text);
    }
}
