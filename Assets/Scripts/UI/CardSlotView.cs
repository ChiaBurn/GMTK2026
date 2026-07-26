using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CountdownAutoBattle.UI
{
    public enum CardSlotType
    {
        Pool,
        Equipment
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public sealed class CardSlotView :
        MonoBehaviour,
        IPointerClickHandler,
        IDropHandler
    {
        [SerializeField]
        private CardSlotType slotType;

        [SerializeField]
        private Image background;

        private PointCardView currentCard;

        public CardSlotType SlotType => slotType;

        public PointCardView CurrentCard => currentCard;

        public bool IsEmpty => currentCard == null;

        private void Awake()
        {
            if (background == null)
            {
                background = GetComponent<Image>();
            }
        }

        public void SetCard(PointCardView card)
        {
            currentCard = card;

            if (card == null)
            {
                return;
            }

            card.transform.SetParent(transform, false);

            RectTransform cardRect = card.RectTransform;

            cardRect.anchorMin = Vector2.zero;
            cardRect.anchorMax = Vector2.one;
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            cardRect.localScale = Vector3.one;

            card.SetCurrentSlot(this);
        }

        public PointCardView RemoveCard()
        {
            PointCardView removedCard = currentCard;
            currentCard = null;

            return removedCard;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            CardPlacementService.Instance?.HandleSlotClicked(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            PointCardView droppedCard =
                eventData.pointerDrag.GetComponent<PointCardView>();

            if (droppedCard == null)
            {
                return;
            }

            CardPlacementService.Instance?.TryPlaceCard(
                droppedCard,
                this);
        }
    }
}