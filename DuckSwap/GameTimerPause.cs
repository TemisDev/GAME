using UnityEngine;
using UnityEngine.UI;

public class GameTimerPause : MonoBehaviour
{
    public Text timerText;
    public Button pauseButton;
    public Text pauseButtonText;

    private float elapsedTime;
    private bool isPaused;

    private void Awake()
    {
        Time.timeScale = 1f;
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);
        UpdateDisplay();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        if (!isPaused)
        {
            elapsedTime += Time.unscaledDeltaTime;
            UpdateDisplay();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pauseButtonText != null)
            pauseButtonText.text = isPaused ? "REANUDAR" : "PAUSAR";
    }

    private void UpdateDisplay()
    {
        if (timerText == null) return;
        int totalSeconds = Mathf.FloorToInt(elapsedTime);
        timerText.text = string.Format("TIEMPO  {0:00}:{1:00}", totalSeconds / 60, totalSeconds % 60);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
