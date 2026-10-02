using MareaAlta.Core;
using UnityEngine;

namespace MareaAlta.World
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class GateController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer gateRenderer;
        [SerializeField] private Transform gateVisual;
        [SerializeField, Min(0.1f)] private float openingSpeed = 1.25f;

        public bool IsOpen { get; private set; }

        private Collider2D blockingCollider;
        private Collider2D[] allGateColliders;
        private BoxCollider2D boxCollider;
        private Vector2 closedColliderSize;
        private Vector2 closedColliderOffset;
        private Vector3 closedVisualScale;
        private Vector3 closedVisualPosition;
        private float openProgress;

        private void Awake()
        {
            blockingCollider = GetComponent<Collider2D>();
            allGateColliders = GetComponentsInChildren<Collider2D>(true);
            boxCollider = blockingCollider as BoxCollider2D;

            if (gateVisual == null)
            {
                Transform visual = transform.Find("GateVisual");
                gateVisual = visual != null ? visual : transform;
            }

            gateRenderer ??= gateVisual.GetComponent<SpriteRenderer>();
            if (gateRenderer != null)
                gateRenderer.drawMode = SpriteDrawMode.Simple;

            closedVisualScale = gateVisual.localScale;
            closedVisualPosition = gateVisual.localPosition;
            if (boxCollider != null)
            {
                closedColliderSize = boxCollider.size;
                closedColliderOffset = boxCollider.offset;
            }

            ApplyVisualState();
        }

        private void Update()
        {
            float target = IsOpen ? 1f : 0f;
            openProgress = Mathf.MoveTowards(openProgress, target, Time.deltaTime * openingSpeed);
            float eased = openProgress * openProgress * (3f - 2f * openProgress);
            float remainingHeight = Mathf.Lerp(1f, 0.04f, eased);

            gateVisual.localScale = new Vector3(
                closedVisualScale.x,
                closedVisualScale.y * remainingHeight,
                closedVisualScale.z);
            gateVisual.localPosition = closedVisualPosition + Vector3.up * (5.5f * eased);

            if (boxCollider != null)
            {
                boxCollider.size = new Vector2(closedColliderSize.x, closedColliderSize.y * remainingHeight);
                boxCollider.offset = closedColliderOffset + Vector2.up * (closedColliderSize.y * 0.5f * eased);
            }

            if (IsOpen)
                DisableAllGateColliders();
        }

        public void Open()
        {
            if (IsOpen)
                return;

            IsOpen = true;
            DisableAllGateColliders();
            ApplyVisualState();
            GameAudio.Instance?.PlayGate();
        }

        public void ConfigureVisual(Transform visual, SpriteRenderer renderer)
        {
            gateVisual = visual;
            gateRenderer = renderer;
        }

        private void ApplyVisualState()
        {
            if (gateRenderer != null)
            {
                gateRenderer.color = IsOpen
                    ? new Color(0.35f, 1f, 0.75f, 0.55f)
                    : Color.white;
            }
        }

        private void DisableAllGateColliders()
        {
            if (allGateColliders == null || allGateColliders.Length == 0)
                allGateColliders = GetComponentsInChildren<Collider2D>(true);

            foreach (Collider2D gateCollider in allGateColliders)
            {
                if (gateCollider == null)
                    continue;

                gateCollider.isTrigger = true;
                gateCollider.enabled = false;
            }
        }
    }
}
