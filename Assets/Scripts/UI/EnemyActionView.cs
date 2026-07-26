using CountdownAutoBattle.Data;
using CountdownAutoBattle.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CountdownAutoBattle.UI
{
    /// <summary>
    /// 顯示單一敵方行動的名稱、效果值與目前倒數。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyActionView : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField]
        private TMP_Text nameText;

        [SerializeField]
        private TMP_Text effectText;

        [SerializeField]
        private TMP_Text countdownText;

        [Header("Display")]
        [SerializeField]
        private Image background;

        [SerializeField]
        private Color normalColor = Color.white;

        [SerializeField]
        private Color readyColor =
            new(1f, 0.85f, 0.2f, 1f);

        private EnemyActionRuntime runtime;

        public EnemyActionRuntime Runtime =>
            runtime;

        public void Bind(EnemyActionRuntime actionRuntime)
        {
            runtime = actionRuntime;

            if (runtime == null)
            {
                Clear();
                return;
            }

            EnemyActionDefinition definition =
                runtime.Definition;

            if (nameText != null)
            {
                nameText.text =
                    definition.DisplayName;
            }

            if (effectText != null)
            {
                effectText.text =
                    GetEffectLabel(
                        definition.EffectType,
                        definition.Power);
            }

            RefreshCountdown();
        }

        public void RefreshCountdown()
        {
            if (runtime == null)
            {
                Clear();
                return;
            }

            int currentCountdown =
                runtime.CurrentCountdown;

            if (countdownText != null)
            {
                countdownText.text =
                    currentCountdown.ToString();
            }

            if (background != null)
            {
                background.color =
                    currentCountdown == 1
                        ? readyColor
                        : normalColor;
            }
        }

        public void Clear()
        {
            runtime = null;

            if (nameText != null)
            {
                nameText.text = "--";
            }

            if (effectText != null)
            {
                effectText.text = "--";
            }

            if (countdownText != null)
            {
                countdownText.text = "--";
            }

            if (background != null)
            {
                background.color = normalColor;
            }
        }

        private static string GetEffectLabel(
            CombatEffectType effectType,
            int power)
        {
            return effectType switch
            {
                CombatEffectType.Shield =>
                    $"SHIELD {power}",

                CombatEffectType.Attack =>
                    $"ATK {power}",

                CombatEffectType.Heal =>
                    $"HEAL {power}",

                _ => power.ToString()
            };
        }
    }
}