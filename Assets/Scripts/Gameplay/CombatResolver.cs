using System;
using System.Collections.Generic;

namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 解析同一回合內的所有戰鬥效果。
    ///
    /// 規則：
    /// 1. 護盾
    /// 2. 攻擊
    /// 3. 恢復
    /// 4. 所有效果完成後才判定死亡
    ///
    /// 玩家與敵人的效果不分先後；
    /// 所有已觸發效果都必須完成解析。
    /// </summary>
    public static class CombatResolver
    {
        public static CombatResolutionResult ResolveRound(
            CombatantState player,
            CombatantState enemy,
            IReadOnlyList<PendingEffect> pendingEffects)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (enemy == null)
            {
                throw new ArgumentNullException(nameof(enemy));
            }

            if (pendingEffects == null)
            {
                throw new ArgumentNullException(
                    nameof(pendingEffects));
            }

            List<PendingEffect> sortedEffects =
                new(pendingEffects.Count);

            foreach (PendingEffect effect in pendingEffects)
            {
                if (effect != null)
                {
                    sortedEffects.Add(effect);
                }
            }

            sortedEffects.Sort(CompareEffects);

            List<CombatResolutionRecord> records =
                new(sortedEffects.Count);

            foreach (PendingEffect effect in sortedEffects)
            {
                CombatantState target =
                    GetTarget(
                        effect.TargetSide,
                        player,
                        enemy);

                CombatResolutionRecord record =
                    ApplyEffect(
                        effect,
                        target);

                records.Add(record);
            }

            /*
             * 規格要求：
             * 所有護盾、攻擊與恢復效果執行完畢後，
             * 才判斷本回合是否死亡。
             */
            bool playerDead = player.IsDead;
            bool enemyDead = enemy.IsDead;

            return new CombatResolutionResult(
                records,
                playerDead,
                enemyDead);
        }

        private static int CompareEffects(
            PendingEffect left,
            PendingEffect right)
        {
            int priorityComparison =
                GetEffectPriority(left.EffectType)
                    .CompareTo(
                        GetEffectPriority(
                            right.EffectType));

            if (priorityComparison != 0)
            {
                return priorityComparison;
            }

            /*
             * 同類型效果內維持穩定順序。
             * 陣營本身不作為優先依據。
             */
            int sourceOrderComparison =
                left.SourceOrder.CompareTo(
                    right.SourceOrder);

            if (sourceOrderComparison != 0)
            {
                return sourceOrderComparison;
            }

            return string.CompareOrdinal(
                left.SourceId,
                right.SourceId);
        }

        private static int GetEffectPriority(
            CombatEffectType effectType)
        {
            return effectType switch
            {
                CombatEffectType.Shield => 0,
                CombatEffectType.Attack => 1,
                CombatEffectType.Heal => 2,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(effectType),
                    effectType,
                    "Unsupported combat effect type.")
            };
        }

        private static CombatantState GetTarget(
            CombatSide targetSide,
            CombatantState player,
            CombatantState enemy)
        {
            return targetSide switch
            {
                CombatSide.Player => player,
                CombatSide.Enemy => enemy,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(targetSide),
                    targetSide,
                    "Unsupported combat side.")
            };
        }

        private static CombatResolutionRecord ApplyEffect(
            PendingEffect effect,
            CombatantState target)
        {
            int hpBefore = target.CurrentHp;
            int shieldBefore = target.Shield;

            switch (effect.EffectType)
            {
                case CombatEffectType.Shield:
                    target.AddShield(effect.Value);

                    return new CombatResolutionRecord(
                        effect,
                        hpBefore,
                        target.CurrentHp,
                        shieldBefore,
                        target.Shield,
                        appliedValue: effect.Value,
                        fullyBlocked: false);

                case CombatEffectType.Attack:
                    DamageApplicationResult damageResult =
                        target.ApplyDamage(effect.Value);

                    return new CombatResolutionRecord(
                        effect,
                        hpBefore,
                        target.CurrentHp,
                        shieldBefore,
                        target.Shield,
                        appliedValue: effect.Value,
                        fullyBlocked:
                            damageResult.WasFullyBlocked);

                case CombatEffectType.Heal:
                    int actualHpChange =
                        target.ApplyHeal(effect.Value);

                    return new CombatResolutionRecord(
                        effect,
                        hpBefore,
                        target.CurrentHp,
                        shieldBefore,
                        target.Shield,
                        appliedValue: actualHpChange,
                        fullyBlocked: false);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(effect.EffectType),
                        effect.EffectType,
                        "Unsupported combat effect type.");
            }
        }
    }
}