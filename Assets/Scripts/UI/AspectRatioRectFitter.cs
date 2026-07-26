using UnityEngine;
using UnityEngine.UI;

namespace CountdownAutoBattle.UI
{
    /// <summary>
    /// 桌面寬型畫面中，將遊戲 UI 限制為指定比例。
    ///
    /// 在橫向限制模式下，Canvas Scaler 改為依高度縮放，
    /// 使 2:3 遊戲區維持穩定的 640×960 邏輯尺寸。
    ///
    /// 手機直式則保留原本 Canvas Scaler 設定與滿版行為。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class AspectRatioRectFitter : MonoBehaviour
    {
        [Header("Aspect Ratio")]
        [SerializeField, Min(0.01f)]
        [Tooltip("目標寬高比。640 / 960 = 2 / 3。")]
        private float targetAspect = 2f / 3f;

        [SerializeField]
        [Tooltip("是否也在直式畫面強制套用目標比例。本專案應保持關閉。")]
        private bool constrainPortraitScreens = false;

        [Header("Canvas Scaling")]
        [SerializeField]
        [Tooltip("橫向比例限制時，改為完全依高度縮放，以維持 640×960 邏輯尺寸。")]
        private bool controlCanvasScaler = true;

        private RectTransform rectTransform;
        private CanvasScaler canvasScaler;

        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        private float originalMatchWidthOrHeight = 0.5f;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            Canvas parentCanvas = GetComponentInParent<Canvas>();

            if (parentCanvas != null)
            {
                canvasScaler = parentCanvas.GetComponent<CanvasScaler>();
            }

            if (canvasScaler != null)
            {
                originalMatchWidthOrHeight =
                    canvasScaler.matchWidthOrHeight;
            }
        }

        private void OnEnable()
        {
            Apply();
        }

        private void OnDisable()
        {
            RestoreCanvasScaler();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            if (screenWidth == lastScreenWidth &&
                screenHeight == lastScreenHeight)
            {
                return;
            }

            Apply();
        }

        private void Apply()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            if (screenWidth <= 0 ||
                screenHeight <= 0 ||
                rectTransform == null)
            {
                return;
            }

            lastScreenWidth = screenWidth;
            lastScreenHeight = screenHeight;

            bool isPortrait = screenHeight >= screenWidth;

            /*
             * 手機直式：
             * 不套固定比例，並還原原本 Canvas Scaler Match。
             */
            if (isPortrait && !constrainPortraitScreens)
            {
                RestoreCanvasScaler();

                SetRect(
                    anchorMin: Vector2.zero,
                    anchorMax: Vector2.one);

                return;
            }

            /*
             * 桌面橫向：
             * 完全依高度縮放。
             *
             * 以 3840×2160 為例，限制成 2:3 後，
             * Unity 邏輯區域會維持約 640×960。
             */
            if (controlCanvasScaler && canvasScaler != null)
            {
                canvasScaler.matchWidthOrHeight = 1f;
            }

            float windowAspect =
                (float)screenWidth / screenHeight;

            float safeTargetAspect =
                Mathf.Max(0.01f, targetAspect);

            if (Mathf.Approximately(
                    windowAspect,
                    safeTargetAspect))
            {
                SetRect(
                    anchorMin: Vector2.zero,
                    anchorMax: Vector2.one);

                return;
            }

            if (windowAspect > safeTargetAspect)
            {
                // 畫面太寬：左右留黑。
                float normalizedWidth =
                    safeTargetAspect / windowAspect;

                float horizontalMargin =
                    (1f - normalizedWidth) * 0.5f;

                SetRect(
                    anchorMin:
                        new Vector2(horizontalMargin, 0f),

                    anchorMax:
                        new Vector2(
                            1f - horizontalMargin,
                            1f));
            }
            else
            {
                // 畫面太高：上下留黑。
                float normalizedHeight =
                    windowAspect / safeTargetAspect;

                float verticalMargin =
                    (1f - normalizedHeight) * 0.5f;

                SetRect(
                    anchorMin:
                        new Vector2(0f, verticalMargin),

                    anchorMax:
                        new Vector2(
                            1f,
                            1f - verticalMargin));
            }
        }

        private void RestoreCanvasScaler()
        {
            if (!controlCanvasScaler ||
                canvasScaler == null)
            {
                return;
            }

            canvasScaler.matchWidthOrHeight =
                originalMatchWidthOrHeight;
        }

        private void SetRect(
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;

            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Current Screen Size")]
        private void ApplyCurrentScreenSize()
        {
            Apply();
        }
#endif
    }
}