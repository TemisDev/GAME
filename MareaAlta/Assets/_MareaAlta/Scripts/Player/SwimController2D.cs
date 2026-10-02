using UnityEngine;
using UnityEngine.InputSystem;

namespace MareaAlta.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class SwimController2D : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField, Min(0.1f)] private float maximumSpeed = 4.5f;
        [SerializeField, Min(0.1f)] private float acceleration = 18f;
        [SerializeField, Min(0.1f)] private float deceleration = 22f;

        [Header("Presentacion opcional")]
        [SerializeField] private SpriteRenderer characterRenderer;
        [SerializeField] private Animator characterAnimator;

        private static readonly int SpeedParameter = Animator.StringToHash("Speed");

        private Rigidbody2D body;
        private Vector2 movementInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            characterRenderer ??= GetComponentInChildren<SpriteRenderer>();
            characterAnimator ??= GetComponentInChildren<Animator>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        private void Update()
        {
            movementInput = ReadMovement();

            if (characterRenderer != null && Mathf.Abs(movementInput.x) > 0.01f)
            {
                characterRenderer.flipX = movementInput.x < 0f;
            }

            if (characterAnimator != null)
            {
                characterAnimator.SetFloat(SpeedParameter, body.linearVelocity.magnitude);
            }
        }

        private void FixedUpdate()
        {
            Vector2 targetVelocity = movementInput * maximumSpeed;
            float changeRate = movementInput.sqrMagnitude > 0f ? acceleration : deceleration;

            body.linearVelocity = Vector2.MoveTowards(
                body.linearVelocity,
                targetVelocity,
                changeRate * Time.fixedDeltaTime);
        }

        private static Vector2 ReadMovement()
        {
            Vector2 input = Vector2.zero;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    input.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    input.x += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                    input.y -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                    input.y += 1f;
            }

            if (Gamepad.current != null)
            {
                Vector2 stickInput = Gamepad.current.leftStick.ReadValue();
                if (stickInput.sqrMagnitude > input.sqrMagnitude)
                    input = stickInput;
            }

            return Vector2.ClampMagnitude(input, 1f);
        }

        private void OnDisable()
        {
            movementInput = Vector2.zero;

            if (body != null)
                body.linearVelocity = Vector2.zero;
        }

        private void OnValidate()
        {
            maximumSpeed = Mathf.Max(0.1f, maximumSpeed);
            acceleration = Mathf.Max(0.1f, acceleration);
            deceleration = Mathf.Max(0.1f, deceleration);
        }
    }
}
