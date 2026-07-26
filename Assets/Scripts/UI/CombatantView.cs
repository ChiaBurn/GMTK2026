using System.Collections;
using CountdownAutoBattle.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CountdownAutoBattle.UI
{
    /// <summary>
    /// 顯示單一戰鬥單位的生命、護盾與效果回饋。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatantView : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField]
        private TMP_Text nameText;

        [SerializeField]
        private TMP_Text hpText;

        [SerializeField]
        private TMP_Text shieldText;

        [Header("Feedback")]
        [SerializeField]
        private Image feedbackImage;

        [SerializeField]
        private Color normalColor = Color.white;

        [SerializeField]
        private Color shieldFlashColor =
            new(0.35f, 0.7f, 1f, 1f);

        [SerializeField]
        private Color attackFlashColor =
            new(1f, 0.35f, 0.35f, 1f);

        [SerializeField]
        private Color healFlashColor =
            new(0.35f, 1f, 0.45f, 1f);

        [SerializeField, Min(0.01f)]
        private float flashDuration = 0.16f;

        private Coroutine flashRoutine;

        public void SetDisplayName(string displayName)
        {
            if (nameText != null)
            {
                nameText.text = displayName;
            }
        }

        public void Refresh(CombatantState state)
        {
            if (state == null)
            {
                Clear();
                return;
            }

            if (hpText != null)
            {
                /*
                 * 戰鬥內部允許暫時負血，
                 * 但畫面只顯示最低 0。
                 */
                int displayedHp =
                    Mathf.Max(0, state.CurrentHp);

                hpText.text =
                    $"HP {displayedHp} / {state.MaxHp}";
            }

            if (shieldText != null)
            {
                shieldText.text =
                    $"SHIELD {state.Shield}";
            }
        }

        public void Clear()
        {
            if (hpText != null)
            {
                hpText.text = "HP -- / --";
            }

            if (shieldText != null)
            {
                shieldText.text = "SHIELD --";
            }

            ResetFeedbackColor();
        }

        public void PlayEffectFeedback(
            CombatEffectType effectType)
        {
            if (feedbackImage == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            Color flashColor = effectType switch
            {
                CombatEffectType.Shield =>
                    shieldFlashColor,

                CombatEffectType.Attack =>
                    attackFlashColor,

                CombatEffectType.Heal =>
                    healFlashColor,

                _ => normalColor
            };

            flashRoutine =
                StartCoroutine(
                    FlashRoutine(flashColor));
        }

        public void ResetView()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            Clear();
        }

        private IEnumerator FlashRoutine(
            Color flashColor)
        {
            feedbackImage.color = flashColor;

            yield return new WaitForSeconds(
                flashDuration);

            feedbackImage.color = normalColor;
            flashRoutine = null;
        }

        private void ResetFeedbackColor()
        {
            if (feedbackImage != null)
            {
                feedbackImage.color = normalColor;
            }
        }
    }
}