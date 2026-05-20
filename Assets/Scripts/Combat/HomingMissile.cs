using UnityEngine;

namespace AetherFrame
{
    // Turns toward the current locked target while preserving forward missile momentum.
    // If no target is assigned, it behaves like a fast unguided projectile.
    public sealed class HomingMissile : Projectile
    {
        [Header("Homing")]
        public Transform target;
        public float turnSpeed = 120f;
        public float acceleration = 10f;
        public float maxSpeed = 58f;

        protected override void Update()
        {
            if (target != null)
            {
                Vector3 toTarget = (target.position - transform.position).normalized;
                Quaternion desiredRotation = Quaternion.LookRotation(toTarget, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, turnSpeed * Time.deltaTime);
            }

            speed = Mathf.MoveTowards(speed, maxSpeed, acceleration * Time.deltaTime);
            base.Update();
        }
    }
}
