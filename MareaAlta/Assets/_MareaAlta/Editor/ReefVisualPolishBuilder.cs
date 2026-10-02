using System.IO;
using System.Linq;
using MareaAlta.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class ReefVisualPolishBuilder
    {
        private const string ScenePath = "Assets/_MareaAlta/Scenes/Demo_PuntaCoral.unity";
        private const string MarkerPath = "Assets/_MareaAlta/Settings/ReefVisualPolish.complete.txt";
        private const string TilesPath = "Assets/ThirdParty/UnderwaterDivingPack/PNG/environment/tiles.png";
        private const string PropsPath = "Assets/ThirdParty/UnderwaterDivingPack/PNG/environment/props.png";
        private const string FishPath = "Assets/ThirdParty/UnderwaterDivingPack/PNG/enemies/fish.png";
        private const string DartFishPath = "Assets/ThirdParty/UnderwaterDivingPack/PNG/enemies/fish-dart.png";
        private const string BigFishPath = "Assets/ThirdParty/UnderwaterDivingPack/PNG/enemies/fish-big.png";
        private const string BubblesPath = "Assets/ThirdParty/UnderwaterDivingPack/PNG/FX/bubbles.png";

        private static bool waitingForEditMode;

        [InitializeOnLoadMethod]
        private static void Schedule()
        {
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(MarkerPath) == null)
                EditorApplication.delayCall += BuildWhenReady;
        }

        private static void BuildWhenReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (!waitingForEditMode)
                {
                    waitingForEditMode = true;
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                }
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += BuildWhenReady;
                return;
            }

            Build();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
                return;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            waitingForEditMode = false;
            EditorApplication.delayCall += BuildWhenReady;
        }

        [MenuItem("Marea Alta/Demo ampliada/2 - Vestir arrecifes y ambiente")]
        public static void Build()
        {
            ConfigureSheet(TilesPath);
            ConfigureSheet(PropsPath);
            ConfigureSheet(FishPath);
            ConfigureSheet(DartFishPath);
            ConfigureSheet(BigFishPath);
            ConfigureSheet(BubblesPath);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Sprite[] tiles = AssetDatabase.LoadAllAssetsAtPath(TilesPath).OfType<Sprite>()
                .OrderBy(SpriteIndex).ToArray();
            Sprite[] props = AssetDatabase.LoadAllAssetsAtPath(PropsPath).OfType<Sprite>()
                .OrderBy(SpriteIndex).ToArray();
            if (tiles.Length < 22 || props.Length < 10)
            {
                Debug.LogError("Marea Alta: no se pudieron cargar las piezas del arrecife.");
                return;
            }

            GameObject previous = GameObject.Find("ReefColliderArt");
            if (previous != null)
                Object.DestroyImmediate(previous);
            var artRoot = new GameObject("ReefColliderArt");

            BoxCollider2D[] colliders = Object.FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None)
                .Where(IsEnvironmentCollider)
                .ToArray();
            foreach (BoxCollider2D collider in colliders)
                DressCollider(artRoot.transform, collider, tiles);

            AddRuinLandmarks(artRoot.transform, props);
            AddAmbientFish(artRoot.transform);
            AddBubbleColumns(artRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath,
                $"Arrecifes visibles alineados con {colliders.Length} colliders; ruinas, peces y burbujas añadidos.");
            AssetDatabase.ImportAsset(MarkerPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Marea Alta: {colliders.Length} colliders convertidos en arrecifes visibles.");
        }

        private static void ConfigureSheet(string path)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        private static bool IsEnvironmentCollider(BoxCollider2D collider)
        {
            string name = collider.gameObject.name;
            return collider.enabled &&
                   (name.StartsWith("Boundary_") || name.StartsWith("Reef_") ||
                    name.StartsWith("Ruin_") || name.StartsWith("Trench_"));
        }

        private static void DressCollider(Transform root, BoxCollider2D collider, Sprite[] tiles)
        {
            SpriteRenderer placeholder = collider.GetComponent<SpriteRenderer>();
            if (placeholder != null)
                placeholder.enabled = false;

            Bounds bounds = collider.bounds;
            bool horizontal = bounds.size.x >= bounds.size.y;
            if (horizontal)
                AddHorizontalReef(root, collider.name, bounds, tiles);
            else
                AddVerticalReef(root, collider.name, bounds, tiles);
        }

        private static void AddHorizontalReef(Transform root, string baseName, Bounds bounds, Sprite[] tiles)
        {
            bool ceiling = bounds.center.y > 1f;
            int count = Mathf.Max(1, Mathf.CeilToInt(bounds.size.x / 7f));
            float width = bounds.size.x / count + 0.18f;
            for (int index = 0; index < count; index++)
            {
                Sprite sprite = tiles[(index % 3) switch { 0 => 6, 1 => 7, _ => 8 }];
                float x = bounds.min.x + width * 0.5f + index * (bounds.size.x / count);
                float y = bounds.center.y + (ceiling ? -0.1f : 0.1f);
                GameObject visual = CreateVisual(root, $"{baseName}_Rock_{index + 1:00}", sprite,
                    new Vector2(x, y), new Vector2(width, bounds.size.y + 1.05f), 3);
                SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
                renderer.flipY = ceiling;
                renderer.color = GetZoneTint(bounds.center.x);
                CenterRenderer(renderer, new Vector2(x, y));
            }
        }

        private static void AddVerticalReef(Transform root, string baseName, Bounds bounds, Sprite[] tiles)
        {
            bool rightWall = bounds.center.x > 26f;
            int count = Mathf.Max(1, Mathf.CeilToInt(bounds.size.y / 4.5f));
            float height = bounds.size.y / count + 0.2f;
            for (int index = 0; index < count; index++)
            {
                Sprite sprite = tiles[2 + index % 4];
                float y = bounds.min.y + height * 0.5f + index * (bounds.size.y / count);
                GameObject visual = CreateVisual(root, $"{baseName}_Wall_{index + 1:00}", sprite,
                    new Vector2(bounds.center.x, y), new Vector2(bounds.size.x + 1.05f, height), 3);
                SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
                renderer.flipX = rightWall;
                renderer.color = GetZoneTint(bounds.center.x);
                CenterRenderer(renderer, new Vector2(bounds.center.x, y));
            }
        }

        private static void AddRuinLandmarks(Transform root, Sprite[] props)
        {
            AddPreservedSprite(root, "RuinArch_Large", props[1], new Vector2(26f, 0.1f), 0.54f, 1,
                new Color(0.72f, 0.86f, 0.82f, 0.88f));
            AddPreservedSprite(root, "RuinColumn_Left", props[2], new Vector2(17.4f, -1.6f), 0.38f, 2,
                new Color(0.72f, 0.9f, 0.78f, 0.9f));
            AddPreservedSprite(root, "RuinColumn_Right", props[3], new Vector2(34f, -1.3f), 0.36f, 2,
                new Color(0.66f, 0.82f, 0.72f, 0.88f));
            AddPreservedSprite(root, "TrenchArch", props[4], new Vector2(52f, 0f), 0.48f, 1,
                new Color(0.48f, 0.7f, 0.72f, 0.82f));
        }

        private static void AddAmbientFish(Transform root)
        {
            AddFishSchool(root, "ReefFish", FishPath, new Vector2(-2f, 1.2f), 7f, 0.28f, 0f, 0.85f);
            AddFishSchool(root, "DartFish", DartFishPath, new Vector2(24f, -1.2f), 9f, 0.22f, 0.7f, 0.9f);
            AddFishSchool(root, "DeepFish", BigFishPath, new Vector2(50f, 0.3f), 8f, 0.16f, 1.4f, 0.75f);
        }

        private static void AddFishSchool(Transform root, string name, string path, Vector2 position,
            float distance, float speed, float phase, float scale)
        {
            Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                .OrderBy(sprite => sprite.name).ToArray();
            if (frames.Length == 0)
                return;

            var fish = new GameObject(name);
            fish.transform.SetParent(root);
            fish.transform.position = position;
            fish.transform.localScale = Vector3.one * scale;
            SpriteRenderer renderer = fish.AddComponent<SpriteRenderer>();
            renderer.sprite = frames[0];
            renderer.color = new Color(0.78f, 0.92f, 1f, 0.8f);
            renderer.sortingOrder = 1;
            CenterRenderer(renderer, position);
            AmbientFish movement = fish.AddComponent<AmbientFish>();
            movement.Configure(frames, distance, speed, phase);
        }

        private static void AddBubbleColumns(Transform root)
        {
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(BubblesPath).OfType<Sprite>().FirstOrDefault();
            if (sprite == null)
                return;

            Vector2[] positions = { new(18f, -5.5f), new(29f, -5.8f), new(45f, -5.6f), new(58f, -5.7f) };
            for (int index = 0; index < positions.Length; index++)
            {
                GameObject bubble = CreateVisual(root, $"ExpandedBubble_{index + 1:00}", sprite,
                    positions[index], new Vector2(0.75f, 0.75f), 0);
                bubble.GetComponent<SpriteRenderer>().color = new Color(0.65f, 0.9f, 1f, 0.28f);
                AmbientBubble drift = bubble.AddComponent<AmbientBubble>();
                drift.Configure(0.32f + index * 0.04f, 0.12f, index * 0.8f);
            }
        }

        private static GameObject CreateVisual(Transform parent, string name, Sprite sprite, Vector2 position,
            Vector2 targetSize, int sortingOrder)
        {
            var visual = new GameObject(name);
            visual.transform.SetParent(parent);
            visual.transform.position = position;
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            Vector2 size = sprite.bounds.size;
            visual.transform.localScale = new Vector3(targetSize.x / size.x, targetSize.y / size.y, 1f);
            CenterRenderer(renderer, position);
            return visual;
        }

        private static void AddPreservedSprite(Transform root, string name, Sprite sprite, Vector2 position,
            float scale, int sortingOrder, Color color)
        {
            var visual = new GameObject(name);
            visual.transform.SetParent(root);
            visual.transform.position = position;
            visual.transform.localScale = Vector3.one * scale;
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
            CenterRenderer(renderer, position);
        }

        private static void CenterRenderer(SpriteRenderer renderer, Vector2 desiredCenter)
        {
            Vector3 correction = (Vector3)desiredCenter - renderer.bounds.center;
            renderer.transform.position += correction;
        }

        private static int SpriteIndex(Sprite sprite)
        {
            string[] parts = sprite.name.Split('_');
            return parts.Length > 1 && int.TryParse(parts[^1], out int index) ? index : int.MaxValue;
        }

        private static Color GetZoneTint(float x)
        {
            if (x < 13f)
                return new Color(0.78f, 1f, 0.82f, 1f);
            if (x < 39f)
                return new Color(0.68f, 0.88f, 0.78f, 1f);
            return new Color(0.52f, 0.76f, 0.76f, 1f);
        }
    }
}
