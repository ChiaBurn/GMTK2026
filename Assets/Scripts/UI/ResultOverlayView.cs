using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CountdownAutoBattle.UI
{
    /// <summary>
    /// 顯示最精簡的戰鬥結果畫面。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResultOverlayView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private TMP_Text resultTitleText;

        [SerializeField]
        private Button restartButton;

        public event Action RestartRequested;

        private void OnEnable()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(
                    HandleRestartClicked);
            }
        }

        private void OnDisable()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(
                    HandleRestartClicked);
            }
        }

        public void Show(bool playerWon)
        {
            gameObject.SetActive(true);

            if (resultTitleText != null)
            {
                resultTitleText.text =
                    playerWon
                        ? "WIN"
                        : "LOSE";
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HandleRestartClicked()
        {
            RestartRequested?.Invoke();
        }
    }
}