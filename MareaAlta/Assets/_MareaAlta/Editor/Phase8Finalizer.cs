using System.IO;
using MareaAlta.UI;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class Phase8Finalizer
    {
        private const string ScenePath =
            "Assets/_MareaAlta/Scenes/Prototype_PuntaCoral.unity";
        private const string CompletionMarker =
            "Assets/_MareaAlta/Settings/Phase8.complete.txt";
        private const string BuildDirectory = "Builds/Windows";
        private const string ExecutablePath = BuildDirectory + "/MareaAlta.exe";

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

            ConfigurePresentation();
            BuildWindowsDemo();
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

        [MenuItem("Marea Alta/Fase 8 - Generar demo de Windows")]
        public static void RunFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            ConfigurePresentation();
            BuildWindowsDemo();
        }

        private static void ConfigurePresentation()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject systems = GameObject.Find("Systems") ?? new GameObject("Systems");
            if (systems.GetComponent<InstructionsOverlay>() == null)
                systems.AddComponent<InstructionsOverlay>();

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 7f;
                mainCamera.backgroundColor = new Color(0.015f, 0.08f, 0.16f);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            PlayerSettings.productName = "Marea Alta - Prototipo";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            AssetDatabase.SaveAssets();
        }

        private static void BuildWindowsDemo()
        {
            Directory.CreateDirectory(BuildDirectory);
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = ExecutablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError(
                    $"Marea Alta: la build falló con {report.summary.totalErrors} errores. " +
                    "Revisar la consola antes de reintentar.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(CompletionMarker));
            File.WriteAllText(
                CompletionMarker,
                $"Build completada: {report.summary.totalSize} bytes.");
            AssetDatabase.ImportAsset(CompletionMarker);
            AssetDatabase.SaveAssets();
            Debug.Log($"Marea Alta: fase 8 completada. Build creada en {ExecutablePath}.");
        }
    }
}
