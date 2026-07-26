using System;
using UnityEngine;

namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 單一戰鬥單位的執行期數值狀態。
    ///
    /// 規則：
    /// - 護盾無上限。
    /// - 攻擊後生命可暫時低於 0。
    /// - 恢復在攻擊後套用。
    /// - 所有效果結算完成後才判定死亡。
    /// </summary>
    [Serializable]
    public sealed class CombatantState
    {
        [SerializeField]
        private CombatSide side;

        [SerializeField]
        private int maxHp;

        [SerializeField]
        private int currentHp;

        [SerializeField]
        private int shield;

        public CombatantState(
            CombatSide side,
            int maxHp,
            int currentHp,
            int shield)
        {
            if (maxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxHp),
                    "Maximum HP must be greater than zero.");
            }

            if (shield < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(shield),
                    "Shield cannot be negative.");
            }

            this.side = side;
            this.maxHp = maxHp;
            this.currentHp = Mathf.Min(currentHp, maxHp);
            this.shield = shield;
        }

        public CombatSide Side => side;

        public int MaxHp => maxHp;

        /// <summary>
        /// 戰鬥解析途中可能暫時小於 0。
        /// </summary>
        public int CurrentHp => currentHp;

        public int Shield => shield;

        /// <summary>
        /// 僅應在整回合所有效果解析完成後使用。
        /// </summary>
        public bool IsDead => currentHp <= 0;

        public void AddShield(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Shield amount cannot be negative.");
            }

            shield = checked(shield + amount);
        }

        /// <summary>
        /// 套用傷害。
        /// 傷害先消耗護盾，剩餘部分再扣除生命。
        /// </summary>
        public DamageApplicationResult ApplyDamage(int damage)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(damage),
                    "Damage cannot be negative.");
            }

            int shieldBefore = shield;
            int hpBefore = currentHp;

            int shieldDamage = Mathf.Min(shield, damage);
            shield -= shieldDamage;

            int remainingDamage = damage - shieldDamage;

            /*
             * 此處刻意不 Clamp 至 0。
             * 規格允許生命在攻擊階段暫時成為負值，
             * 後續恢復可能將其救回。
             */
            currentHp -= remainingDamage;

            return new DamageApplicationResult(
                requestedDamage: damage,
                absorbedByShield: shieldDamage,
                dealtToHp: remainingDamage,
                shieldBefore: shieldBefore,
                shieldAfter: shield,
                hpBefore: hpBefore,
                hpAfter: currentHp);
        }

        public int ApplyHeal(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Heal amount cannot be negative.");
            }

            int hpBefore = currentHp;

            currentHp = Mathf.Min(
                currentHp + amount,
                maxHp);

            return currentHp - hpBefore;
        }

        public void SetMaxHp(
            int newMaxHp,
            bool healByIncreaseAmount = false)
        {
            if (newMaxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(newMaxHp),
                    "Maximum HP must be greater than zero.");
            }

            int difference = newMaxHp - maxHp;
            maxHp = newMaxHp;

            if (healByIncreaseAmount && difference > 0)
            {
                currentHp = Mathf.Min(
                    currentHp + difference,
                    maxHp);
            }
            else
            {
                currentHp = Mathf.Min(
                    currentHp,
                    maxHp);
            }
        }

        public void Reset(
            int resetMaxHp,
            int resetCurrentHp,
            int resetShield)
        {
            if (resetMaxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resetMaxHp));
            }

            if (resetShield < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resetShield));
            }

            maxHp = resetMaxHp;
            currentHp = Mathf.Min(
                resetCurrentHp,
                resetMaxHp);

            shield = resetShield;
        }
    }

    /// <summary>
    /// 單次傷害套用結果。
    /// 後續 UI 特效可依此判斷是純護盾受擊，或生命受到傷害。
    /// </summary>
    public readonly struct DamageApplicationResult
    {
        public DamageApplicationResult(
            int requestedDamage,
            int absorbedByShield,
            int dealtToHp,
            int shieldBefore,
            int shieldAfter,
            int hpBefore,
            int hpAfter)
        {
            RequestedDamage = requestedDamage;
            AbsorbedByShield = absorbedByShield;
            DealtToHp = dealtToHp;
            ShieldBefore = shieldBefore;
            ShieldAfter = shieldAfter;
            HpBefore = hpBefore;
            HpAfter = hpAfter;
        }

        public int RequestedDamage { get; }

        public int AbsorbedByShield { get; }

        public int DealtToHp { get; }

        public int ShieldBefore { get; }

        public int ShieldAfter { get; }

        public int HpBefore { get; }

        public int HpAfter { get; }

        public bool WasFullyBlocked =>
            RequestedDamage > 0 &&
            DealtToHp == 0;

        public bool DamagedHp =>
            DealtToHp > 0;
    }
}