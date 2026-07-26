using System;
using System.Collections.Generic;
using CountdownAutoBattle.UI;

namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 單件已啟用裝備在戰鬥中的倒數狀態。
    ///
    /// 每張點卡獨立循環：
    /// - CurrentValue > 1 時減 1。
    /// - CurrentValue == 1 時回到 BaseValue。
    ///
    /// 若回合開始時所有 CurrentValue 都為 1，
    /// 則裝備於該回合發動並將所有卡重設。
    /// </summary>
    public sealed class EquipmentCountdownRuntime
    {
        private readonly EquipmentView equipmentView;
        private readonly List<int> baseValues = new();
        private readonly List<int> currentValues = new();

        public EquipmentCountdownRuntime(
            EquipmentView equipmentView)
        {
            this.equipmentView =
                equipmentView ??
                throw new ArgumentNullException(
                    nameof(equipmentView));

            if (!equipmentView.IsActivated)
            {
                throw new InvalidOperationException(
                    $"Equipment '{equipmentView.name}' " +
                    "is not activated.");
            }

            IReadOnlyList<CardSlotView> slots =
                equipmentView.Slots;

            foreach (CardSlotView slot in slots)
            {
                int value =
                    slot.CurrentCard.CardData.Value;

                baseValues.Add(value);
                currentValues.Add(value);
            }

            RefreshCardDisplays();
        }

        public EquipmentView EquipmentView =>
            equipmentView;

        public IReadOnlyList<int> BaseValues =>
            baseValues;

        public IReadOnlyList<int> CurrentValues =>
            currentValues;

        public bool IsReadyToTrigger
        {
            get
            {
                foreach (int value in currentValues)
                {
                    if (value != 1)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// 推進一個戰鬥回合。
        ///
        /// 回傳 true 代表本回合觸發裝備。
        /// </summary>
        public bool AdvanceRound()
        {
            if (IsReadyToTrigger)
            {
                ResetToBaseValues();
                RefreshCardDisplays();
                return true;
            }

            for (int i = 0;
                 i < currentValues.Count;
                 i++)
            {
                currentValues[i] =
                    currentValues[i] == 1
                        ? baseValues[i]
                        : currentValues[i] - 1;
            }

            RefreshCardDisplays();
            return false;
        }

        public void RestoreConfigurationDisplay()
        {
            foreach (CardSlotView slot
                     in equipmentView.Slots)
            {
                slot.CurrentCard?
                    .ShowConfigurationValue();
            }
        }

        private void ResetToBaseValues()
        {
            for (int i = 0;
                 i < currentValues.Count;
                 i++)
            {
                currentValues[i] = baseValues[i];
            }
        }

        private void RefreshCardDisplays()
        {
            IReadOnlyList<CardSlotView> slots =
                equipmentView.Slots;

            for (int i = 0;
                 i < slots.Count;
                 i++)
            {
                slots[i].CurrentCard?
                    .ShowCombatCountdown(
                        currentValues[i]);
            }
        }
    }
}