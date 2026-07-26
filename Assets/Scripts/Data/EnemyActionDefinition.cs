using CountdownAutoBattle.Gameplay;
using UnityEngine;

namespace CountdownAutoBattle.Data
{
    /// <summary>
    /// 敵方單一行動的靜態資料。
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnemyActionDefinition",
        menuName = "Countdown Auto Battle/Enemy Action Definition")]
    public sealed class EnemyActionDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string actionId;

        [SerializeField]
        private string displayName;

        [Header("Gameplay")]
        [SerializeField]
        private CombatEffectType effectType;

        [SerializeField, Min(0)]
        private int power;

        [SerializeField, Min(1)]
        private int cycle = 1;

        [Header("UI")]
        [SerializeField, TextArea]
        private string description;

        public string ActionId => actionId;

        public string DisplayName => displayName;

        public CombatEffectType EffectType => effectType;

        public int Power => power;

        public int Cycle => cycle;

        public string Description => description;

#if UNITY_EDITOR
        private void OnValidate()
        {
            power = Mathf.Max(0, power);
            cycle = Mathf.Max(1, cycle);
        }
#endif
    }
}