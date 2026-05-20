using UnityEngine;

namespace AetherFrame
{
    // Lightweight arcade flight controller for a readable third-person combat prototype.
    // Movement is transform-driven so the first playable remains easy to tune in Inspector.
    public sealed class PlayerFlightController : MonoBehaviour
    {
        [Header("Movement")]
        public float cruiseSpeed = 22f;
        public float verticalSpeed = 16f;
        public float boostSpeed = 42f;
        public float acceleration = 28f;
        public float deceleration = 20f;

        [Header("Look")]
        public float mouseSensitivity = 3f;
        public float pitchLimit = 62f;
        public Transform visualRoot;

        [Header("Aether Energy")]
        public float maxEnergy = 100f;
        public float boostDrainPerSecond = 32f;
        public float energyRegenPerSecond = 24f;
        public float minimumBoostEnergy = 6f;

        public float Energy { get; private set; }
        public float EnergyNormalized => maxEnergy <= 0f ? 0f : Energy / maxEnergy;
        public bool IsBoosting { get; private set; }
        public Vector3 Velocity { get; private set; }

        private float yaw;
        private float pitch;

        private void Awake()
        {
            Energy = maxEnergy;
            yaw = transform.eulerAngles.y;
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            UpdateLook();
            UpdateMovement();
            UpdateVisualBanking();
        }

        private void UpdateLook()
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -pitchLimit, pitchLimit);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void UpdateMovement()
        {
            // Local-space input keeps WASD tied to the mecha/camera heading.
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);

            float vertical = 0f;
            if (Input.GetKey(KeyCode.Space))
            {
                vertical += 1f;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                vertical -= 1f;
            }

            bool wantsBoost = Input.GetKey(KeyCode.LeftShift) && input.sqrMagnitude > 0.01f;
            IsBoosting = wantsBoost && Energy > minimumBoostEnergy;

            float targetSpeed = IsBoosting ? boostSpeed : cruiseSpeed;
            Vector3 targetVelocity = (transform.right * input.x + transform.forward * input.z) * targetSpeed;
            targetVelocity += Vector3.up * (vertical * verticalSpeed);

            float lerpRate = targetVelocity.sqrMagnitude > Velocity.sqrMagnitude ? acceleration : deceleration;
            Velocity = Vector3.MoveTowards(Velocity, targetVelocity, lerpRate * Time.deltaTime);
            transform.position += Velocity * Time.deltaTime;

            if (IsBoosting)
            {
                Energy = Mathf.Max(0f, Energy - boostDrainPerSecond * Time.deltaTime);
            }
            else
            {
                Energy = Mathf.Min(maxEnergy, Energy + energyRegenPerSecond * Time.deltaTime);
            }
        }

        private void UpdateVisualBanking()
        {
            if (visualRoot == null)
            {
                return;
            }

            float sideInput = Input.GetAxisRaw("Horizontal");
            float targetRoll = -sideInput * 18f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetRoll);
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetRotation, Time.deltaTime * 6f);
        }
    }
}
