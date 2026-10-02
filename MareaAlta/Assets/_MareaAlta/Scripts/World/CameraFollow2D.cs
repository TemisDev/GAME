using UnityEngine;

namespace MareaAlta.World
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 minimum = new(-1f, 0f);
        [SerializeField] private Vector2 maximum = new(53f, 0f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.22f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 desired = new(
                Mathf.Clamp(target.position.x, minimum.x, maximum.x),
                Mathf.Clamp(target.position.y, minimum.y, maximum.y),
                transform.position.z);

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

        public void Configure(Transform followTarget, Vector2 min, Vector2 max)
        {
            target = followTarget;
            minimum = min;
            maximum = max;
        }
    }
}
