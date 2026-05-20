using UnityEngine;

namespace AetherFrame
{
    // Simple state-lite drone behavior: patrol around a home point, chase the player when
    // close, and fire slow readable shots when inside weapon range.
    public sealed class EnemyDroneAI : MonoBehaviour
    {
        [Header("References")]
        public Transform player;
        public Transform firePoint;
        public Projectile projectilePrefab;

        [Header("Movement")]
        public float patrolRadius = 30f;
        public float patrolSpeed = 9f;
        public float chaseSpeed = 15f;
        public float turnSpeed = 4f;
        public float chaseRange = 85f;
        public float preferredRange = 34f;

        [Header("Combat")]
        public float fireRange = 70f;
        public float fireCooldown = 1.8f;

        public Health Health { get; private set; }

        private Vector3 homePosition;
        private Vector3 patrolTarget;
        private float nextFireTime;

        private void Awake()
        {
            Health = GetComponent<Health>();
            homePosition = transform.position;
            PickNewPatrolTarget();
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= chaseRange)
            {
                ChasePlayer(distanceToPlayer);
            }
            else
            {
                Patrol();
            }
        }

        private void Patrol()
        {
            MoveToward(patrolTarget, patrolSpeed);

            if (Vector3.Distance(transform.position, patrolTarget) < 4f)
            {
                PickNewPatrolTarget();
            }
        }

        private void ChasePlayer(float distanceToPlayer)
        {
            Vector3 targetPosition = distanceToPlayer > preferredRange ? player.position : transform.position - transform.forward * 8f;
            MoveToward(targetPosition, chaseSpeed);

            if (distanceToPlayer <= fireRange && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireCooldown;
                FireAtPlayer();
            }
        }

        private void MoveToward(Vector3 targetPosition, float speed)
        {
            Vector3 toTarget = targetPosition - transform.position;
            if (toTarget.sqrMagnitude > 0.1f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, turnSpeed * Time.deltaTime);
            }

            transform.position += transform.forward * (speed * Time.deltaTime);
        }

        private void PickNewPatrolTarget()
        {
            Vector2 circle = Random.insideUnitCircle * patrolRadius;
            float height = Random.Range(-10f, 16f);
            patrolTarget = homePosition + new Vector3(circle.x, height, circle.y);
        }

        private void FireAtPlayer()
        {
            if (projectilePrefab == null || firePoint == null || player == null)
            {
                return;
            }

            Vector3 direction = (player.position - firePoint.position).normalized;
            Projectile shot = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction, Vector3.up));
            shot.gameObject.SetActive(true);
            shot.team = Team.Enemy;
            shot.owner = gameObject;
        }
    }
}
