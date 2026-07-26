using System.Collections.Generic;

namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 一個完整回合解析完畢後的結果。
    /// </summary>
    public sealed class CombatResolutionResult
    {
        public CombatResolutionResult(
            IReadOnlyList<CombatResolutionRecord> records,
            bool playerDead,
            bool enemyDead)
        {
            Records = records;
            PlayerDead = playerDead;
            EnemyDead = enemyDead;
        }

        public IReadOnlyList<CombatResolutionRecord> Records
        {
            get;
        }

        public bool PlayerDead { get; }

        public bool EnemyDead { get; }

        public bool PlayerWon =>
            EnemyDead && !PlayerDead;

        public bool PlayerLost =>
            PlayerDead || (PlayerDead && EnemyDead);

        public bool BattleContinues =>
            !PlayerDead && !EnemyDead;
    }
}