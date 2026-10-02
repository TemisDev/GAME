using MareaAlta.Resources;
using MareaAlta.Core;
using UnityEngine;

namespace MareaAlta.Interaction
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class DebrisCollectible : MonoBehaviour
    {
        [SerializeField, Min(1)] private int fragmentValue = 1;

        private bool collected;
        private SpriteRenderer debrisRenderer;

        private void Awake()
        {
            debrisRenderer = GetComponent<SpriteRenderer>();
            if (debrisRenderer != null)
                debrisRenderer.drawMode = SpriteDrawMode.Simple;
        }

        private void Update()
        {
            if (!collected)
                transform.Rotate(0f, 0f, 55f * Time.deltaTime);
        }

        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected)
                return;

            EnergyInventory inventory = other.GetComponentInParent<EnergyInventory>();
            if (inventory == null)
                return;

            collected = true;
            inventory.AddFragments(fragmentValue);
            GameAudio.Instance?.PlayPickup();
            gameObject.SetActive(false);
        }

        private void OnValidate()
        {
            fragmentValue = Mathf.Max(1, fragmentValue);
            Collider2D collectibleCollider = GetComponent<Collider2D>();
            if (collectibleCollider != null)
                collectibleCollider.isTrigger = true;
        }
    }
}
