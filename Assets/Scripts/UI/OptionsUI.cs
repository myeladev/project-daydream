using System.Collections.Generic;
using ProjectDaydream.Logic;
using ProjectDaydream.Objects;
using UnityEngine;

namespace ProjectDaydream.UI
{
    public class OptionsUI : MonoBehaviour
    {
        public static OptionsUI Instance;
        public bool IsViewingOptions => Mathf.Approximately(canvasGroup.alpha, 1);
        private CanvasGroup canvasGroup;

        protected void Awake()
        {
            Instance = this;
            canvasGroup = GetComponent<CanvasGroup>();
            Close();
        }

        [SerializeField]
        private InteractOption interactOptionPrefab;
        private List<InteractOption> _options = new List<InteractOption>();
        private Interactable interactable;
        public void Refresh()
        {
            if (interactable is not null)
            {
                var interactOptions = interactable.GetInteractOptions(InteractContext.RightClick);

                foreach (var option in interactOptions)
                {
                    var newButton = Instantiate(interactOptionPrefab.gameObject, transform).GetComponent<InteractOption>();
                    newButton.SetOption(option);
                    _options.Add(newButton);
                }
            }
            else
            {
                Close();
            }
        }

        private void ClearOptions()
        {
            foreach (var option in _options)
            {
                Destroy(option.gameObject);
            }

            _options.Clear();
        }
        
        public void Open(Interactable target)
        {
            interactable = target;
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            Refresh();
        }

        public void ChooseOption(string option)
        {
            interactable.Interact(option, InteractContext.Default);
            Close();
        }

        private void Close()
        {
            interactable = null;
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            ClearOptions();
        }
    }
}
