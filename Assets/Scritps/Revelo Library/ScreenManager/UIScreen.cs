using UnityEngine;
using System.Collections;

namespace ReveloLibrary {
	
	[RequireComponent(typeof(Animator))]
	public class UIScreen : MonoBehaviour {
		public bool AlwaysActive;
		public ScreenDefinitions screenDefinition;

		private CanvasGroup _canvasGroup;
		protected Animator _animator;

		GameObject ScreenWrapper;

		public delegate void Callback();
		Callback callbackOnOpen;
		Callback callbackOnClose;

		bool ScreenOpened;
		bool ScreenClosed;

		public virtual void Awake()
		{
			_animator = GetComponent<Animator>();
			_canvasGroup = gameObject.GetComponentInChildren<CanvasGroup> ();
			
			RectTransform rect = GetComponent<RectTransform>();
			rect.offsetMax = rect.offsetMin = new Vector2(0, 0);
			ScreenWrapper = transform.Find ("Wrapper").gameObject;
			if (!AlwaysActive) ScreenWrapper.SetActive (false);
		}
			
		public virtual void OpenWindow(Callback openCallback = null) {
			callbackOnOpen = openCallback;

			if (!AlwaysActive) 
				ScreenWrapper.SetActive (true);

			//StartCoroutine(WaitForScreenOpen());
			IsOpen = true;
		}

		public virtual void CloseWindow(Callback closeCallback = null) {
			callbackOnClose = closeCallback;
			//StartCoroutine (WaitForScreenClose ());
			IsOpen = false;
			_canvasGroup.blocksRaycasts = false;
			_canvasGroup.interactable = false;
		}

		public bool IsOpen
		{
			get { return Animator.GetBool("IsOpen"); }
			set {
				if (Animator != null) {
					Animator.SetBool("IsOpen", value);
					ScreenOpened = false;
					ScreenClosed = false;
				}
			}
		}

		public Animator Animator {
			get {
				if (_animator == null) {
					_animator = GetComponent<Animator>();
				}
				return _animator;
			}
		}

		public bool InOpenState {
			get {
				return ScreenOpened;
			}
		}

		void OnEndAnimationOpen() {
			if (callbackOnOpen != null) 
				callbackOnOpen();

			ScreenOpened = true;
			_canvasGroup.blocksRaycasts = true;
			_canvasGroup.interactable = true;
		}

		public bool InCloseState {
			get {
				return ScreenClosed;
			}
		}

		void OnEndAnimationClose() {
			if (!AlwaysActive)
				ScreenWrapper.SetActive (false);

			if (callbackOnClose != null) 
				callbackOnClose();
			
			ScreenClosed = true;
		}



		/*
		IEnumerator WaitForScreenOpen() {
			while (!InOpenState)
				yield return null;
			
			if (callbackOnOpen != null) 
				callbackOnOpen();

			Debug.Log ("Ventana " + name + " Abierta por completo");
		}

		IEnumerator WaitForScreenClose() {
			while (!InCloseState)
				yield return null;
				
			if (callbackOnClose != null) 
				callbackOnClose();

			Debug.Log ("Ventana " + name + " Cerrada por completo");
		}
		*/
	}
}
