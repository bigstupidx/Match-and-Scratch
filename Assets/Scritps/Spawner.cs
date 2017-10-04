using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    public const float MINIMUM_SPAWN_TIME = 0f;
    public const int MAX_COLORS_IN_GAME = 8;
	 
    public GameObject PinPrefab;
    public Image nextPin;
    public int nextColor;
    public int currentColor;
    public int colorsInGame;

    private int preInstantiatedPins;

    public Pin lastSpawnedPin;
    private List<Pin> pinsPool;

    public int pinsCount
    {
        get;
        set;
    }

    void Start() {
    }

    void Update() {
        #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameManager.Instance.spawner.ThrowCurrentPin();
            }
        #endif
    }

    public void SpawnPin(float secondsDelay = 0)
    {
        StartCoroutine(Spawn(secondsDelay));
    }

    public void AddColorsInGame(int inc)
    {
        colorsInGame = Mathf.Min(colorsInGame + inc, MAX_COLORS_IN_GAME);
    }
    
    public void Restart()
    {
        preInstantiatedPins = 15;
        colorsInGame = 1;
        // If the Pins are already generated then hide and set them as availables
        if (pinsPool != null)
        {
            HidePins();
        }
        GeneratePinsPool();
        SetNextColor();
        SpawnPin();
    }

    public void Finish() {
        if (pinsPool != null)
        {
            pinsPool.ForEach(p => Destroy(p.gameObject));
            pinsPool.Clear();
        }
    }

    void GeneratePinsPool()
    {
        if (pinsPool == null)
        {
            pinsPool = new List<Pin>();
        }
       /* else
        {
            pinsPool.ForEach(p => Destroy(p.gameObject));
            pinsPool.Clear();
        }
        */


        for (int i = pinsPool.Count; i < preInstantiatedPins; i++)
        {
            pinsPool.Add(CreateNewPin());
        }
    }

    Pin CreateNewPin() {
        GameObject ball = Instantiate(PinPrefab);

        Pin pin = ball.GetComponent<Pin>();
        pin.SetAvailable();
        pinsPool.Add(pin);

        return pin;
    }

    void HidePins()
    {
        pinsPool.ForEach(p => p.SetAvailable());
    }

    void SetNextColor()
    {
        nextColor = Random.Range(0, Mathf.Min(Mathf.Max(0, colorsInGame), GameManager.Instance.posibleColors.Length));
        nextPin.color = GameManager.Instance.posibleColors[nextColor];
    }

    private IEnumerator Spawn(float secondsDelay = 0f)
    {
        yield return new WaitForSeconds(secondsDelay);
        lastSpawnedPin = GetAvailablePin();
        SetNextColor();

    }

    Pin GetAvailablePin()
    {
        Pin available = pinsPool.Find(p => !p.IsAlive);
        if (available == null)
        {
            available = CreateNewPin();
            //preInstantiatedPins = pinsPool.Count;
        }
        currentColor = nextColor;
        string name = pinsCount + "-Type_" + currentColor.ToString();
        available.Setup(transform.position, nextColor, name);

        return available;
    }

    public void ThrowCurrentPin()
    {
        if (GameManager.Instance.currentGamePlayState == GamePlayState.NORMAL && lastSpawnedPin != null)
        {
            lastSpawnedPin.Shoot();
        }
    }
}