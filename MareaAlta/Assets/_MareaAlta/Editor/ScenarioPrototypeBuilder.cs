using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class ScenarioPrototypeBuilder
    {
        private static bool waitingForEditMode;

        private const string ScenePath =
            "Assets/_MareaAlta/Scenes/Prototype_PuntaCoral.unity";
        private const string BlockPrefabPath =
            "Assets/_MareaAlta/Prefabs/Environment/EnvironmentBlock_Prototype.prefab";
        private const string BuildMarkerPath =
            "Assets/_MareaAlta/Settings/Phase6.complete.txt";

        private readonly struct BlockDefinition
        {
            public BlockDefinition(string name, Vector2 position, Vector2 size, Color color)
            {
                Name = name;
                Position = position;
                Size = size;
                Color = color;
            }

            public string Name { get; }
            public Vector2 Position { get; }
            public Vector2 Size { get; }
            public Color Color { get; }
        }

        [InitializeOnLoadMethod]
        private static void ScheduleFirstBuild()
        {
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(BuildMarkerPath) != null)
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

        [MenuItem("Marea Alta/Crear fase 6 - Escenario")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            GameObject blockPrefab = CreateBlockPrefab();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject level = GameObject.Find("Level") ?? new GameObject("Level");
            GameObject section = GetOrCreateChild(level.transform, "Section_01");
            GameObject geometry = GetOrCreateChild(section.transform, "Geometry");

            foreach (BlockDefinition block in GetBlockDefinitions())
                AddOrUpdateBlock(scene, geometry.transform, blockPrefab, block);

            PositionGameplayObjects();
            AddZoneMarkers(section.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EnsureFolder("Assets/_MareaAlta/Settings");
            File.WriteAllText(BuildMarkerPath, "Fase 6 generada correctamente.");
            AssetDatabase.ImportAsset(BuildMarkerPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Marea Alta: fase 6 creada. Recorrido provisional y obstaculos listos.");
        }

        private static GameObject CreateBlockPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(BlockPrefabPath);
            if (existing != null)
                return existing;

            var block = new GameObject("EnvironmentBlock_Prototype");
            SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.color = new Color(0.05f, 0.3f, 0.42f);
            renderer.sortingOrder = 2;

            BoxCollider2D collider = block.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(block, BlockPrefabPath);
            Object.DestroyImmediate(block);
            return prefab;
        }

        private static IEnumerable<BlockDefinition> GetBlockDefinitions()
        {
            Color rock = new(0.04f, 0.28f, 0.38f, 0.98f);
            Color coral = new(0.06f, 0.42f, 0.48f, 0.98f);

            return new[]
            {
                new BlockDefinition("Reef_EntranceTop", new Vector2(-8.2f, 4.6f), new Vector2(6f, 1.5f), rock),
                new BlockDefinition("Reef_EntranceBottom", new Vector2(-6.7f, -4.8f), new Vector2(7f, 1.4f), coral),
                new BlockDefinition("Reef_MiddleTop", new Vector2(-1.5f, 3.7f), new Vector2(4.5f, 1.6f), coral),
                new BlockDefinition("Reef_MiddleBottom", new Vector2(1.2f, -3.8f), new Vector2(5.2f, 1.5f), rock),
                new BlockDefinition("Reef_FinalTop", new Vector2(5.2f, 5.1f), new Vector2(5.5f, 1.3f), rock),
                new BlockDefinition("Reef_FinalBottom", new Vector2(6.2f, -5.1f), new Vector2(4.8f, 1.3f), coral),
                new BlockDefinition("Rock_Island", new Vector2(4.2f, 0.5f), new Vector2(2f, 2.6f), rock),
                new BlockDefinition("Gate_FrameTop", new Vector2(9.6f, 6.8f), new Vector2(2.2f, 1.2f), coral),
                new BlockDefinition("Gate_FrameBottom", new Vector2(9.6f, -6.8f), new Vector2(2.2f, 1.2f), coral)
            };
        }

        private static void AddOrUpdateBlock(
            Scene scene,
            Transform parent,
            GameObject prefab,
            BlockDefinition definition)
        {
            Transform existing = parent.Find(definition.Name);
            GameObject block = existing != null
                ? existing.gameObject
                : PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;

            if (block == null)
                return;

            block.name = definition.Name;
            block.transform.SetParent(parent);
            block.transform.position = definition.Position;
            SpriteRenderer renderer = block.GetComponent<SpriteRenderer>();
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.color = definition.Color;

            Vector2 spriteSize = renderer.sprite.bounds.size;
            block.transform.localScale = new Vector3(
                definition.Size.x / spriteSize.x,
                definition.Size.y / spriteSize.y,
                1f);

            BoxCollider2D collider = block.GetComponent<BoxCollider2D>();
            collider.size = spriteSize;
        }

        private static void PositionGameplayObjects()
        {
            SetPosition("Player_Mara", new Vector3(-10.5f, 0f, 0f));
            SetPosition("Turbine_01", new Vector3(7.4f, 2.7f, 0f));
            SetPosition("Gate_01", new Vector3(9.6f, 0f, 0f));

            Vector2[] debrisPositions =
            {
                new(-9f, 2.2f),
                new(-6.8f, -2.4f),
                new(-3.5f, 0.8f),
                new(0.2f, -1.6f),
                new(4.8f, 2.5f),
                new(7.2f, -2.2f)
            };

            for (int index = 0; index < debrisPositions.Length; index++)
                SetPosition($"Debris_{index + 1:00}", debrisPositions[index]);
        }

        private static void AddZoneMarkers(Transform section)
        {
            GameObject entrance = GetOrCreateChild(section, "Entrance_Marker");
            ConfigureMarker(entrance, new Vector2(-11.6f, 0f), new Vector2(0.25f, 5f),
                new Color(0.2f, 0.75f, 1f, 0.55f));

            GameObject exit = GetOrCreateChild(section, "Exit_Preview");
            ConfigureMarker(exit, new Vector2(11.5f, 0f), new Vector2(0.3f, 5f),
                new Color(0.25f, 1f, 0.55f, 0.6f));
        }

        private static void ConfigureMarker(GameObject marker, Vector2 position, Vector2 size, Color color)
        {
            marker.transform.position = position;
            SpriteRenderer renderer = marker.GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.color = color;
            renderer.sortingOrder = 3;

            Vector2 spriteSize = renderer.sprite.bounds.size;
            marker.transform.localScale = new Vector3(
                size.x / spriteSize.x,
                size.y / spriteSize.y,
                1f);
        }

        private static void EnsureFolder(string folderPath)
        {
            string[] parts = folderPath.Split('/');
            string currentPath = parts[0];

            for (int index = 1; index < parts.Length; index++)
            {
                string nextPath = $"{currentPath}/{parts[index]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                    AssetDatabase.CreateFolder(currentPath, parts[index]);
                currentPath = nextPath;
            }
        }

        private static void SetPosition(string objectName, Vector3 position)
        {
            GameObject target = GameObject.Find(objectName);
            if (target != null)
                target.transform.position = position;
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
