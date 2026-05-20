using UnityEngine;

namespace AetherFrame
{
    // Base straight-flying projectile. Missiles inherit this movement/damage behavior and
    // add steering before the forward movement step.
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile")]
        public Team team = Team.Neutral;
        public float speed = 70f;
        public float damage = 15f;
        public float lifetime = 3f;
        public GameObject owner;

        protected virtual void Update()
        {
            transform.position += transform.forward * (speed * Time.deltaTime);
            lifetime -= Time.deltaTime;

            if (lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            // Ignore the shooter and same-team units so close muzzle shots do not self-hit.
            if (owner != null && other.transform.root == owner.transform.root)
            {
                return;
            }

            TeamMember targetTeam = other.GetComponentInParent<TeamMember>();
            if (targetTeam != null && targetTeam.team == team)
            {
                return;
            }

            Health health = other.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            if (!other.isTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}
