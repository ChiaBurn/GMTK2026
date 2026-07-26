using UnityEngine;

namespace CountdownAutoBattle.Data
{
    [CreateAssetMenu(
        fileName = "EquipmentDefinition",
        menuName = "Countdown Auto Battle/Equipment Definition")]
    public sealed class EquipmentDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string equipmentId;

        [SerializeField]
        private string displayName;

        [Header("Gameplay")]
        [SerializeField]
        private EquipmentEffectType effectType;

        [SerializeField, Range(1, 3)]
        private int slotCount = 1;

        [SerializeField]
        private EquipmentFormulaType formulaType;

        [Header("UI")]
        [SerializeField, TextArea]
        private string formulaDescription;

        public string EquipmentId => equipmentId;

        public string DisplayName => displayName;

        public EquipmentEffectType EffectType => effectType;

        public int SlotCount => slotCount;

        public EquipmentFormulaType FormulaType => formulaType;

        public string FormulaDescription => formulaDescription;
    }
}