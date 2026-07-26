using CountdownAutoBattle.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CountdownAutoBattle.UI
{
    /// <summary>
    /// 點數卡的 UI View。
    ///
    /// 負責：
    /// - 顯示點數卡資料
    /// - 點擊選取
    /// - 拖曳操作
    /// - 根據遊戲階段啟用或停用互動
    /// </summary>
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
        [Header("References")]
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

        private bool isInteractionEnabled = true;
        private bool isDragging;

        public PointCardInstance CardData => cardData;

        public RectTransform RectTransform => rectTransform;

        public CardSlotView CurrentSlot => currentSlot;

        public bool IsInteractionEnabled => isInteractionEnabled;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (background == null)
            {
                background = GetComponent<Image>();
            }

            if (valueText == null)
            {
                valueText = GetComponentInChildren<TMP_Text>();
            }

            Canvas parentCanvas =
                GetComponentInParent<Canvas>();

            if (parentCanvas != null)
            {
                rootCanvas = parentCanvas.rootCanvas;
            }
        }

        public void Bind(PointCardInstance data)
        {
            if (data == null)
            {
                Debug.LogError(
                    "Cannot bind null PointCardInstance.",
                    this);

                return;
            }

            cardData = data;

            if (valueText == null)
            {
                Debug.LogError(
                    "PointCardView requires a TMP_Text reference.",
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

        public void SetInteractionEnabled(bool enabled)
        {
            isInteractionEnabled = enabled;

            /*
             * 若切換階段時正在拖曳，
             * 強制將卡片放回原始槽位。
             */
            if (!enabled && isDragging)
            {
                ReturnToOriginSlot();
            }

            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractionEnabled)
            {
                return;
            }

            if (eventData.button !=
                PointerEventData.InputButton.Left)
            {
                return;
            }

            CardPlacementService.Instance?
                .HandleCardClicked(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isInteractionEnabled)
            {
                return;
            }

            dragOriginSlot = currentSlot;
            isDragging = true;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.85f;

            if (rootCanvas != null)
            {
                transform.SetParent(
                    rootCanvas.transform,
                    true);

                transform.SetAsLastSibling();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isInteractionEnabled ||
                !isDragging ||
                rootCanvas == null)
            {
                return;
            }

            RectTransform canvasRect =
                rootCanvas.transform as RectTransform;

            if (canvasRect == null)
            {
                return;
            }

            bool converted =
                RectTransformUtility
                    .ScreenPointToLocalPointInRectangle(
                        canvasRect,
                        eventData.position,
                        eventData.pressEventCamera,
                        out Vector2 localPoint);

            if (converted)
            {
                rectTransform.localPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }

            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            /*
             * 若沒有成功被其他槽位接收，
             * CurrentSlot 仍會是原始槽位，
             * 此時把卡片重新放回原位。
             */
            if (currentSlot == dragOriginSlot ||
                currentSlot == null)
            {
                dragOriginSlot?.SetCard(this);
            }

            dragOriginSlot = null;
            isDragging = false;
        }

        private void ReturnToOriginSlot()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            if (dragOriginSlot != null)
            {
                dragOriginSlot.SetCard(this);
            }
            else if (currentSlot != null)
            {
                currentSlot.SetCard(this);
            }

            dragOriginSlot = null;
            isDragging = false;
        }
    }
}