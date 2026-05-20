using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AetherFrame
{
    // Shared health component for the player and drones. It keeps death behavior local so
    // combat scripts only need to call TakeDamage and do not care what kind of unit was hit.
    public sealed class Health : MonoBehaviour
    {
        [Header("Health")]
        [Min(1f)] public float maxHealth = 100f;
        public bool reloadSceneOnDeath;

        public float CurrentHealth { get; private set; }
        public float Normalized => maxHealth <= 0f ? 0f : CurrentHealth / maxHealth;
        public bool IsDead => CurrentHealth <= 0f;

        public event Action<Health> Died;
        public event Action<Health> Damaged;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            Damaged?.Invoke(this);

            if (IsDead)
            {
                Died?.Invoke(this);

                if (reloadSceneOnDeath)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
