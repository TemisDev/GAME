using MareaAlta.Core;
using MareaAlta.UI;
using MareaAlta.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class ClosurePrototypeBuilder
    {
        private const string ScenePath =
            "Assets/_MareaAlta/Scenes/Prototype_PuntaCoral.unity";
        private const string ExitPrefabPath =
            "Assets/_MareaAlta/Prefabs/Interactables/Exit_Prototype.prefab";

        private static bool waitingForEditMode;

        [InitializeOnLoadMethod]
        private static void ScheduleFirstBuild()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ExitPrefabPath) != null)
                return;

            EditorApplication.delayCall += BuildWhenEditorIsReady;
        }

        private static void BuildWhenEditorIsReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += BuildWhenEditorIsReady;
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
            EditorApplication.delayCall += BuildWhenEditorIsReady;
        }

        [MenuItem("Marea Alta/Crear fase 7 - Cierre")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            GameObject exitPrefab = CreateExitPrefab();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject systems = GameObject.Find("Systems") ?? new GameObject("Systems");
            GameSession session = systems.GetComponent<GameSession>();
            if (session == null)
                session = systems.AddComponent<GameSession>();
            if (systems.GetComponent<ResultsOverlay>() == null)
                systems.AddComponent<ResultsOverlay>();

            GameObject level = GameObject.Find("Level") ?? new GameObject("Level");
            GameObject section = GetOrCreateChild(level.transform, "Section_01");
            GameObject exit = GameObject.Find("Exit_Trigger");
            if (exit == null)
            {
                exit = PrefabUtility.InstantiatePrefab(exitPrefab, scene) as GameObject;
                exit.name = "Exit_Trigger";
                exit.transform.SetParent(section.transform);
            }

            exit.transform.position = new Vector3(11.5f, 0f, 0f);
            exit.GetComponent<ExitTrigger>().Configure(session);
            EditorUtility.SetDirty(exit.GetComponent<ExitTrigger>());

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Marea Alta: fase 7 creada. Victoria, resultados y reinicio listos.");
        }

        private static GameObject CreateExitPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(ExitPrefabPath);
            if (existing != null)
                return existing;

            var exit = new GameObject("Exit_Prototype");
            BoxCollider2D trigger = exit.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1f, 5f);
            exit.AddComponent<ExitTrigger>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(exit, ExitPrefabPath);
            Object.DestroyImmediate(exit);
            return prefab;
        }

        private static GameObject GetOrCreateChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
                return existing.gameObject;

            var child = new GameObject(name);
            child.transform.SetParent(parent);
            return child;
        }
    }
}
