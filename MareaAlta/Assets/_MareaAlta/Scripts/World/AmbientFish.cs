using UnityEngine;

namespace MareaAlta.World
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class AmbientFish : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.1f)] private float frameRate = 7f;
        [SerializeField, Min(0.1f)] private float travelDistance = 7f;
        [SerializeField, Min(0.1f)] private float speed = 0.8f;
        [SerializeField] private float phase;

        private SpriteRenderer fishRenderer;
        private Vector3 origin;

        private void Awake()
        {
            fishRenderer = GetComponent<SpriteRenderer>();
            origin = transform.position;
        }

        private void Update()
        {
            float cycle = Mathf.Repeat(Time.time * speed + phase, 2f);
            float direction = cycle < 1f ? 1f : -1f;
            float normalized = cycle < 1f ? cycle : 2f - cycle;
            transform.position = origin + Vector3.right * ((normalized - 0.5f) * travelDistance);
            fishRenderer.flipX = direction < 0f;

            if (frames != null && frames.Length > 0)
            {
                int frame = Mathf.FloorToInt((Time.time + phase) * frameRate) % frames.Length;
                fishRenderer.sprite = frames[frame];
            }
        }

        public void Configure(Sprite[] animationFrames, float distance, float movementSpeed, float startPhase)
        {
            frames = animationFrames;
            travelDistance = Mathf.Max(0.1f, distance);
            speed = Mathf.Max(0.1f, movementSpeed);
            phase = startPhase;
        }
    }
}
