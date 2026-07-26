using System;
using System.Collections.Generic;
using CountdownAutoBattle.Data;
using UnityEngine;

namespace CountdownAutoBattle.Gameplay
{
    public static class EquipmentFormulaCalculator
    {
        public static int CalculateFinalValue(
            EquipmentDefinition definition,
            IReadOnlyList<int> cardValues)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (cardValues == null)
            {
                throw new ArgumentNullException(nameof(cardValues));
            }

            if (cardValues.Count != definition.SlotCount)
            {
                throw new ArgumentException(
                    $"Equipment '{definition.DisplayName}' requires " +
                    $"{definition.SlotCount} values, but received " +
                    $"{cardValues.Count}.",
                    nameof(cardValues));
            }

            double rawValue = definition.FormulaType switch
            {
                EquipmentFormulaType.PulseCannon =>
                    CalculatePulseCannon(cardValues),

                EquipmentFormulaType.SyncShield =>
                    CalculateSyncShield(cardValues),

                EquipmentFormulaType.RepairCore =>
                    CalculateRepairCore(cardValues),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(definition.FormulaType),
                    definition.FormulaType,
                    "Unsupported equipment formula.")
            };

            /*
             * 規格要求：
             * 中間可保留小數，最後結果四捨五入。
             *
             * MidpointRounding.AwayFromZero：
             * 2.5 → 3，而不是銀行家捨入成 2。
             */
            return (int)Math.Round(
                rawValue,
                MidpointRounding.AwayFromZero);
        }

        private static double CalculatePulseCannon(
            IReadOnlyList<int> values)
        {
            return 2d * values[0];
        }

        private static double CalculateSyncShield(
            IReadOnlyList<int> values)
        {
            return 2d + values[0] + values[1];
        }

        private static double CalculateRepairCore(
            IReadOnlyList<int> values)
        {
            int highest = int.MinValue;
            int lowest = int.MaxValue;

            foreach (int value in values)
            {
                highest = Mathf.Max(highest, value);
                lowest = Mathf.Min(lowest, value);
            }

            return highest + lowest;
        }
    }
}