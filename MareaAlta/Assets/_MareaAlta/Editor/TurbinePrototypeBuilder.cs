using MareaAlta.Interaction;
using MareaAlta.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class TurbinePrototypeBuilder
    {
        private static bool waitingForEditMode;

        private const string ScenePath =
            "Assets/_MareaAlta/Scenes/Prototype_PuntaCoral.unity";
        private const string PlayerPrefabPath =
            "Assets/_MareaAlta/Prefabs/Player/Player_Mara.prefab";
        private const string TurbinePrefabPath =
            "Assets/_MareaAlta/Prefabs/Interactables/Turbine_Prototype.prefab";
        private const string GatePrefabPath =
            "Assets/_MareaAlta/Prefabs/Environment/Gate_Prototype.prefab";

        [InitializeOnLoadMethod]
        private static void ScheduleBuildIfNeeded()
        {
            if (!NeedsVisualUpdate())
                return;

            EditorApplication.delayCall += BuildWhenEditorIsReady;
        }

        private static bool NeedsVisualUpdate()
        {
            GameObject gate = AssetDatabase.LoadAssetAtPath<GameObject>(GatePrefabPath);
            GameObject turbine = AssetDatabase.LoadAssetAtPath<GameObject>(TurbinePrefabPath);
            if (gate == null || turbine == null)
                return true;

            SpriteRenderer gateRenderer = gate.GetComponent<SpriteRenderer>();
            SpriteRenderer turbineRenderer = turbine.GetComponent<SpriteRenderer>();
            return gateRenderer == null || turbineRenderer == null ||
                   gateRenderer.drawMode != SpriteDrawMode.Simple || gate.transform.localScale.y < 20f ||
                   turbineRenderer.drawMode != SpriteDrawMode.Simple || turbine.transform.localScale.x < 5f;
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

        [MenuItem("Marea Alta/Crear fase 5 - Turbina y compuerta")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            if (!AddInteractorToPlayerPrefab())
            {
                Debug.LogError("Marea Alta: primero debe existir Player_Mara.prefab.");
                return;
            }

            GameObject gatePrefab = CreateGatePrefab();
            GameObject turbinePrefab = CreateTurbinePrefab();
            AddProgressionToScene(gatePrefab, turbinePrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Marea Alta: fase 5 creada. Turbina, gasto de energia y compuerta listos.");
        }

        private static bool AddInteractorToPlayerPrefab()
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefabAsset == null)
                return false;

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (prefabRoot.GetComponent<PlayerInteractor>() == null)
                prefabRoot.AddComponent<PlayerInteractor>();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return true;
        }

        private static GameObject CreateGatePrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(GatePrefabPath);
            bool editingExisting = existing != null;
            GameObject gate = editingExisting
                ? PrefabUtility.LoadPrefabContents(GatePrefabPath)
                : new GameObject("Gate_Prototype");

            SpriteRenderer renderer = gate.GetComponent<SpriteRenderer>() ?? gate.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            renderer.color = new Color(0.95f, 0.2f, 0.18f, 0.9f);
            renderer.sortingOrder = 4;
            renderer.drawMode = SpriteDrawMode.Simple;
            Vector2 spriteSize = renderer.sprite.bounds.size;
            gate.transform.localScale = new Vector3(1.1f / spriteSize.x, 13.5f / spriteSize.y, 1f);

            BoxCollider2D collider = gate.GetComponent<BoxCollider2D>() ?? gate.AddComponent<BoxCollider2D>();
            collider.size = spriteSize;
            if (gate.GetComponent<GateController>() == null)
                gate.AddComponent<GateController>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gate, GatePrefabPath);
            if (editingExisting)
                PrefabUtility.UnloadPrefabContents(gate);
            else
                Object.DestroyImmediate(gate);
            return prefab;
        }

        private static GameObject CreateTurbinePrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(TurbinePrefabPath);
            bool editingExisting = existing != null;
            GameObject turbine = editingExisting
                ? PrefabUtility.LoadPrefabContents(TurbinePrefabPath)
                : new GameObject("Turbine_Prototype");

            SpriteRenderer renderer = turbine.GetComponent<SpriteRenderer>() ?? turbine.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            renderer.color = new Color(0.25f, 0.7f, 1f);
            renderer.sortingOrder = 6;
            renderer.drawMode = SpriteDrawMode.Simple;
            Vector2 spriteSize = renderer.sprite.bounds.size;
            turbine.transform.localScale = new Vector3(1.8f / spriteSize.x, 1.8f / spriteSize.y, 1f);
            turbine.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            CircleCollider2D trigger = turbine.GetComponent<CircleCollider2D>() ?? turbine.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = spriteSize.x * 0.85f;
            if (turbine.GetComponent<TurbineController>() == null)
                turbine.AddComponent<TurbineController>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(turbine, TurbinePrefabPath);
            if (editingExisting)
                PrefabUtility.UnloadPrefabContents(turbine);
            else
                Object.DestroyImmediate(turbine);
            return prefab;
        }

        private static void AddProgressionToScene(GameObject gatePrefab, GameObject turbinePrefab)
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Player_Mara");
            if (player != null && player.GetComponent<PlayerInteractor>() == null)
                player.AddComponent<PlayerInteractor>();

            GameObject level = GameObject.Find("Level") ?? new GameObject("Level");
            GameObject section = GetOrCreateChild(level.transform, "Section_01");

            GameObject gate = GameObject.Find("Gate_01");
            if (gate == null)
            {
                gate = PrefabUtility.InstantiatePrefab(gatePrefab, scene) as GameObject;
                gate.name = "Gate_01";
                gate.transform.SetParent(section.transform);
            }
            gate.transform.position = new Vector3(10.8f, 0f, 0f);
            gate.transform.localScale = gatePrefab.transform.localScale;

            GameObject turbine = GameObject.Find("Turbine_01");
            if (turbine == null)
            {
                turbine = PrefabUtility.InstantiatePrefab(turbinePrefab, scene) as GameObject;
                turbine.name = "Turbine_01";
                turbine.transform.SetParent(section.transform);
            }
            turbine.transform.position = new Vector3(8.2f, 3.8f, 0f);
            turbine.transform.localScale = turbinePrefab.transform.localScale;
            turbine.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            GateController gateController = gate.GetComponent<GateController>();
            TurbineController turbineController = turbine.GetComponent<TurbineController>();
            turbineController.Configure(gateController, 1);

            EditorUtility.SetDirty(turbineController);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
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
