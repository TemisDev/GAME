using System.IO;
using System.Linq;
using MareaAlta.Core;
using MareaAlta.Interaction;
using MareaAlta.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class ExpandedDemoBuilder
    {
        private const string ScenePath = "Assets/_MareaAlta/Scenes/Demo_PuntaCoral.unity";
        private const string MarkerPath = "Assets/_MareaAlta/Settings/ExpandedDemo02.complete.txt";
        private const string DebrisArtPath = "Assets/_MareaAlta/Art/Generated/DebrisCluster.png";
        private const string TurbineArtPath = "Assets/_MareaAlta/Art/Generated/EnergyTurbine.png";
        private const string GateArtPath = "Assets/_MareaAlta/Art/Generated/BulkheadGate.png";
        private const string BackgroundPath = "Assets/ThirdParty/UnderwaterDivingPack/PNG/environment/background.png";
        private const string MidgroundPath = "Assets/ThirdParty/UnderwaterDivingPack/PNG/environment/midground.png";
        private const string PropsPath = "Assets/ThirdParty/UnderwaterDivingPack/PNG/environment/props.png";

        private static bool waitingForEditMode;

        [InitializeOnLoadMethod]
        private static void ScheduleBuild()
        {
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(MarkerPath) == null)
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

        [MenuItem("Marea Alta/Demo ampliada/1 - Crear tres zonas")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WaitForEditMode();
                return;
            }

            ConfigureSprite(DebrisArtPath, 100f);
            ConfigureSprite(TurbineArtPath, 100f);
            ConfigureSprite(GateArtPath, 100f);

            Sprite debrisSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DebrisArtPath);
            Sprite turbineSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TurbineArtPath);
            Sprite gateSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GateArtPath);
            if (debrisSprite == null || turbineSprite == null || gateSprite == null)
            {
                Debug.LogError("Marea Alta: los sprites de la demo ampliada todavía se están importando.");
                EditorApplication.delayCall += BuildWhenReady;
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            DisableOldPrototypeLevel();

            GameObject oldExpanded = GameObject.Find("ExpandedWorld");
            if (oldExpanded != null)
                Object.DestroyImmediate(oldExpanded);

            var world = new GameObject("ExpandedWorld");
            AddZoneArt(world.transform);
            AddWorldBounds(world.transform);
            AddZoneGeometry(world.transform);
            AddDebris(world.transform, debrisSprite);

            GateController gateOne = AddGate(world.transform, "Gate_01_Expanded", new Vector2(13f, 0f), gateSprite);
            GateController gateTwo = AddGate(world.transform, "Gate_02_Expanded", new Vector2(39f, 0f), gateSprite);
            GateController gateThree = AddGate(world.transform, "Gate_03_Expanded", new Vector2(63f, 0f), gateSprite);
            AddTurbine(world.transform, "Turbine_01_Expanded", new Vector2(9.2f, 2.8f), turbineSprite, gateOne);
            AddTurbine(world.transform, "Turbine_02_Expanded", new Vector2(35.2f, -2.6f), turbineSprite, gateTwo);
            AddTurbine(world.transform, "Turbine_03_Expanded", new Vector2(59f, 2.5f), turbineSprite, gateThree);
            AddExit(world.transform);
            AddDecorations(world.transform);
            ConfigurePlayerAndCamera();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath,
                "Demo ampliada v2: tres fases completas, escalas persistentes y compuertas sincronizadas.");
            AssetDatabase.ImportAsset(MarkerPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Marea Alta: demo ampliada de tres zonas creada correctamente.");
        }

        private static void ConfigureSprite(string path, float pixelsPerUnit)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static void DisableOldPrototypeLevel()
        {
            GameObject oldLevel = GameObject.Find("Level");
            if (oldLevel != null)
                oldLevel.SetActive(false);

            GameObject legacyRightBoundary = GameObject.Find("Right");
            if (legacyRightBoundary != null &&
                Mathf.Abs(legacyRightBoundary.transform.position.x - 13f) < 0.1f)
            {
                Collider2D legacyCollider = legacyRightBoundary.GetComponent<Collider2D>();
                if (legacyCollider != null)
                    legacyCollider.enabled = false;
            }
        }

        private static void AddZoneArt(Transform parent)
        {
            Sprite background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            Sprite midground = AssetDatabase.LoadAllAssetsAtPath(MidgroundPath).OfType<Sprite>().FirstOrDefault();
            float[] centers = { 0f, 26f, 52f };
            Color[] colors =
            {
                new(0.54f, 0.8f, 1f),
                new(0.36f, 0.68f, 0.82f),
                new(0.24f, 0.48f, 0.7f)
            };

            for (int index = 0; index < centers.Length; index++)
            {
                if (background != null)
                    AddScaledSprite(parent, $"Zone_{index + 1}_Background", background,
                        new Vector2(centers[index], 0f), new Vector2(27f, 15f), colors[index], -100);
                if (midground != null)
                    AddScaledSprite(parent, $"Zone_{index + 1}_Midground", midground,
                        new Vector2(centers[index], 0f), new Vector2(26f, 14f),
                        new Color(colors[index].r, colors[index].g, colors[index].b, 0.62f), -50);
            }
        }

        private static void AddWorldBounds(Transform parent)
        {
            AddBlock(parent, "Boundary_Top", new Vector2(26f, 7.25f), new Vector2(80f, 1f), new Color(0.02f, 0.12f, 0.2f, 0.18f));
            AddBlock(parent, "Boundary_Bottom", new Vector2(26f, -7.25f), new Vector2(80f, 1f), new Color(0.02f, 0.12f, 0.2f, 0.18f));
            AddBlock(parent, "Boundary_Left", new Vector2(-13.5f, 0f), new Vector2(1f, 15f), new Color(0.02f, 0.12f, 0.2f, 0.12f));
            AddBlock(parent, "Boundary_Right", new Vector2(68.5f, 0f), new Vector2(1f, 15f), new Color(0.02f, 0.12f, 0.2f, 0.12f));
        }

        private static void AddZoneGeometry(Transform parent)
        {
            Color reef = new(0.05f, 0.34f, 0.42f, 0.5f);
            Color ruins = new(0.08f, 0.28f, 0.38f, 0.52f);
            Color trench = new(0.04f, 0.2f, 0.34f, 0.58f);

            AddBlock(parent, "Reef_01", new Vector2(-7f, 4.9f), new Vector2(6f, 1.4f), reef);
            AddBlock(parent, "Reef_02", new Vector2(-4f, -4.8f), new Vector2(7f, 1.5f), reef);
            AddBlock(parent, "Reef_03", new Vector2(4f, 3.8f), new Vector2(4.2f, 1.4f), reef);
            AddBlock(parent, "Reef_04", new Vector2(7f, -4.4f), new Vector2(5f, 1.5f), reef);

            AddBlock(parent, "Ruin_01", new Vector2(18f, 4.6f), new Vector2(6f, 1.4f), ruins);
            AddBlock(parent, "Ruin_02", new Vector2(21f, -4.7f), new Vector2(5f, 1.5f), ruins);
            AddBlock(parent, "Ruin_Pillar", new Vector2(27f, 0.2f), new Vector2(1.8f, 4.1f), ruins);
            AddBlock(parent, "Ruin_03", new Vector2(32f, 4.7f), new Vector2(5f, 1.4f), ruins);
            AddBlock(parent, "Ruin_04", new Vector2(34f, -5f), new Vector2(4.5f, 1.3f), ruins);

            AddBlock(parent, "Trench_01", new Vector2(44f, 4.9f), new Vector2(5f, 1.3f), trench);
            AddBlock(parent, "Trench_02", new Vector2(46f, -4.9f), new Vector2(6f, 1.4f), trench);
            AddBlock(parent, "Trench_03", new Vector2(54f, 3.6f), new Vector2(4f, 1.3f), trench);
            AddBlock(parent, "Trench_04", new Vector2(57f, -4.5f), new Vector2(5.5f, 1.4f), trench);
        }

        private static void AddDebris(Transform parent, Sprite sprite)
        {
            Vector2[] positions =
            {
                new(-8.5f, 2.2f), new(-3.2f, -2.1f), new(5.8f, 1.2f),
                new(17.2f, -2.1f), new(23.8f, 2.4f), new(32.8f, 1.2f),
                new(43.2f, 1.8f), new(49.5f, -2.3f), new(56.1f, 0.8f)
            };

            var root = new GameObject("Collectibles");
            root.transform.SetParent(parent);
            for (int index = 0; index < positions.Length; index++)
            {
                var item = new GameObject($"Debris_Expanded_{index + 1:00}");
                item.transform.SetParent(root.transform);
                item.transform.position = positions[index];
                SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = Color.white;
                renderer.sortingOrder = 8;
                float scale = 1.15f / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
                item.transform.localScale = new Vector3(scale, scale, 1f);
                CircleCollider2D trigger = item.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y) * 0.42f;
                item.AddComponent<DebrisCollectible>();
            }
        }

        private static GateController AddGate(Transform parent, string name, Vector2 position, Sprite sprite)
        {
            var gate = new GameObject(name);
            gate.transform.SetParent(parent);
            gate.transform.position = position;
            var visual = new GameObject("GateVisual");
            visual.transform.SetParent(gate.transform);
            visual.transform.localPosition = Vector3.zero;
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = 7;
            Vector2 size = sprite.bounds.size;
            visual.transform.localScale = new Vector3(2.7f / size.x, 12.2f / size.y, 1f);
            BoxCollider2D collider = gate.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(2.25f, 11.2f);
            GateController controller = gate.AddComponent<GateController>();
            controller.ConfigureVisual(visual.transform, renderer);
            return controller;
        }

        private static void AddTurbine(Transform parent, string name, Vector2 position, Sprite sprite, GateController gate)
        {
            var turbine = new GameObject(name);
            turbine.transform.SetParent(parent);
            turbine.transform.position = position;
            SpriteRenderer renderer = turbine.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = 9;
            float scale = 3f / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            turbine.transform.localScale = new Vector3(scale, scale, 1f);
            CircleCollider2D trigger = turbine.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y) * 0.7f;
            TurbineController controller = turbine.AddComponent<TurbineController>();
            controller.Configure(gate, 1);
            EditorUtility.SetDirty(controller);
        }

        private static void AddExit(Transform parent)
        {
            GameSession session = Object.FindFirstObjectByType<GameSession>();
            var exit = new GameObject("Exit_Expanded");
            exit.transform.SetParent(parent);
            exit.transform.position = new Vector3(66.5f, 0f, 0f);
            BoxCollider2D trigger = exit.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.1f, 7f);
            ExitTrigger exitTrigger = exit.AddComponent<ExitTrigger>();
            exitTrigger.Configure(session);

            Sprite markerSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var markerObject = new GameObject("ExitBeaconVisual");
            markerObject.transform.SetParent(exit.transform);
            markerObject.transform.localPosition = Vector3.zero;
            SpriteRenderer marker = markerObject.AddComponent<SpriteRenderer>();
            marker.sprite = markerSprite;
            marker.color = new Color(0.2f, 1f, 0.65f, 0.72f);
            marker.sortingOrder = 5;
            Vector2 markerSize = markerSprite.bounds.size;
            markerObject.transform.localScale = new Vector3(0.32f / markerSize.x, 6f / markerSize.y, 1f);
        }

        private static void AddDecorations(Transform parent)
        {
            Sprite[] props = AssetDatabase.LoadAllAssetsAtPath(PropsPath).OfType<Sprite>().ToArray();
            if (props.Length == 0)
                return;

            Vector2[] positions =
            {
                new(-10f, -5.5f), new(1f, -5.2f), new(16f, 5.1f),
                new(24f, -5.3f), new(43f, -5.4f), new(51f, 4.9f), new(60f, -5.2f)
            };

            Sprite[] flora = props.Where(sprite => sprite.name is "props_7" or "props_8" or "props_9").ToArray();
            if (flora.Length == 0)
                flora = props;

            for (int index = 0; index < positions.Length; index++)
            {
                var decoration = new GameObject($"Expanded_Flora_{index + 1:00}");
                decoration.transform.SetParent(parent);
                decoration.transform.position = positions[index];
                decoration.transform.localScale = Vector3.one * (0.8f + 0.15f * (index % 3));
                SpriteRenderer renderer = decoration.AddComponent<SpriteRenderer>();
                renderer.sprite = flora[index % flora.Length];
                renderer.color = new Color(0.62f, 0.95f, 0.82f, 0.9f);
                renderer.sortingOrder = 3;
                ScenerySway sway = decoration.AddComponent<ScenerySway>();
                sway.Configure(0.04f, 2f, 0.65f + index * 0.05f, index * 0.7f);
            }
        }

        private static void ConfigurePlayerAndCamera()
        {
            GameObject player = GameObject.Find("Player_Mara");
            if (player != null)
                player.transform.position = new Vector3(-10.5f, 0f, 0f);

            Camera camera = Camera.main;
            if (camera == null)
                return;

            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.transform.position = new Vector3(-1f, 0f, -10f);
            CameraFollow2D follow = camera.GetComponent<CameraFollow2D>();
            if (follow == null)
                follow = camera.gameObject.AddComponent<CameraFollow2D>();
            follow.Configure(player != null ? player.transform : null, new Vector2(-1f, 0f), new Vector2(56f, 0f));
            EditorUtility.SetDirty(follow);
        }

        private static void AddScaledSprite(Transform parent, string name, Sprite sprite, Vector2 position,
            Vector2 targetSize, Color color, int sortingOrder)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent);
            item.transform.position = position;
            SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            Vector2 size = sprite.bounds.size;
            item.transform.localScale = new Vector3(targetSize.x / size.x, targetSize.y / size.y, 1f);
        }

        private static void AddBlock(Transform parent, string name, Vector2 position, Vector2 targetSize, Color color)
        {
            var block = new GameObject(name);
            block.transform.SetParent(parent);
            block.transform.position = position;
            SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            renderer.color = color;
            renderer.sortingOrder = -10;
            Vector2 spriteSize = renderer.sprite.bounds.size;
            block.transform.localScale = new Vector3(targetSize.x / spriteSize.x, targetSize.y / spriteSize.y, 1f);
            BoxCollider2D collider = block.AddComponent<BoxCollider2D>();
            collider.size = spriteSize;
        }
    }
}
