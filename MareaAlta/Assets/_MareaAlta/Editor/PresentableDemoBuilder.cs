using System.IO;
using System.Linq;
using MareaAlta.Core;
using MareaAlta.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class PresentableDemoBuilder
    {
        private const string PrototypeScenePath =
            "Assets/_MareaAlta/Scenes/Prototype_PuntaCoral.unity";
        private const string DemoScenePath =
            "Assets/_MareaAlta/Scenes/Demo_PuntaCoral.unity";
        private const string MenuScenePath =
            "Assets/_MareaAlta/Scenes/MainMenu.unity";
        private const string CompletionMarker =
            "Assets/_MareaAlta/Settings/PresentableDemo01.complete.txt";

        private static bool waitingForEditMode;

        [InitializeOnLoadMethod]
        private static void ScheduleBuild()
        {
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(CompletionMarker) != null)
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

        [MenuItem("Marea Alta/Demo presentable/1 - Crear flujo base")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            CreateGameplayCopy();
            CreateMainMenu();
            ConfigureBuildSettings();

            File.WriteAllText(CompletionMarker, "Flujo base de la demo presentable creado.");
            AssetDatabase.ImportAsset(CompletionMarker);
            AssetDatabase.SaveAssets();
            Debug.Log("Marea Alta: flujo base de la demo presentable creado correctamente.");
        }

        private static void CreateGameplayCopy()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DemoScenePath) == null)
                AssetDatabase.CopyAsset(PrototypeScenePath, DemoScenePath);

            Scene scene = EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);
            GameObject systems = GameObject.Find("Systems") ?? new GameObject("Systems");
            if (systems.GetComponent<PauseController>() == null)
                systems.AddComponent<PauseController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void CreateMainMenu()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuScenePath) != null)
                return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.backgroundColor = new Color(0.01f, 0.06f, 0.14f);

            AddBackground();
            AddDiverDecoration();

            var menu = new GameObject("MainMenu");
            menu.AddComponent<MainMenuController>();

            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }

        private static void AddBackground()
        {
            const string backgroundPath =
                "Assets/ThirdParty/UnderwaterDivingPack/PNG/environment/background.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);
            if (sprite == null)
                return;

            var background = new GameObject("Menu_Background");
            SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.45f, 0.72f, 0.9f);
            renderer.sortingOrder = -10;

            Vector2 size = sprite.bounds.size;
            background.transform.localScale = new Vector3(25f / size.x, 14f / size.y, 1f);
        }

        private static void AddDiverDecoration()
        {
            const string playerPath =
                "Assets/ThirdParty/UnderwaterDivingPack/PNG/player/player-idle.png";
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(playerPath).OfType<Sprite>().FirstOrDefault();
            if (sprite == null)
                return;

            var diver = new GameObject("Menu_Mara");
            SpriteRenderer renderer = diver.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 5;
            diver.transform.position = new Vector3(6.5f, -1.2f, 0f);
            diver.transform.localScale = Vector3.one * 1.35f;
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(DemoScenePath, true)
            };
        }
    }
}
