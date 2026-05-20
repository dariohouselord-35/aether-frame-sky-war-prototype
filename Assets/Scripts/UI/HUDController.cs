using UnityEngine;
using UnityEngine.UI;

namespace AetherFrame
{
    // Polls prototype state into a minimal screen-space HUD. This avoids coupling health,
    // flight, lock-on, and enemy systems directly to UI widgets.
    public sealed class HUDController : MonoBehaviour
    {
        [Header("References")]
        public Health playerHealth;
        public PlayerFlightController playerFlight;
        public LockOnSystem lockOnSystem;

        [Header("Bars")]
        public Image healthFill;
        public Image energyFill;

        [Header("Text")]
        public Text targetText;
        public Text enemyCountText;

        private void Update()
        {
            if (healthFill != null && playerHealth != null)
            {
                healthFill.fillAmount = playerHealth.Normalized;
            }

            if (energyFill != null && playerFlight != null)
            {
                energyFill.fillAmount = playerFlight.EnergyNormalized;
            }

            if (targetText != null && lockOnSystem != null)
            {
                targetText.text = "TARGET: " + lockOnSystem.GetTargetLabel();
            }

            if (enemyCountText != null)
            {
                int livingEnemies = 0;
                EnemyDroneAI[] enemies = FindObjectsByType<EnemyDroneAI>(FindObjectsSortMode.None);
                foreach (EnemyDroneAI enemy in enemies)
                {
                    if (enemy != null && enemy.Health != null && !enemy.Health.IsDead)
                    {
                        livingEnemies++;
                    }
                }

                enemyCountText.text = "DRONES: " + livingEnemies;
            }
        }
    }
}
