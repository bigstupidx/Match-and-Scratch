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

	public ScoreEntry(string _username, int _score, double _date = 0) {
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


public class DisplayHighscores : MonoBehaviour {

	public GameObject scoreElement;
	public GameObject loadingText;

	DateTimeFormatInfo dfi;
	Calendar cal;

	public Transform dailyHighscoresContent;
	List<GameObject> dailyScoreElementsList = new List<GameObject>();

	public Transform weeklyHighscoresContent;
	List<GameObject> weeklyScoreElementsList = new List<GameObject>();

	public Transform allTimesHighscoresContent;
	List<GameObject> allTimesScoreElementsList = new List<GameObject>();

	void Awake(){
		CultureInfo _culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
		CultureInfo _uiculture = (CultureInfo)CultureInfo.CurrentUICulture.Clone();

		_culture.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
		_uiculture.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;

		System.Threading.Thread.CurrentThread.CurrentCulture = _culture;
		System.Threading.Thread.CurrentThread.CurrentUICulture = _uiculture;

		dfi = DateTimeFormatInfo.CurrentInfo;
		cal = dfi.Calendar;
	} 

	// Use this for initialization
	void OnEnable () {
		UpdateHighscores (FirebaseDBManager.instance.highscoreList);
		FirebaseDBManager.instance.Firebase_OnHighscoresUpdate += UpdateHighscores;
	}


	void OnDisable (){
		FirebaseDBManager.instance.Firebase_OnHighscoresUpdate -= UpdateHighscores;
	}

	void RemoveHandles() {
		/*
		if (Dreamlo_HighScores.instance.Dreamlo_OnHighscoresUpdate != null)
			Dreamlo_HighScores.instance.Dreamlo_OnHighscoresUpdate -= UpdateHighscores;
		*/
		if (FirebaseDBManager.instance.Firebase_OnHighscoresUpdate != null)
			FirebaseDBManager.instance.Firebase_OnHighscoresUpdate -= UpdateHighscores;
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
		List<ScoreEntry> todayList = list.FindAll (s => DateTime.FromOADate(s.date).Date == utcDate.Date);
		CleanHighscoreElementsList (dailyScoreElementsList);
		UpdateHighscoreList (todayList, dailyScoreElementsList, dailyHighscoresContent);

		// Weekly
		int utcYear = DateTime.UtcNow.Year;
		List<ScoreEntry> weekList = list.FindAll (entry => 	DateTime.FromOADate(entry.date).Year == utcDate.Year && cal.GetWeekOfYear(DateTime.FromOADate(entry.date), dfi.CalendarWeekRule, dfi.FirstDayOfWeek) ==  cal.GetWeekOfYear(utcDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek));
		
		CleanHighscoreElementsList (weeklyScoreElementsList);
		UpdateHighscoreList (weekList, weeklyScoreElementsList, weeklyHighscoresContent);

		loadingText.SetActive (list.Count == 0);
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
			g.GetComponent<ScoreElement> ().SetScore (elements [i].username, elements [i].score.ToString());	
			goList.Add (g);
		}
	}
}
