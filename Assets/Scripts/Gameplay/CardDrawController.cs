using System.Collections.Generic;
using CountdownAutoBattle.UI;
using UnityEngine;

namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 管理本次關卡的點數卡牌庫、抽卡與重置。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CardDrawController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PointCardView pointCardPrefab;

        [SerializeField]
        private List<CardSlotView> poolSlots = new();

        [SerializeField]
        private List<CardSlotView> allCardSlots = new();

        private readonly List<PointCardInstance> drawPile =
            new();

        private int nextInstanceId = 1;

        private static readonly int[] InitialCardValues =
        {
            1, 2, 2, 2, 3,
            3, 4, 4, 5, 6
        };

        public int DrawPileCount =>
            drawPile.Count;

        public IReadOnlyList<CardSlotView> PoolSlots =>
            poolSlots;

        private void Awake()
        {
            ValidateReferences();
            RebuildDeck();
        }

        /// <summary>
        /// 從剩餘牌庫抽卡，填滿卡池中的空槽。
        /// </summary>
        public int DrawToFillPool()
        {
            int emptySlotCount =
                CountEmptyPoolSlots();

            int drawCount =
                Mathf.Min(
                    emptySlotCount,
                    drawPile.Count);

            for (int i = 0; i < drawCount; i++)
            {
                DrawOneCard();
            }

            return drawCount;
        }

        /// <summary>
        /// 清除所有場上卡片，並重建完整初始牌庫。
        /// </summary>
        public void ResetAllCardsAndDeck()
        {
            ClearAllCardSlots();
            RebuildDeck();
        }

        private void RebuildDeck()
        {
            drawPile.Clear();
            nextInstanceId = 1;

            foreach (int value in InitialCardValues)
            {
                drawPile.Add(
                    new PointCardInstance(
                        nextInstanceId++,
                        value));
            }
        }

        private void ClearAllCardSlots()
        {
            HashSet<PointCardView> cardsToDestroy =
                new();

            foreach (CardSlotView slot in allCardSlots)
            {
                if (slot == null)
                {
                    continue;
                }

                PointCardView card =
                    slot.CurrentCard;

                if (card != null)
                {
                    cardsToDestroy.Add(card);
                }

                slot.RemoveCard();
            }

            /*
             * 防守性清理：
             * 若有卡片在拖曳期間離開 Slot Hierarchy，
             * 仍一併移除。
             */
            PointCardView[] remainingCards =
                FindObjectsByType<PointCardView>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            foreach (PointCardView card
                     in remainingCards)
            {
                if (card != null)
                {
                    cardsToDestroy.Add(card);
                }
            }

            foreach (PointCardView card
                     in cardsToDestroy)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
        }

        private int CountEmptyPoolSlots()
        {
            int count = 0;

            foreach (CardSlotView slot in poolSlots)
            {
                if (slot != null &&
                    slot.IsEmpty)
                {
                    count++;
                }
            }

            return count;
        }

        private void DrawOneCard()
        {
            CardSlotView targetSlot =
                poolSlots.Find(
                    slot =>
                        slot != null &&
                        slot.IsEmpty);

            if (targetSlot == null ||
                drawPile.Count == 0)
            {
                return;
            }

            int randomIndex =
                Random.Range(
                    0,
                    drawPile.Count);

            PointCardInstance cardData =
                drawPile[randomIndex];

            drawPile.RemoveAt(randomIndex);

            PointCardView cardView =
                Instantiate(
                    pointCardPrefab,
                    targetSlot.transform);

            cardView.Bind(cardData);
            targetSlot.SetCard(cardView);
        }

        private void ValidateReferences()
        {
            if (pointCardPrefab == null)
            {
                Debug.LogError(
                    "Point Card Prefab is not assigned.",
                    this);
            }

            if (poolSlots.Count != 5)
            {
                Debug.LogError(
                    $"Pool Slots must contain exactly 5 entries. " +
                    $"Current count: {poolSlots.Count}",
                    this);
            }

            if (allCardSlots.Count == 0)
            {
                Debug.LogError(
                    "All Card Slots is empty.",
                    this);
            }

            foreach (CardSlotView poolSlot
                     in poolSlots)
            {
                if (poolSlot == null)
                {
                    Debug.LogError(
                        "Pool Slots contains an unassigned entry.",
                        this);
                }
            }

            foreach (CardSlotView slot
                     in allCardSlots)
            {
                if (slot == null)
                {
                    Debug.LogError(
                        "All Card Slots contains an unassigned entry.",
                        this);
                }
            }
        }
    }
}