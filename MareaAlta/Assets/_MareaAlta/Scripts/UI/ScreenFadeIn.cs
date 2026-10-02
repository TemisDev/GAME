using UnityEngine;

namespace MareaAlta.UI
{
    public sealed class ScreenFadeIn : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float duration = 0.75f;

        private float alpha = 1f;

        private void Update()
        {
            alpha = Mathf.MoveTowards(alpha, 0f, Time.unscaledDeltaTime / duration);
        }

        private void OnGUI()
        {
            if (alpha <= 0f)
                return;

            Color previousColor = GUI.color;
            GUI.color = new Color(0.005f, 0.025f, 0.06f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }
    }
}
