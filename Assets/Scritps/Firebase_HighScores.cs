using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;

public class Firebase_HighScores : MonoBehaviour {

	public const string DATABASE_REFERENCE = "scores";

	public static Firebase_HighScores instance;
	public List<ScoreEntry> highscoreList = new List<ScoreEntry> ();

	public Action<List<ScoreEntry>> Firebase_OnHighscoresUpdate;

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

		FirebaseDatabase.DefaultInstance.GetReference (DATABASE_REFERENCE)
			.OrderByChild ("score")
			.ValueChanged += HandleValueChange;
	}

	public void AddNewHighscore(string name, int points, double date = 0) {
		string key = FirebaseDatabase.DefaultInstance.GetReference (DATABASE_REFERENCE).Push().Key;

		DateTime utcTime = DateTime.UtcNow;
		if (date == 0)
			date = utcTime.ToOADate();
		
		ScoreEntry score = new ScoreEntry (name, points, date);
		Dictionary<string, object> entryValues = score.ToDictionary ();

		Dictionary<string, object> childUpdates = new Dictionary<string, object>();
		childUpdates[key] = entryValues;

		FirebaseDatabase.DefaultInstance.GetReference (DATABASE_REFERENCE).UpdateChildrenAsync (childUpdates);
	}

	void HandleValueChange (object sender, ValueChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError (args.DatabaseError.Message);
		} else {
			highscoreList.Clear ();
			if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0) {
				foreach (var childSnapshot in args.Snapshot.Children.Reverse()) {
					if (childSnapshot.Child("score") == null || childSnapshot.Child("score").Value == null) {
						Debug.Log("<color=red>Bad data in sample. No Data or Did you forget to call SetEditorDatabaseUrl with your project id?</color>");
						break;
					} else {
						highscoreList.Add (new ScoreEntry (
								childSnapshot.Child ("username").Value.ToString (),
								int.Parse (childSnapshot.Child ("score").Value.ToString ()),
								double.Parse (childSnapshot.Child ("date").Value.ToString ())
							)
						);
					}
				}
				if (Firebase_OnHighscoresUpdate != null)
					Firebase_OnHighscoresUpdate (highscoreList);
			}

		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
