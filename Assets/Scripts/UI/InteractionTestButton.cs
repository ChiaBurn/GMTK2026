using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CountdownAutoBattle.UI
{
    /// <summary>
    /// 第一輪 WebGL 與行動裝置 Pointer Event 驗證。
    /// 點擊按鈕後更新文字，確認滑鼠與觸控事件均能抵達 Unity UI。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class InteractionTestButton : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text label;

        private Button button;
        private int clickCount;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (label == null)
            {
                label = GetComponentInChildren<TMP_Text>();
            }

            if (label == null)
            {
                Debug.LogError(
                    $"{nameof(InteractionTestButton)} requires a TMP_Text child.",
                    this);

                enabled = false;
                return;
            }

            button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            clickCount++;
            label.text = $"TOUCH OK: {clickCount}";

            Debug.Log($"Interaction test click count: {clickCount}");
        }
    }
}