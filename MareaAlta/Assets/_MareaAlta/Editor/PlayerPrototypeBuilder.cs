using System.IO;
using System.Linq;
using MareaAlta.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class PlayerPrototypeBuilder
    {
        private const string ScenePath =
            "Assets/_MareaAlta/Scenes/Prototype_PuntaCoral.unity";
        private const string AnimationFolder =
            "Assets/_MareaAlta/Art/Animations/Player";
        private const string PrefabFolder =
            "Assets/_MareaAlta/Prefabs/Player";
        private const string PlayerSpriteFolder =
            "Assets/ThirdParty/UnderwaterDivingPack/PNG/player";

        [InitializeOnLoadMethod]
        private static void ScheduleFirstBuild()
        {
            string prefabPath = $"{PrefabFolder}/Player_Mara.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
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

        [MenuItem("Marea Alta/Crear prototipo del jugador")]
        public static void Build()
        {
            UnderwaterDivingPackSetup.Configure();

            Sprite[] idleSprites = LoadSprites("player-idle.png");
            Sprite[] swimSprites = LoadSprites("player-swiming.png");

            if (idleSprites.Length == 0 || swimSprites.Length == 0)
            {
                Debug.LogError("Marea Alta: no se encontraron los cuadros de Idle o Swim.");
                return;
            }

            EnsureFolder(AnimationFolder);
            EnsureFolder(PrefabFolder);

            AnimationClip idleClip = CreateOrUpdateClip(
                $"{AnimationFolder}/Mara_Idle.anim", idleSprites, 8f);
            AnimationClip swimClip = CreateOrUpdateClip(
                $"{AnimationFolder}/Mara_Swim.anim", swimSprites, 10f);
            AnimatorController controller = CreateAnimatorController(idleClip, swimClip);
            GameObject prefab = CreatePlayerPrefab(idleSprites[0], controller);

            AddPlayerToPrototypeScene(prefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Marea Alta: Player_Mara, animaciones y escena creados correctamente.");
        }

        private static Sprite[] LoadSprites(string fileName)
        {
            string path = $"{PlayerSpriteFolder}/{fileName}";
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderBy(sprite => sprite.name)
                .ToArray();
        }

        private static AnimationClip CreateOrUpdateClip(
            string assetPath,
            Sprite[] sprites,
            float frameRate)
        {
            var generatedClip = new AnimationClip
            {
                name = Path.GetFileNameWithoutExtension(assetPath),
                frameRate = frameRate
            };

            var binding = new EditorCurveBinding
            {
                path = string.Empty,
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = sprites
                .Select((sprite, index) => new ObjectReferenceKeyframe
                {
                    time = index / frameRate,
                    value = sprite
                })
                .ToArray();

            AnimationUtility.SetObjectReferenceCurve(generatedClip, binding, keyframes);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(generatedClip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(generatedClip, settings);

            AnimationClip existingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (existingClip == null)
            {
                AssetDatabase.CreateAsset(generatedClip, assetPath);
                return generatedClip;
            }

            EditorUtility.CopySerialized(generatedClip, existingClip);
            Object.DestroyImmediate(generatedClip);
            EditorUtility.SetDirty(existingClip);
            return existingClip;
        }

        private static AnimatorController CreateAnimatorController(
            AnimationClip idleClip,
            AnimationClip swimClip)
        {
            string controllerPath = $"{AnimationFolder}/Mara.controller";
            AnimatorController existingController =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

            if (existingController != null)
                return existingController;

            AnimatorController controller =
                AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState idleState = stateMachine.AddState("Idle");
            idleState.motion = idleClip;
            stateMachine.defaultState = idleState;

            AnimatorState swimState = stateMachine.AddState("Swim");
            swimState.motion = swimClip;

            AnimatorStateTransition toSwim = idleState.AddTransition(swimState);
            toSwim.hasExitTime = false;
            toSwim.duration = 0.08f;
            toSwim.AddCondition(AnimatorConditionMode.Greater, 0.05f, "Speed");

            AnimatorStateTransition toIdle = swimState.AddTransition(idleState);
            toIdle.hasExitTime = false;
            toIdle.duration = 0.08f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.05f, "Speed");

            return controller;
        }

        private static GameObject CreatePlayerPrefab(
            Sprite initialSprite,
            RuntimeAnimatorController controller)
        {
            string prefabPath = $"{PrefabFolder}/Player_Mara.prefab";
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
                return existingPrefab;

            var player = new GameObject("Player_Mara");

            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = initialSprite;
            renderer.sortingOrder = 10;

            Animator animator = player.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            CapsuleCollider2D collider = player.AddComponent<CapsuleCollider2D>();
            collider.direction = CapsuleDirection2D.Vertical;
            collider.size = new Vector2(1.1f, 2.4f);

            player.AddComponent<SwimController2D>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
            Object.DestroyImmediate(player);
            return prefab;
        }

        private static void AddPlayerToPrototypeScene(GameObject prefab)
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            GameObject existingPlayer = GameObject.Find("Player_Mara");
            if (existingPlayer == null)
            {
                GameObject player = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
                if (player != null)
                    player.transform.position = Vector3.zero;
            }

            AddBackground();
            AddWorldBounds();

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 7f;
                mainCamera.backgroundColor = new Color(0.03f, 0.15f, 0.28f);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void AddBackground()
        {
            if (GameObject.Find("Prototype_Background") != null)
                return;

            const string backgroundPath =
                "Assets/ThirdParty/UnderwaterDivingPack/PNG/environment/background.png";
            Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);

            if (backgroundSprite == null)
                return;

            var background = new GameObject("Prototype_Background");
            SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
            renderer.sprite = backgroundSprite;
            renderer.sortingOrder = -100;
            renderer.color = new Color(0.55f, 0.75f, 0.9f);
            background.transform.position = new Vector3(0f, 0f, 1f);
            background.transform.localScale = new Vector3(1.5f, 1.1f, 1f);
        }

        private static void AddWorldBounds()
        {
            if (GameObject.Find("WorldBounds") != null)
                return;

            var root = new GameObject("WorldBounds");
            CreateBoundary(root.transform, "Left", new Vector2(-13f, 0f), new Vector2(1f, 16f));
            CreateBoundary(root.transform, "Right", new Vector2(13f, 0f), new Vector2(1f, 16f));
            CreateBoundary(root.transform, "Top", new Vector2(0f, 7.5f), new Vector2(26f, 1f));
            CreateBoundary(root.transform, "Bottom", new Vector2(0f, -7.5f), new Vector2(26f, 1f));
        }

        private static void CreateBoundary(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size)
        {
            var boundary = new GameObject(name);
            boundary.transform.SetParent(parent);
            boundary.transform.position = position;
            BoxCollider2D collider = boundary.AddComponent<BoxCollider2D>();
            collider.size = size;
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
    }
}
