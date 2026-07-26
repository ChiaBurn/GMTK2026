using CountdownAutoBattle.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CountdownAutoBattle.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class PointCardView :
        MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField]
        private TMP_Text valueText;

        [SerializeField]
        private Image background;

        private PointCardInstance cardData;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas rootCanvas;

        private CardSlotView currentSlot;
        private CardSlotView dragOriginSlot;

        public PointCardInstance CardData => cardData;

        public RectTransform RectTransform => rectTransform;

        public CardSlotView CurrentSlot => currentSlot;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            Canvas parentCanvas = GetComponentInParent<Canvas>();

            if (parentCanvas != null)
            {
                rootCanvas = parentCanvas.rootCanvas;
            }
        }

        public void Bind(PointCardInstance data)
        {
            cardData = data;

            if (valueText == null)
            {
                valueText = GetComponentInChildren<TMP_Text>();
            }

            if (background == null)
            {
                background = GetComponent<Image>();
            }

            if (valueText == null)
            {
                Debug.LogError(
                    "PointCardView requires a TMP_Text child.",
                    this);

                return;
            }

            valueText.text = data.Value.ToString();
        }

        public void SetCurrentSlot(CardSlotView slot)
        {
            currentSlot = slot;
        }

        public void SetSelected(bool selected)
        {
            if (background == null)
            {
                return;
            }

            background.color = selected
                ? new Color(1f, 0.82f, 0.25f, 1f)
                : Color.white;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            CardPlacementService.Instance?.HandleCardClicked(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragOriginSlot = currentSlot;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.85f;

            if (rootCanvas != null)
            {
                transform.SetParent(rootCanvas.transform, true);
                transform.SetAsLastSibling();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (rootCanvas == null)
            {
                return;
            }

            RectTransform canvasRect =
                rootCanvas.transform as RectTransform;

            if (canvasRect == null)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
            {
                rectTransform.localPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            /*
             * 若沒有被其他 CardSlotView 成功接收，
             * CurrentSlot 仍會是原始槽，因此回到原位。
             */
            if (currentSlot == dragOriginSlot ||
                currentSlot == null)
            {
                dragOriginSlot?.SetCard(this);
            }

            dragOriginSlot = null;
        }
    }
}