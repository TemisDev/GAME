using UnityEngine;

namespace MareaAlta.World
{
    public sealed class ScenerySway : MonoBehaviour
    {
        [SerializeField] private float positionAmplitude = 0.08f;
        [SerializeField] private float rotationAmplitude = 2.5f;
        [SerializeField] private float frequency = 0.8f;
        [SerializeField] private float phase;

        private Vector3 startPosition;
        private Quaternion startRotation;

        public void Configure(float position, float rotation, float speed, float startPhase)
        {
            positionAmplitude = position;
            rotationAmplitude = rotation;
            frequency = speed;
            phase = startPhase;
        }

        private void Awake()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.time * frequency + phase);
            transform.position = startPosition + Vector3.up * (wave * positionAmplitude);
            transform.rotation = startRotation * Quaternion.Euler(0f, 0f, wave * rotationAmplitude);
        }
    }
}
