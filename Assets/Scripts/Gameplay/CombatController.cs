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
    /// 控制單一關卡的自動戰鬥回合。
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

        [Header("UI")]
        [SerializeField]
        private TMP_Text roundText;

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

            equipmentRuntimes.Clear();

            List<EquipmentView> sortedEquipment =
                new(equipmentViews);

            sortedEquipment.Sort(
                (left, right) =>
                    left.DisplayOrder.CompareTo(
                        right.DisplayOrder));

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

            enemyActionRuntimes.Clear();

            for (int i = 0;
                 i < enemyDefinition.Actions.Count;
                 i++)
            {
                EnemyActionDefinition action =
                    enemyDefinition.Actions[i];

                if (action != null)
                {
                    enemyActionRuntimes.Add(
                        new EnemyActionRuntime(
                            action,
                            displayOrder: i));
                }
            }

            UpdateRoundDisplay();

            Debug.Log(
                $"Combat started | " +
                $"Player HP: {playerState.CurrentHp}, " +
                $"Enemy HP: {enemyState.CurrentHp}, " +
                $"Activated equipment: " +
                $"{equipmentRuntimes.Count}",
                this);
        }

        private IEnumerator RunCombatRoutine()
        {
            /*
             * 回合 0 先呈現初始倒數狀態，
             * 經過第一段間隔後才進入回合 1。
             */
            yield return new WaitForSeconds(
                GetRoundInterval(currentRound));

            while (isRunning)
            {
                currentRound++;
                UpdateRoundDisplay();

                CombatResolutionResult result =
                    ResolveCurrentRound();

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

                PendingEffect effect =
                    CreateEnemyPendingEffect(
                        runtime);

                pendingEffects.Add(effect);
            }

            return CombatResolver.ResolveRound(
                playerState,
                enemyState,
                pendingEffects);
        }

        private PendingEffect
            CreateEnemyPendingEffect(
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

        private float GetRoundInterval(
            int round)
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

            foreach (
                CombatResolutionRecord record
                in result.Records)
            {
                Debug.Log(
                    $"  {record.Effect} | " +
                    $"HP {record.HpBefore}" +
                    $"→{record.HpAfter}, " +
                    $"Shield " +
                    $"{record.ShieldBefore}" +
                    $"→{record.ShieldAfter}",
                    this);
            }
        }

        private bool ValidateConfiguration()
        {
            if (enemyDefinition == null)
            {
                Debug.LogError(
                    "Enemy Definition is not assigned.",
                    this);

                return false;
            }

            if (roundText == null)
            {
                Debug.LogError(
                    "Round Text is not assigned.",
                    this);

                return false;
            }

            if (equipmentViews.Count == 0)
            {
                Debug.LogError(
                    "No Equipment Views assigned.",
                    this);

                return false;
            }

            return true;
        }
    }
}