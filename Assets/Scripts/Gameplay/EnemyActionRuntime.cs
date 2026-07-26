using System;
using CountdownAutoBattle.Data;

namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 敵方單一行動的執行期倒數狀態。
    /// </summary>
    public sealed class EnemyActionRuntime
    {
        public EnemyActionRuntime(
            EnemyActionDefinition definition,
            int displayOrder)
        {
            Definition =
                definition ??
                throw new ArgumentNullException(
                    nameof(definition));

            DisplayOrder = displayOrder;
            CurrentCountdown = definition.Cycle;
        }

        public EnemyActionDefinition Definition
        {
            get;
        }

        public int DisplayOrder { get; }

        public int CurrentCountdown { get; private set; }

        public bool IsReadyToTrigger =>
            CurrentCountdown == 1;

        /// <summary>
        /// 推進一回合。
        ///
        /// 若回合開始時倒數為 1，
        /// 本回合觸發並重設為初始週期。
        /// </summary>
        public bool AdvanceRound()
        {
            if (IsReadyToTrigger)
            {
                CurrentCountdown =
                    Definition.Cycle;

                return true;
            }

            CurrentCountdown--;
            return false;
        }
    }
}