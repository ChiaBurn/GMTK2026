using System.Collections.Generic;
using UnityEngine;

namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 暫時的戰鬥解析 Smoke Test。
    ///
    /// 驗證完成後可以停用或刪除，
    /// 不會成為正式遊戲流程的一部分。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatResolverSmokeTest : MonoBehaviour
    {
        [SerializeField]
        private bool runOnStart = true;

        private void Start()
        {
            if (runOnStart)
            {
                RunTest();
            }
        }

        [ContextMenu("Run Combat Resolver Smoke Test")]
        private void RunTest()
        {
            CombatantState player =
                new(
                    CombatSide.Player,
                    maxHp: 40,
                    currentHp: 40,
                    shield: 0);

            CombatantState enemy =
                new(
                    CombatSide.Enemy,
                    maxHp: 30,
                    currentHp: 30,
                    shield: 0);

            List<PendingEffect> effects = new()
            {
                /*
                 * 玩家本回合增加 6 護盾。
                 */
                new PendingEffect(
                    sourceId: "sync_shield",
                    sourceDisplayName: "Sync Shield",
                    sourceSide: CombatSide.Player,
                    targetSide: CombatSide.Player,
                    effectType: CombatEffectType.Shield,
                    value: 6,
                    sourceOrder: 0),

                /*
                 * 敵人本回合造成 10 傷害。
                 * 應先扣除玩家 6 護盾，再扣除 4 HP。
                 */
                new PendingEffect(
                    sourceId: "enemy_shot",
                    sourceDisplayName: "Shot",
                    sourceSide: CombatSide.Enemy,
                    targetSide: CombatSide.Player,
                    effectType: CombatEffectType.Attack,
                    value: 10,
                    sourceOrder: 0),

                /*
                 * 玩家本回合恢復 3 HP。
                 * 最終應為 39 HP。
                 */
                new PendingEffect(
                    sourceId: "repair_core",
                    sourceDisplayName: "Repair Core",
                    sourceSide: CombatSide.Player,
                    targetSide: CombatSide.Player,
                    effectType: CombatEffectType.Heal,
                    value: 3,
                    sourceOrder: 0),

                /*
                 * 玩家對敵人造成 8 傷害。
                 * 敵人最終應為 22 HP。
                 */
                new PendingEffect(
                    sourceId: "pulse_cannon",
                    sourceDisplayName: "Pulse Cannon",
                    sourceSide: CombatSide.Player,
                    targetSide: CombatSide.Enemy,
                    effectType: CombatEffectType.Attack,
                    value: 8,
                    sourceOrder: 1)
            };

            CombatResolutionResult result =
                CombatResolver.ResolveRound(
                    player,
                    enemy,
                    effects);

            Debug.Log(
                $"Smoke Test Result | " +
                $"Player HP: {player.CurrentHp}/40, " +
                $"Player Shield: {player.Shield}, " +
                $"Enemy HP: {enemy.CurrentHp}/30, " +
                $"Enemy Shield: {enemy.Shield}");

            bool passed =
                player.CurrentHp == 39 &&
                player.Shield == 0 &&
                enemy.CurrentHp == 22 &&
                enemy.Shield == 0 &&
                result.BattleContinues;

            if (passed)
            {
                Debug.Log(
                    "CombatResolver Smoke Test PASSED.",
                    this);
            }
            else
            {
                Debug.LogError(
                    "CombatResolver Smoke Test FAILED.",
                    this);
            }
        }
    }
}