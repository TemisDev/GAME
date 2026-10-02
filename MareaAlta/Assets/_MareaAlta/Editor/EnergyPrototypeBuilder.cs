using MareaAlta.Resources;
using MareaAlta.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class EnergyPrototypeBuilder
    {
        private const string ScenePath =
            "Assets/_MareaAlta/Scenes/Prototype_PuntaCoral.unity";
        private const string PlayerPrefabPath =
            "Assets/_MareaAlta/Prefabs/Player/Player_Mara.prefab";

        [InitializeOnLoadMethod]
        private static void ScheduleBuildIfNeeded()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab == null || playerPrefab.GetComponent<EnergyInventory>() != null)
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

        [MenuItem("Marea Alta/Crear fase 4 - Energia")]
        public static void Build()
        {
            if (!AddEnergyToPlayerPrefab())
            {
                Debug.LogError("Marea Alta: primero debe existir Player_Mara.prefab.");
                return;
            }

            AddEnergyToScenePlayer();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Marea Alta: fase 4 creada. Conversion 3:1 y HUD de energia listos.");
        }

        private static bool AddEnergyToPlayerPrefab()
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefabAsset == null)
                return false;

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (prefabRoot.GetComponent<EnergyInventory>() == null)
                prefabRoot.AddComponent<EnergyInventory>();
            if (prefabRoot.GetComponent<PrototypeHUD>() == null)
                prefabRoot.AddComponent<PrototypeHUD>();

            FragmentInventory oldInventory = prefabRoot.GetComponent<FragmentInventory>();
            if (oldInventory != null)
                Object.DestroyImmediate(oldInventory);

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return true;
        }

        private static void AddEnergyToScenePlayer()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Player_Mara");
            if (player == null)
                return;

            if (player.GetComponent<EnergyInventory>() == null)
                player.AddComponent<EnergyInventory>();
            if (player.GetComponent<PrototypeHUD>() == null)
                player.AddComponent<PrototypeHUD>();

            FragmentInventory oldInventory = player.GetComponent<FragmentInventory>();
            if (oldInventory != null)
                Object.DestroyImmediate(oldInventory);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
