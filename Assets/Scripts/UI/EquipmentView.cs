using System.Collections.Generic;
using CountdownAutoBattle.Data;
using CountdownAutoBattle.Gameplay;
using CountdownAutoBattle.Utilities;
using TMPro;
using UnityEngine;

namespace CountdownAutoBattle.UI
{
    [DisallowMultipleComponent]
    public sealed class EquipmentView : MonoBehaviour
    {
        [Header("Definition")]
        [SerializeField]
        private EquipmentDefinition definition;

        [Header("Slots")]
        [SerializeField]
        private List<CardSlotView> slots = new();

        [Header("Text")]
        [SerializeField]
        private TMP_Text nameText;

        [SerializeField]
        private TMP_Text formulaText;

        [SerializeField]
        private TMP_Text effectValueText;

        [SerializeField]
        private TMP_Text cycleValueText;

        public EquipmentDefinition Definition => definition;

        public bool IsEnabled { get; private set; }

        public int CurrentEffectValue { get; private set; }

        public int CurrentCycleValue { get; private set; }

        private void Awake()
        {
            ValidateConfiguration();
            RefreshStaticText();
            RefreshCalculatedValues();
        }

        private void OnEnable()
        {
            foreach (CardSlotView slot in slots)
            {
                if (slot != null)
                {
                    slot.CardChanged += HandleSlotCardChanged;
                }
            }

            RefreshCalculatedValues();
        }

        private void OnDisable()
        {
            foreach (CardSlotView slot in slots)
            {
                if (slot != null)
                {
                    slot.CardChanged -= HandleSlotCardChanged;
                }
            }
        }

        private void HandleSlotCardChanged(CardSlotView changedSlot)
        {
            RefreshCalculatedValues();
        }

        private void RefreshStaticText()
        {
            if (definition == null)
            {
                return;
            }

            if (nameText != null)
            {
                nameText.text = definition.DisplayName;
            }

            if (formulaText != null)
            {
                formulaText.text =
                    definition.FormulaDescription;
            }
        }

        private void RefreshCalculatedValues()
        {
            IsEnabled = TryGetCardValues(
                out List<int> values);

            if (!IsEnabled)
            {
                CurrentEffectValue = 0;
                CurrentCycleValue = 0;

                if (effectValueText != null)
                {
                    effectValueText.text = "--";
                }

                if (cycleValueText != null)
                {
                    cycleValueText.text = "--";
                }

                return;
            }

            CurrentEffectValue =
                EquipmentFormulaCalculator
                    .CalculateFinalValue(
                        definition,
                        values);

            CurrentCycleValue =
                MathUtility.LeastCommonMultiple(values);

            if (effectValueText != null)
            {
                effectValueText.text =
                    CurrentEffectValue.ToString();
            }

            if (cycleValueText != null)
            {
                cycleValueText.text =
                    CurrentCycleValue.ToString();
            }
        }

        private bool TryGetCardValues(
            out List<int> values)
        {
            values = new List<int>(slots.Count);

            if (definition == null ||
                slots.Count != definition.SlotCount)
            {
                return false;
            }

            foreach (CardSlotView slot in slots)
            {
                if (slot == null ||
                    slot.CurrentCard == null ||
                    slot.CurrentCard.CardData == null)
                {
                    values.Clear();
                    return false;
                }

                values.Add(
                    slot.CurrentCard.CardData.Value);
            }

            return true;
        }

        private void ValidateConfiguration()
        {
            if (definition == null)
            {
                Debug.LogError(
                    "Equipment definition is not assigned.",
                    this);

                return;
            }

            if (slots.Count != definition.SlotCount)
            {
                Debug.LogError(
                    $"Equipment '{definition.DisplayName}' expects " +
                    $"{definition.SlotCount} slots, but the view has " +
                    $"{slots.Count}.",
                    this);
            }
        }
    }
}