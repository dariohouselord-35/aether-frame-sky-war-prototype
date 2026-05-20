using UnityEngine;

namespace AetherFrame
{
    // Smooth chase camera with a small boost shake. The shake is intentionally procedural
    // and low-amplitude so speed feels stronger without making aiming uncomfortable.
    public sealed class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Follow")]
        public Transform target;
        public Vector3 localOffset = new Vector3(0f, 4f, -11f);
        public float positionSmoothTime = 0.11f;
        public float rotationSmooth = 12f;

        [Header("Boost Shake")]
        public PlayerFlightController playerFlight;
        public float boostShakeAmplitude = 0.18f;
        public float boostShakeFrequency = 18f;

        private Vector3 followVelocity;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.TransformPoint(localOffset);
            if (playerFlight != null && playerFlight.IsBoosting)
            {
                float shake = Mathf.Sin(Time.time * boostShakeFrequency) * boostShakeAmplitude;
                desiredPosition += transform.right * shake + transform.up * (shake * 0.4f);
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, positionSmoothTime);

            Quaternion desiredRotation = Quaternion.LookRotation(target.position + target.forward * 8f - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmooth * Time.deltaTime);
        }
    }
}
