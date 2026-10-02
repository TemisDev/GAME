using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace MareaAlta.EditorTools
{
    public static class UnderwaterDivingPackSetup
    {
        private const string PackRoot = "Assets/ThirdParty/UnderwaterDivingPack/PNG";
        private const int PixelsPerUnit = 16;
        private const int PlayerFrameSize = 80;

        [MenuItem("Marea Alta/Preparar paquete Underwater Diving")]
        public static void Configure()
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { PackRoot });

            foreach (string textureGuid in textureGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(textureGuid);
                ConfigureTexture(assetPath);
            }

            foreach (string textureGuid in textureGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(textureGuid);
                if (assetPath.Contains("/player/"))
                    SlicePlayerSheet(assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Marea Alta: paquete configurado. {textureGuids.Length} texturas procesadas.");
        }

        private static void ConfigureTexture(string assetPath)
        {
            if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
                return;

            bool isPlayerSheet = assetPath.Contains("/player/");

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = isPlayerSheet
                ? SpriteImportMode.Multiple
                : SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        private static void SlicePlayerSheet(string assetPath)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (texture == null || importer == null)
                return;

            int columns = texture.width / PlayerFrameSize;
            int rows = texture.height / PlayerFrameSize;

            if (columns <= 0 || rows <= 0 ||
                texture.width % PlayerFrameSize != 0 || texture.height % PlayerFrameSize != 0)
            {
                Debug.LogWarning($"Marea Alta: no se pudo recortar {assetPath} en cuadros de 80x80.");
                return;
            }

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            Dictionary<string, GUID> existingIds = provider.GetSpriteRects()
                .GroupBy(spriteRect => spriteRect.name)
                .ToDictionary(group => group.Key, group => group.First().spriteID);

            string sheetName = Path.GetFileNameWithoutExtension(assetPath).Replace('-', '_');
            var spriteRects = new List<SpriteRect>(columns * rows);

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int frameIndex = row * columns + column;
                    string frameName = $"{sheetName}_{frameIndex:00}";
                    GUID spriteId = existingIds.TryGetValue(frameName, out GUID existingId)
                        ? existingId
                        : GUID.Generate();

                    spriteRects.Add(new SpriteRect
                    {
                        name = frameName,
                        rect = new Rect(
                            column * PlayerFrameSize,
                            texture.height - ((row + 1) * PlayerFrameSize),
                            PlayerFrameSize,
                            PlayerFrameSize),
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                        border = Vector4.zero,
                        spriteID = spriteId
                    });
                }
            }

            provider.SetSpriteRects(spriteRects.ToArray());

            ISpriteNameFileIdDataProvider nameProvider =
                provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameProvider?.SetNameFileIdPairs(
                spriteRects.Select(spriteRect =>
                    new SpriteNameFileIdPair(spriteRect.name, spriteRect.spriteID)));

            provider.Apply();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
