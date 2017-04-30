using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements; 

public enum ButtonBehaviourType {
	HIDE_BUTTON,
	DISABLE_BUTTON,
	HIDE_BUTTON_ICON
}


public class AdsButtonWrapperController : MonoBehaviour {
	

	public ButtonBehaviourType behaviourType;
	public GameObject AdsButton;
	Button buttonComp;
	Image butonBg;

	public GameObject internalIcon;
	public bool continuousCheck;

	public void Awake() {
		butonBg = AdsButton.GetComponent<Image> ();
		buttonComp = AdsButton.GetComponent<Button> ();

		if (internalIcon == null)
			Debug.LogFormat ("No se ha establecido imagenInterna en boton {0}>{1}", transform.parent.name, transform.name);
	}
		
	public void OnEnable ()
	{	
		AdsAvailability ();
		/*
		if (GameManager.instance && !GameManager.instance.unityAds.IsReady && continuousCheck)
			StartCoroutine (WaitForAdsTobyReady ());
			*/
	}

	void Update() {
		if (continuousCheck) {
			AdsAvailability ();
		}
	}

	void AdsAvailability() {
		if (GameManager.instance) {
			switch (behaviourType) {
				case ButtonBehaviourType.DISABLE_BUTTON:
					buttonComp.interactable = GameManager.instance.unityAds.IsReady;
				break;
				case ButtonBehaviourType.HIDE_BUTTON:
					AdsButton.SetActive (GameManager.instance.unityAds.IsReady);
				break;
				case ButtonBehaviourType.HIDE_BUTTON_ICON:
					internalIcon.SetActive (GameManager.instance.unityAds.IsReady);
				break;
			}
		}
	}
	/*
	IEnumerator WaitForAdsTobyReady() {
		if (GameManager.instance) {
			while (!GameManager.instance.unityAds.IsReady)
				yield return new WaitForSeconds (2f);
			AdsAvailability ();
		}
	}
	*/
}
