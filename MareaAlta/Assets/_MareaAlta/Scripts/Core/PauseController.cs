using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MareaAlta.Core
{
    public sealed class PauseController : MonoBehaviour
    {
        [SerializeField] private string menuSceneName = "MainMenu";

        private bool isPaused;
        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                SetPaused(!isPaused);
        }

        private void OnGUI()
        {
            if (!isPaused)
                return;

            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.5f, 0.95f, 1f) }
            };
            buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 21,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            const float width = 430f;
            const float height = 330f;
            Rect panel = new((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUI.Label(new Rect(panel.x, panel.y + 25f, width, 55f), "PAUSA", titleStyle);

            if (GUI.Button(new Rect(panel.x + 85f, panel.y + 105f, 260f, 52f), "CONTINUAR", buttonStyle))
                SetPaused(false);
            if (GUI.Button(new Rect(panel.x + 85f, panel.y + 172f, 260f, 52f), "REINICIAR", buttonStyle))
                RestartScene();
            if (GUI.Button(new Rect(panel.x + 85f, panel.y + 239f, 260f, 52f), "MENÚ PRINCIPAL", buttonStyle))
                ReturnToMenu();
        }

        private void SetPaused(bool paused)
        {
            isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        private void RestartScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
        }
    }
}
