using MareaAlta.Interaction;
using MareaAlta.Resources;
using MareaAlta.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class CollectionPrototypeBuilder
    {
        private const string ScenePath =
            "Assets/_MareaAlta/Scenes/Prototype_PuntaCoral.unity";
        private const string PlayerPrefabPath =
            "Assets/_MareaAlta/Prefabs/Player/Player_Mara.prefab";
        private const string DebrisPrefabPath =
            "Assets/_MareaAlta/Prefabs/Interactables/Debris_Prototype.prefab";

        [InitializeOnLoadMethod]
        private static void ScheduleFirstBuild()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(DebrisPrefabPath) != null)
                return;

            EditorApplication.delayCall += BuildWhenEditorIsReady;
        }

        private static void BuildWhenEditorIsReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += BuildWhenEditorIsReady;
                return;
            }

            Build();
        }

        [MenuItem("Marea Alta/Crear fase 3 - Recoleccion")]
        public static void Build()
        {
            GameObject playerPrefab = AddInventoryToPlayerPrefab();
            if (playerPrefab == null)
            {
                Debug.LogError("Marea Alta: primero debe existir Player_Mara.prefab.");
                return;
            }

            GameObject debrisPrefab = CreateDebrisPrefab();
            AddCollectionToScene(debrisPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Marea Alta: fase 3 creada. Seis residuos, recoleccion unica y contador listos.");
        }

        private static GameObject AddInventoryToPlayerPrefab()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (prefabRoot == null)
                return null;

            if (prefabRoot.GetComponent<EnergyInventory>() == null)
                prefabRoot.AddComponent<EnergyInventory>();
            if (prefabRoot.GetComponent<PrototypeHUD>() == null)
                prefabRoot.AddComponent<PrototypeHUD>();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        }

        private static GameObject CreateDebrisPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(DebrisPrefabPath);
            if (existing != null)
                return existing;

            var debris = new GameObject("Debris_Prototype");
            SpriteRenderer renderer = debris.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            renderer.color = new Color(1f, 0.68f, 0.12f);
            renderer.sortingOrder = 5;
            debris.transform.localScale = Vector3.one * 0.6f;

            CircleCollider2D trigger = debris.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.6f;
            debris.AddComponent<DebrisCollectible>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(debris, DebrisPrefabPath);
            Object.DestroyImmediate(debris);
            return prefab;
        }

        private static void AddCollectionToScene(GameObject debrisPrefab)
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Player_Mara");
            if (player != null)
            {
                if (player.GetComponent<EnergyInventory>() == null)
                    player.AddComponent<EnergyInventory>();
                if (player.GetComponent<PrototypeHUD>() == null)
                    player.AddComponent<PrototypeHUD>();
            }

            GameObject level = GetOrCreateRoot("Level");
            GameObject section = GetOrCreateChild(level.transform, "Section_01");
            GameObject debrisRoot = GetOrCreateChild(section.transform, "Debris");

            Vector2[] positions =
            {
                new(-8f, 3f), new(-4.5f, -2.5f), new(-1f, 2f),
                new(3f, -3f), new(6f, 2.5f), new(9f, -1f)
            };

            for (int index = 0; index < positions.Length; index++)
            {
                string objectName = $"Debris_{index + 1:00}";
                if (debrisRoot.transform.Find(objectName) != null)
                    continue;

                GameObject instance = PrefabUtility.InstantiatePrefab(debrisPrefab, scene) as GameObject;
                if (instance == null)
                    continue;

                instance.name = objectName;
                instance.transform.SetParent(debrisRoot.transform);
                instance.transform.position = positions[index];
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject GetOrCreateRoot(string name)
        {
            return GameObject.Find(name) ?? new GameObject(name);
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
