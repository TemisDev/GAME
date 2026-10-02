using MareaAlta.Core;
using MareaAlta.Resources;
using UnityEngine;

namespace MareaAlta.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ExitTrigger : MonoBehaviour
    {
        [SerializeField] private GameSession gameSession;

        public void Configure(GameSession session)
        {
            gameSession = session;
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            EnergyInventory inventory = other.GetComponentInParent<EnergyInventory>();
            if (inventory == null)
                return;

            gameSession ??= FindFirstObjectByType<GameSession>();
            gameSession?.CompleteLevel(inventory.gameObject, inventory);
        }
    }
}
