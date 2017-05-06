using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;

public class Leaderboards : MonoBehaviour {

	public static Leaderboards instance;
	public List<Highscore> leaderBoardList = new List<Highscore> ();

	private const int MaxScores = 5;

	DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

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
		

	void InitializeFirebase() {
		FirebaseApp app = FirebaseApp.DefaultInstance;
		app.SetEditorDatabaseUrl("https://match-and-scratch-92345929.firebaseio.com/");

		if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);

		FirebaseDatabase.DefaultInstance
			.GetReference("Leaders").OrderByChild("score")
			.ValueChanged += (object sender2, ValueChangedEventArgs e2) => {
				if (e2.DatabaseError != null) {
					Debug.LogError(e2.DatabaseError.Message);
					return;
				}

				leaderBoardList.Clear();

				if (e2.Snapshot != null && e2.Snapshot.ChildrenCount > 0) {
					foreach (var childSnapshot in e2.Snapshot.Children) {
						if (childSnapshot.Child("score") == null || childSnapshot.Child("score").Value == null) {
							Debug.LogError("Bad data in sample.  Did you forget to call SetEditorDatabaseUrl with your project id?");
							break;
						} else {
							leaderBoardList.Insert(1, childSnapshot.Child("score").Value.ToString()
							+ "  " + childSnapshot.Child("email").Value.ToString());
						}
					}
				}
			};
	}

	// A realtime database transaction receives MutableData which can be modified
	// and returns a TransactionResult which is either TransactionResult.Success(data) with
	// modified data or TransactionResult.Abort() which stops the transaction with no changes.
	TransactionResult AddScoreTransaction(MutableData mutableData) {
		List<Object> leaders = mutableData.Value as List<object>;

		if (leaders == null) {
			leaders = new List<object>();
		} else if (mutableData.ChildrenCount >= MaxScores) {
			// If the current list of scores is greater or equal to our maximum allowed number,
			// we see if the new score should be added and remove the lowest existing score.
			long minScore = long.MaxValue;
			object minVal = null;
			foreach (var child in leaders) {
				if (!(child is Dictionary<string, object>))
					continue;
				long childScore = (long)((Dictionary<string, object>)child)["score"];
				if (childScore < minScore) {
					minScore = childScore;
					minVal = child;
				}
			}
			// If the new score is lower than the current minimum, we abort.
			if (minScore > 0) {
				return TransactionResult.Abort();
			}
			// Otherwise, we remove the current lowest to be replaced with the new score.
			leaders.Remove(minVal);
		}

		// Now we add the new score as a new entry that contains the email address and score.
		Dictionary<string, object> newScoreMap = new Dictionary<string, object>();
		newScoreMap["score"] = score;
		newScoreMap["email"] = email;
		leaders.Add(newScoreMap);

		// You must set the Value to indicate data at that location has changed.
		mutableData.Value = leaders;
		return TransactionResult.Success(mutableData);
	}


	// Update is called once per frame
	void Update () {
		
	}
}
