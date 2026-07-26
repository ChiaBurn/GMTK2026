using System;
using System.Collections;
using System.Collections.Generic;
using CountdownAutoBattle.Data;
using CountdownAutoBattle.UI;
using TMPro;
using UnityEngine;

namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 控制單一關卡的自動戰鬥、倒數與 UI 更新。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatController :
        MonoBehaviour
    {
        [Header("Definitions")]
        [SerializeField]
        private EnemyDefinition enemyDefinition;

        [Header("Player Equipment")]
        [SerializeField]
        private List<EquipmentView> equipmentViews =
            new();

        [Header("Main UI")]
        [SerializeField]
        private TMP_Text roundText;

        [SerializeField]
        private CombatantView playerView;

        [SerializeField]
        private CombatantView enemyView;

        [Header("Enemy Action UI")]
        [SerializeField]
        private List<EnemyActionView> enemyActionViews =
            new();

        [Header("Player Initial State")]
        [SerializeField, Min(1)]
        private int playerMaxHp = 40;

        [SerializeField, Min(0)]
        private int playerInitialShield;

        [Header("Combat Timing")]
        [SerializeField, Min(0.01f)]
        private float earlyRoundInterval = 1f;

        [SerializeField, Min(0.01f)]
        private float middleRoundInterval = 0.75f;

        [SerializeField, Min(0.01f)]
        private float lateRoundInterval = 0.5f;

        [SerializeField, Min(0.01f)]
        private float finalRoundInterval = 0.35f;

        [SerializeField, Min(1)]
        private int middleRoundThreshold = 20;

        [SerializeField, Min(1)]
        private int lateRoundThreshold = 35;

        [SerializeField, Min(1)]
        private int finalRoundThreshold = 50;

        [SerializeField, Min(1)]
        private int maximumRound = 300;

        private readonly List<
            EquipmentCountdownRuntime>
            equipmentRuntimes = new();

        private readonly List<
            EnemyActionRuntime>
            enemyActionRuntimes = new();

        private Coroutine combatRoutine;

        private CombatantState playerState;
        private CombatantState enemyState;

        private int currentRound;
        private bool isRunning;

        public CombatantState PlayerState =>
            playerState;

        public CombatantState EnemyState =>
            enemyState;

        public int CurrentRound =>
            currentRound;

        public bool IsRunning =>
            isRunning;

        public event Action<int> RoundChanged;

        public event Action<
            CombatantState,
            CombatantState,
            CombatResolutionResult>
            RoundResolved;

        public event Action<bool> CombatFinished;

        public void StartCombat()
        {
            if (isRunning)
            {
                Debug.LogWarning(
                    "Combat is already running.",
                    this);

                return;
            }

            if (!ValidateConfiguration())
            {
                return;
            }

            InitializeCombat();

            combatRoutine =
                StartCoroutine(
                    RunCombatRoutine());
        }

        public void StopCombat()
        {
            if (combatRoutine != null)
            {
                StopCoroutine(combatRoutine);
                combatRoutine = null;
            }

            isRunning = false;
        }

        public void ResetCombat()
        {
            StopCombat();

            currentRound = 0;
            playerState = null;
            enemyState = null;

            equipmentRuntimes.Clear();
            enemyActionRuntimes.Clear();

            if (roundText != null)
            {
                roundText.text = "DRAW";
            }

            playerView?.ResetView();
            enemyView?.ResetView();

            foreach (EnemyActionView actionView
                     in enemyActionViews)
            {
                actionView?.Clear();
            }
        }

        private void OnDisable()
        {
            StopCombat();
        }

        private void InitializeCombat()
        {
            currentRound = 0;
            isRunning = true;

            playerState =
                new CombatantState(
                    CombatSide.Player,
                    maxHp: playerMaxHp,
                    currentHp: playerMaxHp,
                    shield:
                        playerInitialShield);

            enemyState =
                new CombatantState(
                    CombatSide.Enemy,
                    maxHp:
                        enemyDefinition.MaxHp,
                    currentHp:
                        enemyDefinition.MaxHp,
                    shield:
                        enemyDefinition
                            .InitialShield);

            InitializeEquipmentRuntimes();
            InitializeEnemyActionRuntimes();
            InitializeCombatViews();

            UpdateRoundDisplay();
            RefreshCombatantViews();

            Debug.Log(
                $"Combat started | " +
                $"Player HP: {playerState.CurrentHp}, " +
                $"Enemy HP: {enemyState.CurrentHp}, " +
                $"Activated equipment: " +
                $"{equipmentRuntimes.Count}",
                this);
        }

        private void InitializeEquipmentRuntimes()
        {
            equipmentRuntimes.Clear();

            List<EquipmentView> sortedEquipment =
                new(equipmentViews);

            sortedEquipment.Sort(
                (left, right) =>
                {
                    if (left == null &&
                        right == null)
                    {
                        return 0;
                    }

                    if (left == null)
                    {
                        return 1;
                    }

                    if (right == null)
                    {
                        return -1;
                    }

                    return left.DisplayOrder.CompareTo(
                        right.DisplayOrder);
                });

            foreach (EquipmentView equipment
                     in sortedEquipment)
            {
                if (equipment != null &&
                    equipment.IsActivated)
                {
                    equipmentRuntimes.Add(
                        new EquipmentCountdownRuntime(
                            equipment));
                }
            }
        }

        private void InitializeEnemyActionRuntimes()
        {
            enemyActionRuntimes.Clear();

            for (int i = 0;
                 i < enemyDefinition.Actions.Count;
                 i++)
            {
                EnemyActionDefinition action =
                    enemyDefinition.Actions[i];

                if (action == null)
                {
                    continue;
                }

                enemyActionRuntimes.Add(
                    new EnemyActionRuntime(
                        action,
                        displayOrder: i));
            }
        }

        private void InitializeCombatViews()
        {
            playerView?.SetDisplayName("PLAYER");

            enemyView?.SetDisplayName(
                enemyDefinition.DisplayName);

            for (int i = 0;
                 i < enemyActionViews.Count;
                 i++)
            {
                EnemyActionView actionView =
                    enemyActionViews[i];

                if (actionView == null)
                {
                    continue;
                }

                if (i <
                    enemyActionRuntimes.Count)
                {
                    actionView.gameObject
                        .SetActive(true);

                    actionView.Bind(
                        enemyActionRuntimes[i]);
                }
                else
                {
                    actionView.Clear();
                    actionView.gameObject
                        .SetActive(false);
                }
            }
        }

        private IEnumerator RunCombatRoutine()
        {
            yield return new WaitForSeconds(
                GetRoundInterval(currentRound));

            while (isRunning)
            {
                currentRound++;
                UpdateRoundDisplay();

                CombatResolutionResult result =
                    ResolveCurrentRound();

                RefreshCombatantViews();
                RefreshEnemyActionViews();
                PlayResolutionFeedback(result);

                RoundChanged?.Invoke(currentRound);

                RoundResolved?.Invoke(
                    playerState,
                    enemyState,
                    result);

                LogRoundResult(result);

                if (!result.BattleContinues)
                {
                    FinishCombat(
                        playerWon:
                            result.PlayerWon);

                    yield break;
                }

                if (currentRound >= maximumRound)
                {
                    Debug.LogWarning(
                        $"Maximum round " +
                        $"{maximumRound} reached. " +
                        "Player loses by timeout.",
                        this);

                    FinishCombat(
                        playerWon: false);

                    yield break;
                }

                yield return new WaitForSeconds(
                    GetRoundInterval(
                        currentRound));
            }
        }

        private CombatResolutionResult
            ResolveCurrentRound()
        {
            List<PendingEffect> pendingEffects =
                new();

            foreach (
                EquipmentCountdownRuntime runtime
                in equipmentRuntimes)
            {
                bool triggered =
                    runtime.AdvanceRound();

                if (!triggered)
                {
                    continue;
                }

                PendingEffect effect =
                    runtime.EquipmentView
                        .CreatePendingEffect();

                if (effect != null)
                {
                    pendingEffects.Add(effect);
                }
            }

            foreach (
                EnemyActionRuntime runtime
                in enemyActionRuntimes)
            {
                bool triggered =
                    runtime.AdvanceRound();

                if (!triggered)
                {
                    continue;
                }

                pendingEffects.Add(
                    CreateEnemyPendingEffect(
                        runtime));
            }

            return CombatResolver.ResolveRound(
                playerState,
                enemyState,
                pendingEffects);
        }

        private PendingEffect CreateEnemyPendingEffect(
            EnemyActionRuntime runtime)
        {
            EnemyActionDefinition definition =
                runtime.Definition;

            CombatSide targetSide =
                definition.EffectType ==
                CombatEffectType.Attack
                    ? CombatSide.Player
                    : CombatSide.Enemy;

            return new PendingEffect(
                sourceId:
                    definition.ActionId,

                sourceDisplayName:
                    definition.DisplayName,

                sourceSide:
                    CombatSide.Enemy,

                targetSide:
                    targetSide,

                effectType:
                    definition.EffectType,

                value:
                    definition.Power,

                sourceOrder:
                    runtime.DisplayOrder);
        }

        private void RefreshCombatantViews()
        {
            playerView?.Refresh(playerState);
            enemyView?.Refresh(enemyState);
        }

        private void RefreshEnemyActionViews()
        {
            foreach (EnemyActionView actionView
                     in enemyActionViews)
            {
                if (actionView != null &&
                    actionView.gameObject.activeSelf)
                {
                    actionView.RefreshCountdown();
                }
            }
        }

        private void PlayResolutionFeedback(
            CombatResolutionResult result)
        {
            foreach (CombatResolutionRecord record
                     in result.Records)
            {
                CombatantView targetView =
                    record.Effect.TargetSide ==
                    CombatSide.Player
                        ? playerView
                        : enemyView;

                targetView?.PlayEffectFeedback(
                    record.Effect.EffectType);
            }
        }

        private float GetRoundInterval(int round)
        {
            if (round > finalRoundThreshold)
            {
                return finalRoundInterval;
            }

            if (round > lateRoundThreshold)
            {
                return lateRoundInterval;
            }

            if (round > middleRoundThreshold)
            {
                return middleRoundInterval;
            }

            return earlyRoundInterval;
        }

        private void UpdateRoundDisplay()
        {
            if (roundText != null)
            {
                roundText.text =
                    currentRound.ToString();
            }
        }

        private void FinishCombat(bool playerWon)
        {
            isRunning = false;
            combatRoutine = null;

            RefreshCombatantViews();

            Debug.Log(
                playerWon
                    ? "Combat finished: PLAYER WIN."
                    : "Combat finished: PLAYER LOSE.",
                this);

            CombatFinished?.Invoke(playerWon);
        }

        private void LogRoundResult(
            CombatResolutionResult result)
        {
            Debug.Log(
                $"Round {currentRound} | " +
                $"Player HP: " +
                $"{playerState.CurrentHp}/" +
                $"{playerState.MaxHp}, " +
                $"Shield: {playerState.Shield} | " +
                $"Enemy HP: " +
                $"{enemyState.CurrentHp}/" +
                $"{enemyState.MaxHp}, " +
                $"Shield: {enemyState.Shield} | " +
                $"Effects: {result.Records.Count}",
                this);
        }

        private bool ValidateConfiguration()
        {
            bool valid = true;

            if (enemyDefinition == null)
            {
                Debug.LogError(
                    "Enemy Definition is not assigned.",
                    this);

                valid = false;
            }

            if (roundText == null)
            {
                Debug.LogError(
                    "Round Text is not assigned.",
                    this);

                valid = false;
            }

            if (playerView == null)
            {
                Debug.LogError(
                    "Player View is not assigned.",
                    this);

                valid = false;
            }

            if (enemyView == null)
            {
                Debug.LogError(
                    "Enemy View is not assigned.",
                    this);

                valid = false;
            }

            if (equipmentViews.Count == 0)
            {
                Debug.LogError(
                    "No Equipment Views assigned.",
                    this);

                valid = false;
            }

            if (enemyActionViews.Count <
                enemyDefinition.Actions.Count)
            {
                Debug.LogError(
                    "There are fewer Enemy Action Views " +
                    "than Enemy Actions.",
                    this);

                valid = false;
            }

            return valid;
        }
    }
}