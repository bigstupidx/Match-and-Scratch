using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public struct Highscore 
{
	public string username;
	public int score;
	public DateTime date;

	public Highscore(string _username, int _score, DateTime _date) {
		username = _username;
		score = _score;
		date = _date;
	}
}

public class HighScores : MonoBehaviour
{

	const string privateCode= "J3duL1tLhUSESgpxL-tbHwuoqEW8TI9k-eS_5UfeoOhA";
	const string publicCode	= "58da1c3c12e7f00688faab7b";
	const string webURL		= "http://dreamlo.com/lb/";

	public List<Highscore> highscoreList = new List<Highscore> ();

	public static HighScores instance;

	public delegate void OnHighscoresUpdateEventHandler(List<Highscore> HighscoreList);

	public static event OnHighscoresUpdateEventHandler OnHighscoresUpdate;

	void Awake() {
		if (instance == null) {
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}
		/*
		AddNewHighscore ("user1", 50);
		AddNewHighscore ("user2", 100);
		AddNewHighscore ("user3", 5);
		*/
	}

	public void AddNewHighscore(string username, int score) {
		StartCoroutine(UploadNewHighscore(username, score));
	}

	IEnumerator UploadNewHighscore(string username, int score) {
		WWW www = new WWW (webURL + privateCode + "/add/" + WWW.EscapeURL (username) + "/" + score);
		yield return www;

		if (string.IsNullOrEmpty (www.error))
			Debug.Log("La puntuación se envió correctamente");
		else
			Debug.Log("Hubo un error durante la subida de puntuación: " + www.error);
	}

	public void DownloadHighscores() {
		StartCoroutine (GetHighscores());
	}

	IEnumerator GetHighscores() {
		WWW www = new WWW (webURL + publicCode + "/pipe/");
		yield return www;

		if (string.IsNullOrEmpty (www.error)) {
			FormatHighscores (www.text);
		}
		else
			Debug.Log("Hubo un error durante la obtención de puntuaciones: " + www.error);
	}

	void FormatHighscores(string textSteam) {
		string[] entries = textSteam.Split (new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

		highscoreList.Clear();

		for (int i = 0; i < entries.Length ; i++) {
			string[] entryInfo = entries [i].Split (new char[] { '|' });
			string username = entryInfo [0];
			int score = int.Parse(entryInfo [1]);
			DateTime date = DateTime.Parse(entryInfo [4]);
			highscoreList.Add(new Highscore(username, score, date));
			//Debug.Log (highscoreList [i].username + ": " + highscoreList [i].score + highscoreList [i].date);
		}
		if (OnHighscoresUpdate != null)
			OnHighscoresUpdate (highscoreList);
	}

	void Start() {
		
	}

	void Update() {
		
	}
}