using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ScoreEntry 
{
	public string username;
	public int score;
	public double date;

	public ScoreEntry(string _username, int _score, double _date) {
		username = _username;
		score = _score;
		date = _date;
	}

	public string ToContatString() {
		return username + "|" + score + "|" + date.ToString();
	}

	public Dictionary<string, object> ToDictionary() { 
		Dictionary<string, object> result = new Dictionary<string, object> ();
		result ["username"] = username;
		result ["score"] = score;
		result ["date"] = date;

		return result;
	}
}

public enum HighScoresSource {
	DREAMLO,
	FIREBASE
}

public class DisplayHighscores : MonoBehaviour {

	public GameObject scoreElement;

	public HighScoresSource currentSource;

	public Transform dailyHighscoresContent;
	List<GameObject> dailyScoreElementsList = new List<GameObject>();

	public Transform weeklyHighscoresContent;
	List<GameObject> weeklyScoreElementsList = new List<GameObject>();

	public Transform allTimesHighscoresContent;
	List<GameObject> allTimesScoreElementsList = new List<GameObject>();

	// Use this for initialization
	void OnEnable () {		
		SetSource ((int)currentSource);
	}

	public void SetSource (int source) {
		if (source != (int)currentSource) {
			
			RemoveHandles ();
			if (source == (int)HighScoresSource.DREAMLO) {
				UpdateHighscores (Dreamlo_HighScores.instance.highscoreList);
				Dreamlo_HighScores.instance.Dreamlo_OnHighscoresUpdate += UpdateHighscores;
			} else if (source == (int)HighScoresSource.FIREBASE) {
				UpdateHighscores (Firebase_HighScores.instance.highscoreList);
				Firebase_HighScores.instance.Firebase_OnHighscoresUpdate += UpdateHighscores;
			}
			currentSource = (HighScoresSource)source;
		}
	}

	void OnDisable (){
		RemoveHandles ();
	}

	void RemoveHandles() {

		if (Dreamlo_HighScores.instance.Dreamlo_OnHighscoresUpdate != null)
			Dreamlo_HighScores.instance.Dreamlo_OnHighscoresUpdate -= UpdateHighscores;
		
		if (Firebase_HighScores.instance.Firebase_OnHighscoresUpdate != null)
			Firebase_HighScores.instance.Firebase_OnHighscoresUpdate -= UpdateHighscores;
	}

	/// <summary>
	/// Handles update highscores from Dream.lo
	/// </summary>
	/// <param name="list">List.</param>
	void UpdateHighscores(List<ScoreEntry> list) {
		// Alltimes
		CleanHighscoreElementsList (allTimesScoreElementsList);
		UpdateHighscoreList (list, allTimesScoreElementsList, allTimesHighscoresContent);

		// Daily
		DateTime utcDate = DateTime.UtcNow;
		List<ScoreEntry> todayList = list.FindAll (s => DateTime.FromOADate(s.date).Day == utcDate.Day);
		
		CleanHighscoreElementsList (dailyScoreElementsList);
		UpdateHighscoreList (todayList, dailyScoreElementsList, dailyHighscoresContent);

		// Weekly
		int utcYear = DateTime.UtcNow.Year;
		DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
		Calendar cal = dfi.Calendar;
		List<ScoreEntry> weekList = list.FindAll (entry => 	DateTime.FromOADate(entry.date).Year == utcDate.Year && cal.GetWeekOfYear(DateTime.FromOADate(entry.date), dfi.CalendarWeekRule, dfi.FirstDayOfWeek) ==  cal.GetWeekOfYear(utcDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek));
		
		CleanHighscoreElementsList (weeklyScoreElementsList);
		UpdateHighscoreList (weekList, weeklyScoreElementsList, weeklyHighscoresContent);
	}

	void CleanHighscoreElementsList(List<GameObject> list) {
		foreach (GameObject item in list) {
			DestroyImmediate (item);
		}
		list.Clear();
	}

	void UpdateHighscoreList(List<ScoreEntry> elements, List<GameObject> goList, Transform parent) {
		for (int i = 0; i < elements.GetRange( 0, Mathf.Min( elements.Count, 10 ) ).Count; i++) {
			GameObject g = Instantiate (scoreElement, parent, false);
			if (currentSource == HighScoresSource.DREAMLO)
				g.GetComponent<ScoreElement> ().SetScore (elements [i].username.Split(new char[] {'-'})[0], elements [i].score.ToString());
			else if (currentSource == HighScoresSource.FIREBASE)
				g.GetComponent<ScoreElement> ().SetScore (elements [i].username, elements [i].score.ToString());	
			goList.Add (g);
		}
	}
}
