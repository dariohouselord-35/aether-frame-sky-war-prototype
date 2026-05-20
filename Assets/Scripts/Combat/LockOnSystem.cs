using System.Collections.Generic;
using UnityEngine;

namespace AetherFrame
{
    // Target cycling stays deliberately simple for the prototype: Tab selects the next
    // living drone inside range, and missiles read CurrentTarget when fired.
    public sealed class LockOnSystem : MonoBehaviour
    {
        [Header("Lock-On")]
        public float maxLockDistance = 260f;
        public KeyCode cycleKey = KeyCode.Tab;

        public Transform CurrentTarget { get; private set; }

        private readonly List<EnemyDroneAI> candidates = new List<EnemyDroneAI>();
        private int currentIndex = -1;

        private void Update()
        {
            PruneInvalidTarget();

            if (Input.GetKeyDown(cycleKey))
            {
                CycleTarget();
            }
        }

        public string GetTargetLabel()
        {
            return CurrentTarget == null ? "NO LOCK" : CurrentTarget.name.ToUpperInvariant();
        }

        private void CycleTarget()
        {
            candidates.Clear();
            EnemyDroneAI[] enemies = FindObjectsByType<EnemyDroneAI>(FindObjectsSortMode.None);

            foreach (EnemyDroneAI enemy in enemies)
            {
                if (enemy == null || enemy.Health == null || enemy.Health.IsDead)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= maxLockDistance)
                {
                    candidates.Add(enemy);
                }
            }

            if (candidates.Count == 0)
            {
                CurrentTarget = null;
                currentIndex = -1;
                return;
            }

            currentIndex = (currentIndex + 1) % candidates.Count;
            CurrentTarget = candidates[currentIndex].transform;
        }

        private void PruneInvalidTarget()
        {
            if (CurrentTarget == null)
            {
                return;
            }

            Health health = CurrentTarget.GetComponent<Health>();
            bool tooFar = Vector3.Distance(transform.position, CurrentTarget.position) > maxLockDistance;
            if (health == null || health.IsDead || tooFar)
            {
                CurrentTarget = null;
                currentIndex = -1;
            }
        }
    }
}
