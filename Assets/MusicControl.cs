using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReveloLibrary;

public class MusicControl : MonoBehaviour {

	enum SoundState {
		On,
		Off
	}
	SoundState currentState;
	public Sprite IconOn;
	public Sprite IconOff;

	Image icon;

	void Awake() {
		icon = GetComponent<Image> ();
	}

	void OnEnable() {
		SetSoundState(PlayerPrefs.GetInt("soundState", 1) == 1 ? true : false);
	}

	public void SwitchMusicOnOff () {
		SetSoundState (currentState == SoundState.On);
	}

	void SetSoundState (bool isEnabled) {
		currentState = isEnabled ? SoundState.Off : SoundState.On;
		icon.sprite = isEnabled ? IconOn : IconOff;
		if (AudioMaster.instance)
			AudioMaster.instance.Mute (!isEnabled);
		PlayerPrefs.SetInt("soundState", isEnabled ? 1 : 0);
	}
}
