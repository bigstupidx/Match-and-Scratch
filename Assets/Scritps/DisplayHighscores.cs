using System;
using System.Globalization;
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

		// Alltimes
		CleanHighscoreElementsList (allTimesScoreElementsList);
		UpdateHighscoreList (list, allTimesScoreElementsList, allTimesHighscoresContent);

		// Daily
		DateTime utcDate = DateTime.UtcNow;
		List<Highscore> todayList = list.FindAll (s => s.date.Day == utcDate.Day);
		CleanHighscoreElementsList (dailyScoreElementsList);
		UpdateHighscoreList (todayList, dailyScoreElementsList, dailyHighscoresContent);

		// Weekly
		int utcYear = DateTime.UtcNow.Year;
		DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
		Calendar cal = dfi.Calendar;
		List<Highscore> weekList = list.FindAll (s => 	s.date.Year == utcDate.Year && cal.GetWeekOfYear(s.date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek) ==  cal.GetWeekOfYear(utcDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek));
		CleanHighscoreElementsList (weeklyScoreElementsList);
		UpdateHighscoreList (weekList, weeklyScoreElementsList, weeklyHighscoresContent);

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
			g.GetComponent<ScoreElement> ().SetScore (elements [i].username.Split(new char[] {'-'})[0], elements [i].score.ToString());
			goList.Add (g);
		}
	}
}
