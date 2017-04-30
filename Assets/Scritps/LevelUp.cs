using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUp : MonoBehaviour {

	public GameObject MoreColors;
	public GameObject SpeedUp;
	public GameObject ReverseDirection;

	Animator animator;

	void Awake() {
		animator = GetComponent<Animator> ();
		HideAllIcons ();
	}

	public void Show(DifficultType difficult) {
		HideAllIcons ();

		switch (difficult) {
		case DifficultType.MORE_COLORS:
			MoreColors.SetActive (true);
			break;
		case DifficultType.SPEEDUP:
			SpeedUp.SetActive (true);
			break;
		case DifficultType.REVERSE_ENABLED:
			ReverseDirection.SetActive (true);
			break;
		}
		animator.SetTrigger("start");
	}

	void HideAllIcons() {
		MoreColors.SetActive (false);
		SpeedUp.SetActive (false);
		ReverseDirection.SetActive (false);
	}
}
