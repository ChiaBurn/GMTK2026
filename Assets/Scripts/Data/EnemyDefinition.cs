using System.Collections.Generic;
using UnityEngine;

namespace CountdownAutoBattle.Data
{
    /// <summary>
    /// 敵方單位的靜態資料。
    ///
    /// 一個關卡只會有一個敵人，
    /// 但一個敵人可以持有多個行動。
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnemyDefinition",
        menuName = "Countdown Auto Battle/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string enemyId;

        [SerializeField]
        private string displayName;

        [Header("Gameplay")]
        [SerializeField, Min(1)]
        private int maxHp = 30;

        [SerializeField, Min(0)]
        private int initialShield;

        [SerializeField]
        private List<EnemyActionDefinition> actions = new();

        public string EnemyId => enemyId;

        public string DisplayName => displayName;

        public int MaxHp => maxHp;

        public int InitialShield => initialShield;

        public IReadOnlyList<EnemyActionDefinition> Actions =>
            actions;

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxHp = Mathf.Max(1, maxHp);
            initialShield = Mathf.Max(0, initialShield);
        }
#endif
    }
}