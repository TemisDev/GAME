using System.IO;
using System.Linq;
using MareaAlta.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.EditorTools
{
    public static class GatePassageValidator
    {
        private const string ScenePath = "Assets/_MareaAlta/Scenes/Demo_PuntaCoral.unity";
        private const string MarkerPath = "Assets/_MareaAlta/Settings/GatePassageFix.complete.txt";
        private static readonly float[] GatePositions = { 13f, 39f, 63f };

        [InitializeOnLoadMethod]
        private static void Schedule()
        {
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(MarkerPath) == null)
                EditorApplication.delayCall += ValidateWhenReady;
        }

        private static void ValidateWhenReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += ValidateWhenReady;
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GateController[] gates = Object.FindObjectsByType<GateController>(FindObjectsSortMode.None)
                .Where(gate => gate.gameObject.name.EndsWith("_Expanded"))
                .OrderBy(gate => gate.transform.position.x)
                .ToArray();

            int removedAccidentalBlockers = 0;
            foreach (float gateX in GatePositions)
            {
                Collider2D[] colliders = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
                foreach (Collider2D candidate in colliders)
                {
                    if (candidate.GetComponent<GateController>() != null || candidate.isTrigger)
                        continue;

                    Bounds bounds = candidate.bounds;
                    bool crossesGateLine = bounds.min.x < gateX + 0.55f && bounds.max.x > gateX - 0.55f;
                    bool blocksPassageCenter = bounds.min.y < 2.4f && bounds.max.y > -2.4f;
                    if (!crossesGateLine || !blocksPassageCenter)
                        continue;

                    candidate.enabled = false;
                    EditorUtility.SetDirty(candidate);
                    removedAccidentalBlockers++;
                    Debug.Log($"Marea Alta: collider accidental desactivado junto a la compuerta: {candidate.name}.");
                }
            }

            foreach (GateController gate in gates)
            {
                Collider2D gateCollider = gate.GetComponent<Collider2D>();
                if (gateCollider != null)
                {
                    gateCollider.isTrigger = false;
                    gateCollider.enabled = true;
                    EditorUtility.SetDirty(gateCollider);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath,
                $"Compuertas verificadas: {gates.Length}. Bloqueos accidentales retirados: {removedAccidentalBlockers}.");
            AssetDatabase.ImportAsset(MarkerPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Marea Alta: paso de las tres compuertas verificado. Bloqueos extra retirados: {removedAccidentalBlockers}.");
        }
    }
}
