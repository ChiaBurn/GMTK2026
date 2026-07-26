using UnityEngine;

namespace CountdownAutoBattle.UI
{
    /// <summary>
    /// 在寬型畫面中，將 UI 遊戲區限制於指定長寬比。
    ///
    /// 本專案的設計解析度為 640 × 960，因此目標比例為 2:3。
    /// 手機直式預設不限制比例，以保留既有的滿版顯示行為。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class AspectRatioRectFitter : MonoBehaviour
    {
        [Header("Aspect Ratio")]
        [SerializeField]
        [Min(0.01f)]
        [Tooltip("目標寬高比。640 / 960 = 2 / 3。")]
        private float targetAspect = 2f / 3f;

        [SerializeField]
        [Tooltip("是否也在直式畫面強制套用目標比例。本專案建議保持關閉。")]
        private bool constrainPortraitScreens = false;

        private RectTransform rectTransform;

        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            Apply();
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
             * 手機直式沿用完整可用畫面，不強制限制成 2:3。
             * 這能保留目前三星手機上已驗證正常的顯示行為。
             */
            if (isPortrait && !constrainPortraitScreens)
            {
                SetRect(
                    anchorMin: Vector2.zero,
                    anchorMax: Vector2.one);

                return;
            }

            float windowAspect = (float)screenWidth / screenHeight;
            float safeTargetAspect = Mathf.Max(0.01f, targetAspect);

            if (Mathf.Approximately(windowAspect, safeTargetAspect))
            {
                SetRect(
                    anchorMin: Vector2.zero,
                    anchorMax: Vector2.one);

                return;
            }

            if (windowAspect > safeTargetAspect)
            {
                /*
                 * 畫面比遊戲寬：
                 * 限制內容寬度，左右顯示 LetterboxBackground。
                 */
                float normalizedWidth = safeTargetAspect / windowAspect;
                float horizontalMargin = (1f - normalizedWidth) * 0.5f;

                SetRect(
                    anchorMin: new Vector2(horizontalMargin, 0f),
                    anchorMax: new Vector2(1f - horizontalMargin, 1f));
            }
            else
            {
                /*
                 * 畫面比遊戲高：
                 * 限制內容高度，上下顯示 LetterboxBackground。
                 *
                 * 此分支主要處理 constrainPortraitScreens 開啟，
                 * 或其他特殊窄比例環境。
                 */
                float normalizedHeight = windowAspect / safeTargetAspect;
                float verticalMargin = (1f - normalizedHeight) * 0.5f;

                SetRect(
                    anchorMin: new Vector2(0f, verticalMargin),
                    anchorMax: new Vector2(1f, 1f - verticalMargin));
            }
        }

        private void SetRect(Vector2 anchorMin, Vector2 anchorMax)
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