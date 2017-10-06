using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreElement : MonoBehaviour
{
    public Text username;
    public Text score;

    void Start()
    {
    }

    public void SetScore(string theName, string theScore)
    {
        username.text = theName;
        score.text = theScore;
    }
}
