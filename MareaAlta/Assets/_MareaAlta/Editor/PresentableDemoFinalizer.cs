using System.IO;
using MareaAlta.UI;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class PresentableDemoFinalizer
    {
        private const string MenuScenePath =
            "Assets/_MareaAlta/Scenes/MainMenu.unity";
        private const string DemoScenePath =
            "Assets/_MareaAlta/Scenes/Demo_PuntaCoral.unity";
        private const string CompletionMarker =
            "Assets/_MareaAlta/Settings/PresentableDemo04.complete.txt";
        private const string BuildDirectory = "Builds/Presentable";
        private const string ExecutablePath = BuildDirectory + "/MareaAltaDemo.exe";

        private static bool waitingForEditMode;

        [InitializeOnLoadMethod]
        private static void ScheduleFinalization()
        {
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(CompletionMarker) != null)
                return;

            EditorApplication.delayCall += FinalizeWhenReady;
        }

        private static void FinalizeWhenReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer)
            {
                EditorApplication.delayCall += FinalizeWhenReady;
                return;
            }

            ConfigureScenes();
            BuildDemo();
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
            EditorApplication.delayCall += FinalizeWhenReady;
        }

        [MenuItem("Marea Alta/Demo presentable/4 - Generar build final")]
        public static void RunFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            ConfigureScenes();
            BuildDemo();
        }

        private static void ConfigureScenes()
        {
            AddFadeToScene(MenuScenePath);
            AddFadeToScene(DemoScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(DemoScenePath, true)
            };

            PlayerSettings.productName = "Marea Alta - Demo";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            AssetDatabase.SaveAssets();
        }

        private static void AddFadeToScene(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject transitions = GameObject.Find("ScreenTransition") ?? new GameObject("ScreenTransition");
            if (transitions.GetComponent<ScreenFadeIn>() == null)
                transitions.AddComponent<ScreenFadeIn>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void BuildDemo()
        {
            Directory.CreateDirectory(BuildDirectory);
            var options = new BuildPlayerOptions
            {
                scenes = new[] { MenuScenePath, DemoScenePath },
                locationPathName = ExecutablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError(
                    $"Marea Alta: la build presentable falló con {report.summary.totalErrors} errores.");
                return;
            }

            File.WriteAllText(
                CompletionMarker,
                $"Demo presentable completada: {report.summary.totalSize} bytes.");
            AssetDatabase.ImportAsset(CompletionMarker);
            AssetDatabase.SaveAssets();
            Debug.Log($"Marea Alta: demo presentable creada en {ExecutablePath}.");
        }
    }
}
