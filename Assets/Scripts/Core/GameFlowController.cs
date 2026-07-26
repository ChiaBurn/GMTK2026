using System;
using CountdownAutoBattle.Gameplay;
using CountdownAutoBattle.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CountdownAutoBattle.Core
{
    /// <summary>
    /// 控制單一關卡的主要階段與中央主按鈕。
    ///
    /// 流程：
    /// BeforeDraw
    /// → Configuration
    /// → Combat
    /// → Result
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameFlowController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Button primaryButton;

        [SerializeField]
        private TMP_Text primaryButtonLabel;

        [SerializeField]
        private CardDrawController cardDrawController;

        [Header("Runtime State")]
        [SerializeField]
        private GamePhase currentPhase;

        public GamePhase CurrentPhase =>
            currentPhase;

        public event Action<GamePhase> PhaseChanged;

        private void Awake()
        {
            ValidateReferences();
        }

        private void OnEnable()
        {
            if (primaryButton != null)
            {
                primaryButton.onClick.AddListener(
                    HandlePrimaryButtonClicked);
            }
        }

        private void Start()
        {
            SetPhase(GamePhase.BeforeDraw);
        }

        private void OnDisable()
        {
            if (primaryButton != null)
            {
                primaryButton.onClick.RemoveListener(
                    HandlePrimaryButtonClicked);
            }
        }

        private void HandlePrimaryButtonClicked()
        {
            switch (currentPhase)
            {
                case GamePhase.BeforeDraw:
                    HandleDraw();
                    break;

                case GamePhase.Configuration:
                    BeginCombat();
                    break;

                case GamePhase.Combat:
                case GamePhase.Result:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(currentPhase),
                        currentPhase,
                        null);
            }
        }

        private void HandleDraw()
        {
            if (cardDrawController == null)
            {
                Debug.LogError(
                    "CardDrawController is unavailable.",
                    this);

                return;
            }

            int drawnCount =
                cardDrawController.DrawToFillPool();

            if (drawnCount <= 0)
            {
                Debug.LogWarning(
                    "No cards were drawn.",
                    this);

                return;
            }

            SetPhase(GamePhase.Configuration);
        }

        private void BeginCombat()
        {
            /*
             * 本階段先切換至 Combat 並鎖定配置操作。
             * 下一階段會由 CombatController 啟動自動戰鬥。
             */
            SetPhase(GamePhase.Combat);

            Debug.Log(
                "Combat phase entered. " +
                "Automatic combat is not implemented yet.",
                this);
        }

        private void SetPhase(GamePhase nextPhase)
        {
            currentPhase = nextPhase;

            ApplyPhaseState(nextPhase);
            PhaseChanged?.Invoke(nextPhase);

            Debug.Log(
                $"Game phase changed to: {nextPhase}",
                this);
        }

        private void ApplyPhaseState(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.BeforeDraw:
                    SetPrimaryButton(
                        label: "DRAW",
                        interactable: true,
                        visible: true);

                    SetCardInteraction(false);
                    break;

                case GamePhase.Configuration:
                    SetPrimaryButton(
                        label: "COUNT DOWN",
                        interactable: true,
                        visible: true);

                    SetCardInteraction(true);
                    break;

                case GamePhase.Combat:
                    SetPrimaryButton(
                        label: "0",
                        interactable: false,
                        visible: true);

                    SetCardInteraction(false);
                    break;

                case GamePhase.Result:
                    SetPrimaryButton(
                        label: string.Empty,
                        interactable: false,
                        visible: false);

                    SetCardInteraction(false);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(phase),
                        phase,
                        null);
            }
        }

        private void SetPrimaryButton(
            string label,
            bool interactable,
            bool visible)
        {
            if (primaryButton == null)
            {
                return;
            }

            primaryButton.gameObject.SetActive(visible);
            primaryButton.interactable = interactable;

            if (primaryButtonLabel != null)
            {
                primaryButtonLabel.text = label;
            }
        }

        private static void SetCardInteraction(
            bool enabled)
        {
            if (CardPlacementService.Instance == null)
            {
                Debug.LogWarning(
                    "CardPlacementService instance is unavailable.");

                return;
            }

            CardPlacementService.Instance
                .SetInteractionEnabled(enabled);
        }

        private void ValidateReferences()
        {
            if (primaryButton == null)
            {
                Debug.LogError(
                    "Primary Button is not assigned.",
                    this);
            }

            if (primaryButtonLabel == null)
            {
                Debug.LogError(
                    "Primary Button Label is not assigned.",
                    this);
            }

            if (cardDrawController == null)
            {
                Debug.LogError(
                    "Card Draw Controller is not assigned.",
                    this);
            }
        }
    }
}