using ProjectDaydream.Logic;
using ProjectDaydream.Common;
using TMPro;
using UnityEngine;

namespace ProjectDaydream.UI
{
    public class SleepUIManager : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI timeText;
        [SerializeField]
        private DayNightManager dayNightManager;
        [SerializeField]
        private RectTransform sleepBackgroundBar;
        [SerializeField]
        private RectTransform sleepFillBar;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            _canvasGroup.alpha = false /*TODO:PlayerController.instance.isSleeping*/ ? 1f : 0f;
            //timeText.text = $"[{weatherManager.GetFriendlyTimeString()}]";
            sleepFillBar.UpdateBar(100/*TODO:PlayerController.instance.sleep*/, 100, sleepBackgroundBar);
        }
    }
}
