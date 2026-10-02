using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MareaAlta.EditorTools
{
    public static class MenuUiPreviewOpener
    {
        private const string ScenePath = "Assets/_MareaAlta/Scenes/MainMenu.unity";
        private const string MarkerPath = "Assets/_MareaAlta/Settings/MenuUiPolish.complete.txt";

        [InitializeOnLoadMethod]
        private static void Schedule()
        {
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(MarkerPath) == null)
                EditorApplication.delayCall += OpenWhenReady;
        }

        [MenuItem("Marea Alta/Demo presentable/Abrir menú renovado")]
        public static void OpenWhenReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += OpenWhenReady;
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            File.WriteAllText(MarkerPath, "Menú profesional instalado y escena MainMenu abierta para revisión.");
            AssetDatabase.ImportAsset(MarkerPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Marea Alta: MainMenu abierto para revisar la interfaz renovada.");
        }
    }
}
