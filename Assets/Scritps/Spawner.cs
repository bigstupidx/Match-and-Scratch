using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class Spawner : MonoBehaviour
{
    
    public const float MINIMUM_SPAWN_TIME = 0f;
    public const int MAX_COLORS_IN_GAME = 8;
	 
    public GameObject PinPrefab;
    public Image nextPin;
    public int nextColor;
    public int currentColor;
    public int colorsInGame;

    private int pinsCount;

    public GameObject lastSpawnedPin;

    public int PinsCount
    {
        get
        {
            return pinsCount;
        }
    }

    public void SpawnPin(float secondsDelay = 0)
    {
        StartCoroutine(Spawn(secondsDelay));
    }

    public void AddColorsInGame(int inc)
    {
        colorsInGame = Mathf.Min(colorsInGame + inc, MAX_COLORS_IN_GAME);
    }

    public void Reset()
    {
        pinsCount = 0;
        colorsInGame = 1;
        nextColor = GetNextColor();
        nextPin.color = GameManager.Instance.posibleColors[nextColor];
    }

    private IEnumerator Spawn(float secondsDelay = 0f)
    {

        yield return new WaitForSeconds(secondsDelay);

        currentColor = nextColor;
        lastSpawnedPin = Instantiate(PinPrefab, transform.position, transform.rotation);
        Pin pin = lastSpawnedPin.GetComponent<Pin>();
        pin.colorType = currentColor;
        pin.SetColor(GameManager.Instance.posibleColors[currentColor]);
        lastSpawnedPin.name = pinsCount + "-Type_" + currentColor.ToString();

        pinsCount++;

        nextColor = GetNextColor();
        nextPin.color = GameManager.Instance.posibleColors[nextColor];
    }

    int GetNextColor()
    {
        return Random.Range(0, Mathf.Min(Mathf.Max(0, colorsInGame), GameManager.Instance.posibleColors.Length));
    }

    public void ThrowCurrentPin()
    {
        if (GameManager.Instance.currentGamePlayState == GamePlayState.NORMAL && lastSpawnedPin != null)
        {
            lastSpawnedPin.GetComponent<Pin>().isShooted = true;
        }
    }
}