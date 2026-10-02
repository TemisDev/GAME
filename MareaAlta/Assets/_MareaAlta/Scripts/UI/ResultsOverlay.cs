using MareaAlta.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MareaAlta.UI
{
    [RequireComponent(typeof(GameSession))]
    public sealed class ResultsOverlay : MonoBehaviour
    {
        [SerializeField, Min(1)] private int totalDebrisInLevel = 6;

        private GameSession gameSession;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;

        private void Awake()
        {
            gameSession = GetComponent<GameSession>();
        }

        private void OnGUI()
        {
            if (gameSession.State != GameSession.SessionState.Victory)
                return;

            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.35f, 1f, 0.75f) }
            };

            bodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                normal = { textColor = Color.white }
            };
            buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                hover = { textColor = new Color(0.35f, 1f, 0.75f) }
            };

            float width = 500f;
            float height = 360f;
            Rect panel = new((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUI.Label(new Rect(panel.x, panel.y + 20f, width, 55f), "¡Zona restaurada!", titleStyle);
            GUI.Label(
                new Rect(panel.x, panel.y + 82f, width, 105f),
                $"Residuos recogidos: {gameSession.CollectedFragments}/{totalDebrisInLevel}\n" +
                $"Tiempo: {gameSession.CompletionTime:0.0} s",
                bodyStyle);

            if (GUI.Button(new Rect(panel.x + 90f, panel.y + 205f, 320f, 50f), "JUGAR DE NUEVO", buttonStyle))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if (GUI.Button(new Rect(panel.x + 90f, panel.y + 270f, 320f, 50f), "MENÚ PRINCIPAL", buttonStyle))
                SceneManager.LoadScene("MainMenu");
        }
    }
}
