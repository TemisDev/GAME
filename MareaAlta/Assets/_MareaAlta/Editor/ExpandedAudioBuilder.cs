using System.IO;
using MareaAlta.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class ExpandedAudioBuilder
    {
        private const string MenuScenePath = "Assets/_MareaAlta/Scenes/MainMenu.unity";
        private const string DemoScenePath = "Assets/_MareaAlta/Scenes/Demo_PuntaCoral.unity";
        private const string MarkerPath = "Assets/_MareaAlta/Settings/ExpandedAudio.complete.txt";
        private const string AudioRoot = "Assets/ThirdParty/AudioCC0";
        private const string KenneyRoot = AudioRoot + "/KenneyInterfaceSounds/Audio";

        private static bool waitingForEditMode;

        [InitializeOnLoadMethod]
        private static void ScheduleBuild()
        {
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(MarkerPath) != null)
                return;

            EditorApplication.delayCall += BuildWhenReady;
        }

        private static void BuildWhenReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += BuildWhenReady;
                return;
            }

            Build();
        }

        private static void WaitForEditMode()
        {
            if (waitingForEditMode)
                return;
            waitingForEditMode = true;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
                return;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            waitingForEditMode = false;
            EditorApplication.delayCall += BuildWhenReady;
        }

        [MenuItem("Marea Alta/Demo ampliada/1 - Configurar audio")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            AudioClip music = LoadClip(AudioRoot + "/Underwater-Ambient-Pad.ogg");
            AudioClip pickup = LoadClip(AudioRoot + "/Bubble-Pickup.wav");
            AudioClip energy = LoadClip(KenneyRoot + "/confirmation_002.ogg");
            AudioClip error = LoadClip(KenneyRoot + "/error_004.ogg");
            AudioClip turbine = LoadClip(KenneyRoot + "/switch_006.ogg");
            AudioClip gate = LoadClip(KenneyRoot + "/open_003.ogg");
            AudioClip victory = LoadClip(KenneyRoot + "/confirmation_004.ogg");
            AudioClip ui = LoadClip(KenneyRoot + "/select_002.ogg");

            if (music == null || pickup == null || energy == null || gate == null)
            {
                Debug.LogError("Marea Alta: Unity todavía no terminó de importar los archivos de audio.");
                EditorApplication.delayCall += BuildWhenReady;
                return;
            }

            AddAudioSystem(MenuScenePath, music, pickup, energy, error, turbine, gate, victory, ui);
            AddAudioSystem(DemoScenePath, music, pickup, energy, error, turbine, gate, victory, ui);

            File.WriteAllText(MarkerPath, "Audio CC0 configurado en menú y demo.");
            AssetDatabase.ImportAsset(MarkerPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Marea Alta: música y efectos CC0 configurados correctamente.");
        }

        private static AudioClip LoadClip(string path)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private static void AddAudioSystem(
            string scenePath,
            AudioClip music,
            AudioClip pickup,
            AudioClip energy,
            AudioClip error,
            AudioClip turbine,
            AudioClip gate,
            AudioClip victory,
            AudioClip ui)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject audioObject = GameObject.Find("GameAudio") ?? new GameObject("GameAudio");

            while (audioObject.GetComponents<AudioSource>().Length < 2)
                audioObject.AddComponent<AudioSource>();

            GameAudio audio = audioObject.GetComponent<GameAudio>();
            if (audio == null)
                audio = audioObject.AddComponent<GameAudio>();
            audio.Configure(music, pickup, energy, error, turbine, gate, victory, ui);
            EditorUtility.SetDirty(audio);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
