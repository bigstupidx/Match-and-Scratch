using UnityEngine;
using System.Collections;

public class HighScores : MonoBehaviour
{

	const string privateCode= "J3duL1tLhUSESgpxL-tbHwuoqEW8TI9k-eS_5UfeoOhA";
	const string publicCode	= "58da1c3c12e7f00688faab7b";
	const string webURL		= "http://dreamlo.com/lb/";

	public Highscore[] HighscoreList;

	void Awake() {
		AddNewHighscore ("user1", 50);
		AddNewHighscore ("user2", 100);
		AddNewHighscore ("user3", 5);

		DownloadHighscores ();
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
		StartCoroutine ("GetHighscores");
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

		HighscoreList = new Highscore[entries.Length];

		for (int i = 0; i < entries.Length ; i++) {
			string[] entryInfo = entries [i].Split (new char[] { '|' });
			string username = entryInfo [0];
			int score = int.Parse(entryInfo [1]);
			HighscoreList [i] = new Highscore(username, score);
			Debug.Log (HighscoreList [i].username + ": " + HighscoreList [i].score);
		}
	}

	void Start() {
		
	}

	void Update() {
		
	}
}

public struct Highscore 
{
	public string username;
	public int score;

	public Highscore(string _username, int _score) {
		username = _username;
		score = _score;
	}
}