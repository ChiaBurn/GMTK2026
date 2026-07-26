using System;

namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 同一回合中尚未解析的戰鬥效果。
    ///
    /// 所有玩家裝備與敵方行動先建立 PendingEffect，
    /// 再交由 CombatResolver 統一依類型解析。
    /// </summary>
    [Serializable]
    public sealed class PendingEffect
    {
        public PendingEffect(
            string sourceId,
            string sourceDisplayName,
            CombatSide sourceSide,
            CombatSide targetSide,
            CombatEffectType effectType,
            int value,
            int sourceOrder)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
            {
                throw new ArgumentException(
                    "Source ID cannot be empty.",
                    nameof(sourceId));
            }

            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Effect value cannot be negative.");
            }

            SourceId = sourceId;
            SourceDisplayName = sourceDisplayName;
            SourceSide = sourceSide;
            TargetSide = targetSide;
            EffectType = effectType;
            Value = value;
            SourceOrder = sourceOrder;
        }

        public string SourceId { get; }

        public string SourceDisplayName { get; }

        public CombatSide SourceSide { get; }

        public CombatSide TargetSide { get; }

        public CombatEffectType EffectType { get; }

        public int Value { get; }

        /// <summary>
        /// 同一效果類型內的穩定排序值。
        /// 玩家裝備依畫面自上而下；
        /// 敵方行動依畫面由左至右、由上至下。
        /// </summary>
        public int SourceOrder { get; }

        public override string ToString()
        {
            return
                $"{SourceDisplayName}: " +
                $"{EffectType} {Value}, " +
                $"{SourceSide} -> {TargetSide}";
        }
    }
}