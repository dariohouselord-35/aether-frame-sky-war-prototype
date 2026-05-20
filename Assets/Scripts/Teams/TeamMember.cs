using UnityEngine;

namespace AetherFrame
{
    public sealed class TeamMember : MonoBehaviour
    {
        [Tooltip("Used by projectiles so allied units do not damage each other.")]
        public Team team = Team.Neutral;
    }
}
