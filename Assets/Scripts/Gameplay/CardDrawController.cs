using System.Collections.Generic;
using CountdownAutoBattle.UI;
using UnityEngine;

namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 管理本次 Run 的點數卡牌庫，以及將點數卡抽至卡池的行為。
    ///
    /// 此元件不直接監聽 UI Button；
    /// 關卡流程與按鈕操作由 GameFlowController 負責。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CardDrawController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PointCardView pointCardPrefab;

        [SerializeField]
        private List<CardSlotView> poolSlots = new();

        private readonly List<PointCardInstance> drawPile = new();

        private int nextInstanceId = 1;

        private static readonly int[] InitialCardValues =
        {
            1, 2, 2, 2, 3,
            3, 4, 4, 5, 6
        };

        public int DrawPileCount => drawPile.Count;

        public IReadOnlyList<CardSlotView> PoolSlots => poolSlots;

        private void Awake()
        {
            ValidateReferences();
            CreateInitialDeck();
        }

        /// <summary>
        /// 從剩餘牌庫抽卡，填滿所有空的卡池槽。
        /// 回傳實際抽出的卡片數量。
        /// </summary>
        public int DrawToFillPool()
        {
            int emptySlotCount = CountEmptyPoolSlots();

            int drawCount = Mathf.Min(
                emptySlotCount,
                drawPile.Count);

            for (int i = 0; i < drawCount; i++)
            {
                DrawOneCard();
            }

            return drawCount;
        }

        private void CreateInitialDeck()
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

        private int CountEmptyPoolSlots()
        {
            int count = 0;

            foreach (CardSlotView slot in poolSlots)
            {
                if (slot != null && slot.IsEmpty)
                {
                    count++;
                }
            }

            return count;
        }

        private void DrawOneCard()
        {
            CardSlotView targetSlot =
                poolSlots.Find(slot =>
                    slot != null && slot.IsEmpty);

            if (targetSlot == null || drawPile.Count == 0)
            {
                return;
            }

            int randomIndex =
                Random.Range(0, drawPile.Count);

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

            for (int i = 0; i < poolSlots.Count; i++)
            {
                if (poolSlots[i] == null)
                {
                    Debug.LogError(
                        $"Pool slot at index {i} is not assigned.",
                        this);
                }
            }
        }
    }
}