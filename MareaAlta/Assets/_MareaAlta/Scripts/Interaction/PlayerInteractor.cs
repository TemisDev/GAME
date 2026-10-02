using UnityEngine;
using UnityEngine.InputSystem;

namespace MareaAlta.Interaction
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        private IInteractable currentTarget;

        public string InteractionPrompt => currentTarget?.InteractionPrompt ?? string.Empty;

        private void Update()
        {
            if (currentTarget == null || Keyboard.current == null)
                return;

            if (Keyboard.current.eKey.wasPressedThisFrame)
                currentTarget.Interact(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            IInteractable candidate = other.GetComponentInParent<IInteractable>();
            if (candidate != null)
                currentTarget = candidate;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            IInteractable candidate = other.GetComponentInParent<IInteractable>();
            if (ReferenceEquals(candidate, currentTarget))
                currentTarget = null;
        }

        private void OnDisable()
        {
            currentTarget = null;
        }
    }
}
