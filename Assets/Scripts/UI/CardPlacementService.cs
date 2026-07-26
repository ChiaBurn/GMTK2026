using UnityEngine;

namespace CountdownAutoBattle.UI
{
    /// <summary>
    /// 點數卡移動、交換與點擊配置的唯一修改入口。
    ///
    /// 支援：
    /// - 拖曳至空槽
    /// - 拖曳交換
    /// - 點擊卡片後點擊空槽
    /// - 點擊 A 卡後點擊 B 卡直接交換
    /// - 遊戲階段切換時鎖定互動
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CardPlacementService : MonoBehaviour
    {
        public static CardPlacementService Instance
        {
            get;
            private set;
        }

        private PointCardView selectedCard;
        private bool isInteractionEnabled = true;

        public bool IsInteractionEnabled =>
            isInteractionEnabled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError(
                    "Multiple CardPlacementService instances detected.",
                    this);

                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetInteractionEnabled(bool enabled)
        {
            isInteractionEnabled = enabled;

            if (!enabled)
            {
                ClearSelection();
            }

            PointCardView[] cards =
                FindObjectsByType<PointCardView>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

            foreach (PointCardView card in cards)
            {
                card.SetInteractionEnabled(enabled);
            }
        }

        public void HandleCardClicked(
            PointCardView clickedCard)
        {
            if (!isInteractionEnabled ||
                clickedCard == null)
            {
                return;
            }

            /*
             * 尚未選取卡片：
             * 選取本次點擊的卡片。
             */
            if (selectedCard == null)
            {
                SelectCard(clickedCard);
                return;
            }

            /*
             * 再次點擊同一張卡：
             * 取消選取。
             */
            if (selectedCard == clickedCard)
            {
                ClearSelection();
                return;
            }

            /*
             * 已選取 A 卡，再點擊 B 卡：
             * 以 B 卡所在槽位作為目標，
             * 交由 TryPlaceCard 完成交換。
             */
            CardSlotView targetSlot =
                clickedCard.CurrentSlot;

            if (targetSlot == null)
            {
                Debug.LogWarning(
                    "Clicked card does not belong to a valid slot.",
                    clickedCard);

                ClearSelection();
                return;
            }

            TryPlaceCard(
                selectedCard,
                targetSlot);

            ClearSelection();
        }

        public void HandleSlotClicked(
            CardSlotView targetSlot)
        {
            if (!isInteractionEnabled ||
                selectedCard == null ||
                targetSlot == null)
            {
                return;
            }

            TryPlaceCard(
                selectedCard,
                targetSlot);

            ClearSelection();
        }

        public bool TryPlaceCard(
            PointCardView movingCard,
            CardSlotView targetSlot)
        {
            if (!isInteractionEnabled ||
                movingCard == null ||
                targetSlot == null)
            {
                return false;
            }

            CardSlotView sourceSlot =
                movingCard.CurrentSlot;

            if (sourceSlot == targetSlot)
            {
                targetSlot.SetCard(movingCard);
                return true;
            }

            PointCardView displacedCard =
                targetSlot.CurrentCard;

            sourceSlot?.RemoveCard();
            targetSlot.RemoveCard();

            targetSlot.SetCard(movingCard);

            /*
             * 目標槽原本已有卡片時，
             * 將該卡片放回來源槽，形成交換。
             */
            if (displacedCard != null)
            {
                if (sourceSlot == null)
                {
                    Debug.LogError(
                        "Cannot swap without a valid source slot.",
                        movingCard);

                    targetSlot.RemoveCard();
                    targetSlot.SetCard(displacedCard);

                    return false;
                }

                sourceSlot.SetCard(displacedCard);
            }

            return true;
        }

        private void SelectCard(PointCardView card)
        {
            ClearSelection();

            selectedCard = card;
            selectedCard.SetSelected(true);
        }

        private void ClearSelection()
        {
            if (selectedCard == null)
            {
                return;
            }

            selectedCard.SetSelected(false);
            selectedCard = null;
        }
    }
}