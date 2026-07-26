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
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameFlowController :
        MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Button primaryButton;

        [SerializeField]
        private TMP_Text primaryButtonLabel;

        [SerializeField]
        private CardDrawController
            cardDrawController;

        [SerializeField]
        private CombatController combatController;

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

            if (combatController != null)
            {
                combatController.CombatFinished +=
                    HandleCombatFinished;
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

            if (combatController != null)
            {
                combatController.CombatFinished -=
                    HandleCombatFinished;
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
                    throw new
                        ArgumentOutOfRangeException(
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
            SetPhase(GamePhase.Combat);

            combatController?.StartCombat();
        }

        private void HandleCombatFinished(
            bool playerWon)
        {
            SetPhase(GamePhase.Result);

            Debug.Log(
                playerWon
                    ? "Flow entered Result: WIN."
                    : "Flow entered Result: LOSE.",
                this);
        }

        private void SetPhase(
            GamePhase nextPhase)
        {
            currentPhase = nextPhase;

            ApplyPhaseState(nextPhase);
            PhaseChanged?.Invoke(nextPhase);

            Debug.Log(
                $"Game phase changed to: " +
                $"{nextPhase}",
                this);
        }

        private void ApplyPhaseState(
            GamePhase phase)
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
                    /*
                     * CombatController 會直接更新
                     * 同一個文字元件為回合數。
                     */
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
                    throw new
                        ArgumentOutOfRangeException(
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

            primaryButton.gameObject
                .SetActive(visible);

            primaryButton.interactable =
                interactable;

            if (primaryButtonLabel != null)
            {
                primaryButtonLabel.text =
                    label;
            }
        }

        private static void SetCardInteraction(
            bool enabled)
        {
            if (CardPlacementService.Instance == null)
            {
                Debug.LogWarning(
                    "CardPlacementService instance " +
                    "is unavailable.");

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

            if (combatController == null)
            {
                Debug.LogError(
                    "Combat Controller is not assigned.",
                    this);
            }
        }
    }
}