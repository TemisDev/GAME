using UnityEngine;

namespace MareaAlta.World
{
    public sealed class AmbientBubble : MonoBehaviour
    {
        [SerializeField] private float riseSpeed = 0.45f;
        [SerializeField] private float horizontalDrift = 0.2f;

        private float phase;

        public void Configure(float speed, float drift, float startPhase)
        {
            riseSpeed = speed;
            horizontalDrift = drift;
            phase = startPhase;
        }

        private void Update()
        {
            Vector3 position = transform.position;
            position.y += riseSpeed * Time.deltaTime;
            position.x += Mathf.Sin(Time.time * 1.2f + phase) * horizontalDrift * Time.deltaTime;

            if (position.y > 7.5f)
                position.y = -7.5f;

            transform.position = position;
        }
    }
}
