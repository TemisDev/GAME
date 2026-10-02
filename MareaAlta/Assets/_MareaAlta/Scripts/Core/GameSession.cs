using MareaAlta.Player;
using MareaAlta.Resources;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MareaAlta.Core
{
    public sealed class GameSession : MonoBehaviour
    {
        public enum SessionState
        {
            Playing,
            Victory
        }

        public SessionState State { get; private set; } = SessionState.Playing;
        public float CompletionTime { get; private set; }
        public int CollectedFragments { get; private set; }

        private float startTime;

        private void Awake()
        {
            startTime = Time.time;
        }

        private void Update()
        {
            if (State != SessionState.Victory || Keyboard.current == null)
                return;

            if (Keyboard.current.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void CompleteLevel(GameObject player, EnergyInventory inventory)
        {
            if (State == SessionState.Victory)
                return;

            State = SessionState.Victory;
            GameAudio.Instance?.PlayVictory();
            CompletionTime = Time.time - startTime;
            CollectedFragments = inventory.TotalFragmentsCollected;

            SwimController2D controller = player.GetComponent<SwimController2D>();
            if (controller != null)
                controller.enabled = false;

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null)
                body.linearVelocity = Vector2.zero;
        }
    }
}
