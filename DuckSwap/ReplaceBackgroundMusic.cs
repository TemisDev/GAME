using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class ReplaceBackgroundMusic
{
    private const string AudioPath = "Assets/AudioClips/the_mountain-game-game-music-508018.mp3";
    private const string MarkerPath = "BackgroundMusicReplacement.txt";

    static ReplaceBackgroundMusic()
    {
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        if (Application.isPlaying || File.Exists(MarkerPath)) return;

        var replacement = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);
        if (replacement == null) return;

        int replacements = 0;
        foreach (var scenePath in new[] { "Assets/Scenes/Main.unity", "Assets/_Complete-Game.unity" })
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var source in root.GetComponentsInChildren<AudioSource>(true))
            {
                if (source.clip != null && source.clip.name == "BackgroundMusic")
                {
                    source.clip = replacement;
                    source.playOnAwake = true;
                    source.loop = true;
                    EditorUtility.SetDirty(source);
                    replacements++;
                }
            }
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
        }

        File.WriteAllText(MarkerPath, "Replacements: " + replacements + "\nClip: " + AudioPath);
        AssetDatabase.SaveAssets();
    }
}
