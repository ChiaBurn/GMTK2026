namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 單一效果的解析紀錄。
    /// 後續可用於戰鬥動畫、戰鬥紀錄或除錯。
    /// </summary>
    public sealed class CombatResolutionRecord
    {
        public CombatResolutionRecord(
            PendingEffect effect,
            int hpBefore,
            int hpAfter,
            int shieldBefore,
            int shieldAfter,
            int appliedValue,
            bool fullyBlocked)
        {
            Effect = effect;
            HpBefore = hpBefore;
            HpAfter = hpAfter;
            ShieldBefore = shieldBefore;
            ShieldAfter = shieldAfter;
            AppliedValue = appliedValue;
            FullyBlocked = fullyBlocked;
        }

        public PendingEffect Effect { get; }

        public int HpBefore { get; }

        public int HpAfter { get; }

        public int ShieldBefore { get; }

        public int ShieldAfter { get; }

        /// <summary>
        /// 實際套用值。
        ///
        /// 攻擊時代表原始傷害值；
        /// 護盾時代表增加量；
        /// 恢復時代表實際生命變化量。
        /// </summary>
        public int AppliedValue { get; }

        public bool FullyBlocked { get; }
    }
}