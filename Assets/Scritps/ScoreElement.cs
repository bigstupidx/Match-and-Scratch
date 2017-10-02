using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreElement : MonoBehaviour
{
    Text username;
    Text score;

    void Awake()
    {
        username = transform.Find("Name").GetComponent<Text>();
        score = transform.Find("Score").GetComponent<Text>();
    }

    public void SetScore(string theName, string theScore)
    {
        username.text = theName;
        score.text = theScore;
    }
}
