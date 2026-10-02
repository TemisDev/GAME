using System.IO;
using System.Linq;
using MareaAlta.UI;
using MareaAlta.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class PresentationPolishBuilder
    {
        private const string DemoScenePath =
            "Assets/_MareaAlta/Scenes/Demo_PuntaCoral.unity";
        private const string MenuScenePath =
            "Assets/_MareaAlta/Scenes/MainMenu.unity";
        private const string CompletionMarker =
            "Assets/_MareaAlta/Settings/PresentableDemo02.complete.txt";

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

        [MenuItem("Marea Alta/Demo presentable/2 - Pulir interfaz y ambiente")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            PolishMenu();
            PolishGameplay();

            File.WriteAllText(CompletionMarker, "Interfaz y ambiente de la demo presentable creados.");
            AssetDatabase.ImportAsset(CompletionMarker);
            AssetDatabase.SaveAssets();
            Debug.Log("Marea Alta: interfaz y ambiente de la demo presentable actualizados.");
        }

        private static void PolishMenu()
        {
            Scene scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
            GameObject mara = GameObject.Find("Menu_Mara");
            if (mara != null && mara.GetComponent<MenuFloatEffect>() == null)
                mara.AddComponent<MenuFloatEffect>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void PolishGameplay()
        {
            Scene scene = EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);
            GameObject root = GameObject.Find("Ambient_Bubbles") ?? new GameObject("Ambient_Bubbles");

            const string bubblesPath =
                "Assets/ThirdParty/UnderwaterDivingPack/PNG/FX/bubbles.png";
            Sprite bubbleSprite = AssetDatabase.LoadAllAssetsAtPath(bubblesPath).OfType<Sprite>().FirstOrDefault();
            if (bubbleSprite != null)
            {
                Vector2[] positions =
                {
                    new(-10f, -5.5f), new(-7f, -1f), new(-3f, -6f),
                    new(1f, -2.8f), new(5.5f, -5.8f), new(8f, -1.5f)
                };

                for (int index = 0; index < positions.Length; index++)
                {
                    string name = $"BubbleColumn_{index + 1:00}";
                    Transform existing = root.transform.Find(name);
                    GameObject bubble = existing != null ? existing.gameObject : new GameObject(name);
                    bubble.transform.SetParent(root.transform);
                    bubble.transform.position = positions[index];
                    bubble.transform.localScale = Vector3.one * (0.45f + index * 0.06f);

                    SpriteRenderer renderer = bubble.GetComponent<SpriteRenderer>();
                    if (renderer == null)
                        renderer = bubble.AddComponent<SpriteRenderer>();
                    renderer.sprite = bubbleSprite;
                    renderer.color = new Color(0.65f, 0.9f, 1f, 0.22f);
                    renderer.sortingOrder = 1;

                    AmbientBubble drift = bubble.GetComponent<AmbientBubble>();
                    if (drift == null)
                        drift = bubble.AddComponent<AmbientBubble>();
                    drift.Configure(0.3f + index * 0.06f, 0.12f, index * 0.8f);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
