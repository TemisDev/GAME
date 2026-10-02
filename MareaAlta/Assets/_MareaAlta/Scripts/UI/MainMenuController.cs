using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MareaAlta.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = "Demo_PuntaCoral";

        private const float ReferenceWidth = 1280f;
        private const float ReferenceHeight = 720f;

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle sectionStyle;
        private GUIStyle buttonStyle;
        private GUIStyle buttonHintStyle;
        private GUIStyle footerStyle;
        private GUIStyle diverLabelStyle;

        private Texture2D whiteTexture;
        private Texture2D transparentTexture;

        private void Awake()
        {
            whiteTexture = CreateTexture(Color.white);
            transparentTexture = CreateTexture(Color.clear);
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                StartDemo();
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
                QuitGame();
        }

        private void OnGUI()
        {
            EnsureStyles();

            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            float scale = Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight);
            Vector2 offset = new(
                (Screen.width - ReferenceWidth * scale) * 0.5f,
                (Screen.height - ReferenceHeight * scale) * 0.5f);
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(scale, scale, 1f));

            DrawBackdropTreatment();
            DrawMainCard();
            DrawDiverIdentity();
            DrawFooter();

            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
        }

        private void DrawBackdropTreatment()
        {
            DrawRect(new Rect(0f, 0f, ReferenceWidth, ReferenceHeight), new Color(0.005f, 0.025f, 0.07f, 0.16f));
            DrawRect(new Rect(0f, 0f, 44f, ReferenceHeight), new Color(0.005f, 0.035f, 0.08f, 0.72f));
            DrawRect(new Rect(ReferenceWidth - 44f, 0f, 44f, ReferenceHeight), new Color(0.005f, 0.035f, 0.08f, 0.72f));

            DrawRect(new Rect(0f, 0f, ReferenceWidth, 5f), new Color(0.18f, 0.9f, 0.92f, 0.5f));
            DrawRect(new Rect(0f, ReferenceHeight - 5f, ReferenceWidth, 5f), new Color(0.04f, 0.3f, 0.42f, 0.9f));
        }

        private void DrawMainCard()
        {
            Rect shadow = new(75f, 74f, 720f, 548f);
            Rect card = new(68f, 66f, 720f, 548f);
            DrawRect(shadow, new Color(0f, 0f, 0f, 0.38f));
            DrawRect(card, new Color(0.008f, 0.065f, 0.13f, 0.91f));
            DrawBorder(card, new Color(0.2f, 0.78f, 0.82f, 0.42f), 2f);
            DrawCornerBrackets(card, new Color(0.35f, 1f, 0.9f, 0.85f));

            DrawRect(new Rect(card.x + 32f, card.y + 30f, 218f, 30f), new Color(0.08f, 0.42f, 0.5f, 0.62f));
            GUI.Label(new Rect(card.x + 42f, card.y + 31f, 200f, 28f), "EXPEDICIÓN 01  •  PUNTA CORAL", sectionStyle);

            Rect titleRect = new(card.x + 38f, card.y + 72f, card.width - 76f, 86f);
            GUI.color = new Color(0f, 0.08f, 0.12f, 0.85f);
            GUI.Label(new Rect(titleRect.x + 4f, titleRect.y + 5f, titleRect.width, titleRect.height), "MAREA ALTA", titleStyle);
            GUI.color = Color.white;
            GUI.Label(titleRect, "MAREA ALTA", titleStyle);

            DrawRect(new Rect(card.x + 42f, card.y + 163f, 96f, 4f), new Color(0.25f, 1f, 0.86f, 0.95f));
            DrawRect(new Rect(card.x + 143f, card.y + 164f, 360f, 2f), new Color(0.15f, 0.65f, 0.72f, 0.38f));

            GUI.Label(
                new Rect(card.x + 40f, card.y + 183f, card.width - 82f, 66f),
                "Recupera los fragmentos perdidos y devuelve\nla energía al arrecife de Punta Coral.",
                subtitleStyle);

            Rect startButton = new(card.x + 40f, card.y + 290f, 490f, 72f);
            if (DrawButton(startButton, "INICIAR EXPLORACIÓN", "ENTER  /  ESPACIO", true))
                StartDemo();

            Rect exitButton = new(card.x + 40f, card.y + 378f, 490f, 64f);
            if (DrawButton(exitButton, "SALIR", "ESC", false))
                QuitGame();

            DrawRect(new Rect(card.x + 40f, card.y + 478f, card.width - 80f, 1f), new Color(0.25f, 0.75f, 0.8f, 0.32f));
            GUI.Label(
                new Rect(card.x + 40f, card.y + 492f, card.width - 80f, 32f),
                "DEMO JUGABLE  •  TRES SECTORES  •  OBJETIVO: RESTAURAR EL ARRECIFE",
                footerStyle);
        }

        private bool DrawButton(Rect rect, string label, string hint, bool primary)
        {
            bool hover = rect.Contains(Event.current.mousePosition);
            Color shadow = new(0f, 0f, 0f, 0.36f);
            Color border = primary
                ? new Color(0.25f, 1f, 0.86f, hover ? 1f : 0.72f)
                : new Color(0.35f, 0.72f, 0.8f, hover ? 0.9f : 0.5f);
            Color fill = primary
                ? new Color(0.04f, hover ? 0.34f : 0.25f, hover ? 0.38f : 0.32f, 0.96f)
                : new Color(0.025f, hover ? 0.22f : 0.15f, hover ? 0.3f : 0.23f, 0.94f);

            DrawRect(new Rect(rect.x + 5f, rect.y + 6f, rect.width, rect.height), shadow);
            DrawRect(rect, border);
            DrawRect(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f), fill);
            DrawRect(new Rect(rect.x + 2f, rect.y + 2f, hover ? 9f : 5f, rect.height - 4f),
                primary ? new Color(0.35f, 1f, 0.82f, 0.95f) : new Color(0.25f, 0.7f, 0.78f, 0.75f));

            GUI.Label(new Rect(rect.x + 28f, rect.y + 7f, rect.width - 150f, rect.height - 14f), label, buttonStyle);
            GUI.Label(new Rect(rect.x + rect.width - 142f, rect.y + 8f, 122f, rect.height - 16f), hint, buttonHintStyle);
            return GUI.Button(rect, GUIContent.none, buttonStyle);
        }

        private void DrawDiverIdentity()
        {
            Rect card = new(914f, 526f, 294f, 88f);
            DrawRect(new Rect(card.x + 4f, card.y + 5f, card.width, card.height), new Color(0f, 0f, 0f, 0.3f));
            DrawRect(card, new Color(0.01f, 0.08f, 0.15f, 0.82f));
            DrawBorder(card, new Color(0.22f, 0.75f, 0.8f, 0.48f), 1f);
            DrawRect(new Rect(card.x + 16f, card.y + 18f, 5f, 50f), new Color(1f, 0.52f, 0.16f, 0.95f));
            GUI.Label(new Rect(card.x + 34f, card.y + 12f, card.width - 48f, 30f), "MARA  //  BUZO 01", diverLabelStyle);
            GUI.Label(new Rect(card.x + 34f, card.y + 43f, card.width - 48f, 25f), "ESTADO: LISTA PARA DESCENDER", footerStyle);
        }

        private void DrawFooter()
        {
            Rect strip = new(68f, 643f, 1140f, 44f);
            DrawRect(strip, new Color(0.008f, 0.055f, 0.11f, 0.88f));
            DrawBorder(strip, new Color(0.12f, 0.5f, 0.58f, 0.45f), 1f);
            GUI.Label(
                new Rect(strip.x + 20f, strip.y + 7f, strip.width - 40f, 30f),
                "WASD / FLECHAS   MOVERSE        E   INTERACTUAR        ESC   PAUSA",
                footerStyle);
        }

        private void EnsureStyles()
        {
            titleStyle ??= CreateLabelStyle(64, FontStyle.Bold, new Color(0.58f, 0.98f, 1f), TextAnchor.MiddleLeft);
            subtitleStyle ??= CreateLabelStyle(22, FontStyle.Normal, new Color(0.88f, 0.96f, 1f), TextAnchor.UpperLeft);
            sectionStyle ??= CreateLabelStyle(13, FontStyle.Bold, new Color(0.75f, 1f, 0.92f), TextAnchor.MiddleLeft);
            footerStyle ??= CreateLabelStyle(13, FontStyle.Normal, new Color(0.62f, 0.82f, 0.88f), TextAnchor.MiddleLeft);
            diverLabelStyle ??= CreateLabelStyle(18, FontStyle.Bold, new Color(0.82f, 1f, 0.94f), TextAnchor.MiddleLeft);

            buttonStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 23,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white, background = transparentTexture },
                hover = { textColor = Color.white, background = transparentTexture },
                active = { textColor = Color.white, background = transparentTexture },
                focused = { textColor = Color.white, background = transparentTexture }
            };
            buttonHintStyle ??= CreateLabelStyle(12, FontStyle.Bold, new Color(0.55f, 0.9f, 0.9f), TextAnchor.MiddleRight);
        }

        private void StartDemo()
        {
            MareaAlta.Core.GameAudio.Instance?.PlayUi();
            SceneManager.LoadScene(gameplaySceneName);
        }

        private static void QuitGame()
        {
            MareaAlta.Core.GameAudio.Instance?.PlayUi();
            Application.Quit();
        }

        private void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previous;
        }

        private void DrawBorder(Rect rect, Color color, float thickness)
        {
            DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private void DrawCornerBrackets(Rect rect, Color color)
        {
            const float length = 24f;
            const float thickness = 3f;
            DrawRect(new Rect(rect.x, rect.y, length, thickness), color);
            DrawRect(new Rect(rect.x, rect.y, thickness, length), color);
            DrawRect(new Rect(rect.xMax - length, rect.y, length, thickness), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, length), color);
            DrawRect(new Rect(rect.x, rect.yMax - thickness, length, thickness), color);
            DrawRect(new Rect(rect.x, rect.yMax - length, thickness, length), color);
            DrawRect(new Rect(rect.xMax - length, rect.yMax - thickness, length, thickness), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.yMax - length, thickness, length), color);
        }

        private static GUIStyle CreateLabelStyle(int size, FontStyle fontStyle, Color color, TextAnchor alignment)
        {
            return new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontSize = size,
                fontStyle = fontStyle,
                wordWrap = true,
                normal = { textColor = color }
            };
        }

        private static Texture2D CreateTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "MenuUiRuntimeTexture",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void OnDestroy()
        {
            if (whiteTexture != null)
                Destroy(whiteTexture);
            if (transparentTexture != null)
                Destroy(transparentTexture);
        }
    }
}
