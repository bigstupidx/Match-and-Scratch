using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;

public struct Score 
{
	public string username;
	public int score;
	public double date;

	public Score(string _username, int _score, double _date) {
		username = _username;
		score = _score;
		date = _date;
	}

	public Dictionary<string, object> ToDictionary() { 
		Dictionary<string, object> result = new Dictionary<string, object> ();
		result ["username"] = username;
		result ["score"] = score;
		result ["date"] = date;

		return result;
	}
}

public class Leaderboards : MonoBehaviour {

	public const string DATABASE_REFERENCE = "scores";

	public static Leaderboards instance;
	public List<Score> ScoresList = new List<Score> ();

	public Action<List<Score>> OnLeaderBoardUpdate;

	private const int MaxScores = 5;

	DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
	DatabaseReference dbReference;

	void Awake() {
		if (instance == null) {
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}
	}

	void Start() {
		dependencyStatus = FirebaseApp.CheckDependencies();
		if (dependencyStatus != DependencyStatus.Available) {
			FirebaseApp.FixDependenciesAsync().ContinueWith ( 
				task => {
					dependencyStatus = FirebaseApp.CheckDependencies();
					if (dependencyStatus == DependencyStatus.Available) {
						InitializeFirebase();
					} else {
						Debug.LogError ("Could not resolve all Firebase dependencies: " + dependencyStatus);
					}
				}
			);
		} else {
			InitializeFirebase();
		}
	}
	void Destroy() {
		FirebaseDatabase.DefaultInstance
			.GetReference("scores")
			.ValueChanged -= HandleValueChange; // unsubscribe from ValueChanged.
	}
		

	void InitializeFirebase() {
		FirebaseApp app = FirebaseApp.DefaultInstance;
		app.SetEditorDatabaseUrl("https://match-and-scratch-92345929.firebaseio.com/");

		if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);

		dbReference = FirebaseDatabase.DefaultInstance.GetReference (DATABASE_REFERENCE);
		dbReference.OrderByChild ("score");
		dbReference.ValueChanged += HandleValueChange;
	}

	public void AddScore(string name, int points, double date = 0) {
		string key = dbReference.Push().Key;

		DateTime utcTime = DateTime.UtcNow;
		if (date == 0)
			date = utcTime.ToOADate();
		
		Score score = new Score (name, points, date);
		Dictionary<string, object> entryValues = score.ToDictionary ();

		Dictionary<string, object> childUpdates = new Dictionary<string, object>();
		childUpdates[key] = entryValues;

		dbReference.UpdateChildrenAsync (childUpdates);
	}

	void HandleValueChange (object sender, ValueChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError (args.DatabaseError.Message);
		} else {
			//Debug.Log("args: " + args.Snapshot.ToString());

			ScoresList.Clear ();
			if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0) {
				foreach (var childSnapshot in args.Snapshot.Children) {
					if (childSnapshot.Child("score") == null || childSnapshot.Child("score").Value == null) {
						Debug.Log("<color=red>Bad data in sample. No Data or Did you forget to call SetEditorDatabaseUrl with your project id?</color>");
						break;
					} else {
						ScoresList.Add (new Score (
								childSnapshot.Child ("username").Value.ToString (),
								int.Parse (childSnapshot.Child ("score").Value.ToString ()),
								double.Parse (childSnapshot.Child ("date").Value.ToString ())
							)
						);
					}
				}
				if (OnLeaderBoardUpdate != null)
					OnLeaderBoardUpdate (ScoresList);
			}

		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
