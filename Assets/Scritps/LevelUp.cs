using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SmartLocalization;

public class LevelUp : MonoBehaviour {

	public GameObject MoreColors;
	public GameObject SpeedUp;
	public GameObject ReverseDirection;
	public GameObject ReverseDirectionCancel;
	public GameObject CrazySpeed;
	public GameObject CrazySpeedCancel;

	public Text texto; 

	public Animator animator;

	void Awake() {
		animator = GetComponent<Animator> ();
		HideAllIcons ();
	}

	public void Show(DifficultType difficult) {
		HideAllIcons ();

		switch (difficult) {
			case DifficultType.MORE_COLORS:
				MoreColors.SetActive (true);
				texto.text = LanguageManager.Instance.GetTextValue("game.difficulty.moreballs");
			break;
			case DifficultType.SPEEDUP:
				SpeedUp.SetActive (true);
				texto.text = LanguageManager.Instance.GetTextValue("game.difficulty.speedup");
			break;
			case DifficultType.SWITCH_REVERSE:
				ReverseDirection.SetActive (true);
				texto.text = LanguageManager.Instance.GetTextValue("game.difficulty.reversedir");
			break;
			case DifficultType.SWITCH_REVERSE_CANCEL:
				ReverseDirectionCancel.SetActive (true);
				texto.text = LanguageManager.Instance.GetTextValue("game.difficulty.reversedircancel");
			break;
			case DifficultType.SWITCH_CRAZY_SPEED:
				CrazySpeed.SetActive (true);
				texto.text = LanguageManager.Instance.GetTextValue("game.difficulty.crazyspeed");
			break;
			case DifficultType.SWITCH_CRAZY_SPEED_CANCEL:
				CrazySpeedCancel.SetActive (true);
				texto.text = LanguageManager.Instance.GetTextValue("game.difficulty.crazyspeedcancel");
			break;
		
		}

		animator.SetTrigger("start");
	}

	void HideAllIcons() {
		MoreColors.SetActive (false);
		SpeedUp.SetActive (false);
		ReverseDirection.SetActive (false);
		ReverseDirectionCancel.SetActive (false);
		CrazySpeed.SetActive (false);
		CrazySpeedCancel.SetActive (false);
	}
}
