using MareaAlta.Interaction;
using MareaAlta.Resources;
using MareaAlta.Core;
using UnityEngine;

namespace MareaAlta.World
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TurbineController : MonoBehaviour, IInteractable
    {
        [SerializeField, Min(1)] private int energyCost = 1;
        [SerializeField] private GateController linkedGate;
        [SerializeField] private SpriteRenderer turbineRenderer;
        [SerializeField] private SpriteRenderer statusLight;
        [SerializeField] private Transform rotorVisual;

        private EnergyInventory nearbyInventory;

        public bool IsActivated { get; private set; }

        public string InteractionPrompt
        {
            get
            {
                if (IsActivated)
                    return "Turbina activada";
                if (nearbyInventory != null && nearbyInventory.EnergyCount < energyCost)
                    return $"Falta energía ({nearbyInventory.EnergyCount}/{energyCost})";
                return $"E - Activar turbina ({energyCost} energía)";
            }
        }

        private void Awake()
        {
            turbineRenderer ??= GetComponent<SpriteRenderer>();
            if (turbineRenderer != null)
                turbineRenderer.drawMode = SpriteDrawMode.Simple;

            if (statusLight == null)
            {
                Transform lightTransform = transform.Find("StatusLight");
                if (lightTransform != null)
                    statusLight = lightTransform.GetComponent<SpriteRenderer>();
            }

            if (rotorVisual == null)
                rotorVisual = transform.Find("RotorVisual");

            ApplyVisualState();
        }

        public void Interact(GameObject interactor)
        {
            if (IsActivated)
                return;

            EnergyInventory inventory = interactor.GetComponentInParent<EnergyInventory>();
            if (inventory == null)
                return;

            if (!inventory.TrySpendEnergy(energyCost))
            {
                GameAudio.Instance?.PlayError();
                return;
            }

            IsActivated = true;
            GameAudio.Instance?.PlayTurbine();
            linkedGate?.Open();
            ApplyVisualState();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            EnergyInventory inventory = other.GetComponentInParent<EnergyInventory>();
            if (inventory != null)
                nearbyInventory = inventory;
        }

        private void Update()
        {
            if (rotorVisual != null)
            {
                float rotationSpeed = IsActivated ? 110f : 18f;
                rotorVisual.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            EnergyInventory inventory = other.GetComponentInParent<EnergyInventory>();
            if (inventory == nearbyInventory)
                nearbyInventory = null;
        }

        public void Configure(GateController gate, int cost)
        {
            linkedGate = gate;
            energyCost = Mathf.Max(1, cost);
        }

        private void ApplyVisualState()
        {
            if (turbineRenderer != null)
                turbineRenderer.color = IsActivated ? new Color(0.72f, 1f, 0.88f) : Color.white;

            if (statusLight != null)
                statusLight.color = IsActivated
                    ? new Color(0.2f, 1f, 0.55f, 0.95f)
                    : new Color(1f, 0.35f, 0.16f, 0.9f);
        }

        private void OnValidate()
        {
            energyCost = Mathf.Max(1, energyCost);
        }
    }
}
