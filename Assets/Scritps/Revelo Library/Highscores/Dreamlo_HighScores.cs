using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
///  Highscores powered by: dreamlo.com
///  http://dreamlo.com/lb/J3duL1tLhUSESgpxL-tbHwuoqEW8TI9k-eS_5UfeoOhA
/// </summary>
public class Dreamlo_HighScores : MonoBehaviour
{

	const string privateCode= "J3duL1tLhUSESgpxL-tbHwuoqEW8TI9k-eS_5UfeoOhA";
	const string publicCode	= "58da1c3c12e7f00688faab7b";
	const string webURL		= "http://dreamlo.com/lb/";

	public List<ScoreEntry> highscoreList = new List<ScoreEntry> ();

	public Action<List<ScoreEntry>> Dreamlo_OnHighscoresUpdate;

	public static Dreamlo_HighScores instance;

	List<ScoreEntry> undeliveredHighScores = new List<ScoreEntry> ();

	void Awake() {
		if (instance == null) {
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}
	}

	void Start() {
		ReadUndeliveredScores ();
		StartCoroutine (CheckInternetConnection());
	}

	void ReadUndeliveredScores() {
		string[] undeliveredScoresStream = PlayerPrefs.GetString("undeliveredScores","").Split (new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
		undeliveredHighScores.Clear ();

		for (int i = 0; i < undeliveredScoresStream.Length; i++) {			
			string[] entryInfo = undeliveredScoresStream [i].Split (new char[] { '|' });
			string username = entryInfo [0];
			int score = int.Parse(entryInfo [1]);
			double date = DateTime.Parse(entryInfo [2].parseToDateTime()).ToOADate();
		
			undeliveredHighScores.Add( new ScoreEntry(username, score, date) );
		}
		PlayerPrefs.DeleteKey ("undeliveredScores");
	}

	public void AddNewHighscore(ScoreEntry sc) {
		StartCoroutine(UploadNewHighscore(sc.username, sc.score, sc.date == 0 ? "": DateTime.FromOADate(sc.date).ToString()));
	}

	IEnumerator CheckInternetConnection() {
		// Source: http://answers.unity3d.com/questions/567497/how-to-100-check-internet-availability.html?childToView=744803#answer-744803
		Debug.Log ("<color=white>Checking internet connection...</color>");
		WWW www = new WWW("http://dreamlo.com/");
		yield return www;
		if ( string.IsNullOrEmpty (www.error) ) {
			StartCoroutine( OnInternetConnection (true) );
		} else {
			StartCoroutine( OnInternetConnection (false) );
		}
	}

	IEnumerator OnInternetConnection(bool result) {
		if (result) {
			Debug.Log ("<color=white>... Internet conection available</color>");
			if (undeliveredHighScores.Count > 0)
				SendUndeliveredScores ();
		} else {
			Debug.Log ("<color=white>... Internet connections not available</color>");
			yield return new WaitForSeconds (2f);
			StartCoroutine (CheckInternetConnection());
		}
	}

	void SaveUndeliveredScore (ScoreEntry s) {
		if (!undeliveredHighScores.Contains (s)) {
			undeliveredHighScores.Add (s);
			PlayerPrefs.SetString ( "undeliveredScores", undeliveredHighScores.ToSingleString() );
			Debug.Log ("<color=white>Puntuación guardada en local</color>");
		}
		StartCoroutine (CheckInternetConnection());
	}

	void SendUndeliveredScores () {
		Debug.Log ("<color=white>Enviando puntuaciones guardadas en local</color>");
		foreach (ScoreEntry h in undeliveredHighScores) {
			StartCoroutine ( UploadNewHighscore ( h.username, h.score, DateTime.FromOADate(h.date).ToUniversalTime().ToString().Replace (":", "").Replace ("/", "").Replace (" ", "") ) );
		}
	}

	IEnumerator UploadNewHighscore (string username, int score, string date = "") {
		//Debug.Log ("<color=white>Enviando puntuacion...</color>");
		DateTime utcTime = DateTime.UtcNow;
		//Solo llega una fecha 'date' si se trata de una puntuavción sin enviar
		string utcString = date == "" ? utcTime.ToString ().Replace (":", "").Replace ("/", "").Replace (" ", "") : date;

		WWW www = new WWW (webURL + privateCode + "/add/" + WWW.EscapeURL (username + "-" + utcString) + "/" + score);
		yield return www;

		if (string.IsNullOrEmpty (www.error)) {
			//Debug.Log ("<color=white>... La puntuación se envió correctamente</color>");
			DownloadHighscores ();
		} else {
			Debug.Log ("<color=red>... Hubo un error durante la subida de puntuación: " + www.error + "</color>");
			SaveUndeliveredScore (new ScoreEntry(username,score, utcTime.ToOADate()));
		}
	}

	public void DownloadHighscores() {
		StartCoroutine (GetHighscores());
	}

	IEnumerator GetHighscores() {
		highscoreList.Clear();
		bool isAllDataCollected = false;
		int iterations = 0;
		while (!isAllDataCollected) {
			//Debug.Log ("<color=white>Obteniendo puntuaciones... </color>");
			WWW www = new WWW (webURL + publicCode + "/pipe/" + iterations * 100 + "/100");
			//http://dreamlo.com/lb/58da1c3c12e7f00688faab7b/pipe/0/100
			iterations++;

			yield return www;

			if (string.IsNullOrEmpty (www.error)) {
				if (www.text == "")
					isAllDataCollected = true;
				else
					FormatHighscores (www.text);
			} else {
				Debug.Log ("<color=red>... Hubo un error durante la obtención de puntuaciones: " + www.error + "</color>");
			}
			yield return null;
		}
	}

	void FormatHighscores(string textSteam) {
		string[] entries = textSteam.Split (new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < entries.Length ; i++) {
			string[] entryInfo = entries [i].Split (new char[] { '|' });
			string username = entryInfo [0];
			int score = int.Parse(entryInfo [1]);
			double date = DateTime.Parse(entryInfo [4]).ToOADate();
			highscoreList.Add( new ScoreEntry(username, score, date) );
		}
		if (Dreamlo_OnHighscoresUpdate != null)
			Dreamlo_OnHighscoresUpdate (highscoreList);
	}
}