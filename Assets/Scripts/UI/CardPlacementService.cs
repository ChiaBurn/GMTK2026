using UnityEngine;

namespace CountdownAutoBattle.UI
{
    /// <summary>
    /// 點卡移動、交換與點擊配置的唯一修改入口。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CardPlacementService : MonoBehaviour
    {
        public static CardPlacementService Instance { get; private set; }

        private PointCardView selectedCard;

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

        public void HandleCardClicked(PointCardView clickedCard)
        {
            if (clickedCard == null)
            {
                return;
            }

            /*
             * 尚未選取任何卡片：
             * 將本次點擊的卡片設為選取狀態。
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
             * 將 A 卡移動到 B 卡所在槽位。
             *
             * TryPlaceCard 會偵測目標槽已有 B 卡，
             * 並把 B 卡放回 A 卡原本的槽位，因此形成交換。
             */
            CardSlotView targetSlot = clickedCard.CurrentSlot;

            if (targetSlot == null)
            {
                Debug.LogWarning(
                    "Clicked card does not belong to a valid slot.",
                    clickedCard);

                ClearSelection();
                return;
            }

            TryPlaceCard(selectedCard, targetSlot);
            ClearSelection();
        }

        public void HandleSlotClicked(CardSlotView targetSlot)
        {
            if (selectedCard == null ||
                targetSlot == null)
            {
                return;
            }

            TryPlaceCard(selectedCard, targetSlot);
            ClearSelection();
        }

        public bool TryPlaceCard(
            PointCardView movingCard,
            CardSlotView targetSlot)
        {
            if (movingCard == null ||
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