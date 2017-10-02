using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReveloLibrary;

public class MusicControl : MonoBehaviour
{
    public Image icon;
    public Sprite IconOn;
    public Sprite IconOff;

    enum SoundState
    {
        ON,
        OFF
    }

    private SoundState currentState;

    void OnEnable()
    {
        SetSoundState(PlayerPrefs.GetInt("soundState", 1) == 1 ? true : false);
    }

    public void SwitchMusicOnOff()
    {
        SetSoundState(currentState == SoundState.ON);
    }

    void SetSoundState(bool isEnabled)
    {
        currentState = isEnabled ? SoundState.OFF : SoundState.ON;
        icon.sprite = isEnabled ? IconOn : IconOff;

        if (AudioMaster.instance)
        {
            AudioMaster.instance.Mute(!isEnabled);
        }

        PlayerPrefs.SetInt("soundState", isEnabled ? 1 : 0);
    }
}