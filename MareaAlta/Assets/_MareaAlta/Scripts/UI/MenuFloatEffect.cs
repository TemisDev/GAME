using UnityEngine;

namespace MareaAlta.UI
{
    public sealed class MenuFloatEffect : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.3f;
        [SerializeField] private float frequency = 1.1f;

        private Vector3 startPosition;

        private void Awake()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * frequency) * amplitude);
        }
    }
}
