using UnityEngine;
using UnityEngine.InputSystem;

namespace MareaAlta.UI
{
    public sealed class InstructionsOverlay : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float initialDisplaySeconds = 9f;

        private bool manuallyVisible;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
                manuallyVisible = !manuallyVisible;
        }

        private void OnGUI()
        {
            if (!manuallyVisible && Time.timeSinceLevelLoad > initialDisplaySeconds)
                return;

            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.35f, 0.9f, 1f) }
            };

            bodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                normal = { textColor = Color.white }
            };

            const float width = 540f;
            const float height = 138f;
            Rect panel = new((Screen.width - width) * 0.5f, 18f, width, height);
            GUI.Box(panel, GUIContent.none);
            GUI.Label(new Rect(panel.x, panel.y + 8f, width, 38f), "MAREA ALTA — PUNTA CORAL", titleStyle);
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 45f, width - 24f, 82f),
                "WASD o flechas: nadar   •   Recoge 3 residuos: 1 energía\n" +
                "Acércate a la turbina y pulsa E   •   Cruza hasta la salida verde\n" +
                "Pulsa H para mostrar u ocultar esta ayuda",
                bodyStyle);
        }
    }
}
