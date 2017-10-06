using UnityEngine;
using System.Collections;

namespace ReveloLibrary
{
    [RequireComponent(typeof(Animator))]
    public class UIScreen : MonoBehaviour
    {
        public bool AlwaysActive;
        public ScreenDefinitions screenDefinition;

        public delegate void Callback();

        private bool ScreenOpened;
        private bool ScreenClosed;

        private CanvasGroup canvasGroup;
        private GameObject ScreenWrapper;

        private Callback callbackOnOpen;
        private Callback callbackOnClose;

        protected Animator animator;

        public virtual void Awake()
        {
            animator = GetComponent<Animator>();
            canvasGroup = gameObject.GetComponentInChildren<CanvasGroup>();
			
            RectTransform rect = GetComponent<RectTransform>();
            rect.offsetMax = rect.offsetMin = new Vector2(0, 0);
            ScreenWrapper = transform.GetChild(0).gameObject;

            if (!AlwaysActive)
            {
                ScreenWrapper.SetActive(false);
            }
        }

        public virtual void OpenWindow(Callback openCallback = null)
        {
            callbackOnOpen = openCallback;

            if (!AlwaysActive)
            {
                ScreenWrapper.SetActive(true);
            }

            IsOpen = true;
        }

        public virtual void CloseWindow(Callback closeCallback = null)
        {
            callbackOnClose = closeCallback;
            IsOpen = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public bool IsOpen
        {
            get
            { 
                return Animator.GetBool("IsOpen"); 
            }
            set
            {
                if (Animator != null)
                {
                    Animator.SetBool("IsOpen", value);
                    ScreenOpened = false;
                    ScreenClosed = false;
                }
            }
        }

        public Animator Animator
        {
            get
            {
                if (animator == null)
                {
                    animator = GetComponent<Animator>();
                }
                return animator;
            }
        }

        public bool InOpenState
        {
            get
            {
                return ScreenOpened;
            }
        }

        void OnEndAnimationOpen()
        {
            if (callbackOnOpen != null)
            {
                callbackOnOpen();
            }

            ScreenOpened = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public bool InCloseState
        {
            get
            {
                return ScreenClosed;
            }
        }

        void OnEndAnimationClose()
        {
            if (!AlwaysActive)
            {
                ScreenWrapper.SetActive(false);
            }

            if (callbackOnClose != null)
            {
                callbackOnClose();
            }
			
            ScreenClosed = true;
        }
    }
}
