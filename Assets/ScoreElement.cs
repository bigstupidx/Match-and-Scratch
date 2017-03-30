using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreElement : MonoBehaviour {


	Text name;
	Text score;

	// Use this for initialization
	void Awake () {
		name = transform.Find ("Name").GetComponent<Text> ();
		score = transform.Find ("Score").GetComponent<Text> ();
	}

	void SetScore(string theName, string theScore) {
		name.text = theName;
		score.text = theScore;
	}
}
