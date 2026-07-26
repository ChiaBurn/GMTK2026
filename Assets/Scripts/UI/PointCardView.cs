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
    /// 配置階段：
    /// - 顯示單一初始點數。
    /// - 支援點擊、拖曳與交換。
    ///
    /// 戰鬥階段：
    /// - 大字顯示目前倒數值。
    /// - 小字顯示初始點數。
    /// - CurrentValue 為 1 時顯示黃色。
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
        private TMP_Text baseValueText;

        [SerializeField]
        private Image background;

        [Header("Display Colors")]
        [SerializeField]
        private Color normalColor = Color.white;

        [SerializeField]
        private Color selectedColor =
            new(1f, 0.82f, 0.25f, 1f);

        [SerializeField]
        private Color countdownReadyColor =
            new(1f, 0.85f, 0.2f, 1f);

        private PointCardInstance cardData;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas rootCanvas;

        private CardSlotView currentSlot;
        private CardSlotView dragOriginSlot;

        private bool isInteractionEnabled = true;
        private bool isDragging;
        private bool isSelected;
        private bool isCombatDisplay;

        public PointCardInstance CardData => cardData;

        public RectTransform RectTransform => rectTransform;

        public CardSlotView CurrentSlot => currentSlot;

        public bool IsInteractionEnabled =>
            isInteractionEnabled;

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
                TMP_Text[] texts =
                    GetComponentsInChildren<TMP_Text>(true);

                if (texts.Length > 0)
                {
                    valueText = texts[0];
                }
            }

            Canvas parentCanvas =
                GetComponentInParent<Canvas>();

            if (parentCanvas != null)
            {
                rootCanvas = parentCanvas.rootCanvas;
            }

            ApplyBackgroundColor();
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
            ShowConfigurationValue();
        }

        public void SetCurrentSlot(CardSlotView slot)
        {
            currentSlot = slot;
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            ApplyBackgroundColor();
        }

        public void SetInteractionEnabled(bool enabled)
        {
            isInteractionEnabled = enabled;

            if (!enabled && isDragging)
            {
                ReturnToOriginSlot();
            }

            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 配置階段顯示：只顯示卡片原始點數。
        /// </summary>
        public void ShowConfigurationValue()
        {
            isCombatDisplay = false;

            if (valueText != null && cardData != null)
            {
                valueText.text =
                    cardData.Value.ToString();
            }

            if (baseValueText != null)
            {
                baseValueText.gameObject.SetActive(false);
            }

            ApplyBackgroundColor();
        }

        /// <summary>
        /// 戰鬥階段顯示目前倒數值及原始點數。
        /// </summary>
        public void ShowCombatCountdown(int currentValue)
        {
            isCombatDisplay = true;

            if (valueText != null)
            {
                valueText.text =
                    currentValue.ToString();
            }

            if (baseValueText != null && cardData != null)
            {
                baseValueText.gameObject.SetActive(true);
                baseValueText.text =
                    cardData.Value.ToString();
            }

            bool isReady = currentValue == 1;
            ApplyBackgroundColor(isReady);
        }

        public void OnPointerClick(
            PointerEventData eventData)
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

        public void OnBeginDrag(
            PointerEventData eventData)
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
                rectTransform.localPosition =
                    localPoint;
            }
        }

        public void OnEndDrag(
            PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }

            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

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

        private void ApplyBackgroundColor(
            bool countdownReady = false)
        {
            if (background == null)
            {
                return;
            }

            if (isSelected && !isCombatDisplay)
            {
                background.color = selectedColor;
                return;
            }

            background.color = countdownReady
                ? countdownReadyColor
                : normalColor;
        }
    }
}