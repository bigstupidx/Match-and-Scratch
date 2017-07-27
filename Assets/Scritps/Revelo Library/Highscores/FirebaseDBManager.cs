using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;

public static class ServicesConfiguration {
	public static bool enable_tappx = false;
	public static bool mainscreen_video_rewarded = false;
	public static bool sendscore_video_rewarded = false;

	public static string DataToString () {
		return "enabled_tappx [" + enable_tappx.ToString () +
		"] - mainscreen_video_rewarded [" + mainscreen_video_rewarded.ToString () +
		"] - sendscore_video_rewarded [" + sendscore_video_rewarded.ToString () + "]"; 
	}
}

public class FirebaseDBManager : MonoBehaviour {

	public const string DATABASE_REFERENCE_SCORES = "scores";
	public const string DATABASE_REFERENCE_CONFIG = "config";

	public static FirebaseDBManager instance;
	public List<ScoreEntry> highscoreList = new List<ScoreEntry> ();

	public Action<List<ScoreEntry>> Firebase_OnHighscoresUpdate;

	private const int MaxScores = 5;

	FirebaseApp app;
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

	void Start () {
		dependencyStatus = FirebaseApp.CheckDependencies ();
		if (dependencyStatus != DependencyStatus.Available) {
			FirebaseApp.FixDependenciesAsync ().ContinueWith ( 
				task => {
					dependencyStatus = FirebaseApp.CheckDependencies();
					if (dependencyStatus == DependencyStatus.Available) {
						InitializeFirebaseScores();
					} else {
						Debug.LogError ("Could not resolve all Firebase dependencies: " + dependencyStatus);
					}
				}
			);
		} else {
			InitializeFirebase ();
			InitializeFirebaseConfig ();
			InitializeFirebaseScores ();
		}
	}
	void Destroy() {
		FirebaseDatabase.DefaultInstance
			.GetReference("scores")
			.ValueChanged -= HandleHighscoresValueChange; // unsubscribe from ValueChanged.
	}
		
	void InitializeFirebase () {
		app = FirebaseApp.DefaultInstance;
		app.SetEditorDatabaseUrl("https://match-and-scratch-92345929.firebaseio.com/");

		if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
	}

	void InitializeFirebaseConfig() {
		FirebaseDatabase.DefaultInstance.GetReference (DATABASE_REFERENCE_CONFIG)
			.ValueChanged += HandleConfigValues;
	}

	void InitializeFirebaseScores() {		
		FirebaseDatabase.DefaultInstance.GetReference (DATABASE_REFERENCE_SCORES)
			.OrderByChild ("score")
			.ValueChanged += HandleHighscoresValueChange;
	}

	public void AddNewHighscore(ScoreEntry scoreEntry) {
		string key = FirebaseDatabase.DefaultInstance.GetReference (DATABASE_REFERENCE_SCORES).Push().Key;

		DateTime utcTime = DateTime.UtcNow;
		if (scoreEntry.date == 0)
			scoreEntry.date = utcTime.ToOADate();
		
		Dictionary<string, object> entryValues = scoreEntry.ToDictionary ();

		Dictionary<string, object> childUpdates = new Dictionary<string, object>();
		childUpdates[key] = entryValues;

		FirebaseDatabase.DefaultInstance.GetReference (DATABASE_REFERENCE_SCORES).UpdateChildrenAsync (childUpdates).ContinueWith (task => {
			if(task.Exception != null)
				Debug.Log("Error al insertar Score: " + task.Exception.ToString());
			else if (task.IsCompleted) {
				Debug.Log("Añadido: " + key + ": "+ scoreEntry.ToContatString());
			}
		});
	}

	void HandleHighscoresValueChange (object sender, ValueChangedEventArgs args) {
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

	void HandleConfigValues(object sender, ValueChangedEventArgs args) {
		if (args.DatabaseError != null) {
			Debug.LogError (args.DatabaseError.Message);
		} else {
			if (args.Snapshot != null && args.Snapshot.ChildrenCount > 0) {
				foreach (var childSnapshot in args.Snapshot.Children.Reverse()) {
					if (childSnapshot.Child("enable_tappx") == null || childSnapshot.Child("enable_tappx").Value == null) {
						Debug.Log("<color=red>Bad data in sample. No Data or Did you forget to call SetEditorDatabaseUrl with your project id?</color>");
						break;
					} else {
						ServicesConfiguration.enable_tappx = bool.Parse (childSnapshot.Child ("enable_tappx").Value.ToString ());
						ServicesConfiguration.mainscreen_video_rewarded =  bool.Parse (childSnapshot.Child ("mainscreen_video_rewarded").Value.ToString ());
						ServicesConfiguration.sendscore_video_rewarded =  bool.Parse (childSnapshot.Child ("sendscore_video_rewarded").Value.ToString ());
						Debug.Log (ServicesConfiguration.DataToString ());
						GameManager.instance.SetServicesConfiguration ();
					}
				}
			}
		}
	}
	/*
	public void DreamloToFireBase() {
		if (Dreamlo_HighScores.instance != null) {
			if (Dreamlo_HighScores.instance.highscoreList != null && Dreamlo_HighScores.instance.highscoreList.Count > 0) {
				foreach (ScoreEntry se in Dreamlo_HighScores.instance.highscoreList) {
					string[] nameAndDate = se.username.Split (new char[]{ '-' }, StringSplitOptions.RemoveEmptyEntries);
					ScoreEntry newSe = new ScoreEntry (nameAndDate [0].Replace ('+', ' '), se.score, se.date ); //double.Parse (nameAndDate [1]) );
					AddNewHighscore (newSe);
				}
			}
		}
	}
	
	public void CleanDataBase() {
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference (DATABASE_REFERENCE);

		reference.RemoveValueAsync ().ContinueWith(task => {
			if(task.Exception != null)
				Debug.Log("Error al eliminar: " + task.Exception.ToString());
			else if (task.IsCompleted) {
				Debug.Log("Success!");
			}
		});
	}
	*/
	// Update is called once per frame
	void Update () {
		
	}
}
