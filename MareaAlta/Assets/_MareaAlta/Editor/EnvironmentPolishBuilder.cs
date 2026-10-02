using System.IO;
using System.Linq;
using MareaAlta.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class EnvironmentPolishBuilder
    {
        private const string DemoScenePath =
            "Assets/_MareaAlta/Scenes/Demo_PuntaCoral.unity";
        private const string MenuScenePath =
            "Assets/_MareaAlta/Scenes/MainMenu.unity";
        private const string MidgroundPath =
            "Assets/ThirdParty/UnderwaterDivingPack/PNG/environment/midground.png";
        private const string PropsPath =
            "Assets/ThirdParty/UnderwaterDivingPack/PNG/environment/props.png";
        private const string CompletionMarker =
            "Assets/_MareaAlta/Settings/PresentableDemo03.complete.txt";

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

        [MenuItem("Marea Alta/Demo presentable/3 - Pulir escenario")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            PolishGameplayEnvironment();
            PolishMenuEnvironment();

            File.WriteAllText(CompletionMarker, "Escenario y profundidad visual actualizados.");
            AssetDatabase.ImportAsset(CompletionMarker);
            AssetDatabase.SaveAssets();
            Debug.Log("Marea Alta: ambiente y profundidad visual de la demo actualizados.");
        }

        private static void PolishGameplayEnvironment()
        {
            Scene scene = EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);
            GameObject root = GameObject.Find("Environment_Art") ?? new GameObject("Environment_Art");

            AddMidground(root.transform, "Reef_Midground", -55, new Color(0.72f, 0.9f, 0.9f, 0.72f));
            AddFlora(root.transform);
            SoftenPrototypeGeometry();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void PolishMenuEnvironment()
        {
            Scene scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
            GameObject root = GameObject.Find("Menu_Environment") ?? new GameObject("Menu_Environment");
            AddMidground(root.transform, "Menu_Midground", -5, new Color(0.5f, 0.85f, 0.82f, 0.5f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AddMidground(Transform parent, string name, int order, Color color)
        {
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(MidgroundPath).OfType<Sprite>().FirstOrDefault();
            if (sprite == null)
                return;

            Transform existing = parent.Find(name);
            GameObject layer = existing != null ? existing.gameObject : new GameObject(name);
            layer.transform.SetParent(parent);
            layer.transform.position = Vector3.zero;

            SpriteRenderer renderer = layer.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;

            Vector2 sourceSize = sprite.bounds.size;
            layer.transform.localScale = new Vector3(25f / sourceSize.x, 14f / sourceSize.y, 1f);

            ScenerySway sway = layer.GetComponent<ScenerySway>();
            if (sway == null)
                sway = layer.AddComponent<ScenerySway>();
            sway.Configure(0.04f, 0.18f, 0.35f, 0f);
        }

        private static void AddFlora(Transform parent)
        {
            Sprite[] flora = AssetDatabase.LoadAllAssetsAtPath(PropsPath)
                .OfType<Sprite>()
                .Where(sprite => sprite.name is "props_7" or "props_8" or "props_9")
                .OrderBy(sprite => sprite.name)
                .ToArray();
            if (flora.Length == 0)
                return;

            Vector2[] positions =
            {
                new(-9.6f, -5.4f), new(-5.2f, 4.3f), new(-1.6f, -3.2f),
                new(2.4f, 3.3f), new(5.6f, -4.5f), new(8.2f, 4.6f)
            };

            for (int index = 0; index < positions.Length; index++)
            {
                string name = $"Flora_{index + 1:00}";
                Transform existing = parent.Find(name);
                GameObject plant = existing != null ? existing.gameObject : new GameObject(name);
                plant.transform.SetParent(parent);
                plant.transform.position = positions[index];
                plant.transform.localScale = Vector3.one * (0.8f + (index % 3) * 0.2f);

                SpriteRenderer renderer = plant.GetComponent<SpriteRenderer>();
                if (renderer == null)
                    renderer = plant.AddComponent<SpriteRenderer>();
                renderer.sprite = flora[index % flora.Length];
                renderer.color = new Color(0.7f, 1f, 0.8f, 0.82f);
                renderer.sortingOrder = 3;

                ScenerySway sway = plant.GetComponent<ScenerySway>();
                if (sway == null)
                    sway = plant.AddComponent<ScenerySway>();
                sway.Configure(0.04f, 2.2f, 0.7f + index * 0.06f, index * 0.9f);
            }
        }

        private static void SoftenPrototypeGeometry()
        {
            GameObject geometry = GameObject.Find("Geometry");
            if (geometry == null)
            {
                GameObject section = GameObject.Find("Section_01");
                Transform child = section != null ? section.transform.Find("Geometry") : null;
                geometry = child != null ? child.gameObject : null;
            }

            if (geometry == null)
                return;

            foreach (SpriteRenderer renderer in geometry.GetComponentsInChildren<SpriteRenderer>())
            {
                Color color = renderer.color;
                renderer.color = new Color(color.r * 0.75f, color.g * 0.85f, color.b, 0.58f);
                renderer.sortingOrder = -20;
            }
        }
    }
}
