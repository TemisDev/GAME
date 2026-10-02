using UnityEngine;

namespace MareaAlta.Interaction
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        void Interact(GameObject interactor);
    }
}
