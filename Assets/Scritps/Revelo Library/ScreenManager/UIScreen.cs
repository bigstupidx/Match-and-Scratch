using UnityEngine;
using System.Collections;

namespace ReveloLibrary {
	
	[RequireComponent(typeof(Animator))]
	public class UIScreen : MonoBehaviour {

		public ScreenDefinitions screenDefinition;

		private CanvasGroup _canvasGroup;
		protected Animator _animator;

		public virtual void Awake()
		{
			_animator = GetComponent<Animator>();
			_canvasGroup = GetComponent<CanvasGroup>();
			
			RectTransform rect = GetComponent<RectTransform>();
			rect.offsetMax = rect.offsetMin = new Vector2(0, 0);
		}

		public virtual void Start()	{
		}

		public virtual void UpdateTitle(){
		}

		public virtual void AnimEvent_PrepareModal(){
		}

		public virtual void Update()
		{
			// Aseguramos que si una pantalla no está activada, no es interactuable.
	        if (Animator != null) {

				_canvasGroup.blocksRaycasts = Animator.GetCurrentAnimatorStateInfo (0).IsName ("Open");
				_canvasGroup.interactable = Animator.GetCurrentAnimatorStateInfo (0).IsName ("Open");
			}
		}

		public virtual void OpenWindow() {
		}

		public virtual void CloseWindow() {
		}

		public bool IsOpen
		{
			get { return Animator.GetBool("IsOpen"); }
			set {
				if (Animator != null) {
					Animator.SetBool("IsOpen", value);
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
				return Animator.GetCurrentAnimatorStateInfo(0).IsName("Open");
			}
		}

		public bool InCloseState {
			get {
				return Animator.GetCurrentAnimatorStateInfo(0).IsName("Close");
			}
		}

	}
}
