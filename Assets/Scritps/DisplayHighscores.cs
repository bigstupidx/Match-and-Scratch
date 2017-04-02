using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayHighscores : MonoBehaviour {

	public GameObject scoreElement;

	public Transform dailyHighscoresContent;
	List<GameObject> dailyScoreElementsList = new List<GameObject>();

	public Transform weeklyHighscoresContent;
	List<GameObject> weeklyScoreElementsList = new List<GameObject>();

	public Transform allTimesHighscoresContent;
	List<GameObject> allTimesScoreElementsList = new List<GameObject>();

	// Use this for initialization
	void Start () {
		HighScores.OnHighscoresUpdate += UpdateHighscores;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void UpdateHighscores(List<Highscore> list) {

		CleanHighscoreElementsList (allTimesScoreElementsList);
		UpdateHighscoreList (list, allTimesScoreElementsList, allTimesHighscoresContent);

		List<Highscore> todayList = list.FindAll (s => s.date.Day == DateTime.Now.Day);
		CleanHighscoreElementsList (dailyScoreElementsList);
		UpdateHighscoreList (todayList, dailyScoreElementsList, dailyHighscoresContent);

	}

	void CleanHighscoreElementsList(List<GameObject> list) {
		foreach (GameObject item in list) {
			DestroyImmediate (item);
		}
		list.Clear();
	}

	void UpdateHighscoreList(List<Highscore> elements, List<GameObject> goList, Transform parent) {
		for (int i = 0; i < elements.GetRange( 0, Mathf.Min( elements.Count, 10 ) ).Count; i++) {
			GameObject g = Instantiate (scoreElement, parent, false);
			g.GetComponent<ScoreElement> ().SetScore (elements [i].username, elements [i].score.ToString());
			goList.Add (g);
		}
	}
}
