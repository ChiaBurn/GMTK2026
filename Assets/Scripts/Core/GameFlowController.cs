using System;
using CountdownAutoBattle.Gameplay;
using CountdownAutoBattle.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CountdownAutoBattle.Core
{
    /// <summary>
    /// 控制單一關卡的階段、主按鈕、結果畫面與重置流程。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameFlowController : MonoBehaviour
    {
        [Header("Primary Button")]
        [SerializeField]
        private Button primaryButton;

        [SerializeField]
        private TMP_Text primaryButtonLabel;

        [Header("Controllers")]
        [SerializeField]
        private CardDrawController cardDrawController;

        [SerializeField]
        private CombatController combatController;

        [Header("Result")]
        [SerializeField]
        private ResultOverlayView resultOverlayView;

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

            if (resultOverlayView != null)
            {
                resultOverlayView.RestartRequested +=
                    HandleRestartRequested;
            }
        }

        private void Start()
        {
            ResetRun();
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

            if (resultOverlayView != null)
            {
                resultOverlayView.RestartRequested -=
                    HandleRestartRequested;
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
            combatController.StartCombat();
        }

        private void HandleCombatFinished(
            bool playerWon)
        {
            SetPhase(GamePhase.Result);
            resultOverlayView.Show(playerWon);
        }

        private void HandleRestartRequested()
        {
            ResetRun();
        }

        private void ResetRun()
        {
            resultOverlayView.Hide();

            combatController.ResetCombat();

            cardDrawController
                .ResetAllCardsAndDeck();

            SetPhase(GamePhase.BeforeDraw);
        }

        private void SetPhase(
            GamePhase nextPhase)
        {
            currentPhase = nextPhase;

            ApplyPhaseState(nextPhase);
            PhaseChanged?.Invoke(nextPhase);

            Debug.Log(
                $"Game phase changed to: {nextPhase}",
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
            primaryButton.gameObject.SetActive(visible);
            primaryButton.interactable = interactable;
            primaryButtonLabel.text = label;
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

            if (combatController == null)
            {
                Debug.LogError(
                    "Combat Controller is not assigned.",
                    this);
            }

            if (resultOverlayView == null)
            {
                Debug.LogError(
                    "Result Overlay View is not assigned.",
                    this);
            }
        }
    }
}